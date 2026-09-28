# StitchMind Module Specification
## Module 07: Commercial Finance, Trade LCs & Automated Three-Way Matching

---

| Metadata Field | Value |
|---|---|
| **Module ID** | `MOD-07` / `Phase G` (Sprint 26–30) |
| **BRD Reference** | Section 4, Module 8 (Accounting, Commercial & Finance) |
| **WBS Reference** | `ACC-01` through `ACC-07`, `ACC-RPT` |
| **Acceptance Criteria** | `AC-06` (Automated 3-Way Matching Verification) |
| **Status** | Approved Specification |

---

## 1. Business Purpose & Scope

In apparel export manufacturing, tight margins (typically 8% to 12%) mean that financial discipline is paramount. Paying for unreceived fabric, accepting supplier price markups, or losing track of Back-to-Back (B2B) LC liabilities can quickly turn a profitable garment contract into a loss.

Module 07 delivers:
1. **Multi-layer Chart of Accounts (COA):** Double-entry accounting tracking assets, liabilities, revenues, and cost centers.
2. **Automated Three-Way Matching:** Programmatic validation reconciling Purchase Order prices, GRN verified quantities, and Vendor Bills before authorizing payment.
3. **Trade Finance Management:** Tracking Master LCs, Back-to-Back (B2B) LCs, Foreign Demand Bill Discounting (FDBP), and cash incentive subsidies.
4. **Order-Wise Costing Variance Engine:** Real-time post-costing analysis comparing actual raw materials, direct labor, and overheads against the pre-costing budget.
5. **Multi-Currency & Forex Ledger:** Native accounting in USD, EUR, and GBP with realized/unrealized foreign exchange gain/loss calculations.

---

## 2. Automated Three-Way Matching Architecture

```
[Supplier Submits Bill / Invoice]
               │
               ▼
┌─────────────────────────────────────────────────────────────┐
│ StitchMind 3-Way Matching Engine                            │
├─────────────────────────────────────────────────────────────┤
│ 1. Validate PO: Approved Unit Price == Billed Unit Price?    │
│ 2. Validate GRN: Received & QC-Passed Qty >= Billed Qty?    │
│ 3. Validate LC: Active B2B LC allocation available?        │
└──────────────┬──────────────────────────────┬───────────────┘
               │ (MATCHED)                    │ (MISMATCHED)
               ▼                              ▼
┌─────────────────────────────┐┌──────────────────────────────┐
│ Status: APPROVED_FOR_PAYMENT││ Status: MATCH_FAILED_LOCKED  │
│ Auto-generates AP voucher   ││ Discrepancy report sent to AP│
└─────────────────────────────┘└──────────────────────────────┘
```

---

## 3. Database Schema & Entities

### Schema: `finance`

```
  ┌──────────────────┐           ┌──────────────────┐
  │ ChartOfAccounts  │1         *│   JournalEntry   │
  └────────┬─────────┘           └────────┬─────────┘
           │                              │1
           │*                             │*
  ┌────────▼─────────┐1         *┌────────▼─────────┐
  │   VendorBill     ├───────────► ThreeWayMatchLog │
  └────────┬─────────┘           └──────────────────┘
           │1
           │*
  ┌────────▼─────────┐
  │ CostingVariance  │
  └──────────────────┘
```

### Table 1: `finance.VendorBills` (Accounts Payable)
* `Id` (Guid, PK)
* `BillNumber` (varchar(50), Unique, Index, e.g. `BILL-APEX-2026-091`)
* `SupplierId` (Guid, FK -> `procurement.Suppliers.Id`)
* `PurchaseOrderId` (Guid, FK -> `procurement.PurchaseOrders.Id`)
* `GRNId` (Guid, FK -> `inventory.GoodsReceiptNotes.Id`)
* `InvoiceDate` (date)
* `DueDate` (date)
* `BilledAmount` (decimal(18,4))
* `Currency` (nvarchar(3), default "USD")
* `MatchStatus` (nvarchar(30), default "PENDING_MATCH")
* `IsApprovedForPayment` (bit, default 0)
* `ApprovedBy` (nvarchar(100), null)
* `ApprovedAtUtc` (datetime2, null)

### Table 2: `finance.ThreeWayMatchLogs` (AC-06 Verification)
* `Id` (Guid, PK)
* `VendorBillId` (Guid, FK)
* `POUnitPrice` (decimal(18,4))
* `BilledUnitPrice` (decimal(18,4))
* `PriceVariancePct` (decimal(5,2))
* `GRNPassedQuantity` (decimal(18,4))
* `BilledQuantity` (decimal(18,4))
* `QuantityVariance` (decimal(18,4))
* `IsPriceMatched` (bit)
* `IsQuantityMatched` (bit)
* `OverallMatchResult` (nvarchar(20), e.g. `PASSED`, `DISCREPANCY_FLAGGED`)
* `DiscrepancyNotes` (nvarchar(500), null)

