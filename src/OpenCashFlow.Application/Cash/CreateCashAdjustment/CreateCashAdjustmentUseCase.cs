using OpenCashFlow.Application.Cash.Ports;

namespace OpenCashFlow.Application.Cash.CreateCashAdjustment;

public sealed class CreateCashAdjustmentUseCase(ICashWriter cashWriter) : ICreateCashAdjustmentUseCase
{
    public Task ExecuteAsync(Guid companyId, decimal delta, string reason, string userId, CancellationToken cancellationToken = default)
    {
        if (companyId == Guid.Empty)
        {
            throw new ArgumentException("Company id is required", nameof(companyId));
        }

        if (delta == 0m)
        {
            throw new ArgumentException("Delta must be different from zero", nameof(delta));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("Reason is required", nameof(reason));
        }

        return cashWriter.AdminAdjustAsync(companyId, delta, reason, userId, cancellationToken);
    }
}
