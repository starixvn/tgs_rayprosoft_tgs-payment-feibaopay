namespace Tgs.Payment.FeibaoPay.Client;
public record FeibaoPayWithdrawSettings(
    string Merchant,
    string HashKey,
    string HashIV,
    Guid FormId,
    string BankCode,
    string CardHolder,
    string CardNumber,
    string Gateway,
    string UserId,
    string UserIp);
