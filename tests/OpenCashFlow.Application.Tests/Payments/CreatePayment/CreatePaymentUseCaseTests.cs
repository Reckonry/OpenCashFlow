using OpenCashFlow.Application.Payments.CreatePayment;

namespace OpenCashFlow.Application.Tests.Payments.CreatePayment;

public sealed class CreatePaymentUseCaseTests
{
    private readonly CreatePaymentUseCase _useCase = new();

    [Fact]
    public async Task ExecuteAsync_WithValidCommand_ReturnsNormalizedResult()
    {
        var command = ValidCommand(entryType: "income", amount: 125.50m);

        var result = await _useCase.ExecuteAsync(command);

        Assert.Equal(command.PaymentId, result.PaymentId);
        Assert.Equal(command.TenantId, result.TenantId);
        Assert.Equal(command.UserId, result.UserId);
        Assert.Equal(command.RequestId, result.RequestId);
        Assert.Equal(125.50m, result.Amount);
        Assert.Equal("Income", result.NormalizedEntryType);
        Assert.Equal(125.50m, result.CashDelta);
        Assert.Equal(DateTimeKind.Utc, result.DateIns.Kind);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("-10")]
    public async Task ExecuteAsync_WithZeroOrNegativeAmount_Fails(string amount)
    {
        var command = ValidCommand(amount: decimal.Parse(amount));

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() => _useCase.ExecuteAsync(command));
    }

    [Fact]
    public async Task ExecuteAsync_WithInvalidEntryType_Fails()
    {
        var command = ValidCommand(entryType: "Transfer");

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(command));
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyTenantId_Fails()
    {
        var command = ValidCommand() with { TenantId = Guid.Empty };

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(command));
    }

    [Fact]
    public async Task ExecuteAsync_WithEmptyUserId_Fails()
    {
        var command = ValidCommand() with { UserId = Guid.Empty };

        await Assert.ThrowsAsync<ArgumentException>(() => _useCase.ExecuteAsync(command));
    }

    [Fact]
    public async Task ExecuteAsync_WithOutcome_ReturnsNegativeCashDelta()
    {
        var command = ValidCommand(entryType: "Outcome", amount: 42m);

        var result = await _useCase.ExecuteAsync(command);

        Assert.Equal("Outcome", result.NormalizedEntryType);
        Assert.Equal(-42m, result.CashDelta);
    }

    [Fact]
    public async Task ExecuteAsync_WithIncome_ReturnsPositiveCashDelta()
    {
        var command = ValidCommand(entryType: "Income", amount: 42m);

        var result = await _useCase.ExecuteAsync(command);

        Assert.Equal("Income", result.NormalizedEntryType);
        Assert.Equal(42m, result.CashDelta);
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
            "Application test payment");
    }
}
