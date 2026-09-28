using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Procurement.Domain.Entities;

/// <summary>
/// Material Requisition (MR) directly derived from approved Style BOMs (PRO-01).
/// </summary>
public class MaterialRequisition : BaseEntity
{
    public string RequisitionNumber { get; set; } = string.Empty;
    public Guid StyleId { get; set; }
    public string StyleNumber { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public DateTime RequisitionDate { get; set; } = DateTime.UtcNow;
    public DateTime RequiredDate { get; set; }
    public RequisitionStatus Status { get; set; } = RequisitionStatus.Draft;
    public string? Department { get; set; } = "Merchandising";
    public string? Remarks { get; set; }
    public Guid CompanyId { get; set; }

    public ICollection<MaterialRequisitionItem> Items { get; set; } = new List<MaterialRequisitionItem>();

    public decimal TotalEstimatedValue => Items.Sum(i => i.TotalEstimatedCost);

    public void Submit()
    {
        if (Status != RequisitionStatus.Draft)
            throw new InvalidOperationException("Only draft requisitions can be submitted.");
        Status = RequisitionStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        if (Status != RequisitionStatus.Submitted)
            throw new InvalidOperationException("Requisition must be submitted before approval.");
        Status = RequisitionStatus.Approved;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class MaterialRequisitionItem : BaseEntity
{
    public Guid MaterialRequisitionId { get; set; }
    public MaterialRequisition? MaterialRequisition { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public MaterialCategory Category { get; set; }
    public UOM UnitOfMeasure { get; set; }
    public decimal RequiredQuantity { get; set; }
    public decimal EstimatedUnitPrice { get; set; }
    public decimal TotalEstimatedCost => RequiredQuantity * EstimatedUnitPrice;
}
