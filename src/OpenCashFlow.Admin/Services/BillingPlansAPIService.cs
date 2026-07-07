using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Caching.Memory;
using OpenCashFlow.Contracts.DTOs.Billing;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Admin.Services
{
    public partial class BillingPlansAPIService
    {
        private static readonly JsonSerializerOptions CacheSerializerOptions = new(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
        };

        private static readonly MemoryCacheEntryOptions DefaultCacheEntryOptions = new()
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
        };

        private readonly HttpClient _httpClient;
        private readonly ILogger<BillingPlansAPIService> _logger;
        private readonly IMemoryCache _cache;

        public BillingPlansAPIService(IHttpClientFactory httpClientFactory, ILogger<BillingPlansAPIService> logger, IMemoryCache cache)
        {
            _httpClient = httpClientFactory.CreateClient("API-Client");
            _logger = logger;
            _cache = cache;
        }

        public Task<ApiResponse<IReadOnlyList<BillingPlan_List_DTO>>> GetPlansAsync(BillingPlan_Filter_DTO filters, CancellationToken cancellationToken = default) =>
            GetCollectionAsync<BillingPlan_List_DTO, BillingPlan_Filter_DTO>(
                endpointBase: "/v1/Billing/Plans",
                filters: filters ?? new BillingPlan_Filter_DTO(),
                queryFactory: BuildPlanQuery,
                cacheResource: "plans",
                notFoundMessage: "Nessun piano disponibile.",
                cancellationToken);

        public Task<ApiResponse<IReadOnlyList<BillingSubscription_List_DTO>>> GetSubscriptionsAsync(BillingSubscription_Filter_DTO filters, CancellationToken cancellationToken = default) =>
            GetCollectionAsync<BillingSubscription_List_DTO, BillingSubscription_Filter_DTO>(
                endpointBase: "/v1/Billing/Subscriptions",
                filters: filters ?? new BillingSubscription_Filter_DTO(),
                queryFactory: BuildSubscriptionQuery,
                cacheResource: "subscriptions",
                notFoundMessage: "Nessuna subscription trovata.",
                cancellationToken);

        public async Task<ApiResponse<BillingDashboardKPI_DTO>> GetDashboardKPIAsync(CancellationToken cancellationToken = default)
        {
            const string cacheKey = "BillingPlansAPIService::dashboard-kpi";
            if (_cache.TryGetValue(cacheKey, out BillingDashboardKPI_DTO? cached))
            {
                return new ApiResponse<BillingDashboardKPI_DTO>(true, string.Empty, cached);
            }

            const string endpoint = "/v1/Billing/Dashboard/KPI";

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP GET {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<BillingDashboardKPI_DTO>(false, "Errore durante la comunicazione con il servizio Billing.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BillingDashboardKPI_DTO>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<BillingDashboardKPI_DTO>(false, "Risposta non valida dal servizio Billing.");
                }

                if (payload.Success && payload.Data != null)
                {
                    _cache.Set(cacheKey, payload.Data, DefaultCacheEntryOptions);
                }

                return new ApiResponse<BillingDashboardKPI_DTO>(payload.Success, payload.Message, payload.Data, payload.Errors);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingDashboardKPI_DTO>(false, "Errore inatteso durante il recupero dei KPI dal servizio Billing.");
            }
        }

        private async Task<ApiResponse<IReadOnlyList<TDto>>> GetCollectionAsync<TDto, TFilters>(
            string endpointBase,
            TFilters filters,
            Func<TFilters, IEnumerable<KeyValuePair<string, string?>>> queryFactory,
            string cacheResource,
            string notFoundMessage,
            CancellationToken cancellationToken)
        {
            var cacheKey = BuildCacheKey(cacheResource, filters);
            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<TDto>? cached))
            {
                return new ApiResponse<IReadOnlyList<TDto>>(true, string.Empty, cached);
            }

            var query = queryFactory(filters);
            var endpoint = QueryHelpers.AddQueryString(endpointBase, query);

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    return new ApiResponse<IReadOnlyList<TDto>>(false, notFoundMessage, Array.Empty<TDto>());
                }

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP GET {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<IReadOnlyList<TDto>>(false, "Errore durante la comunicazione con il servizio Billing.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<TDto>>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<IReadOnlyList<TDto>>(false, "Risposta non valida dal servizio Billing.");
                }

                var items = payload.Data?.ToList() ?? new List<TDto>();

                if (payload.Success)
                {
                    _cache.Set(cacheKey, items, DefaultCacheEntryOptions);
                }

                return new ApiResponse<IReadOnlyList<TDto>>(payload.Success, payload.Message, items, payload.Errors);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<IReadOnlyList<TDto>>(false, "Errore inatteso durante il recupero dei dati dal servizio Billing.");
            }
        }

        private static IEnumerable<KeyValuePair<string, string?>> BuildPlanQuery(BillingPlan_Filter_DTO filters)
        {
            var query = new List<KeyValuePair<string, string?>>();

            query.Add(new("OnlyActive", filters.OnlyActive.ToString().ToLowerInvariant()));
            query.Add(new("Page", filters.Page.ToString(CultureInfo.InvariantCulture)));
            query.Add(new("PageSize", filters.PageSize.ToString(CultureInfo.InvariantCulture)));

            if (filters.Visible.HasValue)
            {
                query.Add(new("Visible", filters.Visible.Value.ToString().ToLowerInvariant()));
            }

            if (filters.HasTrial.HasValue)
            {
                query.Add(new("HasTrial", filters.HasTrial.Value.ToString().ToLowerInvariant()));
            }

            if (filters.BillingCycle.HasValue)
            {
                query.Add(new("BillingCycle", filters.BillingCycle.Value.ToString()));
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                query.Add(new("Search", filters.Search.Trim()));
            }

            return query;
        }

        private static IEnumerable<KeyValuePair<string, string?>> BuildSubscriptionQuery(BillingSubscription_Filter_DTO filters)
        {
            var query = new List<KeyValuePair<string, string?>>();

            query.Add(new("IncludeAllCompanies", filters.IncludeAllCompanies.ToString().ToLowerInvariant()));
            query.Add(new("Page", filters.Page.ToString(CultureInfo.InvariantCulture)));
            query.Add(new("PageSize", filters.PageSize.ToString(CultureInfo.InvariantCulture)));

            if (filters.SubscriptionID.HasValue)
            {
                query.Add(new("SubscriptionID", filters.SubscriptionID.Value.ToString()));
            }

            if (filters.TenantID.HasValue)
            {
                query.Add(new("TenantID", filters.TenantID.Value.ToString()));
            }

            if (filters.PlanID.HasValue)
            {
                query.Add(new("PlanID", filters.PlanID.Value.ToString()));
            }

            if (filters.RenewalStatus.HasValue)
            {
                query.Add(new("RenewalStatus", filters.RenewalStatus.Value.ToString()));
            }

            if (filters.RenewalStatuses != null)
            {
                foreach (var status in filters.RenewalStatuses.Where(s => Enum.IsDefined(typeof(RenewalStatus), s)))
                {
                    query.Add(new("RenewalStatuses", status.ToString()));
                }
            }

            if (filters.BillingCycle.HasValue)
            {
                query.Add(new("BillingCycle", filters.BillingCycle.Value.ToString()));
            }

            if (filters.ActiveOn.HasValue)
            {
                query.Add(new("ActiveOn", filters.ActiveOn.Value.ToString("O", CultureInfo.InvariantCulture)));
            }

            if (filters.StartFrom.HasValue)
            {
                query.Add(new("StartFrom", filters.StartFrom.Value.ToString("O", CultureInfo.InvariantCulture)));
            }

            if (filters.StartTo.HasValue)
            {
                query.Add(new("StartTo", filters.StartTo.Value.ToString("O", CultureInfo.InvariantCulture)));
            }

            if (filters.EndFrom.HasValue)
            {
                query.Add(new("EndFrom", filters.EndFrom.Value.ToString("O", CultureInfo.InvariantCulture)));
            }

            if (filters.EndTo.HasValue)
            {
                query.Add(new("EndTo", filters.EndTo.Value.ToString("O", CultureInfo.InvariantCulture)));
            }

            if (filters.NextBillingFrom.HasValue)
            {
                query.Add(new("NextBillingFrom", filters.NextBillingFrom.Value.ToString("O", CultureInfo.InvariantCulture)));
            }

            if (filters.NextBillingTo.HasValue)
            {
                query.Add(new("NextBillingTo", filters.NextBillingTo.Value.ToString("O", CultureInfo.InvariantCulture)));
            }

            if (filters.HasTrial.HasValue)
            {
                query.Add(new("HasTrial", filters.HasTrial.Value.ToString().ToLowerInvariant()));
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                query.Add(new("Search", filters.Search.Trim()));
            }

            return query;
        }

        private static string BuildCacheKey<TFilters>(string resource, TFilters filters)
        {
            var serializedFilters = JsonSerializer.Serialize(filters, CacheSerializerOptions);
            return $"BillingPlansAPIService::{resource}::{serializedFilters}";
        }
    }
}
