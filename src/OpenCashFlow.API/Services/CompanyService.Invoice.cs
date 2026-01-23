using AutoMapper;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using global::Shared.DTOs;
using global::Shared.DTOs.Companies;
using global::Shared.Models;

namespace OpenCashFlow.API.Services
{
    public partial class CompanyService
    {
        public async Task<IEnumerable<Company_Invoice>?> GetCompanyInvoicesAsync(CancellationToken cancellationToken)
        {
            var invoices = await _companyRepository.GetCompanyInvoicesAsync(_authenticationService.GetTenantID(), cancellationToken);
            return invoices;
        }

        public async Task<Company_Invoices_Detail_DTO?> GetCompanyInvoiceByIdAsync(Guid InvoiceID, CancellationToken cancellationToken)
        {
            var invoice = await _companyRepository.GetCompanyInvoiceByIdAsync(InvoiceID, _authenticationService.GetTenantID(), cancellationToken);

            return _mapper.Map<Company_Invoices_Detail_DTO>(invoice);
        }
    }
}
