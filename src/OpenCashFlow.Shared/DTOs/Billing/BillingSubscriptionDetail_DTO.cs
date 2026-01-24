using Shared.Models;
using System;
using System.Collections.Generic;

namespace Shared.DTOs.Billing
{
    /// <summary>
    /// Detailed subscription information for admin management
    /// </summary>
    public class BillingSubscriptionDetail_DTO
    {
        public Guid SubscriptionID { get; set; }
        public Guid TenantID { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? BillingEmail { get; set; }
        public Guid PlanID { get; set; }
        public string PlanName { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public int BillingDay { get; set; }
        public RenewalStatus RenewalStatus { get; set; }
        public double Cost { get; set; }
        public double? Discount { get; set; }
        public string? PromoCode { get; set; }
        public DateTime? DiscountExpiration { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }

        // Stripe specific information
        public string? StripeSubscriptionID { get; set; }
        public string? StripePriceID { get; set; }
        public string? StripeInvoiceID { get; set; }
        public string? StripeCustomerID { get; set; }
        public string? StripeDefaultPaymentMethodID { get; set; }

        // Payment methods
        public List<StripePaymentMethod_DTO> PaymentMethods { get; set; } = new();

        // Recent webhook events
        public List<StripeWebhookEventSummary_DTO> RecentWebhookEvents { get; set; } = new();
    }

    /// <summary>
    /// Admin audit log entry
    /// </summary>
    public class BillingAuditLogEntry_DTO
    {
        public DateTime Timestamp { get; set; }
        public string Action { get; set; } = string.Empty;
        public string Details { get; set; } = string.Empty;
        public string Level { get; set; } = "Information"; // Information, Warning, Error
        public string? PerformedBy { get; set; }
    }

    /// <summary>
    /// Stripe payment method information
    /// </summary>
    public class StripePaymentMethod_DTO
    {
        public string PaymentMethodId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Last4 { get; set; }
        public int? ExpMonth { get; set; }
        public int? ExpYear { get; set; }
        public bool IsDefault { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    /// <summary>
    /// Webhook event summary for subscription history
    /// </summary>
    public class StripeWebhookEventSummary_DTO
    {
        public Guid Id { get; set; }
        public string EventId { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public DateTime CreatedUtc { get; set; }
        public DateTime ReceivedUtc { get; set; }
        public DateTime? ProcessedUtc { get; set; }
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
