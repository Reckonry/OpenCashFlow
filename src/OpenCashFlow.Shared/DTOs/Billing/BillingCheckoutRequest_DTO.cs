using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    public class BillingCheckoutRequest_DTO
    {
        [Required]
        public Guid PlanId { get; set; }

        [Required, Url]
        public string SuccessUrl { get; set; } = string.Empty;

        [Required, Url]
        public string CancelUrl { get; set; } = string.Empty;
    }
}
