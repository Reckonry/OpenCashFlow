using OpenCashFlow.Application.Abstractions;
using OpenCashFlow.Application.Payments.Repositories;

namespace OpenCashFlow.Infrastructure.ApplicationAdapters;

public sealed class DailyPaymentWriterAdapter(IPaymentRepository paymentRepository) : IDailyPaymentWriter
{
    public async Task UpdateDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
    {
        await paymentRepository.UpdateDailyPaymentAsync(
            tenantId,
            date.Date,
            Convert.ToDouble(amount),
            entryType,
            cancellationToken);
    }

    public async Task DeleteDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default)
    {
        await paymentRepository.DeleteDailyPaymentAsync(
            tenantId,
            date.Date,
            Convert.ToDouble(amount),
            entryType,
            cancellationToken);
    }
}
