using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;

namespace OpenCashFlow.Application.Companies.Invoices;

public sealed class GetCompanyInvoiceDetailUseCase(ICompanyReader companyReader) : IGetCompanyInvoiceDetailUseCase
{
    public Task<CompanyInvoiceDetailResult?> ExecuteAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        if (invoiceId == Guid.Empty || tenantId == Guid.Empty)
        {
            return Task.FromResult<CompanyInvoiceDetailResult?>(null);
        }

        return companyReader.GetInvoiceByIdAsync(invoiceId, tenantId, cancellationToken);
    }
}
