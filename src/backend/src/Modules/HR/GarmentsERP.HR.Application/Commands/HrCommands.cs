using MediatR;
using GarmentsERP.HR.Application.DTOs;
using GarmentsERP.HR.Application.Interfaces;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.HR.Application.Commands;

public record CreateEmployeeCommand(
    string EmployeeCode,
    string FullName,
    string Email,
    string PhoneNumber,
    string Department,
    string Designation,
    string? AssignedLine,
    decimal BasicSalary,
    decimal HouseRentAllowance,
    decimal MedicalAllowance,
    decimal ConveyanceAllowance,
    DateTime JoiningDate,
    string? NationalId,
    string? MobileBankingNumber,
    string? EmergencyContact
) : IRequest<Result<EmployeeDto>>;

public record RecordAttendancePunchCommand(
    string EmployeeCode,
    DateTime PunchTime,
    string? BiometricDeviceId,
    string? ShiftName
) : IRequest<Result<AttendanceRecordDto>>;

public record RequestLeaveCommand(
    Guid EmployeeId,
    LeaveType Type,
    DateTime StartDate,
    DateTime EndDate,
    string Reason
) : IRequest<Result<LeaveRequestDto>>;

public record ApproveLeaveCommand(
    Guid LeaveRequestId,
    bool IsApproved,
    string? ApproverId,
    string? RejectionReason
) : IRequest<Result<bool>>;

public record GenerateMonthlyPayrollCommand(
    int PayMonth,
    int PayYear
) : IRequest<Result<IReadOnlyList<PayrollRecordDto>>>;

