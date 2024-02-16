namespace Tgs.Payment.FeibaoPay;
public record WithdrawOrder(
    Guid FormId,
    decimal Amount,
    Uri CallbackUrl,
    string BankCode,
    string CardHolder,
    string CardNumber,
    string Gateway,
    string UserId,
    string UserIp);
