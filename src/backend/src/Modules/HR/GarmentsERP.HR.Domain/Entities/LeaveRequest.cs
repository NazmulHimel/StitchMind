using GarmentsERP.Shared.Domain;

namespace GarmentsERP.HR.Domain.Entities;

public enum LeaveType
{
    Casual = 1,
    Sick = 2,
    Annual = 3,
    Maternity = 4,
    Unpaid = 5
}

public enum LeaveStatus
{
    Pending = 1,
    Approved = 2,
    Rejected = 3
}

public class LeaveRequest : BaseEntity
{
    public Guid EmployeeId { get; set; }
    public Employee Employee { get; set; } = null!;
    public LeaveType Type { get; set; } = LeaveType.Casual;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int TotalDays { get; set; }
    public string Reason { get; set; } = string.Empty;
    public LeaveStatus Status { get; set; } = LeaveStatus.Pending;
    public string? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string? RejectionReason { get; set; }
}
