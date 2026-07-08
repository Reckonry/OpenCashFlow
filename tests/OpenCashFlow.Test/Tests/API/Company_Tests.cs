using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Contracts.DTOs;
using System.Net;
using System.Text.Json;
using Xunit;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Test.Tests
{
    [Collection("NonParallelCollection")]
    public class CompanyApiTests 
    {
        private readonly CustomWebApplicationFactory _factory;

        public CompanyApiTests(CustomWebApplicationFactoryFixture fixture)
        {
            _factory = fixture.Factory;
        }

        #region CREATE
        // Company creation [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/company should create company")]
        public async Task CreateCompany_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                CompanyName = $"New Co {Guid.NewGuid():N}",
                IsActive = true,
                MaxUsers = 10,
                TIN = $"TIN{Guid.NewGuid():N}".Substring(0, 12)
            };

            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Company creation [FAIL] (by an unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/company by unauthorized user should fail")]
        public async Task CreateCompany_UnauthorizedUser_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "Employee");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { CompanyName = "Unauthorized Co" };
            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Company creation [FAIL] (missing required field - es: CompanyName)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/company missing required CompanyName should fail")]
        public async Task CreateCompany_MissingRequiredField_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { CompanyName = "" };
            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // Company creation [FAIL] (CompanyName already exists)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/company with duplicate CompanyName should fail")]
        public async Task CreateCompany_DuplicateCompanyName_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyName = $"Duplicate Name {Guid.NewGuid():N}";
            var payload = new { CompanyName = companyName, MaxUsers = 10 };
            var created = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);

            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // Company creation [FAIL] (TIN already associated with another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/company with duplicate TIN should fail")]
        public async Task CreateCompany_DuplicateTin_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var tin = $"IT{Guid.NewGuid():N}"[..16];
            var created = await client.PostAsJsonAsync("/v1/Company", new { CompanyName = $"TIN Source {Guid.NewGuid():N}", TIN = tin, MaxUsers = 10 });
            Assert.Equal(HttpStatusCode.Created, created.StatusCode);

            var payload = new { CompanyName = $"DupTin {Guid.NewGuid():N}", TIN = tin, MaxUsers = 10 };
            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        #endregion

        #region READ - LISTA
        // Company list read [OK] (authenticated user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/company/all should return list for authenticated user")]
        public async Task GetAllCompanies_Authenticated_ShouldReturnList()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company/All");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var list = await response.Content.ReadFromJsonAsync<IEnumerable<Company_Detail_DTO>>();
            Assert.NotNull(list);
            Assert.True(list!.Any());
        }

        // Company list read [FAIL] (unauthenticated user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/company/all without authentication should fail")]
        public async Task GetAllCompanies_Unauthenticated_ShouldFail()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/v1/Company/All");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }
        #endregion

        #region READ - DETTAGLIO
        // Company detail read [OK] (if own)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/company should return current user's company")]        
        public async Task GetCompany_CurrentUser_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var expectedCompanyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var dto = await response.Content.ReadFromJsonAsync<Company_Detail_DTO>();
            Assert.NotNull(dto);
            Assert.Equal(expectedCompanyId, dto!.TenantID);
        }

        // Company detail read [FAIL] (company without access) - access check not implemented yet
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/company/view/{id} should fail when accessing other company")]
        public async Task GetCompanyDetails_OtherCompany_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var otherCompanyId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var token = await _factory.GenerateJwtTokenAsync(userId);

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/v1/Company/View/{otherCompanyId}");
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound);
        }

        // Company detail read [FAIL] (non-existent ID) - not implemented yet
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/view/{id} with non-existent id should fail")]
        public async Task GetCompanyDetails_NonExistentId_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var nonExistentId = Guid.NewGuid();
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "InstanceAdmin");

            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync($"/v1/Company/View/{nonExistentId}");
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }
        #endregion

        #region UPDATE
        // Company update [OK] (by an authorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/company/{id} should update company")]
        public async Task UpdateCompany_Authorized_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "InstanceAdmin");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = await CreateCompanyAsync(client, $"Update Source {Guid.NewGuid():N}");
            var payload = new { CompanyName = $"Updated {Guid.NewGuid():N}" };
            var response = await client.PutAsJsonAsync($"/v1/Company/{companyId}", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Company update [FAIL] (by an unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/company/{id} by unauthorized user should fail")]
        public async Task UpdateCompany_UnauthorizedUser_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "Employee");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var payload = new { CompanyName = "Not Allowed" };
            var response = await client.PutAsJsonAsync($"/v1/Company/{companyId}", payload);
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Company update [FAIL] (attempt to modify another company's record)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/company/{id} should fail when updating other company")]
        public async Task UpdateCompany_OtherCompany_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var otherCompanyId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var payload = new { CompanyName = "Should Not Update" };
            var response = await client.PutAsJsonAsync($"/v1/Company/{otherCompanyId}", payload);
            Assert.True(response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == HttpStatusCode.NotFound);
        }

        // Company update [FAIL] (change TIN to an existing one)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "PUT /v1/company/{id} changing TIN to duplicate should fail")]
        public async Task UpdateCompany_DuplicateTin_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "InstanceAdmin");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var sourceTin = $"IT{Guid.NewGuid():N}"[..16];
            var companyId = await CreateCompanyAsync(client, $"TIN Update Target {Guid.NewGuid():N}", tin: $"IT{Guid.NewGuid():N}"[..16]);
            _ = await CreateCompanyAsync(client, $"TIN Update Source {Guid.NewGuid():N}", tin: sourceTin);
            var payload = new { CompanyName = $"TIN Update Target Renamed {Guid.NewGuid():N}", TIN = sourceTin };
            var response = await client.PutAsJsonAsync($"/v1/Company/{companyId}", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // Company update [FAIL] (invalidating required fields)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "PUT /v1/company/{id} invalidating required fields should fail")]
        public async Task UpdateCompany_InvalidRequiredFields_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var payload = new { CompanyName = "" };
            var response = await client.PutAsJsonAsync($"/v1/Company/{companyId}", payload);
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }
        #endregion

        #region DELETE
        // Company deletion [OK] (soft delete by an authorized admin)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/company/{id} soft delete should succeed for admin")]
        public async Task DeleteCompany_Authorized_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "InstanceAdmin");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = await CreateCompanyAsync(client, $"Delete Target {Guid.NewGuid():N}");
            var response = await client.DeleteAsync($"/v1/Company/{companyId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // Company deletion [FAIL] (by an unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/company/{id} by unauthorized user should fail")]
        public async Task DeleteCompany_UnauthorizedUser_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000002");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "Employee");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var response = await client.DeleteAsync($"/v1/Company/{companyId}");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }

        // Company deletion [FAIL] (company already deleted)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "DELETE /v1/company/{id} already deleted should fail")]
        public async Task DeleteCompany_AlreadyDeleted_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "InstanceAdmin");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = await CreateCompanyAsync(client, $"Delete Twice Target {Guid.NewGuid():N}");
            var first = await client.DeleteAsync($"/v1/Company/{companyId}");
            Assert.Equal(HttpStatusCode.NoContent, first.StatusCode);
            var response = await client.DeleteAsync($"/v1/Company/{companyId}");
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.NotFound);
        }

        // Company deletion [FAIL] (company linked to active entities - e.g., users, payments)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "DELETE /v1/company/{id} with active relations should fail")]
        public async Task DeleteCompany_WithActiveRelations_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var response = await client.DeleteAsync($"/v1/Company/{companyId}");
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict);
        }
        #endregion

        #region SICUREZZA
        // Accesso API companies senza autenticazione [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/company without authentication should fail")]
        public async Task GetCompany_Unauthenticated_ShouldFail()
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync("/v1/Company");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Companies API access with expired token [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/company with expired token should fail")]
        public async Task GetCompany_ExpiredToken_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            // Token already expired (NotBefore in the past, Expires even further in the past)
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(
                _factory.Services,
                userId,
                expiresInMinutes: -5,
                notBeforeOffsetMinutes: -10);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company");
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        // Accesso API companies con ruolo insufficiente [FAIL] - non implementata policy
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company with insufficient role should fail")]
        public async Task GetCompany_InsufficientRole_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await OpenCashFlow.Test.Utilities.JwtTokenGenerator.GenerateTokenAsync(_factory.Services, userId, role: "Invalid");
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company/All");
            Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
        }
        #endregion

        #region VALIDAZIONI BUSINESS
        // Company creation con campo IsActive = false [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "BusinessRules")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/company with IsActive=false should succeed")]
        public async Task CreateCompany_IsActiveFalse_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { CompanyName = $"Inactive {Guid.NewGuid():N}", IsActive = false };
            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        // Company list filtered by IsActive [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "BusinessRules")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/all?isActive=true should filter active companies")]
        public async Task GetCompanies_FilterByIsActive_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company/All?isActive=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var companies = await response.Content.ReadFromJsonAsync<IEnumerable<Company_Detail_DTO>>();
            Assert.NotNull(companies);
            Assert.All(companies!, company => Assert.True(company.IsActive));
        }

        // Verify expired company (EndingContract in the past) [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "BusinessRules")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company should remain readable for expired self-hosted contract metadata")]
        public async Task VerifyCompanyExpiredContract_ShouldRemainReadable()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            using var db = _factory.CreateDbContext();
            var company = await db.Company_DS.FirstAsync(c => c.TenantID == tenantId);
            var originalEndingContract = company.EndingContract;

            try
            {
                company.EndingContract = DateTime.UtcNow.AddDays(-1);
                await db.SaveChangesAsync();

                var response = await client.GetAsync("/v1/Company");
                Assert.Equal(HttpStatusCode.OK, response.StatusCode);

                var dto = await response.Content.ReadFromJsonAsync<Company_Detail_DTO>();
                Assert.NotNull(dto);
                Assert.Equal(tenantId, dto!.TenantID);
                Assert.True(dto.EndingContract < DateTime.UtcNow);
            }
            finally
            {
                company.EndingContract = originalEndingContract;
                await db.SaveChangesAsync();
            }
        }

        // Company with users over MaxUsers [FAIL on user creation]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "BusinessRules")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/employee should fail when company exceeds MaxUsers")]
        public async Task CreateEmployee_ExceedsMaxUsers_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var tenantId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            using var db = _factory.CreateDbContext();
            var company = await db.Company_DS.FirstAsync(c => c.TenantID == tenantId);
            var originalMaxUsers = company.MaxUsers;
            var activeUsers = await db.Company_Staff_DS.CountAsync(s => s.TenantID == tenantId && !s.IsDeleted);

            try
            {
                company.MaxUsers = activeUsers;
                await db.SaveChangesAsync();

                var payload = new
                {
                    UserFirstName = "Limit",
                    UserLastName = "Exceeded",
                    Email = $"limit_{Guid.NewGuid():N}@example.com",
                    TmpNewPassword = "Aa!23456",
                    TmpNewPasswordRepeat = "Aa!23456"
                };
                var response = await client.PostAsJsonAsync("/v1/Employee", payload);
                Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            }
            finally
            {
                company.MaxUsers = originalMaxUsers;
                await db.SaveChangesAsync();
            }
        }
        #endregion

        #region FILTRI & QUERY
        // Company search by name [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Filtering")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/all?name=Scunio should filter by name")]
        public async Task SearchCompany_ByName_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var name = $"FilterName{Guid.NewGuid():N}";
            _ = await CreateCompanyAsync(client, name);

            var response = await client.GetAsync($"/v1/Company/All?name={Uri.EscapeDataString(name)}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var companies = await response.Content.ReadFromJsonAsync<IEnumerable<Company_Detail_DTO>>();
            Assert.NotNull(companies);
            Assert.Contains(companies!, company => company.CompanyName == name);
            Assert.All(companies!, company => Assert.Contains(name, company.CompanyName));
        }

        // Company search by VAT / TIN [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Filtering")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/all?tin=IT123 should filter by TIN")]
        public async Task SearchCompany_ByTin_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var tin = $"IT{Guid.NewGuid():N}"[..16];
            _ = await CreateCompanyAsync(client, $"FilterTin {Guid.NewGuid():N}", tin: tin);

            var response = await client.GetAsync($"/v1/Company/All?tin={Uri.EscapeDataString(tin)}");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var companies = await response.Content.ReadFromJsonAsync<IEnumerable<Company_Detail_DTO>>();
            Assert.NotNull(companies);
            Assert.Contains(companies!, company => company.NIN == tin);
            Assert.All(companies!, company => Assert.Contains(tin, company.NIN ?? string.Empty));
        }

        // Company search by estimated revenue range [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Filtering")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/all?revenueFrom=1000&revenueTo=100000 should filter by revenue range")]
        public async Task SearchCompany_ByRevenueRange_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyName = $"RevenueFilter {Guid.NewGuid():N}";
            _ = await CreateCompanyAsync(client, companyName, estimatedAnnualRevenue: 50000);

            var response = await client.GetAsync("/v1/Company/All?revenueFrom=1000&revenueTo=100000");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);

            var companies = await response.Content.ReadFromJsonAsync<IEnumerable<Company_Detail_DTO>>();
            Assert.NotNull(companies);
            Assert.Contains(companies!, company => company.CompanyName == companyName);
            Assert.All(companies!, company =>
            {
                Assert.NotNull(company.EstimatedAnnualRevenue);
                Assert.InRange(company.EstimatedAnnualRevenue.Value, 1000, 100000);
            });
        }
        #endregion

        private static async Task<Guid> CreateCompanyAsync(HttpClient client, string companyName, string? tin = null, decimal? estimatedAnnualRevenue = null)
        {
            var response = await client.PostAsJsonAsync("/v1/Company", new
            {
                CompanyName = companyName,
                TIN = tin,
                EstimatedAnnualRevenue = estimatedAnnualRevenue,
                MaxUsers = 10,
                IsActive = true
            });
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);

            var company = await response.Content.ReadFromJsonAsync<Company_Detail_DTO>();
            Assert.NotNull(company);
            return company!.TenantID;
        }

    }
}
