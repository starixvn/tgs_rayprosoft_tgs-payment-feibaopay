using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.FeibaoPay.MessageQueue;

internal class WithdrawProcessor(
    FeibaoPayClient feibaoPayClient,
    IPaymentClient paymentClient,
    IOptions<FeibaoPayOptions> optionsAccessor) : IMessageProcessor
{
    private readonly FeibaoPayClient _feibaoPayClient = feibaoPayClient ?? throw new ArgumentNullException(nameof(feibaoPayClient));
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly FeibaoPayOptions _options = optionsAccessor.Value;

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = WithdrawRequest.Parser.ParseFrom(data.Span);
        var settings = JsonSerializer.Deserialize<FeibaoPayWithdrawSettings>(request.ApplyInfo, FeibaoPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(request.Amount),
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.BankCode,
            settings.CardHolder,
            settings.CardNumber,
            settings.Gateway,
            settings.UserId,
            settings.UserIp);

        var withdrawResponse = await _feibaoPayClient.WithdrawAsync(
            settings.Merchant,
            settings.HashKey,
            settings.HashIV,
            order,
            cancellationToken).ConfigureAwait(false);
        var applyInfo = new FeibaoPayWithdrawApplyInfo(
            settings,
            new FeibaoPayWithdrawResponse(withdrawResponse.FormId));

        await _paymentClient.SaveWithdrawFormThirdPartyOrderNoAsync(
            settings.FormId,
            withdrawResponse.ThirdPartyOrderNo,
            cancellationToken);

        var msg = new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
        };

        return msg.ToByteArray();
    }
}
