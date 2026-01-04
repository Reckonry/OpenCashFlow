using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using global::Shared.Data;
using global::Shared.DTOs.Billing;
using global::Shared.Models;
using global::Shared.Models.Companies;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenCashFlow.API.Services
{
    public partial class BillingService(
        IBillingRepository billingRepository,
        IAuthenticationService authenticationService,
        IStripeService stripeService,
        IMapper mapper,
        ILogger<BillingService> logger,
        ApplicationDbContext dbContext) : IBillingService
    {
        private readonly IBillingRepository _billingRepository = billingRepository;
        private readonly IAuthenticationService _authenticationService = authenticationService;
        private readonly IStripeService _stripeService = stripeService;
        private readonly IMapper _mapper = mapper;
        private readonly ILogger<BillingService> _logger = logger;
        private readonly ApplicationDbContext _dbContext = dbContext;

        private static readonly HashSet<string> ZeroDecimalCurrencies =
        [
            "bif","clp","djf","gnf","jpy","kmf","krw","mga","pyg","rwf","ugx","vnd","vuv","xaf","xof","xpf"
        ];

        public async Task<IEnumerable<BillingPlan_List_DTO>> GetPlansAsync(BillingPlan_Filter_DTO filters, CancellationToken cancellationToken)
        {
            var plans = await _billingRepository.GetPlansAsync(filters, cancellationToken);
            return _mapper.Map<IEnumerable<BillingPlan_List_DTO>>(plans);
        }

        public async Task<IEnumerable<BillingSubscription_List_DTO>> GetSubscriptionsAsync(BillingSubscription_Filter_DTO filters, CancellationToken cancellationToken)
        {
            Guid? companyId = null;
            if (!filters.IncludeAllCompanies)
            {
                companyId = _authenticationService.GetTenantID();
            }
            else if (filters.TenantID.HasValue)
            {
                companyId = filters.TenantID.Value;
            }

            var subscriptions = await _billingRepository.GetSubscriptionsAsync(companyId, filters, cancellationToken);
            return _mapper.Map<IEnumerable<BillingSubscription_List_DTO>>(subscriptions);
        }

        public async Task<BillingDashboardKPI_DTO> GetDashboardKPIAsync(CancellationToken cancellationToken)
        {
            return await _billingRepository.GetDashboardKPIAsync(cancellationToken);
        }

        public async Task<BillingCheckoutSession_DTO> CreateCheckoutSessionAsync(BillingCheckoutRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var company = await RequireCompanyAsync(cancellationToken);
            EnsureStripeCustomer(company);

            var plan = await RequirePlanAsync(request.PlanId, cancellationToken);
            EnsureStripePlan(plan);

            var session = await _stripeService.CreateCheckoutSessionAsync(company, plan, request.SuccessUrl, request.CancelUrl, cancellationToken);

            if (string.IsNullOrWhiteSpace(session.Url))
            {
                throw new InvalidOperationException("Stripe non ha restituito una URL per la sessione di checkout.");
            }

            _logger.LogInformation("Checkout session {SessionId} created for company {CompanyId} and plan {PlanId}.", session.Id, company.TenantID, plan.PlanID);

            return new BillingCheckoutSession_DTO
            {
                SessionId = session.Id,
                Url = session.Url
            };
        }

        public async Task<BillingPortalSession_DTO> CreateCustomerPortalSessionAsync(BillingPortalSessionRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var company = await RequireCompanyAsync(cancellationToken);
            EnsureStripeCustomer(company);

            var portalSession = await _stripeService.CreateCustomerPortalSessionAsync(company.StripeCustomerID!, request.ReturnUrl, cancellationToken);

            _logger.LogInformation("Portal session {SessionId} created for company {CompanyId}.", portalSession.Id, company.TenantID);

            return new BillingPortalSession_DTO
            {
                SessionId = portalSession.Id,
                Url = portalSession.Url
            };
        }

        public async Task<BillingSubscriptionActionResult_DTO> CreateSubscriptionAsync(BillingSubscribeRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var company = await RequireCompanyAsync(cancellationToken);
            EnsureStripeCustomer(company);

            var plan = await RequirePlanAsync(request.PlanId, cancellationToken);
            EnsureStripePlan(plan);

            var options = new SubscriptionCreateOptions
            {
                ProrationBehavior = request.Prorate ? "create_prorations" : "none"
            };

            var subscription = await _stripeService.CreateSubscriptionAsync(company.StripeCustomerID!, plan.StripePriceID!, options, cancellationToken);

            _logger.LogInformation("Stripe subscription {StripeSubscriptionId} created for company {CompanyId} with plan {PlanId}.", subscription.Id, company.TenantID, plan.PlanID);

            return MapSubscription(subscription);
        }

        public async Task<BillingSubscriptionActionResult_DTO> CancelSubscriptionAsync(Guid subscriptionId, bool immediately, CancellationToken cancellationToken)
        {
            var subscription = await RequireSubscriptionAsync(subscriptionId, cancellationToken);
            EnsureStripeSubscription(subscription);

            await _stripeService.CancelSubscriptionAsync(subscription.StripeSubscriptionID!, immediately, cancellationToken);

            var refreshed = await _stripeService.GetSubscriptionAsync(subscription.StripeSubscriptionID!, cancellationToken);
            _logger.LogInformation("Stripe subscription {StripeSubscriptionId} cancelled (immediately={Immediately}).", subscription.StripeSubscriptionID, immediately);
            return MapSubscription(refreshed ?? throw new InvalidOperationException("Subscription non trovata su Stripe dopo la cancellazione."));
        }

        public async Task<BillingSubscriptionActionResult_DTO> ReactivateSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscription = await RequireSubscriptionAsync(subscriptionId, cancellationToken);
            EnsureStripeSubscription(subscription);

            var refreshed = await _stripeService.ReactivateSubscriptionAsync(subscription.StripeSubscriptionID!, cancellationToken);
            _logger.LogInformation("Stripe subscription {StripeSubscriptionId} reactivated.", subscription.StripeSubscriptionID);
            return MapSubscription(refreshed);
        }

        public async Task<BillingSubscriptionActionResult_DTO> ChangeSubscriptionPlanAsync(Guid subscriptionId, BillingChangePlanRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var subscription = await RequireSubscriptionAsync(subscriptionId, cancellationToken);
            EnsureStripeSubscription(subscription);

            var newPlan = await RequirePlanAsync(request.NewPlanId, cancellationToken);
            EnsureStripePlan(newPlan);

            var updated = await _stripeService.ChangeSubscriptionPlanAsync(subscription.StripeSubscriptionID!, newPlan.StripePriceID!, cancellationToken);
            _logger.LogInformation("Stripe subscription {StripeSubscriptionId} changed to price {StripePriceId}.", subscription.StripeSubscriptionID, newPlan.StripePriceID);
            return MapSubscription(updated);
        }

        public async Task<BillingInvoice_DTO> GetLatestInvoiceAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscription = await RequireSubscriptionAsync(subscriptionId, cancellationToken);
            EnsureStripeSubscription(subscription);

            var stripeSubscription = await _stripeService.GetSubscriptionAsync(subscription.StripeSubscriptionID!, cancellationToken)
                ?? throw new InvalidOperationException("Subscription non trovata su Stripe.");

            var invoiceId = stripeSubscription.LatestInvoiceId ?? subscription.StripeInvoiceID;
            if (string.IsNullOrWhiteSpace(invoiceId))
            {
                throw new InvalidOperationException("Nessuna fattura disponibile per questa subscription.");
            }

            var invoice = await _stripeService.GetInvoiceAsync(invoiceId, cancellationToken)
                ?? throw new InvalidOperationException("Fattura non trovata su Stripe.");

            return MapInvoice(invoice);
        }

        public async Task<BillingInvoice_DTO> GetUpcomingInvoiceAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscription = await RequireSubscriptionAsync(subscriptionId, cancellationToken);
            EnsureStripeSubscription(subscription);

            var company = subscription.Company ?? await RequireCompanyAsync(cancellationToken);
            EnsureStripeCustomer(company);

            var plan = subscription.Plan ?? await RequirePlanAsync(subscription.PlanID, cancellationToken);
            EnsureStripePlan(plan);

            var invoice = await _stripeService.GetUpcomingInvoiceAsync(
                company.StripeCustomerID!,
                subscription.StripeSubscriptionID!,
                plan.StripePriceID,
                cancellationToken);

            if (invoice == null)
            {
                throw new InvalidOperationException("Stripe non ha restituito una preview della prossima fattura.");
            }

            return MapInvoice(invoice);
        }

        public async Task AddPaymentMethodAsync(BillingAddPaymentMethodRequest_DTO request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var company = await RequireCompanyAsync(cancellationToken);
            EnsureStripeCustomer(company);

            await _stripeService.AddPaymentMethodAsync(company.StripeCustomerID!, request.PaymentMethodId, cancellationToken);

            if (request.MakeDefault)
            {
                await _stripeService.SetDefaultPaymentMethodAsync(company.StripeCustomerID!, request.PaymentMethodId, cancellationToken);
                company.StripeDefaultPaymentMethodID = request.PaymentMethodId;
                await _billingRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Payment method {PaymentMethodId} added for company {CompanyId} (makeDefault={MakeDefault}).", request.PaymentMethodId, company.TenantID, request.MakeDefault);
        }

        public async Task RemovePaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(paymentMethodId);

            await _stripeService.RemovePaymentMethodAsync(paymentMethodId, cancellationToken);

            var company = await RequireCompanyAsync(cancellationToken);
            if (company.StripeDefaultPaymentMethodID == paymentMethodId)
            {
                company.StripeDefaultPaymentMethodID = null;
                await _billingRepository.SaveChangesAsync(cancellationToken);
            }

            _logger.LogInformation("Payment method {PaymentMethodId} removed for company {CompanyId}.", paymentMethodId, company.TenantID);
        }

        private async Task<Company> RequireCompanyAsync(CancellationToken cancellationToken)
        {
            var companyId = _authenticationService.GetTenantID();
            var company = await _billingRepository.GetCompanyByIdAsync(companyId, cancellationToken);
            if (company == null)
            {
                throw new KeyNotFoundException("Company non trovata per l'utente corrente.");
            }

            return company;
        }

        private async Task<global::Shared.Models.Plan> RequirePlanAsync(Guid planId, CancellationToken cancellationToken)
        {
            var plan = await _billingRepository.GetPlanByIdAsync(planId, cancellationToken);
            if (plan == null)
            {
                throw new KeyNotFoundException("Piano non trovato.");
            }

            return plan;
        }

        private async Task<Company_Subscription> RequireSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            var subscription = await _billingRepository.GetSubscriptionByIdAsync(subscriptionId, cancellationToken);
            if (subscription == null)
            {
                throw new KeyNotFoundException("Subscription non trovata.");
            }

            return subscription;
        }

        private static void EnsureStripeCustomer(Company company)
        {
            if (string.IsNullOrWhiteSpace(company.StripeCustomerID))
            {
                throw new InvalidOperationException("L'azienda non è collegata a un customer Stripe.");
            }
        }

        private static void EnsureStripePlan(global::Shared.Models.Plan plan)
        {
            if (string.IsNullOrWhiteSpace(plan.StripePriceID))
            {
                throw new InvalidOperationException("Il piano selezionato non è collegato a un price Stripe.");
            }
        }

        private static void EnsureStripeSubscription(Company_Subscription subscription)
        {
            if (string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID))
            {
                throw new InvalidOperationException("Non è presente un riferimento alla subscription Stripe.");
            }
        }

        private static BillingSubscriptionActionResult_DTO MapSubscription(Subscription subscription)
        {
            return new BillingSubscriptionActionResult_DTO
            {
                StripeSubscriptionId = subscription.Id,
                StripeCustomerId = subscription.CustomerId,
                StripePriceId = subscription.Items?.Data?.FirstOrDefault()?.Price?.Id,
                Status = subscription.Status ?? string.Empty,
                CurrentPeriodStartUtc = subscription.BillingCycleAnchor,
                CurrentPeriodEndUtc = subscription.CancelAt ?? subscription.EndedAt ?? subscription.BillingCycleAnchor,
                LatestInvoiceId = subscription.LatestInvoiceId
            };
        }

        private static BillingInvoice_DTO MapInvoice(Invoice invoice)
        {
            var currency = (invoice.Currency ?? string.Empty).ToLowerInvariant();
            return new BillingInvoice_DTO
            {
                InvoiceId = invoice.Id,
                Status = invoice.Status ?? string.Empty,
                AmountDue = ConvertAmount(invoice.AmountDue, currency),
                AmountPaid = ConvertAmount(invoice.AmountPaid, currency),
                AmountRemaining = ConvertAmount(invoice.AmountRemaining, currency),
                Currency = currency,
                CreatedUtc = invoice.Created,
                DueDateUtc = invoice.DueDate,
                HostedInvoiceUrl = invoice.HostedInvoiceUrl,
                InvoicePdf = invoice.InvoicePdf
            };
        }

        private static decimal ConvertAmount(long amount, string currency)
        {
            if (ZeroDecimalCurrencies.Contains(currency))
            {
                return amount;
            }

            return amount / 100m;
        }

        public async Task<BillingPortal_DTO> GetCustomerBillingPortalAsync(CancellationToken cancellationToken)
        {
            var companyId = _authenticationService.GetTenantID();
            if (companyId == Guid.Empty)
            {
                throw new UnauthorizedAccessException("Company ID not found for current user.");
            }

            // Get active subscription for the company
            var subscriptions = await _billingRepository.GetSubscriptionsAsync(companyId, new BillingSubscription_Filter_DTO
            {
                RenewalStatuses = [RenewalStatus.ACTIVE, RenewalStatus.TRIAL]
            }, cancellationToken);

            var subscription = subscriptions.FirstOrDefault();

            var portal = new BillingPortal_DTO
            {
                AvailablePlans = (await GetPlansAsync(new BillingPlan_Filter_DTO { OnlyActive = true }, cancellationToken)).ToList()
            };

            if (subscription == null)
            {
                // No active subscription
                return portal;
            }

            // Map subscription data
            portal.SubscriptionID = subscription.SubscriptionID;
            portal.PlanName = subscription.Plan?.Name;
            portal.PlanDescription = subscription.Plan?.Description;
            portal.Cost = subscription.Cost;
            portal.BillingCycle = subscription.BillingCycle;
            portal.NextBillingDate = subscription.NextBillingDate;
            portal.EndDate = subscription.EndDate;
            portal.RenewalStatus = subscription.RenewalStatus;
            portal.Discount = subscription.Discount;
            portal.PromoCode = subscription.PromoCode;

            // Stripe integration
            portal.HasStripeIntegration = !string.IsNullOrWhiteSpace(subscription.StripeSubscriptionID);
            portal.StripeSubscriptionID = subscription.StripeSubscriptionID;
            portal.StripeCustomerID = subscription.Company?.StripeCustomerID;

            if (portal.HasStripeIntegration && !string.IsNullOrWhiteSpace(portal.StripeCustomerID))
            {
                try
                {
                    // Get invoices
                    var invoices = await _stripeService.ListInvoicesAsync(portal.StripeCustomerID, 10);
                    portal.RecentInvoices = invoices.Select(inv => new BillingInvoiceSummary_DTO
                    {
                        InvoiceId = inv.Id,
                        InvoiceNumber = inv.Number ?? inv.Id,
                        Date = inv.Created,
                        DueDate = inv.DueDate,
                        Amount = ConvertAmount(inv.AmountDue, inv.Currency),
                        Currency = inv.Currency.ToUpperInvariant(),
                        Status = inv.Status,
                        PdfUrl = inv.InvoicePdf,
                        IsPaid = inv.Status == "paid"
                    }).ToList();

                    // Check payment failures
                    var failedInvoices = portal.RecentInvoices.Where(i => i.Status == "open" || i.Status == "uncollectible").ToList();
                    if (failedInvoices.Any())
                    {
                        portal.HasPaymentFailed = true;
                        portal.LastPaymentFailedDate = failedInvoices.Max(i => i.Date);
                        portal.PaymentFailureReason = "Il pagamento più recente non è andato a buon fine. Aggiorna il tuo metodo di pagamento.";
                    }

                    // Get upcoming invoice
                    try
                    {
                        var upcomingInvoice = await _stripeService.GetUpcomingInvoiceAsync(portal.StripeCustomerID, portal.StripeSubscriptionID, null, cancellationToken);
                        if (upcomingInvoice != null)
                        {
                            portal.UpcomingInvoice = new BillingInvoice_DTO
                            {
                                InvoiceId = "upcoming",
                                AmountDue = ConvertAmount(upcomingInvoice.AmountDue, upcomingInvoice.Currency),
                                Currency = upcomingInvoice.Currency.ToUpperInvariant(),
                                CreatedUtc = upcomingInvoice.PeriodStart,
                                DueDateUtc = upcomingInvoice.PeriodEnd
                            };
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Could not fetch upcoming invoice for customer {CustomerId}", portal.StripeCustomerID);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error fetching Stripe data for customer portal");
                }
            }

            return portal;
        }
    }
}
