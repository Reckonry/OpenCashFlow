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
    public async Task CompleteSetup_WithInvalidInput_FailsBeforeWriter()
    {
        var reader = new FakeSetupReader(new SetupStatusResult(true, false, false));
        var writer = new FakeSetupWriter();
        var useCase = new CompleteSetupUseCase(reader, writer);

        var result = await useCase.ExecuteAsync(ValidCommand() with { CompanyName = " " });

        Assert.False(result.Success);
        Assert.Equal(CompleteSetupFailure.InvalidInput, result.Failure);
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
        Assert.NotNull(result.TemporaryAdminPassword);
        Assert.Equal(result.TemporaryAdminPassword, writer.TemporaryAdminPassword);
        Assert.True(IsStrongPassword(result.TemporaryAdminPassword));
    }

    private static CompleteSetupCommand ValidCommand()
    {
        return new CompleteSetupCommand(
            "OpenCashFlow Test",
            "admin@example.local",
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
        public string? TemporaryAdminPassword { get; private set; }

        public Task<SetupStatusResult> CompleteAsync(
            CompleteSetupCommand command,
            string temporaryAdminPassword,
            CancellationToken cancellationToken = default)
        {
            Called = true;
            TemporaryAdminPassword = temporaryAdminPassword;
            return Task.FromResult(new SetupStatusResult(false, true, true));
        }
    }

    private static bool IsStrongPassword(string password)
    {
        return password.Length >= 16
            && password.Any(char.IsUpper)
            && password.Any(char.IsLower)
            && password.Any(char.IsDigit)
            && password.Any(ch => !char.IsLetterOrDigit(ch));
    }
}
