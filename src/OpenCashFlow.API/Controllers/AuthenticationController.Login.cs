using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using global::Shared.Models;
using global::Shared.Models.Core;

namespace OpenCashFlow.API.Controllers
{
    public partial class AuthenticationController : ControllerBase
    {
        [HttpPost("login")]
        [EnableRateLimiting("auth-limiter")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            if (request == null)
                return BadRequest(new ApiResponse<object>(false, "Invalid login request."));

            AuthResult authResult = await _authenticationService.Authenticate(request.Username, request.Password, cancellationToken);
            if (authResult.Success == false)
                return Unauthorized(new ApiResponse<AuthResult>(authResult.Success, null!, authResult));

            var FastLoginCookieValue = await _authenticationService.GenerateFastLoginCookieValueAsync(request.Username, request.Password, cancellationToken);
            if (FastLoginCookieValue == null)
                return Unauthorized(new ApiResponse<AuthResult>(false, null!, FastLoginCookieValue));

            // Fix: Correctly initialize the ApiResponse with a List<string> containing the required values
            var response = new ApiResponse<AuthResult>(true, null,
                new AuthResult() { Success = true, FastLoginToken = FastLoginCookieValue.FastLoginToken, Token = authResult.Token, RequiresPasswordChange = authResult.RequiresPasswordChange });

            return Ok(response);
        }

        // FAST LOGIN (solo PIN)
        [HttpPost("fastlogin")]
        [EnableRateLimiting("auth-limiter")]
        public async Task<IActionResult> FastLogin([FromBody] FastLoginRequest request, CancellationToken cancellationToken)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.Pin)) return BadRequest("Invalid fast login request.");
            var result = await _authenticationService.AuthenticateFastAsync(HttpContext, request.Pin, request.FLCookieValue, cancellationToken);

            if (!result.Success)
                return Unauthorized(new { message = result.Message });

            return Ok(new { Token = result.Data });
        }
    }
}
