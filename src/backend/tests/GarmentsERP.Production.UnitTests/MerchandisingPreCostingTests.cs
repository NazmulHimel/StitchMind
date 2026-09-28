using GarmentsERP.Production.Domain.Entities;
using Xunit;

namespace GarmentsERP.Production.UnitTests;

public class MerchandisingPreCostingTests
{
    [Fact]
    public void BuyerOrder_WithColorSizeMatrixAndBOM_ShouldMaintainDossierConsistency()
    {
        // Arrange
        var buyerId = Guid.NewGuid();
        var order = new BuyerOrder
        {
            BuyerId = buyerId,
            OrderNumber = "ORD-2026-CHINO-001",
            OrderQuantity = 24000,
            DeliveryDate = DateTime.UtcNow.AddMonths(3),
            StyleNumber = "ST-2026-CHINO-01",
            StyleName = "Classic Slim Fit Chino",
            Season = "Fall/Winter 2026",
            FabricComposition = "98% Cotton, 2% Spandex Stretch Twill, 240 GSM",
            ProductCategory = "Bottoms / Trouser",
            UnitPrice = 8.50m
        };

        var colorSize = new StyleColorSize
        {
            BuyerOrderId = order.Id,
            ColorName = "Navy",
            PantoneCode = "TCX 19-4010",
            SizeLabel = "32/32",
            Quantity = 3000
        };
        order.ColorSizes.Add(colorSize);

        var bom = new BOMItem
        {
            BuyerOrderId = order.Id,
            ItemCategory = "Fabric",
            ItemCode = "FAB-TWILL-NAVY-240",
            ItemDescription = "98% Cotton 2% Spandex Twill",
            UnitOfMeasure = "Yds",
            NetConsumptionPerDzn = 16.20m,
            WastagePercentage = 5.0m,
            TotalRequiredQty = 34020m
        };
        order.BOMItems.Add(bom);

        // Act & Assert
        Assert.Single(order.ColorSizes);
        Assert.Equal("Navy", order.ColorSizes.First().ColorName);
        Assert.Equal("32/32", order.ColorSizes.First().SizeLabel);
        Assert.Single(order.BOMItems);
        Assert.Equal("FAB-TWILL-NAVY-240", order.BOMItems.First().ItemCode);
        Assert.Equal(8.50m, order.UnitPrice);
        Assert.Equal(8.50m, order.FOBPricePerPc);
        Assert.Equal(102.00m, order.FOBPricePerDzn);
        Assert.Equal(204000m, order.TotalValue);
    }

    [Fact]
    public void PreCosting_ApproveBudget_ShouldTransitionStatusAndSetApprovedTimestamp()
    {
        // Arrange
        var approverId = Guid.NewGuid();
        var preCosting = new PreCosting
        {
            BuyerOrderId = Guid.NewGuid(),
            CostingVersion = 1,
            CostingBasis = "PerDozen",
            FOBPricePerDzn = 98.40m, // $8.20/pc * 12
            TotalMaterialsBudget = 51.84m, // $4.32 fabric + $0.88 trims * 12
            TotalLaborCostPerDzn = 26.40m, // $2.20 CM budget cap * 12
            TotalOverheadPerDzn = 9.60m,
            NetMarginPerDzn = 10.56m,
            IsBudgetApproved = false
        };

        // Act
        preCosting.ApproveBudget(approverId);

        // Assert
        Assert.True(preCosting.IsBudgetApproved);
        Assert.Equal(approverId, preCosting.ApprovedBy);
        Assert.NotNull(preCosting.ApprovedAt);
        Assert.Equal(26.40m, preCosting.TotalLaborCostPerDzn);
        Assert.Equal(98.40m, preCosting.FOBPricePerDzn);
    }
}
