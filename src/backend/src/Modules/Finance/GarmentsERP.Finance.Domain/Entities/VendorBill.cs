using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-02: Vendor Bill (Accounts Payable) for Automated Three-Way Matching (AC-06).
/// Reconciles Purchase Orders, GRN inspection quantities, and billed supplier invoices.
/// </summary>
public class VendorBill : BaseEntity
{
    public string BillNumber { get; set; } = string.Empty; // e.g. BILL-APEX-2026-091
    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = "Apex Textile Mills Ltd.";
    public Guid PurchaseOrderId { get; set; }
    public string PurchaseOrderNumber { get; set; } = "PO-2026-0145";
    public Guid GRNId { get; set; }
    public string GRNNumber { get; set; } = "GRN-2026-0210";
    public DateTime InvoiceDate { get; set; } = DateTime.UtcNow.AddDays(-7);
    public DateTime DueDate { get; set; } = DateTime.UtcNow.AddDays(23); // 30-day term
    public decimal BilledUnitPrice { get; set; } = 2.3500m;
    public decimal BilledQuantity { get; set; } = 36750.0m;
    public decimal BilledAmount { get; set; } = 86362.50m;
    public string Currency { get; set; } = "USD";
    public BillMatchStatus MatchStatus { get; set; } = BillMatchStatus.PendingMatch;
    public bool IsApprovedForPayment { get; set; } = false;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAtUtc { get; set; }

    // ACC-06: Statutory Deductions
    public decimal TaxDeductedAtSourceTDS { get; set; } = 2590.88m; // 3% TDS
    public decimal VatDeductedAtSourceVDS { get; set; } = 6477.19m; // 7.5% VDS
    public decimal NetPayableAmount { get; set; } = 77294.43m;
    public string? Notes { get; set; }

    public ICollection<ThreeWayMatchLog> MatchLogs { get; set; } = new List<ThreeWayMatchLog>();

    public ThreeWayMatchLog ExecuteThreeWayMatch(decimal poUnitPrice, decimal grnPassedQuantity, string evaluatedBy)
    {
        bool priceMatched = (BilledUnitPrice <= poUnitPrice);
        bool quantityMatched = (BilledQuantity <= grnPassedQuantity);

        decimal priceVariancePct = poUnitPrice > 0
            ? Math.Round(((BilledUnitPrice - poUnitPrice) / poUnitPrice) * 100.0m, 2)
            : 0m;
        decimal quantityVariance = BilledQuantity - grnPassedQuantity;

        var log = new ThreeWayMatchLog
        {
            VendorBillId = Id,
            POUnitPrice = poUnitPrice,
            BilledUnitPrice = BilledUnitPrice,
            PriceVariancePct = priceVariancePct,
            GRNPassedQuantity = grnPassedQuantity,
            BilledQuantity = BilledQuantity,
            QuantityVariance = quantityVariance,
            IsPriceMatched = priceMatched,
            IsQuantityMatched = quantityMatched,
            EvaluatedBy = evaluatedBy,
            EvaluatedAtUtc = DateTime.UtcNow
        };

        if (priceMatched && quantityMatched)
        {
            log.OverallMatchResult = ThreeWayMatchResult.Passed;
            log.DiscrepancyNotes = "Three-Way Match PASSED: Billed price and quantity conform 100% to approved PO and QC-cleared GRN.";
            MatchStatus = BillMatchStatus.ApprovedForPayment;
            IsApprovedForPayment = true;
            ApprovedBy = evaluatedBy;
            ApprovedAtUtc = DateTime.UtcNow;
        }
        else
        {
            log.OverallMatchResult = ThreeWayMatchResult.DiscrepancyFlagged;
            log.DiscrepancyNotes = $"Three-Way Match FAILED: {(priceMatched ? "" : $"Price markup detected (+{priceVariancePct}%). ")}{(quantityMatched ? "" : $"Billed quantity exceeds QC-cleared quantity by {quantityVariance} Yds.")} Bill locked.";
            MatchStatus = BillMatchStatus.MatchFailedLocked;
            IsApprovedForPayment = false;
        }

        MatchLogs.Add(log);
        return log;
    }
}
