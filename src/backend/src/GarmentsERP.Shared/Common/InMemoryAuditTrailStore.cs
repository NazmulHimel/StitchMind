using System.Collections.Concurrent;
using GarmentsERP.Shared.Domain;
using GarmentsERP.Shared.Interfaces;

namespace GarmentsERP.Shared.Common;

/// <summary>
/// Thread-safe in-memory audit store with seeded baseline enterprise audit entries (SEC-05).
/// </summary>
public class InMemoryAuditTrailStore : IAuditTrailStore
{
    private readonly ConcurrentBag<AuditTrailLog> _logs = new();

    public InMemoryAuditTrailStore()
    {
        // Seed initial audit trail logs matching realistic garments export transactions
        _logs.Add(new AuditTrailLog
        {
            Id = Guid.NewGuid(),
            EntityName = "PurchaseOrder",
            EntityId = "PO-2026-0145",
            ActionType = "BUDGET_OVERRIDE",
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserName = "tariq.cfo@stitchmind.com",
            UserIpAddress = "192.168.10.45",
            OldValuesJson = "{\"Status\": \"LockedBudgetExceeded\", \"BudgetCap\": 2.20, \"OfferedPrice\": 2.25, \"IsBudgetLocked\": true}",
            NewValuesJson = "{\"Status\": \"Approved\", \"OverrideReason\": \"Authorized emergency yarn procurement under CFO mandate\", \"IsBudgetLocked\": false}",
            TimestampUtc = DateTime.UtcNow.AddHours(-3),
            CompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        });

        _logs.Add(new AuditTrailLog
        {
            Id = Guid.NewGuid(),
            EntityName = "StylePreCosting",
            EntityId = "ST-2026-CHINO-01",
            ActionType = "APPROVE",
            UserId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            UserName = "merchandising.lead@stitchmind.com",
            UserIpAddress = "192.168.10.12",
            OldValuesJson = "{\"IsBudgetApproved\": false, \"Version\": 1}",
            NewValuesJson = "{\"IsBudgetApproved\": true, \"FOBPerDzn\": 90.00, \"TotalMaterialsBudget\": 53.40, \"NetMarginPerDzn\": 12.60}",
            TimestampUtc = DateTime.UtcNow.AddDays(-1),
            CompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        });

        _logs.Add(new AuditTrailLog
        {
            Id = Guid.NewGuid(),
            EntityName = "FabricRoll",
            EntityId = "ROLL-2026-0042",
            ActionType = "STATUS_CHANGE",
            UserId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
            UserName = "qa.auditor@stitchmind.com",
            UserIpAddress = "192.168.20.104",
            OldValuesJson = "{\"Status\": \"Quarantine\", \"PenaltyPoints\": 0}",
            NewValuesJson = "{\"Status\": \"PassedQC\", \"PenaltyPoints\": 14.5, \"PointsPer100SqYds\": 18.2, \"ASTM\": \"Pass\"}",
            TimestampUtc = DateTime.UtcNow.AddHours(-5),
            CompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111")
        });
    }

    public Task RecordAuditLogAsync(AuditTrailLog log, CancellationToken ct = default)
    {
        _logs.Add(log);
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<AuditTrailLog>> GetAuditLogsAsync(
        string? entityName = null,
        string? entityId = null,
        int take = 50,
        CancellationToken ct = default)
    {
        var query = _logs.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(entityName))
        {
            query = query.Where(l => string.Equals(l.EntityName, entityName, StringComparison.OrdinalIgnoreCase));
        }

        if (!string.IsNullOrWhiteSpace(entityId))
        {
            query = query.Where(l => string.Equals(l.EntityId, entityId, StringComparison.OrdinalIgnoreCase));
        }

        var result = query
            .OrderByDescending(l => l.TimestampUtc)
            .Take(take)
            .ToList();

        return Task.FromResult<IReadOnlyList<AuditTrailLog>>(result);
    }
}
