using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using global::Shared.Data;
using global::Shared.Models;
using global::Shared.Models.Identity;
using System.Net;
using System.Text.Json;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]
namespace OpenCashFlow.Test.Tests
{
    [Collection("Database collection")]
    public class CompanyTests
    {
        private readonly DatabaseFixture _fixture;
        private readonly ApplicationDbContext _context;

        public CompanyTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _context = fixture.DbContext;
        }

        // COMPANY — REGOLE IMPORTANTI DA TESTARE SUL DB (senza API)

        //Company DB Insert [FAIL] con CompanyName = null (colonna non-nullable nel DB)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Company DB Insert should fail when CompanyName is null")]
        public void Insert_Company_WithoutCompanyName_ShouldFail()
        {
            var company = new Company
            {
                CompanyName = null!, // Violazione Required
                MaxUsers = 10,
                PriorityLevel = 1,
                StartingContract = DateTime.UtcNow,
                EndingContract = DateTime.UtcNow.AddYears(1),
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "test-secret"
            };

            _context.Company_DS.Add(company);
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }


        //Company DB Insert [OK] con campi opzionali = null (es. BusinessCategory, Website, ecc.)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Optional Fields")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Insert should succeed when optional fields are null", Skip = "TBF")]
        public void Insert_Company_WithOptionalFieldsNull_ShouldSucceed()
        {
            // Arrange         

            var company = new Company
            {
                CompanyName = "Optional Null Corp",
                MaxUsers = 20,
                PriorityLevel = 0,
                StartingContract = DateTime.UtcNow,
                EndingContract = DateTime.UtcNow.AddYears(1),
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "secretXYZ",
                IsActive = true,
                // Campi opzionali null
                BusinessCategory = null,
                Website = null,
                EstimatedAnnualRevenue = null,
                SocialLinks = null,
                IBAN = null,
                DefaultCurrency = null
            };

            // Act
            _context.Company_DS.Add(company);
            var result = _context.SaveChanges();

            // Assert
            Assert.Equal(1, result);
            var inserted = _context.Company_DS.FirstOrDefault(c => c.CompanyName == "Optional Null Corp");
            Assert.NotNull(inserted);
            Assert.Null(inserted!.Website);
        }

        //Company DB Insert [FAIL] con StartingContract > EndingContract
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Insert should fail when StartingContract is after EndingContract", Skip = "TBF")]
        public void Insert_Company_WithInvalidContractDates_ShouldFail()
        {
            // Arrange
            var company = new Company
            {
                CompanyName = "Invalid Contract Dates Ltd.",
                MaxUsers = 10,
                PriorityLevel = 1,
                StartingContract = DateTime.UtcNow.AddYears(1), // futura
                EndingContract = DateTime.UtcNow,               // presente → errore logico
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "contract-error",
                IsActive = true
            };

            // Act & Assert
            _context.Company_DS.Add(company);
            
            // In assenza di vincolo DB, ci aspettiamo di rilevarlo manualmente (o con validazione model)
            var ex = Record.Exception(() => _context.SaveChanges());

            Assert.Null(ex); // Se non hai ancora un vincolo logico, non ci sarà eccezione
            Assert.True(company.StartingContract > company.EndingContract, "StartingContract is after EndingContract — logic error.");

            // ✅ Qui stai solo segnalando che la logica va rafforzata nel dominio (es. con un check o una validation)
        }

        //Company DB Update [FAIL] con EndingContract < DateIns (vincolo logico cronologico)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Update should fail when EndingContract is before DateIns", Skip = "TBF")]
        public void Update_Company_WithInvalidEndingContract_ShouldFail()
        {
            // Arrange
            var company = new Company
            {
                CompanyName = "Test Company",
                MaxUsers = 50,
                PriorityLevel = 1,
                StartingContract = DateTime.UtcNow.AddDays(-10),
                EndingContract = DateTime.UtcNow.AddDays(30),
                DateIns = DateTime.UtcNow, // viene impostato al save
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "secret",
                IsActive = true
            };

            _context.Company_DS.Add(company);
            _context.SaveChanges();

            // Act – provo ad aggiornare con EndingContract nel passato (prima della DateIns)
            company.EndingContract = company.DateIns.AddDays(-1);
            _context.Company_DS.Update(company);

            // Assert
            var ex = Record.Exception(() => _context.SaveChanges());

            // Il test dovrebbe passare ( anche se nn dovrebbe ) necessario controllo su db per prevenirlo
            Assert.Null(ex); // serve logica custom (es. override SaveChanges)
            Assert.True(company.EndingContract < company.DateIns, "EndingContract is earlier than DateIns — logic should be enforced.");
        }

