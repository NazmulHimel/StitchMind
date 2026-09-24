using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Finance.Infrastructure.Data;
using GarmentsERP.Finance.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Finance.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFinanceInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? "Server=localhost;Database=GarmentsERP;Trusted_Connection=True;TrustServerCertificate=True;";

        services.AddDbContext<FinanceDbContext>(options =>
            options.UseSqlServer(connectionString, b => b.MigrationsAssembly(typeof(FinanceDbContext).Assembly.FullName)));

        services.AddScoped<IFinanceRepository, FinanceRepository>();

        return services;
    }
}
