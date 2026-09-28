using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Infrastructure.Services;
using GarmentsERP.Reporting.Infrastructure.Workers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Reporting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReportingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IPdfReportService, QuestPdfReportService>();

        // Enable background worker if configured or enabled by default
        var workerConfig = configuration["Reporting:EnableBackgroundWorker"];
        var enableWorker = string.IsNullOrEmpty(workerConfig) || bool.TryParse(workerConfig, out var val) && val;
        if (enableWorker)
        {
            services.AddHostedService<DailyKpiReportBackgroundWorker>();
        }

        return services;
    }
}
