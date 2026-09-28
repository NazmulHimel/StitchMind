using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Finance.Domain.Entities;

/// <summary>
/// ACC-06: Fixed Asset Register & Automated Straight-Line Depreciation Engine.
/// Tracks capital expenditure, floor machinery lifecycle, and monthly depreciation journal vouchers.
/// </summary>
public class FixedAsset : BaseEntity
{
    public string AssetCode { get; set; } = string.Empty; // e.g. FA-SEW-001
    public string AssetName { get; set; } = "Juki DDL-9000C Direct Drive Lockstitch Machine";
    public string Department { get; set; } = "Sewing Floor Line 04";
    public DateTime PurchaseDate { get; set; } = DateTime.UtcNow.AddYears(-1);
    public decimal OriginalCost { get; set; } = 1450.00m;
    public decimal SalvageValue { get; set; } = 150.00m;
    public int UsefulLifeYears { get; set; } = 5;
    public string DepreciationMethod { get; set; } = "StraightLine";

    public decimal DepreciableBase => Math.Max(0m, OriginalCost - SalvageValue);
    public decimal AnnualDepreciation => UsefulLifeYears > 0 ? Math.Round(DepreciableBase / UsefulLifeYears, 2) : 0m;
    public decimal MonthlyDepreciation => Math.Round(AnnualDepreciation / 12.0m, 2);

    public decimal AccumulatedDepreciation { get; set; } = 260.00m;
    public decimal CurrentBookValue => Math.Max(SalvageValue, OriginalCost - AccumulatedDepreciation);

    public bool IsActive { get; set; } = true;

    public void ApplyMonthlyDepreciation()
    {
        var nextAccum = AccumulatedDepreciation + MonthlyDepreciation;
        if (nextAccum > DepreciableBase)
        {
            AccumulatedDepreciation = DepreciableBase;
        }
        else
        {
            AccumulatedDepreciation = nextAccum;
        }
    }
}
