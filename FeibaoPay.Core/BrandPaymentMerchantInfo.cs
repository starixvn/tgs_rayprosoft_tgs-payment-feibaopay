using System.Text.Json.Serialization;

namespace Tgs.Payment.FeibaoPay;
record BrandPaymentMerchantInfo(
    [property: JsonPropertyName("merchant")] string Merchant,
    [property: JsonPropertyName("hashKey")] string HashKey,
    [property: JsonPropertyName("hashIV")] string HashIV,
    [property: JsonPropertyName("gateWay")] string GateWay);
