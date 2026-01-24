using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    public class BillingCancelSubscriptionRequest_DTO
    {
        /// <summary>
        /// Se true la subscription viene cancellata immediatamente su Stripe.
        /// </summary>
        [Required]
        public bool Immediately { get; set; }
    }
}
