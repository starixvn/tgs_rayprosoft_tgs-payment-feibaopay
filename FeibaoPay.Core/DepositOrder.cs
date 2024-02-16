namespace Tgs.Payment.FeibaoPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string GateWay,
    string? BankCode,
    string PlayerCode,
    string UserIp);
