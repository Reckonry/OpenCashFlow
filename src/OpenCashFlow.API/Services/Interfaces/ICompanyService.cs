using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Companies;

namespace OpenCashFlow.API.Services.Interfaces
{
    public interface ICompanyService
    {
        Task<Company_Detail_DTO?> GetCompanyAsync(CancellationToken cancellationToken);
        Task<Company_Detail_DTO?> GetCompanyAsync(Guid TenantID, CancellationToken cancellationToken);
        Task<IEnumerable<Company_Detail_DTO>?> GetAllCompaniesAsync(CancellationToken cancellationToken);

        #region invoices
        Task<IEnumerable<Company_Invoice_List_DTO>?> GetCompanyInvoicesAsync(CancellationToken cancellationToken);
        Task<Company_Invoices_Detail_DTO?> GetCompanyInvoiceByIdAsync(Guid InvoiceID, CancellationToken cancellationToken);
        #endregion
    }
}
