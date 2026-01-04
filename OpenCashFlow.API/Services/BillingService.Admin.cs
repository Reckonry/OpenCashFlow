using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.DTOs.Billing;
using global::Shared.Models;
using global::Shared.Models.Stripe;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Services
{
    /// <summary>
    /// Admin-specific billing service methods
    /// </summary>
    public partial class BillingService
    {
        public async Task<BillingSubscriptionDetail_DTO> GetSubscriptionDetailAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscription = await _billingRepository.GetSubscriptionByIdAsync(subscriptionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {subscriptionId} not found.");

            var company = await _billingRepository.GetCompanyByIdAsync(subscription.TenantID, cancellationToken)
                ?? throw new KeyNotFoundException($"Company {subscription.TenantID} not found.");

            var plan = await _billingRepository.GetPlanByIdAsync(subscription.PlanID, cancellationToken)
                ?? throw new KeyNotFoundException($"Plan {subscription.PlanID} not found.");

            var detail = new BillingSubscriptionDetail_DTO
            {
                SubscriptionID = subscription.SubscriptionID,
                TenantID = subscription.TenantID,
                CompanyName = company.CompanyName,
                BillingEmail = company.BillingEmail,
                PlanID = subscription.PlanID,
                PlanName = plan.Name,
                StartDate = subscription.StartDate,
                EndDate = subscription.EndDate,
                NextBillingDate = subscription.NextBillingDate,
                BillingCycle = subscription.BillingCycle,
                BillingDay = subscription.BillingDay,
                RenewalStatus = subscription.RenewalStatus,
                Cost = subscription.Cost,
                Discount = subscription.Discount,
                PromoCode = subscription.PromoCode,
                DiscountExpiration = subscription.DiscountExpiration,
                CancellationDate = subscription.CancellationDate,
                CancellationReason = subscription.CancellationReason,
                StripeSubscriptionID = subscription.StripeSubscriptionID,
                StripePriceID = subscription.StripePriceID,
                StripeInvoiceID = subscription.StripeInvoiceID,
                StripeCustomerID = company.StripeCustomerID,
                StripeDefaultPaymentMethodID = company.StripeDefaultPaymentMethodID
            };

            // Fetch payment methods if customer exists in Stripe
            if (!string.IsNullOrWhiteSpace(company.StripeCustomerID))
            {
                try
                {
                    var customer = await _stripeService.GetCustomerAsync(company.StripeCustomerID, cancellationToken);
                    if (customer != null && customer.DefaultSourceId != null)
                    {
                        // For now, just add basic payment method info
                        // Full implementation would use PaymentMethodService.List
                        detail.PaymentMethods.Add(new StripePaymentMethod_DTO
                        {
                            PaymentMethodId = customer.DefaultSourceId,
                            Type = "card",
                            IsDefault = true
                        });
                    }
                }
                catch (StripeException ex)
                {
                    _logger.LogWarning(ex, "Failed to fetch payment methods for customer {CustomerId}", company.StripeCustomerID);
                }
            }

            // Fetch recent webhook events for this subscription
            detail.RecentWebhookEvents = (await GetSubscriptionWebhookEventsAsync(subscriptionId, 20, cancellationToken)).ToList();

            return detail;
        }

        public async Task<BillingAdminActionResult_DTO> SyncSubscriptionFromStripeAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscription = await _billingRepository.GetSubscriptionByIdAsync(subscriptionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {subscriptionId} not found.");

            if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID))
            {
                throw new InvalidOperationException("La subscription non ha un ID Stripe associato.");
            }

            // Get subscription from Stripe
            var stripeSubscription = await _stripeService.GetSubscriptionAsync(subscription.StripeSubscriptionID, cancellationToken);
            if (stripeSubscription == null)
            {
                throw new InvalidOperationException("Subscription non trovata in Stripe.");
            }

            // Manual sync - update local database with Stripe data
            subscription.RenewalStatus = MapStripeStatus(stripeSubscription.Status);
            // Note: Stripe Subscription object has BillingCycleAnchor but not CurrentPeriodEnd/Start directly
            // For full sync, we'd need to fetch from Items or use StripeSyncService

            await _billingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription {SubscriptionId} synced from Stripe {StripeSubscriptionId}", subscriptionId, subscription.StripeSubscriptionID);

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = "Subscription sincronizzata con successo da Stripe."
            };
        }

        public async Task<BillingAdminActionResult_DTO> ExtendTrialAsync(BillingExtendTrialRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await _billingRepository.GetSubscriptionByIdAsync(request.SubscriptionID, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionID} not found.");

            if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID))
            {
                throw new InvalidOperationException("La subscription non ha un ID Stripe associato.");
            }

            // Extend trial in Stripe
            var stripeSubscription = await _stripeService.GetSubscriptionAsync(subscription.StripeSubscriptionID, cancellationToken);
            if (stripeSubscription == null)
            {
                throw new InvalidOperationException("Subscription non trovata in Stripe.");
            }

            var newTrialEnd = stripeSubscription.TrialEnd.HasValue
                ? stripeSubscription.TrialEnd.Value.AddDays(request.Days)
                : DateTime.UtcNow.AddDays(request.Days);

            // Update subscription trial end using Stripe API
            var service = new SubscriptionService();
            await service.UpdateAsync(subscription.StripeSubscriptionID, new SubscriptionUpdateOptions
            {
                TrialEnd = newTrialEnd
            }, cancellationToken: cancellationToken);

            _logger.LogInformation("Trial extended by {Days} days for subscription {SubscriptionId}. Reason: {Reason}",
                request.Days, request.SubscriptionID, request.Reason ?? "N/A");

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = $"Trial esteso di {request.Days} giorni fino al {newTrialEnd:yyyy-MM-dd}."
            };
        }

        public async Task<BillingAdminActionResult_DTO> ApplyCreditAsync(BillingApplyCreditRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await _billingRepository.GetSubscriptionByIdAsync(request.SubscriptionID, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionID} not found.");

            var company = await _billingRepository.GetCompanyByIdAsync(subscription.TenantID, cancellationToken)
                ?? throw new KeyNotFoundException($"Company {subscription.TenantID} not found.");

            if (string.IsNullOrWhiteSpace(company.StripeCustomerID))
            {
                throw new InvalidOperationException("La company non ha un customer Stripe associato.");
            }

            // Create a customer balance transaction (credit) in Stripe
            var creditAmount = -(long)(request.Amount * 100); // Negative for credit
            var currency = request.Currency?.ToLowerInvariant() ?? "eur";

            var service = new CustomerBalanceTransactionService();
            await service.CreateAsync(company.StripeCustomerID, new CustomerBalanceTransactionCreateOptions
            {
                Amount = creditAmount,
                Currency = currency,
                Description = request.Description
            }, cancellationToken: cancellationToken);

            _logger.LogInformation("Credit of {Amount} {Currency} applied to subscription {SubscriptionId} (customer {CustomerId}). Description: {Description}",
                request.Amount, currency, request.SubscriptionID, company.StripeCustomerID, request.Description);

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = $"Credito di {request.Amount:F2} {currency.ToUpperInvariant()} applicato con successo."
            };
        }

        public async Task<BillingAdminActionResult_DTO> SendInvoiceManuallyAsync(BillingSendInvoiceRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            if (string.IsNullOrWhiteSpace(request.StripeInvoiceID))
            {
                throw new ArgumentException("Stripe Invoice ID is required.", nameof(request));
            }

            // Send the invoice via Stripe
            await _stripeService.SendInvoiceAsync(request.StripeInvoiceID, cancellationToken);

            _logger.LogInformation("Invoice {InvoiceId} sent manually", request.StripeInvoiceID);

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = "Fattura inviata con successo al cliente."
            };
        }

        public async Task<BillingAdminActionResult_DTO> AdminCancelSubscriptionAsync(BillingAdminCancelRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await _billingRepository.GetSubscriptionByIdAsync(request.SubscriptionID, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionID} not found.");

            if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID))
            {
                throw new InvalidOperationException("La subscription non ha un ID Stripe associato.");
            }

            // Cancel in Stripe
            await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionID, request.Immediately, cancellationToken);

            // Update in database
            subscription.CancellationDate = DateTime.UtcNow;
            subscription.CancellationReason = $"ADMIN: {request.AdminReason}";
            subscription.RenewalStatus = RenewalStatus.CANCELLED;
            await _billingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogWarning("Subscription {SubscriptionId} cancelled by admin. Immediately: {Immediately}. Reason: {Reason}",
                request.SubscriptionID, request.Immediately, request.AdminReason);

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = request.Immediately
                    ? "Subscription cancellata immediatamente."
                    : "Subscription cancellata a fine periodo di fatturazione."
            };
        }

        public async Task<IEnumerable<StripeWebhookEventSummary_DTO>> GetSubscriptionWebhookEventsAsync(Guid subscriptionId, int limit, CancellationToken cancellationToken)
        {
            var subscription = await _billingRepository.GetSubscriptionByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null || string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID))
            {
                return Enumerable.Empty<StripeWebhookEventSummary_DTO>();
            }

            // Query webhook events from database if dbContext is available
            if (_dbContext == null)
            {
                return Enumerable.Empty<StripeWebhookEventSummary_DTO>();
            }

            var events = await _dbContext.Stripe_Webhook_Events
                .Where(e => e.Payload.Contains(subscription.StripeSubscriptionID))
                .OrderByDescending(e => e.ReceivedUtc)
                .Take(limit)
                .ToListAsync(cancellationToken);

            return events.Select(e => new StripeWebhookEventSummary_DTO
            {
                Id = e.Id,
                EventId = e.EventId,
                EventType = e.EventType,
                CreatedUtc = e.CreatedUtc,
                ReceivedUtc = e.ReceivedUtc,
                ProcessedUtc = e.ProcessedUtc,
                Success = e.Success,
                ErrorMessage = e.ErrorMessage
            });
        }

        private static RenewalStatus MapStripeStatus(string? stripeStatus)
        {
            return stripeStatus?.ToLowerInvariant() switch
            {
                "active" => RenewalStatus.ACTIVE,
                "trialing" => RenewalStatus.TRIAL,
                "past_due" => RenewalStatus.SUSPENDED,
                "canceled" => RenewalStatus.CANCELLED,
                "unpaid" => RenewalStatus.EXPIRED,
                _ => RenewalStatus.PAUSED
            };
        }

        #region Manual Subscription Management (No Stripe)

        public async Task<BillingAdminActionResult_DTO> ManualUpdateSubscriptionAsync(BillingManualUpdateSubscriptionRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await _billingRepository.GetSubscriptionByIdAsync(request.SubscriptionID, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionID} not found.");

            // Aggiorna i dati della subscription manualmente
            subscription.NextBillingDate = request.NextBillingDate;
            subscription.EndDate = request.EndDate;
            subscription.RenewalStatus = request.RenewalStatus;
            subscription.Cost = request.Cost;
            subscription.Discount = request.Discount;
            subscription.PromoCode = request.PromoCode;
            subscription.DiscountExpiration = request.DiscountExpiration;

            await _billingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription {SubscriptionId} aggiornata manualmente. Nuovo stato: {Status}, Prossimo billing: {NextBilling}",
                request.SubscriptionID, request.RenewalStatus, request.NextBillingDate);

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = "Subscription aggiornata con successo."
            };
        }

        public async Task<BillingAdminActionResult_DTO> ManualExtendSubscriptionAsync(BillingManualExtendRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await _billingRepository.GetSubscriptionByIdAsync(request.SubscriptionID, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionID} not found.");

            // Estendi le date
            subscription.EndDate = subscription.EndDate.AddMonths(request.Months);
            subscription.NextBillingDate = subscription.NextBillingDate.AddMonths(request.Months);

            // Se era scaduta o cancellata, riattiva
            if (subscription.RenewalStatus == RenewalStatus.EXPIRED || subscription.RenewalStatus == RenewalStatus.CANCELLED)
            {
                subscription.RenewalStatus = RenewalStatus.ACTIVE;
                subscription.CancellationDate = null;
                subscription.CancellationReason = null;
            }

            await _billingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription {SubscriptionId} estesa manualmente di {Months} mesi. Nuova fine: {EndDate}. Motivo: {Reason}",
                request.SubscriptionID, request.Months, subscription.EndDate, request.Reason ?? "N/A");

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = $"Subscription estesa di {request.Months} mesi fino al {subscription.EndDate:dd/MM/yyyy}."
            };
        }

        public async Task<BillingAdminActionResult_DTO> ManualActivateSubscriptionAsync(BillingManualActivateRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await _billingRepository.GetSubscriptionByIdAsync(request.SubscriptionID, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionID} not found.");

            var plan = await _billingRepository.GetPlanByIdAsync(request.PlanID, cancellationToken)
                ?? throw new KeyNotFoundException($"Plan {request.PlanID} not found.");

            // Attiva/Riattiva la subscription manualmente
            subscription.PlanID = request.PlanID;
            subscription.StartDate = request.StartDate;
            subscription.EndDate = request.EndDate;
            subscription.BillingCycle = request.BillingCycle;
            subscription.Cost = request.Cost;
            subscription.RenewalStatus = RenewalStatus.ACTIVE;
            subscription.NextBillingDate = request.EndDate; // Il prossimo billing sarà alla fine del periodo
            subscription.CancellationDate = null;
            subscription.CancellationReason = null;

            // Cancella i riferimenti Stripe se presenti (gestione manuale)
            subscription.StripeSubscriptionID = null;
            subscription.StripePriceID = null;
            subscription.StripeInvoiceID = null;

            await _billingRepository.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Subscription {SubscriptionId} attivata/riattivata manualmente. Piano: {PlanName}, Periodo: {StartDate} - {EndDate}. Motivo: {Reason}",
                request.SubscriptionID, plan.Name, request.StartDate, request.EndDate, request.Reason ?? "N/A");

            return new BillingAdminActionResult_DTO
            {
                Success = true,
                Message = $"Subscription attivata con piano {plan.Name} dal {request.StartDate:dd/MM/yyyy} al {request.EndDate:dd/MM/yyyy}."
            };
        }

        public async Task<BillingAdminActionResult_DTO> AdminChangePlanAsync(BillingChangePlanRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await _billingRepository.GetSubscriptionByIdAsync(request.SubscriptionID, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {request.SubscriptionID} not found.");

            var oldPlan = await _billingRepository.GetPlanByIdAsync(subscription.PlanID, cancellationToken)
                ?? throw new KeyNotFoundException($"Current plan {subscription.PlanID} not found.");

            var newPlan = await _billingRepository.GetPlanByIdAsync(request.NewPlanId, cancellationToken)
                ?? throw new KeyNotFoundException($"New plan {request.NewPlanId} not found.");

            var isManualSubscription = string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID);

            if (isManualSubscription)
            {
                // Gestione manuale - aggiorna solo il database
                subscription.PlanID = request.NewPlanId;

                // Usa il prezzo del nuovo piano direttamente
                subscription.Cost = (double)newPlan.Price;

                await _billingRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Piano cambiato manualmente per subscription {SubscriptionId} da {OldPlan} a {NewPlan}. Motivo: {Reason}",
                    request.SubscriptionID, oldPlan.Name, newPlan.Name, request.Reason ?? "N/A");

                return new BillingAdminActionResult_DTO
                {
                    Success = true,
                    Message = $"Piano cambiato da '{oldPlan.Name}' a '{newPlan.Name}'. Nuovo costo: €{subscription.Cost:F2}"
                };
            }
            else
            {
                // Gestione Stripe - usa il metodo esistente
                if (string.IsNullOrWhiteSpace(newPlan.StripePriceID))
                {
                    throw new InvalidOperationException($"Il nuovo piano '{newPlan.Name}' non ha un Price ID Stripe configurato.");
                }

                var updatedStripeSubscription = await _stripeService.ChangeSubscriptionPlanAsync(
                    subscription.StripeSubscriptionID!,
                    newPlan.StripePriceID!,
                    cancellationToken);

                // Aggiorna anche il database
                subscription.PlanID = request.NewPlanId;
                subscription.StripePriceID = newPlan.StripePriceID;

                await _billingRepository.SaveChangesAsync(cancellationToken);

                _logger.LogInformation("Piano cambiato tramite Stripe per subscription {SubscriptionId} da {OldPlan} a {NewPlan}. Prorate: {Prorate}. Motivo: {Reason}",
                    request.SubscriptionID, oldPlan.Name, newPlan.Name, request.Prorate, request.Reason ?? "N/A");

                return new BillingAdminActionResult_DTO
                {
                    Success = true,
                    Message = $"Piano cambiato da '{oldPlan.Name}' a '{newPlan.Name}' in Stripe{(request.Prorate ? " con prorata" : "")}."
                };
            }
        }

        /// <summary>
        /// Recupera audit log per una subscription (operazioni admin)
        /// </summary>
        public async Task<List<BillingAuditLogEntry_DTO>> GetSubscriptionAuditLogAsync(
            Guid subscriptionId,
            CancellationToken cancellationToken = default)
        {
            var subscription = await _billingRepository.GetSubscriptionByIdAsync(subscriptionId, cancellationToken)
                ?? throw new KeyNotFoundException($"Subscription {subscriptionId} not found.");

            var auditEntries = new List<BillingAuditLogEntry_DTO>();

            // Entry per creazione subscription
            auditEntries.Add(new BillingAuditLogEntry_DTO
            {
                Timestamp = subscription.DateIns,
                Action = "Subscription Created",
                Details = $"Subscription created for company {subscription.TenantID}. Plan: {subscription.PlanID}, Cost: €{subscription.Cost}",
                Level = "Information",
                PerformedBy = subscription.CreatedBy?.ToString()
            });

            // Entry per modifiche (se esiste DateEdit)
            if (subscription.DateEdit.HasValue)
            {
                auditEntries.Add(new BillingAuditLogEntry_DTO
                {
                    Timestamp = subscription.DateEdit.Value,
                    Action = "Subscription Updated",
                    Details = $"Subscription modified. Current status: {subscription.RenewalStatus}, Next billing: {subscription.NextBillingDate:dd/MM/yyyy}",
                    Level = "Information",
                    PerformedBy = subscription.EditedBy?.ToString()
                });
            }

            // Entry per info sullo stato corrente
            if (subscription.RenewalStatus == RenewalStatus.CANCELLED || subscription.RenewalStatus == RenewalStatus.EXPIRED)
            {
                auditEntries.Add(new BillingAuditLogEntry_DTO
                {
                    Timestamp = subscription.EndDate,
                    Action = $"Subscription {subscription.RenewalStatus}",
                    Details = $"Status changed to {subscription.RenewalStatus}. End date: {subscription.EndDate:dd/MM/yyyy}",
                    Level = "Warning"
                });
            }

            // Aggiungi info integrazione
            if (!string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID))
            {
                auditEntries.Add(new BillingAuditLogEntry_DTO
                {
                    Timestamp = subscription.DateIns,
                    Action = "Stripe Integration Active",
                    Details = $"Subscription linked to Stripe subscription ID: {subscription.StripeSubscriptionID}",
                    Level = "Information",
                    PerformedBy = "System"
                });
            }
            else
            {
                auditEntries.Add(new BillingAuditLogEntry_DTO
                {
                    Timestamp = subscription.DateIns,
                    Action = "Manual Subscription",
                    Details = "Subscription managed manually without Stripe integration",
                    Level = "Information",
                    PerformedBy = "Admin"
                });
            }

            return auditEntries.OrderByDescending(e => e.Timestamp).ToList();
        }

        #endregion
    }
}
