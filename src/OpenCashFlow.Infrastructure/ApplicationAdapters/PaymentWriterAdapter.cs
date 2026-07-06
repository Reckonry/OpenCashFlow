using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Repositories;

namespace OpenCashFlow.Infrastructure.ApplicationAdapters;

public sealed class PaymentWriterAdapter(IPaymentRepository paymentRepository) : IPaymentWriter
{
    public async Task<PaymentSnapshot?> CreateAsync(PaymentDraft payment, CancellationToken cancellationToken = default)
    {
        var createdPayment = await paymentRepository.AddPaymentAsync(
            PaymentApplicationMapping.ToEntity(payment),
            cancellationToken);

        return createdPayment == null ? null : PaymentApplicationMapping.ToSnapshot(createdPayment);
    }

    public async Task<PaymentSnapshot?> UpdateAsync(PaymentUpdateDraft payment, CancellationToken cancellationToken = default)
    {
        var existingPayment = await paymentRepository.GetPaymentByIdForUpdateAsync(
            payment.PaymentId,
            payment.TenantId,
            cancellationToken);

        if (existingPayment == null)
        {
            return null;
        }

        PaymentApplicationMapping.ApplyUpdate(existingPayment, payment);
        var updatedPayment = await paymentRepository.UpdatePaymentAsync(existingPayment, cancellationToken);

        return updatedPayment == null ? null : PaymentApplicationMapping.ToSnapshot(updatedPayment);
    }

    public Task<bool> DeleteAsync(Guid paymentId, Guid tenantId, CancellationToken cancellationToken = default)
    {
        return paymentRepository.DeletePaymentAsync(paymentId, tenantId, cancellationToken);
    }
}
