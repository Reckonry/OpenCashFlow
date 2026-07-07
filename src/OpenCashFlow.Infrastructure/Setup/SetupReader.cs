using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Setup.GetSetupStatus;
using OpenCashFlow.Application.Setup.Ports;
using OpenCashFlow.Contracts.Core;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Setup;

public sealed class SetupReader(ApplicationDbContext db) : ISetupReader
{
    public async Task<SetupStatusResult> GetStatusAsync(CancellationToken cancellationToken = default)
    {
        var hasCompanies = await db.Company_DS.AsNoTracking().AnyAsync(cancellationToken);
        var hasAdminUsers = await db.AspNetUserRole_DS.AsNoTracking()
            .AnyAsync(r => r.RoleID == Configuration.CompanyAdminRoleID || r.RoleID == Configuration.InstanceAdminRoleID, cancellationToken);

        return new SetupStatusResult(
            RequiresSetup: !hasCompanies || !hasAdminUsers,
            HasCompanies: hasCompanies,
            HasAdminUsers: hasAdminUsers);
    }
}
