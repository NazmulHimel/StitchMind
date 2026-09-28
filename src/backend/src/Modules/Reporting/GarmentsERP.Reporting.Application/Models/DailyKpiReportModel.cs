namespace GarmentsERP.Reporting.Application.Models;

public class DailyKpiReportModel
{
    public DateTime ReportDate { get; set; } = DateTime.Today;
    public string ShiftName { get; set; } = "Shift A";
    public int PlannedGarmentOutput { get; set; }
    public int ActualGarmentOutput { get; set; }
    public decimal OverallFactoryEfficiencyPct { get; set; }
    public decimal RunningDHUPct { get; set; }
    public int PresentOperators { get; set; }
    public int TotalOperators { get; set; }
    public List<LineProductionReportItem> LineOutputs { get; set; } = [];
}

public class LineProductionReportItem
{
    public string LineNumber { get; set; } = string.Empty;
    public string StyleNumber { get; set; } = string.Empty;
    public int HourlyTarget { get; set; }
    public int ActualOutput { get; set; }
    public decimal EfficiencyPct { get; set; }
    public string Status { get; set; } = "On Track";
}
