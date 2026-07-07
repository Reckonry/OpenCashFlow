using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.DeletePayment;
using OpenCashFlow.Application.Payments.UpdatePayment;

namespace OpenCashFlow.Application.Tests.Payments.UpdateDeletePayment;

public sealed class UpdateDeletePaymentOrchestratorTests
{
    [Fact]
    public async Task UpdateAsync_CashToNonCash_VoidsCashLedgerAndWritesAudit()
    {
        var paymentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var bankMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var originalPayment = Snapshot(paymentId, tenantId, userId, cashMethodId, documentTypeId, 100m, "Income");
        var store = new FakePaymentStore
        {
            ExistingPayment = originalPayment,
            PaymentMethods =
            {
                [cashMethodId] = new PaymentMethodSnapshot(cashMethodId, "Contanti"),
                [bankMethodId] = new PaymentMethodSnapshot(bankMethodId, "Bank transfer")
            }
        };
        var daily = new FakeDailyPaymentWriter();
        var cash = new FakeCashLedger();
        var audit = new FakeAuditWriter();
        var unitOfWork = new FakeUnitOfWork();
        var orchestrator = new UpdatePaymentOrchestrator(
            new UpdatePaymentUseCase(),
            store,
            store,
            daily,
            cash,
            cash,
            audit,
            unitOfWork);

        var result = await orchestrator.ExecuteAsync(new UpdatePaymentCommand(
            paymentId,
            tenantId,
            userId,
            100m,
            "Income",
            bankMethodId,
            documentTypeId,
            DateTime.UtcNow,
            "Updated payment"));

        Assert.NotNull(result);
        Assert.True(unitOfWork.WasUsed);
        Assert.Single(daily.Deletes);
        Assert.Single(daily.Updates);
        var voidAction = Assert.Single(cash.Voids);
        Assert.Equal(paymentId, voidAction.PaymentId);
        Assert.Equal(100m, voidAction.OriginalAmount);
        Assert.Single(audit.UpdatedPayments);
    }

    [Fact]
    public async Task UpdateAsync_CashToCashAmountChange_UpdatesCashLedger()
    {
        var paymentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var originalPayment = Snapshot(paymentId, tenantId, userId, cashMethodId, documentTypeId, 100m, "Income");
        var store = new FakePaymentStore
        {
            ExistingPayment = originalPayment,
            PaymentMethods = { [cashMethodId] = new PaymentMethodSnapshot(cashMethodId, "Cash") }
        };
        var daily = new FakeDailyPaymentWriter();
        var cash = new FakeCashLedger { NetBalance = 100m };
        var audit = new FakeAuditWriter();
        var orchestrator = new UpdatePaymentOrchestrator(
            new UpdatePaymentUseCase(),
            store,
            store,
            daily,
            cash,
            cash,
            audit,
            new FakeUnitOfWork());

        await orchestrator.ExecuteAsync(new UpdatePaymentCommand(
            paymentId,
            tenantId,
            userId,
            150m,
            "Income",
            cashMethodId,
            documentTypeId,
            DateTime.UtcNow,
            "Updated payment"));

        var update = Assert.Single(cash.Updates);
        Assert.Equal(100m, update.OriginalDelta);
        Assert.Equal(150m, update.NewDelta);
    }

    [Fact]
    public async Task UpdateAsync_MissingPayment_ReturnsNullWithoutSideEffects()
    {
        var store = new FakePaymentStore();
        var daily = new FakeDailyPaymentWriter();
        var cash = new FakeCashLedger();
        var audit = new FakeAuditWriter();
        var orchestrator = new UpdatePaymentOrchestrator(
            new UpdatePaymentUseCase(),
            store,
            store,
            daily,
            cash,
            cash,
            audit,
            new FakeUnitOfWork());

        var result = await orchestrator.ExecuteAsync(new UpdatePaymentCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            50m,
            "Income",
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTime.UtcNow,
            null));

