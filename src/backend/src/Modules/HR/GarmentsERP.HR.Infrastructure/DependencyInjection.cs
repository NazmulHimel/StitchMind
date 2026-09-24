using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using GarmentsERP.HR.Application.Interfaces;
using GarmentsERP.HR.Infrastructure.Data;
using GarmentsERP.HR.Infrastructure.Repositories;

namespace GarmentsERP.HR.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHrInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=GarmentsERP;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

        services.AddDbContext<HrDbContext>(options =>
            options.UseSqlServer(connectionString, b =>
                b.MigrationsAssembly(typeof(HrDbContext).Assembly.FullName)));

        services.AddScoped<IHrRepository, HrRepository>();

        return services;
    }
}