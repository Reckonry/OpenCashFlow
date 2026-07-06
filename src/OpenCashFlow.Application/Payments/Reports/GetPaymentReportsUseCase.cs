using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using OpenCashFlow.Domain.Common;

namespace OpenCashFlow.Application.Payments.Reports;

public sealed class GetPaymentReportsUseCase(IPaymentReportReader paymentReportReader) : IGetPaymentReportsUseCase
{
    public Task<double> GetDailyPaymentAsync(GetDailyPaymentQuery query, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        var tenantId = TenantId.From(query.TenantId);
        return paymentReportReader.GetDailyPaymentAsync(tenantId.Value, query.Date.Date, cancellationToken);
    }

    public Task<double> GetTotalInPeriodAsync(GetPaymentPeriodReportQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePeriod(query);
        return paymentReportReader.GetTotalPaymentsInPeriodAsync(normalized.TenantId, normalized.StartDate, normalized.EndDate, cancellationToken);
    }

    public Task<IReadOnlyList<DailyPaymentResult>> GetDailyPaymentsInPeriodAsync(GetPaymentPeriodReportQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePeriod(query);
        return paymentReportReader.GetDailyPaymentsInPeriodAsync(normalized.TenantId, normalized.StartDate, normalized.EndDate, cancellationToken);
    }

    public Task<double> GetMonthlyPaymentsAsync(GetPaymentMonthReportQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeMonth(query);
        return paymentReportReader.GetMonthlyPaymentsAsync(normalized.TenantId, normalized.Year, normalized.Month, cancellationToken);
    }

    public Task<IReadOnlyList<DailyPaymentResult>> GetAllMonthlyPaymentsAsync(GetPaymentMonthReportQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeMonth(query);
        return paymentReportReader.GetAllMonthlyPaymentsAsync(normalized.TenantId, normalized.Year, normalized.Month, cancellationToken);
    }

    public Task<double> GetYearlyPaymentsAsync(GetPaymentYearReportQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeYear(query);
        return paymentReportReader.GetYearlyPaymentsAsync(normalized.TenantId, normalized.Year, cancellationToken);
    }

    public Task<IReadOnlyList<DailyPaymentResult>> GetAllYearlyPaymentsAsync(GetPaymentYearReportQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizeYear(query);
        return paymentReportReader.GetAllYearlyPaymentsAsync(normalized.TenantId, normalized.Year, cancellationToken);
    }

    private static GetPaymentPeriodReportQuery NormalizePeriod(GetPaymentPeriodReportQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var tenantId = TenantId.From(query.TenantId);
        var start = query.StartDate.Date;
        var end = query.EndDate.Date;

        if (end < start)
        {
            throw new ArgumentException("End date must be greater than or equal to start date.", nameof(query));
        }

        return query with
        {
            TenantId = tenantId.Value,
            StartDate = start,
            EndDate = end
        };
    }

    private static GetPaymentMonthReportQuery NormalizeMonth(GetPaymentMonthReportQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var tenantId = TenantId.From(query.TenantId);

        if (query.Month is < 1 or > 12)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Month), "Month must be between 1 and 12.");
        }

        if (query.Year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Year), "Year must be greater than zero.");
        }

        return query with { TenantId = tenantId.Value };
    }

    private static GetPaymentYearReportQuery NormalizeYear(GetPaymentYearReportQuery query)
    {
        ArgumentNullException.ThrowIfNull(query);
        var tenantId = TenantId.From(query.TenantId);

        if (query.Year < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(query.Year), "Year must be greater than zero.");
        }

        return query with { TenantId = tenantId.Value };
    }
}
