using Microsoft.EntityFrameworkCore;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.HR.Infrastructure.Data;

public class HrDbContext : DbContext
{
    public HrDbContext(DbContextOptions<HrDbContext> options) : base(options) { }

    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<AttendanceRecord> AttendanceRecords => Set<AttendanceRecord>();
    public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
    public DbSet<PayrollRecord> PayrollRecords => Set<PayrollRecord>();
    public DbSet<PayrollSheet> PayrollSheets => Set<PayrollSheet>();

    // Phase 7 Entities
    public DbSet<BiometricDevice> BiometricDevices => Set<BiometricDevice>();
    public DbSet<BiometricPunchLog> BiometricPunchLogs => Set<BiometricPunchLog>();
    public DbSet<WorkShift> WorkShifts => Set<WorkShift>();
    public DbSet<ShiftRoster> ShiftRosters => Set<ShiftRoster>();
    public DbSet<LateAttendancePenalty> LateAttendancePenalties => Set<LateAttendancePenalty>();
    public DbSet<EmployeeLeaveBalance> EmployeeLeaveBalances => Set<EmployeeLeaveBalance>();
    public DbSet<MaternityBenefit> MaternityBenefits => Set<MaternityBenefit>();
    public DbSet<ClinicVisit> ClinicVisits => Set<ClinicVisit>();
    public DbSet<WorkerGrievance> WorkerGrievances => Set<WorkerGrievance>();
    public DbSet<DisciplinaryAction> DisciplinaryActions => Set<DisciplinaryAction>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("hr");

        modelBuilder.Entity<Employee>(b =>
        {
            b.HasIndex(e => e.EmployeeCode).IsUnique();
            b.HasIndex(e => e.BiometricBadgeNumber);
        });

        modelBuilder.Entity<BiometricDevice>(b =>
        {
            b.HasIndex(d => d.DeviceCode).IsUnique();
        });

        modelBuilder.Entity<WorkShift>(b =>
        {
            b.HasIndex(s => s.ShiftCode).IsUnique();
        });

        modelBuilder.Entity<WorkerGrievance>(b =>
        {
            b.HasIndex(g => g.TrackingNumber).IsUnique();
        });

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
    }
}
