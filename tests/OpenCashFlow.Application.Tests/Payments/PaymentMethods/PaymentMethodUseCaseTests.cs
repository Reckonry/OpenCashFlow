using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.PaymentMethods;
using OpenCashFlow.Application.Payments.Ports;

namespace OpenCashFlow.Application.Tests.Payments.PaymentMethods;

public sealed class PaymentMethodUseCaseTests
{
    [Fact]
    public async Task GetAll_WithEmptyTenant_Fails()
    {
        var useCase = new GetPaymentMethodsUseCase(new FakePaymentMethodReader());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(new GetPaymentMethodsQuery(Guid.Empty)));
    }

    [Fact]
    public async Task Create_WithEmptyName_Fails()
    {
        var useCase = new CreatePaymentMethodUseCase(new FakePaymentMethodWriter());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            useCase.ExecuteAsync(ValidCreateCommand() with { Name = " " }));
    }

    [Fact]
    public async Task Create_WithValidCommand_NormalizesAndPassesToWriter()
    {
        var writer = new FakePaymentMethodWriter();
        var useCase = new CreatePaymentMethodUseCase(writer);

        var result = await useCase.ExecuteAsync(ValidCreateCommand(name: "  Contanti  "));

        Assert.NotNull(result);
        Assert.NotNull(writer.LastCreate);
        Assert.Equal("Contanti", writer.LastCreate!.Name);
        Assert.NotEqual(Guid.Empty, writer.LastCreate.PaymentMethodId);
    }

    [Fact]
    public async Task Update_WhenWriterReturnsNull_ReturnsNull()
    {
        var writer = new FakePaymentMethodWriter { ReturnNullOnUpdate = true };
        var useCase = new UpdatePaymentMethodUseCase(writer);

        var result = await useCase.ExecuteAsync(ValidUpdateCommand());

        Assert.Null(result);
        Assert.NotNull(writer.LastUpdate);
    }

    [Fact]
    public async Task Delete_PassesTenantAndIdToWriter()
    {
        var writer = new FakePaymentMethodWriter();
        var useCase = new DeletePaymentMethodUseCase(writer);
        var tenantId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();

        var deleted = await useCase.ExecuteAsync(new DeletePaymentMethodCommand(paymentMethodId, tenantId));

        Assert.True(deleted);
        Assert.Equal(tenantId, writer.LastDeleteTenantId);
        Assert.Equal(paymentMethodId, writer.LastDeletePaymentMethodId);
    }

    [Fact]
    public async Task ListAndDetail_CallReader()
    {
        var reader = new FakePaymentMethodReader();
        var tenantId = Guid.NewGuid();
        var paymentMethodId = Guid.NewGuid();

        await new GetPaymentMethodsUseCase(reader).ExecuteAsync(new GetPaymentMethodsQuery(tenantId));
        await new GetPaymentMethodDetailUseCase(reader).ExecuteAsync(new GetPaymentMethodDetailQuery(paymentMethodId, tenantId));

        Assert.Equal(tenantId, reader.LastListTenantId);
        Assert.Equal(paymentMethodId, reader.LastDetailPaymentMethodId);
        Assert.Equal(tenantId, reader.LastDetailTenantId);
    }

    private static PaymentMethodCreateCommand ValidCreateCommand(string name = "Card")
    {
        return new PaymentMethodCreateCommand(
            Guid.Empty,
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            "Card payment",
            "credit-card",
            true,
            10);
    }

    private static PaymentMethodUpdateCommand ValidUpdateCommand(string name = "Card")
    {
        return new PaymentMethodUpdateCommand(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            name,
            "Card payment",
            "credit-card",
            true,
            10);
    }

    private sealed class FakePaymentMethodReader : IPaymentMethodReader
    {
        public Guid LastListTenantId { get; private set; }
        public Guid LastDetailPaymentMethodId { get; private set; }
        public Guid LastDetailTenantId { get; private set; }

        public Task<IReadOnlyList<PaymentMethodListItem>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
        {
            LastListTenantId = tenantId;
            return Task.FromResult<IReadOnlyList<PaymentMethodListItem>>([]);
        }

        public Task<PaymentMethodResult?> GetByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            LastDetailPaymentMethodId = paymentMethodId;
            LastDetailTenantId = tenantId;
            return Task.FromResult<PaymentMethodResult?>(null);
        }
    }

    private sealed class FakePaymentMethodWriter : IPaymentMethodWriter
    {
        public PaymentMethodCreateCommand? LastCreate { get; private set; }
        public PaymentMethodUpdateCommand? LastUpdate { get; private set; }
        public Guid LastDeletePaymentMethodId { get; private set; }
        public Guid LastDeleteTenantId { get; private set; }
        public bool ReturnNullOnUpdate { get; init; }

        public Task<PaymentMethodResult?> CreateAsync(PaymentMethodCreateCommand command, CancellationToken cancellationToken = default)
        {
            LastCreate = command;
            return Task.FromResult<PaymentMethodResult?>(ToResult(command));
        }

        public Task<PaymentMethodResult?> UpdateAsync(PaymentMethodUpdateCommand command, CancellationToken cancellationToken = default)
        {
            LastUpdate = command;
            return Task.FromResult(ReturnNullOnUpdate ? null : ToResult(command));
        }

        public Task<bool> DeleteAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
        {
            LastDeletePaymentMethodId = paymentMethodId;
            LastDeleteTenantId = tenantId;
            return Task.FromResult(true);
        }

        private static PaymentMethodResult ToResult(PaymentMethodCreateCommand command)
        {
            return new PaymentMethodResult(
                command.PaymentMethodId,
                command.TenantId,
                command.Name,
                command.Description,
                command.Icon,
                command.Visible,
                command.DisplayOrder,
                IsDeleted: false,
                IsDeletedBy: null,
                IsDeletedWhy: null,
                DateDeleted: null,
                CreatedBy: command.UserId,
                DateIns: DateTime.UtcNow,
                EditedBy: null,
                DateEdit: null);
        }

        private static PaymentMethodResult ToResult(PaymentMethodUpdateCommand command)
        {
            return new PaymentMethodResult(
                command.PaymentMethodId,
                command.TenantId,
                command.Name,
                command.Description,
                command.Icon,
                command.Visible,
                command.DisplayOrder,
                IsDeleted: false,
                IsDeletedBy: null,
                IsDeletedWhy: null,
                DateDeleted: null,
                CreatedBy: null,
                DateIns: DateTime.UtcNow,
                EditedBy: command.UserId,
                DateEdit: DateTime.UtcNow);
        }
    }
}
