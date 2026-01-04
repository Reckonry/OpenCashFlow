using System;
using System.Collections.Generic;
using global::Shared.DTOs.Billing;
using global::Shared.Models;

namespace OpenCashFlow.Admin.Models.Billing
{
    public class BillingSubscriptionsListViewModel
    {
        public IReadOnlyList<BillingSubscription_List_DTO> Items { get; init; } = Array.Empty<BillingSubscription_List_DTO>();

        public BillingSubscriptionFilters Filters { get; init; } = new();

        public bool HasNextPage { get; init; }

        public bool HasPreviousPage => Filters.Page > 1;

        public string? ErrorMessage { get; init; }

        public IReadOnlyList<BillingSavedView> SavedViews { get; init; } = Array.Empty<BillingSavedView>();
    }

    public class BillingSubscriptionFilters
    {
        public string? Search { get; set; }

        public RenewalStatus? Status { get; set; }

        public List<RenewalStatus>? Statuses { get; set; }

        public Guid? TenantID { get; set; }

        public BillingCycle? BillingCycle { get; set; }

        public bool? HasTrial { get; set; }

        public DateTime? ActiveOn { get; set; }

        public DateTime? StartFrom { get; set; }

        public DateTime? StartTo { get; set; }

        public DateTime? EndFrom { get; set; }

        public DateTime? EndTo { get; set; }

        public DateTime? NextBillingFrom { get; set; }

        public DateTime? NextBillingTo { get; set; }

        public bool IncludeAllCompanies { get; set; } = true;

        public int Page { get; set; } = 1;

        public int PageSize { get; set; } = 25;
    }

    public record BillingSavedView(string Name, string QueryString, DateTime SavedAtUtc);
}
