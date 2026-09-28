namespace GarmentsERP.Shared.Domain;

/// <summary>
/// Immutable audit log entry recording state transitions, user identity, IP address, and JSON diffs (SEC-05).
/// </summary>
public class AuditTrailLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = "UPDATE"; // INSERT, UPDATE, DELETE, BUDGET_OVERRIDE
    public Guid? UserId { get; set; }
    public string? UserName { get; set; }
    public string? UserIpAddress { get; set; }
    public string? OldValuesJson { get; set; }
    public string? NewValuesJson { get; set; }
    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
    public Guid? CompanyId { get; set; }

    public static AuditTrailLog Create(
        string entityName,
        string entityId,
        string actionType,
        Guid? userId,
        string? userName,
        string? userIpAddress,
        string? oldValuesJson,
        string? newValuesJson,
        Guid? companyId = null)
    {
        return new AuditTrailLog
        {
            EntityName = entityName,
            EntityId = entityId,
            ActionType = actionType,
            UserId = userId,
            UserName = userName,
            UserIpAddress = userIpAddress,
            OldValuesJson = oldValuesJson,
            NewValuesJson = newValuesJson,
            TimestampUtc = DateTime.UtcNow,
            CompanyId = companyId
        };
    }
}
