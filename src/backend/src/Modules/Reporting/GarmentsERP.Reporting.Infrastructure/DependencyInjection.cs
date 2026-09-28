using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Data;
using GarmentsERP.Reporting.Infrastructure.Repositories;
using GarmentsERP.Reporting.Infrastructure.Services;
using GarmentsERP.Reporting.Infrastructure.Services.QuartzJobs;
using GarmentsERP.Reporting.Infrastructure.Workers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace GarmentsERP.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. Database Connection & Context for Scheduled Jobs
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=GarmentsERP;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<SchedulerDbContext>(options =>
            options.UseSqlServer(connectionString, b =>
                b.MigrationsAssembly(typeof(SchedulerDbContext).Assembly.FullName)));

        services.AddScoped<ISchedulerRepository, SchedulerRepository>();

        // 2. SCH-02: Optimized Recyclable MemoryStream Pool (Zero Disk Footprint)
        services.AddSingleton<IOptimizedPdfStreamPool, OptimizedPdfStreamPool>();
        services.AddSingleton<IPdfReportService, QuestPdfReportService>();

        // 3. SCH-03: Smtp Configuration & MailKit Email Dispatcher
        services.Configure<SmtpConfigurationOptions>(options =>
        {
            var section = configuration.GetSection(SmtpConfigurationOptions.SectionName);
            if (section.Exists())
            {
                section.Bind(options);
            }
            else
            {
                // Sensible factory defaults
                options.Host = configuration["Smtp:Host"] ?? "smtp.stitchmind.internal";
                options.Port = int.TryParse(configuration["Smtp:Port"], out var port) ? port : 587;
                options.UseSsl = bool.TryParse(configuration["Smtp:UseSsl"], out var ssl) && ssl;
                options.SenderEmail = configuration["Smtp:SenderEmail"] ?? "reports@stitchmind.com";
                options.SenderName = configuration["Smtp:SenderName"] ?? "StitchMind Automated Digest Dispatcher";
                options.EnableSimulatedDelivery = true;
            }
        });

        services.AddScoped<IEmailDispatcher, MailKitEmailDispatcher>();

        // 4. SCH-01 & SCH-04: Quartz.NET Configuration
        services.AddQuartz(q =>
        {
            // Register execution jobs
            q.AddJob<DailyExecutiveDigestJob>(j => j
                .WithIdentity("DailyExecutiveDigest_8PM", "ManagementReports")
                .StoreDurably()
                .WithDescription("8:00 PM Shiftdown Executive Digest Job"));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        services.AddScoped<IQuartzSchedulerManager, QuartzSchedulerManager>();

        // 5. Quartz Job Initialization & Seed Hosted Service
        services.AddHostedService<QuartzJobInitializerHostedService>();

        return services;
    }
}
