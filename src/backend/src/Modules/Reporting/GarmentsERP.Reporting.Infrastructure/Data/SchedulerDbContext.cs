using GarmentsERP.Reporting.Application.Models;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Reporting.Infrastructure.Data;

public class SchedulerDbContext : DbContext
{
    public SchedulerDbContext(DbContextOptions<SchedulerDbContext> options) : base(options)
    {
    }

    public DbSet<ScheduledReportJob> ScheduledReportJobs => Set<ScheduledReportJob>();
    public DbSet<JobExecutionHistory> JobExecutionHistories => Set<JobExecutionHistory>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("automation");

        modelBuilder.Entity<ScheduledReportJob>(builder =>
        {
            builder.ToTable("ScheduledReportJobs");
            builder.HasKey(j => j.Id);
            builder.Property(j => j.JobName).HasMaxLength(100).IsRequired();
            builder.HasIndex(j => j.JobName).IsUnique();
            builder.Property(j => j.JobGroup).HasMaxLength(50).IsRequired();
            builder.Property(j => j.CronExpression).HasMaxLength(50).IsRequired();
            builder.Property(j => j.CronDescription).HasMaxLength(200);
            builder.Property(j => j.ReportType).HasMaxLength(50).IsRequired();
            builder.Property(j => j.RecipientEmailList).HasMaxLength(1000).IsRequired();
            builder.Property(j => j.EmailSubjectTemplate).HasMaxLength(250).IsRequired();
            builder.Property(j => j.LastExecutionStatus).HasMaxLength(20);
        });

        modelBuilder.Entity<JobExecutionHistory>(builder =>
        {
            builder.ToTable("JobExecutionHistories");
            builder.HasKey(h => h.Id);
            builder.Property(h => h.JobName).HasMaxLength(100).IsRequired();
            builder.Property(h => h.ReportType).HasMaxLength(50).IsRequired();
            builder.Property(h => h.Status).HasMaxLength(20).IsRequired();
            builder.Property(h => h.Recipients).HasMaxLength(1000);
            builder.HasIndex(h => h.JobId);
            builder.HasIndex(h => h.TriggeredAtUtc);
        });
    }
}
