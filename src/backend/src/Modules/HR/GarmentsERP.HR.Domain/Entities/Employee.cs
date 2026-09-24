using GarmentsERP.Shared.Domain;

namespace GarmentsERP.HR.Domain.Entities;

public enum EmployeeStatus
{
    Active = 1,
    OnLeave = 2,
    Suspended = 3,
    Terminated = 4
}

public class Employee : BaseEntity
{
    public string EmployeeCode { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty; // Cutting, Sewing, QC, Warehouse, Admin
    public string Designation { get; set; } = string.Empty; // Operator, Helper, Supervisor, Manager
    public string? AssignedLine { get; set; } // e.g. Line 01, Line 02
    public decimal BasicSalary { get; set; }
    public decimal HouseRentAllowance { get; set; }
    public decimal MedicalAllowance { get; set; }
    public decimal ConveyanceAllowance { get; set; }
    public decimal GrossSalary => BasicSalary + HouseRentAllowance + MedicalAllowance + ConveyanceAllowance;
    public DateTime JoiningDate { get; set; }
    public EmployeeStatus Status { get; set; } = EmployeeStatus.Active;
    public string? NationalId { get; set; }
    public string? MobileBankingNumber { get; set; } // e.g. bKash / Nagad / Bank Account
    public string? EmergencyContact { get; set; }

    public ICollection<AttendanceRecord> AttendanceRecords { get; set; } = new List<AttendanceRecord>();
    public ICollection<LeaveRequest> LeaveRequests { get; set; } = new List<LeaveRequest>();
    public ICollection<PayrollRecord> PayrollRecords { get; set; } = new List<PayrollRecord>();
}
