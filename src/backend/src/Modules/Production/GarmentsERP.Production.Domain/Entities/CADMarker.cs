using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// CUT-01: CAD Marker Entity representing Gerber/Lectra/Optitex nested marker benchmarks.
/// </summary>
public class CADMarker : BaseEntity
{
    public string MarkerName { get; set; } = string.Empty; // e.g. MKR-CHINO-SZ32-34-A
    public Guid? StyleId { get; set; }
    public string StyleNumber { get; set; } = string.Empty;
    public string CADSoftware { get; set; } = "Gerber AccuMark v16";
    public decimal CuttableWidthInches { get; set; } = 58.0m;
    public decimal MarkerLengthYards { get; set; }
    public int GarmentsPerMarker { get; set; } = 4;
    public string SizeDistributionRatio { get; set; } = "32/32: 2 pcs, 34/32: 2 pcs";
    public decimal MarkerEfficiencyPct { get; set; } = 87.20m;
    public decimal PlannedConsumptionPerPcYds { get; set; }
    public string FileFormat { get; set; } = "ASTM/AAMA DXF";

    public void RecalculateConsumption()
    {
        if (GarmentsPerMarker > 0 && MarkerLengthYards > 0)
        {
            PlannedConsumptionPerPcYds = Math.Round(MarkerLengthYards / GarmentsPerMarker, 4);
        }
    }

    public bool MeetsEfficiencyBenchmark(decimal benchmarkPct = 85.0m)
    {
        return MarkerEfficiencyPct >= benchmarkPct;
    }
}
