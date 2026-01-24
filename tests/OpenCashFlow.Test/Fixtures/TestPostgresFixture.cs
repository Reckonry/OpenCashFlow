using OpenCashFlow.Test.Factories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Test.Fixtures
{
    public class TestPostgresFixture : IDisposable
    {
        public CustomWebApplicationFactory Factory { get; }

        public TestPostgresFixture()
        {
            // Usa la connessione di test al tuo Postgres container
            var connectionString = "Host=localhost;Database=OpenCashFlow.Test;Username=opencashflow;Password=SuperSoldi2222!";

            Factory = new CustomWebApplicationFactory(connectionString, useFakeAuth: false);
        }

        public void Dispose()
        {
            Factory.Dispose();
        }
    }

}
