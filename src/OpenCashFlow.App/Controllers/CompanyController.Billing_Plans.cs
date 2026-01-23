using OpenCashFlow.App.ViewModels;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Companies;
using global::Shared.Models;

namespace OpenCashFlow.App.Controllers
{
    public partial class CompanyController : Controller
    {
        [HttpGet("Billing_And_Plans"), ActionName("Company_Billing_Plans")]
        public IActionResult BillingAndPlans()
        {
            Company_Billing_Plans_ViewModel viewModel = new(
                _companyAPIService.GetCompanyInvoicesAsync().Result?.Data ?? []
            );
            return View(viewModel);
        }

        [HttpGet("Invoices/View/{InvoiceID}"), ActionName("View_Invoice")]
        public IActionResult ViewInvoice(Guid InvoiceID)
        {
            Company_Invoices_Detail_DTO? Invoice = _companyAPIService.GetCompanyInvoiceByIDAsync(InvoiceID).Result?.Data;
            return View(Invoice);
        }
    }
}
