namespace OpenCashFlow.Application.Payments.Reports;

public sealed record GetDailyPaymentQuery(Guid TenantId, DateTime Date);
