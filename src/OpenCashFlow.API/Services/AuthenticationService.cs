using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Auth.Audit;
using OpenCashFlow.Application.Auth.AccountConfirmation;
using OpenCashFlow.Application.Auth.FastLogin;
using OpenCashFlow.Application.Auth.ForgotPassword;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Application.Auth.Login;
using OpenCashFlow.Application.Auth.Register;
using OpenCashFlow.Application.Auth.ResetPassword;
using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Contracts.Audit;

namespace OpenCashFlow.API.Services
{
    public partial class AuthenticationService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor,
        ILogger<AuthenticationService> logger,
        IForgotPasswordUseCase forgotPasswordUseCase,
        IResetPasswordUseCase resetPasswordUseCase,
        IValidateResetTokenUseCase validateResetTokenUseCase,
        IPasswordResetTokenGenerator passwordResetTokenGenerator,
        IPasswordResetTokenStore passwordResetTokenStore,
        IUserCredentialReader userCredentialReader,
        IUserPasswordWriter userPasswordWriter,
        IEmployeeCredentialService employeeCredentialService,
        ILoginUseCase loginUseCase,
        IFastLoginUseCase fastLoginUseCase,
        IGenerateFastLoginCookieUseCase generateFastLoginCookieUseCase,
        IRegisterUseCase registerUseCase,
        IConfirmAccountUseCase confirmAccountUseCase,
        IResendConfirmationUseCase resendConfirmationUseCase,
        IAuthUserReader authUserReader,
        IJwtTokenIssuer jwtTokenIssuer,
        IAuthAuditWriter authAuditWriter) : IAuthenticationService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
        private readonly ILogger<AuthenticationService> _logger = logger;
        private readonly IForgotPasswordUseCase _forgotPasswordUseCase = forgotPasswordUseCase;
        private readonly IResetPasswordUseCase _resetPasswordUseCase = resetPasswordUseCase;
        private readonly IValidateResetTokenUseCase _validateResetTokenUseCase = validateResetTokenUseCase;
        private readonly IPasswordResetTokenGenerator _passwordResetTokenGenerator = passwordResetTokenGenerator;
        private readonly IPasswordResetTokenStore _passwordResetTokenStore = passwordResetTokenStore;
        private readonly IUserCredentialReader _userCredentialReader = userCredentialReader;
        private readonly IUserPasswordWriter _userPasswordWriter = userPasswordWriter;
        private readonly IEmployeeCredentialService _employeeCredentialService = employeeCredentialService;
        private readonly ILoginUseCase _loginUseCase = loginUseCase;
        private readonly IFastLoginUseCase _fastLoginUseCase = fastLoginUseCase;
        private readonly IGenerateFastLoginCookieUseCase _generateFastLoginCookieUseCase = generateFastLoginCookieUseCase;
        private readonly IRegisterUseCase _registerUseCase = registerUseCase;
        private readonly IConfirmAccountUseCase _confirmAccountUseCase = confirmAccountUseCase;
        private readonly IResendConfirmationUseCase _resendConfirmationUseCase = resendConfirmationUseCase;
        private readonly IAuthUserReader _authUserReader = authUserReader;
        private readonly IJwtTokenIssuer _jwtTokenIssuer = jwtTokenIssuer;
        private readonly IAuthAuditWriter _authAuditWriter = authAuditWriter;

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

        private async Task WriteAuthenticationAuditAsync(
            AuditEventType eventType,
            string action,
            string? username,
            Guid? userId,
            Guid? tenantId,
            string? additionalInfo,
            CancellationToken cancellationToken)
        {
            try
            {
                var httpContext = _httpContextAccessor.HttpContext;
                await _authAuditWriter.WriteAsync(new AuthAuditEvent(
                    EventType: eventType.ToString(),
                    Action: action,
                    Resource: "Authentication",
                    UserId: userId,
                    TenantId: tenantId,
                    Username: username,
                    IpAddress: httpContext?.Connection.RemoteIpAddress?.ToString(),
                    UserAgent: httpContext?.Request.Headers["User-Agent"].ToString(),
                    Severity: eventType == AuditEventType.Login ? "Info" : "Warning",
                    AdditionalInfo: additionalInfo,
                    TimestampUtc: DateTime.UtcNow),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to write authentication audit event {Action}", action);
            }
        }

    }

}
