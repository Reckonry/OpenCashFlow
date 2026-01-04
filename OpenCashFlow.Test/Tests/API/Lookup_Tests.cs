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
        //todo: Creazione lookup [OK] (utente autorizzato sulla propria company)
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/lookups should succeed when authorized user creates for own company")]
        public void CreateLookup_AuthorizedUser_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Creazione lookup [FAIL] (utente NON autorizzato)
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

        //todo: Creazione lookup [FAIL] (campo obbligatorio mancante - es: PaymentMethodName)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/lookups should fail with missing required field (e.g. PaymentMethodName)")]
        public void CreateLookup_MissingRequiredField_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Creazione lookup [OK] (nome identico ma su company diversa - deve essere consentito)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/lookups should fail with duplicate name in the same company")]
        public void CreateLookup_DuplicateNameInSameCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Creazione lookup [FAIL] (nome duplicato nella stessa company)
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
        //todo: Lettura lista lookup [FAIL] (utente NON autenticato)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should fail for unauthenticated user")]
        public void GetLookups_UnauthenticatedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lettura lista lookup [OK] (solo dati relativi alla propria company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should return only data for user's own company")]
        public void GetLookups_ShouldReturnDataForOwnCompany()
        {
            // todo: implement test
        }

        //todo: Lettura lista lookup [OK] (filtrati per `Visible = true`)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should return only entries with Visible = true")]
        public void GetLookups_ShouldFilterByVisibleTrue()
        {
            // todo: implement test
        }

        //todo: Lettura lista lookup [OK] (utente autorizzato)
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
        //todo: Lettura dettaglio lookup [OK] (lookup della propria company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups/{id} should return details for lookup belonging to user's company")]
        public void GetLookupDetail_OwnCompany_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Lettura dettaglio lookup [FAIL] (lookup di altra company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups/{id} should fail when accessing lookup from another company")]
        public void GetLookupDetail_OtherCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Lettura dettaglio lookup [FAIL] (ID non esistente)
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
        //todo: Modifica lookup [OK] (utente autorizzato)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should succeed for authorized user")]
        public void UpdateLookup_AuthorizedUser_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Modifica lookup [FAIL] (utente non autorizzato)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail for unauthorized user")]
        public void UpdateLookup_UnauthorizedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Modifica lookup [FAIL] (tentativo su lookup di altra company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail when modifying lookup from another company")]
        public void UpdateLookup_OtherCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Modifica lookup [FAIL] (modifica con nome già esistente nella stessa company)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail with duplicate name in the same company")]
        public void UpdateLookup_DuplicateNameInSameCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Modifica lookup [FAIL] (modifica con campi invalidi o nulli)
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
        //todo: Eliminazione lookup [OK] (soft delete da utente autorizzato)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/lookups/{id} should perform soft delete for authorized user")]
        public void DeleteLookup_AuthorizedUser_ShouldSoftDelete()
        {
            // todo: implement test
        }

        //todo: Eliminazione lookup [FAIL] (utente non autorizzato)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail for unauthorized user")]
        public void DeleteLookup_UnauthorizedUser_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Eliminazione lookup [FAIL] (lookup collegato a entità attive, es: pagamenti)
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail when attempting to delete lookup from another company")]
        public void DeleteLookup_OtherCompany_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Eliminazione lookup [FAIL] (lookup di altra company)
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
        //todo: Accesso API lookup senza autenticazione [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/lookups endpoint should fail for unauthenticated access")]
        public void AccessLookupApi_WithoutAuthentication_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Accesso API lookup con token scaduto [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Security")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Any /v1/lookups endpoint should fail with expired token")]
        public void AccessLookupApi_WithExpiredToken_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Accesso API lookup con ruolo insufficiente [FAIL]        
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
        //todo: Creazione lookup con Visible = false [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "POST /v1/lookups should succeed creating lookup with Visible = false")]
        public void CreateLookup_WithVisibleFalse_ShouldSucceed()
        {
            // todo: implement test
        }

        //todo: Lettura lista lookup esclude `Visible = false` per utenti standard
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should exclude Visible = false entries for standard users")]
        public void GetLookups_StandardUser_ShouldExcludeVisibleFalse()
        {
            // todo: implement test
        }

        //todo: Lettura lista lookup mostra anche `Visible = false` per utenti admin
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should include Visible = false entries for admin users")]
        public void GetLookups_AdminUser_ShouldIncludeVisibleFalse()
        {
            // todo: implement test
        }

        //todo: Creazione lookup con descrizione troppo lunga [FAIL]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "POST /v1/lookups should fail with overly long description field")]
        public void CreateLookup_DescriptionTooLong_ShouldFail()
        {
            // todo: implement test
        }

        //todo: Modifica lookup impostando campo obbligatorio a null [FAIL]
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
        //todo: Ricerca lookup per nome [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should filter by name")]
        public void SearchLookups_ByName_ShouldReturnResults()
        {
            // todo: implement test
        }

        //todo: Ricerca lookup per companyID [OK]
        [Trait("Layer", "API")]
        [Trait("Feature", "LookupManagement")]
        [Trait("Type", "Integration")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "GET /v1/lookups should filter by companyID")]
        public void SearchLookups_ByCompanyId_ShouldReturnResults()
        {
            // todo: implement test
        }

        //todo: Ricerca lookup per `Visible` [OK]
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
//        //todo: Creazione lookup [OK] (utente autorizzato sulla propria company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "POST /v1/lookups should succeed for authorized user on own company")]
//        public async Task CreateLookup_AuthorizedUser_OwnCompany_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Creazione lookup [FAIL] (utente NON autorizzato)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "POST /v1/lookups should fail for unauthorized user")]
//        public async Task CreateLookup_UnauthorizedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Creazione lookup [FAIL] (campo obbligatorio mancante - es: PaymentMethodName)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "POST /v1/lookups should fail with missing required field (e.g. PaymentMethodName)")]
//        public async Task CreateLookup_MissingRequiredField_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Creazione lookup [FAIL] (nome duplicato nella stessa company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "POST /v1/lookups should fail with duplicate name in the same company")]
//        public async Task CreateLookup_DuplicateNameInSameCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Creazione lookup [OK] (nome identico ma su company diversa - deve essere consentito)
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
//        //todo: Lettura lista lookup [OK] (utente autorizzato)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should succeed for authorized user")]
//        public async Task GetLookups_AuthorizedUser_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lettura lista lookup [FAIL] (utente NON autenticato)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should fail for unauthenticated user")]
//        public async Task GetLookups_UnauthenticatedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lettura lista lookup [OK] (solo dati relativi alla propria company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should return only entries for user's own company")]
//        public async Task GetLookups_ShouldReturnOwnCompanyData()
//        {
//            // todo: implement test
//        }

//        //todo: Lettura lista lookup [OK] (filtrati per `Visible = true`)
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
//        //todo: Lettura dettaglio lookup [OK] (lookup della propria company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups/{id} should return details for lookup in user's own company")]
//        public async Task GetLookupDetail_OwnCompany_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lettura dettaglio lookup [FAIL] (lookup di altra company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups/{id} should fail when accessing lookup from another company")]
//        public async Task GetLookupDetail_OtherCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Lettura dettaglio lookup [FAIL] (ID non esistente)
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
//        //todo: Modifica lookup [OK] (utente autorizzato)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should succeed for authorized user")]
//        public async Task UpdateLookup_AuthorizedUser_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Modifica lookup [FAIL] (utente non autorizzato)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail for unauthorized user")]
//        public async Task UpdateLookup_UnauthorizedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Modifica lookup [FAIL] (tentativo su lookup di altra company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail when modifying lookup from another company")]
//        public async Task UpdateLookup_OtherCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Modifica lookup [FAIL] (modifica con nome già esistente nella stessa company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "PUT /v1/lookups/{id} should fail with duplicate name in the same company")]
//        public async Task UpdateLookup_DuplicateNameInSameCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Modifica lookup [FAIL] (modifica con campi invalidi o nulli)
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
//        //todo: Eliminazione lookup [OK] (soft delete da utente autorizzato)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "DELETE /v1/lookups/{id} should perform soft delete for authorized user")]
//        public async Task DeleteLookup_AuthorizedUser_ShouldSoftDelete()
//        {
//            // todo: implement test
//        }

//        //todo: Eliminazione lookup [FAIL] (utente non autorizzato)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail for unauthorized user")]
//        public async Task DeleteLookup_UnauthorizedUser_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Eliminazione lookup [FAIL] (lookup di altra company)
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "DELETE /v1/lookups/{id} should fail when deleting lookup from another company")]
//        public async Task DeleteLookup_OtherCompany_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Eliminazione lookup [FAIL] (lookup collegato a entità attive, es: pagamenti)
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
//        //todo: Accesso API lookup senza autenticazione [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "Any /v1/lookups endpoint should fail for unauthenticated access")]
//        public async Task AccessLookupApi_WithoutAuthentication_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Accesso API lookup con token scaduto [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Security")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "Any /v1/lookups endpoint should fail with expired token")]
//        public async Task AccessLookupApi_WithExpiredToken_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Accesso API lookup con ruolo insufficiente [FAIL]
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
//        //todo: Creazione lookup con Visible = false [OK]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "POST /v1/lookups should succeed creating lookup with Visible = false")]
//        public async Task CreateLookup_WithVisibleFalse_ShouldSucceed()
//        {
//            // todo: implement test
//        }

