using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        public class ResendConfirmationRequest
        {
            public required string Username { get; set; }
        }

        [HttpPost("resend-confirmation")]
        [EnableRateLimiting("auth-limiter")]
        [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResendConfirmation([FromBody] ResendConfirmationRequest request, CancellationToken cancellationToken)
        {
            // Always respond 200 to avoid user enumeration
            await _authenticationService.ResendConfirmationAsync(request.Username, cancellationToken);
            return Ok(new { message = "If an account exists, a confirmation email has been sent." });
        }
    }
}
