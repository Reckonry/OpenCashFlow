using Shared.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    /// <summary>
    /// Request per modificare manualmente una subscription (senza Stripe)
    /// </summary>
    public class BillingManualUpdateSubscriptionRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }

        [Required]
        public DateTime NextBillingDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public RenewalStatus RenewalStatus { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Cost { get; set; }

        [Range(0, 100)]
        public double? Discount { get; set; }

        [MaxLength(50)]
        public string? PromoCode { get; set; }

        public DateTime? DiscountExpiration { get; set; }

        [MaxLength(1000)]
        public string? AdminNotes { get; set; }
    }

    /// <summary>
    /// Request per estendere manualmente una subscription
    /// </summary>
    public class BillingManualExtendRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }

        [Required]
        [Range(1, 36)]
        public int Months { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }

    /// <summary>
    /// Request per attivare/riattivare manualmente una subscription
    /// </summary>
    public class BillingManualActivateRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }

        [Required]
        public Guid PlanID { get; set; }

        [Required]
        public DateTime StartDate { get; set; }

        [Required]
        public DateTime EndDate { get; set; }

        [Required]
        public BillingCycle BillingCycle { get; set; }

        [Required]
        [Range(0, double.MaxValue)]
        public double Cost { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }
    }
}
