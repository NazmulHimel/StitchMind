using GarmentsERP.Production.Domain.Entities;
using Xunit;

namespace GarmentsERP.Production.UnitTests;

public class DispatchTests
{
    [Fact]
    public void CartonPack_ShouldInitializeCorrectly()
    {
        var carton = new CartonPack
        {
            CartonBarcode = "CTN-2026-0088",
            BuyerOrderId = Guid.NewGuid(),
            StyleNumber = "ST-882",
            Color = "Navy Blue",
            SizeBreakdownJson = "{\"S\":10,\"M\":20,\"L\":20}",
            TotalQuantityPcs = 50,
            GrossWeightKg = 12.5m,
            NetWeightKg = 11.8m,
            Status = CartonStatus.Packed
        };

        Assert.Equal("CTN-2026-0088", carton.CartonBarcode);
        Assert.Equal(50, carton.TotalQuantityPcs);
        Assert.Equal(CartonStatus.Packed, carton.Status);
    }

    [Fact]
    public void DeliveryChallan_ShouldTrackExportGatePassDetails()
    {
        var challan = new DeliveryChallan
        {
            ChallanNumber = "CH-2026-0012",
            BuyerId = Guid.NewGuid(),
            DestinationAddress = "Chittagong Port Yard 4",
            VehicleNumber = "DHAKA-METRO-TA-11-9044",
            DriverName = "Md. Jashim Uddin",
            DriverPhone = "+880 1712-889900",
            TotalCartons = 80,
            TotalGarmentPcs = 2000,
            TotalWeightKg = 980.5m,
            DispatchDate = DateTime.UtcNow,
            Status = ChallanStatus.Prepared,
            GatePassNumber = "GP-2026-0081"
        };

        Assert.Equal("CH-2026-0012", challan.ChallanNumber);
        Assert.Equal(80, challan.TotalCartons);
        Assert.Equal(2000, challan.TotalGarmentPcs);
        Assert.Equal(ChallanStatus.Prepared, challan.Status);
    }
}
