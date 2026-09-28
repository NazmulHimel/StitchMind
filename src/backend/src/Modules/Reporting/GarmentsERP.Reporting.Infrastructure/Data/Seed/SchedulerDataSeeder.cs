using GarmentsERP.Reporting.Application.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GarmentsERP.Reporting.Infrastructure.Data.Seed;

public static class SchedulerDataSeeder
{
    public static async Task SeedAsync(SchedulerDbContext context, ILogger logger)
    {
        if (await context.ScheduledReportJobs.AnyAsync())
        {
            return;
        }

        logger.LogInformation("Seeding automated Quartz background report schedules...");

        var defaultJobs = new List<ScheduledReportJob>
        {
            // 1. SCH-04: Daily Executive Digest at 8:00 PM
            new()
            {
                Id = Guid.Parse("e1111111-1111-1111-1111-111111111111"),
                JobName = "DailyExecutiveDigest_8PM",
                JobGroup = "ManagementReports",
                CronExpression = "0 0 20 * * ?", // 8:00 PM every day
                CronDescription = "Every day at 08:00 PM (Shift-down Cutoff)",
                ReportType = "DAILY_FACTORY_KPI",
                RecipientEmailList = "board@stitchmind.com,coo@stitchmind.com,cfo@stitchmind.com,gm@stitchmind.com",
                EmailSubjectTemplate = "StitchMind Unit 01 — Daily Factory Executive Digest ({Date})",
                IsActive = true,
                LastExecutedAtUtc = DateTime.UtcNow.Date.AddHours(20).AddDays(-1),
                LastExecutionStatus = "Success",
                LastExecutionDurationMs = 214,
                ExecutionCount = 42,
                FailureCount = 0
            },

            // 2. Weekly OT Social Compliance Audit Report (Friday 9:00 PM)
            new()
            {
                Id = Guid.Parse("e2222222-2222-2222-2222-222222222222"),
                JobName = "WeeklyOtSocialComplianceAudit",
                JobGroup = "ComplianceReports",
                CronExpression = "0 0 21 ? * FRI", // Every Friday at 9:00 PM
                CronDescription = "Every Friday at 09:00 PM (Weekly OT Audit)",
                ReportType = "OT_COMPLIANCE_AUDIT",
                RecipientEmailList = "compliance@stitchmind.com,hr.director@stitchmind.com,audit@stitchmind.com",
                EmailSubjectTemplate = "StitchMind Factory OT Social Compliance Audit Register — Week {Week}",
                IsActive = true,
                LastExecutedAtUtc = DateTime.UtcNow.Date.AddDays(-3).AddHours(21),
                LastExecutionStatus = "Success",
                LastExecutionDurationMs = 380,
                ExecutionCount = 8,
                FailureCount = 0
            },

            // 3. Hourly Sewing Floor Efficiency & Balancing Digest
            new()
            {
                Id = Guid.Parse("e3333333-3333-3333-3333-333333333333"),
                JobName = "HourlySewingFloorEfficiency",
                JobGroup = "FloorOperations",
                CronExpression = "0 0 8-19 * * ?", // Hourly between 8 AM and 7 PM
                CronDescription = "Hourly between 08:00 AM and 07:00 PM",
                ReportType = "SEWING_EFFICIENCY_DIGEST",
                RecipientEmailList = "production.head@stitchmind.com,ie.lead@stitchmind.com",
                EmailSubjectTemplate = "Hourly Floor Efficiency & Line Balancing Alert — {Time}",
                IsActive = true,
                LastExecutedAtUtc = DateTime.UtcNow.AddMinutes(-45),
                LastExecutionStatus = "Success",
                LastExecutionDurationMs = 125,
                ExecutionCount = 280,
                FailureCount = 1
            },

            // 4. Fabric 4-Point Quarantine & Stock Alert (Morning 8:30 AM)
            new()
            {
                Id = Guid.Parse("e4444444-4444-4444-4444-444444444444"),
                JobName = "FabricQuarantineAlert",
                JobGroup = "QualityReports",
                CronExpression = "0 30 8 * * ?", // Every morning at 8:30 AM
                CronDescription = "Daily at 08:30 AM (Quarantine & Lab Hold)",
                ReportType = "FABRIC_QUARANTINE_ALERT",
                RecipientEmailList = "qa.manager@stitchmind.com,store.keeper@stitchmind.com,procurement@stitchmind.com",
                EmailSubjectTemplate = "Fabric Quarantine & Lab Hold Status Report — {Date}",
                IsActive = true,
                LastExecutedAtUtc = DateTime.UtcNow.Date.AddHours(8).AddMinutes(30),
                LastExecutionStatus = "Success",
                LastExecutionDurationMs = 195,
                ExecutionCount = 35,
                FailureCount = 0
            }
        };

        context.ScheduledReportJobs.AddRange(defaultJobs);

        // Seed sample execution history log for demo/testing
        var sampleHistories = new List<JobExecutionHistory>
        {
            new()
            {
                Id = Guid.NewGuid(),
                JobId = defaultJobs[0].Id,
                JobName = defaultJobs[0].JobName,
                ReportType = defaultJobs[0].ReportType,
                TriggeredAtUtc = DateTime.UtcNow.Date.AddDays(-1).AddHours(20),
                CompletedAtUtc = DateTime.UtcNow.Date.AddDays(-1).AddHours(20).AddMilliseconds(214),
                Status = "Success",
                ExecutionDurationMs = 214,
                Recipients = defaultJobs[0].RecipientEmailList,
                PdfSizeBytes = 84520,
                IsManualTrigger = false
            },
            new()
            {
                Id = Guid.NewGuid(),
                JobId = defaultJobs[0].Id,
                JobName = defaultJobs[0].JobName,
                ReportType = defaultJobs[0].ReportType,
                TriggeredAtUtc = DateTime.UtcNow.Date.AddHours(14).AddMinutes(30),
                CompletedAtUtc = DateTime.UtcNow.Date.AddHours(14).AddMinutes(30).AddMilliseconds(198),
                Status = "Success",
                ExecutionDurationMs = 198,
                Recipients = "coo@stitchmind.com",
                PdfSizeBytes = 84110,
                IsManualTrigger = true
            }
        };

        context.JobExecutionHistories.AddRange(sampleHistories);
        await context.SaveChangesAsync();

        logger.LogInformation("Successfully seeded 4 automated report schedules and execution history.");
    }
}
