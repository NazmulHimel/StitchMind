# StitchMind Module Specification
## Module 09: Granular RBAC, Shop-Floor Price Masking & Scheduled Automation

---

| Metadata Field | Value |
|---|---|
| **Module ID** | `MOD-09` / `Phase I` (Sprint 3–5 & Sprint 35) |
| **BRD Reference** | Section 4, Module 10 (Settings & Security) & Section 5 (QuestPDF Worker) |
| **WBS Reference** | `SEC-01` through `SEC-05`, `SCH-01` through `SCH-04` |
| **Acceptance Criteria** | `AC-02` (Shop-Floor Price Masking), `AC-08` (Scheduled QuestPDF Worker) |
| **Status** | Approved Specification |

---

## 1. Business Purpose & Scope

Information security and operational discipline in an export garments factory require both fine-grained permission controls and strategic data segregation:
1. **Granular RBAC Engine:** Defines permissions across 6 discrete actions: `View`, `Create`, `Edit`, `Delete`, `Approve`, `Export`.
2. **Shop-Floor Financial Price Masking:** Strips material prices, buyer FOB rates, and financial margins from production floor tablet and handheld APIs. Floor supervisors see technical metrics (yardage, plies, sizes) while financial rates remain strictly confidential.
3. **Hierarchical Approval Workflows:** Multi-tiered signing pathways for Purchase Orders, Material Requisitions, and Payment Vouchers based on currency thresholds.
4. **Immutable Audit Trail:** Change interceptors recording every create, update, and delete action with user badge ID, IP address, and previous state.
5. **Scheduled QuestPDF Email Automation:** An in-memory Quartz.NET background worker compiling and dispatching management performance digests at 08:00 PM daily.

---

## 2. Shop-Floor Price Masking Architecture (AC-02)

```
[Client Request: Floor Supervisor on Tablet]
                   │
                   ▼
[ASP.NET Core 10 Web API Pipeline]
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│ ClaimsPrincipal Evaluator:                                  │
│ Does user have claim `Permission: ViewFinancialCosts`?       │
└──────────────────────────┬──────────────────────────────────┘
                           │
             ┌─────────────┴─────────────┐
             │ (HAS CLAIM)               │ (MISSING CLAIM: Floor User)
             ▼                           ▼
┌─────────────────────────┐ ┌─────────────────────────────────┐
│ DTO: Unmasked Full Data │ │ DTO Projection with Masking     │
│ UnitPrice: $2.35 / Yd   │ │ UnitPrice: NULL / [RESTRICTED]  │
│ TotalCost: $86,362.50   │ │ TotalCost: NULL / [RESTRICTED]  │
│ Quantity: 36,750 Yards  │ │ Quantity: 36,750 Yards (Shown)  │
└─────────────────────────┘ └─────────────────────────────────┘
```

---

## 3. Database Schema & Entities

### Schema: `security` & `automation`

```
  ┌──────────────────┐           ┌──────────────────┐
  │       Role       │1         *│    Permission    │
  └────────┬─────────┘           └──────────────────┘
           │1
           │*
  ┌────────▼─────────┐1         *┌──────────────────┐
  │     UserRole     ├───────────►   AuditTrailLog  │
  └──────────────────┘           └──────────────────┘
```

### Table 1: `security.Roles`
* `Id` (Guid, PK)
* `RoleName` (nvarchar(50), Unique, e.g. "SuperAdmin", "ProductionSupervisor", "StoreManager", "AccountsManager")
* `Description` (nvarchar(200))
* `CanViewFinancialCosts` (bit, default 0)  <-- CONTROLS PRICE MASKING
* `IsSystemDefault` (bit, default 0)

### Table 2: `security.RolePermissions`
* `Id` (Guid, PK)
* `RoleId` (Guid, FK)
* `ModuleCode` (nvarchar(30), e.g. "PROCUREMENT", "CUTTING", "SEWING", "FINANCE")
* `CanView` (bit, default 0)
* `CanCreate` (bit, default 0)
* `CanEdit` (bit, default 0)
* `CanDelete` (bit, default 0)
* `CanApprove` (bit, default 0)
* `CanExport` (bit, default 0)

