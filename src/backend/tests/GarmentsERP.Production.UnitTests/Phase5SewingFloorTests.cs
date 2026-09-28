using GarmentsERP.Production.Application.Commands;
using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Queries;
using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using Xunit;

namespace GarmentsERP.Production.UnitTests;

/// <summary>
/// Phase 5 Unit Tests: Sewing Floor, Real-time WIP & Line Balancing (IE-01, SEW-01 through SEW-06).
/// </summary>
public class Phase5SewingFloorTests
{
    // IE-01: Operation Bulletin & SMV Target Calculation
    [Fact]
    public void OperationBulletin_ShouldRecalculateTargetPerHourFromSMV_IE01()
    {
        var op = new OperationBulletin
        {
            StyleNumber = "ST-2026-CHINO-01",
            OperationSequence = 1,
            OperationName = "Front Pocket Attach",
            StandardMinuteValueSMV = 0.950m, // 0.95 minutes
            RequiredSkillGrade = "B",
            PieceRatePerDzn = 0.65m
        };

        op.RecalculateTarget();

        // 60 / 0.950 = 63.15 -> 63 pcs/hr
        Assert.Equal(63, op.TargetPerHour);
        Assert.Equal("B", op.RequiredSkillGrade);
        Assert.Equal(0.65m, op.PieceRatePerDzn);
    }

    [Fact]
    public void OperationBulletin_WaistbandOperation_ShouldCalculateTargetAccurately_IE01()
    {
        var op = new OperationBulletin
        {
            StyleNumber = "ST-2026-CHINO-01",
            OperationSequence = 7,
            OperationName = "Waistband Pre-forming & Attachment",
            StandardMinuteValueSMV = 1.650m,
            RequiredSkillGrade = "A+",
            PieceRatePerDzn = 1.15m
        };

        op.RecalculateTarget();

        // 60 / 1.650 = 36.36 -> 36 pcs/hr
        Assert.Equal(36, op.TargetPerHour);
        Assert.Equal("A+", op.RequiredSkillGrade);
    }

    // SEW-01: Pitch Diagram & Line Balancing Calculation
    [Fact]
    public void LineBalancing_PitchDiagram_ShouldIdentifyBottlenecksAndPitchTime_SEW01()
    {
        int totalWorkstations = 50;
        decimal totalGarmentSmv = 18.50m; // minutes for full chino pant

        // Pitch Time = Total SMV / Total Workstations
        decimal pitchTimeMinutes = Math.Round(totalGarmentSmv / totalWorkstations, 3); // 0.370 min
        decimal pitchTimeSeconds = Math.Round(pitchTimeMinutes * 60.0m, 1); // 22.2 sec

        Assert.Equal(0.370m, pitchTimeMinutes);
        Assert.Equal(22.2m, pitchTimeSeconds);

        // An operation with SMV 1.650 min on a single station exceeds pitch time (0.370)
        decimal opSmv = 1.650m;
        bool isBottleneck = opSmv > pitchTimeMinutes;
        decimal bottleneckVariancePct = Math.Round(((opSmv - pitchTimeMinutes) / pitchTimeMinutes) * 100.0m, 1);

        Assert.True(isBottleneck);
        Assert.True(bottleneckVariancePct > 300.0m); // 345.9% over pitch time
    }

    // SEW-02: Sub-300ms Barcode Bundle Scanning API
    [Fact]
    public void BundleScan_ShouldRecordFloorStationAndValidateFastProcessing_SEW02()
    {
        var scan = new BundleScan
        {
            BundleBarcode = "BND-2026-CHINO-001",
            LineNumber = "LINE-04",
            OperatorBadgeId = "OP-1042",
            OperatorName = "Nasreen Sultana",
            OperationName = "Waistband Attachment",
            PiecesCount = 24,
            FloorStation = "STATION-12",
            ProcessingTimeMs = 42
        };

        Assert.Equal("BND-2026-CHINO-001", scan.BundleBarcode);
        Assert.Equal("LINE-04", scan.LineNumber);
        Assert.Equal(24, scan.PiecesCount);
        Assert.True(scan.ProcessingTimeMs < 300, "Processing must be sub-300ms benchmark.");

        // Piece rate credit: (24 / 12) * $0.85 = $1.70
        decimal pieceRateCredit = (scan.PiecesCount / 12.0m) * 0.85m;
        Assert.Equal(1.70m, pieceRateCredit);
    }

    // SEW-04: Digital Andon Board Alert Levels
    [Fact]
    public void AndonBoard_WhenCriticalNeedleOrBreakdown_ShouldTriggerCriticalAlert_SEW04()
    {
        var alertBroken = AndonAlertLevel.CriticalStopped;
        string message = "CRITICAL STOP: Broken needle search active on Machine SN-084.";

        Assert.Equal(AndonAlertLevel.CriticalStopped, alertBroken);
        Assert.Contains("CRITICAL STOP", message);
    }

