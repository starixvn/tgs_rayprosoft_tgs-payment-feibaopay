using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.FeibaoPay.MessageQueue;

internal class WithdrawProcessor(
    FeibaoPayClient feibaoPayClient,
    IPaymentClient paymentClient,
    IOptions<FeibaoPayOptions> optionsAccessor)
    : IMessageProcessor<WithdrawRequest, WithdrawResponse>
{
    private readonly FeibaoPayOptions _options = optionsAccessor.Value;

    public async ValueTask<WithdrawResponse> HandleAsync(
        string subject,
        WithdrawRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<FeibaoPayWithdrawSettings>(data.ApplyInfo, FeibaoPayUtility.SerializerOptions)!;

        var order = new WithdrawOrder(
            settings.FormId,
            decimal.Parse(data.Amount),
            new Uri($"{_options.CallbackBaseUrl}/withdraw/callback"),
            settings.BankCode,
            settings.CardHolder,
            settings.CardNumber,
            settings.Gateway,
            settings.UserId,
            settings.UserIp);

        var withdrawResponse = await feibaoPayClient.WithdrawAsync(
            settings.Merchant,
            settings.HashKey,
            settings.HashIV,
            order,
            cancellationToken).ConfigureAwait(false);
        var applyInfo = new FeibaoPayWithdrawApplyInfo(
            settings,
            new FeibaoPayWithdrawResponse(withdrawResponse.FormId));

        await paymentClient.SaveWithdrawFormThirdPartyOrderNoAsync(
            settings.FormId,
            withdrawResponse.ThirdPartyOrderNo,
            cancellationToken).ConfigureAwait(false);

        return new WithdrawResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
        };
    }
}
