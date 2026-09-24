using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using Xunit;

namespace GarmentsERP.Production.UnitTests;

public class CuttingTests
{
    [Fact]
    public void CuttingOrder_ShouldInitializeWithDefaultValues()
    {
        var order = new CuttingOrder
        {
            CuttingOrderNumber = "CO-9001",
            BuyerOrderId = Guid.NewGuid(),
            StyleNumber = "ST-882",
            FabricType = "100% Pique Cotton",
            TotalRequiredQty = 2000,
            TotalPlies = 100,
            MarkerLength = 5.5m,
            FabricWidth = 60.0m,
            EfficiencyPercentage = 89.5m,
            TableNumber = "Table-01"
        };

        Assert.Equal("CO-9001", order.CuttingOrderNumber);
        Assert.Equal(CuttingStatus.Draft, order.Status);
        Assert.Equal(89.5m, order.EfficiencyPercentage);
        Assert.Equal(100, order.TotalPlies);
    }

    [Theory]
    [InlineData(2000, 50, 40)]
    [InlineData(2050, 50, 41)]
    [InlineData(100, 20, 5)]
    public void BundleCalculation_ShouldReturnAccurateBundleCount(int totalQty, int bundleSize, int expectedBundles)
    {
        var bundleCount = (int)Math.Ceiling((double)totalQty / bundleSize);
        Assert.Equal(expectedBundles, bundleCount);
    }
}
