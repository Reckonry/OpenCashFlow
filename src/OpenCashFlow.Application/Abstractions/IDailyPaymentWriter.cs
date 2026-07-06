namespace OpenCashFlow.Application.Abstractions;

public interface IDailyPaymentWriter
{
    Task UpdateDailyPaymentAsync(
        Guid tenantId,
        DateTime date,
        decimal amount,
        string entryType,
        CancellationToken cancellationToken = default);

    Task DeleteDailyPaymentAsync(
        Guid tenantId,
        DateTime date,
        decimal amount,
        string entryType,
        CancellationToken cancellationToken = default);
}
