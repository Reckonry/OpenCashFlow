using Microsoft.AspNetCore.Mvc;
using OpenCashFlow.Contracts.DTOs;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        [HttpPost("register")]
        public async Task<IActionResult> Register(Register_DTO registration, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.RegistrationAsync(registration, cancellationToken);
            if (!result.Success) return BadRequest(result);
            return Ok(result);
        }

        [HttpGet("confirm_account/{TenantID}/{UserID}")]
        public async Task<IActionResult> ConfirmAccount(Guid TenantID, Guid UserID, CancellationToken cancellationToken)
        {
            var result = await _authenticationService.ConfirmAccountAsync(TenantID, UserID, cancellationToken);
            if (!result) return BadRequest(result);

            return Ok(result);
        }
    }
}
