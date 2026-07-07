using System;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using OpenCashFlow.Contracts.DTOs.Billing;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Admin.Services
{
    /// <summary>
    /// Admin-specific billing API methods
    /// </summary>
    public partial class BillingPlansAPIService
    {
        public async Task<ApiResponse<BillingSubscriptionDetail_DTO>> GetSubscriptionDetailAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        {
            var endpoint = $"/v1/Billing/Admin/Subscription/{subscriptionId}";

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP GET {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<BillingSubscriptionDetail_DTO>(false, "Errore durante il recupero dei dettagli subscription.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<BillingSubscriptionDetail_DTO>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<BillingSubscriptionDetail_DTO>(false, "Risposta non valida dal servizio Billing.");
                }

                return payload;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<BillingSubscriptionDetail_DTO>(false, "Errore inatteso durante il recupero dei dettagli.");
            }
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> SyncSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/Sync";
            var request = new BillingSyncSubscriptionRequest_DTO { SubscriptionID = subscriptionId };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> ExtendTrialAsync(Guid subscriptionId, int days, string? reason, CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/ExtendTrial";
            var request = new BillingExtendTrialRequest_DTO
            {
                SubscriptionID = subscriptionId,
                Days = days,
                Reason = reason
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> ApplyCreditAsync(Guid subscriptionId, decimal amount, string description, string? currency = "EUR", CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/ApplyCredit";
            var request = new BillingApplyCreditRequest_DTO
            {
                SubscriptionID = subscriptionId,
                Amount = amount,
                Description = description,
                Currency = currency
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> SendInvoiceAsync(string stripeInvoiceId, string? emailOverride = null, CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Invoice/Send";
            var request = new BillingSendInvoiceRequest_DTO
            {
                StripeInvoiceID = stripeInvoiceId,
                EmailOverride = emailOverride
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> AdminCancelSubscriptionAsync(Guid subscriptionId, bool immediately, string adminReason, bool notifyCustomer = true, CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/AdminCancel";
            var request = new BillingAdminCancelRequest_DTO
            {
                SubscriptionID = subscriptionId,
                Immediately = immediately,
                AdminReason = adminReason,
                NotifyCustomer = notifyCustomer
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<IEnumerable<StripeWebhookEventSummary_DTO>>> GetWebhookEventsAsync(Guid subscriptionId, int limit = 50, CancellationToken cancellationToken = default)
        {
            var endpoint = $"/v1/Billing/Admin/Subscription/{subscriptionId}/WebhookEvents?limit={limit}";

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP GET {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<IEnumerable<StripeWebhookEventSummary_DTO>>(false, "Errore durante il recupero degli eventi webhook.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<IEnumerable<StripeWebhookEventSummary_DTO>>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<IEnumerable<StripeWebhookEventSummary_DTO>>(false, "Risposta non valida dal servizio Billing.");
                }

                return payload;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<IEnumerable<StripeWebhookEventSummary_DTO>>(false, "Errore inatteso durante il recupero degli eventi.");
            }
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> ManualUpdateSubscriptionAsync(
            Guid subscriptionId,
            DateTime nextBillingDate,
            DateTime endDate,
            RenewalStatus renewalStatus,
            double cost,
            double? discount = null,
            string? promoCode = null,
            DateTime? discountExpiration = null,
            string? adminNotes = null,
            CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/ManualUpdate";
            var request = new BillingManualUpdateSubscriptionRequest_DTO
            {
                SubscriptionID = subscriptionId,
                NextBillingDate = nextBillingDate,
                EndDate = endDate,
                RenewalStatus = renewalStatus,
                Cost = cost,
                Discount = discount,
                PromoCode = promoCode,
                DiscountExpiration = discountExpiration,
                AdminNotes = adminNotes
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> ManualExtendSubscriptionAsync(
            Guid subscriptionId,
            int months,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/ManualExtend";
            var request = new BillingManualExtendRequest_DTO
            {
                SubscriptionID = subscriptionId,
                Months = months,
                Reason = reason
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> ManualActivateSubscriptionAsync(
            Guid subscriptionId,
            Guid planId,
            DateTime startDate,
            DateTime endDate,
            BillingCycle billingCycle,
            double cost,
            string? reason = null,
            CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/ManualActivate";
            var request = new BillingManualActivateRequest_DTO
            {
                SubscriptionID = subscriptionId,
                PlanID = planId,
                StartDate = startDate,
                EndDate = endDate,
                BillingCycle = billingCycle,
                Cost = cost,
                Reason = reason
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<BillingAdminActionResult_DTO>> AdminChangePlanAsync(
            Guid subscriptionId,
            Guid newPlanId,
            string? reason = null,
            bool prorate = true,
            CancellationToken cancellationToken = default)
        {
            var endpoint = "/v1/Billing/Admin/Subscription/ChangePlan";
            var request = new BillingChangePlanRequest_DTO
            {
                SubscriptionID = subscriptionId,
                NewPlanId = newPlanId,
                Reason = reason,
                Prorate = prorate
            };

            return await PostAsync<BillingAdminActionResult_DTO>(endpoint, request, cancellationToken);
        }

        public async Task<ApiResponse<List<BillingAuditLogEntry_DTO>>> GetSubscriptionAuditLogAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            var endpoint = $"/v1/Billing/Admin/Subscription/{subscriptionId}/AuditLog";

            try
            {
                using var response = await _httpClient.GetAsync(endpoint, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP GET {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<List<BillingAuditLogEntry_DTO>>(false, "Errore durante il recupero dell'audit log.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<List<BillingAuditLogEntry_DTO>>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<List<BillingAuditLogEntry_DTO>>(false, "Risposta non valida dal servizio Billing.");
                }

                return payload;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "Errore di rete durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<List<BillingAuditLogEntry_DTO>>(false, "Errore di connessione al servizio Billing.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<List<BillingAuditLogEntry_DTO>>(false, "Errore inatteso durante il recupero dell'audit log.");
            }
        }

        private async Task<ApiResponse<T>> PostAsync<T>(string endpoint, object request, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.PostAsJsonAsync(endpoint, request, cancellationToken).ConfigureAwait(false);

                if (!response.IsSuccessStatusCode)
                {
                    var errorText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                    _logger.LogError("Errore HTTP POST {Endpoint}: {StatusCode} - {Body}", endpoint, response.StatusCode, errorText);
                    return new ApiResponse<T>(false, "Errore durante la comunicazione con il servizio Billing.");
                }

                var payload = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(cancellationToken: cancellationToken).ConfigureAwait(false);

                if (payload == null)
                {
                    _logger.LogError("Risposta vuota o non deserializzabile da {Endpoint}", endpoint);
                    return new ApiResponse<T>(false, "Risposta non valida dal servizio Billing.");
                }

                return payload;
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Errore inatteso durante la chiamata a {Endpoint}", endpoint);
                return new ApiResponse<T>(false, "Errore inatteso durante l'esecuzione dell'operazione.");
            }
        }
    }
}
