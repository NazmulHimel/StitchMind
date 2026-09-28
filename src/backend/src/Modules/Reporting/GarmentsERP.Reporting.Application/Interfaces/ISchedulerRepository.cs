using GarmentsERP.Reporting.Application.Models;

namespace GarmentsERP.Reporting.Application.Interfaces;

public interface ISchedulerRepository
{
    Task<IReadOnlyList<ScheduledReportJob>> GetAllJobsAsync(CancellationToken ct = default);
    Task<ScheduledReportJob?> GetJobByIdAsync(Guid id, CancellationToken ct = default);
    Task<ScheduledReportJob?> GetJobByNameAsync(string jobName, CancellationToken ct = default);
    Task<ScheduledReportJob> CreateJobAsync(ScheduledReportJob job, CancellationToken ct = default);
    Task<ScheduledReportJob> UpdateJobAsync(ScheduledReportJob job, CancellationToken ct = default);
    Task<bool> DeleteJobAsync(Guid id, CancellationToken ct = default);
    Task LogExecutionHistoryAsync(JobExecutionHistory history, CancellationToken ct = default);
    Task<IReadOnlyList<JobExecutionHistory>> GetExecutionHistoryAsync(Guid? jobId = null, int take = 50, CancellationToken ct = default);
    Task<SchedulerDashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken ct = default);
}
