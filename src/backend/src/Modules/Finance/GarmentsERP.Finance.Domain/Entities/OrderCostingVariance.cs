using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-04: Order-Wise Pre-Costing vs Actual Post-Costing Margin Variance Engine.
/// Reconciles budgeted pre-costing metrics with audited actual factory expenses across materials, labor, and overheads.
/// </summary>
public class OrderCostingVariance : BaseEntity
{
    public Guid StyleId { get; set; }
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string StyleName { get; set; } = "Slim Fit Cotton Chino Pant";
    public string BuyerName { get; set; } = "H&M Hennes & Mauritz GBC AB";
    public int OrderQuantity { get; set; } = 20000;
    public decimal BuyerFOBPricePerPc { get; set; } = 9.0000m;
    public decimal BuyerFOBRevenue => Math.Round(OrderQuantity * BuyerFOBPricePerPc, 2);

    // Raw Materials (Fabric + Trims)
    public decimal BudgetRawMaterialsCost { get; set; } = 88500.00m;
    public decimal ActualRawMaterialsCost { get; set; } = 86362.50m;
    public decimal RawMaterialVariance => BudgetRawMaterialsCost - ActualRawMaterialsCost;

    // Direct Labor (Sewing, Cutting, Finishing)
    public decimal BudgetDirectLaborCost { get; set; } = 44000.00m;
    public decimal ActualDirectLaborCost { get; set; } = 43210.00m;
    public decimal DirectLaborVariance => BudgetDirectLaborCost - ActualDirectLaborCost;

    // Factory Overheads & Commercial Charges
    public decimal BudgetOverheadsCost { get; set; } = 30000.00m;
    public decimal ActualOverheadsCost { get; set; } = 29900.00m;
    public decimal OverheadsVariance => BudgetOverheadsCost - ActualOverheadsCost;

    // Aggregates
    public decimal TotalBudgetCost => BudgetRawMaterialsCost + BudgetDirectLaborCost + BudgetOverheadsCost;
    public decimal TotalActualCost => ActualRawMaterialsCost + ActualDirectLaborCost + ActualOverheadsCost;
    public decimal TotalCostVariance => TotalBudgetCost - TotalActualCost;

    // Profit & Margins
    public decimal NetRealizedProfit => BuyerFOBRevenue - TotalActualCost;
    public decimal NetProfitMarginPct => BuyerFOBRevenue > 0
        ? Math.Round((NetRealizedProfit / BuyerFOBRevenue) * 100m, 2)
        : 0m;

    public decimal BudgetedProfit => BuyerFOBRevenue - TotalBudgetCost;
    public decimal BudgetedProfitMarginPct => BuyerFOBRevenue > 0
        ? Math.Round((BudgetedProfit / BuyerFOBRevenue) * 100m, 2)
        : 0m;

    public string Currency { get; set; } = "USD";
    public DateTime AnalyzedAtUtc { get; set; } = DateTime.UtcNow;
    public string EvaluatedBy { get; set; } = "Cost Accounting Department";
}
