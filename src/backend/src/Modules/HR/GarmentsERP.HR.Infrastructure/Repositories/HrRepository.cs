using Microsoft.EntityFrameworkCore;
using GarmentsERP.HR.Application.Interfaces;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Infrastructure.Data;

namespace GarmentsERP.HR.Infrastructure.Repositories;

public class HrRepository : IHrRepository
{
    private readonly HrDbContext _context;

    public HrRepository(HrDbContext context)
    {
        _context = context;
    }

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

    public async Task<IReadOnlyList<PayrollRecord>> GetPayrollRecordsAsync(int? month = null, int? year = null, CancellationToken ct = default)
    {
        var query = _context.PayrollRecords.Include(p => p.Employee).AsQueryable();
        if (month.HasValue) query = query.Where(p => p.PayMonth == month.Value);
        if (year.HasValue) query = query.Where(p => p.PayYear == year.Value);
        return await query.OrderBy(p => p.Employee.EmployeeCode).ToListAsync(ct);
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

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
