using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Infrastructure.Data;
using GarmentsERP.Auth.Infrastructure.Repositories;
using GarmentsERP.Auth.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GarmentsERP.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? "Server=localhost;Database=GarmentsERP;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true";

        services.AddDbContext<AuthDbContext>(options =>
            options.UseSqlServer(connectionString, sql =>
            {
                sql.MigrationsAssembly(typeof(AuthDbContext).Assembly.FullName);
                sql.EnableRetryOnFailure(maxRetryCount: 3, maxRetryDelay: TimeSpan.FromSeconds(5), errorNumbersToAdd: null);
            }));

        services.AddScoped<IAuthRepository, AuthRepository>();
        services.AddSingleton<IPasswordService, PasswordService>();
        services.AddSingleton<IJwtTokenService, JwtTokenService>();
        services.AddSingleton<IMenuBuilderService, MenuBuilderService>();

        return services;
    }
}
