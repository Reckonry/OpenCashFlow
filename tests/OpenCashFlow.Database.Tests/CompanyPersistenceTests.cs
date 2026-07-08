using Microsoft.EntityFrameworkCore;

namespace OpenCashFlow.Database.Tests;

public sealed class CompanyPersistenceTests(DatabaseTestFixture fixture) : IClassFixture<DatabaseTestFixture>
{
    [Fact]
    public async Task Company_soft_delete_flags_are_persisted()
    {
        await using var db = fixture.CreateDbContext();
        var company = PersistenceTestData.Company($"Soft Delete {Guid.NewGuid():N}");
        var deletedBy = Guid.NewGuid();
        var deletedAt = DateTime.UtcNow;

        db.Company_DS.Add(company);
        await db.SaveChangesAsync();

        company.IsDeleted = true;
        company.IsDeletedBy = deletedBy;
        company.IsDeletedWhy = "database integration test";
        company.DateDeleted = deletedAt;
        await db.SaveChangesAsync();

        var persisted = await db.Company_DS
            .AsNoTracking()
            .SingleAsync(x => x.TenantID == company.TenantID);

        Assert.True(persisted.IsDeleted);
        Assert.Equal(deletedBy, persisted.IsDeletedBy);
        Assert.Equal("database integration test", persisted.IsDeletedWhy);
        Assert.NotNull(persisted.DateDeleted);
    }

    [Fact]
    public async Task Company_optional_fields_can_be_null()
    {
        await using var db = fixture.CreateDbContext();
        var company = PersistenceTestData.Company($"Optional Nulls {Guid.NewGuid():N}");
        company.BusinessCategory = null;
        company.Website = null;
        company.EstimatedAnnualRevenue = null;
        company.SocialLinks = null;
        company.IBAN = null;
        company.DefaultCurrency = null;

        db.Company_DS.Add(company);
        await db.SaveChangesAsync();

        var persisted = await db.Company_DS
            .AsNoTracking()
            .SingleAsync(x => x.TenantID == company.TenantID);

        Assert.Null(persisted.BusinessCategory);
        Assert.Null(persisted.Website);
        Assert.Null(persisted.EstimatedAnnualRevenue);
        Assert.Null(persisted.SocialLinks);
        Assert.Null(persisted.IBAN);
        Assert.Null(persisted.DefaultCurrency);
    }
}
