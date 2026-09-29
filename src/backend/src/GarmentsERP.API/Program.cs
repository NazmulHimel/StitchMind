using GarmentsERP.Finance.Application;
using GarmentsERP.Finance.Infrastructure;
using GarmentsERP.Finance.Infrastructure.Data;
using GarmentsERP.Finance.Infrastructure.Data.Seed;
using GarmentsERP.HR.Application;
using GarmentsERP.HR.Infrastructure;
using GarmentsERP.HR.Infrastructure.Data;
using GarmentsERP.HR.Infrastructure.Data.Seed;
using System.Text;
using GarmentsERP.API.Middleware;
using GarmentsERP.API.Services;
using GarmentsERP.Auth.Application;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Infrastructure;
using GarmentsERP.Auth.Infrastructure.Data;
using GarmentsERP.Auth.Infrastructure.Data.Seed;
using GarmentsERP.Inventory.Application;
using GarmentsERP.Inventory.Infrastructure;
using GarmentsERP.Inventory.Infrastructure.Data;
using GarmentsERP.Procurement.Application;
using GarmentsERP.Procurement.Infrastructure;
using GarmentsERP.Procurement.Infrastructure.Data;
using GarmentsERP.Production.Application;
using GarmentsERP.Production.Infrastructure;
using GarmentsERP.Production.Infrastructure.Data;
using GarmentsERP.Production.Infrastructure.Data.Seed;
using GarmentsERP.Reporting.Application;
using GarmentsERP.Reporting.Infrastructure;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Extensions;
using GarmentsERP.Shared.Interceptors;
using GarmentsERP.Shared.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// 1. Controller services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();

// 2. Swagger / OpenAPI with JWT Authorization Support
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "GarmentsERP Modular Monolith API",
        Version = "v1",
        Description = "Enterprise Garments Manufacturing & Resource Planning API (.NET 10 CQRS Architecture)"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT"
    });

    c.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer"),
            new List<string>()
        }
    });
});

// 3. Register Core & Module Services
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddSingleton<IFileStorageService, LocalFileStorageService>();
builder.Services.AddSingleton<IAuditTrailStore, InMemoryAuditTrailStore>();
builder.Services.AddScoped<AuditAndSoftDeleteInterceptor>();

// RabbitMQ / MassTransit Enterprise Event-Bus (FND-03)
builder.Services.AddEnterpriseEventBus(builder.Configuration);

// Auth Module
builder.Services.AddAuthApplication();
builder.Services.AddAuthInfrastructure(builder.Configuration);

// Procurement Module (Phase 2)
builder.Services.AddProcurementApplication();
builder.Services.AddProcurementInfrastructure(builder.Configuration);

// Inventory & Thread Production Module (Phase 2)
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

// HR Module (Phase 4)
builder.Services.AddHrApplication();
builder.Services.AddHrInfrastructure(builder.Configuration);

// Finance Module (Phase 5)
builder.Services.AddFinanceApplication();
builder.Services.AddFinanceInfrastructure(builder.Configuration);

// Production Floor Module (Phase 3)
builder.Services.AddProductionApplication();
builder.Services.AddProductionInfrastructure(builder.Configuration);

// Reporting & QuestPDF Background Engine (Phase 0 / FND-05)
builder.Services.AddReportingApplication();
builder.Services.AddReportingInfrastructure(builder.Configuration);

// 4. JWT Authentication
var jwtSecret = builder.Configuration["Jwt:Secret"] 
    ?? "GarmentsERP_SuperSecureSecretKey_ForEnterpriseApplication_2026_Minimum32Bytes!";
var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "GarmentsERP",
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"] ?? "GarmentsERP.Client",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// 5. CORS Configuration
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:3001"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// 6. HTTP Pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "GarmentsERP API v1");
        c.RoutePrefix = string.Empty; // Swagger UI at root "/"
    });
}

app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new
{
    Status = "Healthy",
    System = "GarmentsERP API",
    Timestamp = DateTime.UtcNow,
    Version = "1.0.0"
}));

app.MapControllers();

// 7. Database Seeder
static async Task EnsureSchemaAndTablesCreatedAsync(DbContext db, string schemaName)
{
    if (db.Database.IsRelational())
    {
        try
        {
            await db.Database.ExecuteSqlRawAsync($"IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = '{schemaName}') EXEC('CREATE SCHEMA [{schemaName}]');");
        }
        catch { }

        var creator = db.Database.GetService<IRelationalDatabaseCreator>();
        try
        {
            await creator.CreateTablesAsync();
        }
        catch
        {
            // tables might already exist
        }
    }
    else
    {
        await db.Database.EnsureCreatedAsync();
    }
}

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();

    // Auth Module
    try
    {
        var authContext = services.GetRequiredService<AuthDbContext>();
        try
        {
            await authContext.Database.ExecuteSqlRawAsync(
                "IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('auth.Roles') AND name = 'CanViewFinancialCosts') " +
                "ALTER TABLE [auth].[Roles] ADD [CanViewFinancialCosts] bit NOT NULL CONSTRAINT DF_Roles_CanViewFinancialCosts DEFAULT 1;");
        }
        catch { }

        var passwordService = services.GetRequiredService<IPasswordService>();
        await AuthDataSeeder.SeedAsync(authContext, passwordService, logger);
    }
    catch (Exception ex)
    {
        logger.LogWarning("Auth database seeding skipped or failed: {Message}", ex.Message);
    }

    // Procurement Module
    try
    {
        var procContext = services.GetRequiredService<ProcurementDbContext>();
        await EnsureSchemaAndTablesCreatedAsync(procContext, "procurement");
        await ProcurementDataSeeder.SeedAsync(procContext);
    }
    catch (Exception ex)
    {
        logger.LogWarning("Procurement database seeding skipped or failed: {Message}", ex.Message);
    }

    // Inventory Module
    try
    {
        var invContext = services.GetRequiredService<InventoryDbContext>();
        await EnsureSchemaAndTablesCreatedAsync(invContext, "inventory");
        await InventoryDataSeeder.SeedAsync(invContext);
    }
    catch (Exception ex)
    {
        logger.LogWarning("Inventory database seeding skipped or failed: {Message}", ex.Message);
    }

    // Production Module
    try
    {
        var prodContext = services.GetRequiredService<ProductionDbContext>();
        await EnsureSchemaAndTablesCreatedAsync(prodContext, "production");
        await ProductionDataSeeder.SeedAsync(prodContext);
    }
    catch (Exception ex)
    {
        logger.LogWarning("Production database seeding skipped or failed: {Message}", ex.Message);
    }

    // Finance Module
    try
    {
        var finContext = services.GetRequiredService<FinanceDbContext>();
        await EnsureSchemaAndTablesCreatedAsync(finContext, "finance");
        await FinanceDataSeeder.SeedAsync(finContext, logger);
    }
    catch (Exception ex)
    {
        logger.LogWarning("Finance database seeding skipped or failed: {Message}", ex.Message);
    }

    // HR Module
    try
    {
        var hrContext = services.GetRequiredService<HrDbContext>();
        await EnsureSchemaAndTablesCreatedAsync(hrContext, "hr");
        await HrDataSeeder.SeedAsync(hrContext);
    }
    catch (Exception ex)
    {
        logger.LogWarning("HR database seeding skipped or failed: {Message}", ex.Message);
    }
}

app.Run();
