using OpenCashFlow.Contracts.DTOs.Identity;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface IRoleService
    {
        Task<IEnumerable<Role_List_DTO>> GetVisibleRolesAsync(CancellationToken cancellationToken);
    }
}

