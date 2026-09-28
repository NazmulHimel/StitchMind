# StitchMind — Enterprise Garments Management System
## Master Implementation Plan & Work Breakdown Structure (WBS)

**Document Version:** 1.0.0 (Official Baseline)  
**Status:** Approved & Active  
**Total Development & QA Effort:** 377 Working Days across 36 Sprints (10 Phases)  
**Primary References:** `StitchMind_BRD_v1.0.docx`, `StitchMind_ERP_Task_List_WBS.xlsx`

---

## 1. Executive Summary & Stack Architecture

StitchMind is an enterprise-grade garments manufacturing execution and resource planning system designed to eliminate operational silos across yarn spinning, trims procurement, CAD cutting, IoT sewing line balancing, in-line QA inspection, bonded warehouse tracking, commercial finance, and biometric compliance payroll.

| Attribute | Specification |
|---|---|
| **Backend Core** | ASP.NET Core 10 (Clean Architecture / DDD, CQRS with MediatR) |
| **High-Throughput Reads** | Dapper for floor scanning (<300ms) and Andon queries |
| **Frontend Client** | Next.js 14 (App Router, SSR, TypeScript, Tailwind CSS, Shadcn/UI) |
| **Database Engine** | Microsoft SQL Server 2022+ with EF Core 10 & Change Interceptors |
| **Asynchronous Broker** | RabbitMQ (AMQP) with MassTransit (Exchanges, Queues, DLQ) |
| **Report Generation** | QuestPDF (C# Fluent API, in-memory zero-disk vector generation) |
| **Background Scheduler** | .NET Hosted BackgroundWorker / Quartz.NET + MailKit |
| **Containerization** | Docker & Docker Compose multi-stage services |

---

## 2. 10-Phase Sprint Roadmap & Effort Distribution

```
Phase 0: Architecture, Docker & Core Foundation (Sprint 1-2)       -->  19 Days
Phase 1: Settings, RBAC Security & Master Data (Sprint 3-5)         -->  34 Days
Phase 2: Procurement, Thread Production & Merchandising (Sprint 6-10) --> 55 Days
Phase 3: Stock, Warehouse & Cutting Department (Sprint 11-16)       -->  63 Days
Phase 4: Sewing Floor, IoT/Tablet QC & Line Balancing (Sprint 17-22) --> 64 Days
Phase 5: Finished Goods & Ex-Factory Shipment (Sprint 23-25)        -->  22 Days
Phase 6: Accounting, Finance & Commercial Trade (Sprint 26-30)       -->  50 Days
Phase 7: HR, Compliance & Dual-Stream Payroll (Sprint 31-34)        -->  44 Days
Phase 8: Background Scheduler & Automated Email System (Sprint 35)   -->  13 Days
Phase 9: System Integration, Stress Testing & Go-Live (Sprint 36)    -->  13 Days
---------------------------------------------------------------------------------
TOTAL ESTIMATED EFFORT                                              = 377 Working Days
```

---

## 3. Work Breakdown Structure (All 63 Tasks)

### Phase 0: Architecture, Docker & Core Foundation (Sprint 1–2, 19 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) |
|---|---|---|---|
| **`FND-01`** | Docker environment setup (`mssql`, `rabbitmq`, `api`, `nextjs`) | Docker, Docker Compose | 3 |
| **`FND-02`** | ASP.NET Core 10 Clean Architecture scaffolding & EF Core base repositories | C#, .NET 10 | 5 |
| **`FND-03`** | RabbitMQ Event-Bus configuration (Exchanges, Queues, DLQ, MassTransit) | RabbitMQ, C# | 4 |
| **`FND-04`** | Next.js core scaffolding (App Router, Tailwind, Shadcn/UI layout) | Next.js, TypeScript | 4 |
| **`FND-05`** | QuestPDF base engine setup & background worker service architecture | C#, QuestPDF | 3 |

---

### Phase 1: Settings, RBAC Security & Master Data (Sprint 3–5, 34 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) |
|---|---|---|---|
| **`SEC-01`** | User management, password policies & JWT/MFA authentication | Identity, JWT, NextAuth | 5 |
| **`SEC-02`** | Granular RBAC matrix engine (View, Create, Edit, Delete, Approve, Export) | Custom Claims, Middleware | 6 |
| **`SEC-03`** | Data-level price masking engine for production floor users | DTO Projections, SQL Policy | 4 |
| **`SEC-04`** | Configurable multi-level approval workflow engine | State Machine, Workflow Tables | 7 |
| **`SEC-05`** | Immutable system audit trail (EF Core change interceptor) | EF Core Interceptors, SQL | 4 |
| **`SEC-06`** | Master data directory (Buyer, Style, Color, Size, Fabric, Currency) | Next.js CRUD, Web API | 5 |
| **`SEC-07`** | Reports: Immutable Audit Trail Log & Role-Permission Matrix PDF | QuestPDF Fluent API | 3 |

---

### Phase 2: Procurement, Thread Production & Merchandising (Sprint 6–10, 55 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) |
|---|---|---|---|
| **`PRO-01`** | Material Requisition (MR) creation linked to Buyer Style IDs | Web API, Next.js UI | 5 |
| **`PRO-02`** | Request for Quotation (RFQ) & Comparative Statement (CS) grid | Next.js Dynamic Tables | 5 |
| **`PRO-03`** | Pre-budget validation lock (blocks PO exceeding pre-costing by >0.00%) | Business Rule Validator | 4 |
| **`PRO-04`** | Purchase Order (PO) creation, approvals & status lifecycle | Workflow Engine, Web API | 6 |
| **`PRO-05`** | Commercial import & Back-to-Back (B2B) LC tracking | SQL Ledger, Web API | 6 |
| **`PRO-06`** | Goods Receipt Note (GRN) & Quarantine stock intake handling | Stock Events, RabbitMQ | 5 |
| **`PRO-07`** | Supplier directory, scoring & performance analytics | Analytical Engine, Dapper | 4 |
| **`THP-01`** | Raw cotton/yarn inward, spinning, drawing & winding batch logs | Stock Batches, Web API | 6 |
| **`THP-02`** | Dyeing chemical recipe calculation engine & cost breakdown | C# Recipe Algorithm | 5 |
| **`THP-03`** | Thread tensile/strength testing & cone barcode packaging | Barcode Gen, Web API | 4 |
| **`PRO-RPT`**| Reports: Commercial PO, PO vs GRN Variance & Thread Lab Certificates | QuestPDF Fluent API | 5 |

---

### Phase 3: Stock, Warehouse & Cutting Department (Sprint 11–16, 63 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) |
|---|---|---|---|
| **`WHS-01`** | Bin & Rack 3D coordinate location mapping engine | SQL Tree, Location UI | 5 |
| **`WHS-02`** | Fabric roll barcode/QR tracking (GSM, Cuttable Width, Shade Group) | Zebra ZPL, Barcode API | 6 |
| **`WHS-03`** | Customs Bond register & Utilization Permit (UP) balance engine | Audit Logic, SQL Store | 7 |
| **`WHS-04`** | Store Issue Note (SIN) & Remnant/Cut-piece inventory management | Issue Workflow, Web API | 5 |
| **`WHS-05`** | Physical inventory cycle counting & variance adjustment authorization | Stock Adjustment Engine | 4 |
| **`CUT-01`** | CAD marker file ingestion & planned consumption benchmark parser | Marker Ingestion Parser | 6 |
| **`CUT-02`** | Knit fabric relaxation time-lock & temperature monitoring engine | Time-lock Validators | 4 |
| **`CUT-03`** | Lay planning, spreading logs & stripe/check pattern matching control | Cutting Tables UI | 5 |
| **`CUT-04`** | Automated bundle ticket & barcode generation engine (Thermal/A4) | SkiaSharp, Barcode Gen | 6 |
| **`CUT-05`** | Cut panel numbering, sorting & fusing machine parameter logging | Part Tracker, Web API | 4 |
| **`CUT-06`** | Subcontract cutting delivery & return shortage reconciliation | Subcontract Ledgers | 5 |
| **`WHS-RPT`**| Reports: FIFO Stock Ledger, Bond UP Audit & Planned vs Actual Wastage | QuestPDF Fluent API | 6 |

---

### Phase 4: Sewing Floor, IoT/Tablet QC & Line Balancing (Sprint 17–22, 64 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) |
|---|---|---|---|
| **`IE-01`**  | IE operation bulletin, step breakdown & SMV/SAM allocation | IE Master UI, Web API | 6 |
| **`SEW-01`** | Pitch diagram & real-time line balancing calculation assistant | Calculation Engine | 5 |
| **`SEW-02`** | Sub-second bundle barcode scanning API for floor stations (<300ms) | Dapper, Fast Scanning API | 7 |
| **`SEW-03`** | Real-time floor WIP tracking & bottleneck detection algorithm | RabbitMQ, Redis/Memory | 6 |
| **`SEW-04`** | Digital Andon board WebSocket data feed for overhead line displays | WebSockets, SignalR, UI | 5 |
| **`SEW-05`** | Sewing machine allocation, breakdown tickets & MTTR/MTBF analytics | Asset Maintenance Log | 5 |
| **`SEW-06`** | Broken needle exchange compliance policy & fragment verification log | Compliance Workflow | 3 |
| **`QC-01`**  | Raw fabric 4-Point System inspection engine (Auto Pass/Fail) | 4-Point Math Calculator | 5 |
| **`QC-02`**  | Tablet-friendly In-Line Traffic Light QC audit interface | Touch-first UI, Fast POST | 5 |
| **`QC-03`**  | Visual garment defect heatmap & End-of-Line rejection log | SVG/Canvas Heatmap | 6 |
| **`QC-04`**  | Final pre-shipment AQL sampling engine (ISO 2859-1) & CAPA workflow | Audit Standard Engine | 5 |
| **`SEW-RPT`**| Reports: Hourly Production (HPM), Line Efficiency, Pareto Defect & AQL Cert | QuestPDF Fluent API | 6 |

---

### Phase 5: Finished Goods & Ex-Factory Shipment (Sprint 23–25, 22 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) |
|---|---|---|---|
| **`SHP-01`** | Carton packing list generator with gross weight check validation | Carton Packing Engine | 5 |
| **`SHP-02`** | Finished Goods (FG) inventory warehouse management | Finished Stock Ledger | 4 |
| **`SHP-03`** | Container/truck barcode loading audit & dispatch verification | Barcode Verification | 4 |
| **`SHP-04`** | Commercial Ex-Factory gate pass execution & order 'Shipped' status lock | Status Workflow, Lock API | 5 |
| **`SHP-RPT`**| Reports: Official Ex-Factory Gate Pass, Packing List & Dispatch Dossier | QuestPDF Fluent API | 4 |

---

### Phase 6: Accounting, Finance & Commercial Trade (Sprint 26–30, 50 Days) — *COMPLETED*

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) | Status |
|---|---|---|---|:---:|
| **`ACC-01`** | Multi-layer Chart of Accounts (COA) & automated double-entry vouchers | Double-entry Accounting | 7 | Done |
| **`ACC-02`** | Automated Three-Way Matching (PO + GRN + Vendor Bill verification) | Reconciliation Engine | 7 | Done |
| **`ACC-03`** | Export Master LC, B2B LC settlements & bill discounting (FDBP) ledger | Commercial Trade Module | 8 | Done |
| **`ACC-04`** | Order-wise pre-costing vs actual post-costing margin variance engine | Cost Accounting Logic | 7 | Done |
| **`ACC-05`** | Multi-currency engine (USD/EUR/GBP) with automated Forex gain/loss | FX Conversion Tables | 4 | Done |
| **`ACC-06`** | Fixed asset depreciation engine & withholding tax/VAT (TDS/VDS) deductions | Automated Ledger Rules | 5 | Done |
| **`ACC-07`** | Accounts Payable/Receivable aging analysis & payment disbursement run | Financial Scheduling | 5 | Done |
| **`ACC-RPT`**| Reports: Balance Sheet, P&L, Cash Flow, Trial Balance, AC-06 & Costing Variance | QuestPDF Fluent API | 7 | Done |

