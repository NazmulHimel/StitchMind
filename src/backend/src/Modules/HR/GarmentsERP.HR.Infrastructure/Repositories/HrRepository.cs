using Microsoft.EntityFrameworkCore;
using GarmentsERP.HR.Application.Interfaces;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;
using GarmentsERP.HR.Infrastructure.Data;

namespace GarmentsERP.HR.Infrastructure.Repositories;

public class HrRepository : IHrRepository
{
    private readonly HrDbContext _context;

    public HrRepository(HrDbContext context)
    {
        _context = context;
    }

    // Employees & Digital Service Book (AC-07)
    public async Task<IReadOnlyList<Employee>> GetEmployeesAsync(string? department = null, CancellationToken ct = default)
    {
        var query = _context.Employees.AsQueryable();
        if (!string.IsNullOrEmpty(department) && department != "ALL")
        {
            query = query.Where(e => e.Department == department);
        }
        return await query.OrderBy(e => e.EmployeeCode).ToListAsync(ct);
    }

    public async Task<Employee?> GetEmployeeByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task<Employee?> GetEmployeeByCodeAsync(string code, CancellationToken ct = default)
    {
        return await _context.Employees.FirstOrDefaultAsync(e => e.EmployeeCode == code, ct);
    }

    public async Task AddEmployeeAsync(Employee employee, CancellationToken ct = default)
    {
        await _context.Employees.AddAsync(employee, ct);
    }

    public Task UpdateEmployeeAsync(Employee employee, CancellationToken ct = default)
    {
        _context.Employees.Update(employee);
        return Task.CompletedTask;
    }

    // Attendance & Biometrics (HR-02)
    public async Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(DateTime? date = null, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _context.AttendanceRecords.Include(a => a.Employee).AsQueryable();
        if (date.HasValue)
        {
            var target = date.Value.Date;
            query = query.Where(a => a.Date == target);
        }
        if (employeeId.HasValue)
        {
            query = query.Where(a => a.EmployeeId == employeeId.Value);
        }
        return await query.OrderByDescending(a => a.Date).ThenBy(a => a.Employee.EmployeeCode).ToListAsync(ct);
    }

