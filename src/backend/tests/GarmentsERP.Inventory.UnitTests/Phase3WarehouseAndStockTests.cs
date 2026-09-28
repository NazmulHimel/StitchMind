using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using Xunit;

namespace GarmentsERP.Inventory.UnitTests;

public class Phase3WarehouseAndStockTests
{
    private readonly Guid _defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public void WarehouseLocation_WhenRollsAssigned_OccupancyCalculatedCorrectly_WHS01()
    {
        // Arrange: 3D bin with capacity of 12 rolls
        var loc = WarehouseLocation.Create("WH1", "A03", "R02", "T01", "B01", WarehouseZone.RawMaterialFabric, 12, _defaultCompanyId);

        // Act & Assert Initial
        Assert.Equal("WH1-A03-R02-T01-B01", loc.LocationCode);
        Assert.Equal(0m, loc.OccupancyPercentage);
        Assert.False(loc.IsFull);

        // Act 1: Assign 6 rolls (50% capacity)
        loc.AssignRoll(6);
        Assert.Equal(50.0m, loc.OccupancyPercentage);
        Assert.False(loc.IsFull);

        // Act 2: Assign another 6 rolls (100% capacity)
        loc.AssignRoll(6);
        Assert.Equal(100.0m, loc.OccupancyPercentage);
        Assert.True(loc.IsFull);

        // Act 3: Overflow attempt must throw InvalidOperationException
        var ex = Assert.Throws<InvalidOperationException>(() => loc.AssignRoll(1));
        Assert.Contains("capacity exceeded", ex.Message);
    }

    [Fact]
    public void FabricRoll_WithShadeAndBarcode_TracksLifecycleAndIssuance_WHS02()
    {
        // Arrange: Roll with Shade A and shrinkage parameters
        var roll = FabricRoll.Create(
            "R-00891",
            Guid.NewGuid(),
            "DYE-TW-2026-01",
            260m,
            58m,
            102.5m,
            "WH1-A03-R02-T01-B01",
            _defaultCompanyId,
            "SUP-APEX-01",
            null,
            FabricGrade.GradeA,
            "ROL-2026-00891",
            null,
            "A",
            -2.5m,
            -1.2m,
            "WH1-A03-R02-T01-B01"
        );

        // Assert setup
        Assert.Equal("ROL-2026-00891", roll.RollBarcode);
        Assert.Equal("A", roll.ShadeGroup);
        Assert.Equal(-2.5m, roll.ShrinkageLengthPct);
        Assert.Equal(RollStatus.Available, roll.Status);

        // Act: Issue 90 yards
        roll.IssueYards(90m);
        Assert.Equal(12.5m, roll.AvailableLengthYards);
        Assert.Equal(RollStatus.InCutting, roll.Status);

        // Act: Consume remaining 12.5 yards
        roll.IssueYards(12.5m);
        Assert.Equal(0m, roll.AvailableLengthYards);
        Assert.Equal(RollStatus.Consumed, roll.Status);
    }

    [Fact]
    public void CustomsBondLedger_CreditAndDebit_MaintainsUPBalance_WHS03()
    {
        // Arrange: Credit raw materials under UP-2026-BOND-8891
        var credit = CustomsBondLedger.RecordCredit(
            "BOND-REG-2026-001",
            "UP-2026-BOND-8891",
            "GRN",
            "GRN-2026-0210",
            "FAB-TW-01",
            "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)",
            36750m,
            0m,
            22050m,
            _defaultCompanyId
        );

        Assert.Equal(36750m, credit.ClosingBalance);
        Assert.Equal(BondTransactionType.ImportCredit, credit.TransactionType);

        // Act: Debit 406 yards for cutting issue
        var debit = CustomsBondLedger.RecordDebit(
            "BOND-REG-2026-001",
            "UP-2026-BOND-8891",
            "STORE_ISSUE_NOTE",
            "SIN-2026-0045",
            "FAB-TW-01",
            "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)",
            406m,
            credit.ClosingBalance,
            _defaultCompanyId
        );

        Assert.Equal(36750m, debit.OpeningBalance);
        Assert.Equal(36344m, debit.ClosingBalance);
        Assert.Equal(BondTransactionType.FloorIssueDebit, debit.TransactionType);

        // Assert: Attempting to debit more than available UP balance must throw statutory violation
        var ex = Assert.Throws<InvalidOperationException>(() =>
            CustomsBondLedger.RecordDebit(
                "BOND-REG-2026-001",
                "UP-2026-BOND-8891",
                "STORE_ISSUE_NOTE",
                "SIN-FAIL",
                "FAB-TW-01",
                "98/2 Cotton Spandex Twill",
                40000m,
                debit.ClosingBalance,
                _defaultCompanyId
            )
        );

        Assert.Contains("STATUTORY BOND VIOLATION", ex.Message);
        Assert.Contains("exceeds available Utilization Permit balance", ex.Message);
    }

