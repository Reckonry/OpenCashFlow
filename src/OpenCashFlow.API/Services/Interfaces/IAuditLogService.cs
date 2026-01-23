using global::Shared.DTOs.Admin;
using global::Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Services.Interfaces
{
    /// <summary>
    /// Service for centralized audit log management
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Records an audit event in the background
        /// </summary>
        Task LogEventAsync(
            AuditEventType eventType,
            string resource,
            string action,
            string? resourceId = null,
            object? changes = null,
            string? additionalInfo = null,
            string? severity = "Info",
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a paginated list of audit logs with filters
        /// </summary>
        Task<(List<AuditLog_List_DTO> Logs, int TotalCount)> GetAuditLogsAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets details for an audit log
        /// </summary>
        Task<AuditLog_Detail_DTO> GetAuditLogDetailAsync(
            Guid auditLogId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Exports the audit log in CSV format
        /// </summary>
        Task<byte[]> ExportAuditLogAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default);
    }
}
