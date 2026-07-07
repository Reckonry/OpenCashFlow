using OpenCashFlow.Application.Payments.Lookups;

namespace OpenCashFlow.Application.Payments.Ports;

public interface IPaymentMethodWriter
{
    Task<PaymentMethodResult?> CreateAsync(PaymentMethodCreateCommand command, CancellationToken cancellationToken = default);

    Task<PaymentMethodResult?> UpdateAsync(PaymentMethodUpdateCommand command, CancellationToken cancellationToken = default);

    Task<bool> DeleteAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default);
}
