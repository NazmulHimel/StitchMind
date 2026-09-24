# GarmentsERP — Database Design Document

## Database: Microsoft SQL Server 2022

---

## Schema Strategy (Schema-per-Module)

| Schema       | Module(s)                              |
|--------------|----------------------------------------|
| auth         | Users, Roles, Permissions, Tokens      |
| procurement  | Suppliers, PurchaseOrders, GRNs        |
| inventory    | Materials, Stock, StockMovements       |
| production   | Orders, CuttingPlan, SewingPlan, QC    |
| hr           | Employees, Attendance, Leave, Payroll  |
| finance      | Accounts, Vouchers, Ledger             |
| reporting    | Report templates and saved configs     |
| audit        | AuditLogs, ActivityLogs                |

---

## Standard Audit Columns (All Tables)

| Column      | Type             | Description                    |
|-------------|------------------|--------------------------------|
| Id          | UNIQUEIDENTIFIER | Primary Key (GUID)             |
| CreatedAt   | DATETIME2        | Row creation timestamp         |
| CreatedBy   | UNIQUEIDENTIFIER | FK -> auth.Users.Id            |
| UpdatedAt   | DATETIME2        | Last update timestamp          |
| UpdatedBy   | UNIQUEIDENTIFIER | FK -> auth.Users.Id            |
| IsDeleted   | BIT              | Soft delete flag               |
| DeletedAt   | DATETIME2        | Soft delete timestamp          |
| DeletedBy   | UNIQUEIDENTIFIER | Who deleted                    |

---

## Core Table Groups

### auth Schema
- auth.Users
- auth.Roles
- auth.UserRoles
- auth.Permissions (Module, Action: Read/Write/Delete/Export)
- auth.RolePermissions
- auth.RefreshTokens

### procurement Schema
- procurement.Suppliers
- procurement.PurchaseOrders
- procurement.PurchaseOrderItems
- procurement.GoodsReceivedNotes
- procurement.GRNItems
- procurement.SupplierInvoices

### inventory Schema
- inventory.MaterialCategories
- inventory.Materials (includes thread/yarn)
- inventory.StockLedger
- inventory.StockTransactions (IN/OUT/ADJUST)
- inventory.Warehouses
- inventory.ReorderAlerts

### production Schema
- production.BuyerOrders
- production.ProductionPlans
- production.CuttingOrders
- production.CuttingLayPlans
- production.SewingLinePlans
- production.SewingDailyOutput
- production.WashingOrders
- production.QCInspections
- production.QCDefects
- production.PackingLists

### hr Schema
- hr.Employees
- hr.Departments
- hr.Designations
- hr.AttendanceLogs
- hr.LeaveTypes
- hr.LeaveApplications
- hr.PayrollPeriods
- hr.PayrollDetails

### finance Schema
- finance.ChartOfAccounts
- finance.CostCenters
- finance.JournalVouchers
- finance.JournalVoucherLines
- finance.BankAccounts
- finance.BankTransactions

---

## Design Rules

1. All PKs are GUID (NEWSEQUENTIALID() for index performance)
2. No cascading deletes — soft delete only
3. Cross-schema joins allowed within same SQL Server instance
4. Cross-module FKs use only IDs, not enforced at DB level
5. Indexed columns: all FK columns, CreatedAt, IsDeleted
6. All money/decimal columns: DECIMAL(18,4)
7. All date columns: DATETIME2(7)

---

*Last updated: 2026-09-24*
