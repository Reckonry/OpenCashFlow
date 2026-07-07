using OpenCashFlow.Application.Roles.Models;
using OpenCashFlow.Application.Roles.Ports;

namespace OpenCashFlow.Application.Roles.GetRoles;

public sealed class GetRolesUseCase(IRoleReader roleReader) : IGetRolesUseCase
{
    public Task<IReadOnlyList<RoleListItem>> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        return roleReader.GetVisibleRolesAsync(cancellationToken);
    }
}
