using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Application.Payments.DeletePayment;

public sealed record DeletePaymentOrchestrationResult(
    bool Deleted,
    PaymentSnapshot? Payment,
    bool DailyPaymentApplied,
    bool CashLedgerApplied);
