using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;

namespace OpenCashFlow.API.Controllers
{
    public partial class PaymentController
    {
        /// <summary>
        /// Ottiene gli eventi calendario per gli incassi
        /// </summary>
        /// <param name="filters">Filtri opzionali per data, utente, tipo, ecc.</param>
        /// <param name="cancellationToken">Token di cancellazione</param>
        /// <returns>Lista di eventi calendario compatibili con FullCalendar.js</returns>
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
