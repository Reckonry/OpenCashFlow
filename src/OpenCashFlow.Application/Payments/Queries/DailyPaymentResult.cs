namespace OpenCashFlow.Application.Payments.Queries;

public sealed record DailyPaymentResult(
    Guid DailyPaymentsId,
    Guid TenantId,
    DateTime CashDate,
    double Total,
    DateTime DateIns);
