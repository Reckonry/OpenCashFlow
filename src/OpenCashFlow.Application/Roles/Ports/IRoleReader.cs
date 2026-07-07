using OpenCashFlow.Application.Roles.Models;

namespace OpenCashFlow.Application.Roles.Ports;

public interface IRoleReader
{
    Task<IReadOnlyList<RoleListItem>> GetVisibleRolesAsync(CancellationToken cancellationToken = default);
}
