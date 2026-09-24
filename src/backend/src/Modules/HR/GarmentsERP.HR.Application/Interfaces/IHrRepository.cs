using GarmentsERP.HR.Domain.Entities;

namespace GarmentsERP.HR.Application.Interfaces;

public interface IHrRepository
{
    Task<IReadOnlyList<Employee>> GetEmployeesAsync(string? department = null, CancellationToken ct = default);
    Task<Employee?> GetEmployeeByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetEmployeeByCodeAsync(string code, CancellationToken ct = default);
    Task AddEmployeeAsync(Employee employee, CancellationToken ct = default);

    Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(DateTime? date = null, Guid? employeeId = null, CancellationToken ct = default);
    Task<AttendanceRecord?> GetAttendanceByEmployeeDateAsync(Guid employeeId, DateTime date, CancellationToken ct = default);
    Task AddAttendanceAsync(AttendanceRecord attendance, CancellationToken ct = default);
    Task UpdateAttendanceAsync(AttendanceRecord attendance, CancellationToken ct = default);

    Task<IReadOnlyList<LeaveRequest>> GetLeaveRequestsAsync(LeaveStatus? status = null, CancellationToken ct = default);
    Task<LeaveRequest?> GetLeaveRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task AddLeaveRequestAsync(LeaveRequest request, CancellationToken ct = default);
    Task UpdateLeaveRequestAsync(LeaveRequest request, CancellationToken ct = default);

    Task<IReadOnlyList<PayrollRecord>> GetPayrollRecordsAsync(int? month = null, int? year = null, CancellationToken ct = default);
    Task AddPayrollRecordsAsync(IEnumerable<PayrollRecord> records, CancellationToken ct = default);
    Task UpdatePayrollRecordAsync(PayrollRecord record, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
