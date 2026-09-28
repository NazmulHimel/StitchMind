namespace GarmentsERP.Reporting.Application.Models;

public class MasterSalarySheetReportModel
{
    public string FactoryName { get; set; } = "StitchMind Garments Manufacturing Ltd.";
    public string FactoryAddress { get; set; } = "Plot 142-146, Gazipur Industrial Area, Dhaka, Bangladesh";
    public string SheetNumber { get; set; } = "PAY-2026-10";
    public int PayMonth { get; set; } = 10;
    public int PayYear { get; set; } = 2026;
    public string PeriodText { get; set; } = "October 2026 / অক্টোবর ২০২৬";

    public int TotalHeadcount { get; set; } = 1250;
    public int TotalPieceRateWorkers { get; set; } = 1100;
    public int TotalStaffWorkers { get; set; } = 150;

    public decimal TotalBaseOrPieceRateEarnings { get; set; } = 108420m;
    public decimal TotalOvertimeAmount { get; set; } = 24780m;
    public decimal TotalAttendanceBonus { get; set; } = 11200m;
    public decimal TotalGrossEarnings { get; set; } = 144400m;
    public decimal TotalDeductions { get; set; } = 1900m;
    public decimal TotalNetDisbursement { get; set; } = 142500m;

    public string ApprovedBy { get; set; } = "Director of Human Resources & Social Compliance";
    public DateTime? ApprovedAt { get; set; } = DateTime.UtcNow;

    public List<MasterSalaryRowModel> Rows { get; set; } = new();
}

public class MasterSalaryRowModel
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string PayStream { get; set; } = string.Empty;
    public decimal BasicOrPieceEarnings { get; set; }
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeAmount { get; set; }
    public decimal AttendanceBonus { get; set; }
    public decimal Allowances { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal NetPayable { get; set; }
    public string PaymentChannel { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
}

public class SocialComplianceOtAuditReportModel
{
    public string FactoryName { get; set; } = "StitchMind Garments Manufacturing Ltd.";
    public string AuditStandard { get; set; } = "BSCI / Sedex SMETA 4-Pillar / ILO Convention 1";
    public int AuditMonth { get; set; } = 10;
    public int AuditYear { get; set; } = 2026;
    public string AuditPeriodText { get; set; } = "October 2026 Audit Cycle";
    public int TotalWorkersAudited { get; set; } = 1250;
    public int CompliantWorkersCount { get; set; } = 1242;
    public int FlaggedWorkersCount { get; set; } = 8;
    public decimal FactoryComplianceRatePct { get; set; } = 99.36m;
    public decimal MaxAllowedDailyOtHours { get; set; } = 2.0m;
    public decimal MaxAllowedWeeklyOtHours { get; set; } = 12.0m;

    public List<OtAuditViolationRowModel> FlaggedWorkers { get; set; } = new();
}

public class OtAuditViolationRowModel
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string AssignedLine { get; set; } = string.Empty;
    public decimal MonthlyOtHoursRecorded { get; set; }
    public decimal PeakDailyOtHours { get; set; }
    public string ViolationSeverity { get; set; } = "Moderate"; // Low, Moderate, High
    public string CorrectiveActionTaken { get; set; } = "Supervisor notified; shift capped at 2.0 hrs/day";
}
