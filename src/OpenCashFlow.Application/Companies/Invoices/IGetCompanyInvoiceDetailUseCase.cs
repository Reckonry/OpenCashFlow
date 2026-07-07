using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.Application.Companies.Invoices;

public interface IGetCompanyInvoiceDetailUseCase
{
    Task<CompanyInvoiceDetailResult?> ExecuteAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default);
}
