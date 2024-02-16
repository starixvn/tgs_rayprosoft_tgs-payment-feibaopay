using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;

namespace Tgs.Payment.FeibaoPay;
public record WithdrawForm(
    Guid Id,
    string? UserId,
    UserType UserType,
    Guid BrandId,
    Guid? BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    Guid BankInfoId,
    FeibaoPayWithdrawApplyInfo? ApplyInfo,
    string? PaymentReturn = null);
