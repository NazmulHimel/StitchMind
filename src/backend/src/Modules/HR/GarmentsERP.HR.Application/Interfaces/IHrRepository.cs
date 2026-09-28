using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;

namespace GarmentsERP.HR.Application.Interfaces;

public interface IHrRepository
{
    // Employees & Digital Service Book (AC-07)
    Task<IReadOnlyList<Employee>> GetEmployeesAsync(string? department = null, CancellationToken ct = default);
    Task<Employee?> GetEmployeeByIdAsync(Guid id, CancellationToken ct = default);
    Task<Employee?> GetEmployeeByCodeAsync(string code, CancellationToken ct = default);
    Task AddEmployeeAsync(Employee employee, CancellationToken ct = default);
    Task UpdateEmployeeAsync(Employee employee, CancellationToken ct = default);

    // Attendance & Biometrics (HR-02)
    Task<IReadOnlyList<AttendanceRecord>> GetAttendanceRecordsAsync(DateTime? date = null, Guid? employeeId = null, CancellationToken ct = default);
    Task<AttendanceRecord?> GetAttendanceByEmployeeDateAsync(Guid employeeId, DateTime date, CancellationToken ct = default);
    Task AddAttendanceAsync(AttendanceRecord attendance, CancellationToken ct = default);
    Task UpdateAttendanceAsync(AttendanceRecord attendance, CancellationToken ct = default);
    Task<IReadOnlyList<BiometricDevice>> GetBiometricDevicesAsync(CancellationToken ct = default);
    Task<BiometricDevice?> GetBiometricDeviceByIdAsync(Guid id, CancellationToken ct = default);
    Task AddBiometricPunchLogAsync(BiometricPunchLog punchLog, CancellationToken ct = default);
    Task<IReadOnlyList<BiometricPunchLog>> GetBiometricPunchLogsAsync(DateTime? date = null, Guid? deviceId = null, CancellationToken ct = default);

    // Work Shifts, Rosters & Penalties (HR-03)
    Task<IReadOnlyList<WorkShift>> GetWorkShiftsAsync(CancellationToken ct = default);
    Task<WorkShift?> GetWorkShiftByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ShiftRoster>> GetShiftRostersAsync(Guid? employeeId = null, DateTime? date = null, CancellationToken ct = default);
    Task AddShiftRosterAsync(ShiftRoster roster, CancellationToken ct = default);
    Task<IReadOnlyList<LateAttendancePenalty>> GetLatePenaltiesAsync(int? month = null, int? year = null, Guid? employeeId = null, CancellationToken ct = default);
    Task AddLatePenaltyAsync(LateAttendancePenalty penalty, CancellationToken ct = default);

    // Leaves, Balances & Maternity Benefits (HR-04)
    Task<IReadOnlyList<LeaveRequest>> GetLeaveRequestsAsync(LeaveStatus? status = null, CancellationToken ct = default);
    Task<LeaveRequest?> GetLeaveRequestByIdAsync(Guid id, CancellationToken ct = default);
    Task AddLeaveRequestAsync(LeaveRequest request, CancellationToken ct = default);
    Task UpdateLeaveRequestAsync(LeaveRequest request, CancellationToken ct = default);
    Task<EmployeeLeaveBalance?> GetLeaveBalanceAsync(Guid employeeId, int year, CancellationToken ct = default);
    Task AddOrUpdateLeaveBalanceAsync(EmployeeLeaveBalance balance, CancellationToken ct = default);
    Task<IReadOnlyList<MaternityBenefit>> GetMaternityBenefitsAsync(CancellationToken ct = default);
    Task<MaternityBenefit?> GetMaternityBenefitByIdAsync(Guid id, CancellationToken ct = default);
    Task AddMaternityBenefitAsync(MaternityBenefit benefit, CancellationToken ct = default);
    Task UpdateMaternityBenefitAsync(MaternityBenefit benefit, CancellationToken ct = default);

    // Dual-Stream Payroll (HR-05 & HR-06)
    Task<IReadOnlyList<PayrollRecord>> GetPayrollRecordsAsync(int? month = null, int? year = null, CancellationToken ct = default);
    Task<PayrollRecord?> GetPayrollRecordByIdAsync(Guid id, CancellationToken ct = default);
    Task AddPayrollRecordsAsync(IEnumerable<PayrollRecord> records, CancellationToken ct = default);
    Task UpdatePayrollRecordAsync(PayrollRecord record, CancellationToken ct = default);
    Task<IReadOnlyList<PayrollSheet>> GetPayrollSheetsAsync(CancellationToken ct = default);
    Task<PayrollSheet?> GetPayrollSheetByIdAsync(Guid id, CancellationToken ct = default);
    Task<PayrollSheet?> GetPayrollSheetByPeriodAsync(int month, int year, CancellationToken ct = default);
    Task AddPayrollSheetAsync(PayrollSheet sheet, CancellationToken ct = default);
    Task UpdatePayrollSheetAsync(PayrollSheet sheet, CancellationToken ct = default);

    // Health Clinic, Grievance Box & Disciplinary Actions (HR-07)
    Task<IReadOnlyList<ClinicVisit>> GetClinicVisitsAsync(DateTime? fromDate = null, Guid? employeeId = null, CancellationToken ct = default);
    Task AddClinicVisitAsync(ClinicVisit visit, CancellationToken ct = default);
    Task<IReadOnlyList<WorkerGrievance>> GetWorkerGrievancesAsync(GrievanceStatus? status = null, CancellationToken ct = default);
    Task<WorkerGrievance?> GetWorkerGrievanceByIdAsync(Guid id, CancellationToken ct = default);
    Task AddWorkerGrievanceAsync(WorkerGrievance grievance, CancellationToken ct = default);
    Task UpdateWorkerGrievanceAsync(WorkerGrievance grievance, CancellationToken ct = default);
    Task<IReadOnlyList<DisciplinaryAction>> GetDisciplinaryActionsAsync(Guid? employeeId = null, CancellationToken ct = default);
    Task AddDisciplinaryActionAsync(DisciplinaryAction action, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