        Assert.Null(result);
        Assert.Empty(store.UpdatedPayments);
        Assert.Empty(daily.Updates);
        Assert.Empty(cash.Applications);
        Assert.Empty(audit.UpdatedPayments);
    }

    [Fact]
    public async Task DeleteAsync_CashPayment_SoftDeletesAndVoidsCashLedger()
    {
        var paymentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var cashMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var payment = Snapshot(paymentId, tenantId, userId, cashMethodId, documentTypeId, 80m, "Income");
        var store = new FakePaymentStore
        {
            ExistingPayment = payment,
            PaymentMethods = { [cashMethodId] = new PaymentMethodSnapshot(cashMethodId, "Contanti") }
        };
        var daily = new FakeDailyPaymentWriter();
        var cash = new FakeCashLedger();
        var audit = new FakeAuditWriter();
        var unitOfWork = new FakeUnitOfWork();
        var orchestrator = new DeletePaymentOrchestrator(
            new DeletePaymentUseCase(),
            store,
            store,
            daily,
            cash,
            audit,
            unitOfWork);

        var result = await orchestrator.ExecuteAsync(new DeletePaymentCommand(paymentId, tenantId, userId));

        Assert.True(result.Deleted);
        Assert.True(unitOfWork.WasUsed);
        Assert.Single(store.DeletedPayments);
        Assert.Single(daily.Deletes);
        Assert.Single(cash.Voids);
        Assert.Single(audit.DeletedPayments);
    }

    [Fact]
    public async Task DeleteAsync_NonCashPayment_DoesNotVoidCashLedger()
    {
        var paymentId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var bankMethodId = Guid.NewGuid();
        var documentTypeId = Guid.NewGuid();
        var payment = Snapshot(paymentId, tenantId, userId, bankMethodId, documentTypeId, 80m, "Income");
        var store = new FakePaymentStore
        {
            ExistingPayment = payment,
            PaymentMethods = { [bankMethodId] = new PaymentMethodSnapshot(bankMethodId, "Bank transfer") }
        };
        var daily = new FakeDailyPaymentWriter();
        var cash = new FakeCashLedger();
        var audit = new FakeAuditWriter();
        var orchestrator = new DeletePaymentOrchestrator(
            new DeletePaymentUseCase(),
            store,
            store,
            daily,
            cash,
            audit,
            new FakeUnitOfWork());

        var result = await orchestrator.ExecuteAsync(new DeletePaymentCommand(paymentId, tenantId, userId));

        Assert.True(result.Deleted);
        Assert.Empty(cash.Voids);
        Assert.Single(audit.DeletedPayments);
    }

    private static PaymentSnapshot Snapshot(
        Guid paymentId,
        Guid tenantId,
        Guid userId,
        Guid paymentMethodId,
        Guid documentTypeId,
        decimal amount,
        string entryType)
    {
        return new PaymentSnapshot(
            paymentId,
            tenantId,
            Guid.NewGuid(),
            amount,
            entryType,
            paymentMethodId,
            documentTypeId,
            userId,
            DateTime.UtcNow,
            "Test payment");
    }

    private sealed class FakePaymentStore : IPaymentReader, IPaymentWriter
    {
        public PaymentSnapshot? ExistingPayment { get; init; }
        public Dictionary<Guid, PaymentMethodSnapshot> PaymentMethods { get; } = [];
        public Dictionary<Guid, DocumentTypeSnapshot> DocumentTypes { get; } = [];
        public List<PaymentUpdateDraft> UpdatedPayments { get; } = [];
        public List<(Guid PaymentId, Guid TenantId)> DeletedPayments { get; } = [];

        public Task<PaymentSnapshot?> GetByIdAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExistingPayment?.PaymentId == paymentId && ExistingPayment.TenantId == tenantId
                    ? ExistingPayment
                    : null);
        }

        public Task<PaymentSnapshot?> GetByRequestIdAsync(Guid requestId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentSnapshot?>(null);
        }

        public Task<PaymentMethodSnapshot?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentMethodSnapshot?>(
                PaymentMethods.TryGetValue(paymentMethodId, out var method)
                    ? method
                    : new PaymentMethodSnapshot(paymentMethodId, "Bank transfer"));
        }

        public Task<DocumentTypeSnapshot?> GetDocumentTypeByIdAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<DocumentTypeSnapshot?>(
                DocumentTypes.TryGetValue(documentTypeId, out var documentType)
                    ? documentType
                    : new DocumentTypeSnapshot(documentTypeId, "Document"));
        }

        public Task<PaymentSnapshot?> CreateAsync(PaymentDraft payment, CancellationToken cancellationToken = default)
        {
            return Task.FromResult<PaymentSnapshot?>(null);
        }

        public Task<PaymentSnapshot?> UpdateAsync(PaymentUpdateDraft payment, CancellationToken cancellationToken = default)
        {
            if (ExistingPayment == null)
            {
                return Task.FromResult<PaymentSnapshot?>(null);
            }

            UpdatedPayments.Add(payment);
            return Task.FromResult<PaymentSnapshot?>(new PaymentSnapshot(
                payment.PaymentId,
                payment.TenantId,
                ExistingPayment.RequestId,
                payment.Amount,
                payment.EntryType,
                payment.PaymentMethodId,
                payment.DocumentTypeId,
                payment.UserId,
                payment.DateIns,
                payment.Description));
        }

        public Task<bool> DeleteAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            if (ExistingPayment?.PaymentId != paymentId || ExistingPayment.TenantId != tenantId)
            {
                return Task.FromResult(false);
            }

            DeletedPayments.Add((paymentId, tenantId));
            return Task.FromResult(true);
        }
    }

    private sealed class FakeDailyPaymentWriter : IDailyPaymentWriter
    {
        public List<DailyPaymentOperation> Updates { get; } = [];
        public List<DailyPaymentOperation> Deletes { get; } = [];

        public Task UpdateDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
        {
            Updates.Add(new DailyPaymentOperation(tenantId, date, amount, entryType));
            return Task.CompletedTask;
        }

        public Task DeleteDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
        {
            Deletes.Add(new DailyPaymentOperation(tenantId, date, amount, entryType));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeCashLedger : ICashLedgerReader, ICashLedgerWriter
    {
        public decimal NetBalance { get; init; }
        public bool WasVoided { get; init; }
        public List<CashApplication> Applications { get; } = [];
        public List<CashApplication> Reapplications { get; } = [];
        public List<CashUpdate> Updates { get; } = [];
        public List<CashVoid> Voids { get; } = [];

        public Task<decimal> GetPaymentNetBalanceAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(NetBalance);
        }

        public Task<bool> HasVoidedPaymentAsync(Guid tenantId, Guid paymentId, CancellationToken cancellationToken = default)
        {
            return Task.FromResult(WasVoided);
        }

        public Task ApplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
        {
            Applications.Add(new CashApplication(tenantId, paymentId, delta, userId));
            return Task.CompletedTask;
        }

        public Task ReapplyPaymentAsync(Guid tenantId, Guid paymentId, decimal delta, Guid userId, CancellationToken cancellationToken = default)
        {
            Reapplications.Add(new CashApplication(tenantId, paymentId, delta, userId));
            return Task.CompletedTask;
        }

        public Task UpdatePaymentAsync(Guid tenantId, Guid paymentId, decimal originalDelta, decimal newDelta, Guid userId, CancellationToken cancellationToken = default)
        {
            Updates.Add(new CashUpdate(tenantId, paymentId, originalDelta, newDelta, userId));
            return Task.CompletedTask;
        }

        public Task VoidPaymentAsync(Guid tenantId, Guid paymentId, decimal originalAmount, Guid userId, CancellationToken cancellationToken = default)
        {
            Voids.Add(new CashVoid(tenantId, paymentId, originalAmount, userId));
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAuditWriter : IAuditWriter
    {
        public List<PaymentSnapshot> CreatedPayments { get; } = [];
        public List<PaymentUpdateAudit> UpdatedPayments { get; } = [];
        public List<PaymentDeletedAudit> DeletedPayments { get; } = [];

        public Task WritePaymentCreatedAsync(PaymentSnapshot payment, CancellationToken cancellationToken = default)
        {
            CreatedPayments.Add(payment);
            return Task.CompletedTask;
        }

        public Task WritePaymentUpdatedAsync(PaymentUpdateAudit payment, CancellationToken cancellationToken = default)
        {
            UpdatedPayments.Add(payment);
            return Task.CompletedTask;
        }

        public Task WritePaymentDeletedAsync(PaymentDeletedAudit payment, CancellationToken cancellationToken = default)
        {
            DeletedPayments.Add(payment);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public bool WasUsed { get; private set; }

        public Task<T> ExecuteInTransactionAsync<T>(
            Func<CancellationToken, Task<T>> operation,
            CancellationToken cancellationToken = default)
        {
            WasUsed = true;
            return operation(cancellationToken);
        }
    }

    private sealed record DailyPaymentOperation(Guid TenantId, DateTime Date, decimal Amount, string EntryType);
    private sealed record CashApplication(Guid TenantId, Guid PaymentId, decimal Delta, Guid UserId);
    private sealed record CashUpdate(Guid TenantId, Guid PaymentId, decimal OriginalDelta, decimal NewDelta, Guid UserId);
    private sealed record CashVoid(Guid TenantId, Guid PaymentId, decimal OriginalAmount, Guid UserId);
}
