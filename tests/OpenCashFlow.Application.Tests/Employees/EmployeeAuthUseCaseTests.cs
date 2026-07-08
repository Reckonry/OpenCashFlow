using OpenCashFlow.Application.Auth.ForgotPassword;
using OpenCashFlow.Application.Auth.Models;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Application.Auth.ResetPassword;
using OpenCashFlow.Application.Companies.Models;
using OpenCashFlow.Application.Companies.Ports;
using OpenCashFlow.Application.Employees.CreateEmployee;
using OpenCashFlow.Application.Employees.Models;
using OpenCashFlow.Application.Employees.Ports;
using OpenCashFlow.Application.Employees.ResendPin;

namespace OpenCashFlow.Application.Tests.Employees;

public sealed class EmployeeAuthUseCaseTests
{
    [Fact]
    public async Task CreateEmployee_WithEmptyEmail_Throws()
    {
        var useCase = new CreateEmployeeUseCase(new FakeEmployeeReader(), new FakeEmployeeWriter(), new FakeCompanyReader(), new FakeCredentialService(), new FakePinService(), new FakeEmployeeNotificationSender());

        await Assert.ThrowsAsync<ArgumentException>(() => useCase.ExecuteAsync(new CreateEmployeeCommand
        {
            TenantID = Guid.NewGuid(),
            CurrentUserID = Guid.NewGuid(),
            Email = "",
            UserFirstName = "Ada",
            NewPassword = "Password1!"
        }));
    }

    [Fact]
    public async Task ResendPin_WithMissingEmployee_ReturnsFalse()
    {
        var useCase = new ResendEmployeePinUseCase(new FakeEmployeeReader(), new FakeEmployeeWriter(), new FakeCredentialService(), new FakePinService(), new FakeEmployeeNotificationSender());

        var result = await useCase.ExecuteAsync(new ResendEmployeePinCommand(Guid.NewGuid(), Guid.NewGuid()));

        Assert.False(result);
    }

    [Fact]
    public async Task ForgotPassword_WithExistingUser_CreatesToken()
    {
        var tokenStore = new FakePasswordResetTokenStore();
        var useCase = new ForgotPasswordUseCase(new FakeUserCredentialReader(), tokenStore, new FakePasswordResetTokenGenerator(), new FakePasswordResetNotificationSender());

        var result = await useCase.ExecuteAsync(new ForgotPasswordCommand("ada@example.local", null, "http://localhost", 30));

        Assert.True(result.TokenCreated);
        Assert.True(tokenStore.Created);
    }

    [Fact]
    public async Task ResetPassword_WithValidToken_UpdatesPasswordAndInvalidatesToken()
    {
        var tokenStore = new FakePasswordResetTokenStore { Valid = true };
        var passwordWriter = new FakeUserPasswordWriter();
        var useCase = new ResetPasswordUseCase(new FakeUserCredentialReader(), passwordWriter, tokenStore, new FakePasswordResetTokenGenerator(), new FakeCredentialService());

        var result = await useCase.ExecuteAsync(new ResetPasswordCommand(FakeUserCredentialReader.UserId, "token", "NewPassword1!"));

        Assert.True(result.Success);
        Assert.True(passwordWriter.Updated);
        Assert.True(tokenStore.Invalidated);
    }

