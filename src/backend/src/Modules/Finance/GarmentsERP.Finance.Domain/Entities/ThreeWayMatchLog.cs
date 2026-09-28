using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-02 & AC-06: Verification Audit Log for Three-Way Matching.
/// Compares Purchase Order price, Goods Receipt Note QC quantity, and Vendor Bill invoice.
/// </summary>
public class ThreeWayMatchLog : BaseEntity
{
    public Guid VendorBillId { get; set; }
    public VendorBill? VendorBill { get; set; }

    public decimal POUnitPrice { get; set; }
    public decimal BilledUnitPrice { get; set; }
    public decimal PriceVariancePct { get; set; }

    public decimal GRNPassedQuantity { get; set; }
    public decimal BilledQuantity { get; set; }
    public decimal QuantityVariance { get; set; }

    public bool IsPriceMatched { get; set; }
    public bool IsQuantityMatched { get; set; }

    public ThreeWayMatchResult OverallMatchResult { get; set; } = ThreeWayMatchResult.Passed;
    public string? DiscrepancyNotes { get; set; }

    public string EvaluatedBy { get; set; } = "Automated Engine";
    public DateTime EvaluatedAtUtc { get; set; } = DateTime.UtcNow;
}
