using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Application.Payments.UpdatePayment;

public sealed record UpdatePaymentOrchestrationResult(
    PaymentSnapshot Payment,
    PaymentSnapshot OriginalPayment,
    UpdatePaymentResult ValidatedPayment,
    bool DailyPaymentApplied,
    bool CashLedgerApplied);
