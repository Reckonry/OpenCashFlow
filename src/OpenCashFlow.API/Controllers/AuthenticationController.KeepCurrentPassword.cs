using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        [Authorize]
        [HttpPost("keep-current-password")]
        public async Task<IActionResult> KeepCurrentPassword(CancellationToken cancellationToken)
        {
            var userId = _authenticationService.GetUserID();

            // Simply remove the password change requirement flag without changing the password
            await _authenticationService.RemovePasswordChangeRequirementAsync(userId, cancellationToken);

            return Ok(new ApiResponse<object>(true, "Password requirement removed"));
        }
    }
}
