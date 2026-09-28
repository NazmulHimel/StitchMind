using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// Bill of Materials (BOM) specification item for garments manufacturing (MOD-01).
/// </summary>
public class BOMItem : BaseEntity
{
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder? BuyerOrder { get; set; }
    public string ItemCategory { get; set; } = string.Empty; // Fabric, Thread, Trims, Labels, Packaging
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = "Yds";
    public decimal NetConsumptionPerDzn { get; set; }
    public decimal WastagePercentage { get; set; } // e.g. 5.00%
    public decimal GrossConsumptionPerDzn => NetConsumptionPerDzn * (1 + (WastagePercentage / 100m));
    public decimal TotalRequiredQty { get; set; }
}
