using Tgs.Payment.Clients;
using Tgs.Payment.FeibaoPay.Client;

namespace Tgs.Payment.FeibaoPay;
public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    FeibaoPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);
