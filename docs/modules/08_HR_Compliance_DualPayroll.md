# StitchMind Module Specification
## Module 08: HR, Attendance, Social Compliance & Dual-Stream Payroll

---

| Metadata Field | Value |
|---|---|
| **Module ID** | `MOD-08` / `Phase H` (Sprint 31–34) |
| **BRD Reference** | Section 4, Module 9 (HR, Compliance & Payroll) |
| **WBS Reference** | `HR-01` through `HR-07`, `HR-RPT` |
| **Acceptance Criteria** | `AC-07` (Child Labor Age Gate Compliance) |
| **Status** | Approved Specification |

---

## 1. Business Purpose & Scope

Apparel export conglomerates undergo rigorous international social compliance audits (e.g. BSCI, Sedex SMETA, WRAP). Non-compliance in worker age verification, excessive unauthorized overtime, or delayed wage payments directly triggers buyer boycotts and contract cancellations.

Module 08 delivers:
1. **Child Labor Age Gate:** Enforces an unbypassable programmatic check blocking onboarding of any worker under 18.0 years.
2. **Biometric Clock Listener:** Real-time TCP/IP synchronization with physical ZKTeco and facial recognition terminals.
3. **Dynamic Shift & Late Penalty Engine:** Manages grace periods, day/night shift rotations, and automated attendance deductions.
4. **Dual-Stream Payroll:** Computes both monthly staff salaries and floor piece-rate earnings with statutory overtime (2x standard rate).
5. **Bilingual Pay Slips & Mobile Payout:** Renders QuestPDF pay slips in English and Bangla (বাংলা) and exports batch files for bKash, Nagad, and bank transfers.

---

## 2. Child Labor Compliance Verification Flow (AC-07)

```
[HR Officer Enters Applicant Birth Date]
                   │
                   ▼
┌─────────────────────────────────────────────────────────────┐
│ StitchMind Age Compliance Validator                         │
│ Formula: (DateOfJoining - DateOfBirth) in Complete Years    │
└──────────────────────────┬──────────────────────────────────┘
                           │
             ┌─────────────┴─────────────┐
             │                           │
   [Age >= 18.00 Years]        [Age < 18.00 Years]
             │                           │
             ▼                           ▼
┌─────────────────────────┐ ┌─────────────────────────────────┐
│ Status: AGE_VERIFIED    │ │ STATUS: BLOCKED_UNDERAGE_LABOR  │
│ Creates Digital Service │ │ Profile creation ABORTED        │
│ Book & Biometric Badge  │ │ Audit alert logged to HR Director│
└─────────────────────────┘ └─────────────────────────────────┘
```

---

## 3. Database Schema & Entities

### Schema: `human_resources`

```
  ┌──────────────────┐           ┌──────────────────┐
  │     Employee     │1         *│ BiometricAttendance
  └────────┬─────────┘           └──────────────────┘
           │1
           │*
  ┌────────▼─────────┐1         *┌──────────────────┐
  │   PayrollSheet   ├───────────►   PayrollDetail  │
  └──────────────────┘           └──────────────────┘
```

### Table 1: `human_resources.Employees` (Digital Service Book)
* `Id` (Guid, PK)
* `EmployeeCode` (varchar(20), Unique, Index, e.g. `EMP-2026-1042`)
* `FullName` (nvarchar(100))
* `FullNameBangla` (nvarchar(100), e.g. "মোঃ রফিকুল ইসলাম")
* `DateOfBirth` (date)
* `NationalIdOrBirthCert` (varchar(30), Unique)
* `CalculatedAgeYears` (decimal(4,2))
* `IsAgeVerified` (bit, default 0)
* `Department` (nvarchar(50), e.g. "Sewing", "Cutting", "Finishing", "Accounts")
* `Designation` (nvarchar(50), e.g. "Senior Sewing Operator", "Line Supervisor")
* `PayType` (nvarchar(20), e.g. `PIECE_RATE`, `MONTHLY_SALARY`)
* `MonthlyBasicSalary` (decimal(18,2))
* `BiometricBadgeNumber` (varchar(30), Unique, Index)
* `DateOfJoining` (date)
* `Status` (nvarchar(20), default "ACTIVE")

