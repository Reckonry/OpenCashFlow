using Shared.Models;
using System;
using System.Collections.Generic;

namespace Shared.DTOs.Billing
{
    /// <summary>
    /// DTO for customer billing portal - complete billing information for logged-in user
    /// </summary>
    public class BillingPortal_DTO
    {
        // Current Subscription Info
        public Guid? SubscriptionID { get; set; }
        public string? PlanName { get; set; }
        public string? PlanDescription { get; set; }
        public double Cost { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public DateTime? NextBillingDate { get; set; }
        public DateTime? EndDate { get; set; }
        public RenewalStatus RenewalStatus { get; set; }
        public bool IsTrialing { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public double? Discount { get; set; }
        public string? PromoCode { get; set; }

        // Stripe Integration
        public bool HasStripeIntegration { get; set; }
        public string? StripeCustomerID { get; set; }
        public string? StripeSubscriptionID { get; set; }
        public string? CustomerPortalUrl { get; set; }

        // Payment Status
        public bool HasPaymentFailed { get; set; }
        public string? PaymentFailureReason { get; set; }
        public DateTime? LastPaymentFailedDate { get; set; }

        // Payment Methods (for Stripe)
        public List<StripePaymentMethod_DTO> PaymentMethods { get; set; } = new();

        // Invoices
        public List<BillingInvoiceSummary_DTO> RecentInvoices { get; set; } = new();
        public BillingInvoice_DTO? UpcomingInvoice { get; set; }

        // Available Plans (for upgrade/downgrade)
        public List<BillingPlan_List_DTO> AvailablePlans { get; set; } = new();
    }

    /// <summary>
    /// Summary of an invoice for list display
    /// </summary>
    public class BillingInvoiceSummary_DTO
    {
        public string InvoiceId { get; set; } = string.Empty;
        public string InvoiceNumber { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EUR";
        public string Status { get; set; } = string.Empty; // paid, open, void, uncollectible
        public string? PdfUrl { get; set; }
        public bool IsPaid { get; set; }
    }
}
