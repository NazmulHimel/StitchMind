using GarmentsERP.Inventory.Application.Interfaces;
using GarmentsERP.Inventory.Infrastructure.Data;
using GarmentsERP.Inventory.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Inventory.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=GarmentsERP;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        services.AddDbContext<InventoryDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(InventoryDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            }));

        services.AddScoped<IInventoryRepository, InventoryRepository>();

        return services;
    }
}
