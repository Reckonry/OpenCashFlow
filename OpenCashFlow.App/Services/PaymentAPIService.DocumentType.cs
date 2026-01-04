using System.Text.Json;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.DTOs;

namespace OpenCashFlow.App.Services
{
    public partial class PaymentAPIService
    {
        public async Task<IEnumerable<Payment_DocumentType_List_DTO>?> GetDocumentTypesAsync()
        {
            var response = await _httpClient.GetAsync("v1/Payment/DocumentTypes");
            if (!response.IsSuccessStatusCode)
                throw new Exception("Errore nella chiamata al server");

            var DocumentTypes = await response.Content.ReadFromJsonAsync<IEnumerable<Payment_DocumentType_List_DTO>?>();
            return DocumentTypes;
        }

        public async Task<ApiResponse<Payment_DocumentType_Detail_DTO>> GetDocumentTypeAsync(Guid DocumentTypeID)
        {
            if (DocumentTypeID == Guid.Empty)
                return new ApiResponse<Payment_DocumentType_Detail_DTO> (false, "DocumentTypeID non può essere vuoto.");

            var response = await _httpClient.GetAsync($"/v1/Payment/DocumentType/{DocumentTypeID}");

            // In caso di 404 Not Found, restituisco envelope con Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Payment_DocumentType_Detail_DTO> (false, "Document Type non trovato." );

            // In caso di altri status non 2xx, logga e restituisci errore
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Payment/DocumentType/{id}: {Status} - {Error}", DocumentTypeID, response.StatusCode, errorText);

                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            // Deserializza direttamente in ApiResponse<Payment_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_DocumentType_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, "Risposta non valida dal server." );

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
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, $"Errore HTTP: {response.StatusCode}");

            }

            // Deserializziamo direttamente in ApiResponse<Payment_Method_Detail_DTO>
            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_DocumentType_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, "Risposta non valida dal server");

            return apiResponse;
        }

        public async Task<ApiResponse<Payment_DocumentType_Detail_DTO>> UpdateDocumentTypeAsync(Payment_DocumentType_Update_DTO editDto)
        {
            if (editDto == null || editDto.DocumentTypeID == Guid.Empty)
                throw new ArgumentNullException(nameof(editDto), "DTO per update non valido.");

            var response = await _httpClient.PutAsJsonAsync($"/v1/Payment/DocumentType/{editDto.DocumentTypeID}", editDto);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP PUT /Payment/DocumentType/{id}: {Status} – {Error}", editDto.DocumentTypeID, response.StatusCode, error);
                return new ApiResponse<Payment_DocumentType_Detail_DTO> (false, $"Errore HTTP: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_DocumentType_Detail_DTO>>();
            if (apiResponse == null)
                return new ApiResponse<Payment_DocumentType_Detail_DTO>(false, "Risposta non valida dal server");

            return apiResponse;
        }

        public async Task<ApiResponse<object>> DeleteDocumentTypeAsync(Guid DocumentTypeID)
        {
            if (DocumentTypeID == Guid.Empty)
                return new ApiResponse<object>(false, "ID DocumentType pagameto non valido.");

            // Endpoint DELETE: /Payment/{id}
            var response = await _httpClient.DeleteAsync($"/v1/Payment/DocumentType/{DocumentTypeID}");
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP DELETE /Payment/DocumentType/{id}: {Status} – {Error}", DocumentTypeID, response.StatusCode, error);
                return new ApiResponse<object>(false, $"Errore HTTP: {response.StatusCode}");
            }

            // L’API potrebbe non restituire body, quindi possiamo restituire Success = true
            return new ApiResponse<object>(true, "");
        }

    }
}
