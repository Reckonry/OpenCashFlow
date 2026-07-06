namespace OpenCashFlow.Application.Payments.Reports;

public sealed record GetPaymentPeriodReportQuery(Guid TenantId, DateTime StartDate, DateTime EndDate);
