using global::Shared.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService
    {
        /// <summary>
        /// Ottiene gli eventi calendario per gli incassi
        /// </summary>
        /// <param name="filters">Filtri opzionali (FromDate, ToDate, UserID, etc.)</param>
        /// <param name="cancellationToken">Token di cancellazione</param>
        /// <returns>Lista di eventi calendario compatibili con FullCalendar</returns>
        public async Task<IEnumerable<Payment_CalendarEvent_DTO>> GetPaymentCalendarEventsAsync(
            Payment_Filter_DTO filters,
            CancellationToken cancellationToken)
        {
            var companyId = _authenticationService.GetTenantID();
            return await _paymentRepository.GetPaymentCalendarEventsAsync(companyId, filters, cancellationToken);
        }
    }
}
