using global::Shared.Models;
using global::Shared.Models.Identity;

namespace OpenCashFlow.API.Repositories.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company?> GetCompanyByIdAsync(Guid TenantID, CancellationToken cancellationToken);
        Task<Guid?> GetUserTenantIDAsync(Guid UserID, CancellationToken cancellationToken);
        Task<string?> GetCompanySecretByTenantIDAsync(Guid TenantID, CancellationToken cancellationToken);

        #region Invoices
        Task<IEnumerable<Company_Invoice>?> GetCompanyInvoicesAsync(Guid TenantID, CancellationToken cancellationToken);
        Task<Company_Invoice?> GetCompanyInvoiceByIdAsync(Guid InvoiceID, Guid TenantID, CancellationToken cancellationToken);
        Task<List<Company>> GetAllCompaniesAsync(CancellationToken cancellationToken);
        #endregion

    }
}
