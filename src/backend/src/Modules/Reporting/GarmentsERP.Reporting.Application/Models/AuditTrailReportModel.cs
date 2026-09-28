namespace GarmentsERP.Reporting.Application.Models;

public class AuditTrailReportItem
{
    public DateTime TimestampUtc { get; set; }
    public string EntityName { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string UserIpAddress { get; set; } = string.Empty;
    public string ChangesSummary { get; set; } = string.Empty;
}

public class AuditTrailReportModel
{
    public string ReportTitle { get; set; } = "SYSTEM AUDIT TRAIL LOG";
    public string GeneratedBy { get; set; } = "System Administrator";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    public string FilterCriteria { get; set; } = "All Modules (Recent 100 Events)";
    public List<AuditTrailReportItem> Items { get; set; } = new();
}
