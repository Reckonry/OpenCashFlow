using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.Models;
using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Repositories
{
    public partial class CompanyRepository(ApplicationDbContext context, ILogger<CompanyRepository> logger) : ICompanyRepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<CompanyRepository> _logger = logger;

        public async Task<Company?> GetCompanyByIdAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Company_DS.AsNoTracking()
                .Where(p => p.TenantID == TenantID).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Guid?> GetUserTenantIDAsync(Guid UserID, CancellationToken cancellationToken)
        {
            // Fix: Correctly select the TenantID from Company_Staff_DS and return it.
            return await _context.Company_Staff_DS.AsNoTracking()
                .Where(p => p.UserID == UserID)
                .Select(x => x.TenantID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<string?> GetCompanySecretByTenantIDAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Company_DS.AsNoTracking()
                .Where(cs => cs.TenantID == TenantID)
                .Select(cs => cs.CompanySecret)
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}