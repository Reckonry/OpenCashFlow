using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Companies;

namespace OpenCashFlow.API.Controllers
{
    public partial class CompanyController
    {
        [HttpGet("[controller]/Invoices")] 
        public async Task<ActionResult<IEnumerable<Company_Invoice_List_DTO>>> GetCompanyInvoices(CancellationToken cancellationToken)
        {
            var invoices = await _companyService.GetCompanyInvoicesAsync(cancellationToken);
            return Ok(invoices);
        }

        [HttpGet("[controller]/Invoice/{InvoiceID}")]
        public async Task<ActionResult<Company_Invoices_Detail_DTO>> GetCompanyInvoiceByID(Guid InvoiceID, CancellationToken cancellationToken)
        {
            var invoice = await _companyService.GetCompanyInvoiceByIdAsync(InvoiceID, cancellationToken);
            return Ok(invoice);
        }
    }
}