//        //todo: Lettura lista lookup esclude `Visible = false` per utenti standard
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should exclude Visible = false entries for standard users")]
//        public async Task GetLookups_StandardUser_ShouldExcludeVisibleFalse()
//        {
//            // todo: implement test
//        }

//        //todo: Lettura lista lookup mostra anche `Visible = false` per utenti admin
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should include Visible = false entries for admin users")]
//        public async Task GetLookups_AdminUser_ShouldIncludeVisibleFalse()
//        {
//            // todo: implement test
//        }

//        //todo: Creazione lookup con descrizione troppo lunga [FAIL]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Validation")]
//        [Trait("Priority", "Medium")]
//        [Fact(DisplayName = "POST /v1/lookups should fail with overly long description field")]
//        public async Task CreateLookup_DescriptionTooLong_ShouldFail()
//        {
//            // todo: implement test
//        }

//        //todo: Modifica lookup impostando campo obbligatorio a null [FAIL]
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
//        //todo: Ricerca lookup per nome [OK]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should return results filtered by name")]
//        public async Task SearchLookups_ByName_ShouldReturnResults()
//        {
//            // todo: implement test
//        }

//        //todo: Ricerca lookup per companyID [OK]
//        [Trait("Layer", "API")]
//        [Trait("Feature", "LookupManagement")]
//        [Trait("Type", "Integration")]
//        [Trait("Priority", "High")]
//        [Fact(DisplayName = "GET /v1/lookups should return results filtered by companyID")]
//        public async Task SearchLookups_ByCompanyId_ShouldReturnResults()
//        {
//            // todo: implement test
//        }

//        //todo: Ricerca lookup per `Visible` [OK]
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
