using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Shared.Extensions;

/// <summary>
/// Enterprise Event-Bus configuration supporting MassTransit with RabbitMQ and In-Memory fallback.
/// Implements Dead-Letter Queues (DLQ), Exponential Backoff Retries, and Clean Architecture modular decoupling.
/// </summary>
public static class EventBusExtensions
{
    public static IServiceCollection AddEnterpriseEventBus(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? configureBus = null)
    {
        services.AddMassTransit(x =>
        {
            // Allow callers to register consumers or sagas
            configureBus?.Invoke(x);

            var eventBusProvider = configuration["EventBus:Provider"] ?? "InMemory";
            var rabbitMqHost = configuration["RabbitMq:Host"];

            // If RabbitMQ is explicitly configured and not forced to InMemory
            if (string.Equals(eventBusProvider, "RabbitMQ", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrWhiteSpace(rabbitMqHost))
            {
                x.UsingRabbitMq((context, cfg) =>
                {
                    var host = rabbitMqHost;
                    var virtualHost = configuration["RabbitMq:VirtualHost"] ?? "/";
                    var username = configuration["RabbitMq:Username"] ?? "guest";
                    var password = configuration["RabbitMq:Password"] ?? "guest";

                    cfg.Host(host, virtualHost, h =>
                    {
                        h.Username(username);
                        h.Password(password);
                    });

                    // Dead Letter Queue (DLQ) & Exponential Retry Policy
                    cfg.UseMessageRetry(retry =>
                    {
                        retry.Exponential(
                            retryLimit: 5,
                            minInterval: TimeSpan.FromMilliseconds(200),
                            maxInterval: TimeSpan.FromSeconds(5),
                            intervalDelta: TimeSpan.FromMilliseconds(200));
                    });

                    // Automatically configure consumer endpoints with _error and _skipped DLQ queues
                    cfg.ConfigureEndpoints(context);
                });
            }
            else
            {
                // In-Memory MassTransit bus (ideal for local dev without Docker, unit/integration testing)
                x.UsingInMemory((context, cfg) =>
                {
                    cfg.UseMessageRetry(retry =>
                    {
                        retry.Interval(3, TimeSpan.FromMilliseconds(100));
                    });

                    cfg.ConfigureEndpoints(context);
                });
            }
        });

        return services;
    }
}
