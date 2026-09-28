# StitchMind Module Specification
## Module 03: Quality Control (QC) & 3D Warehouse Management

---

| Metadata Field | Value |
|---|---|
| **Module ID** | `MOD-03` / `Phase C` (Sprint 11–16 & Sprint 17–22) |
| **BRD Reference** | Section 4, Module 4 (Stock & Warehouse) & Module 7 (Quality Control) |
| **WBS Reference** | `QC-01`, `WHS-01`, `WHS-02`, `WHS-03`, `WHS-RPT` |
| **Acceptance Criteria** | `AC-04` (Quarantine to Production Release Gate) |
| **Status** | Approved Specification |

---

## 1. Business Purpose & Scope

Raw material quality directly dictates factory efficiency, garment defect rates, and buyer rejections. Module 03 bridges warehouse receipt with factory floor operations:
1. **Quarantine Release Gate:** Implements the international **Fabric 4-Point System (ASTM D5430-07)** to audit quarantined fabric rolls. Only passed lots transition to `RELEASED_FOR_PRODUCTION`.
2. **Roll-wise Shade & Shrinkage Segregation:** Groups passed fabric rolls by shade (A, B, C, D) and shrinkage percentage to prevent shade-variation defects across cut garment panels.
3. **3D Warehouse Bin & Rack Mapping:** Manages physical warehouse storage coordinates down to aisle, rack, tier, and bin level.
4. **Customs Bond Register & UP Accounting:** Tracks duty-free imported raw materials under Utilization Permits (UP) for statutory compliance and government audit trails.

---

## 2. 4-Point Inspection Mathematical Model

### Penalty Scoring Scale

| Defect Length | Penalty Points |
|---|---|
| Defect $\le$ 3 inches (75 mm) | **1 Point** |
| Defect > 3 inches and $\le$ 6 inches (150 mm) | **2 Points** |
| Defect > 6 inches and $\le$ 9 inches (230 mm) | **3 Points** |
| Defect > 9 inches (230 mm) | **4 Points** |
| Any hole, cut, or tear (regardless of size) | **4 Points** |

*Note: No single linear yard can be penalized more than 4 points.*

### Points Per 100 Square Yards Formula
$$\text{Score} = \frac{\text{Total Penalty Points} \times 3600}{\text{Inspected Length (Yards)} \times \text{Cuttable Width (Inches)}}$$

### Quality Acceptance Threshold
* **PASS:** $\le 28.0 \text{ points per 100 sq. yards}$.
* **FAIL / REJECT:** $> 28.0 \text{ points per 100 sq. yards}$ (Lot quarantined; supplier debit note issued).

---

## 3. Database Schema & Entities

### Schema: `quality` & `inventory`

```
  ┌──────────────────┐           ┌──────────────────┐
  │     StockLot     │1         *│    FabricRoll    │
  └────────┬─────────┘           └────────┬─────────┘
           │1                             │1
           │*                             │*
  ┌────────▼─────────┐1         *┌────────▼─────────┐
  │   QCInspection   ├───────────► InspectionDefect │
  └────────┬─────────┘           └──────────────────┘
           │1
           ▼ (If Passed)
  ┌──────────────────┐
  │ StockState =     │
  │ RELEASED_FOR_PROD│
  └──────────────────┘
```

### Table 1: `quality.FabricInspections`
* `Id` (Guid, PK)
* `InspectionNumber` (varchar(30), Unique, e.g. `QC-4PT-2026-0045`)
* `StockLotId` (Guid, FK -> `inventory.StockLots.Id`)
* `InspectorId` (nvarchar(100))
* `InspectionDate` (datetime2)
* `TotalRollsInLot` (int, e.g. 368)
* `RollsInspected` (int, e.g. 37, standard 10% sampling)
* `TotalYardsInspected` (decimal(18,2))
* `AveragePointsPer100SqYds` (decimal(6,2))
* `OverallResult` (nvarchar(20), e.g. `PASSED`, `REJECTED`, `CONDITIONAL_PASS`)
* `Remarks` (nvarchar(500))

