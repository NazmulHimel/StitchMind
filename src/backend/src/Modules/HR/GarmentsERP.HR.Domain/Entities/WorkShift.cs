using GarmentsERP.Shared.Domain;

namespace GarmentsERP.HR.Domain.Entities;

public class WorkShift : BaseEntity
{
    public string ShiftCode { get; set; } = "SHIFT-M"; // SHIFT-M, SHIFT-E, SHIFT-N
    public string ShiftName { get; set; } = "Morning Shift (08:00 - 17:00)";
    public TimeSpan StartTime { get; set; } = new TimeSpan(8, 0, 0);
    public TimeSpan EndTime { get; set; } = new TimeSpan(17, 0, 0);
    public int GracePeriodMinutes { get; set; } = 15; // Punch between 08:00 and 08:15 is not penalized
    public int HalfDayThresholdMinutes { get; set; } = 60; // Punch after 09:00 counts as Half-Day
    public bool IsNightShift { get; set; } = false;
    public decimal ShiftAllowanceDaily { get; set; } = 0m;
    public bool IsActive { get; set; } = true;
}

public class ShiftRoster : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public Guid WorkShiftId { get; set; }
    public WorkShift WorkShift { get; set; } = null!;

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }
    public string AssignedLineOrSection { get; set; } = "Sewing Floor Line 01";
    public string? Remarks { get; set; }
}

public class LateAttendancePenalty : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;

    public int PayMonth { get; set; }
    public int PayYear { get; set; }
    public int TotalLateDays { get; set; }
    public int PenaltyDeductionDays { get; set; } // Statutory Bangladesh Labor standard: 3 late punches = 1 day wage deduction
    public decimal DeductionAmount { get; set; }
    public bool IsAppliedToPayroll { get; set; } = false;
}
