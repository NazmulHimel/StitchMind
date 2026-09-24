using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using Xunit;

namespace GarmentsERP.Inventory.UnitTests;

public class InventoryItemTests
{
    [Fact]
    public void Create_ShouldInitializeStockValues()
    {
        var companyId = Guid.NewGuid();
        var item = InventoryItem.Create("FAB-001", "Cotton Denim", "Heavyweight", ItemCategory.Fabric, StockUOM.Yards, 500, 100, 200, 3.50m, "RACK-1", companyId);

        Assert.Equal("FAB-001", item.ItemCode);
        Assert.Equal(500m, item.CurrentStock);
        Assert.Equal(100m, item.SafetyStock);
        Assert.Equal(200m, item.ReorderPoint);
    }

    [Fact]
    public void AddStock_And_DeductStock_ShouldUpdateCurrentStock()
    {
        var item = InventoryItem.Create("FAB-002", "Twill", "Medium", ItemCategory.Fabric, StockUOM.Yards, 100, 10, 20, 2.00m, "RACK-2", Guid.NewGuid());

        item.AddStock(50);
        Assert.Equal(150m, item.CurrentStock);

        item.DeductStock(30);
        Assert.Equal(120m, item.CurrentStock);
    }

    [Fact]
    public void DeductStock_ExceedingAvailable_ShouldThrow()
    {
        var item = InventoryItem.Create("FAB-003", "Poplin", "Light", ItemCategory.Fabric, StockUOM.Yards, 50, 10, 20, 2.00m, "RACK-3", Guid.NewGuid());

        Assert.Throws<InvalidOperationException>(() => item.DeductStock(100));
    }
}

public class FabricRollTests
{
    [Fact]
    public void IssueYards_ShouldDeductFromRoll_And_UpdateStatus()
    {
        var roll = FabricRoll.Create("ROL-001", Guid.NewGuid(), "LOT-A", 380m, 58m, 100m, "BIN-1", Guid.NewGuid());

        roll.IssueYards(40);
        Assert.Equal(60m, roll.AvailableLengthYards);
        Assert.Equal(RollStatus.InCutting, roll.Status);

        roll.IssueYards(60);
        Assert.Equal(0m, roll.AvailableLengthYards);
        Assert.Equal(RollStatus.Consumed, roll.Status);
    }
}

public class ThreadBatchTests
{
    [Fact]
    public void AdvanceStage_ShouldLogStep_And_TrackWastage()
    {
        var batch = ThreadBatch.Create("YRN-001", "40/2 Ne", "100% Poly", "BLK-01", "Black", 1000m, Guid.NewGuid());

        Assert.Equal(ThreadStage.RawFiber, batch.CurrentStage);
        Assert.Equal(ThreadBatchStatus.InProgress, batch.Status);

        batch.AdvanceStage(ThreadStage.Spinning, 970m, 30m, "M-01", "Op-1", "Combed OK");
        Assert.Equal(ThreadStage.Spinning, batch.CurrentStage);
        Assert.Equal(970m, batch.CurrentQuantityKg);
        Assert.Equal(30m, batch.TotalWastageKg);
        Assert.Single(batch.StageLogs);

        batch.AdvanceStage(ThreadStage.Packaged, 960m, 10m, "M-02", "Op-2", "Coned");
        Assert.Equal(ThreadStage.Packaged, batch.CurrentStage);
        Assert.Equal(ThreadBatchStatus.Completed, batch.Status);
        Assert.Equal(40m, batch.TotalWastageKg);
    }
}
