using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Roles.GetRoles;
using OpenCashFlow.Contracts.DTOs.Identity;

namespace OpenCashFlow.API.Services
{
    public class RoleService : IRoleService
    {
        private readonly IGetRolesUseCase _getRolesUseCase;

        public RoleService(IGetRolesUseCase getRolesUseCase)
        {
            _getRolesUseCase = getRolesUseCase;
        }

        public async Task<IEnumerable<Role_List_DTO>> GetVisibleRolesAsync(CancellationToken cancellationToken)
        {
            var roles = await _getRolesUseCase.ExecuteAsync(cancellationToken);
            return roles.Select(role => new Role_List_DTO
            {
                ID = role.ID,
                Name = role.Name
            });
        }
    }
}
