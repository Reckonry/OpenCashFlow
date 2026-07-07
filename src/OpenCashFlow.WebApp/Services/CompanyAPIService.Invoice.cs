using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Companies;

namespace OpenCashFlow.WebApp.Services
{
    public partial class CompanyAPIService
    {
        public async Task<ApiResponse<IEnumerable<Company_Invoice_List_DTO>?>> GetCompanyInvoicesAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/Company/Invoices");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<IEnumerable<Company_Invoice_List_DTO>?>(false, "Company invoices non trovata.");

            // For other non-2xx statuses, log and return an error
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Company/invoices: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<IEnumerable<Company_Invoice_List_DTO>?>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<IEnumerable<Company_Invoice_List_DTO>?>();
            if (apiResponse == null)
                return new ApiResponse<IEnumerable<Company_Invoice_List_DTO>?>(false, "Risposta non valida dal server.");

            return new ApiResponse<IEnumerable<Company_Invoice_List_DTO>?>(true, "", apiResponse);
        }

        public async Task<ApiResponse<Company_Invoices_Detail_DTO?>> GetCompanyInvoiceByIDAsync(Guid InvoiceID)
        {
            var response = await _httpClient.GetAsync($"/v1/Company/Invoice/{InvoiceID}");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Company_Invoices_Detail_DTO?>(false, "Company invoices non trovata.");

            // For other non-2xx statuses, log and return an error
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
