using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Infrastructure.Helpers;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Infrastructure.Payments.Lookups;

public sealed class PaymentMethodWriter(
    ApplicationDbContext context,
    ILogger<PaymentMethodWriter> logger) : IPaymentMethodWriter
{
    public async Task<PaymentMethodResult?> CreateAsync(PaymentMethodCreateCommand command, CancellationToken cancellationToken = default)
    {
        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var method = new Payment_Method_LookUps
            {
                PaymentMethodID = command.PaymentMethodId,
                TenantID = command.TenantId,
                PaymentMethodName = command.Name,
                PaymentMethodDescription = command.Description,
                PaymentMethodIcon = command.Icon,
                Visible = command.Visible,
                DisplayOrder = command.DisplayOrder,
                CreatedBy = command.UserId,
                DateIns = DateTime.UtcNow
            };

            context.PaymentMethod_DS.Add(method);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return PaymentMethodLookupMapping.ToResult(method);
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);
    }

    public async Task<PaymentMethodResult?> UpdateAsync(PaymentMethodUpdateCommand command, CancellationToken cancellationToken = default)
    {
        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var method = await context.PaymentMethod_DS
                .FirstOrDefaultAsync(p => p.PaymentMethodID == command.PaymentMethodId && p.TenantID == command.TenantId && !p.IsDeleted, cancellationToken);

            if (method == null)
            {
                return null;
            }

            method.PaymentMethodName = command.Name;
            method.PaymentMethodDescription = command.Description;
            method.PaymentMethodIcon = command.Icon;
            method.Visible = command.Visible;
            method.DisplayOrder = command.DisplayOrder;
            method.EditedBy = command.UserId;
            method.DateEdit = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return PaymentMethodLookupMapping.ToResult(method);
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);
    }

    public async Task<bool> DeleteAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var method = await context.PaymentMethod_DS
                .FirstOrDefaultAsync(p => p.PaymentMethodID == paymentMethodId && p.TenantID == tenantId, cancellationToken);

            if (method == null)
            {
                return false;
            }

            var isUsed = await context.Payment_DS.AsNoTracking()
                .AnyAsync(p => p.PaymentMethodID == paymentMethodId && p.TenantID == tenantId, cancellationToken);

            if (!isUsed)
            {
                context.PaymentMethod_DS.Remove(method);
            }
            else
            {
                method.IsDeleted = true;
                method.DateDeleted = DateTime.UtcNow;
                context.PaymentMethod_DS.Update(method);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);
    }
}
