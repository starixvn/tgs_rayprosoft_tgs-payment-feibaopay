namespace Tgs.Payment.FeibaoPay.Client;

public record FeibaoPayDepositSettings(
    string Merchant,
    string HashKey,
    string HashIV,
    Guid FormId,
    string GateWay,
    string PlayerCode,
    string UserIp);
