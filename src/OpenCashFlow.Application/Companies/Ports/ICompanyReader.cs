using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.Ports;

public interface ICompanyReader
{
    Task<CompanyResult?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyResult>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<CompanyInvoiceListItem>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken = default);
    Task<CompanyInvoiceDetailResult?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default);
}
