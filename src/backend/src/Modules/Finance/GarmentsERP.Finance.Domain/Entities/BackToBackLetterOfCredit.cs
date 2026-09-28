using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-03: Back-to-Back (B2B) Letter of Credit issued to material suppliers
/// against backed collateral of an Export Master LC.
/// </summary>
public class BackToBackLetterOfCredit : BaseEntity
{
    public string B2BLCNumber { get; set; } = string.Empty; // e.g. B2B-2026-0098
    public Guid MasterLCId { get; set; }
    public MasterLetterOfCredit? MasterLC { get; set; }
    public string MasterLCNumber { get; set; } = "MLC-2026-0891";

    public Guid SupplierId { get; set; }
    public string SupplierName { get; set; } = "Apex Textile Mills Ltd.";

    public decimal Amount { get; set; } = 86362.50m;
    public string Currency { get; set; } = "USD";
    public DateTime IssueDate { get; set; } = DateTime.UtcNow.AddDays(-20);
    public DateTime MaturityDate { get; set; } = DateTime.UtcNow.AddDays(70);
    public int TenorDays { get; set; } = 90;

    public BackToBackLCStatus Status { get; set; } = BackToBackLCStatus.Issued;
    public decimal SettledAmount { get; set; } = 0m;
    public DateTime? SettledAtUtc { get; set; }
    public string? SettlementReference { get; set; }

    public void Settle(decimal amount, string reference)
    {
        SettledAmount += amount;
        SettlementReference = reference;
        SettledAtUtc = DateTime.UtcNow;
        if (SettledAmount >= Amount)
        {
            Status = BackToBackLCStatus.Settled;
        }
    }
}
