using OpenCashFlow.Application.Auth.Audit;

namespace OpenCashFlow.Application.Auth.Ports;

public interface IAuthAuditWriter
{
    Task WriteAsync(AuthAuditEvent auditEvent, CancellationToken cancellationToken = default);
}
