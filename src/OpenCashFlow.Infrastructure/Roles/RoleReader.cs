using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Roles.Models;
using OpenCashFlow.Application.Roles.Ports;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Roles;

public sealed class RoleReader(ApplicationDbContext db) : IRoleReader
{
    public async Task<IReadOnlyList<RoleListItem>> GetVisibleRolesAsync(CancellationToken cancellationToken = default)
    {
        return await db.AspNetRole_DS
            .AsNoTracking()
            .Where(role => role.IsVisible && !role.IsDeleted)
            .OrderBy(role => role.RoleName)
            .Select(role => new RoleListItem(role.RoleID, role.RoleName))
            .ToListAsync(cancellationToken);
    }
}
