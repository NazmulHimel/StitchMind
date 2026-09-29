using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Infrastructure.Data;
using GarmentsERP.Reporting.Infrastructure.Data.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GarmentsERP.Reporting.Infrastructure.Workers;

/// <summary>
/// Hosted service that ensures database tables are seeded and Quartz cron schedules are active upon application startup.
/// </summary>
public class QuartzJobInitializerHostedService : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<QuartzJobInitializerHostedService> _logger;

    public QuartzJobInitializerHostedService(
        IServiceProvider serviceProvider,
        ILogger<QuartzJobInitializerHostedService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Starting QuartzJobInitializerHostedService...");

        using var scope = _serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<SchedulerDbContext>();
        var schedulerManager = scope.ServiceProvider.GetRequiredService<IQuartzSchedulerManager>();

        try
        {
            // Ensure DB schema & tables exist even if database was already created
            if (db.Database.IsRelational())
            {
                var creator = db.Database.GetService<Microsoft.EntityFrameworkCore.Storage.IRelationalDatabaseCreator>();
                try
                {
                    await creator.CreateTablesAsync(cancellationToken);
                }
                catch
                {
                    // Tables might already exist
                }
            }
            else
            {
                await db.Database.EnsureCreatedAsync(cancellationToken);
            }

            await SchedulerDataSeeder.SeedAsync(db, _logger);

            // Register with Quartz scheduler
            await schedulerManager.InitializeScheduledJobsAsync(cancellationToken);

            _logger.LogInformation("Quartz background scheduler initialized successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize Quartz background scheduler: {Error}", ex.Message);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