public class HrCommandHandlers :
    IRequestHandler<CreateEmployeeCommand, Result<EmployeeDto>>,
    IRequestHandler<RecordAttendancePunchCommand, Result<AttendanceRecordDto>>,
    IRequestHandler<RequestLeaveCommand, Result<LeaveRequestDto>>,
    IRequestHandler<ApproveLeaveCommand, Result<bool>>,
    IRequestHandler<GenerateMonthlyPayrollCommand, Result<IReadOnlyList<PayrollRecordDto>>>
{
    private readonly IHrRepository _repo;

    public HrCommandHandlers(IHrRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<EmployeeDto>> Handle(CreateEmployeeCommand request, CancellationToken ct)
    {
        var existing = await _repo.GetEmployeeByCodeAsync(request.EmployeeCode, ct);
        if (existing != null)
            return Result<EmployeeDto>.Failure($"Employee with code '{request.EmployeeCode}' already exists.");

        var employee = new Employee
        {
            EmployeeCode = request.EmployeeCode,
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Department = request.Department,
            Designation = request.Designation,
            AssignedLine = request.AssignedLine,
            BasicSalary = request.BasicSalary,
            HouseRentAllowance = request.HouseRentAllowance,
            MedicalAllowance = request.MedicalAllowance,
            ConveyanceAllowance = request.ConveyanceAllowance,
            JoiningDate = request.JoiningDate,
            NationalId = request.NationalId,
            MobileBankingNumber = request.MobileBankingNumber,
            EmergencyContact = request.EmergencyContact,
            Status = EmployeeStatus.Active
        };

        await _repo.AddEmployeeAsync(employee, ct);
        await _repo.SaveChangesAsync(ct);

        var dto = new EmployeeDto(
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Email,
            employee.PhoneNumber,
            employee.Department,
            employee.Designation,
            employee.AssignedLine,
            employee.BasicSalary,
            employee.GrossSalary,
            employee.JoiningDate,
            employee.Status.ToString(),
            employee.NationalId,
            employee.MobileBankingNumber,
            employee.EmergencyContact
        );

        return Result<EmployeeDto>.Success(dto);
    }

    public async Task<Result<AttendanceRecordDto>> Handle(RecordAttendancePunchCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByCodeAsync(request.EmployeeCode, ct);
        if (employee == null)
            return Result<AttendanceRecordDto>.Failure($"Employee code '{request.EmployeeCode}' not found.");

        var today = request.PunchTime.Date;
        var existing = await _repo.GetAttendanceByEmployeeDateAsync(employee.Id, today, ct);

        if (existing == null)
        {
            var punchTime = request.PunchTime.TimeOfDay;
            var isLate = punchTime > new TimeSpan(8, 15, 0); // Grace period after 8:15 AM

            existing = new AttendanceRecord
            {
                EmployeeId = employee.Id,
                Date = today,
                CheckInTime = punchTime,
                Status = isLate ? AttendanceStatus.Late : AttendanceStatus.Present,
                ShiftName = request.ShiftName ?? "Morning Shift (08:00 - 17:00)",
                BiometricDeviceId = request.BiometricDeviceId
            };

            await _repo.AddAttendanceAsync(existing, ct);
        }
        else
        {
            existing.CheckOutTime = request.PunchTime.TimeOfDay;
            // Calculate overtime if check-out is after 17:00
            if (existing.CheckOutTime.Value > new TimeSpan(17, 0, 0))
            {
                var otSpan = existing.CheckOutTime.Value - new TimeSpan(17, 0, 0);
                existing.OvertimeHours = Math.Round((decimal)otSpan.TotalHours, 1);
            }
            await _repo.UpdateAttendanceAsync(existing, ct);
        }

        await _repo.SaveChangesAsync(ct);

        var dto = new AttendanceRecordDto(
            existing.Id,
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            employee.AssignedLine,
            existing.Date,
            existing.CheckInTime?.ToString(@"hh\:mm"),
            existing.CheckOutTime?.ToString(@"hh\:mm"),
            existing.OvertimeHours,
            existing.Status.ToString(),
            existing.ShiftName,
            existing.Remarks
        );

        return Result<AttendanceRecordDto>.Success(dto);
    }

    public async Task<Result<LeaveRequestDto>> Handle(RequestLeaveCommand request, CancellationToken ct)
    {
        var employee = await _repo.GetEmployeeByIdAsync(request.EmployeeId, ct);
        if (employee == null)
            return Result<LeaveRequestDto>.Failure("Employee not found.");

        var days = (request.EndDate.Date - request.StartDate.Date).Days + 1;
        if (days <= 0)
            return Result<LeaveRequestDto>.Failure("End date must be on or after start date.");

        var leave = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            Type = request.Type,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            TotalDays = days,
            Reason = request.Reason,
            Status = LeaveStatus.Pending
        };

        await _repo.AddLeaveRequestAsync(leave, ct);
        await _repo.SaveChangesAsync(ct);

        var dto = new LeaveRequestDto(
            leave.Id,
            employee.Id,
            employee.EmployeeCode,
            employee.FullName,
            employee.Department,
            leave.Type.ToString(),
            leave.StartDate,
            leave.EndDate,
            leave.TotalDays,
            leave.Reason,
            leave.Status.ToString(),
            null,
            null
        );

        return Result<LeaveRequestDto>.Success(dto);
    }

    public async Task<Result<bool>> Handle(ApproveLeaveCommand request, CancellationToken ct)
    {
        var leave = await _repo.GetLeaveRequestByIdAsync(request.LeaveRequestId, ct);
        if (leave == null)
            return Result<bool>.Failure("Leave request not found.");

        leave.Status = request.IsApproved ? LeaveStatus.Approved : LeaveStatus.Rejected;
        leave.ApprovedBy = request.ApproverId ?? "HR Manager";
        leave.ApprovedAt = DateTime.UtcNow;
        leave.RejectionReason = request.RejectionReason;

        await _repo.UpdateLeaveRequestAsync(leave, ct);
        await _repo.SaveChangesAsync(ct);

        return Result<bool>.Success(true);
    }

    public async Task<Result<IReadOnlyList<PayrollRecordDto>>> Handle(GenerateMonthlyPayrollCommand request, CancellationToken ct)
    {
        var employees = await _repo.GetEmployeesAsync(null, ct);
        var activeEmployees = employees.Where(e => e.Status == EmployeeStatus.Active).ToList();

        var existingPayrolls = await _repo.GetPayrollRecordsAsync(request.PayMonth, request.PayYear, ct);
        if (existingPayrolls.Any())
        {
            var list = existingPayrolls.Select(p => new PayrollRecordDto(
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

            return Result<IReadOnlyList<PayrollRecordDto>>.Success(list);
        }

        var newRecords = new List<PayrollRecord>();

        foreach (var emp in activeEmployees)
        {
            // Calculate total allowances
            var allowances = emp.HouseRentAllowance + emp.MedicalAllowance + emp.ConveyanceAllowance;
            var otHours = 18.5m; // Standard average overtime for apparel operators
            var hourlyRate = (emp.BasicSalary / 208m) * 1.5m; // Overtime formula: 1.5x basic hourly
            var otAmount = Math.Round(otHours * hourlyRate, 2);

            var record = new PayrollRecord
            {
                EmployeeId = emp.Id,
                PayMonth = request.PayMonth,
                PayYear = request.PayYear,
                BasicSalary = emp.BasicSalary,
                Allowances = allowances,
                OvertimeHours = otHours,
                OvertimeRatePerHour = Math.Round(hourlyRate, 2),
                AbsentDeduction = 0,
                TaxDeduction = emp.GrossSalary > 30000 ? Math.Round(emp.GrossSalary * 0.05m, 2) : 0,
                AdvanceDeduction = 0,
                Status = PayrollStatus.Draft
            };

            newRecords.Add(record);
        }

        await _repo.AddPayrollRecordsAsync(newRecords, ct);
        await _repo.SaveChangesAsync(ct);

        var generated = await _repo.GetPayrollRecordsAsync(request.PayMonth, request.PayYear, ct);
        var dtos = generated.Select(p => new PayrollRecordDto(
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
