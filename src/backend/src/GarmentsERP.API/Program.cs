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
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

// Auth Module
builder.Services.AddAuthApplication();
builder.Services.AddAuthInfrastructure(builder.Configuration);

// Procurement Module (Phase 2)
builder.Services.AddProcurementApplication();
builder.Services.AddProcurementInfrastructure(builder.Configuration);

// Inventory & Thread Production Module (Phase 2)
builder.Services.AddInventoryApplication();
builder.Services.AddInventoryInfrastructure(builder.Configuration);

// Production Floor Module (Phase 3)
builder.Services.AddProductionApplication();
builder.Services.AddProductionInfrastructure(builder.Configuration);

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
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    try
    {
        var authContext = services.GetRequiredService<AuthDbContext>();
        var passwordService = services.GetRequiredService<IPasswordService>();
        await AuthDataSeeder.SeedAsync(authContext, passwordService, logger);

        var procContext = services.GetRequiredService<ProcurementDbContext>();
        await ProcurementDataSeeder.SeedAsync(procContext);

        var invContext = services.GetRequiredService<InventoryDbContext>();
        await InventoryDataSeeder.SeedAsync(invContext);

        var prodContext = services.GetRequiredService<ProductionDbContext>();
        await ProductionDataSeeder.SeedAsync(prodContext);
    }
    catch (Exception ex)
    {
        logger.LogWarning("Database seeding skipped or failed: {Message}", ex.Message);
    }
}

app.Run();
