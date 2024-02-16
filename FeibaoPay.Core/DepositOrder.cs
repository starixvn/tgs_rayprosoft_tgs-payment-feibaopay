namespace Tgs.Payment.FeibaoPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    string GateWay,
    string PlayerCode,
    string UserIp);
