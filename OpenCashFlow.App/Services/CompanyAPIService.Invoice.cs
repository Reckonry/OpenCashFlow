using global::Shared.DTOs;
using global::Shared.DTOs.Companies;
using global::Shared.Models;

namespace OpenCashFlow.App.Services
{
    public partial class CompanyAPIService
    {
        public async Task<ApiResponse<IEnumerable<Company_Invoice>?>> GetCompanyInvoicesAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/Company/Invoices");

            // In caso di 404 Not Found, restituisco envelope con Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<IEnumerable<Company_Invoice>?>(false, "Company invoices non trovata.");

            // In caso di altri status non 2xx, logga e restituisci errore
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Company/invoices: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<IEnumerable<Company_Invoice>?>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<Company_Invoice>?>();
            if (apiResponse == null)
                return new ApiResponse<IEnumerable<Company_Invoice>?>(false, "Risposta non valida dal server.");

            return new ApiResponse<IEnumerable<Company_Invoice>?>(true, "", apiResponse);
        }

        public async Task<ApiResponse<Company_Invoices_Detail_DTO?>> GetCompanyInvoiceByIDAsync(Guid InvoiceID)
        {
            var response = await _httpClient.GetAsync($"/v1/Company/Invoice/{InvoiceID}");

            // In caso di 404 Not Found, restituisco envelope con Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Company_Invoices_Detail_DTO?>(false, "Company invoices non trovata.");

            // In caso di altri status non 2xx, logga e restituisci errore
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Company/invoices/{InvoiceID}: {Status} - {Error}", InvoiceID, response.StatusCode, errorText);
                return new ApiResponse<Company_Invoices_Detail_DTO?>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<Company_Invoices_Detail_DTO?>();
            if (apiResponse == null)
                return new ApiResponse<Company_Invoices_Detail_DTO?>(false, "Risposta non valida dal server.");

            return new ApiResponse<Company_Invoices_Detail_DTO?>(true, "", apiResponse);
        }
    }
}
