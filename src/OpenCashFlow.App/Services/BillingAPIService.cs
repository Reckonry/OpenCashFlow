using global::Shared.DTOs.Billing;
using global::Shared.Models;
using System.Net.Http.Json;

namespace OpenCashFlow.App.Services
{
    public class BillingAPIService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<BillingAPIService> _logger;

        public BillingAPIService(IHttpClientFactory httpClientFactory, ILogger<BillingAPIService> logger)
        {
            _httpClient = httpClientFactory.CreateClient("API-Client");
            _logger = logger;
        }

        public async Task<ApiResponse<BillingPortal_DTO>> GetCustomerPortalAsync(CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Portal";

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP GET {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<BillingPortal_DTO>(false, "Errore durante il recupero dei dati billing.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BillingPortal_DTO>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<BillingPortal_DTO>(false, "Risposta non valida dal servizio Billing.");
                }

                return payload;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Errore di rete durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingPortal_DTO>(false, "Errore di connessione al servizio Billing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingPortal_DTO>(false, "Errore inatteso durante il recupero dei dati.");
            }
        }

        public async Task<ApiResponse<BillingPortalSession_DTO>> CreateCustomerPortalSessionAsync(string returnUrl, CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/CreatePortalSession";

            var request = new BillingPortalSessionRequest_DTO
            {
                ReturnUrl = returnUrl
            };

            try
            {
                using var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP POST {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<BillingPortalSession_DTO>(false, "Errore durante la creazione della sessione portal.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BillingPortalSession_DTO>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<BillingPortalSession_DTO>(false, "Risposta non valida dal servizio Billing.");
                }

                return payload;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Errore di rete durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingPortalSession_DTO>(false, "Errore di connessione al servizio Billing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingPortalSession_DTO>(false, "Errore inatteso.");
            }
        }

        public async Task<ApiResponse<BillingSubscriptionActionResult_DTO>> ChangePlanAsync(Guid subscriptionId, Guid newPlanId, CancellationToken cancellationToken = default)
        {
            var endpoint = $"/v1/Billing/Subscription/{subscriptionId}/ChangePlan";

            var request = new BillingChangePlanRequest_DTO
            {
                SubscriptionID = subscriptionId,
                NewPlanId = newPlanId
            };

            try
            {
                using var response = await _httpClient.PutAsJsonAsync(endpoint, request, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP PUT {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<BillingSubscriptionActionResult_DTO>(false, "Errore durante il cambio piano.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BillingSubscriptionActionResult_DTO>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<BillingSubscriptionActionResult_DTO>(false, "Risposta non valida dal servizio Billing.");
                }

                return payload;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Errore di rete durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingSubscriptionActionResult_DTO>(false, "Errore di connessione al servizio Billing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingSubscriptionActionResult_DTO>(false, "Errore inatteso.");
            }
        }
    }
}
