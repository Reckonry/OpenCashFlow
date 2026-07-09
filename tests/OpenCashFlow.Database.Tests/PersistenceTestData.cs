using OpenCashFlow.Infrastructure.Persistence.Entities;
using OpenCashFlow.Infrastructure.Persistence.Entities.Identity;

namespace OpenCashFlow.Database.Tests;

internal static class PersistenceTestData
{
    public static Company Company(string name)
    {
        return new Company
        {
            TenantID = Guid.NewGuid(),
            CompanyName = name,
            MaxUsers = 10,
            PriorityLevel = 1,
            StartingContract = DateTime.UtcNow.AddDays(-1),
            EndingContract = DateTime.UtcNow.AddYears(1),
            GdprConsent = true,
            ContractAcepted = true,
            CompanySecret = $"secret-{Guid.NewGuid():N}",
            IsActive = true
        };
    }

    public static AspNetUser User(string email)
    {
        return new AspNetUser
        {
            UserID = Guid.NewGuid(),
            UserName = email,
            UserFirstName = "Integration",
            UserLastName = "Tester",
            Email = email,
            EmailConfirmed = true,
            IsApproved = true,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
    }

    public static Payment_Method_LookUps PaymentMethod(Guid tenantId, string name)
    {
        return new Payment_Method_LookUps
        {
            PaymentMethodID = Guid.NewGuid(),
            TenantID = tenantId,
            PaymentMethodName = name,
            PaymentMethodDescription = "Integration test method",
            Visible = true,
            DisplayOrder = 10,
            IsDeleted = false,
            DateIns = DateTime.UtcNow
        };
    }

    public static Payment_DocumentType_LookUp DocumentType(Guid tenantId, string name)
    {
        return new Payment_DocumentType_LookUp
        {
            DocumentTypeID = Guid.NewGuid(),
            TenantID = tenantId,
            DocumentTypeName = name,
            DocumentTypeDescription = "Integration test doc",
            Visible = true,
            DisplayOrder = 10,
            IsDeleted = false,
            DateIns = DateTime.UtcNow
        };
    }

    public static Payment Payment(
        Guid tenantId,
        Guid userId,
        Guid paymentMethodId,
        Guid documentTypeId)
    {
        return new Payment
        {
            PaymentID = Guid.NewGuid(),
            TenantID = tenantId,
            RequestId = Guid.NewGuid(),
            Amount = 42.5,
            EntryType = "Income",
            PaymentMethodID = paymentMethodId,
            DocumentTypeID = documentTypeId,
            UserID = userId,
            Description = "Database integration test payment",
            DateIns = DateTime.UtcNow
        };
    }
}
