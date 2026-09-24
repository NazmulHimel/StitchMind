using MediatR;
using GarmentsERP.HR.Application.DTOs;
using GarmentsERP.HR.Application.Interfaces;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.HR.Application.Queries;

public record GetEmployeesQuery(string? Department = null) : IRequest<Result<IReadOnlyList<EmployeeDto>>>;
public record GetAttendanceLogsQuery(DateTime? Date = null, Guid? EmployeeId = null) : IRequest<Result<IReadOnlyList<AttendanceRecordDto>>>;
public record GetLeaveRequestsQuery(LeaveStatus? Status = null) : IRequest<Result<IReadOnlyList<LeaveRequestDto>>>;
public record GetPayrollRecordsQuery(int? Month = null, int? Year = null) : IRequest<Result<IReadOnlyList<PayrollRecordDto>>>;

public class HrQueryHandlers :
    IRequestHandler<GetEmployeesQuery, Result<IReadOnlyList<EmployeeDto>>>,
    IRequestHandler<GetAttendanceLogsQuery, Result<IReadOnlyList<AttendanceRecordDto>>>,
    IRequestHandler<GetLeaveRequestsQuery, Result<IReadOnlyList<LeaveRequestDto>>>,
    IRequestHandler<GetPayrollRecordsQuery, Result<IReadOnlyList<PayrollRecordDto>>>
{
    private readonly IHrRepository _repo;

    public HrQueryHandlers(IHrRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<IReadOnlyList<EmployeeDto>>> Handle(GetEmployeesQuery request, CancellationToken ct)
    {
        var employees = await _repo.GetEmployeesAsync(request.Department, ct);
        var dtos = employees.Select(e => new EmployeeDto(
            e.Id,
            e.EmployeeCode,
            e.FullName,
            e.Email,
            e.PhoneNumber,
            e.Department,
            e.Designation,
            e.AssignedLine,
            e.BasicSalary,
            e.GrossSalary,
            e.JoiningDate,
            e.Status.ToString(),
            e.NationalId,
            e.MobileBankingNumber,
            e.EmergencyContact
        )).ToList();

        return Result<IReadOnlyList<EmployeeDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<AttendanceRecordDto>>> Handle(GetAttendanceLogsQuery request, CancellationToken ct)
    {
        var logs = await _repo.GetAttendanceRecordsAsync(request.Date, request.EmployeeId, ct);
        var dtos = logs.Select(l => new AttendanceRecordDto(
            l.Id,
            l.EmployeeId,
            l.Employee.EmployeeCode,
            l.Employee.FullName,
            l.Employee.Department,
            l.Employee.AssignedLine,
            l.Date,
            l.CheckInTime?.ToString(@"hh\:mm"),
            l.CheckOutTime?.ToString(@"hh\:mm"),
            l.OvertimeHours,
            l.Status.ToString(),
            l.ShiftName,
            l.Remarks
        )).ToList();

        return Result<IReadOnlyList<AttendanceRecordDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<LeaveRequestDto>>> Handle(GetLeaveRequestsQuery request, CancellationToken ct)
    {
        var requests = await _repo.GetLeaveRequestsAsync(request.Status, ct);
        var dtos = requests.Select(l => new LeaveRequestDto(
            l.Id,
            l.EmployeeId,
            l.Employee.EmployeeCode,
            l.Employee.FullName,
            l.Employee.Department,
            l.Type.ToString(),
            l.StartDate,
            l.EndDate,
            l.TotalDays,
            l.Reason,
            l.Status.ToString(),
            l.ApprovedBy,
            l.ApprovedAt
        )).ToList();

        return Result<IReadOnlyList<LeaveRequestDto>>.Success(dtos);
    }

    public async Task<Result<IReadOnlyList<PayrollRecordDto>>> Handle(GetPayrollRecordsQuery request, CancellationToken ct)
    {
        var records = await _repo.GetPayrollRecordsAsync(request.Month, request.Year, ct);
        var dtos = records.Select(p => new PayrollRecordDto(
            p.Id,
            p.EmployeeId,
            p.Employee.EmployeeCode,
            p.Employee.FullName,
            p.Employee.Department,
            p.Employee.Designation,
            p.PayMonth,
            p.PayYear,
            p.BasicSalary,
            p.Allowances,
            p.OvertimeHours,
            p.OvertimeAmount,
            p.AbsentDeduction,
            p.TaxDeduction,
            p.AdvanceDeduction,
            p.TotalDeductions,
            p.NetPayable,
            p.Status.ToString(),
            p.DisbursedDate,
            p.PaymentReference
        )).ToList();

        return Result<IReadOnlyList<PayrollRecordDto>>.Success(dtos);
    }
}
