namespace GarmentsERP.Reporting.Application.Models;

public class BilingualPaySlipReportModel
{
    public string FactoryName { get; set; } = "StitchMind Garments Manufacturing Ltd.";
    public string FactoryAddress { get; set; } = "Plot 142-146, Gazipur Industrial Area, Dhaka, Bangladesh";
    public string FactoryLicense { get; set; } = "DIFE/REG/2026/GZ-0914";

    public string PaySlipNumber { get; set; } = string.Empty; // e.g. SLIP-202610-1042
    public int PayMonth { get; set; } = 10;
    public int PayYear { get; set; } = 2026;
    public string MonthYearText { get; set; } = "October 2026 / অক্টোবর ২০২৬";

    // Employee Bio
    public string EmployeeCode { get; set; } = "EMP-2026-1042";
    public string FullNameEnglish { get; set; } = "Md. Rafiqul Islam";
    public string FullNameBangla { get; set; } = "মোঃ রফিকুল ইসলাম";
    public string DepartmentEnglish { get; set; } = "Sewing Section";
    public string DepartmentBangla { get; set; } = "সিলিং বিভাগ";
    public string DesignationEnglish { get; set; } = "Senior Sewing Operator";
    public string DesignationBangla { get; set; } = "সিনিয়র অপারেটর";
    public string AssignedLine { get; set; } = "Line 04 (Station 22)";
    public string BiometricBadgeNumber { get; set; } = "BIO-BADGE-1042";
    public string NationalId { get; set; } = "19951234567890123";
    public DateTime DateOfJoining { get; set; } = new DateTime(2023, 1, 10);
    public string PayStream { get; set; } = "FLOOR_PIECE_RATE / উৎপাদনভিত্তিক";

    // Attendance Summary
    public int TotalWorkingDays { get; set; } = 26;
    public int DaysPresent { get; set; } = 26;
    public int DaysAbsent { get; set; } = 0;
    public int DaysLate { get; set; } = 0;

    // Production Piece-Rate
    public int CompletedGarmentUnits { get; set; } = 2400;
    public decimal PieceRatePerUnit { get; set; } = 0.0700m;
    public decimal PieceRateEarnings { get; set; } = 168.00m;

    // Fixed & Overtime Earnings
    public decimal BasicSalary { get; set; } = 16000m;
    public decimal Allowances { get; set; } = 0m;
    public decimal OvertimeHours { get; set; } = 26.0m;
    public decimal OvertimeRatePerHour { get; set; } = 1.45m;
    public decimal OvertimeAmount { get; set; } = 37.70m;
    public decimal AttendanceBonus { get; set; } = 10.00m;
    public decimal GrossEarnings { get; set; } = 215.70m;

    // Deductions
    public decimal AbsentDeduction { get; set; } = 0m;
    public decimal LatePenaltyDeduction { get; set; } = 0m;
    public decimal TaxDeduction { get; set; } = 0m;
    public decimal AdvanceDeduction { get; set; } = 0m;
    public decimal TotalDeductions { get; set; } = 0m;

    // Net Payout
    public decimal NetPayable { get; set; } = 215.70m;
    public string DisbursementChannel { get; set; } = "bKash Direct Wallet";
    public string MfsOrAccountNumber { get; set; } = "01722445566";
    public string PaymentReference { get; set; } = "BKASH-SAL-202610-1042";
    public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
}
