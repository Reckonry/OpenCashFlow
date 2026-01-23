using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    /// <summary>
    /// Request per cambiare piano (sia Stripe che manuale)
    /// </summary>
    public class BillingChangePlanRequest_DTO
    {
        [Required]
        public Guid SubscriptionID { get; set; }

        [Required]
        public Guid NewPlanId { get; set; }

        [MaxLength(500)]
        public string? Reason { get; set; }

        public bool Prorate { get; set; } = true;
    }
}
