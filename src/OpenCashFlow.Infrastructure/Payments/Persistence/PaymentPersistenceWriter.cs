using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Persistence;
using OpenCashFlow.Infrastructure.Helpers;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Payments.Persistence;

public sealed class PaymentPersistenceWriter(
    ApplicationDbContext context,
    ILogger<PaymentPersistenceWriter> logger) : IPaymentPersistenceWriter, IPaymentWriter
{
    public async Task<PaymentSnapshot?> CreateAsync(PaymentDraft payment, CancellationToken cancellationToken = default)
    {
        var entity = PaymentPersistenceMapping.ToEntity(payment);

        var methodExists = await context.PaymentMethod_DS.AsNoTracking()
            .Where(dt => dt.TenantID == entity.TenantID || dt.TenantID == null)
            .AnyAsync(pm => pm.PaymentMethodID == entity.PaymentMethodID, cancellationToken);

        if (!methodExists)
        {
            logger.LogWarning("Invalid PaymentMethodID {PaymentMethodID} for CompanyID {CompanyID}", entity.PaymentMethodID, entity.TenantID);
            throw new ArgumentException($"Invalid PaymentMethodID: {entity.PaymentMethodID}", nameof(payment.PaymentMethodId));
        }

        var docExists = await context.Payment_DocumentType_DS.AsNoTracking()
            .Where(dt => dt.TenantID == entity.TenantID || dt.TenantID == null)
            .AnyAsync(dt => dt.DocumentTypeID == entity.DocumentTypeID, cancellationToken);

        if (!docExists)
        {
            logger.LogWarning("Invalid DocumentTypeID {DocumentTypeID} for CompanyID {CompanyID}", entity.DocumentTypeID, entity.TenantID);
            throw new ArgumentException($"Invalid DocumentTypeID: {entity.DocumentTypeID}", nameof(payment.DocumentTypeId));
        }

        var userExists = await context.AspNetUser_DS.AsNoTracking()
            .AnyAsync(u => u.UserID == entity.UserID, cancellationToken);

        if (!userExists)
        {
            logger.LogWarning("Invalid UserID {UserID}", entity.UserID);
            throw new ArgumentException($"Invalid UserID: {entity.UserID}", nameof(payment.UserId));
        }

        var created = await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            context.Payment_DS.Add(entity);
            await context.SaveChangesAsync(cancellationToken);
            return entity;
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);

        return created == null ? null : PaymentPersistenceMapping.ToSnapshot(created);
    }

    public async Task<PaymentSnapshot?> UpdateAsync(PaymentUpdateDraft payment, CancellationToken cancellationToken = default)
    {
        var existingPayment = await context.Payment_DS
            .Where(p => p.PaymentID == payment.PaymentId && p.TenantID == payment.TenantId && !p.IsDeleted)
            .FirstOrDefaultAsync(cancellationToken);

        if (existingPayment == null)
        {
            return null;
        }

        PaymentPersistenceMapping.ApplyUpdate(existingPayment, payment);

        var updated = await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            context.Payment_DS.Update(existingPayment);
            await context.SaveChangesAsync(cancellationToken);
            return existingPayment;
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);

        return updated == null ? null : PaymentPersistenceMapping.ToSnapshot(updated);
    }

    public async Task<bool> SoftDeleteAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            var payment = await context.Payment_DS
                .FirstOrDefaultAsync(c => c.PaymentID == paymentId && c.TenantID == tenantId && !c.IsDeleted, cancellationToken);

            if (payment == null)
            {
                return false;
            }

            payment.IsDeleted = true;
            payment.DateDeleted = DateTime.UtcNow;
            payment.IsDeletedWhy = "User requested payment deletion";
            await context.SaveChangesAsync(cancellationToken);

            return true;
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);
    }

    public Task<bool> DeleteAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return SoftDeleteAsync(paymentId, tenantId, cancellationToken);
    }
}
