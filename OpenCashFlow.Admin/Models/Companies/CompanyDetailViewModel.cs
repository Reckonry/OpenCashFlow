using System;
using System.Collections.Generic;
using OpenCashFlow.Admin.Models.Billing;
using global::Shared.DTOs;
using global::Shared.DTOs.Billing;

namespace OpenCashFlow.Admin.Models.Companies
{
    public class CompanyDetailViewModel
    {
        public required Company_Detail_DTO Company { get; init; }

        public IReadOnlyList<BillingSubscription_List_DTO> Subscriptions { get; init; } = Array.Empty<BillingSubscription_List_DTO>();

        public BillingSubscription_List_DTO? ActiveSubscription { get; init; }

        public bool HasBillingData => ActiveSubscription != null || (Subscriptions?.Count ?? 0) > 0;

        public BillingSubscriptionFilters AppliedFilters { get; init; } = new();
    }
}
