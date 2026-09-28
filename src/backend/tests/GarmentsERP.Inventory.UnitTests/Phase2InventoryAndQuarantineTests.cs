using GarmentsERP.Inventory.Domain.Entities;
using Xunit;

namespace GarmentsERP.Inventory.UnitTests;

public class Phase2InventoryAndQuarantineTests
{
    [Fact]
    public void StockLot_WhenInQuarantine_VerifyAvailableForCuttingShouldBlock_AC04()
    {
        // Arrange: Newly received fabric lot from GRN
        var lot = new StockLot
        {
            LotBatchNumber = "LOT-TW-2026-02",
            ItemCode = "FAB-TW-01",
            Quantity = 36750,
            StockState = StockLot.StateQuarantine,
            QCInspectionStatus = StockLot.QcStatusPending
        };

        // Act
        var (isAllowed, errorMessage) = lot.VerifyAvailableForCutting();

        // Assert: Strict AC-04 block
        Assert.False(isAllowed);
        Assert.Contains("MATERIAL ALLOCATION BLOCKED", errorMessage);
        Assert.Contains("QUARANTINE", errorMessage);
        Assert.Contains("verified QC Inspection PASS certificate", errorMessage);
    }

    [Fact]
    public void FabricInspection_ASTM_D5430_ShouldCalculatePointsCorrectlyAndPass()
    {
        // Arrange: 375 yards inspected across 58.00 inch cuttable width, 110 penalty points detected
        decimal totalPenaltyPoints = 110m;
        decimal yardsInspected = 375m;
        decimal cuttableWidth = 58.00m;

        // Act: Formula = (Points * 3600) / (Yards * Width)
        var score = FabricInspection.CalculateAstmScore(totalPenaltyPoints, yardsInspected, cuttableWidth);

        // Assert: (110 * 3600) / (375 * 58) = 396000 / 21750 = 18.21 pts/100 sq.yd
        Assert.Equal(18.21m, score);
        Assert.True(score <= FabricInspection.AstmAcceptanceThreshold); // <= 28.0
    }

    [Fact]
    public void FabricInspection_WhenPassed_ShouldReleaseStockLotToProduction_AC04()
    {
        // Arrange
        var lot = new StockLot
        {
            LotBatchNumber = "LOT-TW-2026-01",
            StockState = StockLot.StateQuarantine,
            QCInspectionStatus = StockLot.QcStatusPending
        };

        var inspection = new FabricInspection
        {
            InspectionNumber = "QC-4PT-2026-0045",
            InspectorName = "Mahmud Hasan (QC Lead)"
        };

        // Act: 110 points across 375 yds of 58" fabric
        inspection.EvaluateInspection(110m, 375m, 58m, lot);

        // Assert: Inspection passes and StockLot is released
        Assert.Equal("PASSED", inspection.OverallResult);
        Assert.Equal(StockLot.StateReleased, lot.StockState);
        Assert.Equal(StockLot.QcStatusPassed, lot.QCInspectionStatus);
        Assert.NotNull(lot.QCPassedAtUtc);

        // Verify cutting allocation is now permitted
        var (isAllowed, errorMessage) = lot.VerifyAvailableForCutting();
        Assert.True(isAllowed);
        Assert.Empty(errorMessage);
    }

    [Fact]
    public void FabricInspection_WhenExceedingThreshold_ShouldRejectLotAndMaintainBlock()
    {
        // Arrange
        var lot = new StockLot
        {
            LotBatchNumber = "LOT-REJECT-01",
            StockState = StockLot.StateQuarantine,
            QCInspectionStatus = StockLot.QcStatusPending
        };

        var inspection = new FabricInspection
        {
            InspectionNumber = "QC-4PT-FAIL",
            InspectorName = "Inspector Badri"
        };

        // Act: 200 points on 200 yds of 58" fabric -> (200 * 3600) / (200 * 58) = 62.07 pts/100 sq.yd (> 28.0)
        inspection.EvaluateInspection(200m, 200m, 58m, lot);

        // Assert
        Assert.Equal("REJECTED", inspection.OverallResult);
        Assert.Equal(StockLot.StateRejected, lot.StockState);
        Assert.Equal(StockLot.QcStatusFailed, lot.QCInspectionStatus);

        var (isAllowed, errorMessage) = lot.VerifyAvailableForCutting();
        Assert.False(isAllowed);
        Assert.Contains("REJECTED", errorMessage);
    }

    [Fact]
    public void ThreadDyeingRecipe_ShouldComputeLiquorVolumeAndChemicalIngredients_THP02()
    {
        // Arrange: 1,000 Kg yarn batch at 1:10 liquor ratio = 10,000 Liters
        var recipe = new ThreadDyeingRecipe
        {
            RecipeCode = "RCP-POLY-NAVY-40",
            LiquorRatio = 10.0m,
            BatchWeightKg = 1000m
        };

        // Act: Add 2.4 g/L disperse dye @ $14.50/Kg
        recipe.AddIngredient("Disperse Navy Blue", "Dye", concentrationGpl: 2.40m, unitCostPerKg: 14.50m);

        // Assert: 10,000 L * 2.4 g/L = 24,000 g = 24 Kg
        Assert.Equal(10000m, recipe.TotalLiquorVolumeLiters);
        var ingredient = Assert.Single(recipe.Ingredients);
        Assert.Equal(24.000m, ingredient.TotalRequiredQuantityKg);
        Assert.Equal(348.00m, ingredient.TotalCost); // 24 Kg * $14.50
    }

    [Fact]
    public void ThreadTensileTest_ShouldCertifyQualityPassCriteria_THP03()
    {
        // Arrange
        var testPass = new ThreadTensileTest
        {
            TenacityCentiNewtonsPerTex = 39.4m,
            ElongationPercentage = 16.2m
        };

        var testFail = new ThreadTensileTest
        {
            TenacityCentiNewtonsPerTex = 31.0m, // Below 35.0
            ElongationPercentage = 9.5m        // Below 12.0
        };

        // Act & Assert
        Assert.True(testPass.IsCertifiedPassed);
        Assert.False(testFail.IsCertifiedPassed);
    }
}
