namespace GarmentsERP.Reporting.Application.Models;

public enum JobExecutionStatus
{
    Pending,
    Running,
    Success,
    Failed,
    Skipped
}

public class ScheduledReportJob
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string JobName { get; set; } = string.Empty;
    public string JobGroup { get; set; } = "ManagementReports";
    public string CronExpression { get; set; } = "0 0 20 * * ?"; // 8:00 PM daily
    public string CronDescription { get; set; } = "Every day at 08:00 PM";
    public string ReportType { get; set; } = "DAILY_FACTORY_KPI";
    public string RecipientEmailList { get; set; } = "management@stitchmind.com,coo@stitchmind.com,cfo@stitchmind.com";
    public string EmailSubjectTemplate { get; set; } = "Daily Factory Executive KPI Digest — {Date}";
    public bool IsActive { get; set; } = true;
    public DateTime? LastExecutedAtUtc { get; set; }
    public string LastExecutionStatus { get; set; } = "Pending";
    public long? LastExecutionDurationMs { get; set; }
    public int ExecutionCount { get; set; }
    public int FailureCount { get; set; }
    public string? LastErrorMessage { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAtUtc { get; set; }
}

public class JobExecutionHistory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobId { get; set; }
    public string JobName { get; set; } = string.Empty;
    public string ReportType { get; set; } = string.Empty;
    public DateTime TriggeredAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
    public string Status { get; set; } = "Success";
    public long ExecutionDurationMs { get; set; }
    public string Recipients { get; set; } = string.Empty;
    public long PdfSizeBytes { get; set; }
    public string? ErrorMessage { get; set; }
    public bool IsManualTrigger { get; set; }
}

public record ScheduledReportJobDto(
    Guid Id,
    string JobName,
    string JobGroup,
    string CronExpression,
    string CronDescription,
    string ReportType,
    string RecipientEmailList,
    string EmailSubjectTemplate,
    bool IsActive,
    DateTime? LastExecutedAtUtc,
    string LastExecutionStatus,
    long? LastExecutionDurationMs,
    int ExecutionCount,
    int FailureCount,
    string? LastErrorMessage,
    DateTimeOffset? NextFireTimeUtc
);

public record JobExecutionHistoryDto(
    Guid Id,
    Guid JobId,
    string JobName,
    string ReportType,
    DateTime TriggeredAtUtc,
    DateTime? CompletedAtUtc,
    string Status,
    long ExecutionDurationMs,
    string Recipients,
    long PdfSizeBytes,
    string? ErrorMessage,
    bool IsManualTrigger
);

public record CreateScheduledJobRequest(
    string JobName,
    string JobGroup,
    string CronExpression,
    string CronDescription,
    string ReportType,
    string RecipientEmailList,
    string EmailSubjectTemplate,
    bool IsActive
);

public record UpdateScheduledJobRequest(
    string JobName,
    string CronExpression,
    string CronDescription,
    string ReportType,
    string RecipientEmailList,
    string EmailSubjectTemplate,
    bool IsActive
);

public record SchedulerDashboardSummaryDto(
    int TotalJobs,
    int ActiveJobs,
    int ExecutionsToday,
    int FailuresToday,
    DateTimeOffset? NextScheduledRunUtc,
    IReadOnlyList<ScheduledReportJobDto> ActiveJobsList,
    IReadOnlyList<JobExecutionHistoryDto> RecentExecutions
);

public class EmailDispatchResult
{
    public bool IsSuccess { get; set; }
    public string? MessageId { get; set; }
    public string? ErrorMessage { get; set; }
    public int AttemptsCount { get; set; }
    public long DurationMs { get; set; }
    public DateTime DispatchedAtUtc { get; set; } = DateTime.UtcNow;

    public static EmailDispatchResult Success(string messageId, int attempts, long durationMs) => new()
    {
        IsSuccess = true,
        MessageId = messageId,
        AttemptsCount = attempts,
        DurationMs = durationMs
    };

    public static EmailDispatchResult Failure(string error, int attempts, long durationMs) => new()
    {
        IsSuccess = false,
        ErrorMessage = error,
        AttemptsCount = attempts,
        DurationMs = durationMs
    };
}

public class SmtpConfigurationOptions
{
    public const string SectionName = "SmtpConfiguration";
    public string Host { get; set; } = "smtp.stitchmind.internal";
    public int Port { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string UserName { get; set; } = "automation@stitchmind.com";
    public string Password { get; set; } = "StitchMind#SecureSmtp2026!";
    public string SenderEmail { get; set; } = "reports@stitchmind.com";
    public string SenderName { get; set; } = "StitchMind Enterprise Automated Dispatcher";
    public bool EnableSimulatedDelivery { get; set; } = true; // In development/test, logs delivery without needing live SMTP relay
    public int MaxRetryAttempts { get; set; } = 3;
    public int InitialRetryDelayMs { get; set; } = 1000;
}