### Table 2: `human_resources.BiometricAttendance`
* `Id` (bigint, Identity, PK)
* `EmployeeId` (Guid, FK, Index)
* `ClockDate` (date, Index)
* `CheckInTime` (time)
* `CheckOutTime` (time)
* `HoursWorked` (decimal(5,2))
* `OvertimeHours` (decimal(5,2), e.g. 2.00)
* `AttendanceStatus` (nvarchar(20), e.g. `PRESENT`, `LATE`, `ABSENT`, `ON_LEAVE`)
* `DeviceTerminalId` (nvarchar(30))

### Table 3: `human_resources.PayrollDetails`
* `Id` (Guid, PK)
* `PayrollSheetId` (Guid, FK)
* `EmployeeId` (Guid, FK)
* `PayMonthYear` (varchar(7), e.g. "2026-10")
* `PayStream` (nvarchar(20), e.g. `MONTHLY_STAFF`, `FLOOR_PIECE_RATE`)
* `BaseOrPieceRateEarnings` (decimal(18,2))
* `OvertimeEarnings` (decimal(18,2), Formula: `OT_Hours * (Basic/208) * 2`)
* `AttendanceBonus` (decimal(18,2))
* `Deductions` (decimal(18,2))
* `NetDisbursement` (decimal(18,2))
* `PaymentChannel` (nvarchar(20), e.g. `BKASH`, `NAGAD`, `BANK_TRANSFER`)
* `MFSAccountNumber` (varchar(20))

---

## 4. Key Business Logic: Child Labor Age Gate Implementation

```csharp
public async Task<Result<Guid>> RegisterEmployeeAsync(RegisterEmployeeCommand command)
{
    // Calculate exact age on date of joining
    int ageYears = command.DateOfJoining.Year - command.DateOfBirth.Year;
    if (command.DateOfBirth.Date > command.DateOfJoining.AddYears(-ageYears)) ageYears--;

    // AC-07 Statutory Check: Age < 18 is strictly prohibited
    if (ageYears < 18)
    {
        // Log critical security and compliance alert
        await _complianceLogger.LogCriticalViolationAsync(
            "CHILD_LABOR_ATTEMPT", 
            $"Attempted onboarding of candidate with age {ageYears} years (DOB: {command.DateOfBirth:yyyy-MM-dd}). Operation rejected."
        );

        return Result.Failure<Guid>(
            $"STATUTORY COMPLIANCE VIOLATION: Applicant age is {ageYears} years. " +
            "StitchMind strictly prohibits onboarding individuals under 18 years of age under International Labor Standards & Bangladesh Labor Act.");
    }

    var employee = new Employee
    {
        Id = Guid.NewGuid(),
        EmployeeCode = await GenerateEmployeeCodeAsync(),
        FullName = command.FullName,
        FullNameBangla = command.FullNameBangla,
        DateOfBirth = command.DateOfBirth,
        CalculatedAgeYears = ageYears,
        IsAgeVerified = true,
        Department = command.Department,
        Designation = command.Designation,
        PayType = command.PayType,
        MonthlyBasicSalary = command.MonthlyBasicSalary,
        BiometricBadgeNumber = command.BiometricBadgeNumber,
        DateOfJoining = command.DateOfJoining
    };

    _dbContext.Employees.Add(employee);
    await _dbContext.SaveChangesAsync();

    return Result.Success(employee.Id);
}
```

---

## 5. Production Demo Data: Factory Payroll Record

### A. Monthly Payroll Summary (October 2026)
* **Total Factory Headcount:** 1,250 Workers (1,100 Floor Piece-Rate / 150 Management Staff).
* **Total Monthly Disbursement:** **$142,500.00 USD (equivalent in BDT)**.

### B. Sample Operator Payroll Record (Sewing Line 04)
* **Operator:** Md. Rafiqul Islam (`EMP-2026-1042`, Age: 26 Years)
* **Pay Stream:** `FLOOR_PIECE_RATE` (Waistband Attachment Operation # 22)
* **Units Completed:** 2,400 Garments @ $0.0700 / pc = **$168.00**
* **Overtime Worked:** 26 Hours @ $1.45 / hr = **$37.70**
* **Attendance Bonus (100% on-time):** **$10.00**
* **Total Gross Earnings:** **$215.70 USD**
* **Disbursement Channel:** **bKash Wallet** (`017XXXXXXXX`)
* **Pay Slip Generated:** Bilingual QuestPDF document (Bangla & English) ready for buyer audit inspection.
