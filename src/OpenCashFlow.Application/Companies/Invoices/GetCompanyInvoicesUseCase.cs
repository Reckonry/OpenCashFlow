using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;

namespace OpenCashFlow.Application.Companies.Invoices;

public sealed class GetCompanyInvoicesUseCase(ICompanyReader companyReader) : IGetCompanyInvoicesUseCase
{
    public Task<IReadOnlyList<CompanyInvoiceListItem>> ExecuteAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (tenantId == Guid.Empty)
        {
            return Task.FromResult<IReadOnlyList<CompanyInvoiceListItem>>(Array.Empty<CompanyInvoiceListItem>());
        }

        return companyReader.GetInvoicesAsync(tenantId, cancellationToken);
    }
}
