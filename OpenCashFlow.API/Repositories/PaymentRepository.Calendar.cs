using Microsoft.EntityFrameworkCore;
using global::Shared.DTOs;

namespace OpenCashFlow.API.Repositories
{
    public partial class PaymentRepository
    {
        /// <summary>
        /// Ottiene gli eventi calendario raggruppati per data
        /// </summary>
        /// <param name="TenantID">ID della company</param>
        /// <param name="filters">Filtri opzionali (FromDate, ToDate, UserID, etc.)</param>
        /// <param name="cancellationToken">Token di cancellazione</param>
        /// <returns>Lista di eventi calendario con conteggi e totali per data</returns>
        public async Task<IEnumerable<Payment_CalendarEvent_DTO>> GetPaymentCalendarEventsAsync(
            Guid TenantID,
            Payment_Filter_DTO filters,
            CancellationToken cancellationToken)
        {
            var query = _context.Payment_DS.AsNoTracking()
                .Where(p => p.TenantID == TenantID && !p.IsDeleted);

            // Applica filtri
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

            // Raggruppa per data (solo giorno, senza ora)
            var groupedData = await query
                .GroupBy(p => p.DateIns.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount),
                    // Prendi il tipo di entry più comune in quella data
                    EntryType = g.GroupBy(p => p.EntryType)
                                 .OrderByDescending(x => x.Count())
                                 .Select(x => x.Key)
                                 .FirstOrDefault()
                })
                .OrderBy(x => x.Date)
                .ToListAsync(cancellationToken);

            // Mappa in DTO compatibile con FullCalendar
            var events = groupedData.Select(g => new Payment_CalendarEvent_DTO
            {
                Id = $"payment-{g.Date:yyyy-MM-dd}",
                Title = g.Count == 1 ? "1 incasso" : $"{g.Count} incassi",
                Start = g.Date,
                AllDay = true,
                Url = $"/Payments?date={g.Date:yyyy-MM-dd}", // Link alla lista filtrata per data
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
