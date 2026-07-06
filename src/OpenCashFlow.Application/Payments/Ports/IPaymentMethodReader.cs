using OpenCashFlow.Application.Payments.Lookups;

namespace OpenCashFlow.Application.Payments.Ports;

public interface IPaymentMethodReader
{
    Task<IReadOnlyList<PaymentMethodListItem>> GetAllAsync(Guid tenantId, CancellationToken cancellationToken = default);

    Task<PaymentMethodResult?> GetByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default);
}
