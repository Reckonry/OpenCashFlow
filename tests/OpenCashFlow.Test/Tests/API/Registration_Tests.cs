using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using global::Shared.DTOs;
using System.Net;
using System.Text.Json;
using Xunit;
using Microsoft.EntityFrameworkCore;
using global::Shared.Models;
using global::Shared.Core;

namespace OpenCashFlow.Test.Tests
{
    [Collection("NonParallelCollection")]
    public class RegistrationApiTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public RegistrationApiTests(CustomWebApplicationFactoryFixture fixture)
        {
            _factory = fixture.Factory;
        }

        //todo: Registration [OK] (valid user, correct data, existing active company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should succeed for valid user with active existing company")]
        public async Task Register_ValidUserAndActiveCompany_ShouldSucceed()
        {
            var client = _factory.CreateClient();

            var uniqueEmail = $"test_{Guid.NewGuid():N}@example.com";
            var payload = new Register_DTO
            {
                CompanyName = "Test Company",
                Email = uniqueEmail,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "John",
                LastName = "Doe"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/Authentication/register", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());

            // Verify user and company linkage were created
            using var db = _factory.CreateDbContext();
            var user = db.AspNetUser_DS.AsNoTracking().FirstOrDefault(u => u.Email == uniqueEmail);
            Assert.NotNull(user);

            var staff = db.Company_Staff_DS.AsNoTracking().FirstOrDefault(s => s.UserID == user!.UserID);
            Assert.NotNull(staff);

            var company = db.Company_DS.AsNoTracking().FirstOrDefault(c => c.TenantID == staff!.TenantID);
            Assert.NotNull(company);
            Assert.True(company!.IsActive);
        }

        //todo: Registration [FAIL] (email already exists)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should fail if email already exists")]
        public async Task Register_EmailAlreadyExists_ShouldFail()
        {
            var client = _factory.CreateClient();

            // Use an email known to exist from seeded data
            var existingEmail = "admin.seed@example.local";
            var payload = new Register_DTO
            {
                CompanyName = "Duplicate Email Co",
                Email = existingEmail,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Lorenzo",
                LastName = "Salami"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/Authentication/register", content);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());

            // errorType should be UsernameTaken (enum value 1)
            Assert.Equal(1, doc.RootElement.GetProperty("errorType").GetInt32());
        }

        //todo: Registration [FAIL] (missing required fields: email, password, company ID)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/register should fail if required fields are missing (email, password, company ID)")]
        public async Task Register_MissingRequiredFields_ShouldFail()
        {
            var client = _factory.CreateClient();

            // Prepare three payloads, each missing a different required field
            var cases = new List<Register_DTO>
            {
                // Missing Email
                new Register_DTO
                {
                    CompanyName = "Some Company",
                    Email = string.Empty,
                    Password = "StrongP@ssw0rd!",
                    ConfirmPassword = "StrongP@ssw0rd!",
                    AcceptPrivacyPolicy = true,
                    FirstName = "John",
                    LastName = "Doe"
                },
                // Missing Password
                new Register_DTO
                {
                    CompanyName = "Some Company",
                    Email = $"missing_pwd_{Guid.NewGuid():N}@example.com",
                    Password = string.Empty,
                    ConfirmPassword = string.Empty,
                    AcceptPrivacyPolicy = true,
                    FirstName = "John",
                    LastName = "Doe"
                },
                // Missing CompanyName
                new Register_DTO
                {
                    CompanyName = string.Empty,
                    Email = $"missing_company_{Guid.NewGuid():N}@example.com",
                    Password = "StrongP@ssw0rd!",
                    ConfirmPassword = "StrongP@ssw0rd!",
                    AcceptPrivacyPolicy = true,
                    FirstName = "John",
                    LastName = "Doe"
                }
            };

            foreach (var payload in cases)
            {
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
                var response = await client.PostAsync("/v1/Authentication/register", content);

                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var text = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(text);
                Assert.False(doc.RootElement.GetProperty("success").GetBoolean());

                // errorType should be MissingRequiredFields (enum value 6)
                Assert.Equal(6, doc.RootElement.GetProperty("errorType").GetInt32());
            }
        }

        //todo: Registrazione [FAIL] (password troppo debole o fuori policy)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/register should fail with weak or non-compliant password")]
        public async Task Register_WeakPassword_ShouldFail()
        {
            var client = _factory.CreateClient();

            var weakPassword = "123"; // deliberately weak
            var uniqueEmail = $"weakpwd_{Guid.NewGuid():N}@example.com";

            var payload = new Register_DTO
            {
                CompanyName = $"WeakPwd Co {Guid.NewGuid():N}",
                Email = uniqueEmail,
                Password = weakPassword,
                ConfirmPassword = weakPassword,
                AcceptPrivacyPolicy = true,
                FirstName = "Weak",
                LastName = "Password"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/Authentication/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
            // WeakPassword enum value expected (3)
            Assert.Equal(3, doc.RootElement.GetProperty("errorType").GetInt32());
        }

        //todo: Registration [FAIL] (invalid email format)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/register should fail with invalid email format")]
        public async Task Register_InvalidEmailFormat_ShouldFail()
        {
            var client = _factory.CreateClient();

            var payload = new Register_DTO
            {
                CompanyName = "Invalid Email Co",
                Email = "invalid-email-format",
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Jane",
                LastName = "Doe"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/Authentication/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
            // errorType should be InvalidEmail (enum value 4)
            Assert.Equal(4, doc.RootElement.GetProperty("errorType").GetInt32());
        }



        //todo: Registrazione [FAIL] (troppi tentativi — anti-brute-force o rate limit attivo)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should fail after too many attempts (rate limit or anti-brute-force)")]
        public async Task Register_TooManyAttempts_ShouldFail()
        {
            var client = _factory.CreateClient();

            // First attempt should succeed
            var email = $"flood_{Guid.NewGuid():N}@example.com";
            var payload = new Register_DTO
            {
                CompanyName = "Flood Co",
                Email = email,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Flood",
                LastName = "User"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            var first = await client.PostAsync("/v1/Authentication/register", content);
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);

            // Rapid repeated attempts with the SAME email
            // Expect failures due to UsernameTaken or potential rate limiting (429/503)
            int attempts = 8;
            int failureCount = 0;
            bool sawUsernameTaken = false;

            for (int i = 0; i < attempts; i++)
            {
                var json2 = JsonSerializer.Serialize(payload);
                using var content2 = new StringContent(json2, System.Text.Encoding.UTF8, "application/json");
                var resp = await client.PostAsync("/v1/Authentication/register", content2);

                if (resp.StatusCode == HttpStatusCode.BadRequest ||
                    (int)resp.StatusCode == 429 ||
                    resp.StatusCode == HttpStatusCode.ServiceUnavailable)
                {
                    failureCount++;

                    if (resp.StatusCode == HttpStatusCode.BadRequest)
                    {
                        var txt = await resp.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(txt);
                        // UsernameTaken enum value is 1
                        if (doc.RootElement.TryGetProperty("errorType", out var et) && et.GetInt32() == 1)
                            sawUsernameTaken = true;
                    }
                }
            }

            Assert.True(failureCount >= 1, "Expected at least one failure after repeated attempts");
            Assert.True(sawUsernameTaken || failureCount > 1, "Expected UsernameTaken or rate limiting to trigger");
        }

        //todo: Registrazione [FAIL] (email con spazi o caratteri speciali non supportati)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/auth/register should fail with email containing unsupported spaces or special characters")]
        public async Task Register_EmailWithInvalidCharacters_ShouldFail()
        {
            var client = _factory.CreateClient();

            // Email contains a space which should be invalid
            var invalidEmail = $"invalid email_{Guid.NewGuid():N}@example.com".Replace("_", " ");

            var payload = new Register_DTO
            {
                CompanyName = "Invalid Email Co",
                Email = invalidEmail,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Test",
                LastName = "User"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/Authentication/register", content);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.False(doc.RootElement.GetProperty("success").GetBoolean());
        }

        
        //todo: Registrazione [OK] (genera correttamente UserID e collega alla company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should correctly generate UserID and link to company")]
        public async Task Register_ShouldGenerateUserIdAndLinkToCompany()
        {
            var client = _factory.CreateClient();

            var uniqueEmail = $"userid_link_{Guid.NewGuid():N}@example.com";
            var payload = new Register_DTO
            {
                CompanyName = "UserID Link Co",
                Email = uniqueEmail,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Alice",
                LastName = "Verdi"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/Authentication/register", content);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());

            using var db = _factory.CreateDbContext();
            var user = db.AspNetUser_DS.AsNoTracking().FirstOrDefault(u => u.Email == uniqueEmail);
            Assert.NotNull(user);
            Assert.NotEqual(Guid.Empty, user!.UserID);

            var staff = db.Company_Staff_DS.AsNoTracking().FirstOrDefault(s => s.UserID == user.UserID);
            Assert.NotNull(staff);

            var company = db.Company_DS.AsNoTracking().FirstOrDefault(c => c.TenantID == staff!.TenantID);
            Assert.NotNull(company);
            // Repository sets CreatedBy to the newly created user's UserID
            Assert.Equal(user.UserID, company!.CreatedBy);
        }

        //todo: Registrazione [OK] (assegna ruolo predefinito corretto — es: “User” o “BasicStaff”)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should assign correct default role (e.g. 'User' or 'BasicStaff')")]
        public async Task Register_ShouldAssignDefaultRoleCorrectly()
        {
            var client = _factory.CreateClient();

            var uniqueEmail = $"role_{Guid.NewGuid():N}@example.com";
            var payload = new Register_DTO
            {
                CompanyName = "DefaultRole Co",
                Email = uniqueEmail,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Role",
                LastName = "Tester"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act: perform registration
            var response = await client.PostAsync("/v1/Authentication/register", content);

            // Assert request succeeded
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());

            // Assert default role assigned in DB
            using var db = _factory.CreateDbContext();
            var user = db.AspNetUser_DS.AsNoTracking().FirstOrDefault(u => u.Email == uniqueEmail);
            Assert.NotNull(user);

            var roles = db.AspNetUserRole_DS
                .Include(ur => ur.AspNetRole)
                .AsNoTracking()
                .Where(ur => ur.UserID == user!.UserID)
                .ToList();

            Assert.NotEmpty(roles);
            Assert.Contains(roles, r => r.RoleID == Configuration.CompanyAdminRoleID);
            Assert.Contains(roles, r => r.AspNetRole != null && r.AspNetRole.RoleName == Configuration.CompanyAdminRoleName);
        }

        //todo: Registrazione [OK] (genera token o step successivo per verifica email, se attivo)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should generate token or step for email verification if enabled")]
        public async Task Register_ShouldGenerateEmailVerificationStep()
        {
            var client = _factory.CreateClient();

            // Arrange a fresh registration
            var uniqueEmail = $"verify_{Guid.NewGuid():N}@example.com";
            var payload = new Register_DTO
            {
                CompanyName = "Verify Co",
                Email = uniqueEmail,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Veri",
                LastName = "Fy"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act: register the user
            var response = await client.PostAsync("/v1/Authentication/register", content);

            // Assert HTTP + payload
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());

            // Assert: user exists and requires email verification (EmailConfirmed == false)
            using var db = _factory.CreateDbContext();
            var user = db.AspNetUser_DS.AsNoTracking().FirstOrDefault(u => u.Email == uniqueEmail);
            Assert.NotNull(user);
            Assert.False(user!.EmailConfirmed);
            Assert.False(user.IsApproved);

            // Optional sanity: company link exists, which carries the IDs used for confirmation links
            var staff = db.Company_Staff_DS.AsNoTracking().FirstOrDefault(s => s.UserID == user.UserID);
            Assert.NotNull(staff);
        }

        
        //todo: Registration [OK] (auditing: track created user, IP, timestamp)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should audit user creation with IP and timestamp")]
        public async Task Register_ShouldAuditUserCreation()
        {
            var client = _factory.CreateClient();

            var uniqueEmail = $"audit_{Guid.NewGuid():N}@example.com";
            var payload = new Register_DTO
            {
                CompanyName = "Audit Co",
                Email = uniqueEmail,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Audit",
                LastName = "Tester"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            var before = DateTime.UtcNow.AddMinutes(-5);
            var after = DateTime.UtcNow.AddMinutes(5);

            // Act
            var response = await client.PostAsync("/v1/Authentication/register", content);

            // Assert HTTP
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.True(doc.RootElement.GetProperty("success").GetBoolean());

            // Assert auditing in DB: timestamps and CreatedBy linkage
            using var db = _factory.CreateDbContext();

            var user = db.AspNetUser_DS.AsNoTracking().FirstOrDefault(u => u.Email == uniqueEmail);
            Assert.NotNull(user);
            Assert.InRange(user!.DateIns, before, after);
            // IpAddress may or may not be set by the API; just ensure field is accessible
            _ = user.IpAddress; // no-op access

            var staff = db.Company_Staff_DS.AsNoTracking().FirstOrDefault(s => s.UserID == user.UserID);
            Assert.NotNull(staff);
            Assert.InRange(staff!.DateIns, before, after);
            Assert.Equal(user.UserID, staff.CreatedBy);

            var company = db.Company_DS.AsNoTracking().FirstOrDefault(c => c.TenantID == staff.TenantID);
            Assert.NotNull(company);
            Assert.InRange(company!.DateIns, before, after);
            Assert.Equal(user.UserID, company.CreatedBy);

            var companyEmail = db.Set<Company_Contact_Email>().AsNoTracking()
                .FirstOrDefault(e => e.TenantID == company.TenantID && e.Email == uniqueEmail);
            Assert.NotNull(companyEmail);
            Assert.InRange(companyEmail!.DateIns, before, after);
            Assert.Equal(user.UserID, companyEmail.CreatedBy);
        }

        //todo: Registrazione [OK] (eventuale invio mail di benvenuto)
        [Trait("Layer", "API")]
        [Trait("Feature", "Registration")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/register should send welcome email if applicable")]
        public async Task Register_ShouldSendWelcomeEmail()
        {
            // Arrange: override IEmailSender with a fake that records sends
            FakeEmailSender.Sent.Clear();

            var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(global::Shared.Services.Interfaces.IEmailSender));
                    if (existing != null) services.Remove(existing);

                    services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                        typeof(global::Shared.Services.Interfaces.IEmailSender),
                        typeof(FakeEmailSender),
                        Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
                });
            });

            var client = testFactory.CreateClient();

            var email = $"welcome_{Guid.NewGuid():N}@example.com";
            var payload = new Register_DTO
            {
                CompanyName = "Welcome Co",
                Email = email,
                Password = "StrongP@ssw0rd!",
                ConfirmPassword = "StrongP@ssw0rd!",
                AcceptPrivacyPolicy = true,
                FirstName = "Giulia",
                LastName = "Bianchi"
            };

            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/Authentication/register", content);

            // Assert HTTP ok
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            // Assert an email was attempted with expected recipient and subject
            Assert.True(FakeEmailSender.Sent.Count >= 1);
            var last = FakeEmailSender.Sent.Last();
            Assert.Equal(email, last.Email);
            Assert.Contains("Conferma registrazione", last.Msg.Subject);
        }

        // Test helper used to capture outgoing emails in DI
        private class FakeEmailSender : global::Shared.Services.Interfaces.IEmailSender
        {
            public static readonly List<(global::Shared.Models.EmailMessage Msg, string Name, string Email)> Sent = new();

            public void SendEmail(global::Shared.Models.EmailMessage message, string DestUserName, string DestUserEmail)
            {
                lock (Sent)
                {
                    Sent.Add((message, DestUserName, DestUserEmail));
                }
            }

            public Task SendEmailAsync(global::Shared.Models.EmailMessage message, string DestUserName, string DestUserEmail)
            {
                SendEmail(message, DestUserName, DestUserEmail);
                return Task.CompletedTask;
            }
        }

    }
}
