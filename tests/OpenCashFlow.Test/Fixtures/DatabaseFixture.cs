using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using global::Shared.Data;
using OpenCashFlow.Test.Utilities;

namespace OpenCashFlow.Test.Fixtures
{
    public class DatabaseFixture : IAsyncLifetime
    {
        public ApplicationDbContext DbContext { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Console.WriteLine("⚙️ DatabaseFixture INIT");

            // Usa il provider InMemory per evitare la dipendenza da un server PostgreSQL
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            DbContext = new ApplicationDbContext(options);

            Console.WriteLine("⚙️ Migrating database...");
            await DbContext.Database.EnsureDeletedAsync();
            await DbContext.Database.EnsureCreatedAsync();

            Console.WriteLine("⚙️ Seeding test data...");
            TestHelpers.SeedAllTestData(DbContext);
            await DbContext.SaveChangesAsync();

            Console.WriteLine("✅ DatabaseFixture READY");
        }


        public Task DisposeAsync()
        {
            Console.WriteLine("🧹 Disposing DatabaseFixture");
            DbContext?.Dispose();
            return Task.CompletedTask;
        }
    }
}
