using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        [Authorize]
        [HttpPost("change-password-required")]
        public async Task<IActionResult> ChangePasswordRequired([FromBody] ChangePasswordRequiredRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.NewPassword))
                return BadRequest(new ApiResponse<object>(false, "Invalid request"));

            var userId = _authenticationService.GetUserID();
            await _authenticationService.ChangeRequiredPasswordAsync(userId, request.NewPassword, cancellationToken);

            return Ok(new ApiResponse<object>(true, "Password updated"));
        }
    }
}