    public async Task<AttendanceRecord?> GetAttendanceByEmployeeDateAsync(Guid employeeId, DateTime date, CancellationToken ct = default)
    {
        var target = date.Date;
        return await _context.AttendanceRecords
                             .Include(a => a.Employee)
                             .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.Date == target, ct);
    }

    public async Task AddAttendanceAsync(AttendanceRecord attendance, CancellationToken ct = default)
    {
        await _context.AttendanceRecords.AddAsync(attendance, ct);
    }

    public Task UpdateAttendanceAsync(AttendanceRecord attendance, CancellationToken ct = default)
    {
        _context.AttendanceRecords.Update(attendance);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<BiometricDevice>> GetBiometricDevicesAsync(CancellationToken ct = default)
    {
        return await _context.BiometricDevices.OrderBy(d => d.DeviceCode).ToListAsync(ct);
    }

    public async Task<BiometricDevice?> GetBiometricDeviceByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.BiometricDevices.FirstOrDefaultAsync(d => d.Id == id, ct);
    }

    public async Task AddBiometricPunchLogAsync(BiometricPunchLog punchLog, CancellationToken ct = default)
    {
        await _context.BiometricPunchLogs.AddAsync(punchLog, ct);
    }

    public async Task<IReadOnlyList<BiometricPunchLog>> GetBiometricPunchLogsAsync(DateTime? date = null, Guid? deviceId = null, CancellationToken ct = default)
    {
        var query = _context.BiometricPunchLogs.Include(p => p.BiometricDevice).Include(p => p.Employee).AsQueryable();
        if (date.HasValue)
        {
            var target = date.Value.Date;
            query = query.Where(p => p.PunchTimestamp.Date == target);
        }
        if (deviceId.HasValue)
        {
            query = query.Where(p => p.BiometricDeviceId == deviceId.Value);
        }
        return await query.OrderByDescending(p => p.PunchTimestamp).Take(200).ToListAsync(ct);
    }

    // Work Shifts, Rosters & Penalties (HR-03)
    public async Task<IReadOnlyList<WorkShift>> GetWorkShiftsAsync(CancellationToken ct = default)
    {
        return await _context.WorkShifts.OrderBy(s => s.StartTime).ToListAsync(ct);
    }

    public async Task<WorkShift?> GetWorkShiftByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.WorkShifts.FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<IReadOnlyList<ShiftRoster>> GetShiftRostersAsync(Guid? employeeId = null, DateTime? date = null, CancellationToken ct = default)
    {
        var query = _context.ShiftRosters.Include(r => r.Employee).Include(r => r.WorkShift).AsQueryable();
        if (employeeId.HasValue)
        {
            query = query.Where(r => r.EmployeeId == employeeId.Value);
        }
        if (date.HasValue)
        {
            var target = date.Value.Date;
            query = query.Where(r => r.EffectiveFrom <= target && (!r.EffectiveTo.HasValue || r.EffectiveTo.Value >= target));
        }
        return await query.OrderBy(r => r.Employee.EmployeeCode).ToListAsync(ct);
    }

    public async Task AddShiftRosterAsync(ShiftRoster roster, CancellationToken ct = default)
    {
        await _context.ShiftRosters.AddAsync(roster, ct);
    }

    public async Task<IReadOnlyList<LateAttendancePenalty>> GetLatePenaltiesAsync(int? month = null, int? year = null, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _context.LateAttendancePenalties.Include(p => p.Employee).AsQueryable();
        if (month.HasValue) query = query.Where(p => p.PayMonth == month.Value);
        if (year.HasValue) query = query.Where(p => p.PayYear == year.Value);
        if (employeeId.HasValue) query = query.Where(p => p.EmployeeId == employeeId.Value);
        return await query.OrderBy(p => p.Employee.EmployeeCode).ToListAsync(ct);
    }

    public async Task AddLatePenaltyAsync(LateAttendancePenalty penalty, CancellationToken ct = default)
    {
        await _context.LateAttendancePenalties.AddAsync(penalty, ct);
    }

    // Leaves, Balances & Maternity Benefits (HR-04)
    public async Task<IReadOnlyList<LeaveRequest>> GetLeaveRequestsAsync(LeaveStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.LeaveRequests.Include(l => l.Employee).AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(l => l.Status == status.Value);
        }
        return await query.OrderByDescending(l => l.CreatedAt).ToListAsync(ct);
    }

    public async Task<LeaveRequest?> GetLeaveRequestByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.LeaveRequests.Include(l => l.Employee).FirstOrDefaultAsync(l => l.Id == id, ct);
    }

    public async Task AddLeaveRequestAsync(LeaveRequest request, CancellationToken ct = default)
    {
        await _context.LeaveRequests.AddAsync(request, ct);
    }

    public Task UpdateLeaveRequestAsync(LeaveRequest request, CancellationToken ct = default)
    {
        _context.LeaveRequests.Update(request);
        return Task.CompletedTask;
    }

    public async Task<EmployeeLeaveBalance?> GetLeaveBalanceAsync(Guid employeeId, int year, CancellationToken ct = default)
    {
        return await _context.EmployeeLeaveBalances
                             .Include(b => b.Employee)
                             .FirstOrDefaultAsync(b => b.EmployeeId == employeeId && b.CalendarYear == year, ct);
    }

    public async Task AddOrUpdateLeaveBalanceAsync(EmployeeLeaveBalance balance, CancellationToken ct = default)
    {
        var existing = await _context.EmployeeLeaveBalances.FirstOrDefaultAsync(b => b.Id == balance.Id, ct);
        if (existing == null)
            await _context.EmployeeLeaveBalances.AddAsync(balance, ct);
        else
            _context.EmployeeLeaveBalances.Update(balance);
    }

    public async Task<IReadOnlyList<MaternityBenefit>> GetMaternityBenefitsAsync(CancellationToken ct = default)
    {
        return await _context.MaternityBenefits.Include(m => m.Employee).OrderByDescending(m => m.NoticeDate).ToListAsync(ct);
    }

    public async Task<MaternityBenefit?> GetMaternityBenefitByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.MaternityBenefits.Include(m => m.Employee).FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task AddMaternityBenefitAsync(MaternityBenefit benefit, CancellationToken ct = default)
    {
        await _context.MaternityBenefits.AddAsync(benefit, ct);
    }

    public Task UpdateMaternityBenefitAsync(MaternityBenefit benefit, CancellationToken ct = default)
    {
        _context.MaternityBenefits.Update(benefit);
        return Task.CompletedTask;
    }

    // Dual-Stream Payroll (HR-05 & HR-06)
    public async Task<IReadOnlyList<PayrollRecord>> GetPayrollRecordsAsync(int? month = null, int? year = null, CancellationToken ct = default)
    {
        var query = _context.PayrollRecords.Include(p => p.Employee).AsQueryable();
        if (month.HasValue) query = query.Where(p => p.PayMonth == month.Value);
        if (year.HasValue) query = query.Where(p => p.PayYear == year.Value);
        return await query.OrderBy(p => p.Employee.EmployeeCode).ToListAsync(ct);
    }

    public async Task<PayrollRecord?> GetPayrollRecordByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PayrollRecords.Include(p => p.Employee).FirstOrDefaultAsync(p => p.Id == id, ct);
    }

    public async Task AddPayrollRecordsAsync(IEnumerable<PayrollRecord> records, CancellationToken ct = default)
    {
        await _context.PayrollRecords.AddRangeAsync(records, ct);
    }

    public Task UpdatePayrollRecordAsync(PayrollRecord record, CancellationToken ct = default)
    {
        _context.PayrollRecords.Update(record);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<PayrollSheet>> GetPayrollSheetsAsync(CancellationToken ct = default)
    {
        return await _context.PayrollSheets
                             .Include(s => s.Records)
                             .ThenInclude(r => r.Employee)
                             .OrderByDescending(s => s.PayYear)
                             .ThenByDescending(s => s.PayMonth)
                             .ToListAsync(ct);
    }

    public async Task<PayrollSheet?> GetPayrollSheetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.PayrollSheets
                             .Include(s => s.Records)
                             .ThenInclude(r => r.Employee)
                             .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<PayrollSheet?> GetPayrollSheetByPeriodAsync(int month, int year, CancellationToken ct = default)
    {
        return await _context.PayrollSheets
                             .Include(s => s.Records)
                             .ThenInclude(r => r.Employee)
                             .FirstOrDefaultAsync(s => s.PayMonth == month && s.PayYear == year, ct);
    }

    public async Task AddPayrollSheetAsync(PayrollSheet sheet, CancellationToken ct = default)
    {
        await _context.PayrollSheets.AddAsync(sheet, ct);
    }

    public Task UpdatePayrollSheetAsync(PayrollSheet sheet, CancellationToken ct = default)
    {
        _context.PayrollSheets.Update(sheet);
        return Task.CompletedTask;
    }

    // Health Clinic, Grievance Box & Disciplinary Actions (HR-07)
    public async Task<IReadOnlyList<ClinicVisit>> GetClinicVisitsAsync(DateTime? fromDate = null, Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _context.ClinicVisits.Include(c => c.Employee).AsQueryable();
        if (fromDate.HasValue)
        {
            query = query.Where(c => c.VisitDate >= fromDate.Value.Date);
        }
        if (employeeId.HasValue)
        {
            query = query.Where(c => c.EmployeeId == employeeId.Value);
        }
        return await query.OrderByDescending(c => c.VisitDate).ToListAsync(ct);
    }

    public async Task AddClinicVisitAsync(ClinicVisit visit, CancellationToken ct = default)
    {
        await _context.ClinicVisits.AddAsync(visit, ct);
    }

    public async Task<IReadOnlyList<WorkerGrievance>> GetWorkerGrievancesAsync(GrievanceStatus? status = null, CancellationToken ct = default)
    {
        var query = _context.WorkerGrievances.Include(g => g.Employee).AsQueryable();
        if (status.HasValue)
        {
            query = query.Where(g => g.Status == status.Value);
        }
        return await query.OrderByDescending(g => g.SubmissionDate).ToListAsync(ct);
    }

    public async Task<WorkerGrievance?> GetWorkerGrievanceByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.WorkerGrievances.Include(g => g.Employee).FirstOrDefaultAsync(g => g.Id == id, ct);
    }

    public async Task AddWorkerGrievanceAsync(WorkerGrievance grievance, CancellationToken ct = default)
    {
        await _context.WorkerGrievances.AddAsync(grievance, ct);
    }

    public Task UpdateWorkerGrievanceAsync(WorkerGrievance grievance, CancellationToken ct = default)
    {
        _context.WorkerGrievances.Update(grievance);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<DisciplinaryAction>> GetDisciplinaryActionsAsync(Guid? employeeId = null, CancellationToken ct = default)
    {
        var query = _context.DisciplinaryActions.Include(d => d.Employee).AsQueryable();
        if (employeeId.HasValue)
        {
            query = query.Where(d => d.EmployeeId == employeeId.Value);
        }
        return await query.OrderByDescending(d => d.IssuedDate).ToListAsync(ct);
    }

    public async Task AddDisciplinaryActionAsync(DisciplinaryAction action, CancellationToken ct = default)
    {
        await _context.DisciplinaryActions.AddAsync(action, ct);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
