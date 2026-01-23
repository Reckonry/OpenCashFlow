using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using Stripe;
using CompanyModel = global::Shared.Models.Company;
using CompanySubscriptionModel = global::Shared.Models.Company_Subscription;
using PlanModel = global::Shared.Models.Plan;
using BillingCycle = global::Shared.Models.BillingCycle;
using RenewalStatus = global::Shared.Models.RenewalStatus;

namespace OpenCashFlow.API.Services
{
    public class StripeSyncService(
        ApplicationDbContext dbContext,
        IStripeService stripeService,
        Stripe.IStripeClient stripeClient,
        ILogger<StripeSyncService> logger) : IStripeSyncService
    {
        private readonly ApplicationDbContext _dbContext = dbContext;
        private readonly IStripeService _stripeService = stripeService;
        private readonly Stripe.IStripeClient _stripeClient = stripeClient;
        private readonly ILogger<StripeSyncService> _logger = logger;

        private static readonly HashSet<string> ZeroDecimalCurrencies =
        [
            "bif","clp","djf","gnf","jpy","kmf","krw","mga","pyg","rwf","ugx","vnd","vuv","xaf","xof","xpf"
        ];

        public async Task<CompanyModel?> SyncCustomerFromStripeAsync(string stripeCustomerId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(stripeCustomerId);

            var customer = await _stripeService.GetCustomerAsync(stripeCustomerId, cancellationToken).ConfigureAwait(false);
            if (customer is null)
            {
                _logger.LogWarning("Stripe customer {CustomerId} not found during sync.", stripeCustomerId);
                return null;
            }

            Guid? companyId = null;
            if (customer.Metadata != null &&
                customer.Metadata.TryGetValue("TenantID", out var companyIdStr) &&
                Guid.TryParse(companyIdStr, out var parsedCompanyId))
            {
                companyId = parsedCompanyId;
            }

            CompanyModel? company = null;
            if (companyId.HasValue)
            {
                company = await _dbContext.Company_DS
                    .FirstOrDefaultAsync(c => c.TenantID == companyId.Value, cancellationToken)
                    .ConfigureAwait(false);
            }

            company ??= await _dbContext.Company_DS
                .FirstOrDefaultAsync(c => c.StripeCustomerID == stripeCustomerId, cancellationToken)
                .ConfigureAwait(false);

            if (company is null)
            {
                _logger.LogWarning(
                    "Unable to map Stripe customer {CustomerId} to a local company. Add TenantID metadata or StripeCustomerID reference.",
                    stripeCustomerId);
                return null;
            }

            var updated = false;

            if (!string.Equals(company.StripeCustomerID, stripeCustomerId, StringComparison.Ordinal))
            {
                company.StripeCustomerID = stripeCustomerId;
                updated = true;
            }

            var defaultPaymentMethod = customer.InvoiceSettings?.DefaultPaymentMethodId;
            if (!string.Equals(company.StripeDefaultPaymentMethodID, defaultPaymentMethod, StringComparison.Ordinal))
            {
                company.StripeDefaultPaymentMethodID = defaultPaymentMethod;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(customer.Email) &&
                !string.Equals(company.BillingEmail, customer.Email, StringComparison.OrdinalIgnoreCase))
            {
                company.BillingEmail = customer.Email;
                updated = true;
            }

            var country = customer.Address?.Country;
            if (!string.IsNullOrWhiteSpace(country) &&
                !string.Equals(company.DefaultCountry, country, StringComparison.OrdinalIgnoreCase))
            {
                company.DefaultCountry = country;
                updated = true;
            }

            if (!string.IsNullOrWhiteSpace(customer.Name) &&
                string.IsNullOrWhiteSpace(company.CompanyName))
            {
                company.CompanyName = customer.Name;
                updated = true;
            }

            if (updated)
            {
                company.DateEdit = DateTime.UtcNow;
                await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            }

            return company;
        }

        public async Task<CompanySubscriptionModel?> SyncSubscriptionFromStripeAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(stripeSubscriptionId);

            var subscription = await _stripeService.GetSubscriptionAsync(stripeSubscriptionId, cancellationToken).ConfigureAwait(false);
            if (subscription is null)
            {
                _logger.LogWarning("Stripe subscription {SubscriptionId} not found during sync.", stripeSubscriptionId);
                return null;
            }

            if (string.IsNullOrWhiteSpace(subscription.CustomerId))
            {
                _logger.LogWarning("Stripe subscription {SubscriptionId} has no customer reference.", stripeSubscriptionId);
                return null;
            }

            var company = await SyncCustomerFromStripeAsync(subscription.CustomerId, cancellationToken).ConfigureAwait(false)
                ?? await _dbContext.Company_DS
                    .FirstOrDefaultAsync(c => c.StripeCustomerID == subscription.CustomerId, cancellationToken)
                    .ConfigureAwait(false);

            if (company is null)
            {
                _logger.LogWarning("Cannot sync subscription {SubscriptionId}: company with customer {CustomerId} missing.", stripeSubscriptionId, subscription.CustomerId);
                return null;
            }

            var stripePrice = subscription.Items?.Data?.FirstOrDefault()?.Price;
            PlanModel? plan = null;
            if (!string.IsNullOrWhiteSpace(stripePrice?.Id))
            {
                plan = await _dbContext.Plan_DS
                    .FirstOrDefaultAsync(p => p.StripePriceID == stripePrice.Id, cancellationToken)
                    .ConfigureAwait(false);
            }

            var productId = stripePrice?.ProductId;
            if (plan is null && !string.IsNullOrWhiteSpace(productId))
            {
                plan = await SyncPlanFromStripeAsync(productId, cancellationToken).ConfigureAwait(false);
            }

            if (plan is null)
            {
                _logger.LogWarning(
                    "Cannot sync subscription {SubscriptionId}: plan missing for Stripe price {PriceId}.",
                    stripeSubscriptionId,
                    stripePrice?.Id ?? "<null>");
                return null;
            }

            var existingSubscription = await _dbContext.Company_Subscription_DS
                .FirstOrDefaultAsync(s => s.StripeSubscriptionID == stripeSubscriptionId, cancellationToken)
                .ConfigureAwait(false);

            if (existingSubscription is null &&
                subscription.Metadata != null &&
                subscription.Metadata.TryGetValue("GISubscriptionID", out var localSubscriptionId) &&
                Guid.TryParse(localSubscriptionId, out var parsedLocalId))
            {
                existingSubscription = await _dbContext.Company_Subscription_DS
                    .FirstOrDefaultAsync(s => s.SubscriptionID == parsedLocalId, cancellationToken)
                    .ConfigureAwait(false);
            }

            var now = DateTime.UtcNow;
            var mappedCycle = MapBillingCycle(stripePrice?.Recurring?.Interval) ?? BillingCycle.MONTHLY;
            var startDateRaw = subscription.StartDate;
            var startDate = startDateRaw != default ? NormalizeToUtc(startDateRaw) : now;
            var anchorRaw = subscription.BillingCycleAnchor;
            var anchorDate = anchorRaw != default ? NormalizeToUtc(anchorRaw) : CalculateNextBilling(startDate, mappedCycle);
            var endDate = subscription.CancelAt.HasValue
                ? NormalizeToUtc(subscription.CancelAt.Value)
                : anchorDate;
            var nextBilling = anchorDate;

            var billingCycle = mappedCycle;
            var billingDay = nextBilling.Day;

            var costDecimal = ConvertStripeAmount(stripePrice?.UnitAmountDecimal, stripePrice?.Currency);

            var renewalStatus = MapRenewalStatus(subscription.Status);
            double? discount = null;
            DateTime? discountExpiration = null;
            string? promoCode = null;

            var cancellationDate = subscription.CanceledAt.HasValue
                ? NormalizeToUtc(subscription.CanceledAt.Value)
                : (DateTime?)null;

            var cancellationReason = subscription.CancellationDetails?.Comment ??
                                     subscription.CancellationDetails?.Reason;

            if (existingSubscription is null)
            {
                existingSubscription = new CompanySubscriptionModel
                {
                    SubscriptionID = Guid.NewGuid(),
                    TenantID = company.TenantID,
                    PlanID = plan.PlanID,
                    StartDate = startDate,
                    EndDate = endDate,
                    NextBillingDate = nextBilling,
                    BillingCycle = billingCycle,
                    BillingDay = billingDay,
                    RenewalStatus = renewalStatus,
                    LastReminderDate = startDate,
                    NextReminderDate = nextBilling,
                    Cost = Convert.ToDouble(costDecimal ?? 0m),
                    Discount = discount,
                    PromoCode = promoCode,
                    DiscountExpiration = discountExpiration,
                    CancellationDate = cancellationDate,
                    CancellationReason = cancellationReason,
                    StripeSubscriptionID = stripeSubscriptionId,
                    StripePriceID = stripePrice?.Id,
                    StripeInvoiceID = subscription.LatestInvoiceId,
                    DateIns = now,
                    DateEdit = now
                };

                _dbContext.Company_Subscription_DS.Add(existingSubscription);
            }
            else
            {
                existingSubscription.TenantID = company.TenantID;
                existingSubscription.PlanID = plan.PlanID;
                existingSubscription.StartDate = startDate;
                existingSubscription.EndDate = endDate;
                existingSubscription.NextBillingDate = nextBilling;
                existingSubscription.BillingCycle = billingCycle;
                existingSubscription.BillingDay = billingDay;
                existingSubscription.RenewalStatus = renewalStatus;
                existingSubscription.LastReminderDate = existingSubscription.LastReminderDate == default
                    ? startDate
                    : existingSubscription.LastReminderDate;
                existingSubscription.NextReminderDate = nextBilling;
                if (costDecimal.HasValue)
                {
                    existingSubscription.Cost = Convert.ToDouble(costDecimal.Value);
                }
                existingSubscription.Discount = discount;
                existingSubscription.PromoCode = promoCode;
                existingSubscription.DiscountExpiration = discountExpiration;
                existingSubscription.CancellationDate = cancellationDate;
                existingSubscription.CancellationReason = cancellationReason;
                existingSubscription.StripeSubscriptionID = stripeSubscriptionId;
                existingSubscription.StripePriceID = stripePrice?.Id;
                existingSubscription.StripeInvoiceID = subscription.LatestInvoiceId;
                existingSubscription.DateEdit = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return existingSubscription;
        }

        public async Task<PlanModel?> SyncPlanFromStripeAsync(string stripeProductId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(stripeProductId);

            var productService = new ProductService(_stripeClient);
            Product? product;
            try
            {
                product = await productService.GetAsync(stripeProductId, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to fetch Stripe product {ProductId}", stripeProductId);
                return null;
            }

            if (product is null)
            {
                _logger.LogWarning("Stripe product {ProductId} not found.", stripeProductId);
                return null;
            }

            var priceService = new PriceService(_stripeClient);
            Price? price = null;
            if (!string.IsNullOrWhiteSpace(product.DefaultPriceId))
            {
                try
                {
                    price = await priceService.GetAsync(product.DefaultPriceId, cancellationToken: cancellationToken).ConfigureAwait(false);
                }
                catch (StripeException ex)
                {
                    _logger.LogWarning(ex, "Unable to fetch default price {PriceId} for product {ProductId}", product.DefaultPriceId, stripeProductId);
                }
            }

            if (price is null)
            {
                var priceListOptions = new PriceListOptions
                {
                    Product = product.Id,
                    Active = true,
                    Limit = 1
                };

                var prices = await priceService.ListAsync(priceListOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
                price = prices?.Data?.FirstOrDefault();
            }

            var plan = await _dbContext.Plan_DS
                .FirstOrDefaultAsync(p => p.StripeProductID == product.Id, cancellationToken)
                .ConfigureAwait(false);

            var priceId = price?.Id;
            plan ??= await _dbContext.Plan_DS
                .FirstOrDefaultAsync(p => !string.IsNullOrWhiteSpace(p.StripePriceID) && p.StripePriceID == priceId, cancellationToken)
                .ConfigureAwait(false);

            var now = DateTime.UtcNow;
            var unitAmount = ConvertStripeAmount(price?.UnitAmountDecimal, price?.Currency);
            var billingCycle = MapBillingCycle(price?.Recurring?.Interval) ?? BillingCycle.MONTHLY;
            var durationMonths = MapDurationMonths(price) ?? (billingCycle == BillingCycle.YEARLY ? 12 : 1);

            var metadataPlanCode = product.Metadata?.TryGetValue("PlanCode", out var code) == true ? code : null;
            var metadataVisible = product.Metadata?.TryGetValue("Visible", out var visible) == true && bool.TryParse(visible, out var parsedVisible) ? parsedVisible : (bool?)null;
            var metadataSortOrder = product.Metadata?.TryGetValue("SortOrder", out var sortOrderString) == true && int.TryParse(sortOrderString, out var sortOrderValue) ? sortOrderValue : (int?)null;
            var metadataHasTrial = price?.Metadata?.TryGetValue("HasTrial", out var hasTrialString) == true && bool.TryParse(hasTrialString, out var parsedHasTrial) ? parsedHasTrial : (bool?)null;
            int? trialDays = null;
            if (price?.Metadata?.TryGetValue("TrialDays", out var trialDaysString) == true && int.TryParse(trialDaysString, out var parsedTrialDays))
            {
                trialDays = Math.Clamp(parsedTrialDays, 0, int.MaxValue);
            }
            var hasTrial = metadataHasTrial ?? (trialDays.HasValue && trialDays.Value > 0);

            if (plan is null)
            {
                plan = new PlanModel
                {
                    PlanID = Guid.NewGuid(),
                    PlanCode = metadataPlanCode ?? product.Id,
                    Name = string.IsNullOrWhiteSpace(product.Name) ? product.Id : product.Name,
                    Description = product.Description,
                    Image = product.Images?.FirstOrDefault(),
                    Price = unitAmount ?? 0m,
                    DurationMonths = durationMonths,
                    BillingCycle = billingCycle,
                    HasTrial = hasTrial,
                    TrialDays = trialDays,
                    StripeProductID = product.Id,
                    StripePriceID = price?.Id,
                    IsActive = product.Active,
                    Visible = metadataVisible ?? product.Active,
                    SortOrder = metadataSortOrder ?? 0,
                    CreatedAt = now,
                    UpdatedAt = now
                };

                _dbContext.Plan_DS.Add(plan);
            }
            else
            {
                plan.PlanCode = metadataPlanCode ?? plan.PlanCode;
                plan.Name = string.IsNullOrWhiteSpace(product.Name) ? plan.Name : product.Name;
                plan.Description = product.Description ?? plan.Description;
                plan.Image = product.Images?.FirstOrDefault() ?? plan.Image;
                if (unitAmount.HasValue)
                {
                    plan.Price = unitAmount.Value;
                }
                plan.DurationMonths = durationMonths;
                plan.BillingCycle = billingCycle;
                if (trialDays.HasValue)
                {
                    plan.HasTrial = hasTrial;
                    plan.TrialDays = trialDays;
                }
                else if (metadataHasTrial.HasValue)
                {
                    plan.HasTrial = metadataHasTrial.Value;
                    if (!metadataHasTrial.Value)
                    {
                        plan.TrialDays = null;
                    }
                }
                plan.StripeProductID = product.Id;
                plan.StripePriceID = price?.Id ?? plan.StripePriceID;
                plan.IsActive = product.Active;
                if (metadataVisible.HasValue)
                {
                    plan.Visible = metadataVisible.Value;
                }
                else if (!plan.Visible)
                {
                    plan.Visible = product.Active;
                }
                if (metadataSortOrder.HasValue)
                {
                    plan.SortOrder = metadataSortOrder.Value;
                }
                plan.UpdatedAt = now;
            }

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return plan;
        }

        public async Task<int> SyncAllPlansAsync(CancellationToken cancellationToken = default)
        {
            var productService = new ProductService(_stripeClient);
            var options = new ProductListOptions
            {
                Active = true,
                Limit = 100
            };

            var synced = 0;
            try
            {
                await foreach (var product in productService.ListAutoPagingAsync(options, cancellationToken: cancellationToken))
                {
                    var plan = await SyncPlanFromStripeAsync(product.Id, cancellationToken).ConfigureAwait(false);
                    if (plan != null)
                    {
                        synced++;
                    }
                }
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed syncing plans from Stripe.");
                throw;
            }

            return synced;
        }

        public async Task<CompanyModel?> EnsureCustomerExistsAsync(Guid companyId, CancellationToken cancellationToken = default)
        {
            if (companyId == Guid.Empty)
            {
                throw new ArgumentException("CompanyId must be a valid GUID.", nameof(companyId));
            }

            var company = await _dbContext.Company_DS
                .FirstOrDefaultAsync(c => c.TenantID == companyId, cancellationToken)
                .ConfigureAwait(false);

            if (company is null)
            {
                _logger.LogWarning("Unable to ensure Stripe customer for company {CompanyId}: local company not found.", companyId);
                return null;
            }

            if (!string.IsNullOrWhiteSpace(company.StripeCustomerID))
            {
                await SyncCustomerFromStripeAsync(company.StripeCustomerID, cancellationToken).ConfigureAwait(false);
                return company;
            }

            var stripeCustomer = await _stripeService.CreateCustomerAsync(company, cancellationToken).ConfigureAwait(false);
            company.StripeCustomerID = stripeCustomer.Id;
            company.BillingEmail ??= stripeCustomer.Email;
            company.StripeDefaultPaymentMethodID = stripeCustomer.InvoiceSettings?.DefaultPaymentMethodId;
            company.DateEdit = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
            return company;
        }

        private static decimal? ConvertStripeAmount(decimal? amountMinorUnits, string? currency)
        {
            if (!amountMinorUnits.HasValue)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(currency))
            {
                return amountMinorUnits.Value / 100m;
            }

            var normalized = currency.ToLowerInvariant();
            if (ZeroDecimalCurrencies.Contains(normalized))
            {
                return amountMinorUnits.Value;
            }

            return amountMinorUnits.Value / 100m;
        }

        private static BillingCycle? MapBillingCycle(string? interval)
        {
            return interval switch
            {
                "month" => BillingCycle.MONTHLY,
                "year" => BillingCycle.YEARLY,
                null => null,
                _ => null
            };
        }

        private static int? MapDurationMonths(Price? price)
        {
            if (price?.Recurring is null)
            {
                return null;
            }

            var intervalCount = price.Recurring.IntervalCount;
            if (intervalCount <= 0)
            {
                intervalCount = 1;
            }
            var normalizedCount = (int)Math.Clamp(intervalCount, 1, int.MaxValue);
            return price.Recurring.Interval switch
            {
                "month" => normalizedCount,
                "year" => normalizedCount * 12,
                _ => null
            };
        }

        private static DateTime CalculateNextBilling(DateTime startDate, BillingCycle billingCycle)
        {
            return billingCycle switch
            {
                BillingCycle.YEARLY => startDate.AddYears(1),
                _ => startDate.AddMonths(1)
            };
        }

        private static RenewalStatus MapRenewalStatus(string? stripeStatus)
        {
            return stripeStatus?.ToLowerInvariant() switch
            {
                "active" => RenewalStatus.ACTIVE,
                "trialing" => RenewalStatus.TRIAL,
                "canceled" or "cancelled" => RenewalStatus.CANCELLED,
                "paused" => RenewalStatus.PAUSED,
                "past_due" => RenewalStatus.PAUSED,
                "unpaid" => RenewalStatus.SUSPENDED,
                "incomplete" or "incomplete_expired" => RenewalStatus.SUSPENDED,
                _ => RenewalStatus.SUSPENDED
            };
        }

        private static DateTime NormalizeToUtc(DateTime value)
        {
            return value.Kind switch
            {
                DateTimeKind.Utc => value,
                DateTimeKind.Local => value.ToUniversalTime(),
                _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
            };
        }
    }
}
