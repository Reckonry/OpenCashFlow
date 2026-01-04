using global::Shared.DTOs;

namespace OpenCashFlow.App.Services
{
    public partial class PaymentAPIService
    {
        /// <summary>
        /// Ottiene gli eventi calendario per gli incassi dall'API
        /// </summary>
        /// <param name="filters">Filtri opzionali (FromDate, ToDate, UserID, etc.)</param>
        /// <returns>Lista di eventi calendario compatibili con FullCalendar</returns>
        public async Task<IEnumerable<Payment_CalendarEvent_DTO>?> GetPaymentCalendarEventsAsync(Payment_Filter_DTO? filters = null)
        {
            filters ??= new Payment_Filter_DTO();

            var response = await _httpClient.PostAsJsonAsync("v1/Payment/Calendar", filters);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /v1/Payment/Calendar: {Status} - {Error}", response.StatusCode, errorText);
                throw new Exception($"Errore nella chiamata al server: {response.StatusCode} - {errorText}");
            }

            var events = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_CalendarEvent_DTO>?>();
            return events;
        }
    }
}
