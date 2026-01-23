using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Shared.Data;

namespace Shared.Data
{
    public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            // Usa la tua connessione PostgreSQL
            optionsBuilder.UseNpgsql("Host=localhost;Database=opencashflow_db;Username=opencashflow;Password=SuperSoldi2222!;");

            return new ApplicationDbContext(optionsBuilder.Options);
        }
    }
}
