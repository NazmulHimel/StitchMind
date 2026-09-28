# StitchMind Module Specification
## Module 06: Finishing, Packing, Pre-Shipment AQL & Ex-Factory Dispatch

---

| Metadata Field | Value |
|---|---|
| **Module ID** | `MOD-06` / `Phase F` (Sprint 23–25) |
| **BRD Reference** | Section 4, Module 4 (Stock & Shipment) & Module 7 (QC Pre-Shipment) |
| **WBS Reference** | `QC-04`, `SHP-01` through `SHP-04`, `SHP-RPT` |
| **Acceptance Criteria** | `AC-06` (Pre-cursor to 3-Way Match & Revenue Recognition) |
| **Status** | Approved Specification |

---

## 1. Business Purpose & Scope

The Finishing, Packing, and Dispatch module represents the final operational custody gate of export manufacturing. Once goods depart the factory gate, any discrepancy in quantity, carton gross weight, or garment quality results in costly buyer chargebacks or rejected shipments.

Module 06 manages:
1. **Washing & Finishing Execution:** Tracks garment washing (Enzyme/Silicone wash), thread trimming, and pressing.
2. **Pre-Shipment AQL Audit (ISO 2859-1):** Enforces international statistical sampling standards for Major, Minor, and Critical defects.
3. **Carton Packing & Gross Weight Scale Verification:** Validates that every carton contains exact units and sizes by cross-referencing automated weight scales against calculated benchmarks.
4. **Container Loading Barcode Audit:** Verifies carton barcodes as they are loaded into 20ft/40ft shipping containers.
5. **Commercial Ex-Factory Gate Pass & Status Lock:** Issues the statutory outward gate pass and transitions the Style order to `SHIPPED`, locking inventory and unlocking commercial invoicing.

---

## 2. Pre-Shipment AQL Inspection Mathematical Model (ISO 2859-1 Level II)

For Style `ST-2026-CHINO-01` with total order quantity **24,000 Pieces**:

### Sampling Determination
* **Lot Size Range:** 10,001 to 35,000 units.
* **Inspection Level:** General Inspection Level II (Standard Export Industry Norm).
* **Sample Size Code Letter:** **M**
* **Statutory Sample Size:** **315 Pieces**

### Acceptance / Rejection Thresholds (Normal Single Sampling)

| Defect Class | AQL Level | Max Allowed Defects (Ac) | Rejection Point (Re) |
|---|---|---|---|
| **Critical Defects** (Broken needle fragment, sharp wire, bio stain) | **0.00** | **0 Defects** | **1 Defect $\implies$ Immediate Lot Fail** |
| **Major Defects** (Open seam, out-of-tolerance measurement, shade band) | **2.50** | **$\le 14$ Defects (Accept)** | **$\ge 15$ Defects (Reject)** |
| **Minor Defects** (Loose uncut thread, crooked sticker, polybag crease) | **4.00** | **$\le 21$ Defects (Accept)** | **$\ge 22$ Defects (Reject)** |

---

## 3. Database Schema & Entities

### Schema: `shipping` & `quality`

```
  ┌──────────────────┐           ┌──────────────────┐
  │      Style       │1         *│    AQLInspection │
  └────────┬─────────┘           └────────┬─────────┘
           │                              │ (Result = PASSED)
           ▼                              ▼
  ┌──────────────────┐1         *┌──────────────────┐
  │   PackingPlan    ├───────────►   MasterCarton   │
  └────────┬─────────┘           └────────┬─────────┘
           │1                             │1
           │*                             │*
  ┌────────▼─────────┐           ┌────────▼─────────┐
  │ ExFactoryGatePass│           │ ContainerLoading │
  └──────────────────┘           └──────────────────┘
```

### Table 1: `quality.AQLInspections`
* `Id` (Guid, PK)
* `InspectionCertificateNo` (varchar(30), Unique, e.g. `AQL-2026-0038`)
* `StyleId` (Guid, FK)
* `LotSizeQuantity` (int, e.g. 24,000)
* `SampleSize` (int, e.g. 315)
* `CriticalDefectsFound` (int)
* `MajorDefectsFound` (int)
* `MinorDefectsFound` (int)
* `OverallResult` (nvarchar(20), e.g. `PASSED`, `REJECTED`)
* `AuditorAgency` (nvarchar(100), e.g. "Third Party Intertek / Factory QA")
* `AuditedAtUtc` (datetime2)

### Table 2: `shipping.MasterCartons`
* `Id` (Guid, PK)
* `CartonBarcode` (varchar(50), Unique, Index, e.g. `CTN-2026-0001`)
* `StyleId` (Guid, FK)
* `CartonSequenceNumber` (int, e.g. 1 of 1,000)
* `ColorName` (nvarchar(50))
* `SizeBreakdownJson` (nvarchar(500), e.g. `{"32/32": 24}`)
* `UnitsPerCarton` (int, default 24)
* `TareCartonWeightKg` (decimal(6,3), e.g. 1.200 kg)
* `ExpectedNetWeightKg` (decimal(6,3), e.g. 12.480 kg)
* `ExpectedGrossWeightKg` (decimal(6,3), e.g. 13.680 kg)
* `ActualMeasuredWeightKg` (decimal(6,3))
* `WeightVariancePct` (decimal(5,2))
* `WeightAuditStatus` (nvarchar(20), e.g. `WEIGHT_VERIFIED`, `VARIANCE_FLAGGED`)
* `ContainerNumber` (nvarchar(30), null)

