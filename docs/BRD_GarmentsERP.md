# BUSINESS REQUIREMENTS DOCUMENT (BRD)
# GarmentsERP — Enterprise Garments Industry Management System

---

| Field             | Detail                                          |
|-------------------|-------------------------------------------------|
| Document Title    | Business Requirements Document (BRD)           |
| Project Name      | GarmentsERP                                     |
| Document Version  | 1.0.0                                           |
| Prepared By       | Project Team                                    |
| Review Status     | Draft                                           |
| Created Date      | 2026-09-24                                      |
| Last Updated      | 2026-09-24                                      |
| Classification    | Confidential                                    |

---

## TABLE OF CONTENTS

1. Executive Summary
2. Business Background & Problem Statement
3. Project Objectives & Goals
4. Project Scope
5. Stakeholders
6. Business Assumptions & Constraints
7. User Roles & Permission Matrix
8. Functional Requirements — All 23 Modules
9. Non-Functional Requirements
10. System Architecture Requirements
11. Technology Stack Requirements
12. Database Requirements
13. Security Requirements
14. UI/UX Requirements
15. Integration Requirements
16. Report & Document Requirements
17. Performance Requirements
18. Deployment Requirements
19. Glossary of Terms
20. Approval & Sign-Off

---

## 1. EXECUTIVE SUMMARY

GarmentsERP is a comprehensive, enterprise-grade, web-based ERP system designed specifically for the garments and apparel manufacturing industry. The system digitizes and automates all operational processes — from raw material procurement through the complete production lifecycle to final shipment, payroll, and financial accounting.

The application supports multiple users with distinct roles and permissions, provides a responsive interface across all device types (desktop, tablet, mobile), generates professional PDF reports via QuestPDF, and is built on a modern technology stack:
- **Backend:** ASP.NET Core 10 (Modular Monolithic + CQRS)
- **Frontend:** Next.js 14 (App Router, TypeScript)
- **Database:** Microsoft SQL Server 2022
- **Reports:** QuestPDF

**Key Business Value:**
- Eliminate paper-based processes across all factory departments
- Achieve real-time visibility into production, stock, and finances
- Enforce accountability with role-based access control
- Reduce wastage through data-driven decisions
- Accelerate buyer reporting with automated PDF generation

---

## 2. BUSINESS BACKGROUND & PROBLEM STATEMENT

### 2.1 Business Background

The garments industry involves a complex, multi-department operational flow:
- Raw material procurement from domestic/international suppliers
- Thread, yarn, and accessory management
- Production floor operations (cutting, sewing, washing, finishing)
- Quality control at multiple checkpoints
- HR, payroll, and attendance management
- Financial accounting and reporting
- Buyer order management and shipment tracking

### 2.2 Current Problems

| Problem                                   | Business Impact                                  |
|-------------------------------------------|--------------------------------------------------|
| No centralized data system                | Data silos, inconsistent records                 |
| Manual stock tracking                     | Overstocking, stockouts, material wastage        |
| No production visibility                  | Missed delivery dates, poor buyer relations      |
| Paper-based attendance & payroll          | Errors, fraud, slow processing                   |
| No integrated accounting                  | Inaccurate P&L, delayed financial decisions      |
| Lack of QC tracking                       | High defect rate, buyer rejections               |
| No real-time dashboards                   | Management blind spots                           |
| No audit trails                           | Accountability issues, compliance failures       |

---

## 3. PROJECT OBJECTIVES & GOALS

### 3.1 Primary Objectives

1. Build a single, unified ERP platform covering all factory operations
2. Implement a secure, role-based multi-user access system
3. Digitize the complete garments production lifecycle
4. Provide real-time inventory and stock management
5. Automate payroll calculation and HR operations
6. Deliver a complete double-entry accounting module
7. Generate professional PDF reports for all business documents
8. Provide actionable KPI dashboards for management

### 3.2 SMART Goals

| Goal                               | Target                                    |
|------------------------------------|-------------------------------------------|
| Reduce manual data entry time      | By 80% within 3 months of go-live        |
| Achieve real-time stock accuracy   | 99%+ accuracy vs physical count          |
| Reduce payroll processing time     | From 3 days to 4 hours per month         |
| QC defect tracking                 | 100% digital, zero paper records         |
| Report generation time             | Under 5 seconds for any PDF report       |
| System availability                | 99.5% uptime during business hours       |

---

## 4. PROJECT SCOPE

### 4.1 In Scope

| Area                    | Included Features                                              |
|-------------------------|----------------------------------------------------------------|
| Authentication          | Login, logout, JWT, refresh tokens, password reset             |
| User & Role Management  | Multi-user, role-based permissions per module                  |
| Procurement             | Supplier management, PO, GRN, invoice matching                 |
| Inventory               | Multi-warehouse stock, movements, alerts                       |
| Thread/Yarn             | Thread-specific procurement and issue tracking                 |
| Production              | Cutting, sewing, washing, finishing, QC                        |
| Buyer Orders            | Order management, BOM, delivery schedule                       |
| HR & Payroll            | Employee management, attendance, leave, payroll                |
| Finance                 | Full double-entry accounting, ledger, financial statements     |
| Reports                 | QuestPDF-based PDF for all major documents                     |
| Dashboard               | KPI cards, trend charts, department summaries                  |
| Audit Logs              | All data changes tracked                                       |
| Notifications           | In-app alerts for critical events                              |
| Responsive UI           | Works on desktop, tablet, and mobile                           |

### 4.2 Out of Scope (Version 1.0)

