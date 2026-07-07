using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Auth.Audit;
using OpenCashFlow.Infrastructure.Auth;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Test.Tests.Unit;

public sealed class AuthAuditWriter_Tests
{
    [Fact]
    public async Task WriteAsync_PersistsAuthenticationAuditEvent()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"auth-audit-{Guid.NewGuid()}")
            .Options;

        await using var db = new ApplicationDbContext(options);
        var writer = new AuthAuditWriter(db);
        var userId = Guid.NewGuid();
        var tenantId = Guid.NewGuid();

        await writer.WriteAsync(new AuthAuditEvent(
            EventType: "Login",
            Action: "FastLogin",
            Resource: "Authentication",
            UserId: userId,
            TenantId: tenantId,
            Username: "admin@example.local",
            IpAddress: "127.0.0.1",
            UserAgent: "test-agent",
            Severity: "Info",
            AdditionalInfo: "ok",
            TimestampUtc: DateTime.UtcNow));

        var audit = await db.Admin_AuditLog_DS.SingleAsync();
        Assert.Equal("Login", audit.EventType);
        Assert.Equal("Authentication", audit.Resource);
        Assert.Equal("FastLogin", audit.Action);
        Assert.Equal(userId, audit.UserID);
        Assert.Equal(tenantId, audit.TenantID);
        Assert.Equal("admin@example.local", audit.Username);
        Assert.Equal("127.0.0.1", audit.IPAddress);
        Assert.Equal("test-agent", audit.UserAgent);
        Assert.Equal("Info", audit.Severity);
        Assert.Equal("ok", audit.AdditionalInfo);
    }
}
