using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.HR.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHrApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));
        return services;
    }
}
