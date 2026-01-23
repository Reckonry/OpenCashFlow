using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<List<AspNetRole>> GetVisibleRolesAsync(CancellationToken cancellationToken);
    }
}

