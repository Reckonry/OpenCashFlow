using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Contracts.DTOs;
using System.Net;
using System.Text.Json;
using Xunit;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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
        [Fact(DisplayName = "POST /v1/company should create company", Skip = "Endpoint not implemented yet")]
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
        [Fact(DisplayName = "POST /v1/company by unauthorized user should fail", Skip = "Role/Policy not enforced yet")]
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
        [Fact(DisplayName = "POST /v1/company missing required CompanyName should fail", Skip = "Endpoint not implemented yet")]
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
        [Fact(DisplayName = "POST /v1/company with duplicate CompanyName should fail", Skip = "Endpoint not implemented yet")]
        public async Task CreateCompany_DuplicateCompanyName_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new { CompanyName = "Scunio SRL" }; // already present in seed data
            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // Company creation [FAIL] (TIN already associated with another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/company with duplicate TIN should fail", Skip = "Endpoint not implemented yet")]
        public async Task CreateCompany_DuplicateTin_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Assuming one of the seeded companies has a null TIN: this test requires an API/DB constraint.
            var payload = new { CompanyName = $"DupTin {Guid.NewGuid():N}", TIN = "IT12345678901" };
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
        [Fact(DisplayName = "GET /v1/company/view/{id} should fail when accessing other company", Skip = "Access control not implemented: endpoint returns current company ignoring route id")]
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
        [Fact(DisplayName = "GET /v1/company/view/{id} with non-existent id should fail", Skip = "Endpoint ignores route id and returns current company")]        
        public async Task GetCompanyDetails_NonExistentId_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var nonExistentId = Guid.NewGuid();
            var token = await _factory.GenerateJwtTokenAsync(userId);

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
        [Fact(DisplayName = "PUT /v1/company/{id} should update company", Skip = "Endpoint not implemented yet")]
        public async Task UpdateCompany_Authorized_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var payload = new { CompanyName = "Scunio SRL Updated" };
            var response = await client.PutAsJsonAsync($"/v1/Company/{companyId}", payload);
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Company update [FAIL] (by an unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/company/{id} by unauthorized user should fail", Skip = "Role/Policy not enforced yet")]
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
        [Fact(DisplayName = "PUT /v1/company/{id} should fail when updating other company", Skip = "Access control not implemented yet")]
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
        [Fact(DisplayName = "PUT /v1/company/{id} changing TIN to duplicate should fail", Skip = "Endpoint not implemented yet")]
        public async Task UpdateCompany_DuplicateTin_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var payload = new { TIN = "IT12345678901" };
            var response = await client.PutAsJsonAsync($"/v1/Company/{companyId}", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // Company update [FAIL] (invalidating required fields)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "PUT /v1/company/{id} invalidating required fields should fail", Skip = "Endpoint not implemented yet")]
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
        [Fact(DisplayName = "DELETE /v1/company/{id} soft delete should succeed for admin", Skip = "Endpoint not implemented yet")]
        public async Task DeleteCompany_Authorized_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var response = await client.DeleteAsync($"/v1/Company/{companyId}");
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        // Company deletion [FAIL] (by an unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/company/{id} by unauthorized user should fail", Skip = "Role/Policy not enforced yet")]
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
        [Fact(DisplayName = "DELETE /v1/company/{id} already deleted should fail", Skip = "Endpoint not implemented yet")]
        public async Task DeleteCompany_AlreadyDeleted_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var companyId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            _ = await client.DeleteAsync($"/v1/Company/{companyId}");
            var response = await client.DeleteAsync($"/v1/Company/{companyId}");
            Assert.True(response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict || response.StatusCode == HttpStatusCode.NotFound);
        }

        // Company deletion [FAIL] (company linked to active entities - e.g., users, payments)
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "DELETE /v1/company/{id} with active relations should fail", Skip = "Endpoint not implemented yet")]
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
        [Fact(DisplayName = "GET /v1/company with insufficient role should fail", Skip = "Role policy not enforced on endpoints yet")]
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
        [Fact(DisplayName = "POST /v1/company with IsActive=false should succeed", Skip = "Endpoint not implemented yet")]
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
        [Fact(DisplayName = "GET /v1/company/all?isActive=true should filter active companies", Skip = "Filtering not implemented yet")]
        public async Task GetCompanies_FilterByIsActive_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company/All?isActive=true");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Verify expired company (EndingContract in the past) [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "BusinessRules")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company should reflect expired contract state", Skip = "No explicit expired state exposure implemented")]
        public async Task VerifyCompanyExpiredContract_ShouldBeRecognized()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Company with users over MaxUsers [FAIL on user creation]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "BusinessRules")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/employee should fail when company exceeds MaxUsers", Skip = "MaxUsers enforcement not implemented in EmployeeService")]
        public async Task CreateEmployee_ExceedsMaxUsers_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Here you would expect a 409 or 400 if MaxUsers is exceeded
            var payload = new { UserName = $"user_{Guid.NewGuid():N}", Email = $"u_{Guid.NewGuid():N}@ex.com", TmpNewPassword = "Aa!23456" };
            var response = await client.PostAsJsonAsync("/v1/Employee", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        #endregion

        #region FILTRI & QUERY
        // Company search by name [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Filtering")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/all?name=Scunio should filter by name", Skip = "Filtering not implemented yet")]
        public async Task SearchCompany_ByName_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company/All?name=Scunio");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Company search by VAT / TIN [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Filtering")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/all?tin=IT123 should filter by TIN", Skip = "Filtering not implemented yet")]
        public async Task SearchCompany_ByTin_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company/All?tin=IT123");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        // Company search by estimated revenue range [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Filtering")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/company/all?revenueFrom=1000&revenueTo=100000 should filter by revenue range", Skip = "Filtering not implemented yet")]
        public async Task SearchCompany_ByRevenueRange_ShouldSucceed()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            var token = await _factory.GenerateJwtTokenAsync(userId);
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync("/v1/Company/All?revenueFrom=1000&revenueTo=100000");
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
        #endregion

    }
}
