# StitchMind Module Specification
## Module 05: Sewing Floor Management, Industrial Engineering (IE) & Real-Time Andon

---

| Metadata Field | Value |
|---|---|
| **Module ID** | `MOD-05` / `Phase E` (Sprint 17–22) |
| **BRD Reference** | Section 4, Module 6 (Sewing Floor Management) |
| **WBS Reference** | `IE-01`, `SEW-01` through `SEW-06`, `SEW-RPT` |
| **Acceptance Criteria** | `AC-05` (Sub-300ms Scanning & Real-Time Andon Updates) |
| **Status** | Approved Specification |

---

## 1. Business Purpose & Scope

The Sewing Floor is where factory labor generates primary value. In an export apparel facility, small line imbalances or scanning latencies directly compound into missed shipment deadlines and expensive air freights.

Module 05 delivers:
1. **IE Operation Bulletin & SMV Breakdown:** Establishes standard operation benchmarks (SMV/SAM) and machinery requirements for every garment style.
2. **Pitch Diagram & Real-Time Line Balancing:** Identifies station bottlenecks and guides dynamic operator reallocation.
3. **Sub-300ms Barcode Bundle Tracking:** High-speed scanner endpoint recording bundle progress, calculating WIP, and crediting piece-rate earnings.
4. **Digital Andon Display (SignalR / WebSockets):** Real-time overhead production displays showing live hourly output, running efficiency %, and DHU defect rates.
5. **Broken Needle Compliance Gate:** Enforces complete fragment recovery verification before replacement sewing needles can be dispensed.

---

## 2. Real-Time Scanning & Andon Broadcast Architecture

```
[Floor Laser/Tablet Scanner]
              │
              ▼ (< 300ms target response time)
[Fast Scan API Endpoint (Dapper + In-Memory Check)]
              │
              ├──► Writes to database: `sewing.BundleScans`
              ├──► Updates operator piece-rate ledger
              │
              ▼
[RabbitMQ / MassTransit: `BundleProcessedEvent`]
              │
              ├──► Background Aggregator: Updates Hourly Line Output & Efficiency
              │
              ▼ (SignalR Hub Push)
[Overhead Digital Andon TV Displays] ──► Instant Visual Feedback on Floor
```

---

## 3. Database Schema & Entities

### Schema: `sewing`

```
  ┌──────────────────┐           ┌──────────────────┐
  │      Style       │1         *│ OperationBulletin│
  └────────┬─────────┘           └────────┬─────────┘
           │                              │1
           │*                             │*
  ┌────────▼─────────┐1         *┌────────▼─────────┐
  │    SewingLine    ├───────────► LineAllocation   │
  └────────┬─────────┘           └──────────────────┘
           │1
           │*
  ┌────────▼─────────┐1         *┌──────────────────┐
  │    BundleScan    ├───────────► BrokenNeedleLog  │
  └──────────────────┘           └──────────────────┘
```

### Table 1: `sewing.OperationBulletins` (IE Standards)
* `Id` (Guid, PK)
* `StyleId` (Guid, FK -> `merchandising.Styles.Id`)
* `OperationSequence` (int, e.g. 1, 2, 3...)
* `OperationName` (nvarchar(100), e.g. "Waistband Attachment")
* `MachineType` (nvarchar(50), e.g. "Multi-Needle Chainstitch")
* `StandardMinuteValueSMV` (decimal(6,3), e.g. 1.100)
* `TargetPerHour` (int, Formula: `60 / SMV`, e.g. 54 pcs)
* `RequiredSkillGrade` (nvarchar(10), e.g. "A+", "A", "B", "C")
* `PieceRatePerDzn` (decimal(18,4), e.g. $0.8500)

### Table 2: `sewing.SewingLines`
* `Id` (Guid, PK)
* `LineNumber` (nvarchar(20), Unique, e.g. "LINE-04")
* `FloorNumber` (nvarchar(20), e.g. "Floor 2 - Building B")
* `ActiveStyleId` (Guid, FK, null)
* `TotalWorkstations` (int, e.g. 50)
* `CurrentShift` (nvarchar(10), e.g. "Shift A")
* `TargetEfficiencyPct` (decimal(5,2), e.g. 75.00%)
* `PlannedOutputPerHour` (int, e.g. 120)

