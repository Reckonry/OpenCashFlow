namespace OpenCashFlow.API.Services.Interfaces
{
    /// <summary>
    /// Facade per incapsulare tutte le interazioni con Stripe e mantenere la business logic isolata.
    /// </summary>
    public interface IStripeService
    {
        #region Customer Management
        Task<Stripe.Customer> CreateCustomerAsync(global::Shared.Models.Company company, CancellationToken cancellationToken = default);
        Task<Stripe.Customer> UpdateCustomerAsync(string customerId, global::Shared.Models.Company company, CancellationToken cancellationToken = default);
        Task<Stripe.Customer?> GetCustomerAsync(string customerId, CancellationToken cancellationToken = default);
        Task DeleteCustomerAsync(string customerId, CancellationToken cancellationToken = default);
        Task AddPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken = default);
        Task SetDefaultPaymentMethodAsync(string customerId, string paymentMethodId, CancellationToken cancellationToken = default);
        Task RemovePaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken = default);
        #endregion

        #region Subscription Management
        Task<Stripe.Subscription> CreateSubscriptionAsync(string customerId, string priceId, Stripe.SubscriptionCreateOptions? options = null, CancellationToken cancellationToken = default);
        Task<Stripe.Subscription> UpdateSubscriptionAsync(string subscriptionId, Stripe.SubscriptionUpdateOptions options, CancellationToken cancellationToken = default);
        Task CancelSubscriptionAsync(string subscriptionId, bool immediately, CancellationToken cancellationToken = default);
        Task<Stripe.Subscription> ReactivateSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
        Task<Stripe.Subscription> ChangeSubscriptionPlanAsync(string subscriptionId, string newPriceId, CancellationToken cancellationToken = default);
        Task<Stripe.Subscription?> GetSubscriptionAsync(string subscriptionId, CancellationToken cancellationToken = default);
        #endregion

        #region Payment & Checkout
        Task<Stripe.Checkout.Session> CreateCheckoutSessionAsync(global::Shared.Models.Company company, global::Shared.Models.Plan plan, string successUrl, string cancelUrl, CancellationToken cancellationToken = default);
        Task<Stripe.BillingPortal.Session> CreateCustomerPortalSessionAsync(string customerId, string returnUrl, CancellationToken cancellationToken = default);
        Task<Stripe.PaymentIntent> CreatePaymentIntentAsync(decimal amount, string currency, string customerId, CancellationToken cancellationToken = default);
        #endregion

        #region Invoice & Billing
        Task<Stripe.Invoice?> GetInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default);
        Task<Stripe.Invoice?> GetUpcomingInvoiceAsync(string customerId, string? subscriptionId, string? priceId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Stripe.Invoice>> ListInvoicesAsync(string customerId, int limit, CancellationToken cancellationToken = default);
        Task SendInvoiceAsync(string invoiceId, CancellationToken cancellationToken = default);
        #endregion
    }
}
