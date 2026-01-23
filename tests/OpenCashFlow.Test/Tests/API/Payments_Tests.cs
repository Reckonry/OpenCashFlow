using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Test.Utilities;
using Microsoft.Extensions.DependencyInjection;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Models;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Xunit;

namespace OpenCashFlow.Test.Tests
{
    // NOTE: This test class uses real PostgreSQL via Testcontainers for accurate transaction testing.
    // To use InMemory database for faster tests, change to [Collection("NonParallelCollection")]
    // and use CustomWebApplicationFactoryFixture.
    [Collection("PostgresCollection")]
    public class PaymentsApiTests : IDisposable
    {
        private readonly CustomWebApplicationFactory _factory;

        public PaymentsApiTests(PostgresContainerFixture fixture)
        {
            _factory = fixture.AppFactory;

            // Reset database state before each test
            ResetTestData();
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        private void ResetTestData()
        {
            using var scope = _factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Remove all payments
            db.Payment_DS.RemoveRange(db.Payment_DS);
            db.SaveChanges();

            // Re-add the seeded payments in original state
            var seededPayments = TestHelpers.CreateTestPayment();
            foreach (var payment in seededPayments)
            {
                db.Payment_DS.Add(payment);
            }

            db.SaveChanges();
        }

        #region Payment Creation
        // Payment creation [OK] (using an authorized employee)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments should succeed with authorized employee")]
        public async Task CreatePayment_AuthorizedEmployee_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                Amount = 150,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Description = "Test payment"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            Assert.NotNull(apiResponse);
            Assert.True(apiResponse!.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(payment.Amount, apiResponse.Data!.Amount);
        }

        // Payment creation [FAIL] (without authentication)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments without authentication should fail")]
        public async Task CreatePayment_WithoutAuthentication_ShouldFail()
        {
            var client = _factory.CreateClient();
            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Amount = 150,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Description = "Test"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        }

        // Payment creation [FAIL] (using an unauthorized employee)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments with unauthorized employee should fail", Skip = "non ancora implementati ruoli")]
        public async Task CreatePayment_UnauthorizedEmployee_ShouldFail()
        {
            // user 000000000002 belongs to the same company but we'll generate a token with an invalid role
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var token = await JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "Invalid");

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                Amount = 200,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Description = "Unauthorized"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Payment creation [FAIL] (attempting to create it for a different company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments to different company should fail", Skip = "TBF")]
        public async Task CreatePayment_ToDifferentCompany_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000002"),
                UserID = userId,
                Amount = 100,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Description = "Wrong company"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var paymentResponse = JsonSerializer.Deserialize<ApiResponse<Payment_Detail_DTO>>(responseBody, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(paymentResponse);
            Assert.NotNull(paymentResponse.Data);
            Assert.True(paymentResponse.Data.TenantID == Guid.Parse("00000000-0000-0000-0000-000000000001"));

        }

        // Payment creation [FAIL] (missing required field - es: Amount)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/payments missing required field (Amount) should fail")]
        public async Task CreatePayment_MissingRequiredField_ShouldFail()
        {
                        var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                EntryType = nameof(EntryTypeEnum.Income),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001")
                // Amount intentionally missing
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }

        // Payment creation [FAIL] (negative numeric field - es: Amount < 0)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/payments with negative amount should fail")]
        public async Task CreatePayment_NegativeAmount_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                Amount = -10,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Description = "Negative amount"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }

        // Payment creation [FAIL] (non-existent PaymentMethod)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments with non-existent PaymentMethod should fail")]
        public async Task CreatePayment_NonExistentPaymentMethod_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                Amount = 10,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.NewGuid(),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Description = "Wrong method"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }

        // Payment creation [FAIL] (non-existent DocumentType)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments with non-existent DocumentType should fail")]
        public async Task CreatePayment_NonExistentDocumentType_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                Amount = 20,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.NewGuid(),
                Description = "Wrong doc"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        }

        // Payment creation [FAIL] (DocumentType from another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments with DocumentType from another company should fail")]
        public async Task CreatePayment_DocumentTypeFromAnotherCompany_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payment = new Payment_Create_DTO
            {
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                Amount = 40,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000003"),
                Description = "Foreign doc"
            };

