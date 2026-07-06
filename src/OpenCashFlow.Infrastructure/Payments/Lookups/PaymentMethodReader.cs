using Microsoft.EntityFrameworkCore;
using OpenCashFlow.Application.Payments.Lookups;
using OpenCashFlow.Application.Payments.Ports;
using global::Shared.Data;

namespace OpenCashFlow.Infrastructure.Payments.Lookups;

public sealed class PaymentMethodReader(ApplicationDbContext context) : IPaymentMethodReader
{
    public async Task<IReadOnlyList<PaymentMethodListItem>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default)
    {
        var methods = await context.PaymentMethod_DS.AsNoTracking()
            .Where(p => (p.TenantID == tenantId || p.TenantID == null) && !p.IsDeleted)
            .OrderBy(p => p.DisplayOrder)
            .ToListAsync(cancellationToken);

        return methods.Select(PaymentMethodLookupMapping.ToListItem).ToList();
    }

    public async Task<PaymentMethodResult?> GetByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var method = await context.PaymentMethod_DS.AsNoTracking()
            .Where(p => (p.TenantID == tenantId || p.TenantID == null) && p.PaymentMethodID == paymentMethodId)
            .FirstOrDefaultAsync(cancellationToken);

        return method == null ? null : PaymentMethodLookupMapping.ToResult(method);
    }
}
