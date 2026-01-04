using global::Shared.DTOs.Billing;
using global::Shared.Models;

namespace OpenCashFlow.API.Repositories.Interfaces
{
    public interface IBillingRepository
    {
        Task<IEnumerable<Plan>> GetPlansAsync(BillingPlan_Filter_DTO filters, CancellationToken cancellationToken);

        Task<IEnumerable<Company_Subscription>> GetSubscriptionsAsync(Guid? TenantID, BillingSubscription_Filter_DTO filters, CancellationToken cancellationToken);

        Task<BillingDashboardKPI_DTO> GetDashboardKPIAsync(CancellationToken cancellationToken);

        Task<Plan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken);

        Task<Company?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken);

        Task<Company_Subscription?> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken cancellationToken);

        Task<Company_Subscription?> GetLatestSubscriptionForCompanyAsync(Guid companyId, CancellationToken cancellationToken);

        Task SaveChangesAsync(CancellationToken cancellationToken);
    }
}
