using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.Invoices;

public interface IGetCompanyInvoicesUseCase
{
    Task<IReadOnlyList<CompanyInvoiceListItem>> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default);
}
