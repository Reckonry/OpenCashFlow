using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.Models;

namespace OpenCashFlow.API.Repositories
{
    public partial class CompanyRepository
    {
        public async Task<IEnumerable<Company_Invoice>?> GetCompanyInvoicesAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Company_Invoice_DS.AsNoTracking()
                .Where(p => p.TenantID == TenantID).ToListAsync(cancellationToken);
        }
        
        public async Task<Company_Invoice?> GetCompanyInvoiceByIdAsync(Guid InvoiceID, Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Company_Invoice_DS.AsNoTracking()
                .Where(p => p.TenantID == TenantID && p.InvoiceID == InvoiceID)
                .Include(i=>i.Items).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken)
        {
            return await _context.Company_DS.AsNoTracking().ToListAsync(cancellationToken);
        }
    }
}