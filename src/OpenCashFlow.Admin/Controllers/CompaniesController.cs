using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using OpenCashFlow.Admin.Models;
using OpenCashFlow.Admin.Models.Billing;
using OpenCashFlow.Admin.Models.Companies;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Admin.Services;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Contracts.DTOs.Billing;
using System.Collections.Generic;
using System.Linq;

namespace OpenCashFlow.Admin.Controllers
{
    [Authorize(Policy = "InstanceAdmin")]
    public class CompaniesController(
        ILogger<CompaniesController> logger,
        CompanyAPIService companyAPIService,
        BillingPlansAPIService billingPlansService,
        IConfiguration configuration) : Controller
    {
        private readonly ILogger<CompaniesController> _logger = logger;
        private readonly CompanyAPIService _companyAPIService = companyAPIService;
        private readonly BillingPlansAPIService _billingPlansService = billingPlansService;
        private readonly IConfiguration _configuration = configuration;

        private bool BillingEnabled => string.Equals(_configuration["Features:Billing"], "true", StringComparison.OrdinalIgnoreCase);

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
                TempData["CompanyError"] = companyResponse.Message ?? "Unable to retrieve company details.";
                return RedirectToAction(nameof(IndexAsync));
            }

            ApiResponse<IReadOnlyList<BillingSubscription_List_DTO>>? subscriptionResponse = null;
            var subscriptions = new List<BillingSubscription_List_DTO>();

            if (BillingEnabled)
            {
                subscriptionResponse = await _billingPlansService.GetSubscriptionsAsync(new BillingSubscription_Filter_DTO
                {
                    TenantID = TenantID,
                    IncludeAllCompanies = true,
                    Page = 1,
                    PageSize = 25
                });

                subscriptions = subscriptionResponse.Data?.ToList() ?? new List<BillingSubscription_List_DTO>();
            }

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

            if (BillingEnabled && subscriptionResponse?.Success == false)
            {
                ViewData["BillingWarning"] = subscriptionResponse.Message;
            }

            return View(viewModel);
        }
    }   
}
