using System.Text.Json;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Payments;

namespace OpenCashFlow.WebApp.Services
{
    public partial class PaymentAPIService
    {
        public async Task<IEnumerable<Payment_Method_List_DTO>?> GetPaymentMethodsAsync()
        {
            var response = await _httpClient.GetAsync("v1/Payment/PaymentMethods");
            if (!response.IsSuccessStatusCode)
               throw new Exception("Error calling the server.");

            var paymentMethods = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_Method_List_DTO>?>();
            return paymentMethods;
        }

        public async Task<ApiResponse<Payment_Method_Detail_DTO>> GetPaymentMethodAsync(Guid paymentMethodID)
        {
            if (paymentMethodID == Guid.Empty)
                return new ApiResponse<Payment_Method_Detail_DTO> (false, "PaymentMethodID cannot be empty.");

            var response = await _httpClient.GetAsync($"/v1/Payment/PaymentMethod/{paymentMethodID}");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Payment_Method_Detail_DTO> (false, "Payment method not found." );

            // For other non-2xx statuses, log and return an error
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Payment/PaymentMethod/{id}: {Status} - {Error}", paymentMethodID, response.StatusCode, errorText);

                return new ApiResponse<Payment_Method_Detail_DTO>(false, $"Error during request: {response.StatusCode}");
            }

            // Deserialize directly into ApiResponse<Payment_Method_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Method_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Method_Detail_DTO>(false, "Invalid response from server.");

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
                return new ApiResponse<Payment_Method_Detail_DTO>(false, $"HTTP error: {response.StatusCode}");

            }

            // Deserialize directly into ApiResponse<Payment_Method_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Method_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Method_Detail_DTO>(false, "Invalid response from server");

            return apiResponse;
        }

        public async Task<ApiResponse<Payment_Method_Detail_DTO>> UpdatePaymentAsync(Payment_Method_Update_DTO editDto)
        {
            if (editDto == null || editDto.PaymentMethodID == Guid.Empty)
                throw new ArgumentNullException(nameof(editDto), "Invalid update DTO.");

            var response = await _httpClient.PutAsJsonAsync($"/v1/Payment/PaymentMethod/{editDto.PaymentMethodID}", editDto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /v1/Payment/PaymentMethod/{id}: {Status} - {Error}", editDto.PaymentMethodID, response.StatusCode, error);
                return new ApiResponse<Payment_Method_Detail_DTO> (false, $"HTTP error: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Method_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_Method_Detail_DTO>(false, "Invalid response from server");

            return apiResponse;
        }

        public async Task<ApiResponse<object>> DeletePaymentMethodAsync(Guid paymentMethodID)
        {
            if (paymentMethodID == Guid.Empty)
                return new ApiResponse<object>(false, "Invalid payment method ID.");

            // Endpoint DELETE: /Payment/{id}
            var response = await _httpClient.DeleteAsync($"/v1/Payment/PaymentMethod/{paymentMethodID}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP DELETE /v1/Payment/PaymentMethod/{id}: {Status} - {Error}", paymentMethodID, response.StatusCode, error);
                return new ApiResponse<object>(false, $"HTTP error: {response.StatusCode}");
            }

            // The API might not return a body, so we can return Success = true
            return new ApiResponse<object>(true, "");
        }

    }
}
