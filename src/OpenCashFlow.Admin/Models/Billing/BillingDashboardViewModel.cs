using OpenCashFlow.Contracts.DTOs.Billing;

namespace OpenCashFlow.Admin.Models.Billing
{
    public class BillingDashboardViewModel
    {
        public BillingDashboardKPI_DTO? KPI { get; set; }
        public string? ErrorMessage { get; set; }
    }
}
