using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Contracts.DTOs;
using Asp.Versioning;
using System.Security.Claims;
using OpenCashFlow.Application.Companies.Models;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize(Policy = "CompanyMember")]
    [IgnoreAntiforgeryToken]
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
        public async Task<ActionResult<IEnumerable<Company_Detail_DTO>>> GetAllCompanies(
            [FromQuery] bool? isActive,
            [FromQuery] string? name,
            [FromQuery] string? tin,
            [FromQuery] decimal? revenueFrom,
            [FromQuery] decimal? revenueTo,
            CancellationToken cancellationToken)
        {
            var company = await _companyService.GetAllCompaniesAsync(
                new CompanyListQuery(isActive, name, tin, revenueFrom, revenueTo),
                cancellationToken);
            return Ok(company);
        }

        [Authorize(Roles = "CompanyAdmin,InstanceAdmin")]
        [HttpPost("[controller]")]
        public async Task<IActionResult> CreateCompany([FromBody] Company_Detail_DTO model, CancellationToken cancellationToken)
        {
            var result = await _companyService.CreateCompanyAsync(model, cancellationToken);
            return ToActionResult(result, created: true);
        }

        [Authorize(Roles = "CompanyAdmin,InstanceAdmin")]
        [HttpPut("[controller]/{TenantID:guid}")]
        public async Task<IActionResult> UpdateCompany(Guid TenantID, [FromBody] Company_Detail_DTO model, CancellationToken cancellationToken)
        {
            if (!CanAccessTenant(TenantID))
            {
                return Forbid();
            }

            var result = await _companyService.UpdateCompanyAsync(TenantID, model, cancellationToken);
            return ToActionResult(result, created: false);
        }

        [Authorize(Roles = "CompanyAdmin,InstanceAdmin")]
        [HttpDelete("[controller]/{TenantID:guid}")]
        public async Task<IActionResult> DeleteCompany(Guid TenantID, CancellationToken cancellationToken)
        {
            if (!CanAccessTenant(TenantID))
            {
                return Forbid();
            }

            var result = await _companyService.DeleteCompanyAsync(TenantID, cancellationToken);
            return result.Status switch
            {
                CompanyWriteStatus.Success => NoContent(),
                CompanyWriteStatus.NotFound => NotFound(new { error = result.Message }),
                CompanyWriteStatus.AlreadyDeleted => Conflict(new { error = result.Message }),
                CompanyWriteStatus.HasActiveRelations => Conflict(new { error = result.Message }),
                CompanyWriteStatus.ValidationFailed => BadRequest(new { error = result.Message }),
                _ => BadRequest(new { error = result.Message })
            };
        }

        private IActionResult ToActionResult(CompanyWriteResult result, bool created)
        {
            return result.Status switch
            {
                CompanyWriteStatus.Success when created && result.Company is not null =>
                    CreatedAtAction(nameof(GetCompanyDetails), new { TenantID = result.Company.TenantID }, result.Company),
                CompanyWriteStatus.Success when result.Company is not null => Ok(result.Company),
                CompanyWriteStatus.NotFound => NotFound(new { error = result.Message }),
                CompanyWriteStatus.ValidationFailed => BadRequest(new { error = result.Message }),
                CompanyWriteStatus.DuplicateCompanyName => Conflict(new { error = result.Message }),
                CompanyWriteStatus.DuplicateTin => Conflict(new { error = result.Message }),
                _ => BadRequest(new { error = result.Message })
            };
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