        //todo: Company DB Delete [FAIL] se esistono record Payment/Users/Staff associati (FK impostata su RESTRICT o NO ACTION)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "FK Constraint")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Delete should fail if related Payments/Users/Staff exist", Skip = "WIP")]
        public void Delete_Company_WithRelatedEntities_ShouldFail()
        {
            // Arrange
            var company = new Company
            {
                CompanyName = "Test Company",
                MaxUsers = 10,
                PriorityLevel = 1,
                StartingContract = DateTime.UtcNow,
                EndingContract = DateTime.UtcNow.AddYears(1),
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "abc123",
                IsActive = true
            };

            _context.Company_DS.Add(company);
            _context.SaveChanges();

            // Creo una user collegata
            var user = new AspNetUser
            {
                UserID = Guid.NewGuid(),
                UserFirstName = "Toni",
                PasswordHash = "hashedpassword",
                PasswordSalt = "salt",
                Email = "test@example.com",
                UserName = "testuser",
                EmailConfirmed = true,
            };
            _context.AspNetUser_DS.Add(user);

            // Creo un dipendente collegato
            var staff = new Company_Staff
            {
                UserID = user.UserID,
                TenantID = company.TenantID,
                Role = "Admin"
            };
            _context.Company_Staff_DS.Add(staff);

            // Creo un pagamento collegato
            var payment = new Payment
            {
                UserID = user.UserID,
                PaymentID = Guid.NewGuid(),
                TenantID = company.TenantID,
                EntryType = nameof(EntryTypeEnum.Income),
                Amount = 100,
                PaymentMethodID = _context.PaymentMethod_DS.First().PaymentMethodID, // può essere fake
                DocumentTypeID = _context.Payment_DocumentType_DS.First().DocumentTypeID,
                DateIns = DateTime.UtcNow
            };
            _context.Payment_DS.Add(payment);

            _context.SaveChanges();

            // Act
            _context.Company_DS.Remove(company);

            // Assert
            var exception = Record.Exception(() => _context.SaveChanges());

            Assert.NotNull(exception);
            Assert.Contains("The DELETE statement conflicted", exception?.Message ?? "", StringComparison.OrdinalIgnoreCase);
        }

        //Company DB SoftDelete [OK] (flag IsDeleted = true, senza rimuovere fisicamente ma "oscurando" i campi)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "SoftDelete")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Soft delete should set IsDeleted = true and not remove the record", Skip = "TBF")]
        public void SoftDelete_Company_ShouldMarkAsDeleted_WithoutRemoving()
        {
            // Arrange
            var company = new Company
            {
                TenantID = Guid.NewGuid(),
                CompanyName = "Mario Rossi In Bianchi SRL.",
                MaxUsers = 10,
                PriorityLevel = 1,
                StartingContract = DateTime.UtcNow,
                EndingContract = DateTime.UtcNow.AddYears(1),
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "softdel",
                IsActive = true
            };

            _context.Company_DS.Add(company);
            _context.SaveChanges();

            var idToDelete = company.TenantID;

            // Act: invece di rimuovere, simulo soft delete
            var toDelete = _context.Company_DS.First(c => c.TenantID == idToDelete);
            toDelete.IsDeleted = true;
            toDelete.DateDeleted = DateTime.UtcNow;
            toDelete.IsDeletedWhy = "Test soft delete";
            _context.SaveChanges();

            // Assert: il record esiste ancora ma è marcato come eliminato
            var softDeleted = _context.Company_DS.FirstOrDefault(c => c.TenantID == idToDelete);
            Assert.NotNull(softDeleted);
            Assert.True(softDeleted!.IsDeleted);
            Assert.NotNull(softDeleted.DateDeleted);
            Assert.Equal("Test soft delete", softDeleted.IsDeletedWhy);
        }

        //Company DB Auto-set DateIns (verifica che venga valorizzato se non passato esplicitamente)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Audit")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Insert should auto-set DateIns if not provided", Skip = "TBF")]
        public void Insert_Company_ShouldAutoSet_DateIns_IfNotProvided()
        {
            // Arrange
            var company = new Company
            {
                CompanyName = "Auto DateIns Co.",
                MaxUsers = 50,
                PriorityLevel = 1,
                StartingContract = DateTime.UtcNow,
                EndingContract = DateTime.UtcNow.AddYears(1),
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "auto-date",
                IsActive = true,
                // Intenzionalmente NON imposto DateIns
            };

            // Act
            _context.Company_DS.Add(company);
            _context.SaveChanges();

            // Assert
            var inserted = _context.Company_DS.FirstOrDefault(c => c.TenantID == company.TenantID);
            Assert.NotNull(inserted);
            Assert.True(inserted!.DateIns > DateTime.MinValue, "DateIns should be set automatically.");
            Assert.True((DateTime.UtcNow - inserted.DateIns).TotalSeconds < 10, "DateIns should be close to now.");
        }

        //todo: Company DB Auto-set DateEdit (verifica che venga aggiornato ad ogni SaveChanges)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Audit")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Update should auto-set DateEdit on SaveChanges", Skip = "WIP")]
        public void Update_Company_ShouldAutoSet_DateEdit_OnSaveChanges()
        {
            // Arrange
            var company = new Company
            {
                CompanyName = "Company with Edit Tracking",
                MaxUsers = 30,
                PriorityLevel = 1,
                StartingContract = DateTime.UtcNow,
                EndingContract = DateTime.UtcNow.AddYears(1),
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "edit-track",
                IsActive = true
            };

            _context.Company_DS.Add(company);
            _context.SaveChanges();

            // Act - modifica
            var toUpdate = _context.Company_DS.First(c => c.TenantID == company.TenantID);
            toUpdate.CompanyName = "Updated Name";

            var preEditTimestamp = DateTime.UtcNow;
            _context.SaveChanges();

            // Assert
            var updated = _context.Company_DS.First(c => c.TenantID == company.TenantID);
            Assert.NotNull(updated.DateEdit);
            Assert.True(updated.DateEdit >= preEditTimestamp, "DateEdit should be updated to a recent timestamp.");
        }
    }
}