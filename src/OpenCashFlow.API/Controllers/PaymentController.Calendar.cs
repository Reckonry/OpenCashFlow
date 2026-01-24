using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;

namespace OpenCashFlow.API.Controllers
{
    public partial class PaymentController
    {
        /// <summary>
        /// Gets calendar events for payments
        /// </summary>
        /// <param name="filters">Optional filters for date, user, type, etc.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of calendar events compatible with FullCalendar.js</returns>
        [HttpPost("[controller]/Calendar")]
        public async Task<ActionResult<IEnumerable<Payment_CalendarEvent_DTO>>> GetPaymentCalendarEvents(
            [FromBody] Payment_Filter_DTO? filters,
            CancellationToken cancellationToken)
        {
            filters ??= new Payment_Filter_DTO();

            var events = await _paymentService.GetPaymentCalendarEventsAsync(filters, cancellationToken);
            return Ok(events);
        }
    }
}
