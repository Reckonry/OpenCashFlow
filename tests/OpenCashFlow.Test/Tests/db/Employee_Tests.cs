using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.Models;
using global::Shared.Models.Identity;
using System.Net;
using System.Text.Json;
using Xunit;

namespace OpenCashFlow.Test.Tests
{
    [CollectionDefinition("Database collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
    }

    [Collection("Database collection")]
    public class EmployeeTests
    {
        private readonly DatabaseFixture _fixture;
        private readonly ApplicationDbContext _context;

        public EmployeeTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _context = fixture.DbContext;
        }

        // EMPLOYEE (Company_Staff) - IMPORTANT DB RULES (no API)

        //todo: Employee Insert [FAIL] with missing or null UserID (required FK)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Insert Staff FAIL when UserID is missing (FK required)", Skip = "TBF")]
        public void InsertStaff_Fails_WhenUserIDIsMissing()
        {
            // Arrange
            var existingCompanyId = _context.Company_DS.First().TenantID;

            var staff = new Company_Staff
            {
                TenantID = existingCompanyId,
                // UserID = Guid.Empty, // <-- intentionally not set
                TimeCost = 25.00,
                Role = "Saltimbanco"
            };

            // Act & Assert
            Assert.Throws<DbUpdateException>(() =>
            {
                _context.Company_Staff_DS.Add(staff);
                _context.SaveChanges();
            });
        }

        //todo: Employee Insert [FAIL] with missing or null TenantID (required FK)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Insert Staff FAIL when TenantID is missing (FK required)", Skip = "TO BE FIXED")]
        public void InsertStaff_Fails_WhenTenantIDIsMissing()
        {
            // Arrange
            var existingUserId = _context.AspNetUser_DS.First().UserID;

            var aspNetUser = new AspNetUser
            {
                UserID = Guid.NewGuid(),
                Email = "test@boobs.com",
                UserFirstName = "Bobberella",
                PasswordHash = "hash",
                PasswordSalt = "salt",
                UserName = "Phil Settore marketing"
            };

            _context.AspNetUser_DS.Add(aspNetUser);
            _context.SaveChanges();

            var staff = new Company_Staff
            {
                // TenantID = Guid.Empty, // <-- intentionally not set
                UserID = aspNetUser.UserID,
                TimeCost = 25.00,
                Role = "Comunista"
            };

            // Act & Assert
            Assert.Throws<DbUpdateException>(() =>
            {
                _context.Company_Staff_DS.Add(staff);
                _context.SaveChanges();
            });
        }

        //todo: Employee Insert [FAIL] with User not present in DB (FK violation)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Constraint")]
        [Trait("Priority", "Medium")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Insert Staff FAIL when User does not exist in DB (FK violation)", Skip = "TO BE FIXED")]
        public void InsertStaff_Fails_WhenUserDoesNotExistInDb()
        {
            // Arrange
            var existingCompanyId = _context.Company_DS.First().TenantID;

            var staff = new Company_Staff
            {
                TenantID = existingCompanyId,
                UserID = Guid.NewGuid(), // non-existent FK
                Role = "Tecnico scaldabagni"
            };

            // Act & Assert
            Assert.Throws<DbUpdateException>(() =>
            {
                _context.Company_Staff_DS.Add(staff);
                _context.SaveChanges();
            });
        }


        //todo: Employee Insert [OK] with IsManager = true (verify boolean persistence)
        // Employee Insert [OK] with DateIns set correctly (manual or DB default)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Persistence")]
        [Trait("Priority", "Medium")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Employee Insert [OK] with DateIns set manually or via default", Skip = "TBF")]
        public void Insert_Employee_Success_WithValidDateIns()
        {
            // Arrange
            var company = _context.Company_DS.FirstOrDefault();
            Assert.NotNull(company);

            var employee = new Company_Staff
            {
                UserID = Guid.NewGuid(), // New UserID for the test
                TenantID = company!.TenantID,
                TimeCost = 25.5,
                RequireShiftCheckIn = true,
                OutOfReports = false,
            };

            // Act
            _context.Company_Staff_DS.Add(employee);
            _context.SaveChanges();

            // Assert
            var inserted = _context.Company_Staff_DS.FirstOrDefault(e => e.UserID == employee.UserID);
            Assert.NotNull(inserted);
            Assert.True(inserted!.DateIns != default); // can be manual or assigned by the constructor
            Assert.True((inserted.DateIns - DateTime.UtcNow).TotalSeconds < 3); // accepts +/-3s max delta
        }

        //todo: Employee Insert [FAIL] with null roles

        //todo: Employee Update [OK] changes Role, IsManager, Active flag
        

        //todo: Employee Delete [FAIL] if used in Payment (active FK constraint)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Constraint")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Employee Delete [FAIL] if referenced in Payment (FK constraint)", Skip = "WIP")]
        public void Delete_Employee_Fail_WhenReferencedInOtherEntities()
        {
            // Arrange
            
            // Simulate an entity that uses the FK to staff => Payment
            var payment = new Payment
            {
                TenantID = _context.Company_DS.FirstOrDefault()!.TenantID,
                UserID = _context.AspNetUser_DS.FirstOrDefault()!.UserID,
                Amount = 100,
                EntryType = "IN",
                PaymentMethodID = _context.PaymentMethod_DS.First().PaymentMethodID,
                DocumentTypeID = _context.Payment_DocumentType_DS.First().DocumentTypeID,
                DateIns = DateTime.UtcNow
            };

            _context.Payment_DS.Add(payment);
            _context.SaveChanges();

            Company_Staff staff = _context.Company_Staff_DS.FirstOrDefault(x => x.UserID == _context.AspNetUser_DS.FirstOrDefault()!.UserID)!;

            // Act + Assert
            _context.Company_Staff_DS.Remove(staff!);
            Assert.Throws<DbUpdateException>(() =>
            {
                _context.SaveChanges();
            });
        }

        //todo: Employee SoftDelete [OK] sets IsDeleted = true without removing, obfuscating fields
        

        //todo: Employee Insert [FAIL] with existing (UserID, TenantID) combination (unique constraint)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Constraint")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Employee Insert [FAIL] when (UserID, TenantID) combination already exists", Skip = "WIP")]
        public void Insert_Employee_Fail_WhenDuplicateUserCompanyCombination()
        {
            // Arrange
            var user = _context.AspNetUser_DS.FirstOrDefault();
            var company = _context.Company_DS.FirstOrDefault();

            Assert.NotNull(user);
            Assert.NotNull(company);

            var existingStaff = new Company_Staff
            {
                UserID = user!.UserID,
                TenantID = company!.TenantID,
                Role = "Pifferaio magico",
                OutOfReports = false,
                RequireShiftCheckIn = true
            };

            _context.Company_Staff_DS.Add(existingStaff);
            _context.SaveChanges();

            var duplicateStaff = new Company_Staff
            {
                UserID = user.UserID,
                TenantID = company.TenantID,
                Role = "Strega cattiva",
                OutOfReports = true,
                RequireShiftCheckIn = false
            };

            // Act + Assert
            _context.Company_Staff_DS.Add(duplicateStaff);
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //todo: Employee Insert [FAIL] across multiple companies (UserID associated with multiple TenantID)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Constraint")]
        [Trait("Priority", "Medium")]
        [Fact(DisplayName = "Employee Insert [FAIL] if UserID is associated to multiple TenantID", Skip = "WIP")]
        public void Insert_Employee_Fail_WhenUserAssignedToMultipleCompanies()
        {
            // Arrange 
            var user = _context.AspNetUser_DS.FirstOrDefault();
            var company1 = _context.Company_DS.FirstOrDefault();
            var company2 = _context.Company_DS.Skip(1).FirstOrDefault();

            Assert.NotNull(user);
            Assert.NotNull(company1);
            Assert.NotNull(company2);
            Assert.NotEqual(company1!.TenantID, company2!.TenantID);

            // First valid insert
            var staffEntry1 = new Company_Staff
            {
                UserID = user!.UserID,
                TenantID = company1.TenantID,
                Role = "Tecnocratico",
                RequireShiftCheckIn = true
            };
            _context.Company_Staff_DS.Add(staffEntry1);
            _context.SaveChanges();

            // Second insert - same User, different company
            var staffEntry2 = new Company_Staff
            {
                UserID = user.UserID,
                TenantID = company2.TenantID,
                Role = "Analista mouse",
                RequireShiftCheckIn = false
            };

            _context.Company_Staff_DS.Add(staffEntry2);

            // Act + Assert - expected error (or at least I hope so...)
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //Employee Insert [OK] CreatedBy and DateIns set correctly with UTC
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Audit")]
        [Trait("Priority", "Medium")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Employee Insert [OK] with CreatedBy and DateIns set correctly in UTC", Skip = "TBF")]
        public void Insert_Employee_OK_CreatedBy_And_DateIns_UTC()
        {
            // Arrange
            var utcBefore = DateTime.UtcNow;
            var creatorId = _context.AspNetUser_DS.Skip(1).First().UserID;

            var staff = new Company_Staff
            {
                UserID = Guid.NewGuid(),
                TenantID = _context.Company_DS.First().TenantID,
                Role = "Supporter di marmotte",
                CreatedBy = creatorId
                // Do not set DateIns -> DB should handle it
            };

            // Act
            _context.Company_Staff_DS.Add(staff);
            _context.SaveChanges();
            _context.Entry(staff).Reload(); // valore ricaricato dal DB

            // Assert
            Assert.Equal(creatorId, staff.CreatedBy);
            Assert.True(staff.DateIns >= utcBefore && staff.DateIns <= DateTime.UtcNow);
            Assert.Equal(DateTimeKind.Utc, staff.DateIns.Kind);
        }

    }
}