| Feature                         | Reason                                       |
|---------------------------------|----------------------------------------------|
| Mobile native app (iOS/Android) | Future phase (PWA covers mobile needs)       |
| AI/ML demand forecasting        | Future enhancement                           |
| Multi-currency accounting       | Future phase (single currency v1)            |
| E-commerce integration          | Out of current business scope                |
| IoT machine integration         | Future enhancement                           |
| ERP integration (SAP/Oracle)    | Not required currently                       |

---

## 5. STAKEHOLDERS

| Stakeholder              | Role                                            | Priority |
|--------------------------|-------------------------------------------------|----------|
| Factory Owner / MD       | Project sponsor, final approval                 | High     |
| General Manager          | Business requirements provider                  | High     |
| Production Manager       | Production module requirements                  | High     |
| Store Manager            | Inventory, procurement requirements             | High     |
| Accounts Manager         | Finance module requirements                     | High     |
| HR Manager               | HR, payroll requirements                        | High     |
| QC Manager               | QC module requirements                          | Medium   |
| Department Supervisors   | Day-to-day users, UAT testing                   | Medium   |
| IT Administrator         | System deployment, user management              | High     |
| Buyers                   | Order, QC reports, shipment docs                | Medium   |
| Suppliers                | PO receipt, invoice submission                  | Low      |

---

## 6. BUSINESS ASSUMPTIONS & CONSTRAINTS

### 6.1 Assumptions

1. Factory has stable internet connection for web application access
2. Users have modern browsers (Chrome, Edge, Firefox)
3. Microsoft SQL Server 2022 license will be procured
4. Windows Server available for hosting backend API
5. All departments will participate in UAT
6. Data migration from Excel/manual records handled separately
7. Training provided to all end users before go-live
8. Single currency (BDT) for version 1.0

### 6.2 Constraints

| Constraint          | Detail                                                  |
|---------------------|---------------------------------------------------------|
| Technology          | Must use ASP.NET Core 10, Next.js 14, MSSQL              |
| Report Library      | Must use QuestPDF only                                  |
| Architecture        | Modular Monolithic + CQRS mandatory                     |
| Timeline            | 18 weeks (6 phases)                                     |
| Responsive Design   | Must work on all device sizes                           |
| Security            | JWT authentication is mandatory                         |


---

## 7. USER ROLES & PERMISSION MATRIX

### 7.1 Role Definitions

| Role                 | Description                                                   |
|----------------------|---------------------------------------------------------------|
| Super Admin          | Full system control, company/tenant management                |
| Admin                | Full access within own company, manages users                 |
| Accounts Manager     | Full access to Finance and Accounting module                  |
| Store Manager        | Full access to Procurement and Inventory modules              |
| Production Manager   | Full access to all Production department modules              |
| Cutting Supervisor   | Access to Cutting Department module only                      |
| Sewing Supervisor    | Access to Sewing Department module only                       |
| QC Supervisor        | Access to QC module only                                      |
| HR Manager           | Full access to HR, Attendance, Leave, and Payroll             |
| Dispatch Officer     | Access to Packing and Dispatch module                         |
| Viewer / Auditor     | Read-only access to assigned modules                          |

### 7.2 Permission Types
- **C** = Create  
- **R** = Read  
- **U** = Update  
- **D** = Delete  
- **E** = Export PDF  

### 7.3 Permission Matrix

| Module               | Super Admin | Admin | Acct Mgr | Store Mgr | Prod Mgr | HR Mgr | Viewer |
|----------------------|-------------|-------|----------|-----------|----------|--------|--------|
| Auth / Users         | CRUDE       | CRUDE | R        | R         | R        | R      | R      |
| Roles & Permissions  | CRUDE       | CRUDE | -        | -         | -        | -      | -      |
| Company Setup        | CRUDE       | CRUDE | R        | R         | R        | R      | R      |
| Suppliers            | CRUDE       | CRUDE | R        | CRUDE     | R        | -      | R      |
| Procurement          | CRUDE       | CRUDE | R        | CRUDE     | R        | -      | R      |
| Inventory            | CRUDE       | CRUDE | R        | CRUDE     | CRUDE    | -      | R      |
| Thread Management    | CRUDE       | CRUDE | R        | CRUDE     | CRUDE    | -      | R      |
| Buyer Orders         | CRUDE       | CRUDE | R        | R         | CRUDE    | -      | R      |
| Production Planning  | CRUDE       | CRUDE | -        | R         | CRUDE    | -      | R      |
| Cutting Dept         | CRUDE       | CRUDE | -        | R         | CRUDE    | -      | R      |
| Sewing Dept          | CRUDE       | CRUDE | -        | R         | CRUDE    | -      | R      |
| Washing Dept         | CRUDE       | CRUDE | -        | R         | CRUDE    | -      | R      |
| QC                   | CRUDE       | CRUDE | -        | R         | CRUDE    | -      | R      |
| Packing & Dispatch   | CRUDE       | CRUDE | R        | R         | CRUDE    | -      | R      |
| HR & Employees       | CRUDE       | CRUDE | R        | -         | R        | CRUDE  | R      |
| Attendance & Leave   | CRUDE       | CRUDE | R        | -         | R        | CRUDE  | R      |
| Payroll              | CRUDE       | CRUDE | R        | -         | -        | CRUDE  | R      |
| Accounting           | CRUDE       | CRUDE | CRUDE    | -         | -        | -      | R      |
| Reports              | CRUDE       | CRUDE | CRUDE    | CRUDE     | CRUDE    | CRUDE  | RE     |
| Dashboard            | CRUDE       | CRUDE | RE       | RE        | RE       | RE     | R      |
| Audit Logs           | R           | R     | -        | -         | -        | -      | -      |
| Notifications        | CRUDE       | CRUDE | R        | R         | R        | R      | R      |
| Settings             | CRUDE       | CRUDE | R        | R         | R        | R      | -      |

