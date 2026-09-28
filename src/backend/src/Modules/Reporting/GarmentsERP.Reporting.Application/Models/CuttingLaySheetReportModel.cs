namespace GarmentsERP.Reporting.Application.Models;

public class CuttingLaySheetReportModel
{
    public string CuttingOrderNumber { get; set; } = "CUT-2026-0001";
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string BuyerName { get; set; } = "Nordic Fashion AB";
    public string OrderNumber { get; set; } = "BO-2026-0890";
    public string FabricType { get; set; } = "98/2 Cotton Spandex Twill (260 GSM)";
    public string ShadeGroup { get; set; } = "Shade A";
    public string TableNumber { get; set; } = "Table-03";
    public DateTime CuttingDate { get; set; } = DateTime.UtcNow;
    public string MarkerName { get; set; } = "MKR-CHINO-SZ32-34-A";
    public decimal MarkerEfficiencyPct { get; set; } = 87.20m;
    public decimal MarkerLengthYards { get; set; } = 8.42m;
    public int GarmentsPerMarker { get; set; } = 4;
    public int TotalPlies { get; set; } = 60;
    public int TotalGarmentsPlanned { get; set; } = 240;
    public decimal PlannedFabricTotalYds { get; set; } = 505.20m;
    public decimal ActualFabricConsumedYds { get; set; } = 508.50m;
    public decimal EndLossRemnantYds { get; set; } = 3.50m;
    public decimal CuttingWastageVariancePct { get; set; } = 0.68m;
    public string AllocatedRollBarcodes { get; set; } = "ROL-2026-00891, ROL-2026-00892, ROL-2026-00893";
    public string ApprovedBy { get; set; } = "Tariqul Islam (Cutting In-charge)";
    public List<BundleSummaryLine> Bundles { get; set; } = new();
}

public class BundleSummaryLine
{
    public string BundleBarcode { get; set; } = string.Empty;
    public string ComponentName { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string SerialRange { get; set; } = string.Empty;
}
