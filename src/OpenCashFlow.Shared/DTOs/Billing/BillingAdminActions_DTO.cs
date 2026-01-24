using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    /// <summary>
    /// Request to sync subscription from Stripe
    /// </summary>
    public class BillingSyncSubscriptionRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }
    }

    /// <summary>
    /// Request to extend trial period
    /// </summary>
    public class BillingExtendTrialRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }

        [Required]
        [Range(1, 365)]
        public int Days { get; set; }

        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request to apply credit to subscription
    /// </summary>
    public class BillingApplyCreditRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }

        [Required]
        [Range(0.01, 999999.99)]
        public decimal Amount { get; set; }

        [Required]
        [MaxLength(500)]
        public required string Description { get; set; }

        public string? Currency { get; set; } = "EUR";
    }

    /// <summary>
    /// Request to manually send invoice
    /// </summary>
    public class BillingSendInvoiceRequest_DTO
    {
        [Required]
        public required string StripeInvoiceID { get; set; }

        [EmailAddress]
        public string? EmailOverride { get; set; }
    }

    /// <summary>
    /// Request to force admin cancellation
    /// </summary>
    public class BillingAdminCancelRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }

        public bool Immediately { get; set; } = false;

        [Required]
        [MaxLength(1000)]
        public required string AdminReason { get; set; }

        public bool NotifyCustomer { get; set; } = true;
    }

    /// <summary>
    /// Result of admin action
    /// </summary>
    public class BillingAdminActionResult_DTO
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }
    }
}