---

## 8. FUNCTIONAL REQUIREMENTS — ALL MODULES

---

### MODULE 8.01: AUTH & USER MANAGEMENT

**Business Purpose:** Secure access control for all system users.

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| AU-001  | Users log in with username/email + password                               | Critical |
| AU-002  | Issue JWT access token (15-minute expiry) on successful login             | Critical |
| AU-003  | Issue refresh token (7-day expiry) for session continuity                 | Critical |
| AU-004  | Refresh tokens revoked on logout                                          | Critical |
| AU-005  | Forgot password via email OTP/link                                        | High     |
| AU-006  | Password complexity: min 8 chars, uppercase, lowercase, number            | High     |
| AU-007  | Account lockout after 5 failed attempts (30 min lockout)                  | High     |
| AU-008  | Admin can create, edit, activate, and deactivate user accounts            | Critical |
| AU-009  | User profile: name, email, phone, avatar, department                      | High     |
| AU-010  | Each user must belong to at least one role                                | Critical |
| AU-011  | All login/logout events logged with IP address and timestamp              | High     |
| AU-012  | "Remember Me" option for extended session                                 | Medium   |

**Business Rules:**
- Deactivated users cannot log in
- Super Admin accounts cannot be deactivated by any other user
- Password change enforced every 90 days (configurable)

---

### MODULE 8.02: ROLE & PERMISSION MANAGEMENT

**Business Purpose:** Fine-grained, module-level access control.

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| RP-001  | Admin can create and name custom roles                                    | Critical |
| RP-002  | Permissions defined per module: Create, Read, Update, Delete, Export     | Critical |
| RP-003  | Admin can assign/revoke permissions to roles                              | Critical |
| RP-004  | Admin can assign one or more roles to each user                           | Critical |
| RP-005  | Frontend sidebar dynamically renders only permitted modules               | Critical |
| RP-006  | Each page/action validates permission before rendering                    | Critical |
| RP-007  | System-default roles cannot be deleted                                    | High     |
| RP-008  | Permission changes take effect on next login or token refresh             | High     |
| RP-009  | Admin can clone an existing role as a starting template                   | Medium   |

---

### MODULE 8.03: ORGANIZATION / COMPANY SETUP

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| CO-001  | Admin configures company name, logo, address, contact details             | Critical |
| CO-002  | Financial year start and end date configuration                           | Critical |
| CO-003  | Currency and default unit of measure setup                                | High     |
| CO-004  | Create and manage departments                                             | Critical |
| CO-005  | Create designations/job titles                                            | High     |
| CO-006  | Company logo appears on all generated PDF reports                         | High     |

---

### MODULE 8.04: SUPPLIER MANAGEMENT

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| SM-001  | Create supplier profiles                                                  | Critical |
| SM-002  | Profile includes: name, code, type, address, contact, trade license       | Critical |
| SM-003  | Categorize suppliers (fabric, thread, accessories, packaging)             | High     |
| SM-004  | Store supplier bank details for payment processing                        | High     |
| SM-005  | Track supplier outstanding balance in real time                           | High     |
| SM-006  | Show complete purchase history per supplier                               | High     |
| SM-007  | Support supplier deactivation (soft delete only)                          | High     |
| SM-008  | Search suppliers by name, code, category                                  | High     |
| SM-009  | Generate supplier statement report (QuestPDF)                             | Medium   |

---

### MODULE 8.05: MATERIAL PROCUREMENT (PO / GRN / INVOICE)

**Business Process Flow:**
```
Purchase Requisition -> Purchase Order -> Approval -> GRN -> Supplier Invoice -> Payment
```

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| PR-001  | Create Purchase Requisitions                                              | Critical |
| PR-002  | POs created referencing a requisition or standalone                       | Critical |
| PR-003  | PO includes: supplier, date, delivery date, items (material, qty, unit, rate) | Critical |
| PR-004  | PO supports multi-item, multi-unit entries                                | Critical |
| PR-005  | PO goes through configurable approval workflow                            | High     |
| PR-006  | Auto-generate PO number (e.g., PO-2026-0001)                             | Critical |
| PR-007  | GRN created against an approved PO                                        | Critical |
| PR-008  | GRN supports partial delivery (multiple GRNs per PO)                     | High     |
| PR-009  | GRN creation automatically updates stock in Inventory module              | Critical |
| PR-010  | Supplier Invoice matched against GRN (3-way matching)                    | High     |
| PR-011  | PO status tracking: Draft, Pending Approval, Approved, Partial, Closed   | Critical |
| PR-012  | Generate Purchase Order report (QuestPDF)                                 | High     |
| PR-013  | Generate GRN report (QuestPDF)                                            | High     |
| PR-014  | PO cancellation with reason                                               | High     |
| PR-015  | Pending POs and overdue deliveries shown on dashboard                     | High     |

---

### MODULE 8.06: THREAD & YARN MANAGEMENT

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| TY-001  | Thread/yarn catalogue (type, count, color, supplier)                      | Critical |
| TY-002  | Thread procurement follows same PO/GRN cycle                              | Critical |
| TY-003  | Thread stock tracked by color, count, and cone/kg                         | Critical |
| TY-004  | Thread requisition from production departments                            | Critical |
| TY-005  | Track thread issue per order/style per department                         | High     |
| TY-006  | Calculate thread consumption standard per style                           | Medium   |
| TY-007  | Thread re-order alerts based on configured minimum stock level            | High     |