---

### Phase 7: HR, Compliance & Dual-Stream Payroll (Sprint 31–34, 44 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) | Status |
|---|---|---|---|:---:|
| **`HR-01`**  | Digital service book & child labor age validation screening (<18 yrs block) | Validation, Doc Store | 5 | Done |
| **`HR-02`**  | Biometric device (ZKTeco/Facial) real-time TCP/IP synchronization listener | TCP/IP Socket Listener | 7 | Done |
| **`HR-03`**  | Dynamic shift rotation, roster scheduling & late deduction penalty engine | Shift Engine, Scheduler | 5 | Done |
| **`HR-04`**  | Earned leave encashment & statutory maternity benefits calculator | Compliance Benefit Math | 5 | Done |
| **`HR-05`**  | Dual-stream payroll (Monthly Staff salary + Floor piece-rate + OT/EOT) | Batch Payroll Engine | 8 | Done |
| **`HR-06`**  | Automated bank & Mobile Financial Services (bKash/Nagad) payout file exporter | Formatted File Exporter | 4 | Done |
| **`HR-07`**  | Occupational health clinic logs, worker grievance box & disciplinary actions | HR Incident Management | 4 | Done |
| **`HR-RPT`** | Reports: Master Salary Sheet, OT Compliance Audit, Bilingual Pay Slips | QuestPDF Fluent API | 6 | Done |

