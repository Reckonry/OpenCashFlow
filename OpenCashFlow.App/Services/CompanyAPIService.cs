using global::Shared.DTOs;
using global::Shared.Models;

namespace OpenCashFlow.App.Services
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

            // In caso di 404 Not Found, restituisco envelope con Success=false
            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                return new ApiResponse<Company_Detail_DTO>(false, "Company non trovata.");

            // In caso di altri status non 2xx, logga e restituisci errore
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
