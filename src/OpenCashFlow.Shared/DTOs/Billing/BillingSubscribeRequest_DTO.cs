using System;
using System.ComponentModel.DataAnnotations;

namespace Shared.DTOs.Billing
{
    public class BillingSubscribeRequest_DTO
    {
        [Required]
        public Guid PlanId { get; set; }

        /// <summary>
        /// Consente di forzare il calcolo proration al cambio piano (default true).
        /// </summary>
        public bool Prorate { get; set; } = true;
    }
}
