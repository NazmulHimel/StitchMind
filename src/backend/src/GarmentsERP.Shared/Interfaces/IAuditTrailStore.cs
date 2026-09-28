using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Shared.Interfaces;

/// <summary>
/// Persistence contract for recording and querying the immutable system audit trail (SEC-05).
/// </summary>
public interface IAuditTrailStore
{
    Task RecordAuditLogAsync(AuditTrailLog log, CancellationToken ct = default);
    Task<IReadOnlyList<AuditTrailLog>> GetAuditLogsAsync(
        string? entityName = null,
        string? entityId = null,
        int take = 50,
        CancellationToken ct = default);
}