---

### MODULE 8.07: INVENTORY / STOCK MANAGEMENT

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| IN-001  | Material master catalogue with categories                                 | Critical |
| IN-002  | Multi-warehouse/store locations support                                   | High     |
| IN-003  | Stock-IN triggers on GRN approval                                         | Critical |
| IN-004  | Stock-OUT triggers on departmental issue voucher                          | Critical |
| IN-005  | Manual stock adjustment with reason                                       | High     |
| IN-006  | Stock transfer between warehouses                                         | High     |
| IN-007  | Full stock ledger (all movements per material)                            | Critical |
| IN-008  | FIFO or Weighted Average costing (configurable)                           | High     |
| IN-009  | Re-order alerts when stock reaches minimum level                          | High     |
| IN-010  | Current stock value by material and warehouse                             | Critical |
| IN-011  | Stock summary and ledger reports (QuestPDF)                               | High     |
| IN-012  | Barcode-ready material codes                                              | Medium   |


---

### MODULE 8.08: BUYER / ORDER MANAGEMENT

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| BO-001  | Maintain buyer/brand profiles                                             | Critical |
| BO-002  | Buyer profile: name, country, contact, payment terms, compliance docs     | High     |
| BO-003  | Create Sales/Production Orders per buyer                                  | Critical |
| BO-004  | Order includes: style, item, quantity per size, unit price, delivery date | Critical |
| BO-005  | Order supports Bill of Materials (BOM) per style                          | High     |
| BO-006  | Auto-calculate material requirement from BOM + order quantity             | High     |
| BO-007  | Order status: Draft, Confirmed, In Production, Shipped, Closed            | Critical |
| BO-008  | Order amendments with full change log                                     | High     |
| BO-009  | Generate Buyer Order Confirmation report (QuestPDF)                       | High     |
| BO-010  | Orders approaching deadline shown on dashboard                            | High     |

---

### MODULE 8.09: PRODUCTION PLANNING

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| PP-001  | Create production plans per order/style                                   | Critical |
| PP-002  | Plan defines start/end date per department                                | Critical |
| PP-003  | Gantt-style timeline view of active production orders                     | High     |
| PP-004  | Track plan vs actual output daily                                         | Critical |
| PP-005  | Alert management when any department falls behind schedule                | High     |
| PP-006  | Department-wise production efficiency report                              | High     |

---

### MODULE 8.10: CUTTING DEPARTMENT

**Process Flow:**
```
Order -> Fabric Requisition -> Marker Plan -> Lay Plan -> Cutting -> Bundle Card -> Issue to Sewing
```

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| CD-001  | Create Cutting Orders per production plan                                 | Critical |
| CD-002  | Cutting Order: style, color, size ratio, fabric details                   | Critical |
| CD-003  | Record fabric requisition and issue from store                            | Critical |
| CD-004  | Marker Plan entry (panel count, size ratio)                               | High     |
| CD-005  | Lay Plan entry (ply count, lay length, fabric required)                   | High     |
| CD-006  | Auto-calculate fabric consumption from lay plan                           | High     |
| CD-007  | Record actual cut quantity per size per color                             | Critical |
| CD-008  | Track cutting wastage (end bits, splice loss)                             | High     |
| CD-009  | Generate Bundle Cards (bundle ID, size, quantity, destination line)       | Critical |
| CD-010  | Track each bundle from creation to dispatch to sewing                     | High     |
| CD-011  | Generate Cutting Order Sheet report (QuestPDF)                            | High     |

---

### MODULE 8.11: SEWING / STITCHING DEPARTMENT

**Process Flow:**
```
Bundle from Cutting -> Line Assignment -> Daily Target -> Hourly Output Entry -> WIP Tracking -> Complete
```

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| SD-001  | Line Plan creation per order (lines, operators, target)                   | Critical |
| SD-002  | Supervisor enters hourly/daily actual output per line                     | Critical |
| SD-003  | Calculate efficiency: (Actual / Target) x 100%                            | Critical |
| SD-004  | Track WIP (Work-in-Progress) per bundle and per line                      | High     |
| SD-005  | Log machine downtime with reason codes                                    | Medium   |
| SD-006  | Track thread and accessory consumption per order                          | High     |
| SD-007  | Generate Sewing Production Report (QuestPDF)                              | High     |
| SD-008  | Operator-wise efficiency summary                                          | Medium   |

---

### MODULE 8.12: WASHING & FINISHING DEPARTMENT

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| WF-001  | Create Washing Orders from sewing output                                  | Critical |
| WF-002  | Washing Order: wash type, chemical recipe, quantity, target delivery      | High     |
| WF-003  | Record output after washing (pass/reject quantity)                        | Critical |
| WF-004  | Track finishing operations: ironing, folding, tagging                     | High     |
| WF-005  | Record finishing output quantity                                          | Critical |
| WF-006  | Finished goods transferred to QC for final inspection                     | Critical |

---

### MODULE 8.13: QUALITY CONTROL (QC)

**QC Checkpoints:**
1. Inline QC — During sewing (per hour/per bundle)
2. End-Line QC — After sewing is complete
3. Final Inspection — Before packing (AQL-based)

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| QC-001  | Configurable QC checklists per buyer/style                                | Critical |
| QC-002  | Defect catalogue with defect codes and descriptions                       | Critical |
| QC-003  | Record inspection results: Pass / Fail / Alter per bundle/piece           | Critical |
| QC-004  | Record defects with defect type and location on garment                   | Critical |
| QC-005  | Calculate defect rate: (Defects / Inspected) x 100%                      | Critical |
| QC-006  | AQL (Acceptance Quality Limit) sampling table support                     | High     |
| QC-007  | Track re-inspection results after alteration/repair                       | High     |
| QC-008  | Generate QC Inspection Report (QuestPDF)                                  | High     |
| QC-009  | QC pass/fail trend shown on dashboard                                     | High     |

