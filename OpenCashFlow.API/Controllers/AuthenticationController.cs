using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.Core;
using System.Threading;
using OpenCashFlow.API.Repositories.Interfaces;

namespace OpenCashFlow.API.Controllers
{
    [ApiController]
    [Route("v{version:apiVersion}/[controller]")]
    [ApiVersion("1.0")]
    [ApiVersion("2.0")]
    public partial class AuthenticationController : ControllerBase
    {
        private readonly IAuthenticationService _authenticationService;
        private readonly IEmployeeRepository _employeeRepository;
        private readonly ILogger<AuthenticationController> _logger;
        private readonly IConfiguration _configuration;

        public AuthenticationController(IAuthenticationService authenticationService, IEmployeeRepository employeeRepository, ILogger<AuthenticationController> logger, IConfiguration configuration)
        {
            _authenticationService = authenticationService;
            _employeeRepository = employeeRepository;
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

    // Modello per la richiesta di reset password
    // Non usiamo Microsoft.AspNetCore.Identity.Data.ForgotPasswordRequest per evitare conflitti di binding
    public class ForgotPasswordRequest
    {
        public required string Email { get; set; }
    }
}
