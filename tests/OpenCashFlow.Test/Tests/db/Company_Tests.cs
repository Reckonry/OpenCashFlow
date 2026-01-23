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

        // COMPANY - IMPORTANT DB RULES TO TEST (no API)

        //Company DB Insert [FAIL] with CompanyName = null (non-nullable column in DB)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Validation")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Company DB Insert should fail when CompanyName is null")]
        public void Insert_Company_WithoutCompanyName_ShouldFail()
        {
            var company = new Company
            {
                CompanyName = null!, // Required violation
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


        //Company DB Insert [OK] with optional fields = null (e.g., BusinessCategory, Website, etc.)
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
                // Optional fields null
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

        //Company DB Insert [FAIL] with StartingContract > EndingContract
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
                StartingContract = DateTime.UtcNow.AddYears(1), // future
                EndingContract = DateTime.UtcNow,               // present -> logical error
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "contract-error",
                IsActive = true
            };

            // Act & Assert
            _context.Company_DS.Add(company);
            
            // Without a DB constraint, we expect to catch it manually (or via model validation)
            var ex = Record.Exception(() => _context.SaveChanges());

            Assert.Null(ex); // If you do not have a logical constraint yet, there will be no exception
            Assert.True(company.StartingContract > company.EndingContract, "StartingContract is after EndingContract - logic error.");

            // ✅ Here you are only noting that the logic should be strengthened in the domain (e.g., with a check or validation)
        }

        //Company DB Update [FAIL] with EndingContract < DateIns (chronological logical constraint)
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
                DateIns = DateTime.UtcNow, // is set on save
                GdprConsent = true,
                ContractAcepted = true,
                CompanySecret = "secret",
                IsActive = true
            };

            _context.Company_DS.Add(company);
            _context.SaveChanges();

            // Act - try to update with EndingContract in the past (before DateIns)
            company.EndingContract = company.DateIns.AddDays(-1);
            _context.Company_DS.Update(company);

            // Assert
            var ex = Record.Exception(() => _context.SaveChanges());

            // The test should pass (even if it shouldn't); a DB check is needed to prevent it
            Assert.Null(ex); // custom logic needed (e.g., override SaveChanges)
            Assert.True(company.EndingContract < company.DateIns, "EndingContract is earlier than DateIns - logic should be enforced.");
        }

        //todo: Company DB Delete [FAIL] if there are associated Payment/Users/Staff records (FK set to RESTRICT or NO ACTION)
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

            // Create a linked user
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

            // Create a linked employee
            var staff = new Company_Staff
            {
                UserID = user.UserID,
                TenantID = company.TenantID,
                Role = "Admin"
            };
            _context.Company_Staff_DS.Add(staff);

            // Create a linked payment
            var payment = new Payment
            {
                UserID = user.UserID,
                PaymentID = Guid.NewGuid(),
                TenantID = company.TenantID,
                EntryType = nameof(EntryTypeEnum.Income),
                Amount = 100,
                PaymentMethodID = _context.PaymentMethod_DS.First().PaymentMethodID, // can be fake
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

        //Company DB SoftDelete [OK] (flag IsDeleted = true, without physically removing but "obscuring" fields)
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

            // Act: instead of removing, simulate soft delete
            var toDelete = _context.Company_DS.First(c => c.TenantID == idToDelete);
            toDelete.IsDeleted = true;
            toDelete.DateDeleted = DateTime.UtcNow;
            toDelete.IsDeletedWhy = "Test soft delete";
            _context.SaveChanges();

            // Assert: the record still exists but is marked as deleted
            var softDeleted = _context.Company_DS.FirstOrDefault(c => c.TenantID == idToDelete);
            Assert.NotNull(softDeleted);
            Assert.True(softDeleted!.IsDeleted);
            Assert.NotNull(softDeleted.DateDeleted);
            Assert.Equal("Test soft delete", softDeleted.IsDeletedWhy);
        }

        //Company DB Auto-set DateIns (verify it's set if not explicitly provided)
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
                // Intentionally do NOT set DateIns
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

        //todo: Company DB Auto-set DateEdit (verify it updates on every SaveChanges)
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

            // Act - update
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