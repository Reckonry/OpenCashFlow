using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Shared.Models;

namespace Shared.DTOs.Billing
{
    public class BillingSubscription_Filter_DTO
    {
        public Guid? SubscriptionID { get; set; }

        public Guid? TenantID { get; set; }

        public Guid? PlanID { get; set; }

        public RenewalStatus? RenewalStatus { get; set; }

        /// <summary>
        /// Allows filtering by multiple statuses at the same time.
        /// </summary>
        public ICollection<RenewalStatus>? RenewalStatuses { get; set; }

        public BillingCycle? BillingCycle { get; set; }

        /// <summary>
        /// Filters subscriptions active on a specific date (StartDate <= ActiveOn <= EndDate).
        /// </summary>
        public DateTime? ActiveOn { get; set; }

        /// <summary>
        /// Filters subscriptions with a start date on or after this value.
        /// </summary>
        public DateTime? StartFrom { get; set; }

        /// <summary>
        /// Filters subscriptions with a start date on or before this value.
        /// </summary>
        public DateTime? StartTo { get; set; }

        /// <summary>
        /// Filters subscriptions with an end date on or after this value.
        /// </summary>
        public DateTime? EndFrom { get; set; }

        /// <summary>
        /// Filters subscriptions with an end date on or before this value.
        /// </summary>
        public DateTime? EndTo { get; set; }

        /// <summary>
        /// Filters subscriptions with the next renewal (NextBillingDate) on or after this value.
        /// </summary>
        public DateTime? NextBillingFrom { get; set; }

        /// <summary>
        /// Filters subscriptions with the next renewal (NextBillingDate) on or before this value.
        /// </summary>
        public DateTime? NextBillingTo { get; set; }

        public bool? HasTrial { get; set; }

        /// <summary>
        /// If false, forces the use of the current user's company.
        /// </summary>
        public bool IncludeAllCompanies { get; set; }

        public string? Search { get; set; }

        [Range(1, int.MaxValue)]
        public int Page { get; set; } = 1;

        [Range(1, 200)]
        public int PageSize { get; set; } = 50;
    }
}
