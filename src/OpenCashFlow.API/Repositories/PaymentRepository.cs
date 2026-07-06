using System.Data;
using OpenCashFlow.API.Helpers;
using OpenCashFlow.API.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using global::Shared.Data;
using global::Shared.DTOs;
using global::Shared.Models;
using static global::Shared.Enums.Permissions.Customers;
//todo: dotnet add package System.Linq.Dynamic.Core

namespace OpenCashFlow.API.Repositories
{
    public partial class PaymentRepository(ApplicationDbContext context, ILogger<PaymentRepository> logger) : IPaymentRepository
    {
        private readonly ApplicationDbContext _context = context;
        private readonly ILogger<PaymentRepository> _logger = logger;

        public async Task<IEnumerable<Payment>?> GetAllPaymentsAsync(Guid TenantID, Payment_Filter_DTO filters, CancellationToken cancellationToken)
        {
             var query = _context.Payment_DS.AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.PaymentMethod)
                .Include(p => p.DocumentType)
                .Where(p => p.TenantID == TenantID);

            if (filters.PaymentID.HasValue)
                query = query.Where(p => p.PaymentID == filters.PaymentID.Value);
            if (!string.IsNullOrWhiteSpace(filters.EntryType))
                query = query.Where(p => p.EntryType == filters.EntryType);
            if (filters.PaymentMethodID.HasValue)
                query = query.Where(p => p.PaymentMethodID == filters.PaymentMethodID.Value);
            if (filters.DocumentTypeID.HasValue)
                query = query.Where(p => p.DocumentTypeID == filters.DocumentTypeID.Value);
            if (filters.UserID.HasValue)
                query = query.Where(p => p.UserID == filters.UserID.Value);
            query = filters.IsDeleted.HasValue
                ? query.Where(p => p.IsDeleted == filters.IsDeleted.Value)
                : query.Where(p => !p.IsDeleted);
            if (filters.FromDate.HasValue)
                query = query.Where(p => p.DateIns >= filters.FromDate.Value.ToUniversalTime());
            if (filters.ToDate.HasValue)
                query = query.Where(p => p.DateIns <= filters.ToDate.Value.ToUniversalTime());
            if (filters.MinAmount.HasValue)
                query = query.Where(p => p.Amount >= filters.MinAmount.Value);
            if (filters.MaxAmount.HasValue)
                query = query.Where(p => p.Amount <= filters.MaxAmount.Value);
            if (!string.IsNullOrWhiteSpace(filters.Description))
                query = query.Where(p => p.Description != null && EF.Functions.ILike(p.Description, $"%{filters.Description}%"));
         
            query = (filters.SortBy ?? "DateIns") switch
            {
                "Amount" => filters.Desc ? query.OrderByDescending(p => p.Amount) : query.OrderBy(p => p.Amount),
                "EntryType" => filters.Desc ? query.OrderByDescending(p => p.EntryType) : query.OrderBy(p => p.EntryType),
                "PaymentMethod" => filters.Desc
                    ? query.OrderByDescending(p => p.PaymentMethod != null ? p.PaymentMethod.PaymentMethodName : string.Empty)
                    : query.OrderBy(p => p.PaymentMethod != null ? p.PaymentMethod.PaymentMethodName : string.Empty),
                "DocumentType" => filters.Desc
                    ? query.OrderByDescending(p => p.DocumentType != null ? p.DocumentType.DocumentTypeName : string.Empty)
                    : query.OrderBy(p => p.DocumentType != null ? p.DocumentType.DocumentTypeName : string.Empty),
                "Created" or "DateIns" => filters.Desc ? query.OrderByDescending(p => p.DateIns) : query.OrderBy(p => p.DateIns),
                _ => query.OrderByDescending(p => p.DateIns)
            };

            // Paginazione
            var page = Math.Max(filters.Page, 1);
            var pageSize = Math.Clamp(filters.PageSize, 1, 200);
            int skip = (page - 1) * pageSize;
            query = query.Skip(skip).Take(pageSize);
            return await query.ToListAsync(cancellationToken);
        }

