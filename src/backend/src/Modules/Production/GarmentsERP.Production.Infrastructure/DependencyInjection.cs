using GarmentsERP.Production.Application.Interfaces;
using GarmentsERP.Production.Infrastructure.Data;
using GarmentsERP.Production.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Production.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProductionInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=GarmentsERP;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<ProductionDbContext>(options =>
            options.UseSqlServer(connectionString, b =>
                b.MigrationsAssembly(typeof(ProductionDbContext).Assembly.FullName)));

        services.AddScoped<IProductionRepository, ProductionRepository>();

        return services;
    }
}
