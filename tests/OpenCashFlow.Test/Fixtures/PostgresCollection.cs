using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenCashFlow.Test.Fixtures
{
    [CollectionDefinition("Postgres collection")]
    public class PostgresCollection : ICollectionFixture<PostgresContainerFixture> { }

}
