using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

/// <summary>
/// Thread Dyeing Chemical Recipe Calculation Engine & Cost Breakdown (THP-02).
/// Computes liquor volume, dye stuff concentrations, and chemical auxiliaries per batch.
/// </summary>
public class ThreadDyeingRecipe : BaseEntity
{
    public string RecipeCode { get; set; } = string.Empty;
    public string ColorName { get; set; } = string.Empty;
    public string ColorCode { get; set; } = string.Empty; // e.g. "TCX 19-4010 (Navy)"
    public decimal LiquorRatio { get; set; } = 10.0m; // 1:10 liquor ratio standard
    public decimal BatchWeightKg { get; set; }
    public decimal TotalLiquorVolumeLiters => BatchWeightKg * LiquorRatio;

    public decimal DyeStuffCostPerKg { get; set; }
    public decimal ChemicalAuxiliaryCostPerKg { get; set; }
    public decimal UtilityEnergyCostPerKg { get; set; }
    public decimal TotalDyeingCostPerKg => DyeStuffCostPerKg + ChemicalAuxiliaryCostPerKg + UtilityEnergyCostPerKg;

    public ICollection<RecipeIngredient> Ingredients { get; set; } = new List<RecipeIngredient>();

    public void AddIngredient(string chemicalName, string function, decimal concentrationGpl, decimal unitCostPerKg)
    {
        var requiredGrams = TotalLiquorVolumeLiters * concentrationGpl;
        var requiredKg = requiredGrams / 1000m;
        var totalCost = requiredKg * unitCostPerKg;

        Ingredients.Add(new RecipeIngredient
        {
            ThreadDyeingRecipeId = Id,
            ChemicalName = chemicalName,
            Function = function,
            ConcentrationGramsPerLiter = concentrationGpl,
            TotalRequiredQuantityKg = Math.Round(requiredKg, 3),
            UnitCostPerKg = unitCostPerKg,
            TotalCost = Math.Round(totalCost, 2)
        });
    }
}

public class RecipeIngredient : BaseEntity
{
    public Guid ThreadDyeingRecipeId { get; set; }
    public string ChemicalName { get; set; } = string.Empty;
    public string Function { get; set; } = string.Empty; // Dye, Leveling Agent, Acetic Acid, Salt, Softener
    public decimal ConcentrationGramsPerLiter { get; set; }
    public decimal TotalRequiredQuantityKg { get; set; }
    public decimal UnitCostPerKg { get; set; }
    public decimal TotalCost { get; set; }
}

/// <summary>
/// Thread Laboratory Tensile Strength & Quality Test Certificate (THP-03 / ASTM D2256).
/// </summary>
public class ThreadTensileTest : BaseEntity
{
    public string TestCertificateNumber { get; set; } = string.Empty;
    public Guid ThreadBatchId { get; set; }
    public string BatchNumber { get; set; } = string.Empty;
    public string YarnCount { get; set; } = "40/2";
    public DateTime TestDate { get; set; } = DateTime.UtcNow;
    public decimal TenacityCentiNewtonsPerTex { get; set; } // Benchmark: >= 38.0 cN/tex
    public decimal ElongationPercentage { get; set; } // Benchmark: 14.0% - 18.0%
    public decimal SingleYarnBreakingForceGrams { get; set; }
    public decimal UnevennessCVPercentage { get; set; }
    public bool IsCertifiedPassed => TenacityCentiNewtonsPerTex >= 35.0m && ElongationPercentage >= 12.0m;
    public string TestedBy { get; set; } = "Senior Textile Lab Technologist";
    public string PackagingBarcode { get; set; } = string.Empty;
}
