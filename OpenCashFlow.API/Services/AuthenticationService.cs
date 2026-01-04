using OpenCashFlow.API.Repositories;
using OpenCashFlow.API.Repositories.Interfaces;
using OpenCashFlow.API.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using global::Shared.Core;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.Core;
using global::Shared.Models.Identity;
using global::Shared.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService(IConfiguration configuration, IAuthenticationRepository authenticationRepository,
        ICompanyRepository companyRepository, IEmployeeRepository employeeRepository, IHttpContextAccessor httpContextAccessor,
        IEmailSender emailSender, ISlackNotifier slack, IWebHostEnvironment env, ApplicationDbContext context,
        IEmailTemplateService emailTemplateService, ILogger<AuthenticationService> logger) : IAuthenticationService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IAuthenticationRepository _authenticationRepository = authenticationRepository;
        private readonly ICompanyRepository _companyRepository = companyRepository;
        private readonly IEmployeeRepository _employeeRepository = employeeRepository;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ApplicationDbContext _context = context;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly ISlackNotifier _slack = slack;
        private readonly IWebHostEnvironment _env = env;
        private readonly IEmailTemplateService _emailTemplateService = emailTemplateService;
        private readonly ILogger<AuthenticationService> _logger = logger;

        public Guid GetUserID()
        {
            _ = Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst("UserID")?.Value, out Guid UserID);
            if (UserID == Guid.Empty) throw new UnauthorizedAccessException("UserNotFound");
            else return UserID;
        }

        public Guid GetTenantID()
        {
            _ = Guid.TryParse(_httpContextAccessor.HttpContext?.User?.FindFirst("TenantID")?.Value, out Guid TenantID);
            if (TenantID == Guid.Empty) throw new UnauthorizedAccessException("CompanyNotFound");
            else return TenantID;
        }

    }

}
