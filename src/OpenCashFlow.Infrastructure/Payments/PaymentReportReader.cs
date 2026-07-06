using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using global::Shared.Data;

namespace OpenCashFlow.Infrastructure.Payments;

public sealed class PaymentReportReader(ApplicationDbContext context) : IPaymentReportReader
{
    public async Task<double> GetDailyPaymentAsync(Guid tenantId, DateTime date, CancellationToken cancellationToken = default)
    {
        return (await context.DailyCash_DS
            .FirstOrDefaultAsync(c => c.TenantID == tenantId && c.CashDate == date.Date, cancellationToken) ?? new()).Total;
    }

    public async Task<double> GetTotalPaymentsInPeriodAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await context.DailyCash_DS
            .Where(c => c.TenantID == tenantId && c.CashDate >= startDate.Date && c.CashDate <= endDate.Date)
            .SumAsync(c => c.Total, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyPaymentResult>> GetDailyPaymentsInPeriodAsync(Guid tenantId, DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await context.DailyCash_DS.AsNoTracking()
            .Where(c => c.TenantID == tenantId && c.CashDate >= startDate.Date && c.CashDate <= endDate.Date)
            .Select(c => new DailyPaymentResult(c.DailyPaymentsID, c.TenantID, c.CashDate, c.Total, c.DateIns))
            .ToListAsync(cancellationToken);
    }

    public async Task<double> GetMonthlyPaymentsAsync(Guid tenantId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await context.DailyCash_DS
            .Where(c => c.TenantID == tenantId && c.CashDate.Year == year && c.CashDate.Month == month)
            .SumAsync(c => c.Total, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyPaymentResult>> GetAllMonthlyPaymentsAsync(Guid tenantId, int year, int month, CancellationToken cancellationToken = default)
    {
        return await context.DailyCash_DS.AsNoTracking()
            .Where(c => c.TenantID == tenantId && c.CashDate.Year == year && c.CashDate.Month == month)
            .Select(c => new DailyPaymentResult(c.DailyPaymentsID, c.TenantID, c.CashDate, c.Total, c.DateIns))
            .ToListAsync(cancellationToken);
    }

    public async Task<double> GetYearlyPaymentsAsync(Guid tenantId, int year, CancellationToken cancellationToken = default)
    {
        return await context.DailyCash_DS
            .Where(c => c.TenantID == tenantId && c.CashDate.Year == year)
            .SumAsync(c => c.Total, cancellationToken);
    }

    public async Task<IReadOnlyList<DailyPaymentResult>> GetAllYearlyPaymentsAsync(Guid tenantId, int year, CancellationToken cancellationToken = default)
    {
        return await context.DailyCash_DS.AsNoTracking()
            .Where(c => c.TenantID == tenantId && c.CashDate.Year == year)
            .Select(c => new DailyPaymentResult(c.DailyPaymentsID, c.TenantID, c.CashDate, c.Total, c.DateIns))
            .ToListAsync(cancellationToken);
    }
}
