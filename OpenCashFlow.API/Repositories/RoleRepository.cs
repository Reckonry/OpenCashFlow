using OpenCashFlow.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AspNetRole>> GetVisibleRolesAsync(CancellationToken cancellationToken)
        {
            return await _context.AspNetRole_DS
                .AsNoTracking()
                .Where(r => r.IsVisible && !r.IsDeleted)
                .OrderBy(r => r.RoleName)
                .ToListAsync(cancellationToken);
        }
    }
}

