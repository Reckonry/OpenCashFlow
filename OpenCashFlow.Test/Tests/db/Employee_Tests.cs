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

        // EMPLOYEE (Company_Staff) — REGOLE DB IMPORTANTI (senza API)

        //todo: Employee Insert [FAIL] con UserID mancante o nullo (FK obbligatoria)
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
                // UserID = Guid.Empty, // <-- volutamente non impostato
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

        //todo: Employee Insert [FAIL] con TenantID mancante o nullo (FK obbligatoria)
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
                // TenantID = Guid.Empty, // <-- volutamente non impostato
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

        //todo: Employee Insert [FAIL] con User non esistente nel DB (violazione FK)
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
                UserID = Guid.NewGuid(), // FK inesistente
                Role = "Tecnico scaldabagni"
            };

            // Act & Assert
            Assert.Throws<DbUpdateException>(() =>
            {
                _context.Company_Staff_DS.Add(staff);
                _context.SaveChanges();
            });
        }


        //todo: Employee Insert [OK] con flag IsManager = true (verifica persistenza booleana)
        // Employee Insert [OK] con DateIns settata correttamente (manuale o via default DB)
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
                UserID = Guid.NewGuid(), // Nuovo UserID per il test
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
            Assert.True(inserted!.DateIns != default); // può essere manuale o assegnato dal costruttore
            Assert.True((inserted.DateIns - DateTime.UtcNow).TotalSeconds < 3); // accetta ±3s di delta massimo
        }

        //todo: Employee Insert [FAIL] con ruoli null

        //todo: Employee Update [OK] modifica di Role, IsManager, Active flag
        

        //todo: Employee Delete [FAIL] se usato in Payment (vincolo FK attivo)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Employee")]
        [Trait("Type", "Constraint")]
        [Trait("Priority", "High")]
        [Fact(DisplayName = "Employee Delete [FAIL] if referenced in Payment (FK constraint)", Skip = "WIP")]
        public void Delete_Employee_Fail_WhenReferencedInOtherEntities()
        {
            // Arrange
            
            // Simula un'entità che usa la FK verso lo staff => Payment
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

        //todo: Employee SoftDelete [OK] imposta IsDeleted = true senza rimuovere oscurando i campi
        

        //todo: Employee Insert [FAIL] con combinazione (UserID, TenantID) già esistente (vincolo di unicità)
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

        //todo: Employee Insert [FAIL] su più aziende (UserID associato a più TenantID diversi)
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

            // Primo inserimento valido
            var staffEntry1 = new Company_Staff
            {
                UserID = user!.UserID,
                TenantID = company1.TenantID,
                Role = "Tecnocratico",
                RequireShiftCheckIn = true
            };
            _context.Company_Staff_DS.Add(staffEntry1);
            _context.SaveChanges();

            // Secondo inserimento — stesso User, altra azienda
            var staffEntry2 = new Company_Staff
            {
                UserID = user.UserID,
                TenantID = company2.TenantID,
                Role = "Analista mouse",
                RequireShiftCheckIn = false
            };

            _context.Company_Staff_DS.Add(staffEntry2);

            // Act + Assert — atteso errore ( o almeno lo spero ...)
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //Employee Insert [OK] CreatedBy e DateIns impostati correttamente con UTC
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
                // Non impostiamo DateIns → deve pensarci il DB
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