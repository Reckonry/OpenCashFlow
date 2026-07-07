using OpenCashFlow.Application.Setup.CompleteSetup;
using OpenCashFlow.Application.Setup.GetSetupStatus;
using OpenCashFlow.Application.Setup.Ports;

namespace OpenCashFlow.Application.Tests.Setup;

public sealed class SetupUseCaseTests
{
    [Fact]
    public async Task GetStatus_DelegatesToReader()
    {
        var reader = new FakeSetupReader(new SetupStatusResult(true, false, false));
        var useCase = new GetSetupStatusUseCase(reader);

        var result = await useCase.ExecuteAsync();

        Assert.True(result.RequiresSetup);
        Assert.True(reader.Called);
    }

    [Fact]
    public async Task CompleteSetup_WhenAlreadyConfigured_Fails()
    {
        var reader = new FakeSetupReader(new SetupStatusResult(false, true, true));
        var writer = new FakeSetupWriter();
        var useCase = new CompleteSetupUseCase(reader, writer);

        var result = await useCase.ExecuteAsync(ValidCommand());

        Assert.False(result.Success);
        Assert.Equal(CompleteSetupFailure.AlreadyConfigured, result.Failure);
        Assert.False(writer.Called);
    }

    [Fact]
    public async Task CompleteSetup_WithWeakPassword_FailsBeforeWriter()
    {
        var reader = new FakeSetupReader(new SetupStatusResult(true, false, false));
        var writer = new FakeSetupWriter();
        var useCase = new CompleteSetupUseCase(reader, writer);

        var result = await useCase.ExecuteAsync(ValidCommand() with { AdminPassword = "weak" });

        Assert.False(result.Success);
        Assert.Equal(CompleteSetupFailure.WeakPassword, result.Failure);
        Assert.False(writer.Called);
    }

    [Fact]
    public async Task CompleteSetup_WithValidEmptyInstance_CallsWriter()
    {
        var reader = new FakeSetupReader(new SetupStatusResult(true, false, false));
        var writer = new FakeSetupWriter();
        var useCase = new CompleteSetupUseCase(reader, writer);

        var result = await useCase.ExecuteAsync(ValidCommand());

        Assert.True(result.Success);
        Assert.True(writer.Called);
        Assert.False(result.Status!.RequiresSetup);
    }

    private static CompleteSetupCommand ValidCommand()
    {
        return new CompleteSetupCommand(
            "OpenCashFlow Test",
            "admin@example.local",
            "Str0ng!Pass",
            "Admin",
            "User",
            "it",
            "EUR",
            "Europe/Rome",
            "IT");
    }

    private sealed class FakeSetupReader(SetupStatusResult status) : ISetupReader
    {
        public bool Called { get; private set; }

        public Task<SetupStatusResult> GetStatusAsync(CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(status);
        }
    }

    private sealed class FakeSetupWriter : ISetupWriter
    {
        public bool Called { get; private set; }

        public Task<SetupStatusResult> CompleteAsync(CompleteSetupCommand command, CancellationToken cancellationToken = default)
        {
            Called = true;
            return Task.FromResult(new SetupStatusResult(false, true, true));
        }
    }
}