---

### MODULE 8.14: PACKING & DISPATCH

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| PD-001  | Create Packing Lists from QC-passed goods                                 | Critical |
| PD-002  | Packing List: carton number, size, color, quantity per carton             | Critical |
| PD-003  | Generate printable carton labels                                          | High     |
| PD-004  | Create Delivery Challan for each dispatch                                 | Critical |
| PD-005  | Track dispatch: vehicle number, driver, dispatch date                     | High     |
| PD-006  | Update buyer order status to Shipped on dispatch confirmation             | Critical |
| PD-007  | Generate Packing List and Delivery Challan reports (QuestPDF)            | High     |

---

### MODULE 8.15: HR & EMPLOYEE MANAGEMENT

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| HR-001  | Complete employee profiles                                                | Critical |
| HR-002  | Profile: personal info, job info, salary grade, bank info, documents     | Critical |
| HR-003  | Department and designation assignment                                     | Critical |
| HR-004  | Store and manage employee documents (NID, passport, photo)                | High     |
| HR-005  | Track joining date, confirmation date, resignation                        | High     |
| HR-006  | Generate employee ID cards (QuestPDF)                                     | Medium   |
| HR-007  | Employee roster per department                                            | High     |

---

### MODULE 8.16: ATTENDANCE & LEAVE MANAGEMENT

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| AT-001  | Manual daily attendance entry per department                              | Critical |
| AT-002  | Bulk attendance import from CSV (biometric export)                        | High     |
| AT-003  | Status tracking: Present, Absent, Late, Half-Day, Holiday, Leave          | Critical |
| AT-004  | Calculate late-in and early-out minutes                                   | High     |
| AT-005  | Configurable leave types (Annual, Sick, Casual, Maternity, etc.)         | Critical |
| AT-006  | Employee leave application submission                                     | High     |
| AT-007  | Leave applications go through manager approval workflow                   | High     |
| AT-008  | Available leave balance per employee                                      | Critical |
| AT-009  | Generate monthly attendance report (QuestPDF)                             | High     |

---

### MODULE 8.17: PAYROLL PROCESSING

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| PY-001  | Salary grade setup with basic, allowances, and deductions                 | Critical |
| PY-002  | Process monthly payroll for all active employees                          | Critical |
| PY-003  | Payroll includes: basic, HRA, medical, transport, overtime                | Critical |
| PY-004  | Deduct salary for absent days and late attendance                         | Critical |
| PY-005  | Calculate overtime pay (configurable rate)                                | High     |
| PY-006  | Apply provident fund and income tax deductions                            | High     |
| PY-007  | Generate pay slips per employee (QuestPDF)                                | Critical |
| PY-008  | Generate department-wise salary sheet (QuestPDF)                          | Critical |
| PY-009  | Export bank transfer file (CSV format)                                    | High     |
| PY-010  | Processed payroll auto-posts to Finance module                            | High     |

---

### MODULE 8.18: ACCOUNTING & FINANCE

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| AC-001  | Configurable Chart of Accounts (COA)                                      | Critical |
| AC-002  | COA supports: Asset, Liability, Equity, Revenue, Expense groups           | Critical |
| AC-003  | Cost center management                                                    | High     |
| AC-004  | Journal Voucher entry (debit/credit lines)                                | Critical |
| AC-005  | Payment Voucher, Receipt Voucher, Contra Voucher                          | Critical |
| AC-006  | All vouchers require double-entry (total debit = total credit)            | Critical |
| AC-007  | General Ledger per account                                                | Critical |
| AC-008  | Trial Balance for any date range                                          | Critical |
| AC-009  | Profit & Loss Statement                                                   | Critical |
| AC-010  | Balance Sheet                                                             | Critical |
| AC-011  | Bank account management and reconciliation                                | High     |
| AC-012  | Payroll and procurement transactions auto-post to accounting              | High     |
| AC-013  | All financial reports exportable as QuestPDF                              | Critical |

---

### MODULE 8.19: REPORT MODULE (QuestPDF)

**All reports are server-side PDF generated via QuestPDF library.**

| Report ID | Report Name                  | Module        |
|-----------|------------------------------|---------------|
| RPT-001   | Purchase Order               | Procurement   |
| RPT-002   | Goods Received Note (GRN)    | Procurement   |
| RPT-003   | Supplier Statement            | Procurement   |
| RPT-004   | Stock Summary Report         | Inventory     |
| RPT-005   | Stock Ledger (per material)  | Inventory     |
| RPT-006   | Material Issue Voucher       | Inventory     |
| RPT-007   | Cutting Order Sheet          | Cutting       |
| RPT-008   | Bundle Card                  | Cutting       |
| RPT-009   | Sewing Production Report     | Sewing        |
| RPT-010   | QC Inspection Report         | QC            |
| RPT-011   | Packing List                 | Dispatch      |
| RPT-012   | Delivery Challan             | Dispatch      |
| RPT-013   | Buyer Order Confirmation     | Orders        |
| RPT-014   | Employee Pay Slip            | Payroll       |
| RPT-015   | Department Salary Sheet      | Payroll       |
| RPT-016   | Attendance Report            | Attendance    |
| RPT-017   | General Ledger               | Finance       |
| RPT-018   | Trial Balance                | Finance       |
| RPT-019   | Profit & Loss Statement      | Finance       |
| RPT-020   | Balance Sheet                | Finance       |
| RPT-021   | Audit Log Report             | Audit         |

