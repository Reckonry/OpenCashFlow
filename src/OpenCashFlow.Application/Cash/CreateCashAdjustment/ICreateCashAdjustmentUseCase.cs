namespace OpenCashFlow.Application.Cash.CreateCashAdjustment;

public interface ICreateCashAdjustmentUseCase
{
    Task ExecuteAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken cancellationToken = default);
}
