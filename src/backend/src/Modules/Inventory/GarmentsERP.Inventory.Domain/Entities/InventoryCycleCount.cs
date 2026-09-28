using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

/// <summary>
/// WHS-05: Physical Inventory Cycle Counting & Variance Adjustment Authorization.
/// Audits physical warehouse rolls against ERP system ledger quantities.
/// Discrepancies require Warehouse Manager or GM authorization prior to stock ledger adjustment.
/// </summary>
public class InventoryCycleCount : BaseEntity
{
    public string CycleCountNumber { get; set; } = string.Empty;
    public DateTime AuditDate { get; set; } = DateTime.UtcNow;
    public string AuditorName { get; set; } = string.Empty;
    public string LocationZone { get; set; } = "RawMaterialFabric";
    public string Status { get; set; } = "Draft"; // Draft, InReview, Approved_Adjusted
    public string? AuthorizedBy { get; set; }
    public DateTime? AuthorizedAtUtc { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }

    public ICollection<InventoryCycleCountLine> Lines { get; set; } = new List<InventoryCycleCountLine>();

    public void AddLine(
        string itemCode,
        string rollBarcode,
        string locationCode,
        decimal systemQty,
        decimal physicalQty,
        string reasonCode = "RoutineAudit")
    {
        var line = new InventoryCycleCountLine
        {
            InventoryCycleCountId = Id,
            ItemCode = itemCode,
            RollBarcode = rollBarcode,
            LocationCode = locationCode,
            SystemQuantity = systemQty,
            PhysicalCountedQuantity = physicalQty,
            VarianceQuantity = physicalQty - systemQty,
            VariancePercentage = systemQty > 0 ? Math.Round(((physicalQty - systemQty) / systemQty) * 100m, 2) : 0m,
            ReasonCode = reasonCode,
            IsAdjusted = false
        };

        Lines.Add(line);
        UpdatedAt = DateTime.UtcNow;
    }

    public void AuthorizeAdjustment(string managerName)
    {
        AuthorizedBy = managerName;
        AuthorizedAtUtc = DateTime.UtcNow;
        Status = "Approved_Adjusted";

        foreach (var line in Lines)
        {
            line.IsAdjusted = true;
        }

        UpdatedAt = DateTime.UtcNow;
    }
}

public class InventoryCycleCountLine : BaseEntity
{
    public Guid InventoryCycleCountId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string RollBarcode { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty;
    public decimal SystemQuantity { get; set; }
    public decimal PhysicalCountedQuantity { get; set; }
    public decimal VarianceQuantity { get; set; }
    public decimal VariancePercentage { get; set; }
    public string ReasonCode { get; set; } = "RoutineAudit";
    public bool IsAdjusted { get; set; }

    public InventoryCycleCount InventoryCycleCount { get; set; } = null!;
}
