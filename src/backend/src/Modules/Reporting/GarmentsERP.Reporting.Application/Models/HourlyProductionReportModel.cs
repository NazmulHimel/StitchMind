namespace GarmentsERP.Reporting.Application.Models;

public class HourlyProductionReportModel
{
    public string LineNumber { get; set; } = "LINE-04";
    public string LineName { get; set; } = "Woven Pant Line";
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string BuyerName { get; set; } = "Inditex / Zara International";
    public DateTime ReportDate { get; set; } = DateTime.UtcNow;
    public string SupervisorName { get; set; } = "Anisur Rahman";
    public int ActiveWorkstations { get; set; } = 50;
    public decimal TargetEfficiencyPct { get; set; } = 85.0m;
    public decimal ActualEfficiencyPct { get; set; } = 78.4m;
    public int TotalTargetQty { get; set; } = 1200;
    public int TotalActualQty { get; set; } = 1260;
    public decimal DefectRateDHU { get; set; } = 2.10m;
    public List<HourlyProductionSlotItem> Slots { get; set; } = new();
}

public class HourlyProductionSlotItem
{
    public int HourSlot { get; set; }
    public string TimeRange { get; set; } = string.Empty;
    public int TargetQty { get; set; }
    public int ActualOutputQty { get; set; }
    public int DefectCount { get; set; }
    public int Variance => ActualOutputQty - TargetQty;
    public decimal EfficiencyPct => TargetQty > 0 ? Math.Round((decimal)ActualOutputQty / TargetQty * 100.0m, 1) : 0m;
    public string Remarks { get; set; } = string.Empty;
}
