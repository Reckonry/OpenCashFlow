using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Contracts.DTOs;
using System.Threading;
using Asp.Versioning;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [IgnoreAntiforgeryToken]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public partial class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationController(IAuthenticationService authenticationService, ILogger<AuthenticationController> logger, IConfiguration configuration)
        {
            _authenticationService = authenticationService;
            _logger = logger;
            _configuration = configuration;
        }
    }


    public class FastLoginRequest
    {
        public required string Pin { get; set; }
        public required string FLCookieValue { get; set; }
    }

    public class LoginRequest
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class ResetPasswordRequest
    {
        public required string Token { get; set; }
        public required string NewPassword { get; set; }
    }

    public class ChangePasswordRequiredRequest
    {
        public required string NewPassword { get; set; }
    }

    // Model for the reset password request
    // We do not use Microsoft.AspNetCore.Identity.Data.ForgotPasswordRequest to avoid binding conflicts
    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }
}
