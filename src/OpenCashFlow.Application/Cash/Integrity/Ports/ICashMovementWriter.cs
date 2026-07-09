using OpenCashFlow.Application.Cash.Integrity.Commands;
using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashMovementWriter
{
    Task<CashMovementResult?> CreateAsync(CreateCashMovementCommand command, CancellationToken cancellationToken = default);

    Task<CashMovementResult?> ReverseAsync(ReverseCashMovementCommand command, CancellationToken cancellationToken = default);

    Task<CashMovementCorrectionResult?> CorrectAsync(CorrectCashMovementCommand command, CancellationToken cancellationToken = default);
}

public sealed record CashMovementCorrectionResult(
    CashMovementResult Original,
    CashMovementResult Reversal,
    CashMovementResult Replacement);
