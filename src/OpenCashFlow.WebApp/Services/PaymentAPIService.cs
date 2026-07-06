using System.Text.Json;
using global::Shared.DTOs;
using global::Shared.Models;

namespace OpenCashFlow.WebApp.Services
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
            filters ??= new(); // if null, initialize with default values
            var response = await _httpClient.PostAsJsonAsync("v1/Payments/", filters);

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /v1/Payments: {Status} - {Error}", response.StatusCode, errorText);
                throw new Exception($"Error calling the server: {response.StatusCode} - {errorText}");
            }

            var payments = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_List_DTO>?>();
            return payments;
        }

        public async Task<ApiResponse<Payment_Detail_DTO>> GetPaymentAsync(Guid paymentID)
        {
            if (paymentID == Guid.Empty)
                return new ApiResponse<Payment_Detail_DTO> (false, "PaymentID cannot be empty.");

            var response = await _httpClient.GetAsync($"/v1/Payment/{paymentID}");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Payment_Detail_DTO> (false, "Payment not found." );

            // For other non-2xx statuses, log and return an error
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Payment/{id}: {Status} - {Error}", paymentID, response.StatusCode, errorText);

                return new ApiResponse<Payment_Detail_DTO>(false, $"Error during request: {response.StatusCode}");
            }

            // Deserialize directly into ApiResponse<Payment_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Detail_DTO>(false, "Invalid response from server.");

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
                return new ApiResponse<Payment_Detail_DTO>(false, $"HTTP error: {response.StatusCode}");

            }

            // Deserialize directly into ApiResponse<Payment_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Detail_DTO>(false, "Invalid response from server");

            return apiResponse;
        }

        public async Task<ApiResponse<Payment_Detail_DTO>> UpdatePaymentAsync(Payment_Update_DTO editDto)
        {
            if (editDto == null || editDto.PaymentID == Guid.Empty)
                throw new ArgumentNullException(nameof(editDto), "Invalid update DTO.");

            var response = await _httpClient.PutAsJsonAsync($"/v1/Payment/{editDto.PaymentID}", editDto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /Payment/{id}: {Status} – {Error}", editDto.PaymentID, response.StatusCode, error);
                return new ApiResponse<Payment_Detail_DTO> (false, $"HTTP error: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Detail_DTO>(false, "Invalid response from server");

            return apiResponse;
        }

        public async Task<ApiResponse<object>> DeletePaymentAsync(Guid paymentID)
        {
            if (paymentID == Guid.Empty)
                return new ApiResponse<object>(false, "Invalid payment ID.");

            // Endpoint DELETE: /Payment/{id}
            var response = await _httpClient.DeleteAsync($"/v1/Payment/{paymentID}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP DELETE /Payment/{id}: {Status} – {Error}", paymentID, response.StatusCode, error);
                return new ApiResponse<object>(false, $"HTTP error: {response.StatusCode}");
            }

            // The API might not return a body, so we can return Success = true
            return new ApiResponse<object>(true, "");
        }

    }
}
