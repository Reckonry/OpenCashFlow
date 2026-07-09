using System.Net;
using System.Net.Http.Json;
using OpenCashFlow.Contracts.DTOs;

namespace OpenCashFlow.WebApp.Services
{
    public class SetupAPIService(IHttpClientFactory httpClientFactory, ILogger<SetupAPIService> logger)
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient("API-Client");
        private readonly ILogger<SetupAPIService> _logger = logger;

        public async Task<SetupStatus_DTO?> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<SetupStatus_DTO>("/v1/Setup/status", cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to read setup status.");
                return null;
            }
        }

        public async Task<(bool Success, string? Message, SetupCompleted_DTO? Setup)> CompleteSetupAsync(SetupRequest_DTO request, CancellationToken cancellationToken = default)
        {
            var response = await _httpClient.PostAsJsonAsync("/v1/Setup", request, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                var setup = await response.Content.ReadFromJsonAsync<SetupCompleted_DTO>(cancellationToken: cancellationToken);
                return (true, null, setup);
            }

            if (response.StatusCode == HttpStatusCode.Conflict)
            {
                return (false, "This instance is already configured.", null);
            }

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogWarning("Setup failed with status {Status}: {Body}", response.StatusCode, body);
            return (false, "Unable to complete setup. Check the fields and try again.", null);
        }
    }
}
