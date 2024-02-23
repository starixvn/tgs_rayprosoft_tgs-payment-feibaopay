using System.Text.Json;
using Microsoft.Extensions.Options;
using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.FeibaoPay.MessageQueue;

internal class DepositProcessor(
    FeibaoPayClient feibaoPayClient,
    IPaymentClient paymentClient,
    IOptions<FeibaoPayOptions> optionsAccessor)
    : IMessageProcessor<DepositRequest, DepositResponse>
{
    private readonly FeibaoPayOptions _options = optionsAccessor.Value;

    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<FeibaoPayDepositSettings>(
            data.ApplyInfo,
            FeibaoPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.GateWay,
            settings.BankCode,
            settings.PlayerCode,
            settings.UserIp);

        var depositResponse = await feibaoPayClient.DepositAsync(
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

        await paymentClient.ScheduleAsync(
            "tgs.payment.feibaopay.deposit.schedule.reject",
            settings.FormId,
            DateTime.UtcNow.AddMinutes(60D),
            cancellationToken).ConfigureAwait(false);

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, FeibaoPayUtility.SerializerOptions)
        };
    }
}
