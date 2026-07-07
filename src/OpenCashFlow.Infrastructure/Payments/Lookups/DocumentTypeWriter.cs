using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Infrastructure.Helpers;
using OpenCashFlow.Infrastructure.Persistence;
using OpenCashFlow.Infrastructure.Persistence.Entities;

namespace OpenCashFlow.Infrastructure.Payments.Lookups;

public sealed class DocumentTypeWriter(
    ApplicationDbContext context,
    ILogger<DocumentTypeWriter> logger) : IDocumentTypeWriter
{
    public async Task<DocumentTypeResult?> CreateAsync(DocumentTypeCreateCommand command, CancellationToken cancellationToken = default)
    {
        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var documentType = new Payment_DocumentType_LookUp
            {
                DocumentTypeID = command.DocumentTypeId,
                TenantID = command.TenantId,
                DocumentTypeName = command.Name,
                DocumentTypeDescription = command.Description,
                DocumentTypeIcon = command.Icon,
                Visible = command.Visible,
                DisplayOrder = command.DisplayOrder,
                CreatedBy = command.UserId,
                DateIns = DateTime.UtcNow
            };

            context.Payment_DocumentType_DS.Add(documentType);
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return DocumentTypeLookupMapping.ToResult(documentType);
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);
    }

    public async Task<DocumentTypeResult?> UpdateAsync(DocumentTypeUpdateCommand command, CancellationToken cancellationToken = default)
    {
        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var documentType = await context.Payment_DocumentType_DS
                .FirstOrDefaultAsync(p => p.DocumentTypeID == command.DocumentTypeId && p.TenantID == command.TenantId && !p.IsDeleted, cancellationToken);

            if (documentType == null)
            {
                return null;
            }

            documentType.DocumentTypeName = command.Name;
            documentType.DocumentTypeDescription = command.Description;
            documentType.DocumentTypeIcon = command.Icon;
            documentType.Visible = command.Visible;
            documentType.DisplayOrder = command.DisplayOrder;
            documentType.EditedBy = command.UserId;
            documentType.DateEdit = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return DocumentTypeLookupMapping.ToResult(documentType);
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);
    }

    public async Task<bool> DeleteAsync(Guid documentTypeId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return await RetryHelper.ExecuteWithRetryAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

            var documentType = await context.Payment_DocumentType_DS
                .FirstOrDefaultAsync(p => p.DocumentTypeID == documentTypeId && p.TenantID == tenantId, cancellationToken);

            if (documentType == null)
            {
                return false;
            }

            var isUsed = await context.Payment_DS.AsNoTracking()
                .AnyAsync(p => p.DocumentTypeID == documentTypeId && p.TenantID == tenantId, cancellationToken);

            if (!isUsed)
            {
                context.Payment_DocumentType_DS.Remove(documentType);
            }
            else
            {
                documentType.IsDeleted = true;
                documentType.DateDeleted = DateTime.UtcNow;
                context.Payment_DocumentType_DS.Update(documentType);
            }

            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        }, retryCondition: RetryHelper.IsDeadlock, logger: logger);
    }
}
