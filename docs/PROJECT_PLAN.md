# StitchMind — Enterprise Garments Management System
## Master Project Plan

**Version:** 1.0.0
**Date:** 2026-09-24
**Status:** Planning Phase
**Architecture:** Modular Monolithic + CQRS
**Project Directory:** E:\Garments Project\StitchMind

---

## 1. Project Overview

StitchMind is a full-stack, enterprise-grade garments industry management system covering the entire manufacturing lifecycle from raw material procurement to finished goods delivery, with complete financial control, role-based access, and rich PDF reporting.

| Attribute    | Value                                              |
|--------------|----------------------------------------------------|
| Backend      | ASP.NET Core 10 (Modular Monolithic + CQRS)        |
| Frontend     | Next.js 14 (App Router, TypeScript)               |
| Database     | Microsoft SQL Server 2022                         |
| ORM          | Entity Framework Core 10                           |
| Auth         | ASP.NET Core 10 Identity + JWT Bearer                |
| Reports      | QuestPDF                                          |
| Responsive   | Desktop, Tablet, Mobile                           |
| Pattern      | Modular Monolithic + CQRS + Repository Pattern    |

---

## 2. Architecture Overview

```
[Next.js 14 Frontend]
        |  REST API / JWT
[ASP.NET Core 10 API Gateway]
        |
[Modular Monolithic Core]
  +-----------+----------+----------+
  | Auth Mod  | Proc Mod | Prod Mod | ...
  +-----------+----------+----------+
        |
  [CQRS via MediatR]
   Commands -> CommandHandlers -> Write DB
   Queries  -> QueryHandlers  -> Read DB
        |
  [EF Core / Repositories]
        |
  [MSSQL Database]
```

---

## 3. Technology Stack

### Backend
- ASP.NET Core 10 Web API
- MediatR 12 (CQRS Mediator)
- Entity Framework Core 10
- FluentValidation
- ASP.NET Core 10 Identity + JWT
- AutoMapper / Mapster
- Serilog + Seq (Logging)
- QuestPDF (Report Generation)
- xUnit + Moq (Testing)

### Frontend
- Next.js 14 (App Router)
- TypeScript
- shadcn/ui + Radix UI
- Tailwind CSS
- Zustand (State Management)
- TanStack Query (Data Fetching)
- React Hook Form + Zod
- Recharts / Chart.js

### Database
- Microsoft SQL Server 2022
- Schema-per-module design
- EF Core Code-First Migrations

---

## 4. All Application Modules

| #  | Module                         | Priority | Phase |
|----|--------------------------------|----------|-------|
| 01 | Auth & User Management         | Critical | 1     |
| 02 | Role & Permission Management   | Critical | 1     |
| 03 | Organization / Company Setup   | Critical | 1     |
| 04 | Supplier Management            | High     | 2     |
| 05 | Material Procurement (PO/GRN)  | High     | 2     |
| 06 | Thread & Yarn Management       | High     | 2     |
| 07 | Inventory / Stock Management   | High     | 2     |
| 08 | Buyer / Order Management       | High     | 3     |
| 09 | Production Planning            | High     | 3     |
| 10 | Cutting Department             | High     | 3     |
| 11 | Sewing / Stitching Department  | High     | 3     |
| 12 | Washing & Finishing Department | Medium   | 3     |
| 13 | Quality Control (QC)           | High     | 3     |
| 14 | Packing & Dispatch             | Medium   | 4     |
| 15 | HR & Employee Management       | High     | 4     |
| 16 | Attendance & Leave             | Medium   | 4     |
| 17 | Payroll Processing             | High     | 4     |
| 18 | Accounting & Finance           | Critical | 5     |
| 19 | Report Module (QuestPDF)       | High     | 5     |
| 20 | Dashboard & KPIs               | High     | 5     |
| 21 | Audit Log & Activity Tracking  | Medium   | 5     |
| 22 | Notifications & Alerts         | Low      | 6     |
| 23 | Settings & Configuration       | Medium   | 6     |

### Suggested Additional Modules
| Module                 | Business Value                                     |
|------------------------|----------------------------------------------------|
| Sample Management      | Track product samples sent to buyers               |
| Costing / BOM          | Bill of Materials + cost per unit calculation      |
| Shipment / Export      | LC management, export documentation                |
| Machine Maintenance    | Maintenance schedule & breakdown log               |
| Energy Tracking        | Electricity/water/gas usage per department         |
| Compliance & Audit     | Social compliance tracking for buyer audits        |