### Table 3: `security.AuditTrailLogs` (Immutable Ledger)
* `Id` (bigint, Identity, PK)
* `EntityName` (nvarchar(100), Index, e.g. "PurchaseOrder", "Employee")
* `EntityId` (varchar(50), Index)
* `ActionType` (nvarchar(20), e.g. `INSERT`, `UPDATE`, `DELETE`, `BUDGET_OVERRIDE`)
* `UserId` (nvarchar(100))
* `UserIpAddress` (varchar(50))
* `OldValuesJson` (nvarchar(max), null)
* `NewValuesJson` (nvarchar(max))
* `TimestampUtc` (datetime2, Index)

### Table 4: `automation.ScheduledReportJobs` (QuestPDF Background Worker)
* `Id` (Guid, PK)
* `JobName` (nvarchar(100), e.g. "DailyExecutiveDigest_8PM")
* `CronExpression` (nvarchar(50), e.g. "0 0 20 * * ?")
* `ReportType` (nvarchar(50), e.g. "DAILY_FACTORY_KPI")
* `RecipientEmailList` (nvarchar(max))
* `LastExecutedAtUtc` (datetime2, null)
* `LastExecutionStatus` (nvarchar(20))
* `IsActive` (bit, default 1)

---

## 4. Key Business Logic: Price Masking & QuestPDF Worker

### A. Data-Level Price Masking Implementation

```csharp
public class CuttingOrderDtoMappingProfile : Profile
{
    public CuttingOrderDtoMappingProfile()
    {
        CreateMap<CuttingOrder, CuttingOrderDto>()
            .ForMember(dest => dest.FabricCostPerYard, opt => opt.MapFrom((src, dest, _, context) =>
            {
                var userClaims = (ClaimsPrincipal)context.Items["CurrentUser"];
                return userClaims.HasClaim("Permission", "ViewFinancialCosts")
                    ? src.FabricCostPerYard
                    : null; // Stripped for floor roles
            }))
            .ForMember(dest => dest.TotalFabricCost, opt => opt.MapFrom((src, dest, _, context) =>
            {
                var userClaims = (ClaimsPrincipal)context.Items["CurrentUser"];
                return userClaims.HasClaim("Permission", "ViewFinancialCosts")
                    ? src.TotalFabricCost
                    : null; // Stripped for floor roles
            }));
    }
}
```

### B. In-Memory QuestPDF Scheduled Worker (`AC-08`)

```csharp
public class DailyExecutiveReportJob : IJob
{
    private readonly IDapperReader _dapper;
    private readonly IEmailDispatcher _email;

    public async Task Execute(IJobExecutionContext context)
    {
        // 1. In-memory data query
        var kpis = await _dapper.QueryDailyKpiAsync(DateTime.Today);

        // 2. Ultra-fast in-memory vector compilation via QuestPDF
        using var stream = new MemoryStream();
        var document = new FactoryKpiPdfDocument(kpis);
        document.GeneratePdf(stream);
        stream.Position = 0;

        // 3. Automated dispatch via SMTP without disk footprint
        await _email.SendReportAsync(
            to: "management@stitchmind.com",
            subject: $"Daily Factory KPI Digest — {DateTime.Today:dd-MMM-yyyy}",
            attachmentStream: stream,
            fileName: $"KPI_Digest_{DateTime.Today:yyyyMMdd}.pdf"
        );
    }
}
```

---

## 5. Production Demo Data: Role Matrix & Price Masking

### A. Core System Role Definitions

| Role | Financial Access (`CanViewFinancialCosts`) | Permissions Summary |
|---|---|---|
| **Super Admin** | **YES** | Full system administration and tenant control |
| **Accounts / CFO**| **YES** | Double-entry finance, AP/AR, 3-way matching, LC ledgers |
| **Merchandiser** | **YES** | Style creation, Pre-costing sheet, BOM definition |
| **Cutting Supervisor**| **NO (Masked)** | Lay planning, CAD markers, bundle printing (Costs stripped) |
| **Sewing Supervisor** | **NO (Masked)** | Line balancing, SMV review, bundle scanning (Costs stripped) |
| **Store Keeper** | **NO (Masked)** | Stock bin allocation, GRN quarantine receipt (Rates stripped) |

### B. Floor Masking Demonstration
When Cutting Supervisor `SUP-CUT-01` opens Cut Order `CUT-2026-0001`:
* **Visible:** Roll Number `ROL-2026-00891`, Yardage: 508.5 Yds, Cuttable Width: 58", Plies: 60.
* **Masked:** Fabric Unit Price: `[CONFIDENTIAL]`, Order Material Budget: `[CONFIDENTIAL]`.
