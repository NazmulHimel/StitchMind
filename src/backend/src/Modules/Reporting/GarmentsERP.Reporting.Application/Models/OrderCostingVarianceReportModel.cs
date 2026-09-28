namespace GarmentsERP.Reporting.Application.Models;

public class OrderCostingVarianceReportModel
{
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string StyleName { get; set; } = "Slim Fit Cotton Chino Pant";
    public string BuyerName { get; set; } = "H&M Hennes & Mauritz GBC AB";
    public int OrderQuantity { get; set; } = 20000;
    public decimal BuyerFOBPricePerPc { get; set; } = 9.0000m;
    public decimal BuyerFOBRevenue { get; set; } = 180000.00m;
    public decimal BudgetRawMaterialsCost { get; set; } = 88500.00m;
    public decimal ActualRawMaterialsCost { get; set; } = 86362.50m;
    public decimal RawMaterialVariance { get; set; } = 2137.50m;
    public decimal BudgetDirectLaborCost { get; set; } = 44000.00m;
    public decimal ActualDirectLaborCost { get; set; } = 43210.00m;
    public decimal DirectLaborVariance { get; set; } = 790.00m;
    public decimal BudgetOverheadsCost { get; set; } = 30000.00m;
    public decimal ActualOverheadsCost { get; set; } = 29900.00m;
    public decimal OverheadsVariance { get; set; } = 100.00m;
    public decimal TotalBudgetCost { get; set; } = 162500.00m;
    public decimal TotalActualCost { get; set; } = 159472.50m;
    public decimal TotalCostVariance { get; set; } = 3027.50m;
    public decimal NetRealizedProfit { get; set; } = 20527.50m;
    public decimal NetProfitMarginPct { get; set; } = 11.40m;
    public string Currency { get; set; } = "USD";
    public string EvaluatedBy { get; set; } = "Cost Accounting Lead";
    public DateTime AnalyzedAtUtc { get; set; } = DateTime.UtcNow;
}
