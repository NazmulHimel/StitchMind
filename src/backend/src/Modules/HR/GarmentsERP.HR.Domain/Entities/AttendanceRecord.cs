using GarmentsERP.Shared.Domain;

namespace GarmentsERP.HR.Domain.Entities;

public enum AttendanceStatus
{
    Present = 1,
    Late = 2,
    Absent = 3,
    HalfDay = 4,
    OnLeave = 5
}

public class AttendanceRecord : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public DateTime Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public decimal OvertimeHours { get; set; }
    public AttendanceStatus Status { get; set; } = AttendanceStatus.Present;
    public string? ShiftName { get; set; } = "Morning Shift (08:00 - 17:00)";
    public string? BiometricDeviceId { get; set; }
    public string? Remarks { get; set; }
}
