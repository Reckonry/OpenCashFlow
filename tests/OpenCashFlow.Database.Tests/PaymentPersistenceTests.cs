using Microsoft.EntityFrameworkCore;

namespace OpenCashFlow.Database.Tests;

public sealed class PaymentPersistenceTests(DatabaseTestFixture fixture) : IClassFixture<DatabaseTestFixture>
{
    [Fact]
    public async Task Payment_insert_with_unknown_payment_method_fails_fk_constraint()
    {
        await using var db = fixture.CreateDbContext();
        var seed = await SeedPaymentGraphAsync(db);
        var payment = PersistenceTestData.Payment(
            seed.CompanyId,
            seed.UserId,
            Guid.NewGuid(),
            seed.DocumentTypeId);

        db.Payment_DS.Add(payment);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Payment_insert_with_unknown_document_type_fails_fk_constraint()
    {
        await using var db = fixture.CreateDbContext();
        var seed = await SeedPaymentGraphAsync(db);
        var payment = PersistenceTestData.Payment(
            seed.CompanyId,
            seed.UserId,
            seed.PaymentMethodId,
            Guid.NewGuid());

        db.Payment_DS.Add(payment);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Payment_insert_with_unknown_user_fails_fk_constraint()
    {
        await using var db = fixture.CreateDbContext();
        var seed = await SeedPaymentGraphAsync(db);
        var payment = PersistenceTestData.Payment(
            seed.CompanyId,
            Guid.NewGuid(),
            seed.PaymentMethodId,
            seed.DocumentTypeId);

        db.Payment_DS.Add(payment);

        await Assert.ThrowsAsync<DbUpdateException>(() => db.SaveChangesAsync());
    }

    [Fact]
    public async Task Deleting_referenced_payment_method_cascades_payment_with_current_schema()
    {
        await using var db = fixture.CreateDbContext();
        var seed = await SeedPaymentGraphAsync(db);
        var payment = PersistenceTestData.Payment(
            seed.CompanyId,
            seed.UserId,
            seed.PaymentMethodId,
            seed.DocumentTypeId);

        db.Payment_DS.Add(payment);
        await db.SaveChangesAsync();

        var method = await db.PaymentMethod_DS.SingleAsync(x => x.PaymentMethodID == seed.PaymentMethodId);
        db.PaymentMethod_DS.Remove(method);
        await db.SaveChangesAsync();

        var paymentExists = await db.Payment_DS.AnyAsync(x => x.PaymentID == payment.PaymentID);

        Assert.False(paymentExists);
    }

    private static async Task<PaymentSeed> SeedPaymentGraphAsync(OpenCashFlow.Infrastructure.Persistence.ApplicationDbContext db)
    {
        var suffix = Guid.NewGuid().ToString("N");
        var company = PersistenceTestData.Company($"Payment Company {suffix}");
        var user = PersistenceTestData.User($"payment-{suffix}@example.test");
        var method = PersistenceTestData.PaymentMethod(company.TenantID, $"Method {suffix}");
        var documentType = PersistenceTestData.DocumentType(company.TenantID, $"Doc {suffix}");

        db.Company_DS.Add(company);
        db.AspNetUser_DS.Add(user);
        db.PaymentMethod_DS.Add(method);
        db.Payment_DocumentType_DS.Add(documentType);
        await db.SaveChangesAsync();

        return new PaymentSeed(
            company.TenantID,
            user.UserID,
            method.PaymentMethodID,
            documentType.DocumentTypeID);
    }

    private sealed record PaymentSeed(
        Guid CompanyId,
        Guid UserId,
        Guid PaymentMethodId,
        Guid DocumentTypeId);
}
