using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// CUT-03: Cutting Order & Lay Planning Entity.
/// Manages table allocation, CAD marker link, shade-segregated lay planning, and wastage reconciliation.
/// </summary>
public class CuttingOrder : BaseEntity
{
    public string CuttingOrderNumber { get; set; } = string.Empty; // e.g. CUT-2026-0001
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder? BuyerOrder { get; set; }
    public string StyleNumber { get; set; } = string.Empty;
    public string FabricType { get; set; } = string.Empty;
    public int TotalRequiredQty { get; set; }
    public int TotalPlies { get; set; } // e.g. 60 plies
    public decimal MarkerLength { get; set; } // in yards, e.g. 8.42 Yds
    public decimal FabricWidth { get; set; } = 58.0m; // in inches
    public decimal EfficiencyPercentage { get; set; } = 87.2m; // CAD nesting efficiency %
    public CuttingStatus Status { get; set; } = CuttingStatus.Draft;
    public string TableNumber { get; set; } = "Table-01"; // e.g. Table-03
    public DateTime CuttingDate { get; set; } = DateTime.UtcNow;

    // Phase 4 / Module 04 Extensions
    public Guid? MarkerId { get; set; }
    public string MarkerName { get; set; } = string.Empty;
    public string ShadeGroup { get; set; } = "Shade A"; // Mandatory segregation: A, B, C
    public int GarmentsPerMarker { get; set; } = 4;
    public int TotalGarmentsPlanned { get; set; } // Formula: TotalPlies * GarmentsPerMarker
    public decimal ActualFabricConsumedYds { get; set; }
    public decimal EndLossRemnantYds { get; set; }
    public decimal CuttingWastageVariancePct { get; set; }
    public string AllocatedRollBarcodes { get; set; } = string.Empty; // e.g. "ROL-2026-00891, ROL-2026-00892"
    public string ApprovedBy { get; set; } = string.Empty;

    public ICollection<CutBundle> Bundles { get; set; } = new List<CutBundle>();

    public void RecalculateLayPlanning()
    {
        if (GarmentsPerMarker > 0)
        {
            TotalGarmentsPlanned = TotalPlies * GarmentsPerMarker;
        }

        var plannedFabricTotal = TotalPlies * MarkerLength;
        if (plannedFabricTotal > 0 && ActualFabricConsumedYds > 0)
        {
            var difference = ActualFabricConsumedYds - plannedFabricTotal;
            CuttingWastageVariancePct = Math.Round((difference / plannedFabricTotal) * 100m, 2);
        }
    }
}
