using OpenCashFlow.Application.Auth.Audit;
using OpenCashFlow.Application.Auth.Ports;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities.Admin;

namespace OpenCashFlow.Infrastructure.Auth;

public sealed class AuthAuditWriter(ApplicationDbContext db) : IAuthAuditWriter
{
    public async Task WriteAsync(AuthAuditEvent auditEvent, CancellationToken cancellationToken = default)
    {
        db.Admin_AuditLog_DS.Add(new Admin_AuditLog
        {
            EventType = auditEvent.EventType,
            Resource = auditEvent.Resource,
            Action = auditEvent.Action,
            UserID = auditEvent.UserId,
            TenantID = auditEvent.TenantId,
            Username = auditEvent.Username,
            IPAddress = auditEvent.IpAddress,
            UserAgent = auditEvent.UserAgent,
            Timestamp = auditEvent.TimestampUtc,
            Severity = auditEvent.Severity,
            AdditionalInfo = auditEvent.AdditionalInfo
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