**Report Standards:**
- Header: Company logo, name, address, report title
- Footer: Page number (X of Y), generation date/time
- Render time: < 5 seconds
- Downloadable as binary PDF stream
- API endpoint: GET /api/v1/reports/{reportType}/{id}

---

### MODULE 8.20: DASHBOARD & KPIs

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| DB-001  | Role-specific KPI cards on dashboard                                      | Critical |
| DB-002  | KPIs: production output, order fulfillment %, active POs, stock value     | Critical |
| DB-003  | Production trend line chart (last 30 days)                               | High     |
| DB-004  | Department-wise output bar chart                                          | High     |
| DB-005  | QC defect rate trend chart                                                | High     |
| DB-006  | Upcoming delivery deadlines widget                                        | Critical |
| DB-007  | Low stock alerts widget                                                   | Critical |
| DB-008  | Dashboard data refreshes every 5 minutes (or manual refresh)             | High     |
| DB-009  | Dashboard exportable as PDF summary (QuestPDF)                           | Medium   |

---

### MODULE 8.21: AUDIT LOG & ACTIVITY TRACKING

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| AL-001  | Auto-log all Create, Update, Delete operations                            | Critical |
| AL-002  | Log records: User, Module, Action, Old Value, New Value, Timestamp        | Critical |
| AL-003  | Searchable by user, module, date range, and action type                   | High     |
| AL-004  | Audit logs are non-editable and non-deletable                             | Critical |
| AL-005  | Generate Audit Log report as PDF (QuestPDF)                               | High     |
| AL-006  | Retain audit logs for minimum 3 years                                     | High     |

---

### MODULE 8.22: NOTIFICATIONS & ALERTS

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| NT-001  | In-app notifications for critical events                                  | High     |
| NT-002  | Alert: Stock reaches re-order level                                       | Critical |
| NT-003  | Alert: Production behind schedule                                         | High     |
| NT-004  | Alert: PO pending approval more than 24 hours                             | High     |
| NT-005  | Alert: Leave application pending manager approval                         | Medium   |
| NT-006  | Alert: QC rejection rate exceeds threshold                                | High     |
| NT-007  | Notification bell icon shows unread count in header                       | High     |
| NT-008  | Optional email notifications (SMTP configurable)                          | Medium   |

---

### MODULE 8.23: SETTINGS & CONFIGURATION

| Req ID  | Requirement                                                               | Priority |
|---------|---------------------------------------------------------------------------|----------|
| ST-001  | Configure company information and logo                                    | Critical |
| ST-002  | Configure financial year settings                                         | Critical |
| ST-003  | Configure SMTP email server settings                                      | Medium   |
| ST-004  | Configure approval workflows (PO, Leave, etc.)                            | High     |
| ST-005  | Configure re-order level thresholds per material                          | High     |
| ST-006  | Configure payroll components (allowances, deductions)                     | Critical |
| ST-007  | Configure QC defect catalogue                                             | High     |
| ST-008  | Configure PDF report header and footer templates                          | Medium   |
| ST-009  | Maintain application changelog/version history                            | Low      |


---

## 9. NON-FUNCTIONAL REQUIREMENTS

### 9.1 Performance

| Req ID  | Requirement                                         | Target        |
|---------|-----------------------------------------------------|---------------|
| NF-001  | API response — list queries                         | < 2 seconds   |
| NF-002  | API response — single record fetch                  | < 500 ms      |
| NF-003  | PDF report generation                               | < 5 seconds   |
| NF-004  | Dashboard KPI load time                             | < 3 seconds   |
| NF-005  | Concurrent user support without degradation         | 50 users      |
| NF-006  | Database query response time                        | < 1 second    |

### 9.2 Availability & Reliability

| Req ID  | Requirement                                         | Target        |
|---------|-----------------------------------------------------|---------------|
| NF-007  | Uptime during business hours (8 AM - 10 PM)        | 99.5%         |
| NF-008  | Scheduled maintenance window                        | Off-hours only|
| NF-009  | Data backup frequency                               | Daily full + hourly incremental |
| NF-010  | Recovery Time Objective (RTO)                       | < 4 hours     |
| NF-011  | Recovery Point Objective (RPO)                      | < 1 hour      |

### 9.3 Scalability

| Req ID  | Requirement                                                            |
|---------|------------------------------------------------------------------------|
| NF-012  | Architecture must support future horizontal scaling                    |
| NF-013  | Modular design allows new modules without core changes                 |
| NF-014  | DB must efficiently handle 5 years of transactional data               |

### 9.4 Usability

| Req ID  | Requirement                                                            |
|---------|------------------------------------------------------------------------|
| NF-015  | Fully responsive: desktop (1920px), laptop (1366px), tablet (768px), mobile (375px) |
| NF-016  | All forms provide real-time validation feedback                        |
| NF-017  | All data tables support search, sort, filter, and pagination           |
| NF-018  | Operable without browser plugins or special software                   |
| NF-019  | English UI (Bengali language support in future phase)                  |

---

## 10. SYSTEM ARCHITECTURE REQUIREMENTS

| Req ID  | Requirement                                                            |
|---------|------------------------------------------------------------------------|
| AR-001  | Backend must follow Modular Monolithic architecture pattern            |
| AR-002  | CQRS implemented via MediatR 12                                        |
| AR-003  | Each module must have Domain, Application, Infrastructure layers       |
| AR-004  | Modules must not directly reference each other's internal classes      |
| AR-005  | Cross-module communication via published interfaces only               |
| AR-006  | Repository pattern used for all data access                            |
| AR-007  | API follows RESTful conventions with versioning (/api/v1/)             |
| AR-008  | All API responses use standard envelope (success, data, message)       |