### Table 3: `shipping.ExFactoryGatePasses`
* `Id` (Guid, PK)
* `GatePassNumber` (varchar(30), Unique, e.g. `EGP-2026-0072`)
* `StyleId` (Guid, FK)
* `BuyerId` (Guid, FK)
* `CommercialInvoiceNo` (nvarchar(50))
* `TotalCartonsShipped` (int, e.g. 1,000)
* `TotalPiecesShipped` (int, e.g. 24,000)
* `ContainerNumber` (nvarchar(30), e.g. `MSCU-782194-0`)
* `TruckPlateNumber` (nvarchar(30), e.g. `Dhaka Metro-T 14-8891`)
* `CustomsSealNumber` (nvarchar(30), e.g. `SEAL-BD-99120`)
* `DispatchedAtUtc` (datetime2)
* `Status` (nvarchar(20), default "SHIPPED")

---

## 4. Key Business Logic: Ex-Factory Order 'Shipped' Status Lock

```csharp
public async Task<Result> AuthorizeExFactoryGatePassAsync(Guid gatePassId)
{
    var gatePass = await _dbContext.ExFactoryGatePasses
        .Include(gp => gp.Style)
        .FirstOrDefaultAsync(gp => gp.Id == gatePassId);

    if (gatePass == null) return Result.Failure("Gate pass record not found.");

    // 1. Verify AQL Certificate is PASSED
    var aqlPassed = await _dbContext.AQLInspections
        .AnyAsync(a => a.StyleId == gatePass.StyleId && a.OverallResult == "PASSED");

    if (!aqlPassed)
    {
        return Result.Failure("DISPATCH BLOCKED: Order does not have a verified PASSED AQL Inspection Certificate.");
    }

    // 2. Verify all cartons passed weight scale check
    var unverifiedCartons = await _dbContext.MasterCartons
        .CountAsync(c => c.StyleId == gatePass.StyleId && c.WeightAuditStatus != "WEIGHT_VERIFIED");

    if (unverifiedCartons > 0)
    {
        return Result.Failure($"DISPATCH BLOCKED: {unverifiedCartons} cartons failed weight variance checks. Physical audit required.");
    }

    // 3. Lock Style order status to SHIPPED
    gatePass.Status = "SHIPPED";
    gatePass.Style.Status = "Shipped";
    gatePass.DispatchedAtUtc = DateTime.UtcNow;

    await _dbContext.SaveChangesAsync();

    // 4. Trigger Commercial Finance Invoice creation event
    await _publishEndpoint.Publish(new OrderExFactoryShippedEvent(
        gatePass.StyleId, 
        gatePass.GatePassNumber, 
        gatePass.TotalPiecesShipped
    ));

    return Result.Success("Ex-Factory Gate Pass authorized. Style locked as SHIPPED. Accounts Receivable triggered.");
}
```

---

## 5. Production Demo Data: Shipment Execution

### A. Pre-Shipment AQL Inspection Result (`AQL-2026-0038`)
* **Audited Sample:** 315 Random Garments (Audited by Senior QA + Buyer Rep).
* **Defect Counts Found:**
  * **Critical Defects:** **0** (Pass)
  * **Major Defects:** **3** (Tolerance: $\le 14 \implies$ **PASS**) — *2 loose bar-tacks, 1 slight shade variation on coin pocket*.
  * **Minor Defects:** **7** (Tolerance: $\le 21 \implies$ **PASS**) — *5 loose uncut threads, 2 folded care label corners*.
* **AQL Decision:** **PASSED & APPROVED FOR SHIPMENT**.

### B. Carton Packing & Scale Audit
* **Total Cartons:** 1,000 Master Cartons (24 Pieces / Carton = 24,000 Pieces).
* **Automated Floor Scale Check:**
  * Expected Weight per Carton: **13.680 kg** ($\pm 2.0\%$ allowed: 13.406 kg to 13.953 kg).
  * 1,000 / 1,000 Cartons weighed and tagged as `WEIGHT_VERIFIED`.
  * Zero cartons flagged for missing pieces.

### C. Commercial Ex-Factory Gate Pass (`EGP-2026-0072`)
* **Date:** 2026-11-14 (1 day ahead of 2026-11-15 Buyer deadline).
* **Truck / Container:** `MSCU-782194-0` (40ft High Cube Container) | Seal: `SEAL-BD-99120`.
* **Destination:** Chittagong Port $\rightarrow$ Port of Gothenburg (Sweden).
* **System Action:** Style status locked to `SHIPPED`.
