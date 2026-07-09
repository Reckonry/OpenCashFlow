using OpenCashFlow.Application.Cash.Integrity.Commands;
using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashSessionWriter
{
    Task<CashSessionResult?> OpenAsync(OpenCashSessionCommand command, CancellationToken cancellationToken = default);

    Task<CashSessionResult?> CloseAsync(Guid tenantId, Guid cashSessionId, Guid closedByUserId, CancellationToken cancellationToken = default);
}
