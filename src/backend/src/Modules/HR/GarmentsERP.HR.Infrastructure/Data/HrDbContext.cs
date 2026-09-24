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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("hr");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HrDbContext).Assembly);
    }
}
