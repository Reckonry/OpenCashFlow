using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs;
using global::Shared.Models;
using Asp.Versioning;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize]
    [Route("v{version:apiVersion}/")]
    [ApiVersion("1.0")]
    public partial class CompanyController(ICompanyService CompanyService, ILogger<CompanyController> logger) : Controller
    {
        private readonly ICompanyService _companyService = CompanyService;
        private readonly ILogger<CompanyController> _logger = logger;

        [HttpGet("[controller]")]
        public async Task<ActionResult<Company_Detail_DTO>> GetCompany(CancellationToken cancellationToken)
        {
            var company = await _companyService.GetCompanyAsync(cancellationToken);
            return Ok(company);
        }

        //todo: [Authorize(Policy = "InstanceAdmin")]
        [HttpGet("[controller]/View/{TenantID}")]
        public async Task<ActionResult<Company_Detail_DTO>> GetCompanyDetails(Guid TenantID, CancellationToken cancellationToken)
        {
            var company = await _companyService.GetCompanyAsync(cancellationToken);
            return Ok(company);
        }

        //todo: [Authorize(Policy = "InstanceAdmin")]
        [HttpGet("[controller]/All")]
        public async Task<ActionResult<IEnumerable<Company_Detail_DTO>>> GetAllCompanies(CancellationToken cancellationToken)
        {
            var company = await _companyService.GetAllCompaniesAsync(cancellationToken);
            return Ok(company);
        }
    }
}
