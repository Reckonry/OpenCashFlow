using OpenCashFlow.Test.Factories;
using OpenCashFlow.Test.Fixtures;
using OpenCashFlow.Test.Utilities;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Models;
using System.Net;
using System.Text.Json;
using Xunit;

namespace OpenCashFlow.Test.Tests
{
    [Collection("Database collection")]
    public class LookupTests 
    {
        private readonly DatabaseFixture _fixture;
        private readonly ApplicationDbContext _context;

        public LookupTests(DatabaseFixture fixture)
        {
            _fixture = fixture;
            _context = fixture.DbContext;
        }


        #region Payment_Method_LookUps
        //todo: Insert [OK] con PaymentMethodName = "Bonifico" e visibile
        [Trait("Layer", "Database")]
        [Trait("Feature", "PaymentMethod")]
        [Trait("Type", "Insert")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Create PaymentMethod with name 'Bonifico' and Visible = true", Skip = "TBF")]
        public async Task Insert_PaymentMethod_With_Bonifico_Visible()
        {
            // Arrange
            var paymentMethod = new Payment_Method_LookUps
            {
                PaymentMethodName = "Bonifico",
                PaymentMethodDescription = "Pagamento tramite bonifico bancario",
                PaymentMethodIcon = "bank-transfer-icon",
                Visible = true,
                DisplayOrder = 1,
                CreatedBy = _context.AspNetUser_DS.First().UserID,
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.PaymentMethod_DS.Add(paymentMethod);
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.PaymentMethod_DS.FirstOrDefaultAsync(p => p.PaymentMethodName == "Bonifico");
            Assert.NotNull(result);
            Assert.True(result.Visible);
            Assert.Equal("Bonifico", result.PaymentMethodName);
        }

        //todo: Insert [FAIL] con PaymentMethodName = null (campo obbligatorio)
        [Trait("Layer", "Database")]
        [Trait("Feature", "PaymentMethod")]
        [Trait("Type", "Insert")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should throw when PaymentMethodName is null (required field)")]
        public async Task Insert_PaymentMethod_When_NameIsNull()
        {
            // Arrange
            var paymentMethod = new Payment_Method_LookUps
            {
                PaymentMethodName = null!, // Forzatura per testare comportamento con null
                PaymentMethodDescription = "Metodo senza nome",
                PaymentMethodIcon = "warning-icon",
                Visible = true,
                DisplayOrder = 1,
                CreatedBy = _context.AspNetUser_DS.First().UserID,
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.PaymentMethod_DS.Add(paymentMethod);
                await _context.SaveChangesAsync();
            });
        }

        //todo: Insert [FAIL] con PaymentMethodName duplicato per stessa company
        [Trait("Layer", "Database")]
        [Trait("Feature", "PaymentMethod")]
        [Trait("Type", "Constraint")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should throw on duplicate PaymentMethodName for the same company", Skip = "TBF")]
        public async Task Insert_PaymentMethod_When_DuplicateNameInSameCompany()
        {
            // Arrange
            var companyId = Guid.NewGuid();

            var method1 = new Payment_Method_LookUps
            {
                PaymentMethodName = "Carta",
                PaymentMethodDescription = "Pagamento con carta",
                TenantID = _context.Company_DS.First().TenantID,
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            var method2 = new Payment_Method_LookUps
            {
                PaymentMethodName = "Carta", // stesso nome
                PaymentMethodDescription = "Duplicato",
                TenantID = _context.Company_DS.First().TenantID,     // stessa azienda
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            _context.PaymentMethod_DS.Add(method1);
            await _context.SaveChangesAsync();

            // Act & Assert
            _context.PaymentMethod_DS.Add(method2);
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _context.SaveChangesAsync();
            });
        }

        //todo: Insert [OK] con campi opzionali null (es: descrizione)
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Type", "Insert")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should succeed with null optional fields (e.g. Description, Icon)", Skip = "TBF")]
        public async Task Insert_DocumentType_When_OptionalFieldsAreNull()
        {
            // Arrange
            var documentType = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Fattura",
                DocumentTypeDescription = null,
                DocumentTypeIcon = null,
                TenantID = _context.Company_DS.First().TenantID,
                Visible = true,
                DisplayOrder = 1,
                CreatedBy = null,
                EditedBy = null,
                IsDeletedBy = null,
                IsDeletedWhy = null,
                DateDeleted = null,
                DateEdit = null,
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.Payment_DocumentType_DS.Add(documentType);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
        }

        //todo: Insert [FAIL] con TenantID mancante o non esistente
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Type", "Validation")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Insert [FAIL] - Should fail when TenantID is missing or invalid", Skip = "TBF")]
        public async Task Insert_DocumentType_When_TenantIDIsMissingOrInvalid_ShouldThrow()
        {
            // Arrange
            var documentType = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Nota di Credito",
                DocumentTypeDescription = "Documento per storni",
                Visible = true,
                DisplayOrder = 1,
                TenantID = Guid.NewGuid(), // GUID che non esiste nel DB
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            _context.Payment_DocumentType_DS.Add(documentType);
            await Assert.ThrowsAsync<DbUpdateException>(async () => await _context.SaveChangesAsync());
        }

