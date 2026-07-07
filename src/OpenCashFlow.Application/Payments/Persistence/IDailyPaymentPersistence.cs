namespace OpenCashFlow.Application.Payments.Persistence;

public interface IDailyPaymentPersistence
{
    Task UpdateDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default);

    Task DeleteDailyPaymentAsync(Guid tenantId, DateTime date, decimal amount, string entryType, CancellationToken cancellationToken = default);
}
