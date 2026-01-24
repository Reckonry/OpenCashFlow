using System;
using Shared.Models;

namespace Shared.DTOs.Billing
{
    public class BillingPlan_List_DTO
    {
        public Guid PlanID { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int DurationMonths { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public bool HasTrial { get; set; }
        public int? TrialDays { get; set; }
        public bool IsActive { get; set; }
        public bool Visible { get; set; }
        public int SortOrder { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
