using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

/// <summary>
/// International Fabric 4-Point System (ASTM D5430-07) quality inspection audit (MOD-03 / AC-04).
/// </summary>
public class FabricInspection : BaseEntity
{
    public const decimal AstmAcceptanceThreshold = 28.00m; // Max points per 100 sq. yds for export garments

    public string InspectionNumber { get; set; } = string.Empty;
    public Guid StockLotId { get; set; }
    public StockLot? StockLot { get; set; }
    public string InspectorName { get; set; } = string.Empty;
    public DateTime InspectionDate { get; set; } = DateTime.UtcNow;
    public int TotalRollsInLot { get; set; }
    public int RollsInspected { get; set; }
    public decimal TotalYardsInspected { get; set; }
    public decimal CuttableWidthInches { get; set; } = 58.00m;
    public decimal TotalPenaltyPoints { get; set; }
    public decimal AveragePointsPer100SqYds { get; set; }
    public string OverallResult { get; set; } = "PENDING"; // PASSED, REJECTED
    public string? Remarks { get; set; }
    public Guid CompanyId { get; set; }

    /// <summary>
    /// Calculates the ASTM D5430-07 4-Point penalty points per 100 square yards:
    /// Score = (Total Penalty Points * 3600) / (Inspected Length in Yds * Cuttable Width in Inches)
    /// </summary>
    public static decimal CalculateAstmScore(decimal totalPoints, decimal yardsInspected, decimal widthInches)
    {
        if (yardsInspected <= 0 || widthInches <= 0) return 0;
        var denominator = yardsInspected * widthInches;
        var score = (totalPoints * 3600m) / denominator;
        return Math.Round(score, 2);
    }

    /// <summary>
    /// Executes the inspection grading, determines PASS/FAIL, and updates the associated StockLot state.
    /// </summary>
    public void EvaluateInspection(decimal totalPoints, decimal yardsInspected, decimal widthInches, StockLot? linkedLot = null)
    {
        TotalPenaltyPoints = totalPoints;
        TotalYardsInspected = yardsInspected;
        CuttableWidthInches = widthInches;
        AveragePointsPer100SqYds = CalculateAstmScore(totalPoints, yardsInspected, widthInches);

        if (AveragePointsPer100SqYds <= AstmAcceptanceThreshold)
        {
            OverallResult = "PASSED";
            Remarks = $"ASTM D5430-07 PASSED: {AveragePointsPer100SqYds:F2} pts/100 sq.yd (Threshold <= {AstmAcceptanceThreshold:F1}). Lot released to production.";
            linkedLot?.ReleaseToProduction(InspectorName);
        }
        else
        {
            OverallResult = "REJECTED";
            Remarks = $"ASTM D5430-07 REJECTED: {AveragePointsPer100SqYds:F2} pts/100 sq.yd exceeds allowable limit of {AstmAcceptanceThreshold:F1}. Quarantine locked; debit note required.";
            linkedLot?.Reject(InspectorName, Remarks);
        }

        UpdatedAt = DateTime.UtcNow;
    }
}
