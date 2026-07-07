using OpenCashFlow.Application.Companies.Invoices;
using OpenCashFlow.Contracts.DTOs.Companies;

namespace OpenCashFlow.API.Services
{
    public partial class CompanyService
    {
        private readonly IGetCompanyInvoicesUseCase _getCompanyInvoicesUseCase;
        private readonly IGetCompanyInvoiceDetailUseCase _getCompanyInvoiceDetailUseCase;

        public async Task<IEnumerable<Company_Invoice_List_DTO>?> GetCompanyInvoicesAsync(CancellationToken cancellationToken)
        {
            var invoices = await _getCompanyInvoicesUseCase.ExecuteAsync(_authenticationService.GetTenantID(), cancellationToken);
            return invoices.Select(MapInvoiceList).ToList();
        }

        public async Task<Company_Invoices_Detail_DTO?> GetCompanyInvoiceByIdAsync(Guid InvoiceID, CancellationToken cancellationToken)
        {
            var invoice = await _getCompanyInvoiceDetailUseCase.ExecuteAsync(InvoiceID, _authenticationService.GetTenantID(), cancellationToken);
            return invoice is null ? null : MapInvoiceDetail(invoice);
        }
    }
}
