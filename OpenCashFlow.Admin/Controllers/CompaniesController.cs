using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpenCashFlow.Admin.Models;
using OpenCashFlow.Admin.Models.Billing;
using OpenCashFlow.Admin.Models.Companies;
using global::Shared.Models;
using OpenCashFlow.Admin.Services;
using global::Shared.DTOs;
using global::Shared.DTOs.Billing;
using System.Collections.Generic;
using System.Linq;

namespace OpenCashFlow.Admin.Controllers
{
    [Authorize(Policy = "GIManagers")]
    public class CompaniesController(
        ILogger<CompaniesController> logger,
        CompanyAPIService companyAPIService,
        BillingPlansAPIService billingPlansService) : Controller
    {
        private readonly ILogger<CompaniesController> _logger = logger;
        private readonly CompanyAPIService _companyAPIService = companyAPIService;
        private readonly BillingPlansAPIService _billingPlansService = billingPlansService;

        public async Task<IActionResult> IndexAsync()
        {
            var result = await _companyAPIService.GetCompaniesAsync();
            return View(result.Data);
        }

        public async Task<IActionResult> ViewCompanyAsync(Guid TenantID)
        {
            var companyResponse = await _companyAPIService.GetCompanyAsync(TenantID);
            if (!companyResponse.Success || companyResponse.Data == null)
            {
                TempData["CompanyError"] = companyResponse.Message ?? "Impossibile recuperare i dettagli dell'azienda.";
                return RedirectToAction(nameof(IndexAsync));
            }

            var subscriptionResponse = await _billingPlansService.GetSubscriptionsAsync(new BillingSubscription_Filter_DTO
            {
                TenantID = TenantID,
                IncludeAllCompanies = true,
                Page = 1,
                PageSize = 25
            });

            var subscriptions = subscriptionResponse.Data?.ToList() ?? new List<BillingSubscription_List_DTO>();

            var activeSubscription = subscriptions
                .OrderByDescending(s => s.NextBillingDate)
                .FirstOrDefault();

            var viewModel = new CompanyDetailViewModel
            {
                Company = companyResponse.Data,
                Subscriptions = subscriptions,
                ActiveSubscription = activeSubscription,
                AppliedFilters = new BillingSubscriptionFilters
                {
                    TenantID = TenantID,
                    IncludeAllCompanies = true
                }
            };

            if (!subscriptionResponse.Success)
            {
                ViewData["BillingWarning"] = subscriptionResponse.Message;
            }

            return View(viewModel);
        }
    }   
}
