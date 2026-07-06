using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Ports;
using OpenCashFlow.Application.Payments.Repositories;

namespace OpenCashFlow.Infrastructure.ApplicationAdapters;

public sealed class PaymentReaderAdapter(
    IPaymentRepository paymentRepository,
    IPaymentMethodReader paymentMethodReader) : IPaymentReader
{
    public async Task<PaymentSnapshot?> GetByIdAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetPaymentByIdAsync(paymentId, tenantId, cancellationToken);
        return payment == null ? null : PaymentApplicationMapping.ToSnapshot(payment);
    }

    public async Task<PaymentSnapshot?> GetByRequestIdAsync(Guid requestId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var payment = await paymentRepository.GetPaymentByRequestIdAsync(requestId, tenantId, cancellationToken);
        return payment == null ? null : PaymentApplicationMapping.ToSnapshot(payment);
    }

    public async Task<PaymentMethodSnapshot?> GetPaymentMethodByIdAsync(Guid paymentMethodId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        var paymentMethod = await paymentMethodReader.GetByIdAsync(paymentMethodId, tenantId, cancellationToken);
        return paymentMethod == null
            ? null
            : new PaymentMethodSnapshot(paymentMethod.PaymentMethodId, paymentMethod.Name);
    }
}
