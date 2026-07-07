using OpenCashFlow.Contracts.DTOs.Billing;
using System;
using System.Collections.Generic;

namespace OpenCashFlow.Admin.Models.Billing
{
    public class ManageSubscriptionViewModel
    {
        public BillingSubscriptionDetail_DTO? Subscription { get; set; }
        public IReadOnlyList<BillingPlan_List_DTO> AvailablePlans { get; set; } = [];
        public List<BillingAuditLogEntry_DTO> AuditLog { get; set; } = [];
        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }

        // Determines whether the subscription is managed manually (no Stripe)
        public bool IsManualSubscription => string.IsNullOrWhiteSpace(Subscription?.StripeSubscriptionID);

        // For Stripe Dashboard link (only if Stripe is present)
        public string? StripeDashboardUrl => !string.IsNullOrWhiteSpace(Subscription?.StripeSubscriptionID)
            ? $"https://dashboard.stripe.com/subscriptions/{Subscription.StripeSubscriptionID}"
            : null;

        public string? StripeCustomerUrl => !string.IsNullOrWhiteSpace(Subscription?.StripeCustomerID)
            ? $"https://dashboard.stripe.com/customers/{Subscription.StripeCustomerID}"
            : null;
    }
}
