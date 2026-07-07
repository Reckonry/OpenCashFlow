using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Queries;
using OpenCashFlow.Infrastructure.Persistence;

namespace OpenCashFlow.Infrastructure.Payments;

public sealed class PaymentQueryReader(ApplicationDbContext context) : IPaymentQueryReader
{
    public async Task<IReadOnlyList<PaymentListItem>> GetPaymentsAsync(
        PaymentListQuery filters,
        CancellationToken cancellationToken = default)
    {
        var query = context.Payment_DS.AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.PaymentMethod)
            .Include(p => p.DocumentType)
            .Where(p => p.TenantID == filters.TenantId);

        if (filters.PaymentId.HasValue)
            query = query.Where(p => p.PaymentID == filters.PaymentId.Value);
        if (!string.IsNullOrWhiteSpace(filters.EntryType))
            query = query.Where(p => p.EntryType == filters.EntryType);
        if (filters.PaymentMethodId.HasValue)
            query = query.Where(p => p.PaymentMethodID == filters.PaymentMethodId.Value);
        if (filters.DocumentTypeId.HasValue)
            query = query.Where(p => p.DocumentTypeID == filters.DocumentTypeId.Value);
        if (filters.UserId.HasValue)
            query = query.Where(p => p.UserID == filters.UserId.Value);
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

        var skip = (filters.Page - 1) * filters.PageSize;
        return await query
            .Skip(skip)
            .Take(filters.PageSize)
            .Select(p => new PaymentListItem(
                p.PaymentID,
                p.TenantID,
                p.Amount,
                p.EntryType,
                p.PaymentMethodID,
                p.PaymentMethod != null ? p.PaymentMethod.PaymentMethodName : string.Empty,
                p.DocumentTypeID,
                p.DocumentType != null ? p.DocumentType.DocumentTypeName : string.Empty,
                p.Description,
                p.UserID,
                p.User != null ? $"{p.User.UserFirstName} {p.User.UserLastName}".Trim() : string.Empty,
                p.IsDeleted,
                p.IsDeletedBy,
                p.IsDeletedWhy,
                p.DateDeleted,
                p.CreatedBy,
                p.DateIns,
                p.EditedBy,
                p.DateEdit))
            .ToListAsync(cancellationToken);
    }

    public async Task<PaymentDetailResult?> GetPaymentDetailAsync(
        Guid paymentId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await context.Payment_DS.AsNoTracking()
            .Where(p => p.PaymentID == paymentId && p.TenantID == tenantId && !p.IsDeleted)
            .Include(p => p.User)
            .Include(p => p.PaymentMethod)
            .Include(p => p.DocumentType)
            .Select(p => new PaymentDetailResult(
                p.PaymentID,
                p.TenantID,
                p.Amount,
                p.EntryType,
                p.PaymentMethodID,
                p.PaymentMethod != null ? p.PaymentMethod.PaymentMethodName : string.Empty,
                p.DocumentTypeID,
                p.DocumentType != null ? p.DocumentType.DocumentTypeName : string.Empty,
                p.Description,
                p.UserID,
                p.User != null ? $"{p.User.UserFirstName} {p.User.UserLastName}".Trim() : string.Empty,
                p.IsDeleted,
                p.IsDeletedBy,
                p.IsDeletedWhy,
                p.DateDeleted,
                p.CreatedBy,
                p.DateIns,
                p.EditedBy,
                p.DateEdit))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
