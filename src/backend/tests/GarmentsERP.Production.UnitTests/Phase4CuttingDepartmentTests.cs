using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using Xunit;

namespace GarmentsERP.Production.UnitTests;

/// <summary>
/// Phase 4 Unit Tests: Cutting Department & CAD Marker Optimization (CUT-01 through CUT-06).
/// </summary>
public class Phase4CuttingDepartmentTests
{
    // CUT-01: CAD Marker Ingestion & Consumption Benchmark
    [Fact]
    public void CADMarker_ShouldCalculatePlannedConsumptionAccurately_CUT01()
    {
        var marker = new CADMarker
        {
            MarkerName = "MKR-CHINO-SZ32-34-A",
            StyleNumber = "ST-2026-CHINO-01",
            CADSoftware = "Gerber AccuMark v16",
            CuttableWidthInches = 58.0m,
            MarkerLengthYards = 8.42m,
            GarmentsPerMarker = 4,
            MarkerEfficiencyPct = 87.20m
        };

        marker.RecalculateConsumption();

        // 8.42 Yds / 4 Garments = 2.1050 Yds/pc
        Assert.Equal(2.1050m, marker.PlannedConsumptionPerPcYds);
        Assert.True(marker.MeetsEfficiencyBenchmark(85.0m));
    }

    // CUT-02: Mandatory Relaxation Time-Lock Gate
    [Fact]
    public void RelaxationSession_WhenUnder24Hours_ShouldBlockSpreading_CUT02()
    {
        var rollId = Guid.NewGuid();
        // Unrolled 4 hours ago (20 hours remaining)
        var unrollTime = DateTime.UtcNow.AddHours(-4);
        var session = RelaxationSession.StartSession(
            rollId, "ROL-2026-00894", "LOT-TW-2026-01", "98/2 Cotton Spandex Twill",
            24, 24.8m, 64.0m, "Cutter-Master-02", unrollTime);

        Assert.False(session.IsUnlocked());
        var remaining = session.GetRemainingTime();
        Assert.True(remaining.TotalHours > 19 && remaining.TotalHours <= 20);

        var authorized = session.TryAuthorizeSpreading(DateTime.UtcNow, out var reason);
        Assert.False(authorized);
        Assert.Contains("RELAXATION LOCK ACTIVE", reason);
        Assert.Contains("ROL-2026-00894", reason);
    }

    [Fact]
    public void RelaxationSession_When24HoursElapsed_ShouldAuthorizeSpreading_CUT02()
    {
        var rollId = Guid.NewGuid();
        // Unrolled 26 hours ago (Elapsed)
        var unrollTime = DateTime.UtcNow.AddHours(-26);
        var session = RelaxationSession.StartSession(
            rollId, "ROL-2026-00891", "LOT-TW-2026-01", "98/2 Cotton Spandex Twill",
            24, 24.5m, 65.0m, "Cutter-Master-01", unrollTime);

        Assert.True(session.IsUnlocked());
        Assert.Equal(TimeSpan.Zero, session.GetRemainingTime());

        var authorized = session.TryAuthorizeSpreading(DateTime.UtcNow, out var reason);
        Assert.True(authorized);
        Assert.Equal(RelaxationStatus.SpreadingAuthorized, session.Status);
        Assert.Contains("authorized for table spreading", reason);
    }

    // CUT-03: Lay Planning & Planned vs Actual Wastage Calculation
    [Fact]
    public void CuttingOrder_LayPlanningAndWastage_ShouldReconcileAccurately_CUT03()
    {
        var order = new CuttingOrder
        {
            CuttingOrderNumber = "CUT-2026-0001",
            StyleNumber = "ST-2026-CHINO-01",
            TotalPlies = 60,
            MarkerLength = 8.42m,
            GarmentsPerMarker = 4,
            ActualFabricConsumedYds = 508.50m,
            EndLossRemnantYds = 3.50m,
            ShadeGroup = "Shade A",
            TableNumber = "Table-03"
        };

        order.RecalculateLayPlanning();

        // Total planned garments = 60 plies * 4 garments = 240 pcs
        Assert.Equal(240, order.TotalGarmentsPlanned);

        // Planned total = 60 * 8.42 = 505.20 Yds
        // Diff = 508.50 - 505.20 = 3.30 Yds
        // Wastage variance % = (3.30 / 505.20) * 100 = 0.65%
        Assert.Equal(0.65m, order.CuttingWastageVariancePct);
        Assert.True(order.CuttingWastageVariancePct <= 2.0m, "Cutting wastage must be within standard 2.0% limit");
    }

