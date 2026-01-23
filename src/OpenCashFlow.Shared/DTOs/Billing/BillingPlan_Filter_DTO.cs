using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace Shared.DTOs.Billing
{
    public class BillingPlan_Filter_DTO
    {
        public bool OnlyActive { get; set; } = true;

        public bool? Visible { get; set; }

        public bool? HasTrial { get; set; }

        public BillingCycle? BillingCycle { get; set; }

        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 50;
    }
}
