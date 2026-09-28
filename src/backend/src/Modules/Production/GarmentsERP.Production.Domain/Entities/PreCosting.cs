using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// Baseline Pre-Costing budget sheet that locks procurement prices and dictates maximum allowable material costs (MOD-01 / AC-03).
/// </summary>
public class PreCosting : BaseEntity
{
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder? BuyerOrder { get; set; }
    public int CostingVersion { get; set; } = 1;
    public string CostingBasis { get; set; } = "PerDozen";
    public decimal FOBPricePerDzn { get; set; }
    public decimal TotalMaterialsBudget { get; set; }
    public decimal TotalLaborCostPerDzn { get; set; }
    public decimal TotalOverheadPerDzn { get; set; }
    public decimal NetMarginPerDzn { get; set; }
    public bool IsBudgetApproved { get; set; } = false;
    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public ICollection<CostingLine> Lines { get; set; } = new List<CostingLine>();

    public void ApproveBudget(Guid approverUserId)
    {
        IsBudgetApproved = true;
        ApprovedBy = approverUserId;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