    [Fact]
    public void StoreIssueNote_WithRemnantReturn_CalculatesTotalsCorrectly_WHS04()
    {
        // Arrange
        var sin = new StoreIssueNote
        {
            IssueNumber = "SIN-2026-0045",
            TargetDepartment = "Cutting Department (Table 01)",
            StyleNumber = "ST-2026-CHINO-01",
            BuyerOrderNumber = "ORD-2026-001",
            RequestedBy = "Mahfuz Rahman",
            CompanyId = _defaultCompanyId
        };

        // Act: Add 2 items: 1 full roll issued, 1 partial roll with 15 yds remnant returned
        sin.AddItem(Guid.NewGuid(), "ROL-2026-00891", "A", 100m, 100m, 0m);
        sin.AddItem(Guid.NewGuid(), "ROL-2026-00892", "A", 120m, 105m, 15m, "ROL-2026-00892-REM", "WH1-A01-R01-T01-REM");

        // Assert
        Assert.Equal(205m, sin.TotalIssuedYards);
        Assert.Equal(15m, sin.TotalRemnantYards);
        Assert.Equal(StoreIssueStatus.Draft, sin.Status);

        // Act: Approve and issue to floor
        sin.ApproveAndIssue("Enamul Hoque (Warehouse In-Charge)");
        Assert.Equal(StoreIssueStatus.IssuedToFloor, sin.Status);
        Assert.Equal("Enamul Hoque (Warehouse In-Charge)", sin.IssuedBy);
    }

    [Fact]
    public void InventoryCycleCount_VarianceReconciliation_AuthorizesAdjustment_WHS05()
    {
        // Arrange
        var cycleCount = new InventoryCycleCount
        {
            CycleCountNumber = "CC-2026-M10",
            AuditorName = "Mahmud Hasan",
            LocationZone = "RawMaterialFabric",
            CompanyId = _defaultCompanyId
        };

        // Act: Add 2 lines (1 exact match, 1 slight variance)
        cycleCount.AddLine("FAB-TW-01", "ROL-2026-00891", "WH1-A03-R02-T01-B01", 102.5m, 102.5m);
        cycleCount.AddLine("FAB-TW-01", "ROL-2026-00892", "WH1-A03-R02-T01-B02", 100.0m, 99.8m, "EndBitWastage");

        var lineWithVariance = cycleCount.Lines.First(l => l.RollBarcode == "ROL-2026-00892");
        Assert.Equal(-0.2m, lineWithVariance.VarianceQuantity);
        Assert.Equal(-0.20m, lineWithVariance.VariancePercentage);
        Assert.False(lineWithVariance.IsAdjusted);

        // Act: Authorize adjustment
        cycleCount.AuthorizeAdjustment("S. K. Anwar (DGM Supply Chain)");
        Assert.Equal("Approved_Adjusted", cycleCount.Status);
        Assert.Equal("S. K. Anwar (DGM Supply Chain)", cycleCount.AuthorizedBy);
        Assert.True(lineWithVariance.IsAdjusted);
    }
}
