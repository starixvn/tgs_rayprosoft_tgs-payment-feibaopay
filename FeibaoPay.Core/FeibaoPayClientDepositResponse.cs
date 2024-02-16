namespace Tgs.Payment.FeibaoPay;
public record FeibaoPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    string ThirdPartyOrderNo,
    Uri RedirectUrl);
