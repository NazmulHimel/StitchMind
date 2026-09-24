using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public class CuttingOrder : BaseEntity
{
    public string CuttingOrderNumber { get; set; } = string.Empty;
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder? BuyerOrder { get; set; }
    public string StyleNumber { get; set; } = string.Empty;
    public string FabricType { get; set; } = string.Empty;
    public int TotalRequiredQty { get; set; }
    public int TotalPlies { get; set; }
    public decimal MarkerLength { get; set; } // in meters/yards
    public decimal FabricWidth { get; set; } // in inches
    public decimal EfficiencyPercentage { get; set; } // CAD nesting efficiency %
    public CuttingStatus Status { get; set; } = CuttingStatus.Draft;
    public string TableNumber { get; set; } = "Table-01";
    public DateTime CuttingDate { get; set; } = DateTime.UtcNow;

    public ICollection<CutBundle> Bundles { get; set; } = new List<CutBundle>();
}
