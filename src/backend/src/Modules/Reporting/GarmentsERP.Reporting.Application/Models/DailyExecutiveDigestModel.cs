namespace GarmentsERP.Reporting.Application.Models;

public class DailyExecutiveDigestModel
{
    public string FactoryName { get; set; } = "StitchMind Unit 01 (Gazipur Industrial Zone)";
    public string FacilityLocation { get; set; } = "Plot 14-18, Sector 3, Gazipur, Bangladesh";
    public DateTime ReportDate { get; set; } = DateTime.Today;
    public DateTime GeneratedAtUtc { get; set; } = DateTime.UtcNow;
    public string ExecutiveDigestCode { get; set; } = $"DIGEST-{DateTime.Today:yyyyMMdd}-8PM";

    // 1. Executive Summary Strip
    public int TotalWorkforcePresent { get; set; } = 1215;
    public int TotalWorkforceHeadcount { get; set; } = 1250;
    public decimal AttendanceRatePct => TotalWorkforceHeadcount > 0 
        ? Math.Round(((decimal)TotalWorkforcePresent / TotalWorkforceHeadcount) * 100m, 1) 
        : 97.2m;
    public decimal OverallFactoryEfficiencyPct { get; set; } = 86.4m;
    public decimal RunningDhuPct { get; set; } = 1.2m;
    public decimal OnTimeDeliveryRatePct { get; set; } = 99.4m;

    // 2. CAD & Cutting Department KPIs
    public int TotalFabricRollsInspected { get; set; } = 48;
    public decimal FabricInspectedYards { get; set; } = 18450.0m;
    public int Fabric4PointPassCount { get; set; } = 47;
    public int FabricQuarantineCount { get; set; } = 1;
    public int TotalLaySheetsProcessed { get; set; } = 14;
    public decimal TotalPliesLaid { get; set; } = 840;
    public int TotalGarmentUnitsCut { get; set; } = 16800;
    public decimal MarkerEfficiencyPct { get; set; } = 88.6m;
    public int TotalBundlesGenerated { get; set; } = 420;

    // 3. Sewing Floor Performance & Line Balancing
    public int TotalSewingLinesActive { get; set; } = 12;
    public int TargetGarmentUnitsToday { get; set; } = 15000;
    public int ActualGarmentUnitsSewnToday { get; set; } = 14200;
    public decimal SewingTargetAchievementPct => TargetGarmentUnitsToday > 0 
        ? Math.Round(((decimal)ActualGarmentUnitsSewnToday / TargetGarmentUnitsToday) * 100m, 1) 
        : 94.7m;
    public string TopPerformingLine { get; set; } = "Line 04 (Chino Trouser) — 98.7% Eff";
    public string LowestPerformingLine { get; set; } = "Line 08 (Fleece Hoodie) — 78.2% Eff (Bottle-neck: Hood Cord Hemming)";
    public List<LineEfficiencyDigestItem> SewingLines { get; set; } = [];

    // 4. Quality Assurance & Defect Distribution
    public int TotalInlineInspectionsPcs { get; set; } = 3800;
    public int InlineDefectsDetected { get; set; } = 46;
    public int FinishingInspectedCartons { get; set; } = 320;
    public decimal FinishingAqlPassRatePct { get; set; } = 99.2m;
    public int CriticalDefects { get; set; } = 0;
    public int MajorDefects { get; set; } = 14;
    public int MinorDefects { get; set; } = 32;

    // 5. Commercial, Finance & Ex-Factory Dispatch
    public decimal TodayExFactoryShipmentValueUsd { get; set; } = 84500.00m;
    public int TodayGarmentsDispatchedPcs { get; set; } = 6400;
    public int TodayCartonsDispatched { get; set; } = 280;
    public int ActiveExportMasterLcsCount { get; set; } = 8;
    public decimal TotalActiveExportLcValueUsd { get; set; } = 1240000.00m;
    public int LcMaturingWithin7DaysCount { get; set; } = 1;
    public decimal ThreeWayMatchDiscrepanciesUsd { get; set; } = 0.00m;

    // 6. HR, Occupational Health & Social Compliance
    public decimal TotalOvertimeHoursLogged { get; set; } = 1420.0m;
    public decimal AverageOvertimePerOperatorHours => TotalWorkforcePresent > 0 
        ? Math.Round(TotalOvertimeHoursLogged / TotalWorkforcePresent, 1) 
        : 1.2m;
    public int ClinicConsultationsCount { get; set; } = 8;
    public int ClinicFitForFloorDutyCount { get; set; } = 8;
    public int ClinicSentHomeCount { get; set; } = 0;
    public int OpenWorkerGrievancesCount { get; set; } = 0;
    public int UnderageViolationsCount { get; set; } = 0; // AC-07 Child Labor Block
}

public class LineEfficiencyDigestItem
{
    public string LineNumber { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string StyleNumber { get; set; } = string.Empty;
    public int PlannedHourlyTarget { get; set; }
    public int ActualHourlyAchieved { get; set; }
    public decimal LineEfficiencyPct { get; set; }
    public decimal DhuPct { get; set; }
    public string Status { get; set; } = "Normal";
}
