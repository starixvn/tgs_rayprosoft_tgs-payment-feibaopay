using System.Text.Json.Serialization;

namespace Tgs.Payment.FeibaoPay.ViewModels;

public class CallbackDepositOrder
{
    [JsonPropertyName("amount")]
    public required string Amount { get; set; }

    [JsonPropertyName("gateway")]
    public required string Gateway { get; set; }

    [JsonPropertyName("merchant_order_num")]
    public required string MerchantOrderNum { get; set; }

    [JsonPropertyName("merchant_order_remark")]
    public required string MerchantOrderRemark { get; set; }

    [JsonPropertyName("status")]
    public required string Status { get; set; }

    [JsonPropertyName("sign")]
    public required string Sign { get; set; }
}
