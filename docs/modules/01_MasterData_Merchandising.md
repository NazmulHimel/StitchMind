# StitchMind Module Specification
## Module 01: Master Data, Merchandising & Pre-Costing Engine

---

| Metadata Field | Value |
|---|---|
| **Module ID** | `MOD-01` / `Phase A` (Sprint 3–5) |
| **BRD Reference** | Section 4, Module 2 & Module 10 |
| **WBS Reference** | `SEC-06` (Master Data), `PRO-01` (MR/Style), `PRO-03` (Budget Lock) |
| **Status** | Approved Specification |

---

## 1. Business Purpose & Scope

The Merchandising & Master Data module serves as the commercial backbone of StitchMind. It captures buyer order commitments, defines physical garment specifications (BOM, colorways, size breakdown), and calculates the baseline **Pre-Costing Sheet** with mandatory budget caps.

Every subsequent operational stage — purchasing yarn and trims, issuing fabric rolls to the cutting floor, balancing sewing lines, and calculating double-entry AP bills — is strictly validated against the active Style and Pre-Costing budget defined in this module.

---

## 2. Core Domain Entities & Database Schema

### Schema: `merchandising`

```
  ┌───────────────┐           ┌───────────────┐
  │     Buyer     │1         *│     Brand     │
  └───────┬───────┘           └───────────────┘
          │1
          │*
  ┌───────▼───────┐1         *┌───────────────┐
  │     Style     ├───────────►   ColorWay    │
  └───────┬───────┘           └───────────────┘
          │1                  ┌───────────────┐
          ├──────────────────►│   SizeScale   │
          │1                 *└───────────────┘
          ├──────────────────►┌───────────────┐
          │1                 *│    BOMItem    │
          │                   └───────────────┘
          │1
  ┌───────▼───────┐1         *┌───────────────┐
  │  PreCosting   ├───────────►  CostingLine  │
  └───────────────┘           └───────────────┘
```

### Table 1: `merchandising.Buyers`
* `Id` (Guid, PK)
* `BuyerCode` (varchar(20), Unique, e.g. `BYR-001`)
* `BuyerName` (nvarchar(100), e.g. "Nordic Fashion Retail AB")
* `Country` (nvarchar(50), e.g. "Sweden")
* `PaymentTerms` (nvarchar(50), e.g. "At Sight LC")
* `CreditPeriodDays` (int, default 0)
* `IsActive` (bit, default 1)
* `CreatedAtUtc` (datetime2)
* `CreatedBy` (nvarchar(100))

### Table 2: `merchandising.Styles`
* `Id` (Guid, PK)
* `BuyerId` (Guid, FK -> `merchandising.Buyers.Id`)
* `StyleNumber` (nvarchar(50), Index, e.g. `ST-2026-CHINO-01`)
* `StyleDescription` (nvarchar(250))
* `ProductCategory` (nvarchar(50), e.g. "Bottoms (Woven)")
* `FabricComposition` (nvarchar(150), e.g. "98% Cotton, 2% Spandex Twill, 260 GSM")
* `Season` (nvarchar(30), e.g. "Spring / Summer 2026")
* `OrderQuantity` (int, total pieces, e.g. 24,000)
* `FOBPricePerPc` (decimal(18,4), e.g. 7.5000 USD)
* `FOBPricePerDzn` (decimal(18,4), e.g. 90.0000 USD)
* `Currency` (nvarchar(3), default "USD")
* `ExFactoryDate` (date)
* `Status` (nvarchar(30), e.g. "Draft", "CostingApproved", "InProduction", "Shipped", "Closed")

### Table 3: `merchandising.StyleColorSizes` (Matrix Order Breakdown)
* `Id` (Guid, PK)
* `StyleId` (Guid, FK)
* `ColorName` (nvarchar(50), e.g. "Khaki Beige")
* `PantoneCode` (nvarchar(30), e.g. "16-1108 TCX")
* `SizeLabel` (nvarchar(20), e.g. "32/32")
* `Quantity` (int, e.g. 3,200)

### Table 4: `merchandising.BOMItems` (Bill of Materials)
* `Id` (Guid, PK)
* `StyleId` (Guid, FK)
* `ItemCategory` (nvarchar(50), e.g. "Fabric", "Thread", "Trims", "Labels", "Packaging")
* `ItemCode` (nvarchar(30), e.g. `FAB-TW-01`)
* `ItemDescription` (nvarchar(200))
* `UnitOfMeasure` (nvarchar(20), e.g. "Yards", "Cones", "Gross", "Pieces")
* `NetConsumptionPerDzn` (decimal(18,4))
* `WastagePercentage` (decimal(5,2), e.g. 5.00%)
* `GrossConsumptionPerDzn` (decimal(18,4), Formula: `Net * (1 + Wastage/100)`)
* `TotalRequiredQty` (decimal(18,4), Formula: `(OrderQuantity / 12) * GrossConsumptionPerDzn`)

