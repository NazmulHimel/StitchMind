# BUSINESS REQUIREMENT DOCUMENT (BRD)
# StitchMind — Enterprise Garments ERP

---

| Metadata Field       | Value                                                              |
|----------------------|--------------------------------------------------------------------|
| **Document Title**   | Business Requirement Document (BRD) — StitchMind Enterprise Garments ERP |
| **System Name**      | StitchMind (GarmentsERP)                                           |
| **Document Version** | 1.0.0 (Official Baseline)                                          |
| **Status**           | Approved / Active Baseline                                         |
| **Source Documents** | `StitchMind_BRD_v1.0.docx`, `StitchMind_ERP_Task_List_WBS.xlsx`     |
| **Target Platforms** | Cloud / On-Premise Docker Containers (Web, Floor Tablets, Handhelds)|
| **Classification**   | Confidential — Core Enterprise Architectural Baseline              |

---

## TABLE OF CONTENTS

1. [Executive Summary & Vision](#1-executive-summary--vision)
2. [Modern Technology Architecture](#2-modern-technology-architecture)
3. [System Interconnectivity & Data Life-Cycle](#3-system-interconnectivity--data-life-cycle)
4. [Module Specifications & Operational Deliverables (10 Core Modules)](#4-module-specifications--operational-deliverables)
   - [Module 1: Dashboard & Executive Analytics](#module-1-dashboard--executive-analytics)
   - [Module 2: Material Procurement](#module-2-material-procurement)
   - [Module 3: Thread Production (Spinning & Dyeing)](#module-3-thread-production-spinning--dyeing)
   - [Module 4: Stock, Warehouse & Shipment Execution](#module-4-stock-warehouse--shipment-execution)
   - [Module 5: Cutting Department](#module-5-cutting-department)
   - [Module 6: Sewing Floor Management & Line Balancing](#module-6-sewing-floor-management--line-balancing)
   - [Module 7: Quality Control (QC) & Compliance](#module-7-quality-control-qc--compliance)
   - [Module 8: Accounting, Commercial & Finance](#module-8-accounting-commercial--finance)
   - [Module 9: HR, Compliance & Dual-Stream Payroll](#module-9-hr-compliance--dual-stream-payroll)
   - [Module 10: Application Settings, Workflow & Security](#module-10-application-settings-workflow--security)
5. [QuestPDF Background Worker & Email Scheduler](#5-questpdf-background-worker--email-scheduler)
6. [Acceptance Verification Matrix](#6-acceptance-verification-matrix)
7. [Work Breakdown Structure (WBS) & Implementation Roadmap](#7-work-breakdown-structure-wbs--implementation-roadmap)

---

## 1. EXECUTIVE SUMMARY & VISION

**StitchMind** is an enterprise-grade, cloud-native garments manufacturing execution and resource planning system. Engineered specifically for apparel conglomerates and export-oriented factories, it eliminates operational silos by unifying:
- Yarn spinning & raw cotton lot inward
- Trims and fabric procurement
- CAD marker ingestion and cutting floor relaxation gates
- IoT sewing floor line balancing and bundle tracking
- In-line traffic-light QA inspection and AQL audits
- Bonded warehouse tracking & customs Utilization Permit (UP) accounting
- Commercial trade finance (LC, B2B LC, FDBP) & automated 3-way matching
- Biometric compliance payroll (dual-stream: staff salary + floor piece-rate)

By enforcing strict systemic controls — such as budget locks on purchase orders, mandatory quarantine on received raw materials, role-based shop-floor price masking, and automated three-way matching in accounts payable — StitchMind transforms reactive factory operations into a predictive, digitally disciplined ecosystem.

---

## 2. MODERN TECHNOLOGY ARCHITECTURE

| Layer | Technology Selected | Operational Purpose |
|---|---|---|
| **Backend Framework** | .NET Core 10 (C#) | Clean Architecture / DDD, high-throughput transactional APIs, CQRS with MediatR |
| **Frontend Web Client** | Next.js 14 (App Router / SSR) | Low-latency responsive user interfaces across floor tablets, handhelds & desktop |
| **Database Engine** | Microsoft SQL Server 2022+ | ACID transactional reliability with EF Core 10 (write-path) & Dapper (fast read-models) |
| **Asynchronous Broker**| RabbitMQ (AMQP) + MassTransit | Event-driven decoupling for Andon board pushes, alerts, and stock updates |
| **Report Engine** | QuestPDF (C# Fluent API) | Ultra-fast in-memory vector PDF generation without headless browser bloat |
| **Scheduler Service** | .NET Hosted BackgroundWorker / Quartz.NET | Cron-driven automated operational and executive PDF email dispatches |
| **Deployment Pipeline**| Docker & Docker Compose | Isolated multi-stage containerization across cloud and on-premise environments |

### Architectural Principles
1. **Clean Architecture / Domain-Driven Design (DDD):** Domain entities, domain events, and core business rules remain strictly independent of external frameworks and database concerns.
2. **CQRS (Command Query Responsibility Segregation):** Write operations execute via EF Core 10 with change interceptors; high-throughput read operations (floor scanning, Andon boards, report generation) execute via Dapper.
3. **Decoupled Event Bus:** Asynchronous state changes (e.g. `GRNReceivedEvent`, `BundleScannedEvent`, `QCPassedEvent`) are published via RabbitMQ / MassTransit with Dead Letter Queues (DLQ).
4. **Zero-File-System Reporting:** QuestPDF compiles pixel-perfect vector reports in-memory using `MemoryStream` buffers and dispatches them via streaming HTTP or SMTP without writing temporary files to disk.

---

## 3. SYSTEM INTERCONNECTIVITY & DATA LIFE-CYCLE

A foundational principle of StitchMind is that **no module operates in isolation**. The entire operational lifecycle functions as an automated pipeline where transactional events in one department trigger policy-guarded actions in another:

```
[Merchandising Costing] 
       │
       ▼ (Budget Lock Rule: Max 0.00% Variance without CFO override)
[Procurement PO Creation]
       │
       ▼ (Quarantine Gate: Stock locked from floor)
[Warehouse GRN Receipt] 
       │
       ▼ (Pass/Fail 4-Point Audit)
[Quality Control Inspection] ──(Pass)──► [Cutting Floor Relaxation & CAD Marker]
                                                    │
                                                    ▼ (Thermal Bundle Tickets)
                                         [Sewing Line Balancing & Andon Scan]
                                                    │
                                                    ▼ (Carton Packing & AQL)
                                         [Ex-Factory Gate Pass & Shipment]
                                                    │
                                                    ▼ (3-Way Matching: PO + GRN + Invoice)
                                         [Accounts Payable & Trade Finance]
```

### Key Policy Guards:
* **Budget Locks:** Merchandising initiates orders, setting maximum allowable material budgets in the costing engine. Procurement cannot finalize a Purchase Order (PO) if supplier pricing exceeds the pre-costing threshold (>0.00%) without explicit CFO digital authorization.
* **Purchasing & Quarantine:** Warehouse gate entry (GRN) places materials in `Quarantine`; goods are locked from cutting until QC passes them.
* **Floor Execution:** Cutting scans generate bundle tickets; sewing scans register piece-rates and track line bottlenecks in real-time (<300ms scanner response).
* **Finance Release:** Accounts Payable automatically validates PO + GRN + Supplier Bill (Three-Way Match) before authorizing disbursement.
* **Price Masking:** Production floor and cutting operators see quantities and technical specifications, but financial rates and supplier prices are masked.
* **Child Labor / Age Gate:** Onboarding blocks any worker whose birth date indicates an age under 18 years.

---

## 4. MODULE SPECIFICATIONS & OPERATIONAL DELIVERABLES

---

### Module 1: Dashboard & Executive Analytics
* **Live Production & DHU Tracking:** Calculates real-time floor efficiency and defects per hundred units (DHU) at 5-minute intervals.
* **T&A Critical Path Alerts:** Proactive early-warning alerts for order milestones falling behind the Ex-Factory target.
* **Executive Profitability Snapshot:** Masked financial summaries tracking running order profitability and raw material liabilities.
* **Reports Delivered (QuestPDF):** 
  - *Daily Factory KPI Digest*
  - *T&A Critical Deviation Summary*

---

### Module 2: Material Procurement
* **Material Requisitions (MR):** Floor and merchandising demands linked directly to verified Buyer Style IDs.
* **RFQ & Comparative Statement:** Automated multi-supplier pricing, lead time, and payment terms comparative matrix.
* **Pre-Budget Costing Lock:** System blocks PO creation exceeding the merchandising target by >0.00% without CFO authorization.
* **Commercial LC Tracking:** End-to-end logging of Master LCs, Back-to-Back (B2B) LCs, Bill of Entry, and customs dates.
* **Reports Delivered (QuestPDF):** 
  - *Itemized Commercial PO Sheet*
  - *PO vs. GRN Discrepancy Analysis*
  - *Vendor Performance Scorecard*

---

### Module 3: Thread Production (Spinning & Dyeing)
* **Raw Material Lot Inward:** Tracks un-dyed yarn and fiber density, moisture levels, and staple length parameters.
* **Dyeing Recipe Formulation:** Dynamic lab-dip recipe scaling determining precise dye and auxiliary chemical requirements.
* **Quality & Tensile Testing:** Logs tensile strength, elongation at break, twist per inch (TPI), and wash-fastness.
* **Reports Delivered (QuestPDF):** 
  - *Yarn Lot Consumption & Yield Ratio Report*
  - *Dyeing Chemical Costing Sheet*
  - *Thread Quality Certificate*

---

### Module 4: Stock, Warehouse & Shipment Execution
* **3D Bin & Rack Mapping:** Precise coordinate tracking for every fabric roll and accessory carton.
* **Roll-wise Barcode Management:** Every fabric roll records yardage, cuttable width, GSM, and dye shade group.
* **Customs Bond Register:** Strict audit ledger tracking duty-free imported raw materials against Utilization Permits (UP).
* **Ex-Factory Shipment Execution:** Carton weight validation, automated packing lists, and official outward gate passes.
* **Reports Delivered (QuestPDF):** 
  - *Item-wise Stock Ledger (FIFO)*
  - *Customs Bond UP Audit Ledger*
  - *Aged Dead Stock Report*
  - *Ex-Factory Gate Pass*

---

### Module 5: Cutting Department
* **CAD Marker Integration:** Ingestion of nesting marker files (Gerber / Lectra / Optitex) to establish benchmark consumption.
* **Knit Fabric Relaxation Gate:** System locks fabric lots from cutting tables until mandatory relaxation hours (e.g. 24 hrs) elapse.
* **Automated Bundle Ticketing:** Generates thermal/laser barcode tickets for every cut component with size, color, and cut bundle sequence.
* **Reports Delivered (QuestPDF):** 
  - *Daily Cutting Production Log*
  - *CAD Planned vs. Actual Fabric Consumption & Wastage Variance Sheet*

---

### Module 6: Sewing Floor Management & Line Balancing
* **IE Operation Bulletin & SMV:** Breaks down style assembly operations, setting standard SMV/SAM metrics and machinery setups.
* **Real-Time Bundle Tracking:** Instantaneous barcode scanning recording piece-rate outputs and updating line WIP dynamically.
* **Digital Andon & Bottlenecks:** Broadcasts line targets, actual hourly output, and alerts via WebSockets / SignalR to overhead floor displays.
* **Broken Needle Policy:** Enforces full fragment return verification before replacement needles are released for compliance.
* **Reports Delivered (QuestPDF):** 
  - *Hourly Production Monitoring (HPM) Report*
  - *Line Efficiency Trends*
  - *Machine Breakdown & Downtime (MTTR/MTBF)*

---

### Module 7: Quality Control (QC) & Compliance
* **Fabric 4-Point System:** Automated defect penalties per 100 sq. yards yielding immediate Pass/Fail classifications.
* **In-Line Traffic Light QC:** Tablet-optimized interface for roving inspectors auditing bundles (Green, Yellow, Red).
* **AQL Pre-Shipment Engine:** Automated sample size calculation and acceptable defect thresholds under ISO 2859-1 standards.
* **Reports Delivered (QuestPDF):** 
  - *Fabric Roll 4-Point Audit Dossier*
  - *Hourly Defect Pareto Analysis*
  - *Final Pre-Shipment AQL Inspection Certificate*

---

### Module 8: Accounting, Commercial & Finance
* **Trade Finance & Subsidy:** Full Master LC, B2B LC liabilities, export bill discounting (FDBP), and cash incentive subsidy tracking.
* **Order-Wise Costing Variance:** Reconciles actual floor materials and direct labor costs against pre-costing budgets.
* **Three-Way Matching Verification:** Automated validation of PO pricing, GRN quantities, and Vendor Bills before invoice approval.
* **Multi-Currency & Forex Ledger:** Native handling of USD, EUR, and GBP transactions with automated realized/unrealized exchange gain/loss.
* **Reports Delivered (QuestPDF):** 
  - *Balance Sheet*
  - *Profit & Loss (P&L)*
  - *Order-Wise Margin Variance*
  - *Accounts Payable / Receivable Aging Registers*

---

### Module 9: HR, Compliance & Dual-Stream Payroll
* **Digital Service Book & Age Gate:** Comprehensive digital personnel records with strict validation blocking applicants under 18 years.
* **Biometric Device Integration:** Real-time TCP/IP sync with facial/fingerprint clocks handling rotations, shifts, and grace rules.
* **Dual-Structure Payroll:** Calculates monthly staff salaries and floor piece-rate earnings, night allowances, and compliant overtime.
* **Reports Delivered (QuestPDF):** 
  - *Monthly Master Salary Sheet*
  - *Bilingual Pay Slips (Bangla & English)*
  - *Buyers' Social Compliance Audit Pack*

---

### Module 10: Application Settings, Workflow & Security
* **Granular RBAC System:** Configures permissions per resource across six actions: View, Create, Edit, Delete, Approve, Export.
* **Financial Price Masking:** Role-based security hiding fabric prices and vendor costs from floor operators while showing quantities.
* **Hierarchical Approval Engine:** Configurable multi-tier sign-off pathways for POs, requisitions, and payments based on currency thresholds.
* **Reports Delivered (QuestPDF):** 
  - *Immutable System Audit Trail Log*
  - *Role-to-Permission Security Mapping Matrix*

---

## 5. QUESTPDF BACKGROUND WORKER & EMAIL SCHEDULER

StitchMind incorporates a dedicated C# `BackgroundService` inside the `.NET Core 10` runtime. This background worker eliminates manual report compiling by executing on configurable cron triggers (e.g., Daily at 08:00 PM and End-of-Shift).

```
[Quartz.NET Cron Trigger]
           │
           ▼
[Step 1: Data Gathering (Dapper Read-Only Queries)]
           │
           ▼
[Step 2: In-Memory Rendering (QuestPDF MemoryStream)]
           │
           ▼
[Step 3: Automated Dispatch (MailKit SMTP)] ──► Management Mailing Lists
```

1. **Data Gathering:** Reads transactional records using optimized, read-only Dapper queries.
2. **In-Memory Rendering:** Compiles pixel-perfect vector tables, running headers, and page budgets in memory via QuestPDF.
3. **Automated Dispatch:** Dispatches PDF binary streams directly to management mailing lists via secure SMTP without writing temporary files to disk.

---

## 6. ACCEPTANCE VERIFICATION MATRIX

| Ref | Area | Verification Criteria | Expected Operational Outcome |
|---|---|---|---|
| **AC-01** | Infra | Docker Launch | All 4 containers (DB, RabbitMQ, API, Next.js) start up cleanly with zero port collisions. |
| **AC-02** | Security | Price Masking | Floor supervisors cannot view material prices; API strips cost fields for unauthorized claims. |
| **AC-03** | Procure | Budget Lock | PO creation fails if unit price exceeds Merchandising budget by >0.00% without CFO approval. |
| **AC-04** | Store | Quarantine State | Newly received goods via GRN cannot be issued to cutting until QC explicitly enters a Pass status. |
| **AC-05** | Floor | Scanner Response | Sewing bundle barcode reads complete in <300ms, pushing live updates to floor Andon displays. |
| **AC-06** | Finance | Three-Way Match | AP bills cannot be scheduled for payment until PO, GRN, and supplier invoice values align. |
| **AC-07** | HR | Age Compliance | Attempting to onboard a worker with birth date indicating age <18 years blocks profile creation. |
| **AC-08** | Worker | Scheduled PDF | Background worker renders daily performance digest via QuestPDF and delivers it via email on schedule. |

---

## 7. WORK BREAKDOWN STRUCTURE (WBS) & IMPLEMENTATION ROADMAP

The system development is structured into **10 Phases** spanning **36 Sprints** for a total of **377 Development & QA Working Days**.

### Summary of Phases

| Phase | Description | Sprint Span | Days |
|---|---|---|---|
| **Phase 0** | Architecture, Docker & Core Foundation | Sprints 1–2 | 19 Days |
| **Phase 1** | Settings, RBAC Security & Master Data | Sprints 3–5 | 34 Days |
| **Phase 2** | Procurement, Thread Production & Merchandising | Sprints 6–10 | 55 Days |
| **Phase 3** | Stock, Warehouse & Cutting Department | Sprints 11–16 | 63 Days |
| **Phase 4** | Sewing Floor, IoT/Tablet QC & Line Balancing | Sprints 17–22 | 64 Days |
| **Phase 5** | Finished Goods & Ex-Factory Shipment | Sprints 23–25 | 22 Days |
| **Phase 6** | Accounting, Finance & Commercial Trade | Sprints 26–30 | 50 Days |
| **Phase 7** | HR, Compliance & Dual-Stream Payroll | Sprints 31–34 | 44 Days |
| **Phase 8** | Background Scheduler & Automated Email System | Sprint 35 | 13 Days |
| **Phase 9** | System Integration, Stress Testing & Go-Live | Sprint 36 | 13 Days |
| **TOTAL** | **Complete Project Implementation Cycle** | **36 Sprints** | **377 Days** |

---

### Detailed Task List (All 63 Work Packages)

#### Phase 0: Architecture, Docker & Core Foundation (Sprint 1–2, 19 Days)
* **`FND-01`** (3 Days): Docker environment setup (`mssql`, `rabbitmq`, `api`, `nextjs`). Layer: Docker, Docker Compose.
* **`FND-02`** (5 Days): ASP.NET Core 10 Clean Architecture scaffolding & EF Core base repositories. Layer: C#, .NET 10.
* **`FND-03`** (4 Days): RabbitMQ Event-Bus configuration (Exchanges, Queues, DLQ, MassTransit). Layer: RabbitMQ, C#.
* **`FND-04`** (4 Days): Next.js core scaffolding (App Router, Tailwind, Shadcn/UI layout). Layer: Next.js, TypeScript.
* **`FND-05`** (3 Days): QuestPDF base engine setup & background worker service architecture. Layer: C#, QuestPDF.

#### Phase 1: Settings, RBAC Security & Master Data (Sprint 3–5, 34 Days)
* **`SEC-01`** (5 Days): User management, password policies & JWT/MFA authentication. Layer: Identity, JWT, NextAuth.
* **`SEC-02`** (6 Days): Granular RBAC matrix engine (View, Create, Edit, Delete, Approve, Export). Layer: Custom Claims, Middleware.
* **`SEC-03`** (4 Days): Data-level price masking engine for production floor users. Layer: DTO Projections, SQL Policy.
* **`SEC-04`** (7 Days): Configurable multi-level approval workflow engine. Layer: State Machine, Workflow Tables.
* **`SEC-05`** (4 Days): Immutable system audit trail (EF Core change interceptor). Layer: EF Core Interceptors, SQL.
* **`SEC-06`** (5 Days): Master data directory (Buyer, Style, Color, Size, Fabric, Currency). Layer: Next.js CRUD, Web API.
* **`SEC-07`** (3 Days): Reports: Immutable Audit Trail Log & Role-Permission Matrix PDF. Layer: QuestPDF Fluent API.

#### Phase 2: Procurement, Thread Production & Merchandising (Sprint 6–10, 55 Days)
* **`PRO-01`** (5 Days): Material Requisition (MR) creation linked to Buyer Style IDs. Layer: Web API, Next.js UI.
* **`PRO-02`** (5 Days): Request for Quotation (RFQ) & Comparative Statement (CS) grid. Layer: Next.js Dynamic Tables.
* **`PRO-03`** (4 Days): Pre-budget validation lock (blocks PO exceeding pre-costing). Layer: Business Rule Validator.
* **`PRO-04`** (6 Days): Purchase Order (PO) creation, approvals & status lifecycle. Layer: Workflow Engine, Web API.
* **`PRO-05`** (6 Days): Commercial import & Back-to-Back (B2B) LC tracking. Layer: SQL Ledger, Web API.
* **`PRO-06`** (5 Days): Goods Receipt Note (GRN) & Quarantine stock intake handling. Layer: Stock Events, RabbitMQ.
* **`PRO-07`** (4 Days): Supplier directory, scoring & performance analytics. Layer: Analytical Engine, Dapper.
* **`THP-01`** (6 Days): Raw cotton/yarn inward, spinning, drawing & winding batch logs. Layer: Stock Batches, Web API.
* **`THP-02`** (5 Days): Dyeing chemical recipe calculation engine & cost breakdown. Layer: C# Recipe Algorithm.
* **`THP-03`** (4 Days): Thread tensile/strength testing & cone barcode packaging. Layer: Barcode Gen, Web API.
* **`PRO-RPT`** (5 Days): Reports: Commercial PO, PO vs GRN Variance & Thread Lab Certificates. Layer: QuestPDF Fluent API.

#### Phase 3: Stock, Warehouse & Cutting Department (Sprint 11–16, 63 Days)
* **`WHS-01`** (5 Days): Bin & Rack 3D coordinate location mapping engine. Layer: SQL Tree, Location UI.
* **`WHS-02`** (6 Days): Fabric roll barcode/QR tracking (GSM, Cuttable Width, Shade Group). Layer: Zebra ZPL, Barcode API.
* **`WHS-03`** (7 Days): Customs Bond register & Utilization Permit (UP) balance engine. Layer: Audit Logic, SQL Store.
* **`WHS-04`** (5 Days): Store Issue Note (SIN) & Remnant/Cut-piece inventory management. Layer: Issue Workflow, Web API.
* **`WHS-05`** (4 Days): Physical inventory cycle counting & variance adjustment authorization. Layer: Stock Adjustment Engine.
* **`CUT-01`** (6 Days): CAD marker file ingestion & planned consumption benchmark parser. Layer: Marker Ingestion Parser.
* **`CUT-02`** (4 Days): Knit fabric relaxation time-lock & temperature monitoring engine. Layer: Time-lock Validators.
* **`CUT-03`** (5 Days): Lay planning, spreading logs & stripe/check pattern matching control. Layer: Cutting Tables UI.
* **`CUT-04`** (6 Days): Automated bundle ticket & barcode generation engine (Thermal/A4). Layer: SkiaSharp, Barcode Gen.
* **`CUT-05`** (4 Days): Cut panel numbering, sorting & fusing machine parameter logging. Layer: Part Tracker, Web API.
* **`CUT-06`** (5 Days): Subcontract cutting delivery & return shortage reconciliation. Layer: Subcontract Ledgers.
* **`WHS-RPT`** (6 Days): Reports: FIFO Stock Ledger, Bond UP Audit & Planned vs Actual Wastage. Layer: QuestPDF Fluent API.

#### Phase 4: Sewing Floor, IoT/Tablet QC & Line Balancing (Sprint 17–22, 64 Days)
* **`IE-01`** (6 Days): IE operation bulletin, step breakdown & SMV/SAM allocation. Layer: IE Master UI, Web API.
* **`SEW-01`** (5 Days): Pitch diagram & real-time line balancing calculation assistant. Layer: Calculation Engine.
* **`SEW-02`** (7 Days): Sub-second bundle barcode scanning API for floor stations. Layer: Dapper, Fast Scanning API.
* **`SEW-03`** (6 Days): Real-time floor WIP tracking & bottleneck detection algorithm. Layer: RabbitMQ, Redis/Memory.
* **`SEW-04`** (5 Days): Digital Andon board WebSocket data feed for overhead line displays. Layer: WebSockets, SignalR, UI.
* **`SEW-05`** (5 Days): Sewing machine allocation, breakdown tickets & MTTR/MTBF analytics. Layer: Asset Maintenance Log.
* **`SEW-06`** (3 Days): Broken needle exchange compliance policy & fragment verification log. Layer: Compliance Workflow.
* **`QC-01`** (5 Days): Raw fabric 4-Point System inspection engine (Auto Pass/Fail). Layer: 4-Point Math Calculator.
* **`QC-02`** (5 Days): Tablet-friendly In-Line Traffic Light QC audit interface. Layer: Touch-first UI, Fast POST.
* **`QC-03`** (6 Days): Visual garment defect heatmap & End-of-Line rejection log. Layer: SVG/Canvas Heatmap.
* **`QC-04`** (5 Days): Final pre-shipment AQL sampling engine (ISO 2859-1) & CAPA workflow. Layer: Audit Standard Engine.
* **`SEW-RPT`** (6 Days): Reports: Hourly Production (HPM), Line Efficiency, Pareto Defect & AQL Cert. Layer: QuestPDF Fluent API.

#### Phase 5: Finished Goods & Ex-Factory Shipment (Sprint 23–25, 22 Days)
* **`SHP-01`** (5 Days): Carton packing list generator with gross weight check validation. Layer: Carton Packing Engine.
* **`SHP-02`** (4 Days): Finished Goods (FG) inventory warehouse management. Layer: Finished Stock Ledger.
* **`SHP-03`** (4 Days): Container/truck barcode loading audit & dispatch verification. Layer: Barcode Verification.
* **`SHP-04`** (5 Days): Commercial Ex-Factory gate pass execution & order 'Shipped' status lock. Layer: Status Workflow, Lock API.
* **`SHP-RPT`** (4 Days): Reports: Official Ex-Factory Gate Pass, Packing List & Dispatch Dossier. Layer: QuestPDF Fluent API.

#### Phase 6: Accounting, Finance & Commercial Trade (Sprint 26–30, 50 Days)
* **`ACC-01`** (7 Days): Multi-layer Chart of Accounts (COA) & automated double-entry vouchers. Layer: Double-entry Accounting.
* **`ACC-02`** (7 Days): Automated Three-Way Matching (PO + GRN + Vendor Bill verification). Layer: Reconciliation Engine.
* **`ACC-03`** (8 Days): Export Master LC, B2B LC settlements & bill discounting (FDBP) ledger. Layer: Commercial Trade Module.
* **`ACC-04`** (7 Days): Order-wise pre-costing vs actual post-costing margin variance engine. Layer: Cost Accounting Logic.
* **`ACC-05`** (4 Days): Multi-currency engine (USD/EUR/GBP) with automated Forex gain/loss. Layer: FX Conversion Tables.
* **`ACC-06`** (5 Days): Fixed asset depreciation engine & withholding tax/VAT (TDS/VDS) deductions. Layer: Automated Ledger Rules.
* **`ACC-07`** (5 Days): Accounts Payable/Receivable aging analysis & payment disbursement run. Layer: Financial Scheduling.
* **`ACC-RPT`** (7 Days): Reports: Balance Sheet, P&L, Cash Flow, Trial Balance & Order Profitability. Layer: QuestPDF Fluent API.

#### Phase 7: HR, Compliance & Dual-Stream Payroll (Sprint 31–34, 44 Days)
* **`HR-01`** (5 Days): Digital service book & child labor age validation screening (<18 yrs block). Layer: Validation, Doc Store.
* **`HR-02`** (7 Days): Biometric device (ZKTeco/Facial) real-time TCP/IP synchronization listener. Layer: TCP/IP Socket Listener.
* **`HR-03`** (5 Days): Dynamic shift rotation, roster scheduling & late deduction penalty engine. Layer: Shift Engine, Scheduler.
* **`HR-04`** (5 Days): Earned leave encashment & statutory maternity benefits calculator. Layer: Compliance Benefit Math.
* **`HR-05`** (8 Days): Dual-stream payroll (Monthly Staff salary + Floor piece-rate + OT/EOT). Layer: Batch Payroll Engine.
* **`HR-06`** (4 Days): Automated bank & Mobile Financial Services (bKash/Nagad) payout file exporter. Layer: Formatted File Exporter.
* **`HR-07`** (4 Days): Occupational health clinic logs, worker grievance box & disciplinary actions. Layer: HR Incident Management.
* **`HR-RPT`** (6 Days): Reports: Master Salary Sheet, OT Compliance Audit, Bilingual Pay Slips. Layer: QuestPDF Fluent API.

#### Phase 8: Background Scheduler & Automated Email System (Sprint 35, 13 Days)
* **`SCH-01`** (4 Days): Quartz.NET cron-trigger manager & database execution schedule tables. Layer: Quartz.NET, C# Worker.
* **`SCH-02`** (3 Days): In-memory QuestPDF rendering pipeline optimization (`MemoryStream` buffers). Layer: C# Memory Profiling.
* **`SCH-03`** (3 Days): Secure SMTP / SES email dispatcher service with automated retry policies. Layer: MailKit, Error Handlers.
* **`SCH-04`** (3 Days): End-to-End scheduled email test (8:00 PM automated management digest delivery). Layer: Integration Testing.

#### Phase 9: System Integration, Stress Testing & Go-Live (Sprint 36, 13 Days)
* **`UAT-01`** (5 Days): End-to-End cross-module lifecycle testing (MR -> PO -> Cut -> Sew -> Ship -> Pay). Layer: Full Team QA.
* **`UAT-02`** (4 Days): 2,000+ concurrent floor barcode scans & tablet stress testing. Layer: k6 / JMeter Stress Test.
* **`UAT-03`** (4 Days): Production Docker deployment setup, reverse proxy & automated DB backups. Layer: CI/CD, Production Nginx.

---

### End of Business Requirements Document
