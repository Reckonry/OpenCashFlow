using System.Text.Json;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Payments;

namespace OpenCashFlow.WebApp.Services
{
    public partial class PaymentAPIService
    {
        public async Task<IEnumerable<Payment_DocumentType_List_DTO>?> GetDocumentTypesAsync()
        {
            var response = await _httpClient.GetAsync("v1/Payment/DocumentTypes");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Error calling the server.");

            var DocumentTypes = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_DocumentType_List_DTO>?>();
            return DocumentTypes;
        }

        public async Task<ApiResponse<Payment_DocumentType_Detail_DTO>> GetDocumentTypeAsync(Guid DocumentTypeID)
        {
            if (DocumentTypeID == Guid.Empty)
                return new ApiResponse<Payment_DocumentType_Detail_DTO> (false, "DocumentTypeID cannot be empty.");

            var response = await _httpClient.GetAsync($"/v1/Payment/DocumentType/{DocumentTypeID}");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Payment_DocumentType_Detail_DTO> (false, "Document type not found." );

            // For other non-2xx statuses, log and return an error
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Payment/DocumentType/{id}: {Status} - {Error}", DocumentTypeID, response.StatusCode, errorText);

                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, $"Error during request: {response.StatusCode}");
            }

            // Deserialize directly into ApiResponse<Payment_DocumentType_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_DocumentType_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, "Invalid response from server.");

            return apiResponse; 
        }


        public async Task<ApiResponse<Payment_DocumentType_Detail_DTO>> AddDocumentTypeAsync(Payment_DocumentType_Create_DTO DocumentType)
        {
            if (DocumentType == null) throw new ArgumentNullException(nameof(DocumentType));

            var response = await _httpClient.PostAsJsonAsync("/v1/Payment/DocumentType", DocumentType);
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP POST /Payments/DocumentType: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, $"HTTP error: {response.StatusCode}");

            }

            // Deserialize directly into ApiResponse<Payment_DocumentType_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_DocumentType_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, "Invalid response from server");

            return apiResponse;
        }

        public async Task<ApiResponse<Payment_DocumentType_Detail_DTO>> UpdateDocumentTypeAsync(Payment_DocumentType_Update_DTO editDto)
        {
            if (editDto == null || editDto.DocumentTypeID == Guid.Empty)
                throw new ArgumentNullException(nameof(editDto), "Invalid update DTO.");

            var response = await _httpClient.PutAsJsonAsync($"/v1/Payment/DocumentType/{editDto.DocumentTypeID}", editDto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /Payment/DocumentType/{id}: {Status} – {Error}", editDto.DocumentTypeID, response.StatusCode, error);
                return new ApiResponse<Payment_DocumentType_Detail_DTO> (false, $"HTTP error: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_DocumentType_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, "Invalid response from server");

            return apiResponse;
        }

        public async Task<ApiResponse<object>> DeleteDocumentTypeAsync(Guid DocumentTypeID)
        {
            if (DocumentTypeID == Guid.Empty)
                return new ApiResponse<object>(false, "Invalid payment document type ID.");

            // Endpoint DELETE: /Payment/{id}
            var response = await _httpClient.DeleteAsync($"/v1/Payment/DocumentType/{DocumentTypeID}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP DELETE /Payment/DocumentType/{id}: {Status} – {Error}", DocumentTypeID, response.StatusCode, error);
                return new ApiResponse<object>(false, $"HTTP error: {response.StatusCode}");
            }

            // The API might not return a body, so we can return Success = true
            return new ApiResponse<object>(true, "");
        }

    }
}
