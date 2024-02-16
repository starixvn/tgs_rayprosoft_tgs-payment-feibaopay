namespace Tgs.Payment.FeibaoPay.Client;
public record FeibaoPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo);
