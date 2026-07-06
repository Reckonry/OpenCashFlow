using OpenCashFlow.WebApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;
using global::Shared.Models;

namespace OpenCashFlow.WebApp.Controllers
{
    [Authorize(Roles = "CompanyAdmin")]
    [Route("[Controller]")]
    public partial class CompanyController : Controller
    {
        private readonly ILogger<CompanyController> _logger;
        private readonly CompanyAPIService _companyAPIService;
        private readonly PaymentAPIService _paymentAPIService;
        private readonly IHttpClientFactory _httpClientFactory;

        public CompanyController(ILogger<CompanyController> logger, CompanyAPIService CompanyAPIService, IHttpClientFactory httpClientFactory, PaymentAPIService paymentAPIService)
        {
            _logger = logger;
            _companyAPIService = CompanyAPIService;
            _httpClientFactory = httpClientFactory;
            _paymentAPIService = paymentAPIService;
        }

        public async Task<IActionResult> IndexAsync()
        {
            ApiResponse<Company_Detail_DTO>? x = await _companyAPIService.GetCompanyAsync();
            return View(x.Data);
        }
    }
}
