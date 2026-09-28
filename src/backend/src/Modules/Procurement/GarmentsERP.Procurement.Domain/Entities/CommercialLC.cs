using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Procurement.Domain.Entities;

/// <summary>
/// Commercial Letters of Credit (Trade Finance Ledger) for Master LCs and Back-to-Back LCs (PRO-05).
/// </summary>
public class CommercialLC : BaseEntity
{
    public const decimal StatutoryB2BCeilingPercentage = 70.00m; // Central Bank statutory 70% B2B ceiling

    public string LCNumber { get; set; } = string.Empty;
    public LCType LCType { get; set; } = LCType.BackToBackLC;
    public Guid? ParentMasterLCId { get; set; }
    public CommercialLC? ParentMasterLC { get; set; }
    public string IssuingBank { get; set; } = string.Empty;
    public string AdvisingBank { get; set; } = string.Empty;
    public string ApplicantName { get; set; } = string.Empty;
    public string BeneficiaryName { get; set; } = string.Empty;
    public decimal LCAmount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public DateTime ExpiryDate { get; set; }
    public DateTime ShipmentDeadline { get; set; }
    public LCStatus Status { get; set; } = LCStatus.Opened;
    public Guid? LinkedPurchaseOrderId { get; set; }
    public string? TenorTerms { get; set; } = "90 Days Deferred Maturity";
    public Guid CompanyId { get; set; }

    public ICollection<CommercialLC> ChildBackToBackLCs { get; set; } = new List<CommercialLC>();

    public decimal CalculateCeilingPercentage(decimal masterLCAmount)
    {
        if (masterLCAmount <= 0) return 0;
        return (LCAmount / masterLCAmount) * 100m;
    }

    public bool ValidateStatutoryB2BCeiling(decimal masterLCAmount)
    {
        if (masterLCAmount <= 0) return false;
        var percentage = CalculateCeilingPercentage(masterLCAmount);
        return percentage <= StatutoryB2BCeilingPercentage;
    }
}
