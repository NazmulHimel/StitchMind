using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Reporting.Infrastructure.Repositories;

public class SchedulerRepository : ISchedulerRepository
{
    private readonly SchedulerDbContext _db;

    public SchedulerRepository(SchedulerDbContext db)
    {
        _db = db;
    }

    public async Task<IReadOnlyList<ScheduledReportJob>> GetAllJobsAsync(CancellationToken ct = default)
    {
        return await _db.ScheduledReportJobs
            .AsNoTracking()
            .OrderBy(j => j.JobName)
            .ToListAsync(ct);
    }

    public async Task<ScheduledReportJob?> GetJobByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _db.ScheduledReportJobs.FirstOrDefaultAsync(j => j.Id == id, ct);
    }

    public async Task<ScheduledReportJob?> GetJobByNameAsync(string jobName, CancellationToken ct = default)
    {
        return await _db.ScheduledReportJobs.FirstOrDefaultAsync(j => j.JobName == jobName, ct);
    }

    public async Task<ScheduledReportJob> CreateJobAsync(ScheduledReportJob job, CancellationToken ct = default)
    {
        _db.ScheduledReportJobs.Add(job);
        await _db.SaveChangesAsync(ct);
        return job;
    }

    public async Task<ScheduledReportJob> UpdateJobAsync(ScheduledReportJob job, CancellationToken ct = default)
    {
        job.UpdatedAtUtc = DateTime.UtcNow;
        _db.ScheduledReportJobs.Update(job);
        await _db.SaveChangesAsync(ct);
        return job;
    }

    public async Task<bool> DeleteJobAsync(Guid id, CancellationToken ct = default)
    {
        var job = await _db.ScheduledReportJobs.FirstOrDefaultAsync(j => j.Id == id, ct);
        if (job == null) return false;

        _db.ScheduledReportJobs.Remove(job);
        await _db.SaveChangesAsync(ct);
        return true;
    }

    public async Task LogExecutionHistoryAsync(JobExecutionHistory history, CancellationToken ct = default)
    {
        _db.JobExecutionHistories.Add(history);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<JobExecutionHistory>> GetExecutionHistoryAsync(Guid? jobId = null, int take = 50, CancellationToken ct = default)
    {
        var query = _db.JobExecutionHistories.AsNoTracking();

        if (jobId.HasValue && jobId.Value != Guid.Empty)
        {
            query = query.Where(h => h.JobId == jobId.Value);
        }

        return await query
            .OrderByDescending(h => h.TriggeredAtUtc)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<SchedulerDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default)
    {
        var jobs = await GetAllJobsAsync(ct);
        var todayUtc = DateTime.UtcNow.Date;

        var executionsToday = await _db.JobExecutionHistories
            .AsNoTracking()
            .Where(h => h.TriggeredAtUtc >= todayUtc)
            .ToListAsync(ct);

        var recentHistories = await _db.JobExecutionHistories
            .AsNoTracking()
            .OrderByDescending(h => h.TriggeredAtUtc)
            .Take(10)
            .Select(h => new JobExecutionHistoryDto(
                h.Id,
                h.JobId,
                h.JobName,
                h.ReportType,
                h.TriggeredAtUtc,
                h.CompletedAtUtc,
                h.Status,
                h.ExecutionDurationMs,
                h.Recipients,
                h.PdfSizeBytes,
                h.ErrorMessage,
                h.IsManualTrigger
            ))
            .ToListAsync(ct);

        var jobDtos = jobs.Select(j => new ScheduledReportJobDto(
            j.Id,
            j.JobName,
            j.JobGroup,
            j.CronExpression,
            j.CronDescription,
            j.ReportType,
            j.RecipientEmailList,
            j.EmailSubjectTemplate,
            j.IsActive,
            j.LastExecutedAtUtc,
            j.LastExecutionStatus,
            j.LastExecutionDurationMs,
            j.ExecutionCount,
            j.FailureCount,
            j.LastErrorMessage,
            null
        )).ToList();

        return new SchedulerDashboardSummaryDto(
            TotalJobs: jobs.Count,
            ActiveJobs: jobs.Count(j => j.IsActive),
            ExecutionsToday: executionsToday.Count,
            FailuresToday: executionsToday.Count(e => e.Status == "Failed"),
            NextScheduledRunUtc: null,
            ActiveJobsList: jobDtos,
            RecentExecutions: recentHistories
        );
    }
}
