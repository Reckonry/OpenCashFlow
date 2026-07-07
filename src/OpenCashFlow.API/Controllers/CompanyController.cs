using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Contracts.DTOs;
using Asp.Versioning;
using System.Security.Claims;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize(Policy = "CompanyMember")]
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
            if (!CanAccessTenant(TenantID))
            {
                return Forbid();
            }

            var company = await _companyService.GetCompanyAsync(TenantID, cancellationToken);
            if (company == null)
            {
                return NotFound();
            }

            return Ok(company);
        }

        //todo: [Authorize(Policy = "InstanceAdmin")]
        [HttpGet("[controller]/All")]
        public async Task<ActionResult<IEnumerable<Company_Detail_DTO>>> GetAllCompanies(CancellationToken cancellationToken)
        {
            var company = await _companyService.GetAllCompaniesAsync(cancellationToken);
            return Ok(company);
        }

        [Authorize(Policy = "CompanyAdmin")]
        [HttpPost("[controller]")]
        public IActionResult CreateCompany()
        {
            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        [Authorize(Policy = "CompanyAdmin")]
        [HttpPut("[controller]/{TenantID:guid}")]
        public IActionResult UpdateCompany(Guid TenantID)
        {
            if (!CanAccessTenant(TenantID))
            {
                return Forbid();
            }

            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        [Authorize(Policy = "CompanyAdmin")]
        [HttpDelete("[controller]/{TenantID:guid}")]
        public IActionResult DeleteCompany(Guid TenantID)
        {
            if (!CanAccessTenant(TenantID))
            {
                return Forbid();
            }

            return StatusCode(StatusCodes.Status501NotImplemented);
        }

        private bool CanAccessTenant(Guid tenantId)
        {
            if (User.IsInRole("InstanceAdmin"))
            {
                return true;
            }

            var tenantClaim = User.FindFirst("TenantID")?.Value;
            return Guid.TryParse(tenantClaim, out var currentTenantId) && currentTenantId == tenantId;
        }
    }
}
