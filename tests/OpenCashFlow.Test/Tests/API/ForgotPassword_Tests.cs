using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Application.Abstractions;
using System.Net;
using System.Text.Json;
using Xunit;
using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace OpenCashFlow.Test.Tests.API
{
    [Collection("NonParallelCollection")]
    public class ForgotPasswordApiTests
    {
        private const string ExistingEmail = "user.seed@example.local";
        private readonly CustomWebApplicationFactory _factory;

        public ForgotPasswordApiTests(CustomWebApplicationFactoryFixture fixture)
        {
            _factory = fixture.Factory;
        }

        #region ForgotPassword Tests

        /// <summary>
        /// Test: ForgotPassword with a valid email should send email and create a token in the database
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ForgotPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/forgot-password should send email and create token for valid email")]
        public async Task ForgotPassword_ValidEmail_ShouldSendEmailAndCreateToken()
        {
            // Arrange: usa un'email esistente nel database di test
            var existingEmail = ExistingEmail;

            // Clear any sent emails from fake sender
            FakeEmailSender.Sent.Clear();

            var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(OpenCashFlow.Application.Abstractions.IEmailSender));
                    if (existing != null) services.Remove(existing);

                    services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                        typeof(OpenCashFlow.Application.Abstractions.IEmailSender),
                        typeof(FakeEmailSender),
                        Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
                });
            });

            var client = testFactory.CreateClient();

            var payload = new { Email = existingEmail };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/Authentication/forgot-password", content);

            // Assert HTTP response
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.Contains("If the account exists, you will receive an email shortly.", doc.RootElement.GetProperty("message").GetString());

            // Assert email was sent
            Assert.Single(FakeEmailSender.Sent);
            var sentEmail = FakeEmailSender.Sent.First();
            Assert.Equal(existingEmail, sentEmail.Email);
            Assert.Contains("Reset Password", sentEmail.Msg.Subject);

            // Assert token was created in database
            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            Assert.NotNull(user);
            Assert.NotNull(user!.PasswordResetToken);
            Assert.NotNull(user.PasswordResetTokenValidUntil);
            Assert.True(user.PasswordResetTokenValidUntil > DateTime.UtcNow);
        }

        /// <summary>
        /// Test: ForgotPassword with a non-existent email should respond OK without leaking info (security)
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ForgotPassword")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/forgot-password should return OK for non-existent email without exposing info")]
        public async Task ForgotPassword_NonExistentEmail_ShouldReturnOkWithoutExposingInfo()
        {
            // Arrange
            var nonExistentEmail = $"nonexistent_{Guid.NewGuid():N}@example.com";
            var client = _factory.CreateClient();

            var payload = new { Email = nonExistentEmail };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/Authentication/forgot-password", content);

            // Assert - same behavior whether the user exists or not (security)
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.Contains("If the account exists, you will receive an email shortly.", doc.RootElement.GetProperty("message").GetString());
        }

        /// <summary>
        /// Test: ForgotPassword with empty/null email should fail with BadRequest
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ForgotPassword")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/forgot-password should fail with BadRequest for empty/null email")]
        public async Task ForgotPassword_EmptyEmail_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            var testCases = new[]
            {
                new { Email = "" },
                new { Email = "   " }
            };

            foreach (var payload in testCases)
            {
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await client.PostAsync("/v1/Authentication/forgot-password", content);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var text = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(text);
                Assert.Contains("Email is required", doc.RootElement.GetProperty("message").GetString());
            }
        }

        /// <summary>
        /// Test: Generated token should be unique (GUID) and saved correctly in the database
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ForgotPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "ForgotPassword should generate unique token and save it in database")]
        public async Task ForgotPassword_ShouldGenerateUniqueTokenAndSaveInDatabase()
        {
            // Arrange
            var existingEmail = ExistingEmail;
            FakeEmailSender.Sent.Clear();

            var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(OpenCashFlow.Application.Abstractions.IEmailSender));
                    if (existing != null) services.Remove(existing);

                    services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                        typeof(OpenCashFlow.Application.Abstractions.IEmailSender),
                        typeof(FakeEmailSender),
                        Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
                });
            });

            var client = testFactory.CreateClient();

            var payload = new { Email = existingEmail };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act - first request
            await client.PostAsync("/v1/Authentication/forgot-password", content);

            using var db1 = _factory.CreateDbContext();
            var user1 = await db1.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            var firstToken = user1!.PasswordResetToken;

            // Simulate a second request
            using var content2 = new StringContent(json, Encoding.UTF8, "application/json");
            await client.PostAsync("/v1/Authentication/forgot-password", content2);

            using var db2 = _factory.CreateDbContext();
            var user2 = await db2.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            var secondToken = user2!.PasswordResetToken;

            // Assert - i token devono essere diversi (rigenerato)
            Assert.NotNull(firstToken);
            Assert.NotNull(secondToken);
            Assert.NotEqual(firstToken, secondToken);
        }

        /// <summary>
        /// Test: Token should have configurable expiration (default 30 minutes)
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ForgotPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "ForgotPassword token should have configurable expiration (default 30 minutes)")]
        public async Task ForgotPassword_TokenShouldHaveConfigurableExpiration()
        {
            // Arrange
            var existingEmail = ExistingEmail;
            FakeEmailSender.Sent.Clear();

            var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(OpenCashFlow.Application.Abstractions.IEmailSender));
                    if (existing != null) services.Remove(existing);

                    services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                        typeof(OpenCashFlow.Application.Abstractions.IEmailSender),
                        typeof(FakeEmailSender),
                        Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
                });
            });

            var client = testFactory.CreateClient();

            var payload = new { Email = existingEmail };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            var beforeRequest = DateTime.UtcNow;

            // Act
            await client.PostAsync("/v1/Authentication/forgot-password", content);

            var afterRequest = DateTime.UtcNow;

            // Assert
            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            Assert.NotNull(user!.PasswordResetTokenValidUntil);

            // Il token dovrebbe scadere tra 29 e 31 minuti da ora (default 30 minuti)
            var expectedExpiration = beforeRequest.AddMinutes(30);
            var actualExpiration = user.PasswordResetTokenValidUntil.Value;

            var difference = Math.Abs((actualExpiration - expectedExpiration).TotalMinutes);
            Assert.True(difference <= 2, $"Token expiration should be around 30 minutes. Actual difference: {difference} minutes");
        }

        /// <summary>
        /// Test: Sent email should contain a link with a Base64Url-encoded token
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ForgotPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "ForgotPassword email should contain reset link with Base64Url encoded token")]
        public async Task ForgotPassword_EmailShouldContainResetLinkWithEncodedToken()
        {
            // Arrange
            var existingEmail = ExistingEmail;
            FakeEmailSender.Sent.Clear();

            var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(OpenCashFlow.Application.Abstractions.IEmailSender));
                    if (existing != null) services.Remove(existing);

                    services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                        typeof(OpenCashFlow.Application.Abstractions.IEmailSender),
                        typeof(FakeEmailSender),
                        Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
                });
            });

            var client = testFactory.CreateClient();

            var payload = new { Email = existingEmail };
            var json = JsonSerializer.Serialize(payload);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // Act
            await client.PostAsync("/v1/Authentication/forgot-password", content);

            // Assert
            Assert.Single(FakeEmailSender.Sent);
            var sentEmail = FakeEmailSender.Sent.First();
            var emailContent = sentEmail.Msg.Content;

            // Verifica che l'email contenga un link di reset
            Assert.Contains("reset-password?token=", emailContent);

            // Estrai il token dall'email
            var tokenMatch = System.Text.RegularExpressions.Regex.Match(emailContent, @"reset-password\?token=([^\s""<>]+)");
            Assert.True(tokenMatch.Success);
            var encodedToken = tokenMatch.Groups[1].Value;

            // Verifica che il token possa essere decodificato
            var decodedBytes = WebEncoders.Base64UrlDecode(encodedToken);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);
            Assert.NotEmpty(decodedToken);

            // Verifica che il token decodificato corrisponda a quello salvato nel DB
            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            Assert.Equal(user!.PasswordResetToken, decodedToken);
        }

        /// <summary>
        /// Test: ForgotPassword by UserID instead of email should work correctly
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ForgotPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "ForgotPassword by UserID should work correctly")]
        public async Task ForgotPassword_ByUserId_ShouldWork()
        {
            // Arrange
            var existingEmail = ExistingEmail;

            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            Assert.NotNull(user);

            var userId = user!.UserID;

            FakeEmailSender.Sent.Clear();

            // This test verifies the service method directly via the repository
            // since there is no specific API endpoint for ForgotPassword by UserID

            using var scope = _factory.Services.CreateScope();
            var authService = scope.ServiceProvider.GetRequiredService<OpenCashFlow.API.Services.Interfaces.IAuthenticationService>();

            // Act
            await authService.ForgotPasswordAsync(userId, CancellationToken.None);

            // Assert - verify that the token was created
            using var db2 = _factory.CreateDbContext();
            var updatedUser = await db2.AspNetUser_DS.FirstOrDefaultAsync(u => u.UserID == userId);
            Assert.NotNull(updatedUser!.PasswordResetToken);
            Assert.NotNull(updatedUser.PasswordResetTokenValidUntil);
            Assert.True(updatedUser.PasswordResetTokenValidUntil > DateTime.UtcNow);
        }

        #endregion

        #region ResetPassword Tests

        /// <summary>
        /// Test: ResetPassword with a valid token should update the password
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should update password with valid token")]
        public async Task ResetPassword_ValidToken_ShouldUpdatePassword()
        {
            // Arrange - first create a reset token
            var existingEmail = ExistingEmail;
            FakeEmailSender.Sent.Clear();

            var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(OpenCashFlow.Application.Abstractions.IEmailSender));
                    if (existing != null) services.Remove(existing);

                    services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                        typeof(OpenCashFlow.Application.Abstractions.IEmailSender),
                        typeof(FakeEmailSender),
                        Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
                });
            });

            var client = testFactory.CreateClient();

            // Richiedi reset password
            var forgotPayload = new { Email = existingEmail };
            var forgotJson = JsonSerializer.Serialize(forgotPayload);
            using var forgotContent = new StringContent(forgotJson, Encoding.UTF8, "application/json");
            await client.PostAsync("/v1/Authentication/forgot-password", forgotContent);

            // Ottieni il token dal database
            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            var token = user!.PasswordResetToken!;
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            var newPassword = "NewSecureP@ssw0rd!";
            var resetPayload = new { Token = encodedToken, NewPassword = newPassword };
            var resetJson = JsonSerializer.Serialize(resetPayload);
            using var resetContent = new StringContent(resetJson, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/Authentication/reset-password", resetContent);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.Contains("Password reset completed successfully", doc.RootElement.GetProperty("message").GetString());

            // Verifica che il token sia stato cancellato dal database
            using var db2 = _factory.CreateDbContext();
            var updatedUser = await db2.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            Assert.Null(updatedUser!.PasswordResetToken);
            Assert.Null(updatedUser.PasswordResetTokenValidUntil);
        }

        /// <summary>
        /// Test: ResetPassword with an expired token should fail
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should fail with expired token")]
        public async Task ResetPassword_ExpiredToken_ShouldFail()
        {
            // Arrange - manually create an expired token in the database
            var existingEmail = ExistingEmail;

            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            Assert.NotNull(user);

            var expiredToken = Guid.NewGuid().ToString("N");
            user!.PasswordResetToken = expiredToken;
            user.PasswordResetTokenValidUntil = DateTime.UtcNow.AddMinutes(-10); // Scaduto 10 minuti fa
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(expiredToken));

            var resetPayload = new { Token = encodedToken, NewPassword = "NewP@ssw0rd!" };
            var resetJson = JsonSerializer.Serialize(resetPayload);
            using var resetContent = new StringContent(resetJson, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/Authentication/reset-password", resetContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.Contains("Token is invalid or expired", doc.RootElement.GetProperty("message").GetString());
        }

        /// <summary>
        /// Test: ResetPassword with an invalid token should fail
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should fail with invalid token")]
        public async Task ResetPassword_InvalidToken_ShouldFail()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidToken = Guid.NewGuid().ToString("N");
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(invalidToken));

            var resetPayload = new { Token = encodedToken, NewPassword = "NewP@ssw0rd!" };
            var resetJson = JsonSerializer.Serialize(resetPayload);
            using var resetContent = new StringContent(resetJson, Encoding.UTF8, "application/json");

            // Act
            var response = await client.PostAsync("/v1/Authentication/reset-password", resetContent);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.Contains("Token is invalid or expired", doc.RootElement.GetProperty("message").GetString());
        }

        /// <summary>
        /// Test: ResetPassword with empty/null token should fail with BadRequest
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should fail with empty/null token")]
        public async Task ResetPassword_EmptyToken_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            var testCases = new[]
            {
                new { Token = "", NewPassword = "NewP@ssw0rd!" },
                new { Token = "   ", NewPassword = "NewP@ssw0rd!" }
            };

            foreach (var payload in testCases)
            {
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await client.PostAsync("/v1/Authentication/reset-password", content);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var text = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(text);
                Assert.Contains("Token and new password are required", doc.RootElement.GetProperty("message").GetString());
            }
        }

        /// <summary>
        /// Test: ResetPassword with empty/null new password should fail with BadRequest
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ResetPassword")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/auth/reset-password should fail with empty/null new password")]
        public async Task ResetPassword_EmptyNewPassword_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();
            var validToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(Guid.NewGuid().ToString("N")));

            var testCases = new[]
            {
                new { Token = validToken, NewPassword = "" },
                new { Token = validToken, NewPassword = "   " }
            };

            foreach (var payload in testCases)
            {
                var json = JsonSerializer.Serialize(payload);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Act
                var response = await client.PostAsync("/v1/Authentication/reset-password", content);

                // Assert
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
                var text = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(text);
                Assert.Contains("Token and new password are required", doc.RootElement.GetProperty("message").GetString());
            }
        }

        #endregion

        #region ValidateResetToken Tests

        /// <summary>
        /// Test: ValidateResetToken with a valid token should return isValid=true
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ValidateResetToken")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/auth/validate-reset-token should return isValid=true for valid token")]
        public async Task ValidateResetToken_ValidToken_ShouldReturnIsValidTrue()
        {
            // Arrange - crea un token valido
            var existingEmail = ExistingEmail;
            FakeEmailSender.Sent.Clear();

            var testFactory = _factory.WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var existing = services.FirstOrDefault(d => d.ServiceType == typeof(OpenCashFlow.Application.Abstractions.IEmailSender));
                    if (existing != null) services.Remove(existing);

                    services.Add(new Microsoft.Extensions.DependencyInjection.ServiceDescriptor(
                        typeof(OpenCashFlow.Application.Abstractions.IEmailSender),
                        typeof(FakeEmailSender),
                        Microsoft.Extensions.DependencyInjection.ServiceLifetime.Singleton));
                });
            });

            var client = testFactory.CreateClient();

            // Crea un token tramite forgot-password
            var forgotPayload = new { Email = existingEmail };
            var forgotJson = JsonSerializer.Serialize(forgotPayload);
            using var forgotContent = new StringContent(forgotJson, Encoding.UTF8, "application/json");
            await client.PostAsync("/v1/Authentication/forgot-password", forgotContent);

            // Ottieni il token dal database
            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            var token = user!.PasswordResetToken!;
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));

            // Act
            var response = await client.GetAsync($"/v1/Authentication/validate-reset-token?token={encodedToken}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.True(doc.RootElement.GetProperty("isValid").GetBoolean());
            Assert.False(doc.RootElement.GetProperty("isExpired").GetBoolean());
            Assert.Equal("Token is valid", doc.RootElement.GetProperty("message").GetString());
        }

        /// <summary>
        /// Test: ValidateResetToken with an expired token should return isExpired=true
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ValidateResetToken")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/auth/validate-reset-token should return isExpired=true for expired token")]
        public async Task ValidateResetToken_ExpiredToken_ShouldReturnIsExpiredTrue()
        {
            // Arrange - create an expired token
            var existingEmail = ExistingEmail;

            using var db = _factory.CreateDbContext();
            var user = await db.AspNetUser_DS.FirstOrDefaultAsync(u => u.Email == existingEmail);
            Assert.NotNull(user);

            var expiredToken = Guid.NewGuid().ToString("N");
            user!.PasswordResetToken = expiredToken;
            user.PasswordResetTokenValidUntil = DateTime.UtcNow.AddMinutes(-10); // Scaduto
            await db.SaveChangesAsync();

            var client = _factory.CreateClient();
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(expiredToken));

            // Act
            var response = await client.GetAsync($"/v1/Authentication/validate-reset-token?token={encodedToken}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.False(doc.RootElement.GetProperty("isValid").GetBoolean());
            Assert.True(doc.RootElement.GetProperty("isExpired").GetBoolean());
            Assert.Equal("Token expired", doc.RootElement.GetProperty("message").GetString());
        }

        /// <summary>
        /// Test: ValidateResetToken with an invalid token should return isValid=false
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ValidateResetToken")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/auth/validate-reset-token should return isValid=false for invalid token")]
        public async Task ValidateResetToken_InvalidToken_ShouldReturnIsValidFalse()
        {
            // Arrange
            var client = _factory.CreateClient();
            var invalidToken = Guid.NewGuid().ToString("N");
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(invalidToken));

            // Act
            var response = await client.GetAsync($"/v1/Authentication/validate-reset-token?token={encodedToken}");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            Assert.False(doc.RootElement.GetProperty("isValid").GetBoolean());
            Assert.False(doc.RootElement.GetProperty("isExpired").GetBoolean());
            Assert.Equal("Token is invalid", doc.RootElement.GetProperty("message").GetString());
        }

        /// <summary>
        /// Test: ValidateResetToken with an empty token should fail with BadRequest
        /// </summary>
        [Trait("Layer", "API")]
        [Trait("Feature", "ValidateResetToken")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/auth/validate-reset-token should fail with BadRequest for empty token")]
        public async Task ValidateResetToken_EmptyToken_ShouldReturnBadRequest()
        {
            // Arrange
            var client = _factory.CreateClient();

            // Act
            var response = await client.GetAsync("/v1/Authentication/validate-reset-token?token=");

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);

            // Verify that the message is present
            if (doc.RootElement.TryGetProperty("message", out var messageProp))
            {
                Assert.Contains("Token is required", messageProp.GetString()!);
            }
            else
            {
                // If there is no message property, ensure it is at least BadRequest
                Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            }
        }

        #endregion

        #region Token Encoding/Decoding Tests

        /// <summary>
        /// Test: Token Base64Url encoding/decoding should work correctly
        /// </summary>
        [Trait("Layer", "Unit")]
        [Trait("Feature", "TokenEncoding")]
        [Trait("Type", "Unit")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "Token Base64Url encoding/decoding should work correctly")]
        public void TokenEncoding_Base64Url_ShouldEncodeAndDecodeCorrectly()
        {
            // Arrange
            var originalToken = Guid.NewGuid().ToString("N");

            // Act - encode
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(originalToken));

            // Act - decode
            var decodedBytes = WebEncoders.Base64UrlDecode(encodedToken);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            // Assert
            Assert.Equal(originalToken, decodedToken);
            Assert.DoesNotContain("+", encodedToken); // Base64Url non usa '+'
            Assert.DoesNotContain("/", encodedToken); // Base64Url non usa '/'
            Assert.DoesNotContain("=", encodedToken); // Base64Url non usa padding '='
        }

        #endregion

        #region Test Helper Classes

        /// <summary>
        /// Fake email sender per catturare le email inviate durante i test
        /// </summary>
        private class FakeEmailSender : OpenCashFlow.Application.Abstractions.IEmailSender
        {
            public static readonly List<(EmailMessage Msg, string Name, string Email)> Sent = new();

            public void SendEmail(EmailMessage message, string DestUserName, string DestUserEmail)
            {
                lock (Sent)
                {
                    Sent.Add((message, DestUserName, DestUserEmail));
                }
            }

            public Task SendEmailAsync(EmailMessage message, string DestUserName, string DestUserEmail)
            {
                SendEmail(message, DestUserName, DestUserEmail);
                return Task.CompletedTask;
            }
        }

        #endregion
    }

    #region Request DTOs

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    #endregion
}
