namespace Tgs.Payment.FeibaoPay.Client;
public record FeibaoPayWithdrawReturn
{
    public string ThirdPartyOrderNo { get; set; } = default!;

    public decimal Amount { get; set; }

    public Guid FormId { get; set; }

    public string Status { get; set; } = default!;

    public string Gateway { get; set; } = default!;
    public string MerchantOrderRemark { get; set; } = default!;

    public string Note { get; set; } = default!;
}