---

## 11. TECHNOLOGY STACK REQUIREMENTS

| Layer                  | Required Technology                         |
|------------------------|---------------------------------------------|
| Backend Framework      | ASP.NET Core 10 Web API                      |
| Architecture Pattern   | Modular Monolithic + CQRS                   |
| CQRS Mediator          | MediatR 12+                                 |
| ORM                    | Entity Framework Core 10                     |
| Validation             | FluentValidation                            |
| Object Mapping         | AutoMapper or Mapster                       |
| Logging                | Serilog with Seq sink                       |
| Authentication         | ASP.NET Core 10 Identity + JWT Bearer          |
| PDF Reports            | QuestPDF (mandatory, no substitution)       |
| API Documentation      | Swagger / Scalar                            |
| Frontend Framework     | Next.js 14 (App Router)                     |
| Frontend Language      | TypeScript                                  |
| UI Components          | shadcn/ui + Radix UI                        |
| CSS                    | Tailwind CSS                                |
| State Management       | Zustand                                     |
| API Client             | TanStack Query (React Query)                |
| Form Handling          | React Hook Form + Zod                       |
| Charts/Graphs          | Recharts or Chart.js                        |
| Database               | Microsoft SQL Server 2022                   |
| DB Migrations          | EF Core Code-First Migrations               |
| Unit Testing           | xUnit + Moq + FluentAssertions              |

---

## 12. DATABASE REQUIREMENTS

| Req ID  | Requirement                                                            |
|---------|------------------------------------------------------------------------|
| DB-001  | Database engine: Microsoft SQL Server 2022                             |
| DB-002  | All primary keys must be GUID (uniqueidentifier)                       |
| DB-003  | All critical tables must have soft-delete columns (IsDeleted, DeletedAt, DeletedBy) |
| DB-004  | All tables must have audit columns (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy) |
| DB-005  | Schema-per-module strategy: auth, inventory, procurement, production, hr, finance |
| DB-006  | All money columns: DECIMAL(18,4)                                       |
| DB-007  | All date columns: DATETIME2(7)                                         |
| DB-008  | Daily database backup must be automated                                |
| DB-009  | Database connections must use connection pooling                       |
| DB-010  | All FK columns and filter columns must be indexed                      |

---

## 13. SECURITY REQUIREMENTS

| Req ID  | Requirement                                                            |
|---------|------------------------------------------------------------------------|
| SE-001  | All endpoints except login require valid JWT                           |
| SE-002  | JWT access tokens expire after 15 minutes                              |
| SE-003  | Refresh tokens expire after 7 days (single-use)                        |
| SE-004  | Passwords hashed using BCrypt via ASP.NET Identity                     |
| SE-005  | All API inputs validated server-side via FluentValidation              |
| SE-006  | SQL injection prevented via parameterized EF Core queries              |
| SE-007  | XSS protection on all user-input fields                                |
| SE-008  | CORS configured for trusted frontend origins only                      |
| SE-009  | All communication over HTTPS with TLS 1.2 minimum                     |
| SE-010  | Sensitive data never logged (passwords, tokens, PII)                   |
| SE-011  | Account lockout after 5 consecutive failed login attempts              |
| SE-012  | All permission checks enforced server-side                             |
| SE-013  | API rate limiting to prevent abuse                                     |

---

## 14. UI/UX REQUIREMENTS

| Req ID  | Requirement                                                            |
|---------|------------------------------------------------------------------------|
| UX-001  | Fully responsive: desktop, tablet, mobile                              |
| UX-002  | Sidebar navigation dynamically renders based on user permissions       |
| UX-003  | All data lists: search, server-side filter/sort/paginate               |
| UX-004  | Forms show real-time validation errors                                 |
| UX-005  | Confirmation dialogs before all destructive operations                 |
| UX-006  | Loading spinners during all async operations                           |
| UX-007  | Success and error toast notifications after all actions                |
| UX-008  | Application supports light and dark mode                               |
| UX-009  | All interactive elements have unique, descriptive IDs                  |
| UX-010  | Page load time under 3 seconds on standard broadband                   |

---

## 15. INTEGRATION REQUIREMENTS

| Integration                    | Type           | Priority | Phase |
|--------------------------------|----------------|----------|-------|
| Biometric attendance system    | CSV import     | High     | V1    |
| Bank payment file export       | CSV export     | High     | V1    |
| SMTP email server              | Outbound only  | Medium   | V1    |
| Future: SMS Gateway            | Outbound       | Low      | V2    |
| Future: ERP system integration | REST API       | Low      | V3    |

---

## 16. REPORT & DOCUMENT REQUIREMENTS

| Requirement                                                                  |
|------------------------------------------------------------------------------|
| All PDF reports generated using QuestPDF library (mandatory)                 |
| Report header: company logo, name, address, report title                     |
| Report footer: page number (X of Y), generation date/time                   |
| Reports support A4 and Letter paper size (configurable per report)           |
| All report endpoints require Export permission                               |
| Reports downloadable as binary PDF streams                                   |
| Report generation logged in audit trail                                      |
| All reports rendered server-side (no client-side PDF generation)             |

---

## 17. PERFORMANCE REQUIREMENTS

| Metric                              | Requirement                              |
|-------------------------------------|------------------------------------------|
| Max concurrent users                | 50 (v1), scalable to 200 (future)       |
| API list query response             | < 2 seconds (10,000+ records)           |
| API single record response          | < 500 milliseconds                       |
| PDF report generation               | < 5 seconds                              |
| Page initial load (Next.js)         | < 3 seconds                              |
| Database query execution            | < 1 second for standard queries          |
| File upload size limit              | 10 MB per file                           |

