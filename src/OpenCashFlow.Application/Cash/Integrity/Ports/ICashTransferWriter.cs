using OpenCashFlow.Application.Cash.Integrity.Commands;
using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashTransferWriter
{
    Task<CashTransferResult?> CreateAsync(CreateCashTransferCommand command, CancellationToken cancellationToken = default);

    Task<CashTransferResult?> ReverseAsync(ReverseCashTransferCommand command, CancellationToken cancellationToken = default);

    Task<CashTransferCorrectionResult?> CorrectAsync(CorrectCashTransferCommand command, CancellationToken cancellationToken = default);
}
