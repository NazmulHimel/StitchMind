using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Auth.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(assembly);
        });

        services.AddValidatorsFromAssembly(assembly);

        return services;
    }
}
