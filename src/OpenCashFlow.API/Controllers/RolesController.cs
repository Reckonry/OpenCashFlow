using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using global::Shared.DTOs.Identity;
using Asp.Versioning;

namespace OpenCashFlow.API.Controllers
{
    [ApiController, Authorize]
    [Route("v{version:apiVersion}/")]
    [ApiVersion("1.0")]
    public class RolesController(IRoleService roleService) : Controller
    {
        private readonly IRoleService _roleService = roleService;

        [HttpGet("[controller]/Visible")]
        public async Task<ActionResult<IEnumerable<Role_List_DTO>>> GetVisibleRoles(CancellationToken cancellationToken)
        {
            var roles = await _roleService.GetVisibleRolesAsync(cancellationToken);
            return Ok(roles);
        }
    }
}

