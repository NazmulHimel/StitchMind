using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Production.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddProductionApplication(this IServiceCollection services)
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
