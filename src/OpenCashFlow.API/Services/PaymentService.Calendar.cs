using global::Shared.DTOs;

namespace OpenCashFlow.API.Services
{
    public partial class PaymentService
    {
        /// <summary>
        /// Gets calendar events for payments
        /// </summary>
        /// <param name="filters">Optional filters (FromDate, ToDate, UserID, etc.)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of calendar events compatible with FullCalendar</returns>
        public async Task<IEnumerable<Payment_CalendarEvent_DTO>> GetPaymentCalendarEventsAsync(
            Payment_Filter_DTO filters,
            CancellationToken cancellationToken)
        {
            var companyId = _authenticationService.GetTenantID();
            return await _paymentRepository.GetPaymentCalendarEventsAsync(companyId, filters, cancellationToken);
        }
    }
}
