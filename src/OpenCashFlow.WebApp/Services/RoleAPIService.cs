using global::Shared.DTOs.Identity;
using global::Shared.Models;

namespace OpenCashFlow.WebApp.Services
{
    public class RoleAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RoleAPIService> _logger;

        public RoleAPIService(IHttpClientFactory httpClientFactory, ILogger<RoleAPIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API-Client");
            _logger = logger;
        }

        public async Task<ApiResponse<IEnumerable<Role_List_DTO>>> GetVisibleRolesAsync()
        {
            var response = await _httpClient.GetAsync($"/v1/Roles/Visible");

            if (!response.IsSuccessStatusCode)
            {
                var errorText = await response.Content.ReadAsStringAsync();
                _logger.LogError("Errore HTTP GET /v1/Roles/Visible: {Status} - {Error}", response.StatusCode, errorText);
                return new ApiResponse<IEnumerable<Role_List_DTO>>(false, $"Errore durante la richiesta: {response.StatusCode}");
            }

            var roles = await response.Content.ReadFromJsonAsync<IEnumerable<Role_List_DTO>>();
            if (roles == null)
                return new ApiResponse<IEnumerable<Role_List_DTO>>(false, "Risposta non valida dal server.");

            return new ApiResponse<IEnumerable<Role_List_DTO>>(true, string.Empty, roles);
        }
    }
}

