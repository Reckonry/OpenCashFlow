using System.Text.Json;
using OpenCashFlow.Application.Abstractions;
using global::Shared.Data;
using global::Shared.Enums;
using global::Shared.Models.Admin;

namespace OpenCashFlow.Infrastructure.Audit;

public sealed class AuditRepository(ApplicationDbContext db) : IAuditRepository
{
    public async Task WritePaymentCreatedAsync(PaymentSnapshot payment, CancellationToken cancellationToken = default)
    {
        var changes = new
        {
            payment.Amount,
            EntryType = payment.EntryType,
            PaymentMethodID = payment.PaymentMethodId,
            DocumentTypeID = payment.DocumentTypeId
        };

        db.Admin_AuditLog_DS.Add(new Admin_AuditLog
        {
            EventType = AuditEventType.PaymentCreated.ToString(),
            Resource = "Payment",
            Action = "Create",
            ResourceID = payment.PaymentId.ToString(),
            UserID = payment.UserId,
            Changes = JsonSerializer.Serialize(changes),
            Timestamp = DateTime.UtcNow,
            Severity = "Info",
            TenantID = payment.TenantId
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task WritePaymentUpdatedAsync(PaymentUpdateAudit payment, CancellationToken cancellationToken = default)
    {
        var changes = new
        {
            Amount = new { Before = payment.Before.Amount, After = payment.After.Amount },
            EntryType = new { Before = payment.Before.EntryType, After = payment.After.EntryType },
            PaymentMethodID = new { Before = payment.Before.PaymentMethodId, After = payment.After.PaymentMethodId }
        };

        db.Admin_AuditLog_DS.Add(new Admin_AuditLog
        {
            EventType = AuditEventType.PaymentUpdated.ToString(),
            Resource = "Payment",
            Action = "Update",
            ResourceID = payment.After.PaymentId.ToString(),
            UserID = payment.ActorUserId,
            Changes = JsonSerializer.Serialize(changes),
            Timestamp = DateTime.UtcNow,
            Severity = "Info",
            TenantID = payment.After.TenantId
        });

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task WritePaymentDeletedAsync(PaymentDeletedAudit payment, CancellationToken cancellationToken = default)
    {
        var changes = new
        {
            payment.Payment.Amount,
            EntryType = payment.Payment.EntryType,
            PaymentMethodID = payment.Payment.PaymentMethodId
        };

        db.Admin_AuditLog_DS.Add(new Admin_AuditLog
        {
            EventType = AuditEventType.PaymentDeleted.ToString(),
            Resource = "Payment",
            Action = "SoftDelete",
            ResourceID = payment.Payment.PaymentId.ToString(),
            UserID = payment.ActorUserId,
            Changes = JsonSerializer.Serialize(changes),
            Timestamp = DateTime.UtcNow,
            Severity = "Info",
            TenantID = payment.Payment.TenantId
        });

        await db.SaveChangesAsync(cancellationToken);
    }
}