### Table 5: `merchandising.PreCostings` & `merchandising.CostingLines`
* `Id` (Guid, PK)
* `StyleId` (Guid, FK, Unique)
* `CostingVersion` (int, default 1)
* `CostingBasis` (nvarchar(20), default "PerDozen")
* `FOBPricePerDzn` (decimal(18,4), e.g. 90.00)
* `TotalMaterialsBudget` (decimal(18,4), e.g. 53.40)
* `CostOfMakingCM` (decimal(18,4), e.g. 15.00)
* `WashingCost` (decimal(18,4), e.g. 6.00)
* `FactoryOverheads` (decimal(18,4), e.g. 4.80)
* `CommercialCost` (decimal(18,4), e.g. 1.80)
* `TargetNetProfit` (decimal(18,4), e.g. 9.00)
* `IsLocked` (bit, default 0)
* `ApprovedBy` (nvarchar(100))
* `ApprovedAtUtc` (datetime2)

**`CostingLines` (Per-Item Budget Caps):**
* `Id` (Guid, PK)
* `PreCostingId` (Guid, FK)
* `BOMItemId` (Guid, FK)
* `MaxBudgetUnitPrice` (decimal(18,4))  <-- HARD BUDGET CAP ENFORCED ON PO CREATION
* `BudgetTotalAmount` (decimal(18,4))

---

## 3. Pre-Costing Budget Lock Algorithm

```csharp
public async Task<Result> ValidatePurchaseOrderBudgetAsync(Guid styleId, Guid bomItemId, decimal proposedUnitPrice)
{
    var costingLine = await _dbContext.CostingLines
        .AsNoTracking()
        .FirstOrDefaultAsync(cl => cl.BOMItemId == bomItemId && cl.PreCosting.StyleId == styleId);

    if (costingLine == null)
    {
        return Result.Failure("No approved pre-costing line found for this item and style.");
    }

    if (proposedUnitPrice > costingLine.MaxBudgetUnitPrice)
    {
        decimal variancePct = ((proposedUnitPrice - costingLine.MaxBudgetUnitPrice) / costingLine.MaxBudgetUnitPrice) * 100m;
        
        return Result.Failure(
            $"HARD LOCK VIOLATION: Proposed price {proposedUnitPrice:C4} exceeds pre-costing budget of {costingLine.MaxBudgetUnitPrice:C4} by {variancePct:F2}%. " +
            "Purchase Order locked. CFO authorization required.");
    }

    return Result.Success();
}
```

---

## 4. Production Master Demo Data Record

### Style: `ST-2026-CHINO-01`
* **Customer:** Nordic Fashion Retail AB (`BYR-001`)
* **Order Qty:** 24,000 Pieces (2,000 Dozen)
* **Buyer FOB:** $7.50 / pc ($90.00 / Dozen) = $180,000.00 USD
* **BOM Breakdown & Pre-Costing Caps:**

| Item Code | Description | UOM | Gross / Dzn | Total Demand | Max Budget Unit Rate | Budget Cost / Dzn |
|---|---|---|---|---|---|---|
| `FAB-TW-01` | 98/2 Cotton Spandex Twill 260 GSM | Yards | 18.375 | 36,750 Yds | **$2.4000 / Yd** | $44.10 |
| `FAB-PK-01` | 65/35 TC Pocketing Fabric 120 GSM | Yards | 3.090 | 6,180 Yds | **$0.8500 / Yd** | $2.63 |
| `THR-SP-40` | 100% Spun Poly Thread 40/2 | Cones | 0.816 | 1,632 Cones | **$2.1000 / Cone** | $1.71 |
| `BTN-MH-01` | 24L Metal Shank Button | Gross | 0.0858 | 171.6 Grs | **$1.8000 / Grs** | $0.15 |
| `ZIP-MT-05` | #5 Brass Metal Zipper 6.5" | Pieces | 12.120 | 24,240 Pcs | **$0.1650 / Pc** | $2.00 |
| `LBL-MN-01` | Woven Main Brand Label | Pieces | 12.240 | 24,480 Pcs | **$0.0400 / Pc** | $0.49 |
| `LBL-CR-01` | Printed Care Label | Pieces | 12.240 | 24,480 Pcs | **$0.0200 / Pc** | $0.24 |
| `PKG-PB-01` | Polybag 20"x14" | Pieces | 12.240 | 24,480 Pcs | **$0.0350 / Pc** | $0.43 |
| `PKG-CT-01` | 7-Ply Master Carton | Pieces | 0.510 | 1,020 Pcs | **$1.4500 / Pc** | $0.74 |

* **Total Raw Materials Budget per Dozen:** **$53.40** ($106,800.00 for order)
* **Direct Labor (CM):** **$15.00 / Dzn** ($30,000.00 for order)
* **Washing:** **$6.00 / Dzn** ($12,000.00 for order)
* **Overheads:** **$4.80 / Dzn** ($9,600.00 for order)
* **Commercial / Banking:** **$1.80 / Dzn** ($3,600.00 for order)
* **Factory Net Margin:** **$9.00 / Dzn (10.00%)** ($18,000.00 net profit)
