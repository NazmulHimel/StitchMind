using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-03: Export Master Letter of Credit (MLC) for trade finance.
/// Provides collateral basis for opening Back-to-Back (B2B) LCs up to authorized percentage (e.g. 70%).
/// </summary>
public class MasterLetterOfCredit : BaseEntity
{
    public string MLCNumber { get; set; } = string.Empty; // e.g. MLC-2026-0891
    public Guid BuyerId { get; set; }
    public string BuyerName { get; set; } = "H&M Hennes & Mauritz GBC AB";
    public string IssuingBank { get; set; } = "Swedbank Sweden";
    public string AdvisingBank { get; set; } = "Standard Chartered Bank Dhaka";
    public string Beneficiary { get; set; } = "StitchMind Garments Industries Ltd.";
    public decimal TotalAmount { get; set; } = 180000.00m;
    public string Currency { get; set; } = "USD";
    public DateTime IssueDate { get; set; } = DateTime.UtcNow.AddDays(-30);
    public DateTime ExpiryDate { get; set; } = DateTime.UtcNow.AddDays(90);
    public DateTime LatestShipmentDate { get; set; } = DateTime.UtcNow.AddDays(45);
    public decimal MaxB2BAllocationPct { get; set; } = 70.0m;
    public decimal MaxB2BAllocationAmount => Math.Round(TotalAmount * (MaxB2BAllocationPct / 100m), 2);
    public decimal UtilizedB2BAmount { get; set; } = 0m;
    public decimal AvailableB2BAmount => Math.Max(0m, MaxB2BAllocationAmount - UtilizedB2BAmount);
    public LetterOfCreditStatus Status { get; set; } = LetterOfCreditStatus.Active;
    public decimal RealizedAmount { get; set; } = 0m;
    public DateTime? RealizedAtUtc { get; set; }

    public ICollection<BackToBackLetterOfCredit> BackToBackLCs { get; set; } = new List<BackToBackLetterOfCredit>();
    public ICollection<ForeignBillDiscounting> DiscountingBills { get; set; } = new List<ForeignBillDiscounting>();

    public bool CanAllocateB2B(decimal requestedAmount)
    {
        return AvailableB2BAmount >= requestedAmount;
    }

    public void RecordB2BAllocation(decimal amount)
    {
        if (!CanAllocateB2B(amount))
            throw new InvalidOperationException($"Insufficient B2B allocation headroom. Available: ${AvailableB2BAmount}, Requested: ${amount}");

        UtilizedB2BAmount += amount;
        if (Status == LetterOfCreditStatus.Active && UtilizedB2BAmount > 0)
        {
            Status = LetterOfCreditStatus.PartiallyUtilized;
        }
    }

    public void RealizeProceeds(decimal amount)
    {
        RealizedAmount += amount;
        RealizedAtUtc = DateTime.UtcNow;
        if (RealizedAmount >= TotalAmount)
        {
            Status = LetterOfCreditStatus.FullyRealized;
        }
    }
}
