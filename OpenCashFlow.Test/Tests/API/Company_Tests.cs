using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using global::Shared.DTOs;
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
        // Creazione company [OK]
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

        // Creazione company [FAIL] (da utente NON autorizzato)
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

        // Creazione company [FAIL] (campo obbligatorio mancante - es: CompanyName)
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

        // Creazione company [FAIL] (CompanyName già esistente)
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

            var payload = new { CompanyName = "Scunio SRL" }; // già presente nei seed
            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }

        // Creazione company [FAIL] (TIN già associato a un’altra company)
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

            // Ammesso che una delle aziende seed abbia TIN null: questo test richiede endpoint e vincolo lato API/DB
            var payload = new { CompanyName = $"DupTin {Guid.NewGuid():N}", TIN = "IT12345678901" };
            var response = await client.PostAsJsonAsync("/v1/Company", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        #endregion

        #region READ - LISTA
        // Lettura lista companies [OK] (utente autenticato)
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

        // Lettura lista companies [FAIL] (utente non autenticato)
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
        // Lettura dettaglio company [OK] (se propria)
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

        // Lettura dettaglio company [FAIL] (company di cui non si ha accesso) - non ancora implementato il controllo
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

        // Lettura dettaglio company [FAIL] (ID non esistente) - non ancora implementato
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
        // Modifica dati company [OK] (da utente autorizzato)
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

        // Modifica dati company [FAIL] (da utente NON autorizzato)
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

        // Modifica dati company [FAIL] (tentativo di modificare una company altrui)
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

        // Modifica dati company [FAIL] (modifica TIN in uno già esistente)
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

        // Modifica dati company [FAIL] (invalidazione campi obbligatori)
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
        // Eliminazione company [OK] (soft delete da admin autorizzato)
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

        // Eliminazione company [FAIL] (da utente non autorizzato)
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

        // Eliminazione company [FAIL] (company già eliminata)
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

        // Eliminazione company [FAIL] (company collegata ad entità attive - es: utenti, pagamenti)
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

        // Accesso API companies con token scaduto [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/company with expired token should fail")]
        public async Task GetCompany_ExpiredToken_ShouldFail()
        {
            var userId = Guid.Parse("00000000-0000-0000-0000-000000000001");
            // Token già scaduto (NotBefore nel passato, Expires ancora più nel passato)
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
        // Creazione company con campo IsActive = false [OK]
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

        // Lettura companies filtrate per IsActive [OK]
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

        // Verifica company scaduta (EndingContract nel passato) [OK]
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

        // Company con utenti oltre MaxUsers [FAIL su creazione utente]
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

            // Qui ci si aspetterebbe un 409 o 400 se superato MaxUsers
            var payload = new { UserName = $"user_{Guid.NewGuid():N}", Email = $"u_{Guid.NewGuid():N}@ex.com", TmpNewPassword = "Aa!23456" };
            var response = await client.PostAsJsonAsync("/v1/Employee", payload);
            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        }
        #endregion

        #region FILTRI & QUERY
        // Ricerca company per nome [OK]
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

        // Ricerca company per partita IVA / TIN [OK]
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

        // Ricerca company per range di fatturato stimato [OK]
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
