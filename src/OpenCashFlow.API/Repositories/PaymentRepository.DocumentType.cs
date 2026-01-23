using System.Data;
using OpenCashFlow.API.Helpers;
using OpenCashFlow.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using global::Shared.Data;
using global::Shared.Models;
using static global::Shared.Enums.Permissions.Customers;

namespace OpenCashFlow.API.Repositories
{
    public partial class PaymentRepository : IPaymentRepository
    {       
        public async Task<IEnumerable<Payment_DocumentType_LookUp>?> GetAllDocumentTypesAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Payment_DocumentType_DS.AsNoTracking()
                .Where(p => (p.TenantID == TenantID || p.TenantID == null) && !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<Payment_DocumentType_LookUp?> GetDocumentTypeByIdAsync(Guid DocumentTypeID, Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Payment_DocumentType_DS.AsNoTracking()
                .Where(p => (p.TenantID == TenantID || p.TenantID == null) && p.DocumentTypeID == DocumentTypeID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Payment_DocumentType_LookUp?> AddPaymentDocumentTypeAsync(Payment_DocumentType_LookUp DocumentType, CancellationToken cancellationToken)
        {
            if (DocumentType == null) return null;

            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, cancellationToken);

                _context.Payment_DocumentType_DS.Add(DocumentType);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                return DocumentType;

            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }


        public async Task<Payment_DocumentType_LookUp?> UpdateDocumentTypeAsync(Payment_DocumentType_LookUp DocumentType, CancellationToken cancellationToken)
        {
            if (DocumentType == null || DocumentType.TenantID==null) return null;

            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                _context.Payment_DocumentType_DS.Update(DocumentType);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return DocumentType;

            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }

        public async Task<bool> DeleteDocumentTypeAsync(Guid DocumentTypeID, Guid TenantID, CancellationToken cancellationToken)
        {
            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, cancellationToken);

                var documentType = await _context.Payment_DocumentType_DS
                    .FirstOrDefaultAsync(c => c.DocumentTypeID == DocumentTypeID && c.TenantID == TenantID, cancellationToken);

                if (documentType == null)
                    return false; // not personal or not found

                // Check usage within same company
                var isUsed = await _context.Payment_DS.AsNoTracking()
                    .AnyAsync(p => p.DocumentTypeID == DocumentTypeID && p.TenantID == TenantID, cancellationToken);

                if (!isUsed)
                {
                    _context.Payment_DocumentType_DS.Remove(documentType);
                }
                else
                {
                    documentType.IsDeleted = true;
                    documentType.DateDeleted = DateTime.UtcNow;
                    _context.Payment_DocumentType_DS.Update(documentType);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return true;

            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }

    }
}
