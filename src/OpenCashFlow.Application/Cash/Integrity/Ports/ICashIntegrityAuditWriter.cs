using OpenCashFlow.Application.Cash.Integrity.Models;

namespace OpenCashFlow.Application.Cash.Integrity.Ports;

public interface ICashIntegrityAuditWriter
{
    Task WriteAsync(CashIntegrityAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
