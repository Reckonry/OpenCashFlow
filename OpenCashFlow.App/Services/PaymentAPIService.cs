using System.Text.Json;
using global::Shared.DTOs;
using global::Shared.Models;

namespace OpenCashFlow.App.Services
{
    public partial class PaymentAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<PaymentAPIService> _logger;

        public PaymentAPIService(IHttpClientFactory httpClientFactory, ILogger<PaymentAPIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API-Client");
            _logger = logger;
        }

        public async Task<IEnumerable<Payment_List_DTO>?> GetPaymentsAsync(Payment_Filter_DTO? filters = null)
        {
            filters ??= new(); // se null, istanzio con i valori di default
            var response = await _httpClient.PostAsJsonAsync("v1/Payments/", filters);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /v1/Payments: {Status} - {Error}", response.StatusCode, errorText);
                throw new Exception($"Errore nella chiamata al server: {response.StatusCode} - {errorText}");
            }

            var payments = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_List_DTO>?>();
            return payments;
        }

        public async Task<ApiResponse<Payment_Detail_DTO>> GetPaymentAsync(Guid paymentID)
        {
            if (paymentID == Guid.Empty)
                return new ApiResponse<Payment_Detail_DTO> (false, "PaymentID non può essere vuoto.");

            var response = await _httpClient.GetAsync($"/v1/Payment/{paymentID}");

            // In caso di 404 Not Found, restituisco envelope con Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Payment_Detail_DTO> (false, "Payment non trovato." );

            // In caso di altri status non 2xx, logga e restituisci errore
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Payment/{id}: {Status} - {Error}", paymentID, response.StatusCode, errorText);

                return new ApiResponse<Payment_Detail_DTO>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            // Deserializza direttamente in ApiResponse<Payment_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Detail_DTO>(false, "Risposta non valida dal server." );

            return apiResponse; 
        }


        public async Task<ApiResponse<Payment_Detail_DTO>> AddPaymentAsync(Payment_Create_DTO payment)
        {
            if (payment == null) throw new ArgumentNullException(nameof(payment));

            var response = await _httpClient.PostAsJsonAsync("/v1/Payment", payment);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /Payments: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<Payment_Detail_DTO>(false, $"Errore HTTP: {response.StatusCode}");

            }

            // Deserializziamo direttamente in ApiResponse<Payment_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Detail_DTO>(false, "Risposta non valida dal server");

            return apiResponse;
        }

        public async Task<ApiResponse<Payment_Detail_DTO>> UpdatePaymentAsync(Payment_Update_DTO editDto)
        {
            if (editDto == null || editDto.PaymentID == Guid.Empty)
                throw new ArgumentNullException(nameof(editDto), "DTO per update non valido.");

            var response = await _httpClient.PutAsJsonAsync($"/v1/Payment/{editDto.PaymentID}", editDto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /Payment/{id}: {Status} – {Error}", editDto.PaymentID, response.StatusCode, error);
                return new ApiResponse<Payment_Detail_DTO> (false, $"Errore HTTP: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Detail_DTO>(false, "Risposta non valida dal server");

            return apiResponse;
        }

        public async Task<ApiResponse<object>> DeletePaymentAsync(Guid paymentID)
        {
            if (paymentID == Guid.Empty)
                return new ApiResponse<object>(false, "ID pagameto non valido.");

            // Endpoint DELETE: /Payment/{id}
            var response = await _httpClient.DeleteAsync($"/v1/Payment/{paymentID}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP DELETE /Payment/{id}: {Status} – {Error}", paymentID, response.StatusCode, error);
                return new ApiResponse<object>(false, $"Errore HTTP: {response.StatusCode}");
            }

            // L’API potrebbe non restituire body, quindi possiamo restituire Success = true
            return new ApiResponse<object>(true, "");
        }

    }
}
