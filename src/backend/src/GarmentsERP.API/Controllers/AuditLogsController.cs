using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Domain;
using GarmentsERP.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditTrailStore _auditTrailStore;

    public AuditLogsController(IAuditTrailStore auditTrailStore)
    {
        _auditTrailStore = auditTrailStore;
    }

    /// <summary>
    /// Retrieves immutable system audit trail records (SEC-05).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs(
        [FromQuery] string? entityName,
        [FromQuery] string? entityId,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var logs = await _auditTrailStore.GetAuditLogsAsync(entityName, entityId, take, ct);
        return Ok(Result<IReadOnlyList<AuditTrailLog>>.Success(logs));
    }
}
