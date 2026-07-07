using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.CreatePayment;

namespace OpenCashFlow.Application.Tests.Payments.CreatePayment;

public sealed class CreatePaymentOrchestratorTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidCommand_CreatesPayment()
    {
        var paymentStore = new FakePaymentStore();
        var dailyPayment = new FakeDailyPaymentWriter();
        var cashLedger = new FakeCashLedgerWriter();
        var audit = new FakeAuditWriter();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        var result = await orchestrator.ExecuteAsync(ValidCommand());

        Assert.NotNull(result);
        Assert.False(result.WasExisting);
        Assert.Single(paymentStore.CreatedPayments);
        Assert.Equal(result.Payment.PaymentId, paymentStore.CreatedPayments[0].PaymentId);
    }

    [Fact]
    public async Task ExecuteAsync_WithSameRequestId_ReturnsExistingPayment()
    {
        var command = ValidCommand();
        var existingPayment = SnapshotFrom(command);
        var paymentStore = new FakePaymentStore { ExistingPayment = existingPayment };
        var dailyPayment = new FakeDailyPaymentWriter();
        var cashLedger = new FakeCashLedgerWriter();
        var audit = new FakeAuditWriter();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        var result = await orchestrator.ExecuteAsync(command);

        Assert.NotNull(result);
        Assert.True(result.WasExisting);
        Assert.Equal(existingPayment.PaymentId, result.Payment.PaymentId);
        Assert.Empty(paymentStore.CreatedPayments);
        Assert.Empty(dailyPayment.Updates);
        Assert.Empty(cashLedger.Applications);
        Assert.Empty(audit.CreatedPayments);
    }

    [Fact]
    public async Task ExecuteAsync_AfterCreate_UpdatesDailyPayment()
    {
        var command = ValidCommand(entryType: "Income", amount: 33m);
        var paymentStore = new FakePaymentStore();
        var dailyPayment = new FakeDailyPaymentWriter();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, new FakeCashLedgerWriter(), new FakeAuditWriter());

        var result = await orchestrator.ExecuteAsync(command);

        Assert.NotNull(result);
        Assert.True(result.DailyPaymentApplied);
        var update = Assert.Single(dailyPayment.Updates);
        Assert.Equal(command.TenantId, update.TenantId);
        Assert.Equal(command.DateIns, update.Date);
        Assert.Equal(33m, update.Amount);
        Assert.Equal("Income", update.EntryType);
    }

    [Fact]
    public async Task ExecuteAsync_WithCashLikePaymentMethod_AppliesCashLedger()
    {
        var command = ValidCommand(entryType: "Outcome", amount: 25m);
        var paymentStore = new FakePaymentStore
        {
            PaymentMethod = new PaymentMethodSnapshot(command.PaymentMethodId!.Value, "Contanti")
        };
        var dailyPayment = new FakeDailyPaymentWriter();
        var cashLedger = new FakeCashLedgerWriter();
        var audit = new FakeAuditWriter();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        var result = await orchestrator.ExecuteAsync(command);

        Assert.NotNull(result);
        Assert.True(result.CashLedgerApplied);
        var application = Assert.Single(cashLedger.Applications);
        Assert.Equal(command.TenantId, application.TenantId);
        Assert.Equal(command.PaymentId, application.PaymentId);
        Assert.Equal(-25m, application.Delta);
        Assert.Equal(command.UserId, application.UserId);
    }

    [Fact]
    public async Task ExecuteAsync_WithNonCashPaymentMethod_DoesNotApplyCashLedger()
    {
        var paymentStore = new FakePaymentStore
        {
            PaymentMethod = new PaymentMethodSnapshot(Guid.NewGuid(), "Bank transfer")
        };
        var dailyPayment = new FakeDailyPaymentWriter();
        var cashLedger = new FakeCashLedgerWriter();
        var audit = new FakeAuditWriter();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        var result = await orchestrator.ExecuteAsync(ValidCommand());

        Assert.NotNull(result);
        Assert.False(result.CashLedgerApplied);
        Assert.Empty(cashLedger.Applications);
    }

    [Fact]
    public async Task ExecuteAsync_AfterCreate_WritesAudit()
    {
        var paymentStore = new FakePaymentStore();
        var dailyPayment = new FakeDailyPaymentWriter();
        var cashLedger = new FakeCashLedgerWriter();
        var audit = new FakeAuditWriter();
        var command = ValidCommand();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        var result = await orchestrator.ExecuteAsync(command);

        Assert.NotNull(result);
        var auditedPayment = Assert.Single(audit.CreatedPayments);
        Assert.Equal(result.Payment.PaymentId, auditedPayment.PaymentId);
    }

    [Fact]
    public async Task ExecuteAsync_WhenWriterReturnsNull_DoesNotWriteCashOrAudit()
    {
        var paymentStore = new FakePaymentStore { ReturnNullOnCreate = true };
        var dailyPayment = new FakeDailyPaymentWriter();
        var cashLedger = new FakeCashLedgerWriter();
        var audit = new FakeAuditWriter();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        var result = await orchestrator.ExecuteAsync(ValidCommand());

        Assert.Null(result);
        Assert.Empty(dailyPayment.Updates);
        Assert.Empty(cashLedger.Applications);
        Assert.Empty(audit.CreatedPayments);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDailyWriterFails_DoesNotWriteCashOrAudit()
    {
        var paymentStore = new FakePaymentStore
        {
            PaymentMethod = new PaymentMethodSnapshot(Guid.NewGuid(), "Contanti")
        };
        var dailyPayment = new FakeDailyPaymentWriter { ThrowOnUpdate = true };
        var cashLedger = new FakeCashLedgerWriter();
        var audit = new FakeAuditWriter();
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        await Assert.ThrowsAsync<InvalidOperationException>(() => orchestrator.ExecuteAsync(ValidCommand()));

        Assert.Empty(cashLedger.Applications);
        Assert.Empty(audit.CreatedPayments);
    }

    [Fact]
    public async Task ExecuteAsync_UsesExpectedOrder_CreateDailyCashAudit()
    {
        var operationLog = new List<string>();
        var command = ValidCommand();
        var paymentStore = new FakePaymentStore
        {
            PaymentMethod = new PaymentMethodSnapshot(command.PaymentMethodId!.Value, "Cash"),
            OperationLog = operationLog
        };
        var dailyPayment = new FakeDailyPaymentWriter { OperationLog = operationLog };
        var cashLedger = new FakeCashLedgerWriter { OperationLog = operationLog };
        var audit = new FakeAuditWriter { OperationLog = operationLog };
        var orchestrator = CreateOrchestrator(paymentStore, dailyPayment, cashLedger, audit);

        await orchestrator.ExecuteAsync(command);

        Assert.Equal(["create", "daily", "cash", "audit"], operationLog);
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidAmount_FailsBeforeWriter()
    {
        var paymentStore = new FakePaymentStore();
        var orchestrator = CreateOrchestrator(paymentStore, new FakeDailyPaymentWriter(), new FakeCashLedgerWriter(), new FakeAuditWriter());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => orchestrator.ExecuteAsync(ValidCommand(amount: 0m)));

        Assert.Empty(paymentStore.CreatedPayments);
    }

    private static CreatePaymentOrchestrator CreateOrchestrator(
        FakePaymentStore paymentStore,
        FakeDailyPaymentWriter dailyPaymentWriter,
        FakeCashLedgerWriter cashLedgerWriter,
        FakeAuditWriter auditWriter)
    {
        return new CreatePaymentOrchestrator(
            new CreatePaymentUseCase(),
            paymentStore,
            paymentStore,
            dailyPaymentWriter,
            cashLedgerWriter,
            auditWriter,
            new FakeUnitOfWork());
    }

    private static CreatePaymentCommand ValidCommand(string entryType = "Income", decimal amount = 10m)
    {
        return new CreatePaymentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            amount,
            entryType,
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            "Orchestrator test payment");
    }

    private static PaymentSnapshot SnapshotFrom(CreatePaymentCommand command)
    {
        return new PaymentSnapshot(
            command.PaymentId,
            command.TenantId,
            command.RequestId,
            command.Amount,
            command.EntryType,
            command.PaymentMethodId!.Value,
            command.DocumentTypeId!.Value,
            command.UserId,
            command.DateIns,
            command.Description);
    }

    private sealed class FakePaymentStore : IPaymentReader, IPaymentWriter
    {
        public PaymentSnapshot? ExistingPayment { get; init; }

        public PaymentMethodSnapshot? PaymentMethod { get; init; }

        public DocumentTypeSnapshot? DocumentType { get; init; }

        public List<string>? OperationLog { get; init; }

        public bool ReturnNullOnCreate { get; init; }

        public List<PaymentSnapshot> CreatedPayments { get; } = [];

        public Task<PaymentSnapshot?> GetByIdAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentSnapshot?>(null);
        }

        public Task<PaymentSnapshot?> GetByRequestIdAsync(Guid requestId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExistingPayment?.RequestId == requestId && ExistingPayment.TenantId == tenantId
                    ? ExistingPayment
                    : null);
        }

        public Task<PaymentMethodSnapshot?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentMethodSnapshot?>(PaymentMethod ?? new PaymentMethodSnapshot(paymentMethodId, "Bank transfer"));
        }

        public Task<DocumentTypeSnapshot?> GetDocumentTypeByIdAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DocumentTypeSnapshot?>(DocumentType ?? new DocumentTypeSnapshot(documentTypeId, "Document"));
        }

        public Task<PaymentSnapshot?> CreateAsync(PaymentDraft payment, CancellationToken cancellationToken = default)
        {
            if (ReturnNullOnCreate)
            {
                return Task.FromResult<PaymentSnapshot?>(null);
            }

            var createdPayment = new PaymentSnapshot(
                payment.PaymentId,
                payment.TenantId,
                payment.RequestId,
                payment.Amount,
                payment.EntryType,
                payment.PaymentMethodId,
                payment.DocumentTypeId,
                payment.UserId,
                payment.DateIns,
                payment.Description);

            CreatedPayments.Add(createdPayment);
            OperationLog?.Add("create");

            return Task.FromResult<PaymentSnapshot?>(createdPayment);
        }

        public Task<PaymentSnapshot?> UpdateAsync(PaymentUpdateDraft payment, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentSnapshot?>(null);
        }

        public Task<bool> DeleteAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(false);
        }
    }

    private sealed class FakeDailyPaymentWriter : IDailyPaymentWriter
    {
        public List<DailyPaymentUpdate> Updates { get; } = [];

        public List<string>? OperationLog { get; init; }

        public bool ThrowOnUpdate { get; init; }

        public Task UpdateDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
        {
            if (ThrowOnUpdate)
            {
                throw new InvalidOperationException("Daily payment update failed.");
            }

            Updates.Add(new DailyPaymentUpdate(tenantId, date, amount, entryType));
            OperationLog?.Add("daily");
            return Task.CompletedTask;
        }

        public Task DeleteDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCashLedgerWriter : ICashLedgerWriter
    {
        public List<CashApplication> Applications { get; } = [];

        public List<string>? OperationLog { get; init; }

        public Task ApplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
        {
            Applications.Add(new CashApplication(tenantId, paymentId, delta, userId));
            OperationLog?.Add("cash");
            return Task.CompletedTask;
        }

        public Task ReapplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task UpdatePaymentAsync(Guid tenantId, Guid paymentId, decimal originalDelta, decimal newDelta, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task VoidPaymentAsync(Guid tenantId, Guid paymentId, decimal originalAmount, Guid userId, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditWriter : IAuditWriter
    {
        public List<PaymentSnapshot> CreatedPayments { get; } = [];

        public List<string>? OperationLog { get; init; }

        public Task WritePaymentCreatedAsync(PaymentSnapshot payment, CancellationToken cancellationToken = default)
        {
            CreatedPayments.Add(payment);
            OperationLog?.Add("audit");
            return Task.CompletedTask;
        }

        public Task WritePaymentUpdatedAsync(PaymentUpdateAudit payment, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }

        public Task WritePaymentDeletedAsync(PaymentDeletedAudit payment, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            return operation(cancellationToken);
        }
    }

    private sealed record DailyPaymentUpdate(Guid TenantId, DateTime Date, decimal Amount, string EntryType);

    private sealed record CashApplication(Guid TenantId, Guid PaymentId, decimal Delta, Guid UserId);
}
