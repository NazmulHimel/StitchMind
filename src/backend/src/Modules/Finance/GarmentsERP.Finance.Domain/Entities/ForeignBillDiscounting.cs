using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-03: Foreign Demand Bill Discounting / Purchase (FDBP) Ledger.
/// Accelerates working capital cash flow by obtaining post-shipment export bill discounting
/// from advising/negotiating banks against clean shipping documents.
/// </summary>
public class ForeignBillDiscounting : BaseEntity
{
    public string FDBPNumber { get; set; } = string.Empty; // e.g. FDBP-2026-0034
    public Guid MasterLCId { get; set; }
    public MasterLetterOfCredit? MasterLC { get; set; }
    public string MasterLCNumber { get; set; } = "MLC-2026-0891";

    public string BankName { get; set; } = "Standard Chartered Bank Dhaka";
    public string BillOfLadingNumber { get; set; } = "BL-SWED-2026-7890";
    public decimal FOBValue { get; set; } = 180000.00m;
    public decimal AdvancePercentage { get; set; } = 90.0m;
    public decimal AdvancedAmount { get; set; } = 162000.00m;
    public decimal BankDiscountCharges { get; set; } = 1875.00m;
    public decimal NetProceedsRealized { get; set; } = 160125.00m;
    public string Currency { get; set; } = "USD";
    public DateTime RealizationDate { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Realized";
    public string? Remarks { get; set; }
}
