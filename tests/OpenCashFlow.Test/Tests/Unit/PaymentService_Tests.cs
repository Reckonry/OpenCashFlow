using AutoMapper;
using OpenCashFlow.API.Services;
using OpenCashFlow.API.Services.Interfaces;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Repositories;
using OpenCashFlow.Application.Payments.Calendar;
using OpenCashFlow.Application.Payments.CreatePayment;
using OpenCashFlow.Application.Payments.DeletePayment;
using OpenCashFlow.Application.Payments.GetPaymentDetail;
using OpenCashFlow.Application.Payments.GetPayments;
using OpenCashFlow.Application.Payments.PaymentMethods;
using OpenCashFlow.Application.Payments.Reports;
using OpenCashFlow.Application.Payments.UpdatePayment;
using OpenCashFlow.Infrastructure.ApplicationAdapters;
using OpenCashFlow.Infrastructure.Cash;
using OpenCashFlow.Infrastructure.Payments;
using OpenCashFlow.Infrastructure.Payments.Lookups;
using OpenCashFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging.Abstractions;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Models;
using global::Shared.Models.Core;

namespace OpenCashFlow.Test.Tests.Unit;

public class PaymentService_Tests
{
    private static IMapper CreateMapper(Action<IMapperConfigurationExpression>? configure = null)
    {
        var config = new MapperConfiguration(cfg => configure?.Invoke(cfg), NullLoggerFactory.Instance);
        return config.CreateMapper();
    }

    private sealed class StubAuthenticationService(Guid companyId, Guid userId) : IAuthenticationService
    {
        private readonly Guid _companyId = companyId;
        private readonly Guid _userId = userId;

        public Guid GetUserID() => _userId;
        public Guid GetTenantID() => _companyId;

