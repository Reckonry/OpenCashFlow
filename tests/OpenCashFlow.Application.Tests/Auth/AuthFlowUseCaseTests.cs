using OpenCashFlow.Application.Auth.FastLogin;
using OpenCashFlow.Application.Auth.Login;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Application.Auth.Register;

namespace OpenCashFlow.Application.Tests.Auth;

public sealed class AuthFlowUseCaseTests
{
    private static readonly Guid UserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid TenantId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public async Task Login_WithValidPassword_ReturnsToken()
    {
        var reader = new FakeAuthUserReader();
        var password = new FakePasswordVerifier();
        var issuer = new FakeJwtTokenIssuer();
        var useCase = new LoginUseCase(reader, password, issuer);

        var result = await useCase.ExecuteAsync(new LoginCommand("admin@example.local", "Strong1!", true, 60));

        Assert.True(result.Success);
        Assert.Equal("token", result.Token);
        Assert.Equal(UserId, result.UserID);
        Assert.Equal(TenantId, result.TenantID);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Fails()
    {
        var reader = new FakeAuthUserReader();
        var password = new FakePasswordVerifier { VerifyResult = false };
        var useCase = new LoginUseCase(reader, password, new FakeJwtTokenIssuer());

        var result = await useCase.ExecuteAsync(new LoginCommand("admin@example.local", "wrong", true, 60));

        Assert.False(result.Success);
        Assert.Equal(AuthFailure.InvalidCredentials, result.ErrorType);
    }

    [Fact]
    public async Task FastLogin_WithValidCookieAndPin_ReturnsToken()
    {
        var reader = new FakeAuthUserReader();
        var cookie = new FakeFastLoginCookieService { Valid = true };
        var useCase = new FastLoginUseCase(reader, cookie, new FakeJwtTokenIssuer());

        var result = await useCase.ExecuteAsync(new FastLoginCommand("12345", "cookie", 60));

        Assert.True(result.Success);
        Assert.Equal("token", result.Token);
        Assert.Equal(TenantId, result.TenantID);
    }

    [Fact]
    public async Task Register_WithWeakPassword_Fails()
    {
        var password = new FakePasswordVerifier { StrongPassword = false };
        var useCase = new RegisterUseCase(new FakeAuthUserReader(), new FakeAuthUserWriter(), password, new FakeRegistrationNotificationSender());

        var result = await useCase.ExecuteAsync(new RegisterCommand("Company", "admin@example.local", "weak", "weak", true, "Admin", "User"));

        Assert.False(result.Success);
        Assert.Equal(RegistrationFailure.WeakPassword, result.ErrorType);
    }

    [Fact]
    public async Task Register_WithValidCommand_CreatesUserAndCompany()
    {
        var writer = new FakeAuthUserWriter();
        var useCase = new RegisterUseCase(new FakeAuthUserReader(), writer, new FakePasswordVerifier(), new FakeRegistrationNotificationSender());

        var result = await useCase.ExecuteAsync(new RegisterCommand("Company", "admin@example.local", "Strong1!", "Strong1!", true, "Admin", "User"));

        Assert.True(result.Success);
        Assert.True(writer.Created);
    }

    private sealed class FakeAuthUserReader : IAuthUserReader
    {
        public bool UserExists { get; set; }
        public bool CompanyExists { get; set; } = true;
        public AuthenticatedUserResult? User { get; set; } = new()
        {
            UserID = UserId,
            UserName = "admin",
            Email = "admin@example.local",
            UserFirstName = "Admin",
            PasswordSalt = "salt",
            PasswordHash = "hash",
            IsApproved = true,
            Roles = ["CompanyAdmin"]
        };

        public Task<bool> CompanyExistsAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(CompanyExists);

        public Task<string?> GetCompanySecretAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<string?>("secret");

        public Task<AuthenticatedUserResult?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(User);

        public Task<AuthenticatedUserResult?> GetByTenantAndPinAsync(Guid tenantId, string pin, CancellationToken cancellationToken = default)
            => Task.FromResult(pin == "12345" ? User : null);

        public Task<AuthenticatedUserResult?> GetByUsernameEmailOrPhoneAsync(string userInput, CancellationToken cancellationToken = default)
            => Task.FromResult(User);

        public Task<IReadOnlyList<AuthClaimResult>> GetUserClaimsAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<AuthClaimResult>>([]);

        public Task<Guid?> GetUserTenantIdAsync(Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult<Guid?>(TenantId);

        public Task<bool> UserExistsAsync(string usernameOrEmail, CancellationToken cancellationToken = default)
            => Task.FromResult(UserExists);
    }

    private sealed class FakeAuthUserWriter : IAuthUserWriter
    {
        public bool Created { get; private set; }

        public Task<bool> ConfirmAccountAsync(Guid tenantId, Guid userId, CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> CreateRegisteredCompanyAndUserAsync(RegistrationDraft draft, Guid tenantId, Guid userId, string passwordSalt, string passwordHash, CancellationToken cancellationToken = default)
        {
            Created = true;
            return Task.FromResult(true);
        }
    }

    private sealed class FakePasswordVerifier : IAuthPasswordVerifier
    {
        public bool VerifyResult { get; set; } = true;
        public bool StrongPassword { get; set; } = true;

        public string GenerateSalt() => "salt";
        public string HashPassword(string password, string salt) => "hash";
        public bool IsStrongPassword(string password) => StrongPassword;
        public bool VerifyPassword(string password, string salt, string expectedHash) => VerifyResult;
    }

    private sealed class FakeFastLoginCookieService : IFastLoginCookieService
    {
        public bool Valid { get; set; }

        public string CreateCookiePayload(Guid tenantId, string companySecret) => "cookie";
        public (Guid? TenantID, bool IsValid) ValidateCookiePayload(string payload) => (TenantId, Valid);
    }

    private sealed class FakeJwtTokenIssuer : IJwtTokenIssuer
    {
        public string IssueToken(AuthenticatedUserResult user, Guid tenantId, IReadOnlyList<AuthClaimResult> customClaims, int sessionMinutes) => "token";
    }

    private sealed class FakeRegistrationNotificationSender : IRegistrationNotificationSender
    {
        public Task SendFastLoginPinAsync(RegistrationFastLoginPinNotification notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task SendRegistrationConfirmationAsync(RegistrationConfirmationNotification notification, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
