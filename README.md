# GarmentsERP

> Enterprise-Grade Garments & Apparel Manufacturing Management System (.NET 10 Modular Monolith + CQRS + Next.js)

## Architecture Overview

```
GarmentsERP.slnx (24 Projects)
|
+-- src/
|   +-- GarmentsERP.API                          (ASP.NET Core 10 Web API, JWT, Swagger, Middleware)
|   +-- GarmentsERP.Shared                       (BaseEntity, Result<T>, ApiResponse<T>, Exceptions)
|   |
|   +-- Modules/
|       +-- Auth/                                (Domain, Application [CQRS/MediatR], Infrastructure [EF Core MSSQL])
|       +-- Procurement/                         (Material POs, Suppliers, GRN, Raw Material Verification)
|       +-- Inventory/                           (Fabric Rolls, Trims & Accessories, Finished Cartons)
|       +-- Production/                          (Cutting Department, Sewing Lines, Finishing, WIP Tracking)
|       +-- Finance/                             (Chart of Accounts, AP/AR, General Ledger, Vouchers)
|       +-- HR/                                  (Employee Directory, Biometric Attendance, Payroll)
|       +-- Reporting/                           (QuestPDF Report Generation Engine)
|
+-- tests/
    +-- GarmentsERP.Auth.UnitTests               (17 Unit Tests - All Passing)
    +-- GarmentsERP.Shared.UnitTests             (4 Unit Tests - All Passing)
```

## Frontend Architecture (Next.js App Router)
- **Framework:** Next.js 16 (App Router), React, TypeScript
- **Styling:** Custom Vanilla CSS Design System with dark mode, glassmorphism, responsive sidebar & mobile drawer
- **Navigation:** Permission-based dynamic sidebar menu built from backend permissions
- **Role Switcher (Demo):** Instant 1-click evaluation of permission profiles (Super Admin, Production Manager, Cutting Supervisor, Sewing Supervisor, QC Supervisor, Accounts Manager, Store Manager)
- **Icons:** Lucide Icons (`lucide-react`)

---

## Default Seed Credentials

| Property | Value |
|---|---|
| **URL (API)** | `http://localhost:5224` |
| **URL (Swagger)** | `http://localhost:5224` (or `/swagger`) |
| **URL (Frontend)** | `http://localhost:3000` |
| **SuperAdmin Username** | `admin` |
| **SuperAdmin Password** | `Admin@123456` |
| **Default Company ID** | `11111111-1111-1111-1111-111111111111` |

---

## Quick Start Guide

### 1. Run Backend API (.NET 10)
```powershell
./scripts/run-backend.ps1
# Or:
cd src/backend
dotnet run --project src/GarmentsERP.API/GarmentsERP.API.csproj
```

### 2. Run Frontend Web App (Next.js)
```powershell
./scripts/run-frontend.ps1
# Or:
cd src/frontend
npm run dev
```

### 3. Run Test Suite
```powershell
./scripts/run-tests.ps1
# Or:
cd src/backend
dotnet test GarmentsERP.slnx
```

---

## Project Status

- [x] **Phase 1: Foundation (Completed)**
  - [x] Solution Scaffold: 24 .NET 10 projects in clean architecture modular monolith
  - [x] Core Shared abstractions (`BaseEntity`, `Result<T>`, `ApiResponse<T>`, `ICurrentUserService`, `IFileStorageService`)
  - [x] Auth Module: CQRS Commands (`Login`, `RefreshToken`, `Logout`, `RegisterUser`)
  - [x] Auth Module: CQRS Queries (`GetCurrentUser`, `GetMenuItems`, `GetUsers`)
  - [x] MSSQL Database Schema with `auth` schema, EF Core migrations, and automated seeder (115 permissions + 10 system roles + admin)
  - [x] Next.js Frontend with dynamic permission-based navigation, responsive sidebar, auth context, and role switcher
  - [x] Unit test suite (21 unit tests passing)
- [ ] **Phase 2: Procurement & Inventory (Ready for Implementation)**
  - [ ] Supplier Management & Scorecards
  - [ ] Purchase Order (PO) workflow with multi-level approval
  - [ ] Goods Receipt Note (GRN) inspection & fabric roll receiving
  - [ ] Thread / Yarn spinning batch management
  - [ ] Stock alerts, bin allocation, and re-order triggers
- [ ] **Phase 3: Production Floor**
- [ ] **Phase 4: HR & Dispatch**
- [ ] **Phase 5: Finance & QuestPDF Reporting**
- [ ] **Phase 6: Polish & Production Deployment**

---
*GarmentsERP Project — Modular Monolith Architecture*