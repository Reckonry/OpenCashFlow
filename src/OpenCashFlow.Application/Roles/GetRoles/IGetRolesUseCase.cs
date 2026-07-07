using OpenCashFlow.Application.Roles.Models;

namespace OpenCashFlow.Application.Roles.GetRoles;

public interface IGetRolesUseCase
{
    Task<IReadOnlyList<RoleListItem>> ExecuteAsync(CancellationToken cancellationToken = default);
}
