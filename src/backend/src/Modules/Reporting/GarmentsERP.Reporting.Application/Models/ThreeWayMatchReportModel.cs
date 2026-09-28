namespace GarmentsERP.Reporting.Application.Models;

public class ThreeWayMatchReportModel
{
    public string BillNumber { get; set; } = "BILL-APEX-2026-091";
    public string SupplierName { get; set; } = "Apex Textile Mills Ltd.";
    public string PONumber { get; set; } = "PO-2026-0145";
    public decimal POUnitPrice { get; set; } = 2.3500m;
    public string GRNNumber { get; set; } = "GRN-2026-0210";
    public decimal GRNPassedQuantity { get; set; } = 36750.0m;
    public decimal BilledUnitPrice { get; set; } = 2.3500m;
    public decimal BilledQuantity { get; set; } = 36750.0m;
    public decimal BilledAmount { get; set; } = 86362.50m;
    public string Currency { get; set; } = "USD";
    public decimal PriceVariancePct { get; set; } = 0.0m;
    public decimal QuantityVariance { get; set; } = 0.0m;
    public bool IsPriceMatched { get; set; } = true;
    public bool IsQuantityMatched { get; set; } = true;
    public string OverallMatchResult { get; set; } = "PASSED";
    public string? DiscrepancyNotes { get; set; } = "Three-Way Match PASSED: Billed price and quantity conform 100% to approved PO and QC-cleared GRN.";
    public decimal TaxDeductedAtSourceTDS { get; set; } = 2590.88m;
    public decimal VatDeductedAtSourceVDS { get; set; } = 6477.19m;
    public decimal NetPayableAmount { get; set; } = 77294.43m;
    public string EvaluatedBy { get; set; } = "Automated Engine";
    public DateTime EvaluatedAtUtc { get; set; } = DateTime.UtcNow;
}
