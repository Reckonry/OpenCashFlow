using System;

namespace Shared.DTOs.Billing
{
    public class BillingSubscriptionActionResult_DTO
    {
        public required string StripeSubscriptionId { get; set; }
        public string? StripeCustomerId { get; set; }
        public string? StripePriceId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CurrentPeriodStartUtc { get; set; }
        public DateTime? CurrentPeriodEndUtc { get; set; }
        public string? LatestInvoiceId { get; set; }
    }
}
