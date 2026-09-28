using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// Granular budget line inside a Pre-Costing sheet (MOD-01).
/// </summary>
public class CostingLine : BaseEntity
{
    public Guid PreCostingId { get; set; }
    public PreCosting? PreCosting { get; set; }
    public string Category { get; set; } = "Fabric"; // Fabric, Thread, Trims, Labor, Overhead
    public string ItemDescription { get; set; } = string.Empty;
    public decimal ConsumptionPerDzn { get; set; }
    public decimal UnitRate { get; set; }
    public decimal TotalBudgetPerDzn => ConsumptionPerDzn * UnitRate;
}
