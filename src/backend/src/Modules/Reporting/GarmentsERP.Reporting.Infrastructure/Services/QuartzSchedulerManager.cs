using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Services.QuartzJobs;
using Microsoft.Extensions.Logging;
using Quartz;

namespace GarmentsERP.Reporting.Infrastructure.Services;

/// <summary>
/// Implements SCH-01: Quartz.NET cron-trigger manager & database execution schedule synchronization.
/// Dynamically maps scheduled job models from the automation schema into Quartz JobDetail & CronTrigger definitions.
/// </summary>
public class QuartzSchedulerManager : IQuartzSchedulerManager
{
    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ISchedulerRepository _repository;
    private readonly ILogger<QuartzSchedulerManager> _logger;

    public QuartzSchedulerManager(
        ISchedulerFactory schedulerFactory,
        ISchedulerRepository repository,
        ILogger<QuartzSchedulerManager> logger)
    {
        _schedulerFactory = schedulerFactory;
        _repository = repository;
        _logger = logger;
    }

    public async Task InitializeScheduledJobsAsync(CancellationToken ct = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var jobs = await _repository.GetAllJobsAsync(ct);

        _logger.LogInformation("Initializing {Count} Quartz.NET scheduled jobs from automation database...", jobs.Count);

        foreach (var job in jobs)
        {
            try
            {
                await ScheduleOrUpdateJobInternalAsync(scheduler, job, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to initialize Quartz job '{JobName}' with cron '{Cron}': {Error}", job.JobName, job.CronExpression, ex.Message);
            }
        }
    }

    public async Task ScheduleOrUpdateJobAsync(ScheduledReportJob job, CancellationToken ct = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(ct);
        await ScheduleOrUpdateJobInternalAsync(scheduler, job, ct);
    }

    private async Task ScheduleOrUpdateJobInternalAsync(IScheduler scheduler, ScheduledReportJob job, CancellationToken ct)
    {
        var jobKey = new JobKey(job.JobName, job.JobGroup);
        var triggerKey = new TriggerKey($"{job.JobName}_Trigger", job.JobGroup);

        // Delete existing job and trigger if present
        var existingJob = await scheduler.GetJobDetail(jobKey, ct);
        if (existingJob != null)
        {
            await scheduler.DeleteJob(jobKey, ct);
        }

        if (!job.IsActive)
        {
            _logger.LogInformation("Job '{JobName}' is marked INACTIVE. Omitted from active scheduler registration.", job.JobName);
            return;
        }

        var jobDetail = JobBuilder.Create<DailyExecutiveDigestJob>()
            .WithIdentity(jobKey)
            .WithDescription(job.CronDescription)
            .UsingJobData("JobId", job.Id.ToString())
            .UsingJobData("JobName", job.JobName)
            .UsingJobData("ReportType", job.ReportType)
            .UsingJobData("RecipientEmailList", job.RecipientEmailList)
            .UsingJobData("EmailSubjectTemplate", job.EmailSubjectTemplate)
            .StoreDurably()
            .Build();

        var trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .ForJob(jobDetail)
            .WithDescription(job.CronDescription)
            .WithCronSchedule(job.CronExpression)
            .Build();

        await scheduler.ScheduleJob(jobDetail, trigger, cancellationToken: ct);

        var nextFireTime = trigger.NextFireTimeUtc;
        _logger.LogInformation(
            "Quartz Job '{JobName}' successfully scheduled with cron '{Cron}'. Next Fire Time: {NextFire}",
            job.JobName, job.CronExpression, nextFireTime?.ToString("yyyy-MM-dd HH:mm:ss 'UTC'") ?? "N/A");
    }

    public async Task<bool> TriggerJobNowAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _repository.GetJobByIdAsync(jobId, ct);
        if (job == null) return false;

        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var jobKey = new JobKey(job.JobName, job.JobGroup);

        var existing = await scheduler.GetJobDetail(jobKey, ct);
        if (existing == null)
        {
            // Ensure job is registered even if inactive or newly created
            await ScheduleOrUpdateJobInternalAsync(scheduler, job, ct);
        }

        var dataMap = new JobDataMap
        {
            { "IsManualTrigger", true },
            { "JobId", job.Id.ToString() },
            { "TriggeredBy", "SchedulerManagementDashboard" }
        };

        await scheduler.TriggerJob(jobKey, dataMap, ct);
        _logger.LogInformation("Manual execution triggered for Quartz job '{JobName}'.", job.JobName);
        return true;
    }

    public async Task<bool> PauseJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _repository.GetJobByIdAsync(jobId, ct);
        if (job == null) return false;

        job.IsActive = false;
        await _repository.UpdateJobAsync(job, ct);

        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var jobKey = new JobKey(job.JobName, job.JobGroup);
        var existing = await scheduler.GetJobDetail(jobKey, ct);
        if (existing != null)
        {
            await scheduler.PauseJob(jobKey, ct);
        }

        _logger.LogInformation("Quartz job '{JobName}' paused successfully.", job.JobName);
        return true;
    }

    public async Task<bool> ResumeJobAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _repository.GetJobByIdAsync(jobId, ct);
        if (job == null) return false;

        job.IsActive = true;
        await _repository.UpdateJobAsync(job, ct);

        var scheduler = await _schedulerFactory.GetScheduler(ct);
        await ScheduleOrUpdateJobInternalAsync(scheduler, job, ct);

        _logger.LogInformation("Quartz job '{JobName}' resumed successfully.", job.JobName);
        return true;
    }

    public async Task<DateTimeOffset?> GetNextFireTimeAsync(Guid jobId, CancellationToken ct = default)
    {
        var job = await _repository.GetJobByIdAsync(jobId, ct);
        if (job == null) return null;

        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var triggerKey = new TriggerKey($"{job.JobName}_Trigger", job.JobGroup);
        var trigger = await scheduler.GetTrigger(triggerKey, ct);
        return trigger?.NextFireTimeUtc;
    }

    public async Task<IReadOnlyList<ScheduledReportJobDto>> GetAllJobsWithNextFireTimeAsync(CancellationToken ct = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(ct);
        var jobs = await _repository.GetAllJobsAsync(ct);
        var result = new List<ScheduledReportJobDto>();

        foreach (var j in jobs)
        {
            var triggerKey = new TriggerKey($"{j.JobName}_Trigger", j.JobGroup);
            var trigger = await scheduler.GetTrigger(triggerKey, ct);
            var nextFire = trigger?.NextFireTimeUtc;

            result.Add(new ScheduledReportJobDto(
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
                nextFire
            ));
        }

        return result;
    }
}