    // CUT-04: Serialized Component Bundle & Barcode Generation
    [Fact]
    public void CutBundle_ShouldGenerateSerializedBarcodeAndZpl_CUT04()
    {
        var bundle = new CutBundle
        {
            BundleNumber = "BND-2026-0001-01",
            Barcode = "BND-2026-0001-01",
            StyleNumber = "ST-2026-CHINO-01",
            Size = "32/32",
            Color = "Khaki Beige",
            ShadeGroup = "A",
            ComponentName = "FRONT PANEL",
            Quantity = 24,
            SerialStart = 1,
            SerialEnd = 24,
            BundleSequence = 1,
            TotalBundlesInOrder = 10,
            CurrentFloorStation = "CUTTING_OUTPUT"
        };

        Assert.Equal("BND-2026-0001-01", bundle.Barcode);
        Assert.Equal(24, (bundle.SerialEnd - bundle.SerialStart) + 1);

        var zpl = bundle.GenerateZplTicket("Nordic Fashion", "CUT-2026-0001");
        Assert.Contains("^XA", zpl);
        Assert.Contains("STYLE: ST-2026-CHINO-01", zpl);
        Assert.Contains("COMPONENT: FRONT PANEL", zpl);
        Assert.Contains("BND-2026-0001-01", zpl);
        Assert.Contains("^XZ", zpl);
    }

    // CUT-05: Fusing Machine Parameter Logging
    [Fact]
    public void FusingMachineLog_Validation_ShouldEnforceOptimalParameters_CUT05()
    {
        var compliantLog = new FusingMachineLog
        {
            CutOrderNumber = "CUT-2026-0001",
            MachineId = "FUS-02",
            ComponentName = "Waistband",
            TemperatureC = 150.0m,
            PressureBar = 3.5m,
            SpeedSeconds = 12.0m
        };

        var isValid = compliantLog.ValidateFusingParameters(out var validMsg);
        Assert.True(isValid);
        Assert.True(compliantLog.IsApproved);

        var outOfSpecLog = new FusingMachineLog
        {
            CutOrderNumber = "CUT-2026-0001",
            MachineId = "FUS-02",
            ComponentName = "Collar",
            TemperatureC = 120.0m, // Below 135°C
            PressureBar = 3.5m,
            SpeedSeconds = 12.0m
        };

        var isInvalid = outOfSpecLog.ValidateFusingParameters(out var invalidMsg);
        Assert.False(isInvalid);
        Assert.False(outOfSpecLog.IsApproved);
        Assert.Contains("Delamination risk", invalidMsg);
    }

    // CUT-06: Subcontract Cutting Reconciliation
    [Fact]
    public void SubcontractChallan_Reconciliation_ShouldCalculateShortageAndStatus_CUT06()
    {
        var challan = new SubcontractCuttingChallan
        {
            ChallanNumber = "SCC-2026-0012",
            SubcontractorName = "Apex Precision Cutting Mills",
            ExpectedCutPanels = 1440,
            FabricSentYds = 1200.0m
        };

        // Received 1432 good + 4 defective = 1436 accounted for (4 missing shortage = 0.28% <= 1.0%)
        challan.Reconcile(1432, 4, "Tariqul Islam", "Reconciliation normal");

        Assert.Equal(4, challan.ShortageCount);
        Assert.Equal(0.28m, challan.ShortagePercentage);
        Assert.Equal(SubcontractStatus.Reconciled, challan.Status);

        // High shortage test (>1.0%)
        var highShortageChallan = new SubcontractCuttingChallan
        {
            ChallanNumber = "SCC-2026-0013",
            ExpectedCutPanels = 1000
        };
        highShortageChallan.Reconcile(970, 5, "Tariqul Islam", "Shortage investigation required");

        Assert.Equal(25, highShortageChallan.ShortageCount);
        Assert.Equal(2.50m, highShortageChallan.ShortagePercentage);
        Assert.Equal(SubcontractStatus.VarianceUnderReview, highShortageChallan.Status);
    }
}
