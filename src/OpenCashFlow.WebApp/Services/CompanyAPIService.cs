using OpenCashFlow.Contracts.DTOs;

namespace OpenCashFlow.WebApp.Services
{
    public partial class CompanyAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<CompanyAPIService> _logger;

        public CompanyAPIService(IHttpClientFactory httpClientFactory, ILogger<CompanyAPIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API-Client");
            _logger = logger;
        }

        public async Task<ApiResponse<Company_Detail_DTO>> GetCompanyAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/Company/");

            // If 404 Not Found, return an envelope with Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Company_Detail_DTO>(false, "Company non trovata.");

            // For other non-2xx statuses, log and return an error
            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Company/: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<Company_Detail_DTO>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            var apiResponse = await response.Content.ReadFromJsonAsync<Company_Detail_DTO>();
            if (apiResponse == null)
                return new ApiResponse<Company_Detail_DTO>(false, "Risposta non valida dal server.");

            return new ApiResponse<Company_Detail_DTO>(true, "", apiResponse);
        }
    }
}
