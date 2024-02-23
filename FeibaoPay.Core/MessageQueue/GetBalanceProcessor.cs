using System.Text.Json;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.FeibaoPay.MessageQueue;

internal class GetBalanceProcessor(FeibaoPayClient feibaoPayClient)
    : IMessageProcessor<PaymentBalanceRequest, PaymentBalanceResponse>
{
    public async ValueTask<PaymentBalanceResponse> HandleAsync(
        string subject,
        PaymentBalanceRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        var settings = JsonSerializer.Deserialize<BrandPaymentMerchantInfo>(
            data.BrandPaymentSettings)!;

        var result = await feibaoPayClient.QueryBalanceAsync(
            settings.Merchant,
            settings.HashKey,
            settings.HashIV,
            [settings.GateWay],
            cancellationToken).ConfigureAwait(false);

        return new PaymentBalanceResponse
        {
            Balance = result![settings.GateWay]
        };
    }
}
