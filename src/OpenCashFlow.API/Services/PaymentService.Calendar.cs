using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Application.Payments.Calendar;

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
            filters ??= new Payment_Filter_DTO();
            var events = await _getPaymentCalendarUseCase.ExecuteAsync(
                new GetPaymentCalendarQuery(ToPaymentListQuery(filters, companyId)),
                cancellationToken);

            return events.Select(e => new Payment_CalendarEvent_DTO
            {
                Id = e.Id,
                Title = e.Title,
                Start = e.Start,
                AllDay = e.AllDay,
                Url = e.Url,
                ExtendedProps = new PaymentEventExtendedProps
                {
                    Calendar = e.Calendar,
                    PaymentCount = e.PaymentCount,
                    TotalAmount = e.TotalAmount,
                    Date = e.Date,
                    EntryType = e.EntryType
                }
            }).ToList();
        }
    }
}
