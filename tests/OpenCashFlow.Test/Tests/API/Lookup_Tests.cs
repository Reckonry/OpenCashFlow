using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using global::Shared.DTOs;
using System.Net;
using System.Text.Json;
using Xunit;

namespace OpenCashFlow.Test.Tests
{
    [Collection("NonParallelCollection")]
    public class LookupApiTests
    {
        private readonly CustomWebApplicationFactory _factory;

        public LookupApiTests(CustomWebApplicationFactoryFixture fixture)
        {
            _factory = fixture.Factory;
        }

        #region Payment Method
        #region CREATE
        //todo: Lookup creation [OK] (authorized user on their own company)
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/lookups should succeed when authorized user creates for own company")]
        public void CreateLookup_AuthorizedUser_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Lookup creation [FAIL] (unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/lookups should fail when unauthorized user attempts creation")]
        public void CreateLookup_UnauthorizedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup creation [FAIL] (missing required field - es: PaymentMethodName)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/lookups should fail with missing required field (e.g. PaymentMethodName)")]
        public void CreateLookup_MissingRequiredField_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup creation [OK] (same name but on a different company - should be allowed)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/lookups should fail with duplicate name in the same company")]
        public void CreateLookup_DuplicateNameInSameCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup creation [FAIL] (duplicate name in the same company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/lookups should allow identical name in different company")]
        public void CreateLookup_SameNameDifferentCompany_ShouldSucceed()
        {
            // todo: implement test
        }

        #endregion

        #region READ - LISTA
        //todo: Lookup list read [FAIL] (unauthenticated user)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should fail for unauthenticated user")]
        public void GetLookups_UnauthenticatedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup list read [OK] (only data related to the same company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should return only data for user's own company")]
        public void GetLookups_ShouldReturnDataForOwnCompany()
        {
            // todo: implement test
        }

        //todo: Lookup list read [OK] (filtered by `Visible = true`)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should return only entries with Visible = true")]
        public void GetLookups_ShouldFilterByVisibleTrue()
        {
            // todo: implement test
        }

        //todo: Lookup list read [OK] (authorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should succeed for authorized user")]
        public void GetLookups_AuthorizedUser_ShouldSucceed()
        {
            // todo: implement test
        }

        #endregion

        #region READ - DETTAGLIO
        //todo: Lookup detail read [OK] (lookup from the same company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups/{id} should return details for lookup belonging to user's company")]
        public void GetLookupDetail_OwnCompany_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Lookup detail read [FAIL] (lookup from another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups/{id} should fail when accessing lookup from another company")]
        public void GetLookupDetail_OtherCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup detail read [FAIL] (non-existent ID)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "GET /v1/lookups/{id} should fail when ID does not exist")]
        public void GetLookupDetail_NonExistentId_ShouldFail()
        {
            // todo: implement test
        }
        #endregion

        #region UPDATE
        //todo: Lookup update [OK] (authorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should succeed for authorized user")]
        public void UpdateLookup_AuthorizedUser_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Lookup update [FAIL] (unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail for unauthorized user")]
        public void UpdateLookup_UnauthorizedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup update [FAIL] (attempt on another company's lookup)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail when modifying lookup from another company")]
        public void UpdateLookup_OtherCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup update [FAIL] (update with name already existing in the same company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail with duplicate name in the same company")]
        public void UpdateLookup_DuplicateNameInSameCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup update [FAIL] (update with invalid or null fields)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail with invalid or null fields")]
        public void UpdateLookup_InvalidOrNullFields_ShouldFail()
        {
            // todo: implement test
        }
        #endregion

        #region DELETE
        //todo: Lookup deletion [OK] (soft delete da authorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/lookups/{id} should perform soft delete for authorized user")]
        public void DeleteLookup_AuthorizedUser_ShouldSoftDelete()
        {
            // todo: implement test
        }

        //todo: Lookup deletion [FAIL] (unauthorized user)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail for unauthorized user")]
        public void DeleteLookup_UnauthorizedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup deletion [FAIL] (lookup linked to active entities, e.g., payments)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail when attempting to delete lookup from another company")]
        public void DeleteLookup_OtherCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup deletion [FAIL] (lookup from another company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail if lookup is linked to active entities (e.g. payments)")]
        public void DeleteLookup_LinkedToActiveEntities_ShouldFail()
        {
            // todo: implement test
        }
        #endregion

        #region SICUREZZA
        //todo: Lookup API access senza autenticazione [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/lookups endpoint should fail for unauthenticated access")]
        public void AccessLookupApi_WithoutAuthentication_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup API access con expired token [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/lookups endpoint should fail with expired token")]
        public void AccessLookupApi_WithExpiredToken_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup API access con insufficient role [FAIL]        
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/lookups endpoint should fail with insufficient role")]
        public void AccessLookupApi_WithInsufficientRole_ShouldFail()
        {
            // todo: implement test
        }
        #endregion

        #region VALIDAZIONI BUSINESS
        //todo: Lookup creation con Visible = false [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/lookups should succeed creating lookup with Visible = false")]
        public void CreateLookup_WithVisibleFalse_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Lookup list read excludes `Visible = false` for standard users
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should exclude Visible = false entries for standard users")]
        public void GetLookups_StandardUser_ShouldExcludeVisibleFalse()
        {
            // todo: implement test
        }

        //todo: Lookup list read also shows `Visible = false` for admin users
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should include Visible = false entries for admin users")]
        public void GetLookups_AdminUser_ShouldIncludeVisibleFalse()
        {
            // todo: implement test
        }

        //todo: Lookup creation con description too long [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/lookups should fail with overly long description field")]
        public void CreateLookup_DescriptionTooLong_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lookup update setting required field to null [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail when required field is set to null")]
        public void UpdateLookup_NullRequiredField_ShouldFail()
        {
            // todo: implement test
        }
        #endregion

        #region FILTRI & QUERY
        //todo: Lookup search by name [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should filter by name")]
        public void SearchLookups_ByName_ShouldReturnResults()
        {
            // todo: implement test
        }

        //todo: Lookup search by company ID [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should filter by companyID")]
        public void SearchLookups_ByCompanyId_ShouldReturnResults()
        {
            // todo: implement test
        }

        //todo: Lookup search by `Visible` [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should filter by Visible property")]
        public void SearchLookups_ByVisible_ShouldReturnResults()
        {
            // todo: implement test
        }
        #endregion
        #endregion

//        #region Document Type
//        #region CREATE
//        //todo: Lookup creation [OK] (authorized user on their own company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "POST /v1/lookups should succeed for authorized user on own company")]
//        public async Task CreateLookup_AuthorizedUser_OwnCompany_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup creation [FAIL] (unauthorized user)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "POST /v1/lookups should fail for unauthorized user")]
//        public async Task CreateLookup_UnauthorizedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup creation [FAIL] (missing required field - es: PaymentMethodName)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "POST /v1/lookups should fail with missing required field (e.g. PaymentMethodName)")]
//        public async Task CreateLookup_MissingRequiredField_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup creation [FAIL] (duplicate name in the same company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "POST /v1/lookups should fail with duplicate name in the same company")]
//        public async Task CreateLookup_DuplicateNameInSameCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup creation [OK] (same name but on a different company - should be allowed)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "POST /v1/lookups should allow identical name in different companies")]
//        public async Task CreateLookup_SameNameDifferentCompany_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        #endregion

//        #region READ - LISTA
//        //todo: Lookup list read [OK] (authorized user)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should succeed for authorized user")]
//        public async Task GetLookups_AuthorizedUser_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup list read [FAIL] (unauthenticated user)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should fail for unauthenticated user")]
//        public async Task GetLookups_UnauthenticatedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup list read [OK] (only data related to the same company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should return only entries for user's own company")]
//        public async Task GetLookups_ShouldReturnOwnCompanyData()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup list read [OK] (filtered by `Visible = true`)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should filter entries with Visible = true")]
//        public async Task GetLookups_ShouldFilterVisibleTrue()
//        {
//            // todo: implement test
//        }

//        #endregion

//        #region READ - DETTAGLIO
//        //todo: Lookup detail read [OK] (lookup from the same company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups/{id} should return details for lookup in user's own company")]
//        public async Task GetLookupDetail_OwnCompany_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup detail read [FAIL] (lookup from another company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups/{id} should fail when accessing lookup from another company")]
//        public async Task GetLookupDetail_OtherCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup detail read [FAIL] (non-existent ID)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "GET /v1/lookups/{id} should fail when ID does not exist")]
//        public async Task GetLookupDetail_NonExistentId_ShouldFail()
//        {
//            // todo: implement test
//        }

//        #endregion

//        #region UPDATE
//        //todo: Lookup update [OK] (authorized user)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should succeed for authorized user")]
//        public async Task UpdateLookup_AuthorizedUser_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup update [FAIL] (unauthorized user)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail for unauthorized user")]
//        public async Task UpdateLookup_UnauthorizedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup update [FAIL] (attempt on another company's lookup)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail when modifying lookup from another company")]
//        public async Task UpdateLookup_OtherCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup update [FAIL] (update with name already existing in the same company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail with duplicate name in the same company")]
//        public async Task UpdateLookup_DuplicateNameInSameCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup update [FAIL] (update with invalid or null fields)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail with invalid or null fields")]
//        public async Task UpdateLookup_InvalidOrNullFields_ShouldFail()
//        {
//            // todo: implement test
//        }

//        #endregion

//        #region DELETE
//        //todo: Lookup deletion [OK] (soft delete da authorized user)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "DELETE /v1/lookups/{id} should perform soft delete for authorized user")]
//        public async Task DeleteLookup_AuthorizedUser_ShouldSoftDelete()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup deletion [FAIL] (unauthorized user)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail for unauthorized user")]
//        public async Task DeleteLookup_UnauthorizedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup deletion [FAIL] (lookup from another company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail when deleting lookup from another company")]
//        public async Task DeleteLookup_OtherCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup deletion [FAIL] (lookup linked to active entities, e.g., payments)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail if lookup is linked to active entities (e.g. payments)")]
//        public async Task DeleteLookup_LinkedToActiveEntities_ShouldFail()
//        {
//            // todo: implement test
//}

//        #endregion

//        #region SICUREZZA
//        //todo: Lookup API access senza autenticazione [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "Any /v1/lookups endpoint should fail for unauthenticated access")]
//        public async Task AccessLookupApi_WithoutAuthentication_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup API access con expired token [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "Any /v1/lookups endpoint should fail with expired token")]
//        public async Task AccessLookupApi_WithExpiredToken_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup API access con insufficient role [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "Any /v1/lookups endpoint should fail with insufficient role")]
//        public async Task AccessLookupApi_WithInsufficientRole_ShouldFail()
//        {
//            // todo: implement test
//        }

//        #endregion

//        #region VALIDAZIONI BUSINESS
//        //todo: Lookup creation con Visible = false [OK]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "POST /v1/lookups should succeed creating lookup with Visible = false")]
//        public async Task CreateLookup_WithVisibleFalse_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup list read excludes `Visible = false` for standard users
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should exclude Visible = false entries for standard users")]
//        public async Task GetLookups_StandardUser_ShouldExcludeVisibleFalse()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup list read also shows `Visible = false` for admin users
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should include Visible = false entries for admin users")]
//        public async Task GetLookups_AdminUser_ShouldIncludeVisibleFalse()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup creation con description too long [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "POST /v1/lookups should fail with overly long description field")]
//        public async Task CreateLookup_DescriptionTooLong_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup update setting required field to null [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail when required field is set to null")]
//        public async Task UpdateLookup_NullRequiredField_ShouldFail()
//        {
//            // todo: implement test
//        }

//        #endregion

//        #region FILTRI & QUERY
//        //todo: Lookup search by name [OK]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should return results filtered by name")]
//        public async Task SearchLookups_ByName_ShouldReturnResults()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup search by company ID [OK]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should return results filtered by companyID")]
//        public async Task SearchLookups_ByCompanyId_ShouldReturnResults()
//        {
//            // todo: implement test
//        }

//        //todo: Lookup search by `Visible` [OK]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should return results filtered by Visible property")]
//        public async Task SearchLookups_ByVisible_ShouldReturnResults()
//        {
//            // todo: implement test
//        }

//        #endregion
//        #endregion
        
    }
}