        public Task<AuthResult> Authenticate(string username, string password, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<ApiResponse<string?>> AuthenticateFastAsync(HttpContext httpContext, string pin, string FLCookieValue, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AuthResult> GenerateFastLoginCookieValueAsync(string username, string password, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task ForgotPasswordAsync(string email, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task ForgotPasswordAsync(Guid UserID, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task ResetPasswordAsync(Guid UserID, string token, string newPassword, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<Core_RegistrationResult> RegistrationAsync(Register_DTO registration, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> ConfirmAccountAsync(Guid TenantID, Guid UserID, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<bool> ResendConfirmationAsync(string usernameOrEmail, CancellationToken cancellationToken) => throw new NotImplementedException();
        public Task<AuthResult> RegenerateTokenWithUpdatedClaimsAsync(Guid userId, CancellationToken cancellationToken) => throw new NotImplementedException();
    }

    private sealed class NoOpAuditWriter : IAuditWriter
    {
        public Task WritePaymentCreatedAsync(PaymentSnapshot payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task WritePaymentUpdatedAsync(PaymentUpdateAudit payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task WritePaymentDeletedAsync(PaymentDeletedAudit payment, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private static PaymentService CreatePaymentService(
        ApplicationDbContext context,
        IAuthenticationService authService,
        IMapper mapper,
        ICashLedgerWriter? cashLedgerWriter = null)
    {
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var paymentMethodReader = new PaymentMethodReader(context);
        var paymentMethodWriter = new PaymentMethodWriter(context, NullLogger<PaymentMethodWriter>.Instance);
        var documentTypeReader = new DocumentTypeReader(context);
        var documentTypeWriter = new DocumentTypeWriter(context, NullLogger<DocumentTypeWriter>.Instance);
        var cashLedgerRepository = new CashLedgerRepository(context);
        var cashLedgerAdapter = new CashLedgerWriterAdapter(cashLedgerRepository);
        var effectiveCashLedgerWriter = cashLedgerWriter ?? cashLedgerAdapter;
        var auditWriter = new NoOpAuditWriter();
        var unitOfWork = new EfUnitOfWork(context);

        var createPaymentOrchestrator = new CreatePaymentOrchestrator(
            new CreatePaymentUseCase(),
            new PaymentReaderAdapter(paymentRepository, paymentMethodReader),
            new PaymentWriterAdapter(paymentRepository),
            new DailyPaymentWriterAdapter(paymentRepository),
            effectiveCashLedgerWriter,
            auditWriter,
            unitOfWork);

        var updatePaymentOrchestrator = new UpdatePaymentOrchestrator(
            new UpdatePaymentUseCase(),
            new PaymentReaderAdapter(paymentRepository, paymentMethodReader),
            new PaymentWriterAdapter(paymentRepository),
            new DailyPaymentWriterAdapter(paymentRepository),
            cashLedgerAdapter,
            effectiveCashLedgerWriter,
            auditWriter,
            unitOfWork);

        var deletePaymentOrchestrator = new DeletePaymentOrchestrator(
            new DeletePaymentUseCase(),
            new PaymentReaderAdapter(paymentRepository, paymentMethodReader),
            new PaymentWriterAdapter(paymentRepository),
            new DailyPaymentWriterAdapter(paymentRepository),
            effectiveCashLedgerWriter,
            auditWriter,
            unitOfWork);

        return new PaymentService(
            authService,
            createPaymentOrchestrator,
            updatePaymentOrchestrator,
            deletePaymentOrchestrator,
            new GetPaymentsUseCase(new PaymentQueryReader(context)),
            new GetPaymentDetailUseCase(new PaymentQueryReader(context)),
            new GetPaymentReportsUseCase(new PaymentReportReader(context)),
            new GetPaymentCalendarUseCase(new PaymentCalendarReader(context)),
            new GetPaymentMethodsUseCase(paymentMethodReader),
            new GetPaymentMethodDetailUseCase(paymentMethodReader),
            new CreatePaymentMethodUseCase(paymentMethodWriter),
            new UpdatePaymentMethodUseCase(paymentMethodWriter),
            new DeletePaymentMethodUseCase(paymentMethodWriter),
            new OpenCashFlow.Application.Payments.DocumentTypes.GetDocumentTypesUseCase(documentTypeReader),
            new OpenCashFlow.Application.Payments.DocumentTypes.GetDocumentTypeDetailUseCase(documentTypeReader),
            new OpenCashFlow.Application.Payments.DocumentTypes.CreateDocumentTypeUseCase(documentTypeWriter),
            new OpenCashFlow.Application.Payments.DocumentTypes.UpdateDocumentTypeUseCase(documentTypeWriter),
            new OpenCashFlow.Application.Payments.DocumentTypes.DeleteDocumentTypeUseCase(documentTypeWriter),
            NullLogger<PaymentService>.Instance);
    }

    private sealed class FailingCashLedgerWriter : ICashLedgerWriter
    {
        public Task ApplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated cash ledger failure for testing atomicity");
        }

        public Task ReapplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated cash ledger failure for testing atomicity");

        public Task UpdatePaymentAsync(Guid tenantId, Guid paymentId, decimal originalDelta, decimal newDelta, Guid userId, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated cash ledger failure for testing atomicity");

        public Task VoidPaymentAsync(Guid tenantId, Guid paymentId, decimal originalAmount, Guid userId, CancellationToken cancellationToken = default)
            => throw new InvalidOperationException("Simulated cash ledger failure for testing atomicity");
    }

    [Fact]
    public async Task DeletePaymentAsync_WhenCashAlias_RevertsCashBalance()
    {
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var paymentId = Guid.NewGuid();
        const double paymentAmount = 120.5;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_cash_void_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = paymentMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Cassa in contanti",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DS.Add(new Payment
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = paymentAmount,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = paymentMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Incasso contanti",
            UserID = userId,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var cashService = new CashService(context, NullLogger<CashService>.Instance);
        await cashService.ApplyPaymentAsync(companyId, paymentId, Convert.ToDecimal(paymentAmount), userId.ToString(), CancellationToken.None);

        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper();
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper);

        var deleted = await paymentService.DeletePaymentAsync(paymentId, CancellationToken.None);

        Assert.True(deleted);

        var balance = await context.CashBalances.AsNoTracking().SingleAsync();
        Assert.Equal(0m, balance.Balance);

        var voidLedger = await context.CashLedgers.AsNoTracking()
            .FirstOrDefaultAsync(x => x.RefType == "Void" &&
                                     (x.RefId == paymentId || x.OriginalPaymentId == paymentId));
        Assert.NotNull(voidLedger);
        Assert.Equal(-Convert.ToDecimal(paymentAmount), voidLedger!.Delta);
    }

    [Fact]
    public async Task AddPaymentAsync_WithSameRequestId_ReturnsExistingPayment_DoesNotCreateDuplicate()
    {
        // Arrange
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        const double paymentAmount = 150.75;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_idempotency_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Setup test data
        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = paymentMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Cassa in contanti",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var cashService = new CashService(context, NullLogger<CashService>.Instance);
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Payment_Create_DTO, Payment>();
            cfg.CreateMap<Payment, Payment_Create_DTO>();
        });
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper);

        var paymentDto = new Payment_Create_DTO
        {
            RequestId = requestId,
            TenantID = companyId,
            UserID = userId,
            Amount = paymentAmount,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = paymentMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Test idempotency",
            DateIns = DateTime.UtcNow
        };

        // Act - First submission
        var firstResult = await paymentService.AddPaymentAsync(paymentDto, CancellationToken.None);

        // Act - Second submission with same RequestId (simulating double-click)
        var secondResult = await paymentService.AddPaymentAsync(paymentDto, CancellationToken.None);

        // Assert
        Assert.NotNull(firstResult);
        Assert.NotNull(secondResult);
        Assert.Equal(firstResult.PaymentID, secondResult.PaymentID);
        Assert.Equal(requestId, firstResult.RequestId);
        Assert.Equal(requestId, secondResult.RequestId);

        // Verify only one payment was created
        var paymentsCount = await context.Payment_DS.CountAsync(p => p.RequestId == requestId);
        Assert.Equal(1, paymentsCount);

        // Verify only one cash ledger entry was created
        var ledgerCount = await context.CashLedgers.CountAsync(l => l.RefType == "Payment");
        Assert.Equal(1, ledgerCount);
    }

    [Fact]
    public async Task AddPaymentAsync_WhenCashServiceFails_ThrowsException()
    {
        // NOTE: This test verifies that exceptions from CashService are propagated correctly.
        // InMemoryDatabase doesn't fully support transactions/rollbacks, so we can only verify
        // that the exception is thrown. For full atomicity testing, use a real database in integration tests.

        // Arrange
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var requestId = Guid.NewGuid();
        const double paymentAmount = 200.00;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_exception_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Setup test data
        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = paymentMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Cassa in contanti",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        // Create a cash ledger writer that throws an exception
        var mockCashService = new FailingCashService();
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Payment_Create_DTO, Payment>();
            cfg.CreateMap<Payment, Payment_Create_DTO>();
        });
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper, new FailingCashLedgerWriter());

        var paymentDto = new Payment_Create_DTO
        {
            RequestId = requestId,
            TenantID = companyId,
            UserID = userId,
            Amount = paymentAmount,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = paymentMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Test atomicity exception propagation",
            DateIns = DateTime.UtcNow
        };

        // Act & Assert - Should throw exception from the Application cash ledger port
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await paymentService.AddPaymentAsync(paymentDto, CancellationToken.None));

        // Verify the exception message matches what we expect
        Assert.Contains("Simulated cash ledger failure", exception.Message);
    }

    // Mock ICashService retained for constructor compatibility in update/delete paths.
    private sealed class FailingCashService : ICashService
    {
        public Task ApplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
        {
            throw new InvalidOperationException("Simulated cash service failure for testing atomicity");
        }

        public Task ReapplyPaymentAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task RefundAsync(Guid companyId, Guid paymentId, decimal amount, string userId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task VoidAsync(Guid companyId, Guid paymentId, decimal originalAmount, string userId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task UpdatePaymentAsync(Guid companyId, Guid paymentId, decimal originalDelta, decimal newDelta, string userId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task AdminAdjustAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task RebuildBalanceAsync(Guid companyId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<decimal> GetCurrentAsync(Guid companyId, CancellationToken ct)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<global::Shared.Models.Cash.CashLedger>> GetLedgerAsync(Guid companyId, DateTimeOffset? from, DateTimeOffset? to, int skip, int take, CancellationToken ct)
            => throw new NotImplementedException();
    }

    [Fact]
    public async Task UpdatePaymentAsync_CashToNonCashToCash_CorrectlyManagesCashLedger()
    {
        // This test verifies the bug fix for: Cash → POS → Cash flow
        // Previously, going back to cash after being voided would fail due to idempotency check

        // Arrange
        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var posMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        const double initialAmount = 100.0;

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_cash_pos_cash_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        // Setup payment methods
        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = cashMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Cassa in contanti",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = posMethodId,
            PaymentMethodName = "POS",
            PaymentMethodDescription = "Pagamento con carta",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 1,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            DisplayOrder = 0,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var cashService = new CashService(context, NullLogger<CashService>.Instance);
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Payment_Create_DTO, Payment>();
            cfg.CreateMap<Payment, Payment_Create_DTO>();
            cfg.CreateMap<Payment, Payment_Detail_DTO>();
            cfg.CreateMap<Payment, Payment_Update_DTO>();
        });
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper);

        // 1. Create payment with CASH method
        var createDto = new Payment_Create_DTO
        {
            RequestId = Guid.NewGuid(),
            TenantID = companyId,
            UserID = userId,
            Amount = initialAmount,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Initial cash payment",
            DateIns = DateTime.UtcNow
        };

        var created = await paymentService.AddPaymentAsync(createDto, CancellationToken.None);
        Assert.NotNull(created);
        var paymentId = created.PaymentID;

        // Verify initial cash balance
        var balance1 = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(100m, balance1);

        var ledgerEntries1 = await context.CashLedgers.Where(x => x.CompanyId == companyId).ToListAsync();
        Assert.Single(ledgerEntries1);
        Assert.Equal("Payment", ledgerEntries1[0].RefType);
        Assert.Equal(100m, ledgerEntries1[0].Delta);

        // 2. Update payment to POS (non-cash)
        var updateToPos = new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            UserID = userId,
            Amount = initialAmount,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = posMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Changed to POS",
            DateIns = DateTime.UtcNow
        };

        var updated1 = await paymentService.UpdatePaymentAsync(updateToPos, CancellationToken.None);
        Assert.NotNull(updated1);

        // Verify cash balance is now 0 (voided)
        var balance2 = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(0m, balance2);

        var ledgerEntries2 = await context.CashLedgers.Where(x => x.CompanyId == companyId).ToListAsync();
        Assert.Equal(2, ledgerEntries2.Count);
        var voidEntry = ledgerEntries2.FirstOrDefault(x => x.RefType == "Void");
        Assert.NotNull(voidEntry);
        Assert.Equal(-100m, voidEntry!.Delta);

        // 3. Update payment back to CASH - THIS IS THE BUG FIX TEST
        var updateToCash = new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            UserID = userId,
            Amount = initialAmount,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Back to cash",
            DateIns = DateTime.UtcNow
        };

        var updated2 = await paymentService.UpdatePaymentAsync(updateToCash, CancellationToken.None);
        Assert.NotNull(updated2);

        // Verify cash balance is back to 100 (re-applied)
        var balance3 = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(100m, balance3);

        // Verify there's a PaymentReapply entry (not Payment, to avoid idempotency block)
        var ledgerEntries3 = await context.CashLedgers.Where(x => x.CompanyId == companyId).ToListAsync();
        Assert.Equal(3, ledgerEntries3.Count);
        var reapplyEntry = ledgerEntries3.FirstOrDefault(x => x.RefType == "PaymentReapply");
        Assert.NotNull(reapplyEntry);
        Assert.Equal(100m, reapplyEntry!.Delta);

        // Verify ledger history: Payment (+100) → Void (-100) → PaymentReapply (+100) = 100
        var totalDelta = ledgerEntries3.Sum(x => x.Delta);
        Assert.Equal(100m, totalDelta);
    }

    [Fact]
    public async Task UpdatePaymentAsync_CashToPosToCashWithDifferentAmount_CorrectlyAppliesNewAmount()
    {
        // Test scenario: Cash 100€ → POS → Cash 200€
        // Expected: Void 100, Reapply 200, Balance = 200

        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var posMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_amount_change_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = cashMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Pagamento in contanti",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = posMethodId,
            PaymentMethodName = "POS",
            PaymentMethodDescription = "Pagamento con carta",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var cashService = new CashService(context, NullLogger<CashService>.Instance);
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Payment_Create_DTO, Payment>();
            cfg.CreateMap<Payment, Payment_Create_DTO>();
            cfg.CreateMap<Payment, Payment_Detail_DTO>();
            cfg.CreateMap<Payment, Payment_Update_DTO>();
        });
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper);