    private sealed class FakeEmployeeReader : IEmployeeReader
    {
        public Task<IReadOnlyList<EmployeeListItem>> GetEmployeesAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<IReadOnlyList<EmployeeListItem>>([]);
        public Task<EmployeeDetailResult?> GetEmployeeByIdAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<EmployeeDetailResult?>(null);
        public Task<EmployeeCredentialSnapshot?> GetEmployeeCredentialAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult<EmployeeCredentialSnapshot?>(null);
        public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default) => Task.FromResult(false);
        public Task<bool> EmailInUseByOtherUserAsync(string email, Guid currentUserId, CancellationToken cancellationToken = default) => Task.FromResult(false);
    }

    private sealed class FakeEmployeeWriter : IEmployeeWriter
    {
        public Task<EmployeeDetailResult?> CreateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<EmployeeDetailResult?>(new EmployeeDetailResult
            {
                TenantID = employee.TenantID,
                UserID = employee.UserID,
                UserName = employee.UserName,
                UserFirstName = employee.UserFirstName,
                Email = employee.Email
            });
        }

        public Task<bool> UpdateAsync(EmployeeWriteDraft employee, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task<bool> SoftDeleteAsync(Guid userId, Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult(true);
        public Task UpdateMyProfileAsync(Guid userId, EmployeeProfileUpdate model, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task UpdatePinHashAsync(Guid userId, string pinHash, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeCompanyReader : ICompanyReader
    {
        public Task<CompanyResult?> GetByIdAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<CompanyResult?>(new CompanyResult { TenantID = tenantId, CompanyName = "Test", MaxUsers = 50 });

        public Task<IReadOnlyList<CompanyResult>> GetAllAsync(CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CompanyResult>>([]);

        public Task<IReadOnlyList<CompanyResult>> GetAllAsync(CompanyListQuery query, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CompanyResult>>([]);

        public Task<bool> ExistsByNameAsync(string companyName, Guid? excludingTenantId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> ExistsByTinAsync(string tin, Guid? excludingTenantId = null, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> HasActiveRelationsAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<(long MaxUsers, int ActiveUsers)?> GetUserLimitAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<(long MaxUsers, int ActiveUsers)?>((50, 1));

        public Task<IReadOnlyList<CompanyInvoiceListItem>> GetInvoicesAsync(Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<IReadOnlyList<CompanyInvoiceListItem>>([]);

        public Task<CompanyInvoiceDetailResult?> GetInvoiceByIdAsync(Guid invoiceId, Guid tenantId, CancellationToken cancellationToken = default)
            => Task.FromResult<CompanyInvoiceDetailResult?>(null);
    }

    private sealed class FakeCredentialService : IEmployeeCredentialService
    {
        public string GenerateSalt() => "salt";
        public string HashSecret(string value, string salt) => $"hash:{value}:{salt}";
        public bool IsStrongPassword(string password) => !string.IsNullOrWhiteSpace(password) && password.Length >= 8;
    }

    private sealed class FakePinService : IEmployeePinService
    {
        public Task<string> GenerateUniquePinAsync(Guid tenantId, CancellationToken cancellationToken = default) => Task.FromResult("12345");
    }

    private sealed class FakeEmployeeNotificationSender : IEmployeeNotificationSender
    {
        public Task SendEmployeeCreatedPinAsync(EmployeePinNotification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SendEmployeePinChangedAsync(EmployeePinNotification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task SendEmployeeEmailChangedAsync(EmployeeEmailChangedNotification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakeUserCredentialReader : IUserCredentialReader
    {
        public static readonly Guid UserId = Guid.NewGuid();

        public Task<AuthUserCredential?> GetByEmailOrUserNameAsync(string userInput, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AuthUserCredential?>(new AuthUserCredential(UserId, "ada", "ada@example.local", "Ada", "salt", true, false));
        }

        public Task<AuthUserCredential?> GetByIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<AuthUserCredential?>(new AuthUserCredential(userId, "ada", "ada@example.local", "Ada", "salt", true, false));
        }
    }

    private sealed class FakePasswordResetTokenStore : IPasswordResetTokenStore
    {
        public bool Created { get; private set; }
        public bool Invalidated { get; private set; }
        public bool Valid { get; init; }

        public Task CreateAsync(Guid userId, string token, DateTime expiresAt, CancellationToken cancellationToken = default)
        {
            Created = true;
            return Task.CompletedTask;
        }

        public Task<bool> HasValidTokenAsync(Guid userId, string token, CancellationToken cancellationToken = default) => Task.FromResult(Valid);
        public Task<Guid?> GetUserIdFromTokenAsync(string token, CancellationToken cancellationToken = default) => Task.FromResult<Guid?>(FakeUserCredentialReader.UserId);
        public Task<PasswordResetTokenValidation> ValidateAsync(string token, CancellationToken cancellationToken = default) => Task.FromResult(new PasswordResetTokenValidation(Valid, false, FakeUserCredentialReader.UserId, "ada", "ada@example.local", "Ada"));

        public Task InvalidateAsync(Guid userId, string token, CancellationToken cancellationToken = default)
        {
            Invalidated = true;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUserPasswordWriter : IUserPasswordWriter
    {
        public bool Updated { get; private set; }
        public Task UpdatePasswordHashAsync(Guid userId, string passwordHash, CancellationToken cancellationToken = default)
        {
            Updated = true;
            return Task.CompletedTask;
        }

        public Task RemovePasswordChangeRequirementAsync(Guid userId, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class FakePasswordResetTokenGenerator : IPasswordResetTokenGenerator
    {
        public string GenerateToken() => "token";
        public string EncodeToken(string token) => token;
        public string DecodeTokenOrPassthrough(string token) => token;
    }

    private sealed class FakePasswordResetNotificationSender : IPasswordResetNotificationSender
    {
        public Task SendPasswordResetAsync(PasswordResetNotification notification, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
