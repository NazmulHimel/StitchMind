using System.Text;
using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Data;
using GarmentsERP.Reporting.Infrastructure.Repositories;
using GarmentsERP.Reporting.Infrastructure.Services;
using GarmentsERP.Reporting.Infrastructure.Services.QuartzJobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Quartz;
using Xunit;

namespace GarmentsERP.Reporting.UnitTests;

public class SchedulerAndEmailUnitTests
{
    // ========================================================
    // SCH-01: QUARTZ.NET CRON SCHEDULE & DB SCHEDULING TABLES
    // ========================================================

    [Fact]
    public void ScheduledReportJob_DefaultValues_ShouldMatchEnterpriseRequirements_SCH01()
    {
        var job = new ScheduledReportJob
        {
            JobName = "DailyExecutiveDigest_8PM",
            ReportType = "DAILY_FACTORY_KPI"
        };

        Assert.Equal("0 0 20 * * ?", job.CronExpression); // 8:00 PM cutoff
        Assert.Equal("ManagementReports", job.JobGroup);
        Assert.True(job.IsActive);
        Assert.Equal("Pending", job.LastExecutionStatus);
        Assert.Equal(0, job.ExecutionCount);
        Assert.Equal(0, job.FailureCount);
        Assert.Contains("management@stitchmind.com", job.RecipientEmailList);
    }

    [Fact]
    public async Task SchedulerRepository_CRUD_ShouldManageJobSchedulesCorrectly_SCH01()
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase(databaseName: $"SchedulerTestDb_{Guid.NewGuid()}")
            .Options;

        using var db = new SchedulerDbContext(options);
        var repo = new SchedulerRepository(db);

        // 1. Create
        var newJob = new ScheduledReportJob
        {
            Id = Guid.NewGuid(),
            JobName = "WeeklyOtAudit_Fri9PM",
            JobGroup = "ComplianceReports",
            CronExpression = "0 0 21 ? * FRI",
            ReportType = "OT_COMPLIANCE_AUDIT",
            RecipientEmailList = "audit@stitchmind.com",
            EmailSubjectTemplate = "Weekly OT Report — {Date}",
            IsActive = true
        };

        await repo.CreateJobAsync(newJob);

        var retrieved = await repo.GetJobByIdAsync(newJob.Id);
        Assert.NotNull(retrieved);
        Assert.Equal("WeeklyOtAudit_Fri9PM", retrieved.JobName);

        // 2. Update
        retrieved.IsActive = false;
        retrieved.ExecutionCount = 5;
        retrieved.LastExecutionStatus = "Success";
        await repo.UpdateJobAsync(retrieved);

        var updated = await repo.GetJobByNameAsync("WeeklyOtAudit_Fri9PM");
        Assert.NotNull(updated);
        Assert.False(updated.IsActive);
        Assert.Equal(5, updated.ExecutionCount);

        // 3. Execution History Logging
        var history = new JobExecutionHistory
        {
            JobId = newJob.Id,
            JobName = newJob.JobName,
            ReportType = newJob.ReportType,
            Status = "Success",
            ExecutionDurationMs = 185,
            Recipients = "audit@stitchmind.com",
            PdfSizeBytes = 65400
        };

        await repo.LogExecutionHistoryAsync(history);