        public async Task<Payment?> GetPaymentByIdAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Payment_DS.AsNoTracking()
                .Where(p => p.PaymentID == PaymentID && p.TenantID == TenantID && !p.IsDeleted)
                .Include(p => p.User)
                .Include(p => p.PaymentMethod)
                .Include(p => p.DocumentType)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Payment?> GetPaymentByIdForUpdateAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken)
        {
            // WITH tracking for updates - don't load navigation properties to avoid FK conflicts
            return await _context.Payment_DS
                .Where(p => p.PaymentID == PaymentID && p.TenantID == TenantID && !p.IsDeleted)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Payment?> GetPaymentByRequestIdAsync(Guid requestId, Guid TenantID, CancellationToken cancellationToken)
        {
            return await _context.Payment_DS.AsNoTracking()
                .Where(p => p.RequestId == requestId && p.TenantID == TenantID)
                .Include(p => p.User)
                .Include(p => p.PaymentMethod)
                .Include(p => p.DocumentType)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<Payment?> AddPaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            if (payment == null) return null;

            // ✅ VALIDAZIONE A MONTE
            var methodExists = await _context.PaymentMethod_DS.AsNoTracking().Where(dt => dt.TenantID == payment.TenantID || dt.TenantID == null)
                .AnyAsync(pm => pm.PaymentMethodID == payment.PaymentMethodID, cancellationToken);

            if (!methodExists)
            {
                _logger.LogWarning("Invalid PaymentMethodID {PaymentMethodID} for CompanyID {CompanyID}", payment.PaymentMethodID, payment.TenantID);
                throw new ArgumentException($"Invalid PaymentMethodID: {payment.PaymentMethodID}", nameof(payment.PaymentMethodID));
            }

            var docExists = await _context.Payment_DocumentType_DS.AsNoTracking().Where(dt => dt.TenantID == payment.TenantID || dt.TenantID == null)
                .AnyAsync(dt => dt.DocumentTypeID == payment.DocumentTypeID, cancellationToken);

            if (!docExists)
            {
                _logger.LogWarning("Invalid DocumentTypeID {DocumentTypeID} for CompanyID {CompanyID}", payment.DocumentTypeID, payment.TenantID);
                throw new ArgumentException($"Invalid DocumentTypeID: {payment.DocumentTypeID}", nameof(payment.DocumentTypeID));
            }

            // Ensure User exists to satisfy FK (AspNetUsers)
            var userExists = await _context.AspNetUser_DS.AsNoTracking()
                .AnyAsync(u => u.UserID == payment.UserID, cancellationToken);
            if (!userExists)
            {
                _logger.LogWarning("Invalid UserID {UserID}", payment.UserID);
                throw new ArgumentException($"Invalid UserID: {payment.UserID}", nameof(payment.UserID));
            }

            // ✅ No transaction here - managed by caller (PaymentService)
            // Retry logic is only for deadlocks during SaveChanges
            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _context.Payment_DS.Add(payment);
                await _context.SaveChangesAsync(cancellationToken);
                return payment;
            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }


        public async Task<Payment?> UpdatePaymentAsync(Payment payment, CancellationToken cancellationToken)
        {
            if (payment == null) return null;

            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                _context.Payment_DS.Update(payment);
                await _context.SaveChangesAsync(cancellationToken);
                return payment;
            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }

        public async Task<bool> DeletePaymentAsync(Guid PaymentID, Guid TenantID, CancellationToken cancellationToken)
        {
            return await RetryHelper.ExecuteWithRetryAsync(async () =>
            {
                var payment = await _context.Payment_DS
                    .FirstOrDefaultAsync(c => c.PaymentID == PaymentID && c.TenantID == TenantID && !c.IsDeleted, cancellationToken);

                if (payment == null) return false;

                payment.IsDeleted = true;
                payment.DateDeleted = DateTime.UtcNow;
                payment.IsDeletedWhy = "User requested payment deletion";
                await _context.SaveChangesAsync(cancellationToken);

                return true;
            }, retryCondition: RetryHelper.IsDeadlock, logger: _logger);
        }
    }
}