### Table 3: `sewing.BundleScans` (Sub-300ms Scanning Table)
* `Id` (bigint, Identity, PK)
* `BundleBarcode` (varchar(50), Index, e.g. `BND-2026-0001-01`)
* `LineId` (Guid, FK, Index)
* `OperatorBadgeId` (varchar(30), Index)
* `OperationId` (Guid, FK)
* `PiecesCount` (int, e.g. 24)
* `ScanTimestampUtc` (datetime2, Index)
* `ProcessingTimeMs` (int)

### Table 4: `sewing.BrokenNeedleLogs` (Social Compliance)
* `Id` (Guid, PK)
* `LogNumber` (varchar(30), Unique, e.g. `BNL-2026-0089`)
* `LineId` (Guid, FK)
* `OperatorBadgeId` (varchar(30))
* `MachineBarcode` (varchar(30))
* `BreakageReason` (nvarchar(100))
* `AllFragmentsRecovered` (bit)
* `SupervisorVerificationBadge` (varchar(30))
* `LoggedAtUtc` (datetime2)
* `ReplacementNeedleIssued` (bit)

---

## 4. Key Business Logic: Fast Bundle Scanning Implementation

```csharp
public async Task<ScanResult> RecordBundleScanAsync(ScanBundleCommand command)
{
    var stopwatch = Stopwatch.StartNew();

    // 1. Fast read via Dapper for sub-300ms response
    const string sqlCheck = @"
        SELECT b.Id, b.QuantityInBundle, b.CurrentFloorStation 
        FROM cutting.CutBundles b WITH (NOLOCK)
        WHERE b.BundleBarcode = @Barcode";

    var bundle = await _dapperConnection.QueryFirstOrDefaultAsync<BundleDto>(sqlCheck, new { Barcode = command.BundleBarcode });

    if (bundle == null)
    {
        return ScanResult.Failed("INVALID_BARCODE", "Bundle ticket does not exist.");
    }

    // 2. Insert scan entry
    const string sqlInsert = @"
        INSERT INTO sewing.BundleScans (BundleBarcode, LineId, OperatorBadgeId, OperationId, PiecesCount, ScanTimestampUtc, ProcessingTimeMs)
        VALUES (@Barcode, @LineId, @BadgeId, @OpId, @Qty, SYSUTCDATETIME(), @Elapsed);";

    stopwatch.Stop();
    await _dapperConnection.ExecuteAsync(sqlInsert, new {
        Barcode = command.BundleBarcode,
        LineId = command.LineId,
        BadgeId = command.OperatorBadgeId,
        OpId = command.OperationId,
        Qty = bundle.QuantityInBundle,
        Elapsed = (int)stopwatch.ElapsedMilliseconds
    });

    // 3. Fire-and-forget event bus publish for Andon display broadcast
    _ = _publishEndpoint.Publish(new BundleScannedEvent(
        command.LineId, 
        command.BundleBarcode, 
        bundle.QuantityInBundle, 
        command.OperationId
    ));

    return ScanResult.Success(bundle.QuantityInBundle, (int)stopwatch.ElapsedMilliseconds);
}
```

---

## 5. Production Demo Data: Line 04 Performance

* **Active Style:** `ST-2026-CHINO-01` (Men's Slim Fit Chino, SMV: 18.50)
* **Manpower:** 50 Operators + 2 Line Supervisors + 2 In-line QC
* **Hour 5 Metrics (1:00 PM – 2:00 PM):**
  * Target: 120 Pcs | **Actual Scanned Output: 126 Pcs (105% of target)**
  * Running Daily Efficiency: **78.4%** (Target: 75.0%)
  * Defects Caught in Line: 4 minor stitches out of 190 inspected (**DHU: 2.10%**)
* **Broken Needle Incident:**
  * 1 broken needle on Machine `SN-084` at 10:15 AM.
  * All 3 fragments (Tip, Shank, Eye) recovered and verified by Supervisor `SUP-012`.
  * Replacement needle dispensed at 10:19 AM (Downtime: 4 minutes).
