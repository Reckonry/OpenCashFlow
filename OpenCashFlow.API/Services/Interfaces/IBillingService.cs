using global::Shared.DTOs.Billing;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface IBillingService
    {
        Task<IEnumerable<BillingPlan_List_DTO>> GetPlansAsync(BillingPlan_Filter_DTO filters, CancellationToken cancellationToken);

        Task<IEnumerable<BillingSubscription_List_DTO>> GetSubscriptionsAsync(BillingSubscription_Filter_DTO filters, CancellationToken cancellationToken);

        Task<BillingDashboardKPI_DTO> GetDashboardKPIAsync(CancellationToken cancellationToken);

        Task<BillingCheckoutSession_DTO> CreateCheckoutSessionAsync(BillingCheckoutRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingPortalSession_DTO> CreateCustomerPortalSessionAsync(BillingPortalSessionRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingSubscriptionActionResult_DTO> CreateSubscriptionAsync(BillingSubscribeRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingSubscriptionActionResult_DTO> CancelSubscriptionAsync(Guid subscriptionId, bool immediately, CancellationToken cancellationToken);

        Task<BillingSubscriptionActionResult_DTO> ReactivateSubscriptionAsync(Guid subscriptionId, CancellationToken cancellationToken);

        Task<BillingSubscriptionActionResult_DTO> ChangeSubscriptionPlanAsync(Guid subscriptionId, BillingChangePlanRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingInvoice_DTO> GetLatestInvoiceAsync(Guid subscriptionId, CancellationToken cancellationToken);

        Task<BillingInvoice_DTO> GetUpcomingInvoiceAsync(Guid subscriptionId, CancellationToken cancellationToken);

        Task AddPaymentMethodAsync(BillingAddPaymentMethodRequest_DTO request, CancellationToken cancellationToken);

        Task RemovePaymentMethodAsync(string paymentMethodId, CancellationToken cancellationToken);

        // Admin Actions
        Task<BillingSubscriptionDetail_DTO> GetSubscriptionDetailAsync(Guid subscriptionId, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> SyncSubscriptionFromStripeAsync(Guid subscriptionId, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> ExtendTrialAsync(BillingExtendTrialRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> ApplyCreditAsync(BillingApplyCreditRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> SendInvoiceManuallyAsync(BillingSendInvoiceRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> AdminCancelSubscriptionAsync(BillingAdminCancelRequest_DTO request, CancellationToken cancellationToken);

        Task<IEnumerable<StripeWebhookEventSummary_DTO>> GetSubscriptionWebhookEventsAsync(Guid subscriptionId, int limit, CancellationToken cancellationToken);

        // Manual Subscription Management (no Stripe)
        Task<BillingAdminActionResult_DTO> ManualUpdateSubscriptionAsync(BillingManualUpdateSubscriptionRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> ManualExtendSubscriptionAsync(BillingManualExtendRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> ManualActivateSubscriptionAsync(BillingManualActivateRequest_DTO request, CancellationToken cancellationToken);

        Task<BillingAdminActionResult_DTO> AdminChangePlanAsync(BillingChangePlanRequest_DTO request, CancellationToken cancellationToken);

        Task<List<BillingAuditLogEntry_DTO>> GetSubscriptionAuditLogAsync(Guid subscriptionId, CancellationToken cancellationToken);

        // Customer Portal
        Task<BillingPortal_DTO> GetCustomerBillingPortalAsync(CancellationToken cancellationToken);
    }
}
