using OpenCashFlow.Test.Fixtures;
using Xunit;

namespace OpenCashFlow.Test
{
    [CollectionDefinition("NonParallelCollection", DisableParallelization = true)]
    public class NonParallelCollection : ICollectionFixture<CustomWebApplicationFactoryFixture>
    {
        // No code needed here, the collection definition is used to group tests that should not run in parallel
    }

    [CollectionDefinition("PostgresCollection", DisableParallelization = true)]
    public class PostgresCollection : ICollectionFixture<PostgresContainerFixture>
    {
        // This collection uses a real PostgreSQL database via Testcontainers
        // Provides more realistic testing environment including transaction support
    }
}