            var response = await client.PostAsJsonAsync("/v1/Payment", payment);

            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Forbidden);
        }
        #endregion

        #region READ / List
        // ✅ [OK] - Payment list read (dipendente autorizzato)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments should return seeded payments")]
        public async Task GetPayments_ShouldReturnSuccessAndData()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var filters = new Payment_Filter_DTO();
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var payments = JsonSerializer.Deserialize<List<Payment_List_DTO>>(responseBody, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(payments);
            Assert.True(payments.Count > 0, "Expected at least one seeded payment.");
        }

        // ❌ [FAIL] - Payment list read (dipendente NON autorizzato)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments without token should return 401 Unauthorized")]
        public async Task GetPayments_WithoutToken_ShouldFail()
        {
            // ❌ Non settiamo Authorization
            var client = _factory.CreateClient();

            var filters = new Payment_Filter_DTO();
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            // ✅ Verify that access without a token is rejected
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        // Payment list read [OK] (limited to payments from the same company)
        // ❌ [FAIL] - Payment list read (attempt to read payments from another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/payments/{id} should return 403 if payment belongs to another company")]
        public async Task GetPayment_WithWrongCompany_ShouldReturnForbidden()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000005");
            var response = await client.GetAsync($"/v1/payment/{paymentId}");

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound);
        }


        // READ - DETTAGLIO
        // Single payment read [OK] (if it belongs to the same company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/payments/{id} should return payment if it belongs to the user's company")]
        public async Task GetPayment_BelongsToCompany_ShouldSucceed()
        {
            // Luca is associated with Company 000000000001
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var response = await client.GetAsync($"/v1/payment/{paymentId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var apiResponse = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();

            Assert.NotNull(apiResponse);
            Assert.True(apiResponse!.Success);
            Assert.NotNull(apiResponse.Data);
            Assert.Equal(paymentId, apiResponse.Data!.PaymentID);
        }

        // Single payment read [FAIL] (if it does NOT belong to the same company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/payments/{id} should fail if it belongs to another company")]
        public async Task GetPayment_OtherCompany_ShouldFail()
        {
            // Luca is associated with Company 000000000001
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000005");
            var response = await client.GetAsync($"/v1/payment/{paymentId}");

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || 
                response.StatusCode == HttpStatusCode.NotFound || 
                response.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Single payment read [FAIL] (if the ID does not exist)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/payments/{id} should return NotFound if ID does not exist")]
        public async Task GetPayment_NonExistentId_ShouldReturnNotFound()
        {
            // Luca is associated with Company 000000000001
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0990-000000000099");
            var response = await client.GetAsync($"/v1/payment/{paymentId}");

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Unauthorized);
        }
        #endregion

        // UPDATE
        // Payment update [OK] (by an authorized owner user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/payments/{id} should succeed for authorized and owning user")]
        public async Task UpdatePayment_AuthorizedUser_ShouldSucceed()
        {
            // Luca is associated with Company 000000000001
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            var existing = await client.GetFromJsonAsync<ApiResponse<Payment_Detail_DTO>>($"/v1/payment/{paymentId}");
            Assert.NotNull(existing);

            var dto = existing!.Data!;
            dto.Amount = 999;

            var response = await client.PutAsJsonAsync($"/v1/Payment/{paymentId}", dto);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var updated = await response.Content.ReadFromJsonAsync<ApiResponse<Payment_Detail_DTO>>();
            Assert.NotNull(updated);
            Assert.Equal(999, updated!.Data!.Amount);
            
        }

        // Payment update [FAIL] (unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/payments/{id} should fail for unauthorized user")]
        public async Task UpdatePayment_UnauthorizedUser_ShouldFail()
        {
            // use employee from another company
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000005");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var dto = new Payment_Detail_DTO
            {
                PaymentID = paymentId,
                TenantID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                UserID = userId,
                Amount = 10,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                Description = "test"
            };

            var response = await client.PutAsJsonAsync($"/v1/Payment/{paymentId}", dto);

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.Unauthorized);
        }

        // Payment update [FAIL] (PaymentMethod di altra company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/payments/{id} should fail if PaymentMethod is from another company", Skip = "Da capire come limitare nel db stesso questo")]
        public async Task UpdatePayment_PaymentMethodFromAnotherCompany_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var existing = await client.GetFromJsonAsync<ApiResponse<Payment_Detail_DTO>>($"/v1/payment/{paymentId}");
            Assert.NotNull(existing);

            var dto = existing!.Data!;
            dto.PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000004");

            var response = await client.PutAsJsonAsync($"/v1/Payment/{paymentId}", dto);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Payment update [FAIL] (DocumentType from another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/payments/{id} should fail if DocumentType is from another company", Skip = "Da capire come limitare nel db questo...")]
        public async Task UpdatePayment_DocumentTypeFromAnotherCompany_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var existing = await client.GetFromJsonAsync<ApiResponse<Payment_Detail_DTO>>($"/v1/payment/{paymentId}");
            Assert.NotNull(existing);

            var dto = existing!.Data!;
            dto.DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000003");

            var response = await client.PutAsJsonAsync($"/v1/Payment/{paymentId}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Payment update [FAIL] (attempt to modify a locked field e.g., UserID)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/payments/{id} should fail if trying to modify locked field (UserID)", Skip = "non ancora previso a sistema...")]
        public async Task UpdatePayment_ModifyLockedField_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var existing = await client.GetFromJsonAsync<ApiResponse<Payment_Detail_DTO>>($"/v1/payment/{paymentId}");
            Assert.NotNull(existing);

            var dto = existing!.Data!;
            dto.UserID = Guid.Parse("00000000-0000-0000-0000-000000000002");

            var response = await client.PutAsJsonAsync($"/v1/Payment/{paymentId}", dto);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #region Deletition
        // Payment deletion [OK] (soft delete by an authorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/payments/{id} should perform soft delete for authorized user")]
        public async Task DeletePayment_AuthorizedUser_ShouldSoftDelete()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var response = await client.DeleteAsync($"/v1/Payment/{paymentId}");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Payment deletion [FAIL] (unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/payments/{id} should fail for unauthorized user")]
        public async Task DeletePayment_UnauthorizedUser_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000005");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var response = await client.DeleteAsync($"/v1/Payment/{paymentId}");

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound);
        }

        // Payment deletion [FAIL] (on a different company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/payments/{id} should fail when payment belongs to another company")]
        public async Task DeletePayment_OtherCompany_ShouldFail()
        {
            // Luca is associated with Company 000000000001
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // This payment belongs to Company 000000000002 (non autorizzata per Luca)
            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000010");
            var response = await client.DeleteAsync($"/v1/Payment/{paymentId}");

            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound);
        }

        // Payment deletion [FAIL] (on an already deleted payment)
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "DELETE /v1/payments/{id} should fail if payment is already deleted")]
        public async Task DeletePayment_AlreadyDeleted_ShouldFail()
        {
            // Luca is associated with Company 000000000001
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Attempt to delete the same payment twice
            var paymentId = Guid.Parse("00000000-0000-0000-0000-000000000999");

            var first = await client.DeleteAsync($"/v1/Payment/{paymentId}");
            Assert.Equal(HttpStatusCode.OK, first.StatusCode);

            var second = await client.DeleteAsync($"/v1/Payment/{paymentId}");

            Assert.True(second.StatusCode == HttpStatusCode.NotFound || second.StatusCode == HttpStatusCode.Forbidden);
        }

        // SECURITY
        // Payments API access without authentication [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/payments API access without authentication should fail")]
        public async Task AccessPaymentsApi_WithoutAuthentication_ShouldFail()
        {
            var client = _factory.CreateClient();

            var filters = new Payment_Filter_DTO();
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Payments API access with expired token [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/payments API access with expired token should fail")]
        public async Task AccessPaymentsApi_WithExpiredToken_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            // create an expired token
            var expiredToken = await JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, expiresInMinutes: -1, notBeforeOffsetMinutes: -10);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", expiredToken);

            var filters = new Payment_Filter_DTO();
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Payments API access with invalid role [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/payments API access with invalid role should fail", Skip = "not done yet roles")]
        public async Task AccessPaymentsApi_WithInvalidRole_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "Invalid");

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var filters = new Payment_Filter_DTO();
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        // FILTRI & QUERY
        // Payment list read con filtro per date [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments should return filtered list by date range")]
        public async Task GetPayments_FilterByDateRange_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var filters = new Payment_Filter_DTO()
            {
                FromDate = DateTime.Today.AddDays(-1),
                ToDate = DateTime.Today.AddDays(1)
            };
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var payments = JsonSerializer.Deserialize<List<Payment_List_DTO>>(responseBody, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(payments);
            Assert.True(payments.Count > 0, "Expected at least one seeded payment.");
        }

        // Payment list read con filtro per tipo documento [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments should return filtered list by document type")]
        public async Task GetPayments_FilterByDocumentType_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var filters = new Payment_Filter_DTO()
            {
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var payments = JsonSerializer.Deserialize<List<Payment_List_DTO>>(responseBody, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(payments);
            Assert.True(payments.Count > 0, "Expected at least one seeded payment.");
        }

        // Payment list read con filtro per payment method [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments should return filtered list by payment method")]
        public async Task GetPayments_FilterByPaymentMethod_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var filters = new Payment_Filter_DTO()
            {
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001")
            };
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var payments = JsonSerializer.Deserialize<List<Payment_List_DTO>>(responseBody, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(payments);
            Assert.True(payments.Count > 0, "Expected at least one seeded payment.");
        }

        // Payment list read con filtro combinato [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/payments should return filtered list with combined filters")]
        public async Task GetPayments_FilterByCombinedCriteria_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var filters = new Payment_Filter_DTO()
            {
                DocumentTypeID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                PaymentMethodID = Guid.Parse("00000000-0000-0000-0000-000000000001"),
                FromDate = DateTime.Today.AddDays(-2),
                ToDate = DateTime.Today.AddDays(2)
            };
            var json = JsonSerializer.Serialize(filters);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("/v1/payments", content);

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var responseBody = await response.Content.ReadAsStringAsync();
            var payments = JsonSerializer.Deserialize<List<Payment_List_DTO>>(responseBody, options: new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            Assert.NotNull(payments);
            Assert.True(payments.Count > 0, "Expected at least one seeded payment.");
        }

    }
}
