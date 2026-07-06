using OpenCashFlow.Domain.Common;
using OpenCashFlow.Domain.Payments;

namespace OpenCashFlow.Domain.Tests.Common;

public sealed class IdentityValueObjectTests
{
    [Fact]
    public void TenantId_rejects_empty_guid()
    {
        Assert.Throws<ArgumentException>(() => TenantId.From(Guid.Empty));
    }

    [Fact]
    public void UserId_rejects_empty_guid()
    {
        Assert.Throws<ArgumentException>(() => UserId.From(Guid.Empty));
    }

    [Fact]
    public void PaymentId_rejects_empty_guid()
    {
        Assert.Throws<ArgumentException>(() => PaymentId.From(Guid.Empty));
    }
}

