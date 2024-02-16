namespace Tgs.Payment.FeibaoPay.Client;
public record FeibaoPayDepositReturn
{
    public string ThirdPartyOrderNo { get; set; } = default!;

    public decimal OrderAmount { get; set; }

    public string GateWay { get; set; } = default!;

    public Guid FormId { get; set; }

    public string Status { get; set; } = default!;
}
