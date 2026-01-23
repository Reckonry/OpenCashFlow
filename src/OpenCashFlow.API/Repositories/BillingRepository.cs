using OpenCashFlow.API.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using global::Shared.Data;
using global::Shared.DTOs.Billing;
using global::Shared.Models;

namespace OpenCashFlow.API.Repositories
{
    public class BillingRepository(ApplicationDbContext context, ILogger<BillingRepository> logger) : IBillingRepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<BillingRepository> _logger = logger;

        public async Task<IEnumerable<Plan>> GetPlansAsync(BillingPlan_Filter_DTO filters, CancellationToken cancellationToken)
        {
            var safePage = filters.Page <= 0 ? 1 : filters.Page;
            var safePageSize = filters.PageSize <= 0 ? 50 : filters.PageSize;

            var query = _context.Plan_DS.AsNoTracking()
                .Where(p => !p.IsDeleted);

            if (filters.OnlyActive)
            {
                query = query.Where(p => p.IsActive);
            }

            if (filters.Visible.HasValue)
            {
                query = query.Where(p => p.Visible == filters.Visible.Value);
            }

            if (filters.HasTrial.HasValue)
            {
                query = query.Where(p => p.HasTrial == filters.HasTrial.Value);
            }

            if (filters.BillingCycle.HasValue)
            {
                query = query.Where(p => p.BillingCycle == filters.BillingCycle.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.Trim();
                query = query.Where(p =>
                    EF.Functions.ILike(p.Name, $"%{search}%") ||
                    EF.Functions.ILike(p.PlanCode, $"%{search}%"));
            }

            query = query
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.Name);

            var skip = (safePage - 1) * safePageSize;

            return await query
                .Skip(skip)
                .Take(safePageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Company_Subscription>> GetSubscriptionsAsync(Guid? TenantID, BillingSubscription_Filter_DTO filters, CancellationToken cancellationToken)
        {
            var safePage = filters.Page <= 0 ? 1 : filters.Page;
            var safePageSize = filters.PageSize <= 0 ? 50 : filters.PageSize;

            var query = _context.Company_Subscription_DS.AsNoTracking()
                .Include(s => s.Plan)
                .Include(s => s.Company)
                .AsQueryable();

            if (!filters.IncludeAllCompanies)
            {
                if (!TenantID.HasValue)
                {
                    _logger.LogWarning("TenantID non disponibile per l'utente corrente. Nessuna subscription verrà restituita.");
                    return Array.Empty<Company_Subscription>();
                }

                query = query.Where(s => s.TenantID == TenantID.Value);
            }
            else if (filters.TenantID.HasValue)
            {
                query = query.Where(s => s.TenantID == filters.TenantID.Value);
            }

            if (filters.SubscriptionID.HasValue)
            {
                query = query.Where(s => s.SubscriptionID == filters.SubscriptionID.Value);
            }

            if (filters.PlanID.HasValue)
            {
                query = query.Where(s => s.PlanID == filters.PlanID.Value);
            }

            HashSet<RenewalStatus>? renewalStatuses = null;
            if (filters.RenewalStatuses != null && filters.RenewalStatuses.Count > 0)
            {
                renewalStatuses = filters.RenewalStatuses
                    .Where(status => Enum.IsDefined(typeof(RenewalStatus), status))
                    .ToHashSet();
            }

            if (filters.RenewalStatus.HasValue)
            {
                renewalStatuses ??= new HashSet<RenewalStatus>();
                renewalStatuses.Add(filters.RenewalStatus.Value);
            }

            if (renewalStatuses != null && renewalStatuses.Count > 0)
            {
                query = query.Where(s => renewalStatuses.Contains(s.RenewalStatus));
            }

            if (filters.BillingCycle.HasValue)
            {
                query = query.Where(s => s.BillingCycle == filters.BillingCycle.Value);
            }

            if (filters.ActiveOn.HasValue)
            {
                var activeOn = filters.ActiveOn.Value;
                query = query.Where(s => s.StartDate <= activeOn && s.EndDate >= activeOn);
            }

            if (filters.StartFrom.HasValue)
            {
                var from = filters.StartFrom.Value;
                query = query.Where(s => s.StartDate >= from);
            }

            if (filters.StartTo.HasValue)
            {
                var to = filters.StartTo.Value;
                query = query.Where(s => s.StartDate <= to);
            }

            if (filters.EndFrom.HasValue)
            {
                var from = filters.EndFrom.Value;
                query = query.Where(s => s.EndDate >= from);
            }

            if (filters.EndTo.HasValue)
            {
                var to = filters.EndTo.Value;
                query = query.Where(s => s.EndDate <= to);
            }

            if (filters.NextBillingFrom.HasValue)
            {
                var from = filters.NextBillingFrom.Value;
                query = query.Where(s => s.NextBillingDate >= from);
            }

            if (filters.NextBillingTo.HasValue)
            {
                var to = filters.NextBillingTo.Value;
                query = query.Where(s => s.NextBillingDate <= to);
            }

            if (filters.HasTrial.HasValue)
            {
                query = query.Where(s => s.Plan != null && s.Plan.HasTrial == filters.HasTrial.Value);
            }

            if (!string.IsNullOrWhiteSpace(filters.Search))
            {
                var search = filters.Search.Trim();
                query = query.Where(s =>
                    (s.Plan != null && (EF.Functions.ILike(s.Plan.Name, $"%{search}%") || EF.Functions.ILike(s.Plan.PlanCode, $"%{search}%"))) ||
                    (s.Company != null && EF.Functions.ILike(s.Company.CompanyName ?? string.Empty, $"%{search}%")));
            }

            query = query.OrderByDescending(s => s.NextBillingDate);

            var skip = (safePage - 1) * safePageSize;

            return await query
                .Skip(skip)
                .Take(safePageSize)
                .ToListAsync(cancellationToken);
        }

        public async Task<BillingDashboardKPI_DTO> GetDashboardKPIAsync(CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            var next7Days = now.AddDays(7);
            var next30Days = now.AddDays(30);
            var previousMonthStart = now.AddMonths(-1).Date;

            var allActiveSubscriptions = await _context.Company_Subscription_DS
                .AsNoTracking()
                .Include(s => s.Plan)
                .Where(s => s.RenewalStatus == RenewalStatus.ACTIVE && s.StartDate <= now && s.EndDate >= now)
                .ToListAsync(cancellationToken);

            var previousMonthSubscriptions = await _context.Company_Subscription_DS
                .AsNoTracking()
                .Include(s => s.Plan)
                .Where(s => s.RenewalStatus == RenewalStatus.ACTIVE && s.StartDate <= previousMonthStart && s.EndDate >= previousMonthStart)
                .ToListAsync(cancellationToken);

            var activeTrials = await _context.Company_Subscription_DS
                .AsNoTracking()
                .CountAsync(s => s.RenewalStatus == RenewalStatus.TRIAL, cancellationToken);

            var upcomingRenewalsNext7 = allActiveSubscriptions.Count(s =>
                s.NextBillingDate >= now && s.NextBillingDate <= next7Days);

            var upcomingRenewalsNext30 = allActiveSubscriptions.Count(s =>
                s.NextBillingDate >= now && s.NextBillingDate <= next30Days);

            var suspendedSubscriptions = await _context.Company_Subscription_DS
                .AsNoTracking()
                .CountAsync(s => s.RenewalStatus == RenewalStatus.SUSPENDED, cancellationToken);

            var expiringTrials = await _context.Company_Subscription_DS
                .AsNoTracking()
                .CountAsync(s => s.RenewalStatus == RenewalStatus.TRIAL && s.EndDate >= now && s.EndDate <= next7Days, cancellationToken);

            var cancelledSubscriptions = await _context.Company_Subscription_DS
                .AsNoTracking()
                .CountAsync(s => s.RenewalStatus == RenewalStatus.CANCELLED && s.CancellationDate != null && s.CancellationDate >= now.AddDays(-30), cancellationToken);

            var attentionRequired = suspendedSubscriptions + expiringTrials + cancelledSubscriptions;

            var monthlyRevenue = allActiveSubscriptions
                .Where(s => s.BillingCycle == BillingCycle.MONTHLY)
                .Sum(s => s.Cost);

            var yearlyRevenue = allActiveSubscriptions
                .Where(s => s.BillingCycle == BillingCycle.YEARLY)
                .Sum(s => s.Cost);

            var previousMonthMRR = previousMonthSubscriptions
                .Where(s => s.BillingCycle == BillingCycle.MONTHLY)
                .Sum(s => s.Cost);

            var mrrGrowth = previousMonthMRR > 0
                ? ((monthlyRevenue - previousMonthMRR) / previousMonthMRR) * 100
                : 0;

            return new BillingDashboardKPI_DTO
            {
                ActiveTrials = activeTrials,
                UpcomingRenewals = upcomingRenewalsNext30,
                UpcomingRenewalsNext7Days = upcomingRenewalsNext7,
                UpcomingRenewalsNext30Days = upcomingRenewalsNext30,
                MonthlyRecurringRevenue = (decimal)monthlyRevenue,
                QuarterlyRecurringRevenue = 0,
                YearlyRecurringRevenue = (decimal)yearlyRevenue,
                AttentionRequired = attentionRequired,
                FailedRenewals = suspendedSubscriptions,
                ExpiringTrials = expiringTrials,
                PendingCancellations = cancelledSubscriptions,
                MRRGrowthPercentage = Math.Round((decimal)mrrGrowth, 2),
                TotalActiveSubscriptions = allActiveSubscriptions.Count
            };
        }

        public Task<Plan?> GetPlanByIdAsync(Guid planId, CancellationToken cancellationToken)
        {
            return _context.Plan_DS
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PlanID == planId, cancellationToken);
        }

        public Task<Company?> GetCompanyByIdAsync(Guid companyId, CancellationToken cancellationToken)
        {
            return _context.Company_DS
                .Include(c => c.CompanyAddresses)
                .FirstOrDefaultAsync(c => c.TenantID == companyId, cancellationToken);
        }

        public Task<Company_Subscription?> GetSubscriptionByIdAsync(Guid subscriptionId, CancellationToken cancellationToken)
        {
            return _context.Company_Subscription_DS
                .Include(s => s.Plan)
                .Include(s => s.Company)
                .FirstOrDefaultAsync(s => s.SubscriptionID == subscriptionId, cancellationToken);
        }

        public Task<Company_Subscription?> GetLatestSubscriptionForCompanyAsync(Guid companyId, CancellationToken cancellationToken)
        {
            return _context.Company_Subscription_DS
                .AsNoTracking()
                .Include(s => s.Plan)
                .Where(s => s.TenantID == companyId)
                .OrderByDescending(s => s.DateIns)
                .ThenByDescending(s => s.StartDate)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public Task SaveChangesAsync(CancellationToken cancellationToken)
        {
            return _context.SaveChangesAsync(cancellationToken);
        }
    }
}
