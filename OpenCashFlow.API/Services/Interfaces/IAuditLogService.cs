using global::Shared.DTOs.Admin;
using global::Shared.Enums;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OpenCashFlow.API.Services.Interfaces
{
    /// <summary>
    /// Service per la gestione centralizzata dell'audit log
    /// </summary>
    public interface IAuditLogService
    {
        /// <summary>
        /// Registra un evento di audit in background
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
        /// Ottiene lista paginata di audit log con filtri
        /// </summary>
        Task<(List<AuditLog_List_DTO> Logs, int TotalCount)> GetAuditLogsAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Ottiene dettaglio di un audit log
        /// </summary>
        Task<AuditLog_Detail_DTO> GetAuditLogDetailAsync(
            Guid auditLogId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Esporta audit log in formato CSV
        /// </summary>
        Task<byte[]> ExportAuditLogAsync(
            AuditLog_Filter_DTO filter,
            CancellationToken cancellationToken = default);
    }
}
