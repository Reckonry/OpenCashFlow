using System.Data;
using System.Xml.Linq;
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
        public async Task<IEnumerable<Payment_Method_LookUps>?> GetAllPaymentMethodsAsync(Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.PaymentMethod_DS.AsNoTracking()
                .Where(p => (p.TenantID == TenantID || p.TenantID == null) && !p.IsDeleted)
                .OrderBy(p => p.DisplayOrder)
                .ToListAsync(cancellationToken);
        }

        public async Task<Payment_Method_LookUps?> GetPaymentMethodByIdAsync(Guid PaymentMethodID, Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.PaymentMethod_DS.AsNoTracking()
                .Where(p => (p.TenantID == TenantID || p.TenantID == null) && p.PaymentMethodID == PaymentMethodID)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Payment_Method_LookUps?> AddPaymentMethodAsync(Payment_Method_LookUps paymentMethod, CancellationToken cancellationToken)
        {
            if (paymentMethod == null) return null;

            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, cancellationToken);

                _context.PaymentMethod_DS.Add(paymentMethod);
                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return paymentMethod;

            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }


        public async Task<Payment_Method_LookUps?> UpdatePaymentMethodAsync(Payment_Method_LookUps paymentMethod, CancellationToken cancellationToken)
        {
            if (paymentMethod == null || paymentMethod.TenantID == null) return null;

            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
                _context.PaymentMethod_DS.Update(paymentMethod);
                await _context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);
                return paymentMethod;

            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }

        public async Task<bool> DeletePaymentMethodAsync(Guid PaymentMethodID, Guid TenantID, CancellationToken cancellationToken)
        {
            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(
                    IsolationLevel.ReadCommitted, cancellationToken);

                var paymentMethod = await _context.PaymentMethod_DS
                    .FirstOrDefaultAsync(c => c.PaymentMethodID == PaymentMethodID && c.TenantID == TenantID, cancellationToken);

                if (paymentMethod == null)
                    return false; // not personal or not found

                var isUsed = await _context.Payment_DS.AsNoTracking()
                    .AnyAsync(p => p.PaymentMethodID == PaymentMethodID && p.TenantID == TenantID, cancellationToken);

                if (!isUsed)
                {
                    _context.PaymentMethod_DS.Remove(paymentMethod);
                }
                else
                {
                    paymentMethod.IsDeleted = true;
                    paymentMethod.DateDeleted = DateTime.UtcNow;
                    _context.PaymentMethod_DS.Update(paymentMethod);
                }

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return true;

            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }


    }
}
