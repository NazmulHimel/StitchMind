using GarmentsERP.Reporting.Application.Models;

namespace GarmentsERP.Reporting.Application.Interfaces;

public interface IQuartzSchedulerManager
{
    Task InitializeScheduledJobsAsync(CancellationToken ct = default);
    Task ScheduleOrUpdateJobAsync(ScheduledReportJob job, CancellationToken ct = default);
    Task<bool> TriggerJobNowAsync(Guid jobId, CancellationToken ct = default);
    Task<bool> PauseJobAsync(Guid jobId, CancellationToken ct = default);
    Task<bool> ResumeJobAsync(Guid jobId, CancellationToken ct = default);
    Task<DateTimeOffset?> GetNextFireTimeAsync(Guid jobId, CancellationToken ct = default);
    Task<IReadOnlyList<ScheduledReportJobDto>> GetAllJobsWithNextFireTimeAsync(CancellationToken ct = default);
}
