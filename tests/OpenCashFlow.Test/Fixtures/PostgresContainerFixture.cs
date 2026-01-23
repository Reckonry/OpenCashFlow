using OpenCashFlow.Test.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OpenCashFlow.Test.Fixtures
{
    public class PostgresContainerFixture : IAsyncLifetime
    {
        public PostgreSqlTestContainerFactory ContainerFactory { get; private set; } = null!;
        public CustomWebApplicationFactory AppFactory { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            // Avvia container UNA SOLA VOLTA
            ContainerFactory = new PostgreSqlTestContainerFactory();
            await ContainerFactory.StartAsync();

            // Crea la Factory con la connessione del container
            AppFactory = new CustomWebApplicationFactory(ContainerFactory.ConnectionString);
        }

        public async Task DisposeAsync()
        {
            await ContainerFactory.StopAsync();
        }
    }

}
