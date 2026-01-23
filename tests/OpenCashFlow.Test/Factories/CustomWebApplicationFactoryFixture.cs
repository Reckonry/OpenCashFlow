using OpenCashFlow.Test.Factories;

public class CustomWebApplicationFactoryFixture : IDisposable
{
    public CustomWebApplicationFactory Factory { get; }

    public CustomWebApplicationFactoryFixture()
    {
        // Usa un database in-memory con nome univoco per ogni test run
        var uniqueDbName = $"TestDb_{Guid.NewGuid():N}";

        var useFakeAuth = true;

        Factory = new CustomWebApplicationFactory(uniqueDbName, useFakeAuth);
    }

    public void Dispose()
    {
        Factory?.Dispose();
    }
}
