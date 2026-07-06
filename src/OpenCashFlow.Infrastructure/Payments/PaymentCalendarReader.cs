using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using global::Shared.Data;

namespace OpenCashFlow.Infrastructure.Payments;

public sealed class PaymentCalendarReader(ApplicationDbContext context) : IPaymentCalendarReader
{
    public async Task<IReadOnlyList<PaymentCalendarEventResult>> GetPaymentCalendarEventsAsync(
        PaymentListQuery filters,
        CancellationToken cancellationToken = default)
    {
        var query = context.Payment_DS.AsNoTracking()
            .Where(p => p.TenantID == filters.TenantId && !p.IsDeleted);

        if (filters.UserId.HasValue)
            query = query.Where(p => p.UserID == filters.UserId.Value);
        if (!string.IsNullOrWhiteSpace(filters.EntryType))
            query = query.Where(p => p.EntryType == filters.EntryType);
        if (filters.PaymentMethodId.HasValue)
            query = query.Where(p => p.PaymentMethodID == filters.PaymentMethodId.Value);
        if (filters.FromDate.HasValue)
            query = query.Where(p => p.DateIns >= filters.FromDate.Value.ToUniversalTime());
        if (filters.ToDate.HasValue)
            query = query.Where(p => p.DateIns <= filters.ToDate.Value.ToUniversalTime());

        var groupedData = await query
            .GroupBy(p => p.DateIns.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                TotalAmount = g.Sum(p => p.Amount),
                EntryType = g.GroupBy(p => p.EntryType)
                    .OrderByDescending(x => x.Count())
                    .Select(x => x.Key)
                    .FirstOrDefault()
            })
            .OrderBy(x => x.Date)
            .ToListAsync(cancellationToken);

        return groupedData.Select(g => new PaymentCalendarEventResult(
                Id: $"payment-{g.Date:yyyy-MM-dd}",
                Title: g.Count == 1 ? "1 incasso" : $"{g.Count} incassi",
                Start: g.Date,
                AllDay: true,
                Url: $"/Payments?date={g.Date:yyyy-MM-dd}",
                Calendar: g.EntryType?.ToLower() ?? "payments",
                PaymentCount: g.Count,
                TotalAmount: g.TotalAmount,
                Date: g.Date.ToString("yyyy-MM-dd"),
                EntryType: g.EntryType))
            .ToList();
    }
}
