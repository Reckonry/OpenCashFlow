using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Contracts.Auth;
using OpenCashFlow.Contracts.Core;
using OpenCashFlow.Contracts.DTOs;
using OpenCashFlow.Test.Factories;

namespace OpenCashFlow.Test.Tests.API;

[Trait("Layer", "API")]
[Trait("Feature", "Setup")]
[Trait("Type", "Integration")]
public sealed class SetupApiTests : IDisposable
{
    private readonly CustomWebApplicationFactory _factory = new(
        $"SetupDb_{Guid.NewGuid():N}",
        useFakeAuth: false,
        seedTestData: false);

    [Fact]
    public async Task Status_WithFreshDatabase_RequiresSetup()
    {
        var client = _factory.CreateClient();

        var status = await client.GetFromJsonAsync<SetupStatus_DTO>("/v1/Setup/status");

        Assert.NotNull(status);
        Assert.True(status!.RequiresSetup);
        Assert.False(status.HasCompanies);
        Assert.False(status.HasAdminUsers);
    }

    [Fact]
    public async Task Create_WithFreshDatabase_CreatesFirstAdminAndTemporaryPassword()
    {
        var client = _factory.CreateClient();
        var request = ValidRequest();

        var response = await client.PostAsJsonAsync("/v1/Setup", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var completed = await response.Content.ReadFromJsonAsync<SetupCompleted_DTO>();
        Assert.NotNull(completed);
        Assert.Equal(request.AdminEmail, completed!.AdminEmail);
        Assert.True(IsStrongPassword(completed.TemporaryAdminPassword));
        Assert.False(completed.Status.RequiresSetup);

        using var db = _factory.CreateDbContext();
        var company = await db.Company_DS.AsNoTracking().SingleAsync();
        var admin = await db.AspNetUser_DS.AsNoTracking().SingleAsync();
        var cashBalance = await db.CashBalances.AsNoTracking().SingleAsync();

        Assert.Equal(request.CompanyName, company.CompanyName);
        Assert.Equal(request.AdminEmail, admin.Email);
        Assert.True(admin.UserMustChangePassword);
        Assert.Equal(company.TenantID, cashBalance.CompanyId);

        var secondResponse = await client.PostAsJsonAsync("/v1/Setup", request);
        Assert.Equal(HttpStatusCode.Conflict, secondResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/v1/Authentication/login", new
        {
            Username = request.AdminEmail,
            Password = completed.TemporaryAdminPassword
        });

        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);
        var login = await loginResponse.Content.ReadFromJsonAsync<ApiResponse<AuthResult>>();
        Assert.NotNull(login?.Data);
        Assert.True(login!.Data!.Success);
        Assert.True(login.Data.RequiresPasswordChange);
        Assert.False(string.IsNullOrWhiteSpace(login.Data.Token));
    }

    [Fact]
    public async Task Create_WithInvalidData_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var request = ValidRequest();
        request.CompanyName = string.Empty;

        var response = await client.PostAsJsonAsync("/v1/Setup", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    public void Dispose()
    {
        _factory.Dispose();
    }

    private static SetupRequest_DTO ValidRequest()
    {
        return new SetupRequest_DTO
        {
            CompanyName = "OpenCashFlow Fresh Install",
            AdminEmail = $"owner-{Guid.NewGuid():N}@example.local",
            AdminFirstName = "Owner",
            AdminLastName = "Admin",
            Language = "it",
            Currency = "EUR",
            Timezone = "Europe/Rome",
            Country = "IT"
        };
    }

    private static bool IsStrongPassword(string password)
    {
        return password.Length >= 16
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