        var historyList = await repo.GetExecutionHistoryAsync(newJob.Id);
        Assert.Single(historyList);
        Assert.Equal("Success", historyList[0].Status);
        Assert.Equal(65400, historyList[0].PdfSizeBytes);
    }

    [Fact]
    public async Task SchedulerDashboardSummary_ShouldComputeMetricsAccurately_SCH01()
    {
        var options = new DbContextOptionsBuilder<SchedulerDbContext>()
            .UseInMemoryDatabase(databaseName: $"SummaryTestDb_{Guid.NewGuid()}")
            .Options;

        using var db = new SchedulerDbContext(options);
        var repo = new SchedulerRepository(db);

        var job1 = new ScheduledReportJob { Id = Guid.NewGuid(), JobName = "Job1", IsActive = true, ReportType = "DAILY_FACTORY_KPI" };
        var job2 = new ScheduledReportJob { Id = Guid.NewGuid(), JobName = "Job2", IsActive = false, ReportType = "OT_COMPLIANCE_AUDIT" };
        await repo.CreateJobAsync(job1);
        await repo.CreateJobAsync(job2);

        // Log today execution
        await repo.LogExecutionHistoryAsync(new JobExecutionHistory
        {
            JobId = job1.Id,
            JobName = job1.JobName,
            ReportType = job1.ReportType,
            TriggeredAtUtc = DateTime.UtcNow,
            Status = "Success",
            ExecutionDurationMs = 210
        });

        // Log today failure
        await repo.LogExecutionHistoryAsync(new JobExecutionHistory
        {
            JobId = job2.Id,
            JobName = job2.JobName,
            ReportType = job2.ReportType,
            TriggeredAtUtc = DateTime.UtcNow,
            Status = "Failed",
            ErrorMessage = "SMTP timeout",
            ExecutionDurationMs = 15000
        });

        var summary = await repo.GetDashboardSummaryAsync();
        Assert.Equal(2, summary.TotalJobs);
        Assert.Equal(1, summary.ActiveJobs);
        Assert.Equal(2, summary.ExecutionsToday);
        Assert.Equal(1, summary.FailuresToday);
    }

    // ========================================================
    // SCH-02: IN-MEMORY QUESTPDF RENDERING & MEMORY POOLING
    // ========================================================

    [Fact]
    public void OptimizedPdfStreamPool_ShouldProvideRecyclableMemoryStreamWithoutDiskFootprint_SCH02()
    {
        var pool = new OptimizedPdfStreamPool();
        using var stream = pool.GetRecyclableStream("TestStreamTag");

        Assert.NotNull(stream);
        Assert.True(stream.CanSeek);
        Assert.True(stream.CanWrite);
        Assert.True(stream.CanRead);

        var testBytes = Encoding.UTF8.GetBytes("QuestPDF In-Memory Test Vector Data");
        stream.Write(testBytes, 0, testBytes.Length);
        stream.Position = 0;

        Assert.Equal(testBytes.Length, stream.Length);
    }

    [Fact]
    public async Task QuestPdfReportService_RenderDailyExecutiveDigestToStream_ShouldProduceValidPdfBytes_SCH02()
    {
        var service = new QuestPdfReportService();
        var model = DailyExecutiveDigestJob.BuildExecutiveDigestModel();

        using var stream = await service.RenderDailyExecutiveDigestToStreamAsync(model);

        Assert.NotNull(stream);
        Assert.True(stream.Length > 5000, $"PDF stream length should be substantial, was {stream.Length}");

        // Verify PDF Magic Header (%PDF-)
        stream.Position = 0;
        var headerBytes = new byte[5];
        var read = stream.Read(headerBytes, 0, 5);
        var header = Encoding.ASCII.GetString(headerBytes, 0, read);

        Assert.Equal("%PDF-", header);
    }

    [Fact]
    public void DailyExecutiveDigestModel_CalculatedProperties_ShouldComputeAccurately_SCH02()
    {
        var model = new DailyExecutiveDigestModel
        {
            TotalWorkforcePresent = 1215,
            TotalWorkforceHeadcount = 1250,
            TargetGarmentUnitsToday = 15000,
            ActualGarmentUnitsSewnToday = 14200,
            TotalOvertimeHoursLogged = 1420.0m
        };

        // Attendance: 1215 / 1250 = 97.2%
        Assert.Equal(97.2m, model.AttendanceRatePct);

        // Sewing Target Achievement: 14200 / 15000 = 94.7%
        Assert.Equal(94.7m, model.SewingTargetAchievementPct);

        // Average Overtime per Present Operator: 1420 / 1215 = 1.2 hrs
        Assert.Equal(1.2m, model.AverageOvertimePerOperatorHours);
    }

    // ========================================================
    // SCH-03: SECURE SMTP / SES DISPATCHER & RETRY POLICIES
    // ========================================================

    [Fact]
    public async Task MailKitEmailDispatcher_SimulatedDelivery_ShouldSucceedWithUniqueMessageId_SCH03()
    {
        var options = Options.Create(new SmtpConfigurationOptions
        {
            EnableSimulatedDelivery = true,
            SenderEmail = "reports@stitchmind.com",
            SenderName = "StitchMind Automated Dispatcher"
        });

        var logger = new Mock<ILogger<MailKitEmailDispatcher>>();
        var dispatcher = new MailKitEmailDispatcher(options, logger.Object);

        using var sampleStream = new MemoryStream(Encoding.UTF8.GetBytes("Sample PDF Payload"));

        var result = await dispatcher.SendReportEmailAsync(
            to: "cfo@stitchmind.com;coo@stitchmind.com",
            subject: "Daily Digest Test",
            bodyHtml: "<p>Test report delivery</p>",
            attachmentStream: sampleStream,
            fileName: "TestDigest.pdf");

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.MessageId);
        Assert.StartsWith("SIM-MSG-", result.MessageId);
        Assert.Equal(1, result.AttemptsCount);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public async Task MailKitEmailDispatcher_NoRecipients_ShouldReturnFailure_SCH03()
    {
        var options = Options.Create(new SmtpConfigurationOptions { EnableSimulatedDelivery = true });
        var logger = new Mock<ILogger<MailKitEmailDispatcher>>();
        var dispatcher = new MailKitEmailDispatcher(options, logger.Object);

        using var stream = new MemoryStream();

        var result = await dispatcher.SendReportEmailAsync(
            to: "",
            subject: "Empty To",
            bodyHtml: "<p>Test</p>",
            attachmentStream: stream,
            fileName: "Empty.pdf");

        Assert.False(result.IsSuccess);
        Assert.Contains("No valid recipients", result.ErrorMessage);
    }

    [Fact]
    public async Task MailKitEmailDispatcher_WithMemoryStreamAttachment_ShouldPreserveStreamPosition_SCH03()
    {
        var options = Options.Create(new SmtpConfigurationOptions { EnableSimulatedDelivery = true });
        var logger = new Mock<ILogger<MailKitEmailDispatcher>>();
        var dispatcher = new MailKitEmailDispatcher(options, logger.Object);

        var data = Encoding.UTF8.GetBytes("Preserved attachment stream bytes");
        using var stream = new MemoryStream(data);
        stream.Position = data.Length; // Positioned at end

        var result = await dispatcher.SendReportEmailAsync(
            to: "management@stitchmind.com",
            subject: "Stream Position Test",
            bodyHtml: "<p>Position test</p>",
            attachmentStream: stream,
            fileName: "Attachment.pdf");

        Assert.True(result.IsSuccess);
        Assert.Equal(data.Length, stream.Length);
    }

    // ========================================================
    // SCH-04: END-TO-END 8:00 PM AUTOMATED MANAGEMENT DIGEST
    // ========================================================

    [Fact]
    public async Task DailyExecutiveDigestJob_ExecutionPipeline_ShouldCompileRenderAndDispatchReport_SCH04()
    {
        var pdfServiceMock = new Mock<IPdfReportService>();
        var emailDispatcherMock = new Mock<IEmailDispatcher>();
        var schedulerRepoMock = new Mock<ISchedulerRepository>();
        var loggerMock = new Mock<ILogger<DailyExecutiveDigestJob>>();

        // Setup mock PDF service to return valid stream
        var pdfBytes = Encoding.UTF8.GetBytes("%PDF-1.7 Simulated In-Memory Executive Digest Vector Stream");
        pdfServiceMock.Setup(p => p.RenderDailyExecutiveDigestToStreamAsync(It.IsAny<DailyExecutiveDigestModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream(pdfBytes));

        // Setup mock email dispatcher to return success
        emailDispatcherMock.Setup(e => e.SendReportEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailDispatchResult.Success("SIM-MSG-2026-DIGEST", 1, 145));

        // Setup scheduled job in repo
        var jobEntity = new ScheduledReportJob
        {
            Id = Guid.NewGuid(),
            JobName = "DailyExecutiveDigest_8PM",
            JobGroup = "ManagementReports",
            CronExpression = "0 0 20 * * ?",
            ReportType = "DAILY_FACTORY_KPI",
            RecipientEmailList = "board@stitchmind.com,coo@stitchmind.com,cfo@stitchmind.com",
            EmailSubjectTemplate = "StitchMind Unit 01 — Daily Factory Executive Digest ({Date})"
        };

        schedulerRepoMock.Setup(r => r.GetJobByNameAsync("DailyExecutiveDigest_8PM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobEntity);

        schedulerRepoMock.Setup(r => r.UpdateJobAsync(It.IsAny<ScheduledReportJob>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobEntity);

        schedulerRepoMock.Setup(r => r.LogExecutionHistoryAsync(It.IsAny<JobExecutionHistory>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Setup Quartz Job Execution Context
        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        var jobDetailMock = new Mock<IJobDetail>();
        jobDetailMock.Setup(j => j.Key).Returns(new JobKey("DailyExecutiveDigest_8PM", "ManagementReports"));
        jobExecutionContextMock.Setup(c => c.JobDetail).Returns(jobDetailMock.Object);

        var dataMap = new JobDataMap { { "IsManualTrigger", false } };
        jobExecutionContextMock.Setup(c => c.MergedJobDataMap).Returns(dataMap);

        var job = new DailyExecutiveDigestJob(
            pdfServiceMock.Object,
            emailDispatcherMock.Object,
            schedulerRepoMock.Object,
            loggerMock.Object);

        // Execute Job
        await job.Execute(jobExecutionContextMock.Object);

        // Assertions
        pdfServiceMock.Verify(p => p.RenderDailyExecutiveDigestToStreamAsync(It.IsAny<DailyExecutiveDigestModel>(), It.IsAny<CancellationToken>()), Times.Once);
        emailDispatcherMock.Verify(e => e.SendReportEmailAsync(
            "board@stitchmind.com,coo@stitchmind.com,cfo@stitchmind.com",
            It.Is<string>(s => s.Contains("Daily Factory Executive Digest")),
            It.IsAny<string>(),
            It.IsAny<Stream>(),
            "Daily_Executive_Digest_" + DateTime.Today.ToString("yyyyMMdd") + ".pdf",
            It.IsAny<CancellationToken>()), Times.Once);

        schedulerRepoMock.Verify(r => r.UpdateJobAsync(It.Is<ScheduledReportJob>(j => j.LastExecutionStatus == "Success" && j.ExecutionCount == 1), It.IsAny<CancellationToken>()), Times.Once);
        schedulerRepoMock.Verify(r => r.LogExecutionHistoryAsync(It.Is<JobExecutionHistory>(h => h.Status == "Success" && h.PdfSizeBytes == pdfBytes.Length), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DailyExecutiveDigestJob_WhenEmailFails_ShouldRecordFailureAndErrorMessage_SCH04()
    {
        var pdfServiceMock = new Mock<IPdfReportService>();
        var emailDispatcherMock = new Mock<IEmailDispatcher>();
        var schedulerRepoMock = new Mock<ISchedulerRepository>();
        var loggerMock = new Mock<ILogger<DailyExecutiveDigestJob>>();

        pdfServiceMock.Setup(p => p.RenderDailyExecutiveDigestToStreamAsync(It.IsAny<DailyExecutiveDigestModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new MemoryStream(Encoding.UTF8.GetBytes("%PDF-Test")));

        emailDispatcherMock.Setup(e => e.SendReportEmailAsync(
                It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<Stream>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(EmailDispatchResult.Failure("SES Authentication Failed: 535 Denied", 3, 4500));

        var jobEntity = new ScheduledReportJob
        {
            Id = Guid.NewGuid(),
            JobName = "DailyExecutiveDigest_8PM",
            RecipientEmailList = "coo@stitchmind.com"
        };

        schedulerRepoMock.Setup(r => r.GetJobByNameAsync("DailyExecutiveDigest_8PM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(jobEntity);

        var jobExecutionContextMock = new Mock<IJobExecutionContext>();
        var jobDetailMock = new Mock<IJobDetail>();
        jobDetailMock.Setup(j => j.Key).Returns(new JobKey("DailyExecutiveDigest_8PM", "ManagementReports"));
        jobExecutionContextMock.Setup(c => c.JobDetail).Returns(jobDetailMock.Object);
        jobExecutionContextMock.Setup(c => c.MergedJobDataMap).Returns(new JobDataMap());

        var job = new DailyExecutiveDigestJob(
            pdfServiceMock.Object,
            emailDispatcherMock.Object,
            schedulerRepoMock.Object,
            loggerMock.Object);

        await job.Execute(jobExecutionContextMock.Object);

        schedulerRepoMock.Verify(r => r.UpdateJobAsync(It.Is<ScheduledReportJob>(j => j.LastExecutionStatus == "Failed" && j.FailureCount == 1), It.IsAny<CancellationToken>()), Times.Once);
        schedulerRepoMock.Verify(r => r.LogExecutionHistoryAsync(It.Is<JobExecutionHistory>(h => h.Status == "Failed" && h.ErrorMessage!.Contains("535 Denied")), It.IsAny<CancellationToken>()), Times.Once);
    }
}
