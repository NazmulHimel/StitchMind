using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Services.QuartzJobs;
using GarmentsERP.Shared.Common;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SchedulerController : ControllerBase
{
    private readonly ISchedulerRepository _repository;
    private readonly IQuartzSchedulerManager _schedulerManager;
    private readonly IPdfReportService _pdfService;
    private readonly IEmailDispatcher _emailDispatcher;
    private readonly ILogger<SchedulerController> _logger;

    public SchedulerController(
        ISchedulerRepository repository,
        IQuartzSchedulerManager schedulerManager,
        IPdfReportService pdfService,
        IEmailDispatcher emailDispatcher,
        ILogger<SchedulerController> logger)
    {
        _repository = repository;
        _schedulerManager = schedulerManager;
        _pdfService = pdfService;
        _emailDispatcher = emailDispatcher;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboardSummary(CancellationToken ct)
    {
        var summary = await _repository.GetDashboardSummaryAsync(ct);
        var activeWithNextFire = await _schedulerManager.GetAllJobsWithNextFireTimeAsync(ct);

        // Find the earliest upcoming scheduled run
        var nextRun = activeWithNextFire
            .Where(j => j.IsActive && j.NextFireTimeUtc.HasValue)
            .OrderBy(j => j.NextFireTimeUtc)
            .FirstOrDefault()?.NextFireTimeUtc;

        var fullSummary = new SchedulerDashboardSummaryDto(
            TotalJobs: activeWithNextFire.Count,
            ActiveJobs: activeWithNextFire.Count(j => j.IsActive),
            ExecutionsToday: summary.ExecutionsToday,
            FailuresToday: summary.FailuresToday,
            NextScheduledRunUtc: nextRun,
            ActiveJobsList: activeWithNextFire,
            RecentExecutions: summary.RecentExecutions
        );

        return Ok(Result<SchedulerDashboardSummaryDto>.Success(fullSummary));
    }

    [HttpGet("jobs")]
    public async Task<IActionResult> GetAllJobs(CancellationToken ct)
    {
        var jobs = await _schedulerManager.GetAllJobsWithNextFireTimeAsync(ct);
        return Ok(Result<IReadOnlyList<ScheduledReportJobDto>>.Success(jobs));
    }

    [HttpGet("jobs/{id}")]
    public async Task<IActionResult> GetJobById(Guid id, CancellationToken ct)
    {
        var job = await _repository.GetJobByIdAsync(id, ct);
        if (job == null)
        {
            return NotFound(Result<ScheduledReportJobDto>.Failure($"Scheduled job with ID '{id}' was not found."));
        }

        var nextFire = await _schedulerManager.GetNextFireTimeAsync(id, ct);
        var dto = new ScheduledReportJobDto(
            job.Id,
            job.JobName,
            job.JobGroup,
            job.CronExpression,
            job.CronDescription,
            job.ReportType,
            job.RecipientEmailList,
            job.EmailSubjectTemplate,
            job.IsActive,
            job.LastExecutedAtUtc,
            job.LastExecutionStatus,
            job.LastExecutionDurationMs,
            job.ExecutionCount,
            job.FailureCount,
            job.LastErrorMessage,
            nextFire
        );

        return Ok(Result<ScheduledReportJobDto>.Success(dto));
    }

    [HttpPost("jobs")]
    public async Task<IActionResult> CreateJob([FromBody] CreateScheduledJobRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.JobName))
        {
            return BadRequest(Result<ScheduledReportJobDto>.Failure("JobName is required."));
        }

        var existing = await _repository.GetJobByNameAsync(req.JobName, ct);
        if (existing != null)
        {
            return BadRequest(Result<ScheduledReportJobDto>.Failure($"A scheduled job with name '{req.JobName}' already exists."));
        }

        var newJob = new ScheduledReportJob
        {
            JobName = req.JobName,
            JobGroup = req.JobGroup,
            CronExpression = req.CronExpression,
            CronDescription = req.CronDescription,
            ReportType = req.ReportType,
            RecipientEmailList = req.RecipientEmailList,
            EmailSubjectTemplate = req.EmailSubjectTemplate,
            IsActive = req.IsActive
        };

        var created = await _repository.CreateJobAsync(newJob, ct);
        await _schedulerManager.ScheduleOrUpdateJobAsync(created, ct);

        var nextFire = await _schedulerManager.GetNextFireTimeAsync(created.Id, ct);
        var dto = new ScheduledReportJobDto(
            created.Id,
            created.JobName,
            created.JobGroup,
            created.CronExpression,
            created.CronDescription,
            created.ReportType,
            created.RecipientEmailList,
            created.EmailSubjectTemplate,
            created.IsActive,
            created.LastExecutedAtUtc,
            created.LastExecutionStatus,
            created.LastExecutionDurationMs,
            created.ExecutionCount,
            created.FailureCount,
            created.LastErrorMessage,
            nextFire
        );

        return Created($"/api/scheduler/jobs/{created.Id}", Result<ScheduledReportJobDto>.Success(dto));
    }

    [HttpPut("jobs/{id}")]
    public async Task<IActionResult> UpdateJob(Guid id, [FromBody] UpdateScheduledJobRequest req, CancellationToken ct)
    {
        var job = await _repository.GetJobByIdAsync(id, ct);
        if (job == null)
        {
            return NotFound(Result<ScheduledReportJobDto>.Failure($"Scheduled job with ID '{id}' was not found."));
        }

        job.JobName = req.JobName;
        job.CronExpression = req.CronExpression;
        job.CronDescription = req.CronDescription;
        job.ReportType = req.ReportType;
        job.RecipientEmailList = req.RecipientEmailList;
        job.EmailSubjectTemplate = req.EmailSubjectTemplate;
        job.IsActive = req.IsActive;

        var updated = await _repository.UpdateJobAsync(job, ct);
        await _schedulerManager.ScheduleOrUpdateJobAsync(updated, ct);

        var nextFire = await _schedulerManager.GetNextFireTimeAsync(updated.Id, ct);
        var dto = new ScheduledReportJobDto(
            updated.Id,
            updated.JobName,
            updated.JobGroup,
            updated.CronExpression,
            updated.CronDescription,
            updated.ReportType,
            updated.RecipientEmailList,
            updated.EmailSubjectTemplate,
            updated.IsActive,
            updated.LastExecutedAtUtc,
            updated.LastExecutionStatus,
            updated.LastExecutionDurationMs,
            updated.ExecutionCount,
            updated.FailureCount,
            updated.LastErrorMessage,
            nextFire
        );

        return Ok(Result<ScheduledReportJobDto>.Success(dto));
    }

    [HttpDelete("jobs/{id}")]
    public async Task<IActionResult> DeleteJob(Guid id, CancellationToken ct)
    {
        var success = await _repository.DeleteJobAsync(id, ct);
        if (!success)
        {
            return NotFound(Result<bool>.Failure($"Scheduled job with ID '{id}' was not found."));
        }

        return Ok(Result<bool>.Success(true));
    }

    [HttpPost("jobs/{id}/trigger")]
    public async Task<IActionResult> TriggerJobNow(Guid id, CancellationToken ct)
    {
        var success = await _schedulerManager.TriggerJobNowAsync(id, ct);
        if (!success)
        {
            return NotFound(Result<bool>.Failure($"Could not trigger scheduled job '{id}'. Job not found."));
        }

        return Ok(Result<string>.Success($"Manual run successfully triggered for job '{id}'. Execution underway."));
    }

    [HttpPost("jobs/{id}/pause")]
    public async Task<IActionResult> PauseJob(Guid id, CancellationToken ct)
    {
        var success = await _schedulerManager.PauseJobAsync(id, ct);
        if (!success)
        {
            return NotFound(Result<bool>.Failure($"Job '{id}' was not found."));
        }

        return Ok(Result<string>.Success($"Job '{id}' successfully paused in Quartz scheduler."));
    }

    [HttpPost("jobs/{id}/resume")]
    public async Task<IActionResult> ResumeJob(Guid id, CancellationToken ct)
    {
        var success = await _schedulerManager.ResumeJobAsync(id, ct);
        if (!success)
        {
            return NotFound(Result<bool>.Failure($"Job '{id}' was not found."));
        }

        return Ok(Result<string>.Success($"Job '{id}' successfully resumed in Quartz scheduler."));
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetExecutionHistory([FromQuery] Guid? jobId, [FromQuery] int take = 50, CancellationToken ct = default)
    {
        var histories = await _repository.GetExecutionHistoryAsync(jobId, take, ct);
        var dtos = histories.Select(h => new JobExecutionHistoryDto(
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
        )).ToList();

        return Ok(Result<IReadOnlyList<JobExecutionHistoryDto>>.Success(dtos));
    }

    [HttpGet("digest/preview-pdf")]
    public async Task<IActionResult> PreviewExecutiveDigestPdf(CancellationToken ct)
    {
        var model = DailyExecutiveDigestJob.BuildExecutiveDigestModel();
        var pdfBytes = await _pdfService.GenerateDailyExecutiveDigestReportAsync(model, ct);
        return File(pdfBytes, "application/pdf", $"DailyExecutiveDigest_8PM_{DateTime.Today:yyyyMMdd}.pdf");
    }

    [HttpPost("digest/send-test-email")]
    public async Task<IActionResult> SendTestDigestEmail([FromQuery] string recipient, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(recipient))
        {
            return BadRequest(Result<EmailDispatchResult>.Failure("Recipient email address is required."));
        }

        var model = DailyExecutiveDigestJob.BuildExecutiveDigestModel();
        using var pdfStream = await _pdfService.RenderDailyExecutiveDigestToStreamAsync(model, ct);

        var result = await _emailDispatcher.SendReportEmailAsync(
            to: recipient,
            subject: $"[TEST DISPATCH] StitchMind Daily Executive Digest — {DateTime.Today:dd-MMM-yyyy}",
            bodyHtml: "<p>This is a manual test dispatch of the StitchMind Executive Digest via Quartz/MailKit.</p>",
            attachmentStream: pdfStream,
            fileName: $"Daily_Executive_Digest_{DateTime.Today:yyyyMMdd}.pdf",
            ct: ct);

        return Ok(Result<EmailDispatchResult>.Success(result));
    }
}
