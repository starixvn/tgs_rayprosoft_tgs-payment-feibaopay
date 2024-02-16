namespace Tgs.Payment.FeibaoPay;
public record FeibaoPayClientWithdrawResponse(
    Guid FormId,
    string ThirdPartyOrderNo,
    decimal Amount);
