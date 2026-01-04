using global::Shared.Models;

namespace OpenCashFlow.API.Services.Interfaces
{
    /// <summary>
    /// Operations to keep local billing entities aligned with Stripe.
    /// </summary>
    public interface IStripeSyncService
    {
        Task<Company?> SyncCustomerFromStripeAsync(string stripeCustomerId, CancellationToken cancellationToken = default);
        Task<Company_Subscription?> SyncSubscriptionFromStripeAsync(string stripeSubscriptionId, CancellationToken cancellationToken = default);
        Task<Plan?> SyncPlanFromStripeAsync(string stripeProductId, CancellationToken cancellationToken = default);
        Task<int> SyncAllPlansAsync(CancellationToken cancellationToken = default);
        Task<Company?> EnsureCustomerExistsAsync(Guid companyId, CancellationToken cancellationToken = default);
    }
}
