# GarmentsERP — Module Detailed Plans

---

## MODULE 01: Auth & User Management

### Purpose
Secure login, token management, multi-user support.

### Features
- Login with username/email + password
- JWT access token (15 min) + Refresh token (7 days)
- Logout (revoke refresh token)
- Change password / Forgot password
- User profile management
- User activation / deactivation

### Key Entities
- User (Id, FullName, Email, Username, Phone, IsActive, CompanyId)
- RefreshToken (Id, UserId, Token, ExpiryDate, IsRevoked, DeviceInfo)

---

## MODULE 02: Role & Permission Management

### Purpose
Control what each user can do in each module.

### Features
- Create / Edit / Delete roles
- Assign permissions per module per role (Create, Read, Update, Delete, Export)
- Assign roles to users
- Permission-based dynamic frontend menu

### Key Entities
- Role (Id, Name, Description, IsSystem)
- Permission (Id, Module, Action)
- RolePermission (RoleId, PermissionId)
- UserRole (UserId, RoleId)

---

## MODULE 03: Organization / Company Setup

### Purpose
Configure company/factory details.

### Features
- Company name, logo, address, contact
- Financial year setup
- Currency and unit of measure setup
- Department management

---

## MODULE 04: Supplier Management

### Features
- Supplier registration (name, address, contact, bank info)
- Supplier categories (fabric, thread, accessories)
- Supplier rating / performance tracking
- Supplier outstanding balance view

---

## MODULE 05: Material Procurement (PO → GRN → Invoice)

### Business Flow
Purchase Requisition -> Purchase Order -> Approval -> GRN -> Supplier Invoice -> Payment

### Features
- Purchase Requisition creation
- PO creation with multi-item, multi-unit support
- Approval workflow (configurable approvers)
- Goods Received Note (GRN) against PO
- Partial delivery handling
- Supplier invoice matching
- QuestPDF: PO Report, GRN Report

---

## MODULE 06: Thread & Yarn Management

### Purpose
Manage thread/yarn-specific procurement and production.

### Features
- Thread types catalogue (polyester, cotton, nylon, etc.)
- Count/weight/color specifications
- Thread consumption planning per order
- Thread stock tracking
- Thread issue to production departments

---

## MODULE 07: Inventory / Stock Management

### Features
- Material master catalogue
- Multi-warehouse support
- Stock IN (from GRN), Stock OUT (issue to departments)
- Stock adjustment with reason
- Stock transfer between warehouses
- Re-order level alerts
- Stock ledger report
- FIFO / weighted average costing

---

## MODULE 08: Buyer / Order Management

### Features
- Buyer profile (name, country, contact, payment terms)
- Sales Order / Booking creation
- Style / product specification
- Order BOM (Bill of Materials)
- Order status tracking
- Delivery schedule

---

## MODULE 09: Production Planning

### Features
- Production plan creation per order
- Department-wise capacity planning
- Scheduling (start date, end date per dept)
- Plan vs actual tracking
- Bottleneck alerts

---

## MODULE 10: Cutting Department

### Business Flow
Order -> Marker Making -> Lay Plan -> Cutting -> Bundle Creation -> Issue to Sewing

### Features
- Cutting order creation
- Fabric requisition from store
- Marker plan (size ratio, panels)
- Lay plan (no. of plies, length)
- Cut quantity tracking per size
- Bundle / card creation
- Wastage recording
- QuestPDF: Cutting Order Sheet

---

## MODULE 11: Sewing / Stitching Department

### Business Flow
Bundle from Cutting -> Line Assignment -> Daily Output -> WIP Tracking -> Sewing Complete

### Features
- Line plan creation (operator assignment)
- Target vs actual hourly/daily output
- WIP (Work-in-Progress) tracking per bundle
- Operator efficiency reporting
- Machine downtime log
- QuestPDF: Sewing Production Report

---

## MODULE 12: Washing & Finishing Department

### Features
- Washing order creation from sewing output
- Wash type specification
- Finishing operations (ironing, folding)
- Output tracking

---

## MODULE 13: Quality Control (QC)

### QC Points
1. Inline QC (during sewing)
2. End-line QC (after sewing)
3. Final Inspection (before packing)

### Features
- Inspection checklist (configurable per buyer)
- Defect catalogue
- Defect recording per bundle / piece
- Pass / Fail / Alter decision
- AQL sampling support
- QuestPDF: QC Inspection Report

---

## MODULE 14: Packing & Dispatch

### Features
- Packing list creation per shipment
- Carton packing (pieces per carton)
- Delivery challan generation
- Shipment tracking (vessel/flight info)
- QuestPDF: Delivery Challan, Packing List

---

## MODULE 15: HR & Employee Management

### Features
- Employee registration (personal, job, bank info)
- Department & designation assignment
- Document management (NID, passport, photos)
- Employee history log

---

## MODULE 16: Attendance & Leave

### Features
- Biometric-ready attendance import (CSV/API)
- Manual attendance entry
- Late in / early out tracking
- Leave types (annual, sick, casual, maternity)
- Leave application & approval workflow
- Leave balance calculation
- QuestPDF: Attendance Report

---

## MODULE 17: Payroll Processing

### Features
- Salary grade & structure setup
- Allowances & deductions configuration
- Payroll processing per period (monthly)
- Overtime calculation
- Tax deduction (configurable)
- Bank transfer file export
- QuestPDF: Salary Sheet / Pay Slip

---

## MODULE 18: Accounting & Finance

### Chart of Accounts (COA)
- Standard double-entry bookkeeping
- Asset, Liability, Equity, Revenue, Expense groups

### Features
- Chart of Accounts setup
- Cost center management
- Journal Voucher (Debit/Credit entries)
- Payment Voucher, Receipt Voucher, Contra Voucher
- Bank reconciliation
- General Ledger
- Trial Balance
- Profit & Loss Statement
- Balance Sheet
- QuestPDF: All financial reports

---

## MODULE 19: Report Module (QuestPDF)

### Implementation
- QuestPDF library integrated in Reporting project
- All reports rendered server-side as PDF
- API endpoint: GET /api/v1/reports/{reportType}?params
- Frontend: "Download PDF" button on each list/detail page

---

## MODULE 20: Dashboard & KPIs

### KPI Cards
- Daily production output
- Order fulfillment rate
- Stock value
- Pending POs
- Defect rate (QC)
- Employee attendance rate
- Revenue vs cost (Finance)

### Charts
- Production trend (line chart)
- Department-wise output (bar chart)
- Stock levels (gauge)
- Financial overview (pie chart)

---

## MODULE 21: Audit Log & Activity Tracking

### Features
- All data changes logged automatically (EF Core interceptor)
- Log: Who, What, When, Old Value, New Value
- Search / filter audit logs
- Export audit log as PDF

---

## MODULE 22: Notifications & Alerts

### Features
- In-app notifications
- Re-order stock alerts
- Production deadline alerts
- Leave approval notifications
- Email notifications (configurable)

---

## MODULE 23: Settings & Configuration

### Features
- System settings (company, currency, date format)
- Email server configuration (SMTP)
- Backup configuration
- Report header / footer customization
- Approval workflow configuration

---

*Last updated: 2026-09-24*
