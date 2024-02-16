using System.Text.Json;
using Google.Protobuf;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.FeibaoPay.MessageQueue;

internal class DepositProcessor(
    FeibaoPayClient feibaoPayClient,
    IPaymentClient paymentClient,
    IOptions<FeibaoPayOptions> optionsAccessor) : IMessageProcessor
{
    private readonly FeibaoPayClient _feibaoPayClient = feibaoPayClient ?? throw new ArgumentNullException(nameof(feibaoPayClient));
    private readonly IPaymentClient _paymentClient = paymentClient ?? throw new ArgumentNullException(nameof(paymentClient));
    private readonly FeibaoPayOptions _options = optionsAccessor.Value;

    public async ValueTask<ReadOnlyMemory<byte>> HandleAsync(ReadOnlyMemory<byte> data, CancellationToken cancellationToken = default)
    {
        var request = DepositRequest.Parser.ParseFrom(data.Span);

        var settings = JsonSerializer.Deserialize<FeibaoPayDepositSettings>(
            request.ApplyInfo,
            FeibaoPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(request.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.GateWay,
            settings.PlayerCode,
            settings.UserIp);

        var depositResponse = await _feibaoPayClient.DepositAsync(
            settings.Merchant,
            settings.HashKey,
            settings.HashIV,
            order,
            cancellationToken).ConfigureAwait(false);

        var applyInfo = new FeibaoPayDepositApplyInfo(
            settings,
            new FeibaoPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo));

        await _paymentClient.ScheduleAsync(
            "tgs.payment.feibaopay.deposit.schedule.reject",
            settings.FormId,
            DateTime.UtcNow.AddMinutes(60D),
            cancellationToken).ConfigureAwait(false);

        var msg = new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
        };

        return msg.ToByteArray();
    }
}