        //todo: Insert [OK] con TenantID = null se è un lookup globale 
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Type", "Insert")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Insert [OK] - Should insert global DocumentType with null TenantID", Skip = "TBF")]
        public async Task Insert_DocumentType_When_GlobalLookup_ShouldSucceed()
        {
            // Arrange
            var documentType = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Ricevuta",
                DocumentTypeDescription = "Documento generico",
                Visible = true,
                DisplayOrder = 2,
                TenantID = null, // Global Lookup
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.Payment_DocumentType_DS.Add(documentType);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(1, result);
            var inserted = await _context.Payment_DocumentType_DS
                .FirstOrDefaultAsync(dt => dt.DocumentTypeName == "Ricevuta");
            Assert.NotNull(inserted);
            Assert.Null(inserted!.TenantID);
        }

        //todo: SoftDelete [OK] imposta IsDeleted = true senza rimuovere 
        [Trait("Layer", "Database")]
        [Trait("Feature", "PaymentMethod")]
        [Trait("Type", "SoftDelete")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should mark PaymentMethod as deleted without removing it", Skip = "TBF")]
        public async Task SoftDelete_PaymentMethod_ShouldMarkAsDeleted()
        {
            // Arrange
            var method = new Payment_Method_LookUps
            {
                PaymentMethodName = "Contanti",
                PaymentMethodDescription = "Pagamento in contanti",
                Visible = true,
                TenantID = _context.Company_DS.First().TenantID,
                DateIns = DateTime.UtcNow
            };

            _context.PaymentMethod_DS.Add(method);
            await _context.SaveChangesAsync();

            // Act
            method.IsDeleted = true;
            method.IsDeletedBy = _context.AspNetUser_DS.First().UserID;
            method.IsDeletedWhy = "Obsoleto";
            method.DateDeleted = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Assert
            var softDeleted = await _context.PaymentMethod_DS.FindAsync(method.PaymentMethodID);
            Assert.NotNull(softDeleted);
            Assert.True(softDeleted!.IsDeleted);
            Assert.NotNull(softDeleted.DateDeleted);
        }

        //todo: Visibility [OK] solo record con Visible = true restituiti da query standard

        //todo: Update [OK] cambia nome/metadati del metodo
        [Trait("Layer", "Database")]
        [Trait("Feature", "PaymentMethod")]
        [Trait("Type", "Query")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should return only PaymentMethods with Visible = true", Skip = "TBF")]
        public async Task Query_PaymentMethods_ShouldReturnOnlyVisible()
        {
            // Arrange
            var visibleMethod = new Payment_Method_LookUps
            {
                PaymentMethodName = "Carta",
                Visible = true,
                TenantID = _context.Company_DS.First().TenantID,
                DateIns = DateTime.UtcNow
            };

            var hiddenMethod = new Payment_Method_LookUps
            {
                PaymentMethodName = "Assegno",
                Visible = false,
                TenantID = visibleMethod.TenantID,
                DateIns = DateTime.UtcNow
            };

            _context.PaymentMethod_DS.AddRange(visibleMethod, hiddenMethod);
            await _context.SaveChangesAsync();

            // Act
            var visibleResults = await _context.PaymentMethod_DS
                .Where(pm => pm.Visible && !pm.IsDeleted)
                .ToListAsync();

            // Assert
            Assert.Single(visibleResults);
            Assert.Equal("Carta", visibleResults.First().PaymentMethodName);
        }

        //todo: Delete [FAIL] se metodo è associato a pagamenti (vincolo FK)
        [Trait("Layer", "Database")]
        [Trait("Feature", "PaymentMethod")]
        [Trait("Type", "Constraint")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should fail if PaymentMethod is linked to existing Payments", Skip = "TBF")]
        public async Task Delete_PaymentMethod_WhenLinkedToPayments_ShouldFail()
        {
            // Arrange
            var method = new Payment_Method_LookUps
            {
                PaymentMethodName = "Contanti",
                Visible = true,
                TenantID = _context.Company_DS.First().TenantID,
                DateIns = DateTime.UtcNow
            };

            _context.PaymentMethod_DS.Add(method);
            await _context.SaveChangesAsync();

            var payment = new Payment
            {
                PaymentID = Guid.NewGuid(),
                Amount = 100.00,
                EntryType = nameof(EntryTypeEnum.Income),
                TenantID = method.TenantID!.Value,
                UserID = _context.AspNetUser_DS.First().UserID,
                PaymentMethodID = method.PaymentMethodID,
                DocumentTypeID = _context.Payment_DocumentType_DS.First().DocumentTypeID,
                DateIns = DateTime.UtcNow
            };

            _context.Payment_DS.Add(payment);
            await _context.SaveChangesAsync();

            // Act
            _context.PaymentMethod_DS.Remove(method);

            // Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                await _context.SaveChangesAsync();
            });
        }

        #endregion

        #region Payment_DocumentType_LookUp
        //todo: Insert [OK] con DocumentTypeName = "Fattura"
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Type", "Insert")]
        [Trait("Scope", "Integration")]
        [Trait("Priority", "High")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should insert DocumentType with name 'Fattura' successfully", Skip = "TBF")]
        public async Task Insert_DocumentType_WithValidName_ShouldSucceed()
        {
            // Arrange
            var docType = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Fattura",
                DocumentTypeDescription = "Fattura standard",
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.Payment_DocumentType_DS.Add(docType);
            await _context.SaveChangesAsync();

            // Assert
            var saved = await _context.Payment_DocumentType_DS.FirstOrDefaultAsync(d => d.DocumentTypeName == "Fattura");
            Assert.NotNull(saved);
            Assert.True(saved.Visible);
            Assert.Equal("Fattura", saved.DocumentTypeName);
        }

        //todo: Insert [FAIL] con DocumentTypeName null
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Insert")]
        [Trait("Scope", "Integration")]
        [Trait("Priority", "Critical")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should throw when DocumentTypeName is null")]
        public async Task Insert_DocumentType_WithNullName_ShouldFail()
        {
            // Arrange
            var docType = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = null!, // Forzatura per simulare comportamento errato
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Payment_DocumentType_DS.Add(docType);
                await _context.SaveChangesAsync();
            });
        }

        //todo: Insert [FAIL] con duplicato su stessa company    
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Insert")]
        [Trait("Scope", "Integration")]
        [Trait("Priority", "Critical")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should throw when DocumentTypeName is duplicated for the same company", Skip = "TBF")]
        public async Task Insert_DocumentType_DuplicateNameForSameCompany_ShouldFail()
        {
            // Arrange
            var docType1 = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Fattura",
                TenantID = _context.Company_DS.First().TenantID,
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            var docType2 = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Fattura", // stesso nome
                TenantID = _context.Company_DS.First().TenantID,      // stessa azienda
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            _context.Payment_DocumentType_DS.Add(docType1);
            await _context.SaveChangesAsync();

            // Act & Assert
            await Assert.ThrowsAsync<DbUpdateException>(async () =>
            {
                _context.Payment_DocumentType_DS.Add(docType2);
                await _context.SaveChangesAsync();
            });
        }

        //todo: Insert [OK] su più aziende
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Insert")]
        [Trait("Scope", "Integration")]
        [Trait("Priority", "Medium")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should allow same DocumentTypeName for different companies", Skip = "TBF")]
        public async Task Insert_DocumentType_SameNameAcrossDifferentCompanies_ShouldSucceed()
        {
            // Arrange
            var docTypeA = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Fattura",
                TenantID = _context.Company_DS.First().TenantID,
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            var docTypeB = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Fattura", // stesso nome
                TenantID = _context.Company_DS.Skip(1).First().TenantID,       // azienda diversa
                Visible = true,
                DateIns = DateTime.UtcNow
            };

            // Act
            _context.Payment_DocumentType_DS.AddRange(docTypeA, docTypeB);
            var result = await _context.SaveChangesAsync();

            // Assert
            Assert.Equal(2, result); // Entrambi devono essere salvati correttamente
        }

        //todo: SoftDelete [OK] corretto
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Feature", "Company")]
        [Trait("Type", "SoftDelete")]
        [Trait("Scope", "Integration")]
        [Trait("Priority", "High")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should set IsDeleted = true without physical removal", Skip = "TBF")]
        public async Task SoftDelete_DocumentType_ShouldSetFlag_WithoutRemovingRecord()
        {
            // Arrange
            var docType = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Scontrino",
                Visible = true,
                DateIns = DateTime.UtcNow
            };
            _context.Payment_DocumentType_DS.Add(docType);
            await _context.SaveChangesAsync();

            // Act
            var inserted = await _context.Payment_DocumentType_DS.FirstAsync();
            inserted.IsDeleted = true;
            inserted.DateDeleted = DateTime.UtcNow;
            inserted.IsDeletedWhy = "Test case";
            await _context.SaveChangesAsync();

            // Assert
            var result = await _context.Payment_DocumentType_DS
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(x => x.DocumentTypeID == inserted.DocumentTypeID);

            Assert.NotNull(result);
            Assert.True(result!.IsDeleted);
            Assert.Equal("Test case", result.IsDeletedWhy);
            Assert.NotNull(result.DateDeleted);
        }

        //todo: FK [FAIL] delete se usato nei pagamenti
        [Trait("Layer", "Database")]
        [Trait("Feature", "DocumentType")]
        [Trait("Feature", "Company")]
        [Trait("Type", "Delete")]
        [Trait("Scope", "Integration")]
        [Trait("Priority", "Critical")]
        //[Fact(Skip = "WIP")] // TBF (to be fixed)
        [Fact(DisplayName = "Should throw if DocumentType is referenced by a Payment", Skip = "TBF")]
        public async Task Delete_DocumentType_ShouldFail_IfUsedInPayments()
        {
            // Arrange

            var documentType = new Payment_DocumentType_LookUp
            {
                DocumentTypeName = "Nota credito",
                Visible = true,
                DateIns = DateTime.UtcNow
            };
            _context.Payment_DocumentType_DS.Add(documentType);
            await _context.SaveChangesAsync();

            var payment = new Payment
            {
                Amount = 500,
                EntryType = "Exit",
                DocumentTypeID = documentType.DocumentTypeID,
                TenantID = _context.Company_DS.First().TenantID,
                UserID = _context.AspNetUser_DS.First().UserID,
                DateIns = DateTime.UtcNow
            };
            _context.Payment_DS.Add(payment);
            await _context.SaveChangesAsync();

            // Act
            _context.Payment_DocumentType_DS.Remove(documentType);

            // Assert
            var exception = await Assert.ThrowsAsync<DbUpdateException>(() => _context.SaveChangesAsync());
            Assert.Contains("constraint", exception.InnerException?.Message ?? exception.Message, StringComparison.OrdinalIgnoreCase);
        }

        #endregion

    }
}