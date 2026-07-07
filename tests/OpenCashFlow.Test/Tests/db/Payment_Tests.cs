using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;
using Xunit;

namespace OpenCashFlow.Test.Tests
{
    [Collection("Database collection")]
    public class PaymentTests
    {
        private readonly DatabaseFixture _fixture;
        private readonly ApplicationDbContext _context;

        public PaymentTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _context = fixture.DbContext;
        }

        //Insert [OK] with all valid fields and existing PaymentMethod/DocumentType
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Insert")]
        [Trait("Priority", "Critical")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "POST payment with valid data should be inserted successfully", Skip = "TBF")]
        public void Insert_ValidPayment_Success()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var documentTypeId = _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var companyId = _context.Company_DS.First().TenantID;
            var userId = _context.Company_Staff_DS.First().UserID;

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = 500,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = methodId,
                DocumentTypeID = documentTypeId,
                Description = "New valid payment",
                UserID = userId,
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.Payment_DS.Add(newPayment);
            _context.SaveChanges();

            // Assert
            var inserted = _context.Payment_DS.Find(newPayment.PaymentID);
            Assert.NotNull(inserted);
            Assert.Equal(500, inserted!.Amount);
        }       

        //Insert [FAIL] with EntryType = null (required field)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Critical")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "POST payment with null EntryType should fail due to required constraint")]
        public void Insert_PaymentWithNullEntryType_ShouldFail()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var documentTypeId =  _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var companyId = _context.Company_DS.First().TenantID;
            var userId = _context.Company_Staff_DS.First().UserID;

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = 250,
                EntryType = null!, // EntryType is required
                PaymentMethodID = methodId,
                DocumentTypeID = documentTypeId,
                Description = "Invalid payment with null EntryType",
                UserID = userId,
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            _context.Payment_DS.Add(newPayment);
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //todo: Insert [FAIL] with non-existent PaymentMethodID (FK violation)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "FK")]
        [Trait("Priority", "Critical")]
        [Trait("Scope", "Integration")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "POST payment with non-existing PaymentMethodID should fail due to FK violation", Skip = "TO BE FIXED")]
        public void Insert_PaymentWithInvalidPaymentMethodID_ShouldFail()
        {
            // Arrange
            var invalidMethodId = Guid.NewGuid(); // Does not exist in DB
            var documentTypeId = _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var companyId = _context.Company_DS.First().TenantID;
            var userId = _context.Company_Staff_DS.First().UserID;

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = 250,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = invalidMethodId,
                DocumentTypeID = documentTypeId,
                Description = "Invalid payment with unknown PaymentMethodID",
                UserID = userId,
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            _context.Payment_DS.Add(newPayment);
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //todo: Insert [FAIL] with non-existent DocumentTypeID (FK violation)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "FK")]
        [Trait("Priority", "Critical")]
        [Trait("Scope", "Integration")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "POST payment with non-existing DocumentTypeID should fail due to FK violation", Skip = "TO BE FIXED")]
        public void Insert_PaymentWithInvalidDocumentTypeID_ShouldFail()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var invalidDocumentTypeId = Guid.NewGuid(); // Does not exist
            var companyId = _context.Company_DS.First().TenantID;
            var userId = _context.Company_Staff_DS.First().UserID;

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = 250,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = methodId,
                DocumentTypeID = invalidDocumentTypeId,
                Description = "Invalid payment with unknown DocumentTypeID",
                UserID = userId,
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            _context.Payment_DS.Add(newPayment);
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //todo: Insert [FAIL] with non-existent TenantID (FK violation)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "FK")]
        [Trait("Priority", "Critical")]
        [Trait("Scope", "Integration")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "POST payment with non-existing TenantID should fail due to FK violation", Skip = "TO BE FIXED")]
        public void Insert_PaymentWithInvalidCompanyID_ShouldFail()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var documentTypeId = _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var userId = _context.Company_Staff_DS.First().UserID;
            var invalidCompanyId = Guid.NewGuid(); // Does not exist

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = invalidCompanyId,
                Amount = 250,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = methodId,
                DocumentTypeID = documentTypeId,
                Description = "Invalid payment with unknown TenantID",
                UserID = userId,
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            _context.Payment_DS.Add(newPayment);
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //todo: Insert [FAIL] with non-existent UserID (FK violation)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "FK")]
        [Trait("Priority", "Critical")]
        [Trait("Scope", "Integration")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "POST payment with non-existing UserID should fail due to FK violation", Skip = "TO BE FIXED")]
        public void Insert_PaymentWithInvalidUserID_ShouldFail()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var documentTypeId = _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var companyId = _context.Company_DS.First().TenantID;
            var invalidUserId = Guid.NewGuid(); // Does not exist

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = 250,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = methodId,
                DocumentTypeID = documentTypeId,
                Description = "Invalid payment with unknown UserID",
                UserID = invalidUserId,
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            _context.Payment_DS.Add(newPayment);
            Assert.Throws<DbUpdateException>(() => _context.SaveChanges());
        }

        //todo: Insert [FAIL] with negative Amount value
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "Normal")]
        [Trait("Scope", "Integration")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "POST payment with negative Amount should fail business validation", Skip = "TO BE FIXED")]
        public void Insert_PaymentWithNegativeAmount_ShouldFail()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var documentTypeId = _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var companyId = _context.Company_DS.First().TenantID;
            var userId = _context.Company_Staff_DS.First().UserID;

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = -100, // Negative value
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = methodId,
                DocumentTypeID = documentTypeId,
                Description = "Invalid payment with negative amount",
                UserID = userId,
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.Payment_DS.Add(newPayment);

            // Assert
            // todo: I don't think I added the DB constraint... check...

            _context.SaveChanges(); // For now, this will likely pass since there is no check
            Assert.True(newPayment.Amount >= 0, "Amount should not be negative");
        }

        //Insert [OK] with optional fields null (e.g., Description, EditedBy, DateEdit)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Insert")]
        [Trait("Priority", "Normal")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "POST payment with optional fields null should succeed")]
        public void Insert_PaymentWithOptionalFieldsNull_ShouldSucceed()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var documentTypeId = _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var companyId = _context.Company_DS.First().TenantID;
            var userId = _context.Company_Staff_DS.First().UserID;

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = 100.00,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = methodId,
                DocumentTypeID = documentTypeId,
                Description = null,          // Optional field
                UserID = userId,
                EditedBy = null,             // Optional field
                DateEdit = null,             // Optional field
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.Payment_DS.Add(newPayment);
            _context.SaveChanges();

            // Assert
            var inserted = _context.Payment_DS.FirstOrDefault(p => p.PaymentID == newPayment.PaymentID);
            Assert.NotNull(inserted);
            Assert.Null(inserted.Description);
            Assert.Null(inserted.EditedBy);
            Assert.Null(inserted.DateEdit);
        }

        // Insert [OK] verifies DateIns is set automatically
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Audit")]
        [Trait("Priority", "Normal")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "POST payment should automatically set DateIns timestamp")]
        public void Insert_Payment_ShouldAutoSet_DateIns()
        {
            // Arrange
            var methodId = _context.PaymentMethod_DS.First().PaymentMethodID;
            var documentTypeId = _context.Payment_DocumentType_DS.First().DocumentTypeID;
            var companyId = _context.Company_DS.First().TenantID;
            var userId = _context.Company_Staff_DS.First().UserID;

            var newPayment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                TenantID = companyId,
                Amount = 500.00,
                EntryType = nameof(EntryTypeEnum.Income),
                PaymentMethodID = methodId,
                DocumentTypeID = documentTypeId,
                UserID = userId,
                // Leave DateIns unset to verify it gets assigned
            };

            // Act
            _context.Payment_DS.Add(newPayment);
            _context.SaveChanges();

            // Assert
            var inserted = _context.Payment_DS.FirstOrDefault(p => p.PaymentID == newPayment.PaymentID);
            Assert.NotNull(inserted);
            Assert.True(inserted!.DateIns > DateTime.MinValue, "DateIns should be automatically set.");
        }

        // Update [OK] changes Amount, Description, DateEdit
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Update")]
        [Trait("Priority", "Normal")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "POST payment should update Amount, Description and DateEdit")]
        public void Update_Payment_ShouldModify_Amount_Description_DateEdit()
        {
            var payment = _context.Payment_DS.First();

            payment.Amount = 750.00;
            payment.Description = "Updated test description";
            var editDate = DateTime.UtcNow;
            payment.DateEdit = editDate;

            _context.SaveChanges();

            var updated = _context.Payment_DS.Find(payment.PaymentID);
            Assert.NotNull(updated);
            Assert.Equal(750.00, updated!.Amount);
            Assert.Equal("Updated test description", updated.Description);
            Assert.Equal(editDate, updated.DateEdit);
        }

        // Update [FAIL] changes PaymentMethodID to one from another company (figure out how to add constraint)
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Validation")]
        [Trait("Priority", "High")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "PUT payment should fail when changing PaymentMethodID to one from another company", Skip = "TBF")]
        public Task Update_Payment_ShouldFail_WhenChangingToPaymentMethodFromOtherCompany()
        {
            // Arrange
            var originalPayment = _context.Payment_DS.Include(p => p.PaymentMethod).FirstOrDefault();
            Assert.NotNull(originalPayment);

            var newInvalidPaymentMethod = new Payment_Method_LookUps
            {
                PaymentMethodID = Guid.NewGuid(),
                PaymentMethodName = "InvalidMethod",
                PaymentMethodDescription = "From other company",
                Visible = true,
                TenantID = Guid.NewGuid(), // Simulate another company
                DateIns = DateTime.UtcNow
            };

            _context.PaymentMethod_DS.Add(newInvalidPaymentMethod);
            _context.SaveChanges();

            // Attempt to update
            originalPayment!.PaymentMethodID = newInvalidPaymentMethod.PaymentMethodID;

            // Act & Assert
            try
            {
                _context.SaveChanges();
                Assert.Fail("Expected logic to prevent cross-company PaymentMethod usage.");
            }
            catch (Exception ex)
            {
                Assert.Contains("company", ex.Message.ToLower()); // example message
            }

            return Task.CompletedTask;
        }

        // Delete [OK]
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Delete")]
        [Trait("Priority", "Normal")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "DELETE payment should remove it from database", Skip = "TBF")]
        public void Delete_Payment_ShouldRemoveFromDatabase()
        {
            // Arrange
            var paymentToDelete = _context.Payment_DS.FirstOrDefault();
            Assert.NotNull(paymentToDelete);

            // Act
            _context.Payment_DS.Remove(paymentToDelete!);
            _context.SaveChanges();

            // Assert
            var deleted = _context.Payment_DS.Find(paymentToDelete!.PaymentID);
            Assert.Null(deleted);
        }

        //todo: Delete [FAIL] if FK from other entities prevents it (e.g., payments referenced elsewhere)

        // SoftDelete [OK] sets IsDeleted = true, does not remove the record 
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "SoftDelete")]
        [Trait("Priority", "Normal")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Soft delete should set IsDeleted = true and keep the payment in DB")]
        public void SoftDelete_Payment_ShouldMarkAsDeleted_AndNotRemoveFromDb()
        {
            // Arrange
            var payment = _context.Payment_DS.FirstOrDefault();
            Assert.NotNull(payment);

            // Act
            payment!.IsDeleted = true;
            payment.DateEdit = DateTime.UtcNow;
            _context.SaveChanges();

            // Re-fetch to confirm soft delete
            var updated = _context.Payment_DS.IgnoreQueryFilters().FirstOrDefault(p => p.PaymentID == payment.PaymentID);

            // Assert
            Assert.NotNull(updated);
            Assert.True(updated!.IsDeleted);
        }

        //todo: Query [OK] only non-deleted payments 
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Query")]
        [Trait("Priority", "Normal")]
        [Trait("Scope", "Integration")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Query should return only payments where IsDeleted = false", Skip = "TBF")]
        public void QueryPayments_ShouldReturnOnlyNonDeletedRecords()
        {
            // Arrange

            // Simulate a deleted payment
            var payment = _context.Payment_DS.FirstOrDefault();
            Assert.NotNull(payment);
            payment!.IsDeleted = true;
            _context.SaveChanges();

            // Act
            var visiblePayments = _context.Payment_DS.ToList();

            // Assert
            Assert.DoesNotContain(visiblePayments, p => p.IsDeleted);
        }

        // Query [OK] gets a specific payment 
        [Trait("Layer", "Database")]
        [Trait("Feature", "Payments")]
        [Trait("Type", "Query")]
        [Trait("Priority", "Low")]
        [Trait("Scope", "Integration")]
        [Fact(DisplayName = "Should retrieve seeded payment with related data")]
        public void Retrieve_SeededPayment_Successfully()
        {
            // Arrange

            // Act
            var payment = _context.Payment_DS
                .Include(p => p.PaymentMethod)
                .Include(p => p.DocumentType)
                .FirstOrDefault();

            // Assert
            Assert.NotNull(payment);
            Assert.Equal(1000.00, payment!.Amount);
            Assert.Equal(nameof(EntryTypeEnum.Income), payment.EntryType);
            Assert.Equal("Credit Card", payment.PaymentMethod!.PaymentMethodName);
            Assert.Equal("Invoice", payment.DocumentType!.DocumentTypeName);
        }
    }
}
