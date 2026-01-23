using System;
using Shared.Models;

namespace Shared.DTOs.Billing
{
    public class BillingSubscription_List_DTO
    {
        public Guid SubscriptionID { get; set; }
        public Guid TenantID { get; set; }
        public string? CompanyName { get; set; }
        public Guid PlanID { get; set; }
        public string PlanCode { get; set; } = string.Empty;
        public string PlanName { get; set; } = string.Empty;
        public bool PlanHasTrial { get; set; }
        public int? PlanTrialDays { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime NextBillingDate { get; set; }
        public BillingCycle BillingCycle { get; set; }
        public int BillingDay { get; set; }
        public RenewalStatus RenewalStatus { get; set; }
        public DateTime LastReminderDate { get; set; }
        public DateTime NextReminderDate { get; set; }
        public double Cost { get; set; }
        public double? Discount { get; set; }
        public string? PromoCode { get; set; }
        public DateTime? DiscountExpiration { get; set; }
        public DateTime? CancellationDate { get; set; }
        public string? CancellationReason { get; set; }
        public DateTime DateIns { get; set; }
        public DateTime? DateEdit { get; set; }
    }
}
