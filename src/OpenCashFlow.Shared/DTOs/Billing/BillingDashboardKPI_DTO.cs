namespace Shared.DTOs.Billing
{
    public class BillingDashboardKPI_DTO
    {
        public int ActiveTrials { get; set; }
        public int UpcomingRenewals { get; set; }
        public int UpcomingRenewalsNext7Days { get; set; }
        public int UpcomingRenewalsNext30Days { get; set; }
        public decimal MonthlyRecurringRevenue { get; set; }
        public decimal QuarterlyRecurringRevenue { get; set; }
        public decimal YearlyRecurringRevenue { get; set; }
        public int AttentionRequired { get; set; }
        public int FailedRenewals { get; set; }
        public int ExpiringTrials { get; set; }
        public int PendingCancellations { get; set; }
        public decimal MRRGrowthPercentage { get; set; }
        public int TotalActiveSubscriptions { get; set; }
    }
}
