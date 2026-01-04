using System.Text.Json;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.DTOs;

namespace OpenCashFlow.App.Services
{
    public partial class PaymentAPIService
    {
        public async Task<IEnumerable<Payment_Method_List_DTO>?> GetPaymentMethodsAsync()
        {
            var response = await _httpClient.GetAsync("v1/Payment/PaymentMethods");
            if (!response.IsSuccessStatusCode)
               throw new Exception("Errore nella chiamata al server");

            var paymentMethods = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_Method_List_DTO>?>();
            return paymentMethods;
        }

        public async Task<ApiResponse<Payment_Method_Detail_DTO>> GetPaymentMethodAsync(Guid paymentMethodID)
        {
            if (paymentMethodID == Guid.Empty)
                return new ApiResponse<Payment_Method_Detail_DTO> (false, "PaymentMethodID non può essere vuoto.");

            var response = await _httpClient.GetAsync($"/v1/Payment/PaymentMethod/{paymentMethodID}");

            // In caso di 404 Not Found, restituisco envelope con Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Payment_Method_Detail_DTO> (false, "Payment Method non trovato." );

            // In caso di altri status non 2xx, logga e restituisci errore
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Payment/PaymentMethod/{id}: {Status} - {Error}", paymentMethodID, response.StatusCode, errorText);

                return new ApiResponse<Payment_Method_Detail_DTO>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            // Deserializza direttamente in ApiResponse<Payment_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Method_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Method_Detail_DTO>(false, "Risposta non valida dal server." );

            return apiResponse; 
        }


        public async Task<ApiResponse<Payment_Method_Detail_DTO>> AddPaymentMethodAsync(Payment_Method_Create_DTO paymentMethod)
        {
            if (paymentMethod == null) throw new ArgumentNullException(nameof(paymentMethod));

            var response = await _httpClient.PostAsJsonAsync("/v1/Payment/PaymentMethod", paymentMethod);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /v1/Payment/PaymentMethod: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<Payment_Method_Detail_DTO>(false, $"Errore HTTP: {response.StatusCode}");

            }

            // Deserializziamo direttamente in ApiResponse<Payment_Method_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Method_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Method_Detail_DTO>(false, "Risposta non valida dal server");

            return apiResponse;
        }

        public async Task<ApiResponse<Payment_Method_Detail_DTO>> UpdatePaymentAsync(Payment_Method_Update_DTO editDto)
        {
            if (editDto == null || editDto.PaymentMethodID == Guid.Empty)
                throw new ArgumentNullException(nameof(editDto), "DTO per update non valido.");

            var response = await _httpClient.PutAsJsonAsync($"/v1/Payment/PaymentMethod/{editDto.PaymentMethodID}", editDto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /v1/Payment/PaymentMethod/{id}: {Status} - {Error}", editDto.PaymentMethodID, response.StatusCode, error);
                return new ApiResponse<Payment_Method_Detail_DTO> (false, $"Errore HTTP: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Method_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Method_Detail_DTO>(false, "Risposta non valida dal server");

            return apiResponse;
        }

        public async Task<ApiResponse<object>> DeletePaymentMethodAsync(Guid paymentMethodID)
        {
            if (paymentMethodID == Guid.Empty)
                return new ApiResponse<object>(false, "ID Methodo pagameto non valido.");

            // Endpoint DELETE: /Payment/{id}
            var response = await _httpClient.DeleteAsync($"/v1/Payment/PaymentMethod/{paymentMethodID}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP DELETE /v1/Payment/PaymentMethod/{id}: {Status} - {Error}", paymentMethodID, response.StatusCode, error);
                return new ApiResponse<object>(false, $"Errore HTTP: {response.StatusCode}");
            }

            // L’API potrebbe non restituire body, quindi possiamo restituire Success = true
            return new ApiResponse<object>(true, "");
        }

    }
}
