using GarmentsERP.HR.Domain.Entities;

namespace GarmentsERP.HR.Application.DTOs;

public record EmployeeDto(
    Guid Id,
    string EmployeeCode,
    string FullName,
    string Email,
    string PhoneNumber,
    string Department,
    string Designation,
    string? AssignedLine,
    decimal BasicSalary,
    decimal GrossSalary,
    DateTime JoiningDate,
    string StatusName,
    string? NationalId,
    string? MobileBankingNumber,
    string? EmergencyContact
);

public record AttendanceRecordDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    string? AssignedLine,
    DateTime Date,
    string? CheckInTime,
    string? CheckOutTime,
    decimal OvertimeHours,
    string StatusName,
    string? ShiftName,
    string? Remarks
);

public record LeaveRequestDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    string LeaveTypeName,
    DateTime StartDate,
    DateTime EndDate,
    int TotalDays,
    string Reason,
    string StatusName,
    string? ApprovedBy,
    DateTime? ApprovedAt
);

public record PayrollRecordDto(
    Guid Id,
    Guid EmployeeId,
    string EmployeeCode,
    string EmployeeName,
    string Department,
    string Designation,
    int PayMonth,
    int PayYear,
    decimal BasicSalary,
    decimal Allowances,
    decimal OvertimeHours,
    decimal OvertimeAmount,
    decimal AbsentDeduction,
    decimal TaxDeduction,
    decimal AdvanceDeduction,
    decimal TotalDeductions,
    decimal NetPayable,
    string StatusName,
    DateTime? DisbursedDate,
    string? PaymentReference
);