        // 1. Create payment with CASH 100€
        var createDto = new Payment_Create_DTO
        {
            RequestId = Guid.NewGuid(),
            TenantID = companyId,
            UserID = userId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Initial 100",
            DateIns = DateTime.UtcNow
        };

        var created = await paymentService.AddPaymentAsync(createDto, CancellationToken.None);
        var paymentId = created!.PaymentID;

        Assert.Equal(100m, await cashService.GetCurrentAsync(companyId, CancellationToken.None));

        // 2. Change to POS
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = posMethodId,
            DocumentTypeID = documentTypeId,
            Description = "POS",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        Assert.Equal(0m, await cashService.GetCurrentAsync(companyId, CancellationToken.None));

        // 3. Change back to CASH with 200€
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 200.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Cash 200",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        // Verify final balance is 200 (not 100)
        var finalBalance = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(200m, finalBalance);

        // Verify ledger: Payment(+100) → Void(-100) → PaymentReapply(+200) = 200
        var ledger = await context.CashLedgers.Where(x => x.CompanyId == companyId).ToListAsync();
        Assert.Equal(3, ledger.Count);
        Assert.Equal(200m, ledger.Sum(x => x.Delta));
    }

    [Fact]
    public async Task UpdatePaymentAsync_CashIncomeToPosToCashOutcome_CorrectlyHandlesTypeChange()
    {
        // Test scenario: Cash Income 100€ → POS → Cash Outcome 50€
        // Expected: Balance changes from +100 to 0 to -50

        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var posMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_type_change_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = cashMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Pagamento in contanti",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = posMethodId,
            PaymentMethodName = "POS",
            PaymentMethodDescription = "Pagamento con carta",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var cashService = new CashService(context, NullLogger<CashService>.Instance);
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Payment_Create_DTO, Payment>();
            cfg.CreateMap<Payment, Payment_Create_DTO>();
            cfg.CreateMap<Payment, Payment_Detail_DTO>();
            cfg.CreateMap<Payment, Payment_Update_DTO>();
        });
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper);

        // 1. Create INCOME payment with CASH 100€
        var createDto = new Payment_Create_DTO
        {
            RequestId = Guid.NewGuid(),
            TenantID = companyId,
            UserID = userId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Income 100",
            DateIns = DateTime.UtcNow
        };

        var created = await paymentService.AddPaymentAsync(createDto, CancellationToken.None);
        var paymentId = created!.PaymentID;

        Assert.Equal(100m, await cashService.GetCurrentAsync(companyId, CancellationToken.None));

        // 2. Change to POS
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = posMethodId,
            DocumentTypeID = documentTypeId,
            Description = "POS",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        Assert.Equal(0m, await cashService.GetCurrentAsync(companyId, CancellationToken.None));

        // 3. Change to CASH OUTCOME 50€
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 50.0,
            EntryType = nameof(EntryTypeEnum.Outcome),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Cash Outcome 50",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        // Verify final balance is -50 (outcome)
        var finalBalance = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(-50m, finalBalance);

        // Verify ledger: Payment(+100) → Void(-100) → PaymentReapply(-50) = -50
        var ledger = await context.CashLedgers.Where(x => x.CompanyId == companyId).ToListAsync();
        Assert.Equal(3, ledger.Count);
        Assert.Equal(-50m, ledger.Sum(x => x.Delta));
    }

    [Fact]
    public async Task UpdatePaymentAsync_CashToCashAmountChange_UsesUpdatePayment()
    {
        // Test scenario: Cash 100€ → Cash 150€ (no method change)
        // Expected: Uses UpdatePaymentAsync (delta +50), not Reapply

        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_cash_update_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = cashMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Pagamento in contanti",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var cashService = new CashService(context, NullLogger<CashService>.Instance);
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Payment_Create_DTO, Payment>();
            cfg.CreateMap<Payment, Payment_Create_DTO>();
            cfg.CreateMap<Payment, Payment_Detail_DTO>();
            cfg.CreateMap<Payment, Payment_Update_DTO>();
        });
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper);

        // 1. Create payment with CASH 100€
        var createDto = new Payment_Create_DTO
        {
            RequestId = Guid.NewGuid(),
            TenantID = companyId,
            UserID = userId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Cash 100",
            DateIns = DateTime.UtcNow
        };

        var created = await paymentService.AddPaymentAsync(createDto, CancellationToken.None);
        var paymentId = created!.PaymentID;

        Assert.Equal(100m, await cashService.GetCurrentAsync(companyId, CancellationToken.None));

        // 2. Update to Cash 150€ (same method, different amount)
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 150.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Cash 150",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        // Verify balance is 150
        var finalBalance = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(150m, finalBalance);

        // Verify ledger uses PaymentUpdate: Payment(+100) → PaymentUpdate(+50) = 150
        var ledger = await context.CashLedgers.Where(x => x.CompanyId == companyId).ToListAsync();
        Assert.Equal(2, ledger.Count);
        Assert.Contains(ledger, x => x.RefType == "Payment" && x.Delta == 100m);
        Assert.Contains(ledger, x => x.RefType == "PaymentUpdate" && x.Delta == 50m);
        Assert.Equal(150m, ledger.Sum(x => x.Delta));
    }

    [Fact]
    public async Task UpdatePaymentAsync_CashToPosToCashToPosRepeatedly_CorrectlyVoidsReappliedPayments()
    {
        // Test scenario: Cash 100€ → POS → Cash 100€ → POS
        // Expected: All voids work correctly, final balance = 0

        var companyId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var posMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: $"payments_repeated_toggle_{Guid.NewGuid()}")
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        await using var context = new ApplicationDbContext(options);

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = cashMethodId,
            PaymentMethodName = "Contanti",
            PaymentMethodDescription = "Pagamento in contanti",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.PaymentMethod_DS.Add(new Payment_Method_LookUps
        {
            PaymentMethodID = posMethodId,
            PaymentMethodName = "POS",
            PaymentMethodDescription = "Pagamento con carta",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.Payment_DocumentType_DS.Add(new Payment_DocumentType_LookUp
        {
            DocumentTypeID = documentTypeId,
            DocumentTypeName = "Ricevuta",
            DocumentTypeDescription = "Incasso",
            TenantID = companyId,
            Visible = true,
            IsDeleted = false,
            DateIns = DateTime.UtcNow,
        });

        context.AspNetUser_DS.Add(new global::Shared.Models.Identity.AspNetUser
        {
            UserID = userId,
            UserName = "tester",
            UserFirstName = "Test",
            Email = "tester@example.com",
            EmailConfirmed = true,
            PasswordHash = "hash",
            IsApproved = true,
            DateIns = DateTime.UtcNow,
        });

        await context.SaveChangesAsync();

        var cashService = new CashService(context, NullLogger<CashService>.Instance);
        var paymentRepository = new PaymentRepository(context, NullLogger<PaymentRepository>.Instance);
        var mapper = CreateMapper(cfg =>
        {
            cfg.CreateMap<Payment_Create_DTO, Payment>();
            cfg.CreateMap<Payment, Payment_Create_DTO>();
            cfg.CreateMap<Payment, Payment_Detail_DTO>();
            cfg.CreateMap<Payment, Payment_Update_DTO>();
        });
        var authService = new StubAuthenticationService(companyId, userId);
        var paymentService = CreatePaymentService(context, authService, mapper);

        // 1. Create payment with CASH 100€
        var createDto = new Payment_Create_DTO
        {
            RequestId = Guid.NewGuid(),
            TenantID = companyId,
            UserID = userId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Initial Cash",
            DateIns = DateTime.UtcNow
        };

        var created = await paymentService.AddPaymentAsync(createDto, CancellationToken.None);
        var paymentId = created!.PaymentID;

        var balance1 = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(100m, balance1);

        // 2. Change to POS (first time)
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = posMethodId,
            DocumentTypeID = documentTypeId,
            Description = "POS 1",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        var balance2 = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(0m, balance2);

        // 3. Change back to CASH
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = cashMethodId,
            DocumentTypeID = documentTypeId,
            Description = "Cash again",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        var balance3 = await cashService.GetCurrentAsync(companyId, CancellationToken.None);
        Assert.Equal(100m, balance3);

        // 4. Change to POS again (THIS IS THE BUG TEST)
        await paymentService.UpdatePaymentAsync(new Payment_Detail_DTO
        {
            PaymentID = paymentId,
            TenantID = companyId,
            Amount = 100.0,
            EntryType = nameof(EntryTypeEnum.Income),
            PaymentMethodID = posMethodId,
            DocumentTypeID = documentTypeId,
            Description = "POS 2",
            DateIns = DateTime.UtcNow
        }, CancellationToken.None);

        // Verify final balance is 0 (not 100)
        var balance4 = await cashService.GetCurrentAsync(companyId, CancellationToken.None);

        // Debug: Print all ledger entries
        var ledger = await context.CashLedgers.Where(x => x.CompanyId == companyId).OrderBy(x => x.CreatedAtUtc).ToListAsync();
        Console.WriteLine($"Total ledger entries: {ledger.Count}");
        foreach (var entry in ledger)
        {
            Console.WriteLine($"RefType: {entry.RefType}, RefId: {entry.RefId}, Delta: {entry.Delta}, Reason: {entry.Reason}");
        }
        Console.WriteLine($"Final balance: {balance4}, Expected: 0");

        Assert.Equal(0m, balance4);
        Assert.Equal(4, ledger.Count);

        var payment = ledger.FirstOrDefault(x => x.RefType == "Payment");
        Assert.NotNull(payment);
        Assert.Equal(100m, payment!.Delta);

        var void1 = ledger.Where(x => x.RefType == "Void").OrderBy(x => x.CreatedAtUtc).FirstOrDefault();
        Assert.NotNull(void1);
        Assert.Equal(-100m, void1!.Delta);

        var reapply = ledger.FirstOrDefault(x => x.RefType == "PaymentReapply");
        Assert.NotNull(reapply);
        Assert.Equal(100m, reapply!.Delta);

        var void2 = ledger.Where(x => x.RefType == "Void").OrderBy(x => x.CreatedAtUtc).Skip(1).FirstOrDefault();
        Assert.NotNull(void2);
        Assert.Equal(-100m, void2!.Delta);

        // Total should be 0
        Assert.Equal(0m, ledger.Sum(x => x.Delta));
    }
}