### Table 2: `inventory.FabricRolls` (Barcode Tracking)
* `Id` (Guid, PK)
* `RollBarcode` (varchar(50), Unique, Index, e.g. `ROL-2026-00891`)
* `StockLotId` (Guid, FK)
* `SupplierRollNo` (nvarchar(50))
* `Yardage` (decimal(18,2), e.g. 102.50)
* `CuttableWidthInches` (decimal(6,2), e.g. 58.00)
* `MeasuredGSM` (decimal(6,2), e.g. 262.00)
* `ShadeGroup` (nvarchar(10), e.g. "A", "B", "C")
* `ShrinkageLengthPct` (decimal(5,2), e.g. -2.50%)
* `ShrinkageWidthPct` (decimal(5,2), e.g. -1.20%)
* `LocationCode` (nvarchar(30), e.g. `WH1-A03-R02-T01-B04`)
* `Status` (nvarchar(20), e.g. `QUARANTINE`, `AVAILABLE`, `ISSUED_TO_CUTTING`, `CONSUMED`)

### Table 3: `inventory.CustomsBondLedger`
* `Id` (Guid, PK)
* `BondRegisterNumber` (varchar(50), Index)
* `UtilizationPermitNo` (varchar(50), e.g. `UP-2026-BOND-8891`)
* `TransactionType` (nvarchar(20), e.g. `IMPORT_CREDIT`, `FLOOR_ISSUE_DEBIT`, `EXPORT_RECONCILED`)
* `ReferenceDocType` (nvarchar(30), e.g. `GRN`, `STORE_ISSUE_NOTE`, `BILL_OF_EXPORT`)
* `ReferenceDocNumber` (nvarchar(50))
* `Quantity` (decimal(18,4))
* `RemainingUPBalance` (decimal(18,4))
* `TransactionDate` (datetime2)

---

## 4. Quarantine Release Workflow Logic

```csharp
public async Task<Result> FinalizeFabricInspectionAsync(Guid inspectionId)
{
    var inspection = await _dbContext.FabricInspections
        .Include(fi => fi.InspectedRolls)
        .FirstOrDefaultAsync(fi => fi.Id == inspectionId);

    if (inspection == null) return Result.Failure("Inspection record not found.");

    if (inspection.AveragePointsPer100SqYds <= 28.0m)
    {
        inspection.OverallResult = "PASSED";

        // Transition StockLot state
        var stockLot = await _dbContext.StockLots.FindAsync(inspection.StockLotId);
        stockLot.StockState = "RELEASED_FOR_PRODUCTION";
        stockLot.QCInspectionStatus = "PASSED";
        stockLot.QCPassedAtUtc = DateTime.UtcNow;

        // Transition individual rolls to AVAILABLE
        foreach (var roll in stockLot.FabricRolls)
        {
            roll.Status = "AVAILABLE";
        }

        // Publish event for Cutting Department
        await _publishEndpoint.Publish(new StockLotReleasedForCuttingEvent(stockLot.Id, stockLot.BOMItemId));
        
        return Result.Success("Inspection PASSED: Stock released for cutting floor allocation.");
    }
    else
    {
        inspection.OverallResult = "REJECTED";
        
        var stockLot = await _dbContext.StockLots.FindAsync(inspection.StockLotId);
        stockLot.StockState = "REJECTED_QUARANTINE";
        stockLot.QCInspectionStatus = "FAILED";

        return Result.Failure($"Inspection FAILED: Score {inspection.AveragePointsPer100SqYds:F2} pts/100 sq.yds exceeds 28.0 threshold. Materials locked in Quarantine.");
    }
}
```

---

## 5. Production Demo Data: Inspection & Warehouse Layout

### A. Inspection Summary (`QC-4PT-2026-0045`)
* **Associated Lot:** `LOT-TW-2026-01` (`GRN-2026-0210`, Twill 260 GSM)
* **Lot Size:** 368 Rolls (36,750 Yards)
* **Audited Sample:** 37 Rolls (3,700 Yards)
* **Average Defect Points:** **3.85 points / 100 sq. yards** (Far below 28.0 ceiling)
* **Overall Outcome:** **PASSED**
* **Shade Group Allocation:**
  * **Shade A (Standard Navy/Khaki):** 240 Rolls (65.2%)
  * **Shade B (Slightly Lighter Hue):** 115 Rolls (31.3%)
  * **Shade C (Slightly Darker Hue):** 13 Rolls (3.5%) — *Tagged for separate cutting lay planning*.

### B. 3D Warehouse Coordinate Map
* **Assigned Location:** Warehouse 1, Aisle 3, Racks 2–5.
* **Format:** `WH1-A03-R02-T01-B01` through `WH1-A03-R05-T03-B04`.
* **Bond Register:** Credited 36,750 Yards to `UP-2026-BOND-8891`.
