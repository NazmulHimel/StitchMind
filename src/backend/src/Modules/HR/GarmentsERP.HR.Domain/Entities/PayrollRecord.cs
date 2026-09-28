using GarmentsERP.Shared.Domain;
using GarmentsERP.HR.Domain.Enums;

namespace GarmentsERP.HR.Domain.Entities;

public enum PayrollStatus
{
    Draft = 1,
    Approved = 2,
    Disbursed = 3
}

public class PayrollSheet : BaseEntity
{
    public string SheetNumber { get; set; } = string.Empty; // e.g. PAY-2026-10
    public int PayMonth { get; set; }
    public int PayYear { get; set; }
    public int TotalHeadcount { get; set; }
    public int TotalPieceRateWorkers { get; set; }
    public int TotalStaffWorkers { get; set; }

    public decimal TotalBaseOrPieceRateEarnings { get; set; }
    public decimal TotalOvertimeAmount { get; set; }
    public decimal TotalAttendanceBonus { get; set; }
    public decimal TotalGrossEarnings { get; set; }
    public decimal TotalDeductions { get; set; }
    public decimal TotalNetDisbursement { get; set; } // e.g. $142,500.00 USD

    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DisbursedAt { get; set; }

    public ICollection<PayrollRecord> Records { get; set; } = new List<PayrollRecord>();
}

public class PayrollRecord : BaseEntity
{
    public Guid? PayrollSheetId { get; set; }
    public PayrollSheet? PayrollSheet { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int PayMonth { get; set; }
    public int PayYear { get; set; }

    // Dual-Stream Configuration
    public PayStreamType PayStream { get; set; } = PayStreamType.MonthlyStaff;
    public decimal BasicSalary { get; set; }
    public decimal Allowances { get; set; }

    // Piece-Rate Production Earnings (Floor Operators)
    public int CompletedGarmentUnits { get; set; } // e.g. 2,400 garments
    public decimal PieceRatePerUnit { get; set; } // e.g. $0.0700 / pc
    public decimal PieceRateEarnings => Math.Round(CompletedGarmentUnits * PieceRatePerUnit, 2);

    // Statutory Overtime (Formula: OT_Hours * (Basic / 208) * 2)
    public decimal OvertimeHours { get; set; }
    public decimal OvertimeRatePerHour { get; set; }
    public decimal OvertimeAmount => Math.Round(OvertimeHours * OvertimeRatePerHour, 2);

    // Attendance Incentives
    public decimal AttendanceBonus { get; set; } // e.g. $10.00 / ৳1,000 for zero lates/absences

    // Deductions
    public decimal AbsentDeduction { get; set; }
    public decimal LatePenaltyDeduction { get; set; }
    public decimal TaxDeduction { get; set; }
    public decimal AdvanceDeduction { get; set; }
    public decimal TotalDeductions => AbsentDeduction + LatePenaltyDeduction + TaxDeduction + AdvanceDeduction;

    // Gross and Net Computation
    public decimal GrossEarnings => PayStream == PayStreamType.FloorPieceRate
        ? (PieceRateEarnings + OvertimeAmount + AttendanceBonus + Allowances)
        : (BasicSalary + Allowances + OvertimeAmount + AttendanceBonus);

    public decimal NetPayable => GrossEarnings - TotalDeductions;

    // Disbursement Routing
    public PaymentDisbursementChannel DisbursementChannel { get; set; } = PaymentDisbursementChannel.bKash;
    public string? MfsOrAccountNumber { get; set; }
    public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    public DateTime? DisbursedDate { get; set; }
    public string? PaymentReference { get; set; }
}
