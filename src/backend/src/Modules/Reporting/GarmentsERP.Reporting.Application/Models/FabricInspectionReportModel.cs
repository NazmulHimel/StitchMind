namespace GarmentsERP.Reporting.Application.Models;

public class FabricInspectionReportModel
{
    public string InspectionNumber { get; set; } = string.Empty;
    public string LotBatchNumber { get; set; } = string.Empty;
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string InspectorName { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public int TotalRollsInLot { get; set; }
    public int RollsInspected { get; set; }
    public decimal TotalYardsInspected { get; set; }
    public decimal CuttableWidthInches { get; set; }
    public decimal TotalPenaltyPoints { get; set; }
    public decimal PointsPer100SqYards { get; set; }
    public decimal AcceptanceThreshold { get; set; } = 28.0m;
    public string OverallResult { get; set; } = "PASSED";
    public string Remarks { get; set; } = string.Empty;
}
