# GarmentsERP — Architecture Design Document

## Pattern: Modular Monolithic + CQRS

### Why Modular Monolithic?
- Simpler deployment than microservices (single process, single DB)
- Each module is fully encapsulated (own Domain, Application, Infrastructure)
- Modules communicate only via interfaces, never direct class references
- Easy to split into microservices later if needed

### Why CQRS?
- Separates read (queries) from write (commands) — better performance and clarity
- Commands go through strict validation and domain logic
- Queries can use optimized read paths (Dapper, AsNoTracking)
- Scales well as business logic grows

---

## Layer Responsibilities

| Layer          | Responsibility                                                 |
|----------------|----------------------------------------------------------------|
| API Layer      | HTTP controllers, route mapping, auth middleware, Swagger      |
| Application    | Use cases: Commands, Queries, Handlers, DTOs, Validators       |
| Domain         | Entities, Value Objects, Domain Events, Business Rules         |
| Infrastructure | EF Core DbContext, Repositories, Migrations, External services |
| Shared Kernel  | BaseEntity, Result<T>, Pagination, Exceptions, Extensions      |

---

## Cross-Cutting Concerns

| Concern             | Implementation                          |
|--------------------|-----------------------------------------|
| Authentication      | JWT Bearer tokens                       |
| Authorization       | Custom Permission Policy                |
| Logging             | Serilog -> File + Seq                   |
| Validation          | FluentValidation pipeline behaviors     |
| Exception Handling  | Global exception middleware             |
| Caching             | In-memory cache + optional Redis        |
| Soft Delete         | EF Core global query filters            |
| Audit Trails        | EF Core SaveChanges interceptor         |

---

## Module Communication Rules

1. Modules NEVER reference each other s internal classes directly
2. Cross-module data access is done via public interfaces (IModuleService)
3. Domain events may be published via MediatR for loose coupling
4. Shared DB: modules use separate schemas but same SQL Server instance

---

## Security Design

- JWT tokens (15 min expiry) + Refresh Tokens (7 days)
- Refresh tokens stored in DB with device fingerprint
- Password hashing via BCrypt (via ASP.NET Identity)
- Permission claims embedded in JWT
- Every controller endpoint decorated with [HasPermission("Module.Action")]
- API rate limiting via ASP.NET Core rate limiter middleware

---

*Last updated: 2026-09-24*


---

## .NET 10 — Why We Chose It

| Attribute        | Detail                                               |
|-----------------|------------------------------------------------------|
| Release          | November 11, 2025 (GA — Stable)                     |
| Type             | LTS (Long-Term Support)                              |
| Support Until    | November 14, 2028                                    |
| C# Version       | C# 14                                                |
| Target Framework | net10.0                                              |

### Key .NET 10 Advantages for GarmentsERP

| Feature                        | Benefit                                              |
|-------------------------------|------------------------------------------------------|
| Kestrel ~35% faster            | Handles 50+ concurrent factory users efficiently     |
| C# 14 `field` keyword          | Cleaner CQRS command/query record definitions        |
| Built-in OpenAPI support       | No extra Swashbuckle — `AddOpenApi()` built-in       |
| Improved JWT / Identity        | Passkey support, flexible auth policies              |
| Better JIT inlining            | Reduced CPU/memory for IIS self-hosted deployments   |
| EF Core 10 improvements        | Faster query translation, better LINQ support        |
| LTS until 2028                 | No forced upgrade during project lifetime            |

### NuGet Package Targets (net10.0)

| Package                    | Version (approx)  |
|---------------------------|-------------------|
| MediatR                    | 12.x              |
| Microsoft.EntityFrameworkCore.SqlServer | 10.x |
| FluentValidation.AspNetCore | 11.x             |
| AutoMapper                 | 14.x              |
| Serilog.AspNetCore         | 9.x               |
| QuestPDF                   | 2025.x            |
| Swashbuckle / Scalar       | Built-in .NET 10  |

*Last updated: 2026-09-24 (Updated to .NET 10 LTS)*