---

## 5. Development Phases

### Phase 1 — Foundation (Weeks 1-3)
- Backend: Solution scaffold (Modular Monolithic structure)
- Frontend: Next.js setup with auth layout
- Auth module: Login, JWT, Refresh Token, Logout
- Role & Permission module with granular CRUD per module
- Permission-based dynamic sidebar menu (frontend)
- Organization / Company setup

### Phase 2 — Procurement & Inventory (Weeks 4-6)
- Supplier management
- Material Procurement: PO → Approval → GRN → Invoice
- Thread / Yarn production management
- Stock / Inventory module with alerts and re-order logic

### Phase 3 — Production Floor (Weeks 7-10)
- Buyer & Order management
- Production planning & scheduling
- Cutting department (marker, lay plan, cutting order)
- Sewing department (line plan, hourly output, WIP)
- Washing & Finishing department
- QC module (inline, end-line, final inspection)

### Phase 4 – HR & Dispatch (Weeks 11-13) [COMPLETED]
- [x] HR & Employee Management (profiles, compensation structure, line assignments, banking)
- [x] Biometric-Ready Attendance & Shift Management (punch simulator, overtime @ 1.5x)
- [x] Leave Management Workflow (casual, sick, annual, maternity, supervisor approvals)
- [x] Automated Monthly Payroll (1-click batch generation, payslips, deductions & net payable)
- [x] Packing & Export Dispatch (Carton barcode tracking, Delivery Challans & Gate Passes)

### Phase 5 — Finance & Reporting (Weeks 14-16)
- Chart of Accounts, Journal Vouchers, Ledger
- Trial Balance, Profit & Loss, Balance Sheet
- QuestPDF reports for all modules
- Dashboard KPIs and charts

### Phase 6 — Polish & Go-Live (Weeks 17-18)
- Notifications & alerts
- Settings & configurations
- Audit logs
- Performance & load testing
- Deployment (IIS / Docker)

---

## 6. Role Hierarchy

| Role                 | Access Scope                                          |
|----------------------|-------------------------------------------------------|
| Super Admin          | Full system, multi-company management                 |
| Admin                | Full access within own company                        |
| Accounts Manager     | Accounting & Finance module full access               |
| Store Manager        | Inventory, Procurement full access                    |
| Production Manager   | All production department modules                     |
| Cutting Supervisor   | Cutting department only                               |
| Sewing Supervisor    | Sewing department only                                |
| QC Supervisor        | QC module only                                        |
| HR Manager           | HR, Attendance, Payroll                               |
| Dispatch Officer     | Packing & Dispatch                                    |
| Viewer / Auditor     | Read-only access to assigned modules                  |

> Every role has granular CREATE / READ / UPDATE / DELETE permissions per module feature.

---

## 7. Backend Modular Monolithic Solution Structure

```
StitchMind.sln
|
+-- src/
|   +-- StitchMind.API                          (Entry point, Controllers, Middleware)
|   +-- StitchMind.Shared                       (BaseEntity, Result<T>, Exceptions, Extensions)
|   |
|   +-- Modules/
|       +-- Auth/
|       |   +-- StitchMind.Auth.Domain
|       |   +-- StitchMind.Auth.Application     (Commands, Queries, Handlers, DTOs, Validators)
|       |   +-- StitchMind.Auth.Infrastructure  (Repos, EF Configs, Identity)
|       |
|       +-- Procurement/
|       |   +-- StitchMind.Procurement.Domain
|       |   +-- StitchMind.Procurement.Application
|       |   +-- StitchMind.Procurement.Infrastructure
|       |
|       +-- Inventory/
|       |   +-- StitchMind.Inventory.Domain
|       |   +-- StitchMind.Inventory.Application
|       |   +-- StitchMind.Inventory.Infrastructure
|       |
|       +-- Production/
|       |   +-- Cutting/
|       |   |   +-- StitchMind.Production.Cutting.Domain
|       |   |   +-- StitchMind.Production.Cutting.Application
|       |   |   +-- StitchMind.Production.Cutting.Infrastructure
|       |   |
|       |   +-- Sewing/
|       |   +-- Washing/
|       |   +-- QualityControl/
|       |
|       +-- HR/
|       +-- Finance/
|       +-- Reporting/
|
+-- tests/
    +-- StitchMind.Auth.UnitTests
    +-- StitchMind.Procurement.UnitTests
    +-- StitchMind.Integration.Tests
```

