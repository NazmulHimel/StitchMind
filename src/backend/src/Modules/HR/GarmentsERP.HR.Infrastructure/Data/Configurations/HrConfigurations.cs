using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using GarmentsERP.HR.Domain.Entities;

namespace GarmentsERP.HR.Infrastructure.Data.Configurations;

public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
{
    public void Configure(EntityTypeBuilder<Employee> builder)
    {
        builder.ToTable("Employees");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(e => e.EmployeeCode).IsUnique();
        builder.Property(e => e.FullName).HasMaxLength(150).IsRequired();
        builder.Property(e => e.Department).HasMaxLength(100).IsRequired();
        builder.Property(e => e.Designation).HasMaxLength(100).IsRequired();
        builder.Property(e => e.BasicSalary).HasPrecision(18, 2);
        builder.Property(e => e.HouseRentAllowance).HasPrecision(18, 2);
        builder.Property(e => e.MedicalAllowance).HasPrecision(18, 2);
        builder.Property(e => e.ConveyanceAllowance).HasPrecision(18, 2);
    }
}

public class AttendanceConfiguration : IEntityTypeConfiguration<AttendanceRecord>
{
    public void Configure(EntityTypeBuilder<AttendanceRecord> builder)
    {
        builder.ToTable("AttendanceRecords");
        builder.HasKey(a => a.Id);
        builder.HasIndex(a => new { a.EmployeeId, a.Date }).IsUnique();
        builder.Property(a => a.OvertimeHours).HasPrecision(5, 2);
        builder.HasOne(a => a.Employee)
               .WithMany(e => e.AttendanceRecords)
               .HasForeignKey(a => a.EmployeeId);
    }
}

public class LeaveRequestConfiguration : IEntityTypeConfiguration<LeaveRequest>
{
    public void Configure(EntityTypeBuilder<LeaveRequest> builder)
    {
        builder.ToTable("LeaveRequests");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Reason).HasMaxLength(500);
        builder.HasOne(l => l.Employee)
               .WithMany(e => e.LeaveRequests)
               .HasForeignKey(l => l.EmployeeId);
    }
}

public class PayrollRecordConfiguration : IEntityTypeConfiguration<PayrollRecord>
{
    public void Configure(EntityTypeBuilder<PayrollRecord> builder)
    {
        builder.ToTable("PayrollRecords");
        builder.HasKey(p => p.Id);
        builder.HasIndex(p => new { p.EmployeeId, p.PayMonth, p.PayYear }).IsUnique();
        builder.Property(p => p.BasicSalary).HasPrecision(18, 2);
        builder.Property(p => p.Allowances).HasPrecision(18, 2);
        builder.Property(p => p.OvertimeRatePerHour).HasPrecision(18, 2);
        builder.Property(p => p.AbsentDeduction).HasPrecision(18, 2);
        builder.Property(p => p.TaxDeduction).HasPrecision(18, 2);
        builder.Property(p => p.AdvanceDeduction).HasPrecision(18, 2);
        builder.HasOne(p => p.Employee)
               .WithMany(e => e.PayrollRecords)
               .HasForeignKey(p => p.EmployeeId);
    }
}
