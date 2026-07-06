namespace OpenCashFlow.Application.Payments.Reports;

public sealed record GetPaymentMonthReportQuery(Guid TenantId, int Year, int Month);