---

## 8. CQRS Pattern Example

### Command Side (Write)
```csharp
// Command
public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    DateTime OrderDate,
    List<PurchaseOrderItemDto> Items
) : IRequest<Result<Guid>>;

// Handler
public class CreatePurchaseOrderCommandHandler
    : IRequestHandler<CreatePurchaseOrderCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(
        CreatePurchaseOrderCommand request,
        CancellationToken cancellationToken)
    {
        // validate -> create domain entity -> save -> return id
    }
}
```

### Query Side (Read)
```csharp
// Query
public record GetPurchaseOrdersQuery(int Page, int PageSize)
    : IRequest<Result<PagedList<PurchaseOrderDto>>>;

// Handler
public class GetPurchaseOrdersQueryHandler
    : IRequestHandler<GetPurchaseOrdersQuery, Result<PagedList<PurchaseOrderDto>>>
{
    // reads via Dapper or EF AsNoTracking for performance
}
```

---

## 9. Database Design Strategy

- Schema-per-module: auth, inventory, procurement, production, hr, finance
- All PKs: GUID (uniqueidentifier)
- Soft-delete: IsDeleted, DeletedAt, DeletedBy on all critical tables
- Audit columns: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy on all tables
- No direct cross-module foreign keys (use IDs only across module boundaries)

---

## 10. Report Module (QuestPDF)

| Report                       | Module        |
|------------------------------|---------------|
| Purchase Order (PO)          | Procurement   |
| Goods Received Note (GRN)    | Procurement   |
| Stock Summary / Ledger       | Inventory     |
| Cutting Order Sheet          | Production    |
| Sewing Production Report     | Production    |
| QC Inspection Report         | QC            |
| Delivery Challan             | Dispatch      |
| Salary / Payroll Sheet       | HR            |
| Attendance Report            | HR            |
| General Ledger               | Finance       |
| Trial Balance                | Finance       |
| Profit & Loss Statement      | Finance       |
| Balance Sheet                | Finance       |
| Buyer Order Summary          | Orders        |
| KPI Dashboard PDF Export     | Dashboard     |

---

## 11. API Design Conventions

- Base URL: /api/v1/{module}/{resource}
- HTTP Methods: GET (query), POST (command-create), PUT (command-update), DELETE (command-delete)
- Response wrapper: { success, data, message, errors }
- Pagination: { page, pageSize, totalCount, items }
- Auth header: Authorization: Bearer {token}

---

*Document owned by: StitchMind Project Team*
*Last updated: 2026-09-24*


---

## 12. File Storage Strategy

| Attribute          | Decision                                           |
|--------------------|----------------------------------------------------|
| Phase 1 Storage    | Local Server Disk (D:\StitchMind\Storage\)        |
| Phase 2 Storage    | Azure Blob Storage (future migration)              |
| Abstraction Layer  | IFileStorageService interface (swap with no code change) |
| Files in DB?       | NO — only relative file path stored in DB columns |
| Max file size      | 20 MB per file                                     |
| Allowed types      | .jpg, .png, .pdf, .doc, .docx, .xls, .xlsx, .csv  |
| File serving       | Streamed through authenticated API controller only |
| Disk needed (Yr1)  | ~40 GB (recommend 200 GB dedicated drive)          |

### Storage Folder Layout
```
D:\StitchMind\Storage\
  +-- company\{companyId}\logo\
  +-- employees\{companyId}\{employeeId}\photo|nid|passport|contracts\
  +-- procurement\{companyId}\purchase-orders|grn|invoices\
  +-- production\{companyId}\styles|cutting|compliance\
  +-- hr\{companyId}\attendance\imports\ | payroll\
  +-- reports\{companyId}\generated\ (60-min cache, auto-purged)
  +-- temp\ (daily auto-cleanup)
```

> Full details: `docs/architecture/FILE_STORAGE.md`