---

### Phase 8: Background Scheduler & Automated Email System (Sprint 35, 13 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) | Status |
|---|---|---|---|---|
| **`SCH-01`** | Quartz.NET cron-trigger manager & database execution schedule tables | Quartz.NET, C# Worker | 4 | Done |
| **`SCH-02`** | In-memory QuestPDF rendering pipeline optimization (`MemoryStream` buffers) | C# Memory Profiling | 3 | Done |
| **`SCH-03`** | Secure SMTP / SES email dispatcher service with automated retry policies | MailKit, Error Handlers | 3 | Done |
| **`SCH-04`** | End-to-End scheduled email test (8:00 PM automated management digest delivery) | Integration Testing | 3 | Done |

---

### Phase 9: System Integration, Stress Testing & Go-Live (Sprint 36, 13 Days)

| Task ID | Task Description & Functional Scope | Implementation Layer | ETA (Days) | Status |
|---|---|---|---|---|
| **`UAT-01`** | End-to-End cross-module lifecycle testing (MR -> PO -> Cut -> Sew -> Ship -> Pay) | Full Team QA | 5 | Done |
| **`UAT-02`** | 2,000+ concurrent floor barcode scans & tablet stress testing | k6 / JMeter Stress Test | 4 | Done |
| **`UAT-03`** | Production Docker deployment setup, reverse proxy & automated DB backups | CI/CD, Production Nginx | 4 | Done |

---

## 4. Definition of Done (DoD) & Acceptance Criteria

Every work package adheres to the 8 core acceptance criteria defined in the BRD:
1. **`AC-01` (Infra):** Zero port collision on multi-container Docker Compose.
2. **`AC-02` (Security):** Price masking active at DTO/query layer for floor roles.
3. **`AC-03` (Procure):** Pre-costing budget lock blocks PO >0.00% without CFO authorization.
4. **`AC-04` (Store):** Inward GRN automatically locks stock in `Quarantine` until QC registers a Pass.
5. **`AC-05` (Floor):** Floor barcode scans execute in under 300ms and push live updates to Andon boards.
6. **`AC-06` (Finance):** Automated 3-way matching reconciles PO + GRN + Supplier Bill prior to payment.
7. **`AC-07` (HR):** Age gate blocks profile creation for any candidate under 18 years.
8. **`AC-08` (Worker):** In-memory QuestPDF worker compiles and sends daily digest via email automatically.