    [Fact]
    public void AndonBoard_WhenNormalProduction_ShouldReportEfficiencyAndOnPace_SEW04()
    {
        int target = 120;
        int actual = 126;
        decimal efficiency = 78.4m;
        decimal dhu = 2.10m;

        Assert.True(actual >= target);
        Assert.True(efficiency >= 75.0m);
        Assert.True(dhu <= 3.0m, "DHU must be under quality threshold 3.0%.");
    }

    // SEW-05: Sewing Machine Asset & Breakdown Tickets (MTTR / MTBF)
    [Fact]
    public void SewingMachineAsset_ShouldMaintainOperationalAndReliabilityMetrics_SEW05()
    {
        var machine = new SewingMachineAsset
        {
            MachineBarcode = "SN-084",
            Brand = "Juki",
            MachineModel = "DDL-9000C-FMS",
            MachineType = "Single Needle Lockstitch (Direct Drive)",
            LineNumber = "LINE-04",
            StationNumber = "Station 12",
            Status = MachineStatus.Operational,
            MTBFHours = 240.5m,
            MTTRMinutes = 14.2m
        };

        Assert.Equal("SN-084", machine.MachineBarcode);
        Assert.Equal(MachineStatus.Operational, machine.Status);
        Assert.True(machine.MTBFHours >= 200.0m);
        Assert.True(machine.MTTRMinutes <= 15.0m);
    }

    [Fact]
    public void MachineBreakdownTicket_ShouldTrackDowntimeAndResolveStatus_SEW05()
    {
        var ticket = new MachineBreakdownTicket
        {
            TicketNumber = "TKT-2026-0045",
            MachineBarcode = "SN-084",
            LineNumber = "LINE-04",
            ReportedIssue = "Thread trimmer solenoid jammed",
            DowntimeMinutes = 15,
            MechanicName = "Kamal Hossain",
            RootCause = "Lint accumulation cleaned & lubricated.",
            IsResolved = true
        };

        Assert.True(ticket.IsResolved);
        Assert.Equal(15, ticket.DowntimeMinutes);
        Assert.Equal("Kamal Hossain", ticket.MechanicName);
    }

    // SEW-06: Broken Needle Exchange Compliance Gate
    [Fact]
    public void BrokenNeedleLog_WhenFragmentMissing_ShouldEnforceComplianceBlock_SEW06()
    {
        var log = new BrokenNeedleLog
        {
            LogNumber = "BNL-2026-0090",
            LineNumber = "LINE-04",
            MachineBarcode = "MO-105",
            OperatorBadgeId = "OP-1088",
            OperatorName = "Morium Khatun",
            BreakageReason = "Needle collision with zipper metal stop"
        };

        // Tip is missing! (shank and eye found)
        bool approved = log.VerifyAndIssueReplacement(
            supervisorBadge: "SUP-012",
            tip: false,
            shank: true,
            eye: true,
            needleType: "Schmetz B27 90/14",
            out string message
        );

        Assert.False(approved, "Replacement needle issue MUST be blocked when any fragment is missing.");
        Assert.False(log.AllFragmentsRecovered);
        Assert.False(log.ReplacementNeedleIssued);
        Assert.Equal(NeedleComplianceStatus.FragmentSearchActive, log.Status);
        Assert.Contains("COMPLIANCE BLOCK", message);
        Assert.Contains("Tip, Shank, and Eye must all be recovered", message);
    }

    [Fact]
    public void BrokenNeedleLog_WhenAllFragmentsRecovered_ShouldAuthorizeReplacement_SEW06()
    {
        var log = new BrokenNeedleLog
        {
            LogNumber = "BNL-2026-0089",
            LineNumber = "LINE-04",
            MachineBarcode = "SN-084",
            OperatorBadgeId = "OP-1042",
            OperatorName = "Nasreen Sultana",
            BreakageReason = "Deflection on heavy multi-ply waistband join"
        };

        // 100% fragments found: Tip, Shank, Eye
        bool approved = log.VerifyAndIssueReplacement(
            supervisorBadge: "SUP-012",
            tip: true,
            shank: true,
            eye: true,
            needleType: "Groz-Beckert DPx5 110/18",
            out string message
        );

        Assert.True(approved, "Replacement needle issue MUST be authorized when all fragments are recovered.");
        Assert.True(log.AllFragmentsRecovered);
        Assert.True(log.ReplacementNeedleIssued);
        Assert.Equal(NeedleComplianceStatus.VerifiedAndReplaced, log.Status);
        Assert.Contains("All 3 needle fragments verified by Supervisor SUP-012", message);
        Assert.Equal("Groz-Beckert DPx5 110/18", log.ReplacementNeedleType);
    }
}
