using Microsoft.EntityFrameworkCore;
using global::Shared.DTOs;

namespace OpenCashFlow.Infrastructure.Payments
{
    public partial class PaymentRepository
    {
        /// <summary>
        /// Gets calendar events grouped by date
        /// </summary>
        /// <param name="TenantID">Company ID</param>
        /// <param name="filters">Optional filters (FromDate, ToDate, UserID, etc.)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of calendar events with counts and totals per date</returns>
        public async Task<IEnumerable<Payment_CalendarEvent_DTO>> GetPaymentCalendarEventsAsync(
            Guid TenantID,
            Payment_Filter_DTO filters,
            CancellationToken cancellationToken)
        {
            var query = _context.Payment_DS.AsNoTracking()
                .Where(p => p.TenantID == TenantID && !p.IsDeleted);

            // Apply filters
            if (filters.UserID.HasValue)
                query = query.Where(p => p.UserID == filters.UserID.Value);

            if (!string.IsNullOrWhiteSpace(filters.EntryType))
                query = query.Where(p => p.EntryType == filters.EntryType);

            if (filters.PaymentMethodID.HasValue)
                query = query.Where(p => p.PaymentMethodID == filters.PaymentMethodID.Value);

            if (filters.FromDate.HasValue)
                query = query.Where(p => p.DateIns >= filters.FromDate.Value.ToUniversalTime());

            if (filters.ToDate.HasValue)
                query = query.Where(p => p.DateIns <= filters.ToDate.Value.ToUniversalTime());

            // Group by date (day only, no time)
            var groupedData = await query
                .GroupBy(p => p.DateIns.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount),
                    // Take the most common entry type on that date
                    EntryType = g.GroupBy(p => p.EntryType)
                                 .OrderByDescending(x => x.Count())
                                 .Select(x => x.Key)
                                 .FirstOrDefault()
                })
                .OrderBy(x => x.Date)
                .ToListAsync(cancellationToken);

            // Map to a FullCalendar-compatible DTO
            var events = groupedData.Select(g => new Payment_CalendarEvent_DTO
            {
                Id = $"payment-{g.Date:yyyy-MM-dd}",
                Title = g.Count == 1 ? "1 incasso" : $"{g.Count} incassi",
                Start = g.Date,
                AllDay = true,
                Url = $"/Payments?date={g.Date:yyyy-MM-dd}", // Link to the filtered list by date
                ExtendedProps = new PaymentEventExtendedProps
                {
                    Calendar = g.EntryType?.ToLower() ?? "payments",
                    PaymentCount = g.Count,
                    TotalAmount = g.TotalAmount,
                    Date = g.Date.ToString("yyyy-MM-dd"),
                    EntryType = g.EntryType
                }
            });

            return events;
        }
    }
}
