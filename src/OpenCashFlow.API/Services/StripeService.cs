using OpenCashFlow.API.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly.Retry;
using System.Collections.Generic;
using global::Shared.Options;
using global::Shared.Services.Stripe;
using Stripe;
using CheckoutSession = Stripe.Checkout.Session;
using CheckoutSessionService = Stripe.Checkout.SessionService;
using CheckoutSessionCreateOptions = Stripe.Checkout.SessionCreateOptions;
using CheckoutSessionLineItemOptions = Stripe.Checkout.SessionLineItemOptions;
using PortalSession = Stripe.BillingPortal.Session;
using PortalSessionService = Stripe.BillingPortal.SessionService;
using CompanyModel = global::Shared.Models.Company;
using PlanModel = global::Shared.Models.Plan;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OpenCashFlow.API.Services
{
    public class StripeService : IStripeService
    {
        private static readonly HashSet<string> ZeroDecimalCurrencies =
        [
            "bif","clp","djf","gnf","jpy","kmf","krw","mga","pyg","rwf","ugx","vnd","vuv","xaf","xof","xpf"
        ];

        private readonly Stripe.IStripeClient _client;
        private readonly StripeSettings _settings;
        private readonly ILogger<StripeService> _logger;
        private readonly AsyncRetryPolicy _retryPolicy;

        public StripeService(
            Stripe.IStripeClient stripeClient,
            IOptions<StripeSettings> settings,
            ILogger<StripeService> logger)
        {
            _client = stripeClient;
            _settings = settings.Value;
            _logger = logger;
            _retryPolicy = StripeRetryPolicies.CreateAsyncRetryPolicy((attempt, delay, exception) =>
            {
                _logger.LogWarning(exception, "Retrying Stripe call after {Delay}. Attempt {Attempt}", delay, attempt);
            });
        }

        #region Customer Management
        public Task<Customer> CreateCustomerAsync(CompanyModel company, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(company);

            var service = new CustomerService(_client);
            var options = new CustomerCreateOptions
            {
                Name = company.CompanyName,
                Email = company.BillingEmail,
                Metadata = new Dictionary<string, string>
                {
                    ["TenantID"] = company.TenantID.ToString()
                }
            };

            if (!string.IsNullOrWhiteSpace(company.DefaultCountry))
            {
                options.Address = new AddressOptions { Country = company.DefaultCountry };
            }

            var requestOptions = BuildRequestOptions("customer_create", company.TenantID.ToString());

            return ExecuteAsync(ct => service.CreateAsync(options, requestOptions, ct), "CreateCustomer", cancellationToken);
        }

        public Task<Customer> UpdateCustomerAsync(string customerId, CompanyModel company, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentNullException.ThrowIfNull(company);

            var service = new CustomerService(_client);
            var options = new CustomerUpdateOptions
            {
                Name = company.CompanyName,
                Email = company.BillingEmail,
                Metadata = new Dictionary<string, string>
                {
                    ["TenantID"] = company.TenantID.ToString()
                }
            };

            var requestOptions = BuildRequestOptions("customer_update", customerId);

            return ExecuteAsync(ct => service.UpdateAsync(customerId, options, requestOptions, ct), "UpdateCustomer", cancellationToken);
        }

        public Task<Customer?> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);

            var service = new CustomerService(_client);
            return ExecuteAsync(ct => service.GetAsync(customerId, requestOptions: null, cancellationToken: ct), "GetCustomer", cancellationToken);
        }

        public Task DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);

            var service = new CustomerService(_client);
            return ExecuteAsync(async ct =>
            {
                await service.DeleteAsync(customerId, requestOptions: null, cancellationToken: ct).ConfigureAwait(false);
            }, "DeleteCustomer", cancellationToken);
        }

        public Task AddPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(paymentMethodId);

            var service = new PaymentMethodService(_client);
            var options = new PaymentMethodAttachOptions { Customer = customerId };
            var requestOptions = BuildRequestOptions("paymentmethod_attach", customerId, paymentMethodId);

            return ExecuteAsync(async ct =>
            {
                await service.AttachAsync(paymentMethodId, options, requestOptions, ct).ConfigureAwait(false);
            }, "AddPaymentMethod", cancellationToken);
        }

        public Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(paymentMethodId);

            var service = new CustomerService(_client);
            var options = new CustomerUpdateOptions
            {
                InvoiceSettings = new CustomerInvoiceSettingsOptions
                {
                    DefaultPaymentMethod = paymentMethodId
                }
            };
            var requestOptions = BuildRequestOptions("customer_set_default_pm", customerId, paymentMethodId);

            return ExecuteAsync(async ct =>
            {
                await service.UpdateAsync(customerId, options, requestOptions, ct).ConfigureAwait(false);
            }, "SetDefaultPaymentMethod", cancellationToken);
        }
        #endregion

        #region Subscription Management
        public Task<Subscription> CreateSubscriptionAsync(string customerId, string priceId, SubscriptionCreateOptions? options = null, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(priceId);

            var service = new SubscriptionService(_client);
            options ??= new SubscriptionCreateOptions();
            options.Customer = customerId;
            options.Items ??= new List<SubscriptionItemOptions>();
            if (!options.Items.Any())
            {
                options.Items.Add(new SubscriptionItemOptions { Price = priceId });
            }

            var requestOptions = BuildRequestOptions("subscription_create", customerId, priceId);

            return ExecuteAsync(ct => service.CreateAsync(options, requestOptions, ct), "CreateSubscription", cancellationToken);
        }

        public Task<Subscription> UpdateSubscriptionAsync(string subscriptionId, SubscriptionUpdateOptions options, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
            ArgumentNullException.ThrowIfNull(options);

            var service = new SubscriptionService(_client);
            var requestOptions = BuildRequestOptions("subscription_update", subscriptionId);

            return ExecuteAsync(ct => service.UpdateAsync(subscriptionId, options, requestOptions, ct), "UpdateSubscription", cancellationToken);
        }

        public Task CancelSubscriptionAsync(string subscriptionId, bool immediately, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);

            var service = new SubscriptionService(_client);

            return ExecuteAsync(async ct =>
            {
                if (immediately)
                {
                    await service.CancelAsync(subscriptionId, new SubscriptionCancelOptions(), requestOptions: null, cancellationToken: ct).ConfigureAwait(false);
                }
                else
                {
                    var updateOptions = new SubscriptionUpdateOptions { CancelAtPeriodEnd = true };
                    await service.UpdateAsync(subscriptionId, updateOptions, requestOptions: null, cancellationToken: ct).ConfigureAwait(false);
                }
            }, "CancelSubscription", cancellationToken);
        }

        public Task<Subscription> ReactivateSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
            var service = new SubscriptionService(_client);
            var options = new SubscriptionUpdateOptions { CancelAtPeriodEnd = false };
            var requestOptions = BuildRequestOptions("subscription_reactivate", subscriptionId);

            return ExecuteAsync(ct => service.UpdateAsync(subscriptionId, options, requestOptions, ct), "ReactivateSubscription", cancellationToken);
        }

        public async Task<Subscription> ChangeSubscriptionPlanAsync(string subscriptionId, string newPriceId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
            ArgumentException.ThrowIfNullOrWhiteSpace(newPriceId);

            var service = new SubscriptionService(_client);
            var subscription = await GetSubscriptionAsync(subscriptionId, cancellationToken).ConfigureAwait(false)
                ?? throw new InvalidOperationException($"Subscription {subscriptionId} not found in Stripe.");

            if (subscription.Items?.Data == null || subscription.Items.Data.Count == 0)
            {
                throw new InvalidOperationException($"Subscription {subscriptionId} has no items to update.");
            }

            var primaryItemId = subscription.Items.Data[0].Id;

            var updateOptions = new SubscriptionUpdateOptions
            {
                Items = subscription.Items.Data.Select(item => new SubscriptionItemOptions
                {
                    Id = item.Id,
                    Price = item.Id == primaryItemId ? newPriceId : item.Price?.Id
                }).ToList()
            };

            var requestOptions = BuildRequestOptions("subscription_change_plan", subscriptionId, newPriceId);

            return await ExecuteAsync(ct => service.UpdateAsync(subscriptionId, updateOptions, requestOptions, ct), "ChangeSubscriptionPlan", cancellationToken).ConfigureAwait(false);
        }

        public Task<Subscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(subscriptionId);
            var service = new SubscriptionService(_client);

            return ExecuteAsync(ct => service.GetAsync(subscriptionId, requestOptions: null, cancellationToken: ct), "GetSubscription", cancellationToken);
        }
        #endregion

        #region Payment & Checkout
        public Task<CheckoutSession> CreateCheckoutSessionAsync(CompanyModel company, PlanModel plan, string successUrl, string cancelUrl, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(company);
            ArgumentNullException.ThrowIfNull(plan);
            ArgumentException.ThrowIfNullOrWhiteSpace(successUrl);
            ArgumentException.ThrowIfNullOrWhiteSpace(cancelUrl);

            if (string.IsNullOrWhiteSpace(plan.StripePriceID))
            {
                throw new InvalidOperationException("Plan missing Stripe price ID.");
            }

            var service = new CheckoutSessionService(_client);
            var options = new CheckoutSessionCreateOptions
            {
                Customer = company.StripeCustomerID,
                Mode = "subscription",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
                LineItems =
                [
                    new CheckoutSessionLineItemOptions
                    {
                        Price = plan.StripePriceID,
                        Quantity = 1
                    }
                ],
                Metadata = new Dictionary<string, string>
                {
                    ["TenantID"] = company.TenantID.ToString(),
                    ["PlanID"] = plan.PlanID.ToString()
                }
            };

            var requestOptions = BuildRequestOptions("checkout_session", company.TenantID.ToString(), plan.PlanID.ToString());

            return ExecuteAsync(ct => service.CreateAsync(options, requestOptions, ct), "CreateCheckoutSession", cancellationToken);
        }

        public Task<PortalSession> CreateCustomerPortalSessionAsync(string customerId, string returnUrl, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            ArgumentException.ThrowIfNullOrWhiteSpace(returnUrl);

            var service = new PortalSessionService(_client);
            var options = new Stripe.BillingPortal.SessionCreateOptions
            {
                Customer = customerId,
                ReturnUrl = returnUrl
            };

            var requestOptions = BuildRequestOptions("customer_portal_session", customerId);

            return ExecuteAsync(ct => service.CreateAsync(options, requestOptions, ct), "CreateCustomerPortalSession", cancellationToken);
        }

        public Task<PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(currency);

            var service = new PaymentIntentService(_client);
            var normalizedCurrency = currency.ToLowerInvariant();
            var intentOptions = new PaymentIntentCreateOptions
            {
                Amount = ConvertToStripeAmount(amount, normalizedCurrency),
                Currency = normalizedCurrency,
                Customer = string.IsNullOrWhiteSpace(customerId) ? null : customerId,
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true
                }
            };

            var requestOptions = BuildRequestOptions("payment_intent", customerId ?? "anonymous", normalizedCurrency, amount.ToString("0.##"));

            return ExecuteAsync(ct => service.CreateAsync(intentOptions, requestOptions, ct), "CreatePaymentIntent", cancellationToken);
        }
        #endregion

        #region Invoice & Billing
        public Task<Invoice?> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);

            var service = new InvoiceService(_client);
            return ExecuteAsync(ct => service.GetAsync(invoiceId, requestOptions: null, cancellationToken: ct), "GetInvoice", cancellationToken);
        }

        public Task<Invoice?> GetUpcomingInvoiceAsync(string customerId, string? subscriptionId, string? priceId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);

            var service = new InvoiceService(_client);
            var options = new InvoiceCreatePreviewOptions
            {
                Customer = customerId,
                Subscription = subscriptionId
            };

            if (!string.IsNullOrWhiteSpace(priceId))
            {
                options.SubscriptionDetails = new InvoiceSubscriptionDetailsOptions
                {
                    Items =
                    [
                        new InvoiceSubscriptionDetailsItemOptions
                        {
                            Price = priceId,
                            Quantity = 1
                        }
                    ]
                };
            }

            return ExecuteAsync(ct => service.CreatePreviewAsync(options, requestOptions: null, cancellationToken: ct), "GetUpcomingInvoice", cancellationToken);
        }

        public Task RemovePaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(paymentMethodId);

            var service = new PaymentMethodService(_client);
            return ExecuteAsync(async ct =>
            {
                await service.DetachAsync(paymentMethodId, options: null, requestOptions: null, ct).ConfigureAwait(false);
            }, "RemovePaymentMethod", cancellationToken);
        }

        public async Task<IEnumerable<Invoice>> ListInvoicesAsync(string customerId, int limit, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(customerId);
            if (limit <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(limit), "Limit must be greater than zero.");
            }

            var service = new InvoiceService(_client);
            var options = new InvoiceListOptions
            {
                Customer = customerId,
                Limit = limit
            };

            var invoices = await ExecuteAsync(ct => service.ListAsync(options, requestOptions: null, cancellationToken: ct), "ListInvoices", cancellationToken).ConfigureAwait(false);
            return invoices?.Data ?? Enumerable.Empty<Invoice>();
        }

        public Task SendInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(invoiceId);

            var service = new InvoiceService(_client);
            var requestOptions = BuildRequestOptions("invoice_send", invoiceId);

            return ExecuteAsync(async ct =>
            {
                await service.SendInvoiceAsync(invoiceId, options: null, requestOptions, ct).ConfigureAwait(false);
            }, "SendInvoice", cancellationToken);
        }
        #endregion

        #region Helpers
        private Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, string operation, CancellationToken cancellationToken)
        {
            return _retryPolicy.ExecuteAsync(async ct =>
            {
                try
                {
                    _logger.LogInformation("Stripe {Operation} - starting", operation);
                    var result = await action(ct).ConfigureAwait(false);
                    _logger.LogInformation("Stripe {Operation} - completed", operation);
                    return result;
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Stripe {Operation} failed: {Message}", operation, ex.Message);
                    throw;
                }
            }, cancellationToken);
        }

        private Task ExecuteAsync(Func<CancellationToken, Task> action, string operation, CancellationToken cancellationToken)
        {
            return _retryPolicy.ExecuteAsync(async ct =>
            {
                try
                {
                    _logger.LogInformation("Stripe {Operation} - starting", operation);
                    await action(ct).ConfigureAwait(false);
                    _logger.LogInformation("Stripe {Operation} - completed", operation);
                }
                catch (StripeException ex)
                {
                    _logger.LogError(ex, "Stripe {Operation} failed: {Message}", operation, ex.Message);
                    throw;
                }
            }, cancellationToken);
        }

        private RequestOptions BuildRequestOptions(string prefix, params string[] components)
        {
            var key = BuildIdempotencyKey(prefix, components);
            return new RequestOptions
            {
                IdempotencyKey = key
            };
        }

        private static string BuildIdempotencyKey(string prefix, params string[] components)
        {
            var normalized = string.Join("|", components.Where(c => !string.IsNullOrWhiteSpace(c)));
            if (string.IsNullOrWhiteSpace(normalized))
            {
                normalized = Guid.NewGuid().ToString("N");
            }

            var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
            var hash = Convert.ToHexString(hashBytes).ToLowerInvariant();
            var key = $"{prefix}_{hash}";
            return key.Length > 255 ? key[..255] : key;
        }

        private static long ConvertToStripeAmount(decimal amount, string currency)
        {
            if (ZeroDecimalCurrencies.Contains(currency))
            {
                return (long)Math.Round(amount, MidpointRounding.AwayFromZero);
            }

            return (long)Math.Round(amount * 100m, MidpointRounding.AwayFromZero);
        }
        #endregion
    }
}