### Table 3: `finance.OrderCostingVariances` (Post-Costing Analysis)
* `Id` (Guid, PK)
* `StyleId` (Guid, FK -> `merchandising.Styles.Id`, Unique)
* `BuyerFOBRevenue` (decimal(18,4))
* `BudgetRawMaterialsCost` (decimal(18,4))
* `ActualRawMaterialsCost` (decimal(18,4))
* `RawMaterialVariance` (decimal(18,4), Formula: `Budget - Actual`)
* `BudgetDirectLaborCost` (decimal(18,4))
* `ActualDirectLaborCost` (decimal(18,4))
* `DirectLaborVariance` (decimal(18,4))
* `BudgetOverheadsCost` (decimal(18,4))
* `ActualOverheadsCost` (decimal(18,4))
* `NetRealizedProfit` (decimal(18,4))
* `NetProfitMarginPct` (decimal(5,2))

---

## 4. Key Business Logic: Automated Three-Way Matching Implementation

```csharp
public async Task<Result> ExecuteThreeWayMatchAsync(Guid vendorBillId)
{
    var bill = await _dbContext.VendorBills
        .Include(b => b.PurchaseOrder)
        .Include(b => b.GRN)
            .ThenInclude(g => g.StockLots)
        .FirstOrDefaultAsync(b => b.Id == vendorBillId);

    if (bill == null) return Result.Failure("Vendor bill record not found.");

    // Retrieve approved PO item details
    var poLine = await _dbContext.PurchaseOrderLines
        .FirstOrDefaultAsync(p => p.PurchaseOrderId == bill.PurchaseOrderId);

    // Retrieve QC-passed quantity from GRN lots
    var grnPassedQty = bill.GRN.StockLots
        .Where(sl => sl.QCInspectionStatus == "PASSED")
        .Sum(sl => sl.Quantity);

    // Evaluate 3-Way Match conditions
    bool priceMatch = (bill.BilledUnitPrice <= poLine.UnitPrice);
    bool quantityMatch = (bill.BilledQuantity <= grnPassedQty);

    var matchLog = new ThreeWayMatchLog
    {
        Id = Guid.NewGuid(),
        VendorBillId = bill.Id,
        POUnitPrice = poLine.UnitPrice,
        BilledUnitPrice = bill.BilledUnitPrice,
        GRNPassedQuantity = grnPassedQty,
        BilledQuantity = bill.BilledQuantity,
        IsPriceMatched = priceMatch,
        IsQuantityMatched = quantityMatch
    };

    if (priceMatch && quantityMatch)
    {
        matchLog.OverallMatchResult = "PASSED";
        bill.MatchStatus = "APPROVED_FOR_PAYMENT";
        bill.IsApprovedForPayment = true;
        bill.ApprovedAtUtc = DateTime.UtcNow;

        _dbContext.ThreeWayMatchLogs.Add(matchLog);
        await _dbContext.SaveChangesAsync();

        return Result.Success("Three-Way Match PASSED: Bill authorized for bank payment scheduling.");
    }
    else
    {
        matchLog.OverallMatchResult = "DISCREPANCY_FLAGGED";
        bill.MatchStatus = "MATCH_FAILED_LOCKED";
        bill.IsApprovedForPayment = false;

        _dbContext.ThreeWayMatchLogs.Add(matchLog);
        await _dbContext.SaveChangesAsync();

        return Result.Failure("Three-Way Match FAILED: Invoice price or quantity exceeds approved PO/GRN values. Bill locked.");
    }
}
```

---

## 5. Production Demo Data: Financial Settlement Record

### A. Three-Way Match: Apex Textile Mills (`BILL-APEX-2026-091`)
* **Purchase Order (`PO-2026-0145`):** Approved Rate = **$2.3500 / Yd** | Qty = 36,750 Yds.
* **Goods Receipt (`GRN-2026-0210`):** QC-Passed Quantity = **36,750 Yds** (Zero rejects).
* **Vendor Bill:** Billed Rate = **$2.3500 / Yd** | Billed Qty = **36,750 Yds** = **$86,362.50 USD**.
* **Three-Way Match Evaluation:**
  * Price Match: **TRUE ($2.35 == $2.35)**
  * Qty Match: **TRUE (36,750 == 36,750)**
  * Outcome: **PASSED & APPROVED FOR PAYMENT**.

### B. Trade Finance Settlement
* **Export Master LC Realization:** Swedbank Sweden settles **$180,000.00 USD** upon presentation of clean Bill of Lading.
* **B2B LC Settlement:** Standard Chartered Bank debits **$86,362.50 USD** to settle Apex Textile Mills B2B LC `B2B-2026-0098`.
* **Realized Gross Trade Margin:** **$93,637.50 USD**.
* **Net Realized Factory Profit for Style `ST-2026-CHINO-01`:** **$20,527.50 USD (11.40% margin)**.
