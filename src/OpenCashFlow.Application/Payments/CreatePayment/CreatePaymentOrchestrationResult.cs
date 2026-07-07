using OpenCashFlow.Application.Abstractions;

namespace OpenCashFlow.Application.Payments.CreatePayment;

public sealed record CreatePaymentOrchestrationResult(
    PaymentSnapshot Payment,
    CreatePaymentResult ValidatedPayment,
    bool WasExisting,
    bool DailyPaymentApplied,
    bool CashLedgerApplied);