---

## 18. DEPLOYMENT REQUIREMENTS

| Requirement                                                                  |
|------------------------------------------------------------------------------|
| Backend deployable on Windows Server with IIS                                |
| Docker containerization support (optional for future use)                    |
| Frontend deployable as Next.js standalone build                              |
| Environment-based configuration: Development, Staging, Production            |
| Database migrations run automatically on deployment                          |
| Swagger/Scalar API docs available in Dev/Staging only (disabled in Production) |
| Application must enforce SSL/HTTPS in production                             |
| CI/CD pipeline recommended (GitHub Actions or Azure DevOps)                  |
| Environment variables used for all secrets (no hardcoded credentials)        |

---

## 19. GLOSSARY OF TERMS

| Term     | Definition                                                                   |
|----------|------------------------------------------------------------------------------|
| AQL      | Acceptance Quality Limit — standard for sampling in QC inspection            |
| BOM      | Bill of Materials — list of materials required per style/product             |
| CQRS     | Command Query Responsibility Segregation — architecture pattern              |
| GRN      | Goods Received Note — document recording received materials from supplier    |
| JWT      | JSON Web Token — stateless authentication token                              |
| KPI      | Key Performance Indicator — measurable business metric                       |
| LC       | Letter of Credit — payment instrument for export orders                      |
| MediatR  | .NET library implementing the Mediator pattern for CQRS                      |
| PO       | Purchase Order — formal order to a supplier                                  |
| QC       | Quality Control — process of ensuring product meets standards                |
| UAT      | User Acceptance Testing — final testing by business users                    |
| WIP      | Work-in-Progress — items in mid-production stage                             |
| FIFO     | First In, First Out — inventory valuation method                             |
| COA      | Chart of Accounts — structured list of all accounting accounts               |
| ERP      | Enterprise Resource Planning — integrated business management system         |
| SMTP     | Simple Mail Transfer Protocol — email sending protocol                       |
| REST     | Representational State Transfer — API architectural style                    |
| ORM      | Object-Relational Mapper — database abstraction layer                        |
| CRUD     | Create, Read, Update, Delete — basic data operations                         |
| RTO      | Recovery Time Objective — max acceptable downtime                            |
| RPO      | Recovery Point Objective — max acceptable data loss window                   |

---

## 20. APPROVAL & SIGN-OFF

| Role                     | Name            | Signature | Date       |
|--------------------------|-----------------|-----------|------------|
| Project Sponsor / Owner  | ___________     | _______   | __/__/____ |
| General Manager          | ___________     | _______   | __/__/____ |
| IT Manager               | ___________     | _______   | __/__/____ |
| Project Lead / Architect | ___________     | _______   | __/__/____ |
| QA Lead                  | ___________     | _______   | __/__/____ |
| Business Analyst         | ___________     | _______   | __/__/____ |

---

> **Document Control:**
> This BRD is a living document. All changes must be reviewed and re-approved by the Project Sponsor.
> Version history must be maintained.

---

*GarmentsERP BRD v1.0.0 | Created: 2026-09-24 | Status: Draft for Review*
*Confidential — For Internal Use Only*


---

## 21. FILE STORAGE REQUIREMENTS

| Req ID  | Requirement                                                                  | Priority |
|---------|------------------------------------------------------------------------------|----------|
| FS-001  | Files must NOT be stored inside the database (no VARBINARY columns for files)| Critical |
| FS-002  | Only the relative file path shall be stored in database columns              | Critical |
| FS-003  | Phase 1: Files stored on local server disk (D:\GarmentsERP\Storage\)        | Critical |
| FS-004  | Phase 2: Migrate to Azure Blob Storage with zero business logic changes      | High     |
| FS-005  | All file types must be validated (whitelist: jpg, png, pdf, doc, xlsx, csv) | Critical |
| FS-006  | Executable and script files must always be rejected (.exe, .bat, .sh, .ps1) | Critical |
| FS-007  | Maximum file size: 20 MB per upload                                          | High     |
| FS-008  | File names must be sanitized (GUID prefix + regex clean)                     | Critical |
| FS-009  | Files must be served only through authenticated API endpoints (never direct) | Critical |
| FS-010  | File download requires valid JWT and appropriate module Read permission       | Critical |
| FS-011  | Generated QuestPDF reports streamed on-demand (not permanently stored)       | High     |
| FS-012  | Report cache (60-min TTL) stored in reports/generated/ folder                | Medium   |
| FS-013  | File storage configuration must be environment-variable driven (no hardcode) | High     |
| FS-014  | File backup: daily incremental, weekly full, 90-day retention               | High     |
| FS-015  | Dedicated storage disk recommended: 200 GB for Year 1 (~40 GB estimated use) | Medium   |

### File Types Stored

| Module          | File Types                                            |
|-----------------|-------------------------------------------------------|
| Company Setup   | Logo (.jpg, .png)                                     |
| HR / Employees  | NID scan, passport, photo, contract (.jpg, .png, .pdf)|
| Procurement     | PO scan, GRN doc, supplier invoice (.pdf, .jpg)       |
| Production      | Style images, tech pack, marker files (.jpg, .png, .pdf) |
| Compliance      | Audit reports, buyer docs (.pdf, .doc, .docx)         |
| Attendance      | Biometric import files (.csv, .xlsx)                  |
| Reports         | QuestPDF generated reports (.pdf - streamed/cached)   |
