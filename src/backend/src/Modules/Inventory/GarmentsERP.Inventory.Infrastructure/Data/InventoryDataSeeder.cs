using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Inventory.Infrastructure.Data;

public static class InventoryDataSeeder
{
    public static async Task SeedAsync(InventoryDbContext db)
    {
        if (await db.InventoryItems.AnyAsync()) return;

        var defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // 1. Seed Inventory Items
        var itm1 = InventoryItem.Create("FAB-DNM-12OZ", "100% Cotton Indigo Denim (12oz)", "Heavyweight denim for 5-pocket jeans", ItemCategory.Fabric, StockUOM.Yards, 15000, 2000, 4000, 3.80m, "WH-A-RACK-01", defaultCompanyId);
        var itm2 = InventoryItem.Create("FAB-TWL-240", "Cotton Twill 240 GSM (Khaki)", "Chino trouser twill fabric", ItemCategory.Fabric, StockUOM.Yards, 8200, 1500, 2500, 2.90m, "WH-A-RACK-04", defaultCompanyId);
        var itm3 = InventoryItem.Create("FAB-JSY-180", "Single Jersey 100% Cotton 180 GSM (Black)", "T-shirt knit fabric", ItemCategory.Fabric, StockUOM.Kilograms, 3400, 500, 1000, 5.20m, "WH-B-RACK-02", defaultCompanyId);
        var itm4 = InventoryItem.Create("THD-EPIC-BLK", "Coats Epic 40/2 Spun Poly Thread (Black)", "All-purpose lockstitch sewing thread", ItemCategory.Thread, StockUOM.Cones, 1200, 200, 400, 2.20m, "WH-C-BIN-12", defaultCompanyId);
        var itm5 = InventoryItem.Create("THD-EPIC-WHT", "Coats Epic 40/2 Spun Poly Thread (White)", "All-purpose lockstitch sewing thread", ItemCategory.Thread, StockUOM.Cones, 950, 200, 400, 2.20m, "WH-C-BIN-13", defaultCompanyId);
        var itm6 = InventoryItem.Create("THD-EPIC-203", "Coats Dual Duty 20/3 Heavy Thread", "Heavy topstitch & waistband thread", ItemCategory.Thread, StockUOM.Cones, 180, 100, 250, 3.10m, "WH-C-BIN-14", defaultCompanyId); // Low stock!
        var itm7 = InventoryItem.Create("TRM-ZIP-001", "5 Inch Antique Brass Zipper (Closed-end)", "Fly zipper for denim jeans", ItemCategory.Trims, StockUOM.Pieces, 14500, 2000, 5000, 0.45m, "WH-D-BIN-01", defaultCompanyId);
        var itm8 = InventoryItem.Create("TRM-BTN-002", "17mm Donut Tack Button (Gunmetal)", "Waistband shank button", ItemCategory.Trims, StockUOM.Pieces, 3200, 5000, 8000, 0.08m, "WH-D-BIN-08", defaultCompanyId); // Low stock!
        var itm9 = InventoryItem.Create("PKG-TAG-001", "Recycled Craft Paper Hangtag", "Brand hangtag with cotton string", ItemCategory.Packaging, StockUOM.Pieces, 45000, 5000, 10000, 0.04m, "WH-E-PALLET-03", defaultCompanyId);

        await db.InventoryItems.AddRangeAsync(itm1, itm2, itm3, itm4, itm5, itm6, itm7, itm8, itm9);
        await db.SaveChangesAsync();

        // 2. Seed Fabric Rolls for FAB-DNM-12OZ
        var r1 = FabricRoll.Create("ROL-2026-001", itm1.Id, "LOT-IND-901", 385m, 58m, 120m, "RACK-01-A", defaultCompanyId, "SUP-ROL-881");
        var r2 = FabricRoll.Create("ROL-2026-002", itm1.Id, "LOT-IND-901", 385m, 58m, 135m, "RACK-01-B", defaultCompanyId, "SUP-ROL-882");
        var r3 = FabricRoll.Create("ROL-2026-003", itm1.Id, "LOT-IND-901", 380m, 58m, 140m, "RACK-01-C", defaultCompanyId, "SUP-ROL-883");
        var r4 = FabricRoll.Create("ROL-2026-004", itm1.Id, "LOT-IND-902", 390m, 58m, 125m, "RACK-01-D", defaultCompanyId, "SUP-ROL-884", grade: FabricGrade.GradeB);
        var r5 = FabricRoll.Create("ROL-2026-005", itm1.Id, "LOT-IND-902", 385m, 58m, 150m, "RACK-01-E", defaultCompanyId, "SUP-ROL-885");
        var r6 = FabricRoll.Create("ROL-2026-006", itm1.Id, "LOT-IND-902", 385m, 58m, 110m, "RACK-01-F", defaultCompanyId, "SUP-ROL-886");

        await db.FabricRolls.AddRangeAsync(r1, r2, r3, r4, r5, r6);

        // 3. Seed Stock Transactions
        var tx1 = StockTransaction.Create("TX-REC-001", itm1.Id, StockTransactionType.Receipt, 15000, 3.80m, "PIONEER-TRUCK", "WH-A-RACK-01", adminUserId, defaultCompanyId, "GRN-202609-1001", notes: "Received from Pioneer Denim GRN-202609-1001");
        var tx2 = StockTransaction.Create("TX-REC-002", itm4.Id, StockTransactionType.Receipt, 1200, 2.20m, "COATS-DELIVERY", "WH-C-BIN-12", adminUserId, defaultCompanyId, "GRN-202609-1002", notes: "Received sewing thread from Coats Bangladesh");
        var tx3 = StockTransaction.Create("TX-ISS-001", itm1.Id, StockTransactionType.IssueToCutting, 1200, 3.80m, "WH-A-RACK-01", "CUTTING-TABLE-01", adminUserId, defaultCompanyId, "CUT-2026-901", notes: "Issued 1,200 yds for cutting order CUT-2026-901");

        await db.StockTransactions.AddRangeAsync(tx1, tx2, tx3);

        // 4. Seed Thread Batches (Yarn & Thread Production Pipeline)
        var b1 = ThreadBatch.Create("YRN-2026-101", "40/2 Ne", "100% Spun Polyester", "BLK-001", "Jet Black", 1000m, defaultCompanyId, adminUserId, "High tenacity thread for Levi's contract");
        b1.AdvanceStage(ThreadStage.Spinning, 970m, 30m, "SPIN-04", "Kabir Hossain", "Ring spinning completed within tolerance");
        b1.AdvanceStage(ThreadStage.Dyeing, 955m, 15m, "DYE-HT-02", "Rafiqul Islam", "Reactive dyeing shade matched delta-E < 0.3");
        b1.AdvanceStage(ThreadStage.Winding, 948m, 7m, "WND-CONE-01", "Shamim Reza", "Cone cross-winding with silicone lube emulsion");

        var b2 = ThreadBatch.Create("YRN-2026-102", "20/3 Ne", "100% Ring Spun Cotton", "IND-042", "Indigo Denim Navy", 750m, defaultCompanyId, adminUserId, "Heavy contrast topstitching thread");
        b2.AdvanceStage(ThreadStage.Spinning, 730m, 20m, "SPIN-02", "Jahangir Alam", "Doubling & twisting verified");
        b2.AdvanceStage(ThreadStage.Dyeing, 718m, 12m, "DYE-VAT-01", "Rafiqul Islam", "Sulphur indigo shade dipping");

        var b3 = ThreadBatch.Create("YRN-2026-103", "50/2 Ne", "100% Combed Cotton", "WHT-001", "Optical White", 500m, defaultCompanyId, adminUserId, "Shirt buttonhole & embroidery thread");
        b3.AdvanceStage(ThreadStage.Spinning, 485m, 15m, "SPIN-01", "Kabir Hossain", "Fine carded combing complete");

        await db.ThreadBatches.AddRangeAsync(b1, b2, b3);

        // 5. Seed Stock Lots for GRN-2026-0210 & Quarantine Gate (AC-04 / PRO-06)
        var lotPassed = new StockLot
        {
            LotBatchNumber = "LOT-TW-2026-01",
            ItemCode = "FAB-TW-01",
            ItemDescription = "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)",
            WarehouseLocationCode = "WH1-Aisle-03-Bay-02",
            Quantity = 36750,
            UnitOfMeasure = "Yds",
            StockState = StockLot.StateReleased,
            QCInspectionStatus = StockLot.QcStatusPassed,
            QCInspectorId = "Mahmud Hasan (QC Lead)",
            QCPassedAtUtc = DateTime.UtcNow.AddDays(-1),
            CompanyId = defaultCompanyId
        };

        var lotQuarantined = new StockLot
        {
            LotBatchNumber = "LOT-TW-2026-02",
            ItemCode = "FAB-TW-01",
            ItemDescription = "98/2 Cotton Spandex Twill 260 GSM (Desert Khaki)",
            WarehouseLocationCode = "WH1-Aisle-03-Bay-05",
            Quantity = 5000,
            UnitOfMeasure = "Yds",
            StockState = StockLot.StateQuarantine, // Active Quarantine Lock!
            QCInspectionStatus = StockLot.QcStatusPending,
            CompanyId = defaultCompanyId
        };

        await db.StockLots.AddRangeAsync(lotPassed, lotQuarantined);
        await db.SaveChangesAsync();

        // 6. Seed ASTM D5430-07 Fabric 4-Point Inspection (QC-4PT-2026-0045)
        var inspection = new FabricInspection
        {
            InspectionNumber = "QC-4PT-2026-0045",
            StockLotId = lotPassed.Id,
            InspectorName = "Mahmud Hasan (QC Lead)",
            InspectionDate = DateTime.UtcNow.AddDays(-1),
            TotalRollsInLot = 368,
            RollsInspected = 4,
            TotalYardsInspected = 375,
            CuttableWidthInches = 58.00m,
            TotalPenaltyPoints = 110, // Score: (110 * 3600) / (375 * 58) = 18.21 pts/100 sq.yd <= 28.0 => PASSED!
            AveragePointsPer100SqYds = 18.21m,
            OverallResult = "PASSED",
            Remarks = "ASTM D5430-07 PASSED: 18.21 pts/100 sq.yd. Zero major holes detected. Shade grouping A approved. Released to production.",
            CompanyId = defaultCompanyId
        };
        await db.FabricInspections.AddAsync(inspection);

        // 7. Seed Thread Dyeing Recipe & Cost Breakdown (THP-02)
        var recipe = new ThreadDyeingRecipe
        {
            RecipeCode = "RCP-POLY-NAVY-40",
            ColorName = "Deep Navy Blue",
            ColorCode = "TCX 19-4010",
            LiquorRatio = 10.0m,
            BatchWeightKg = 1000m,
            DyeStuffCostPerKg = 1.45m,
            ChemicalAuxiliaryCostPerKg = 0.55m,
            UtilityEnergyCostPerKg = 0.40m
        };
        recipe.AddIngredient("Disperse Navy Blue C-2G", "Disperse Dye", 2.40m, 14.50m);
        recipe.AddIngredient("Acetic Acid 98%", "pH Buffer (4.5 - 5.0)", 0.50m, 1.80m);
        recipe.AddIngredient("Leveling Agent LF-10", "Leveling & Penetration", 1.00m, 3.20m);
        recipe.AddIngredient("Dispersing Agent DA-100", "Dispersing & Suspension", 1.20m, 2.60m);
        recipe.AddIngredient("Silicon Softener SE-80", "Lubrication for sewing", 2.00m, 2.80m);
        await db.ThreadDyeingRecipes.AddAsync(recipe);

        // 8. Seed Thread Tensile & Quality Test Certificate (THP-03 / ASTM D2256)
        var tensileTest = new ThreadTensileTest
        {
            TestCertificateNumber = "CERT-TEN-2026-0089",
            ThreadBatchId = b1.Id,
            BatchNumber = b1.BatchNumber,
            YarnCount = "40/2 Ne Spun Polyester",
            TenacityCentiNewtonsPerTex = 39.4m, // >= 35.0 (Passed)
            ElongationPercentage = 16.2m, // 14% - 18% (Optimal)
            SingleYarnBreakingForceGrams = 410m,
            UnevennessCVPercentage = 9.8m,
            TestedBy = "Senior Textile Lab Technologist",
            PackagingBarcode = "BAR-THD-2026-9901"
        };
        await db.ThreadTensileTests.AddAsync(tensileTest);

        // 9. Seed 3D Warehouse Bin & Rack Coordinates (WHS-01)
        var loc1 = WarehouseLocation.Create("WH1", "A03", "R02", "T01", "B01", WarehouseZone.RawMaterialFabric, 12, defaultCompanyId);
        loc1.AssignRoll(6);
        var loc2 = WarehouseLocation.Create("WH1", "A03", "R02", "T01", "B02", WarehouseZone.RawMaterialFabric, 12, defaultCompanyId);
        loc2.AssignRoll(8);
        var loc3 = WarehouseLocation.Create("WH1", "A03", "R02", "T01", "B03", WarehouseZone.RawMaterialFabric, 12, defaultCompanyId);
        loc3.AssignRoll(10);
        var loc4 = WarehouseLocation.Create("WH1", "A03", "R02", "T01", "B04", WarehouseZone.RawMaterialFabric, 12, defaultCompanyId);
        loc4.AssignRoll(12); // Full!
        var locQuarantine = WarehouseLocation.Create("WH1", "A03", "R05", "T03", "B05", WarehouseZone.QuarantineStorage, 20, defaultCompanyId);
        locQuarantine.AssignRoll(5);
        var locRemnant = WarehouseLocation.Create("WH1", "A01", "R01", "T01", "REM", WarehouseZone.RemnantStorage, 30, defaultCompanyId);
        locRemnant.AssignRoll(3);

        await db.WarehouseLocations.AddRangeAsync(loc1, loc2, loc3, loc4, locQuarantine, locRemnant);

        // 10. Seed Fabric Rolls with Barcodes, Shade Groups & Coordinates (WHS-02)
        var twillRoll1 = FabricRoll.Create(
            "R-00891", itm2.Id, "DYE-TW-2026-01", 260m, 58m, 102.5m, loc1.LocationCode, defaultCompanyId,
            "SUP-APEX-01", null, FabricGrade.GradeA, "ROL-2026-00891", lotPassed.Id, "A", -2.5m, -1.2m, loc1.LocationCode);
        var twillRoll2 = FabricRoll.Create(
            "R-00892", itm2.Id, "DYE-TW-2026-01", 260m, 58m, 100.0m, loc2.LocationCode, defaultCompanyId,
            "SUP-APEX-02", null, FabricGrade.GradeA, "ROL-2026-00892", lotPassed.Id, "A", -2.4m, -1.2m, loc2.LocationCode);
        var twillRoll3 = FabricRoll.Create(
            "R-00893", itm2.Id, "DYE-TW-2026-01", 261m, 58m, 98.5m, loc3.LocationCode, defaultCompanyId,
            "SUP-APEX-03", null, FabricGrade.GradeA, "ROL-2026-00893", lotPassed.Id, "B", -2.6m, -1.3m, loc3.LocationCode);
        var twillRoll4 = FabricRoll.Create(
            "R-00894", itm2.Id, "DYE-TW-2026-01", 260m, 58m, 105.0m, loc4.LocationCode, defaultCompanyId,
            "SUP-APEX-04", null, FabricGrade.GradeA, "ROL-2026-00894", lotPassed.Id, "C", -2.8m, -1.5m, loc4.LocationCode);
        var rRemnant = FabricRoll.Create(
            "R-00895-REM", itm2.Id, "DYE-TW-2026-01", 260m, 58m, 15.0m, locRemnant.LocationCode, defaultCompanyId,
            "SUP-APEX-01", null, FabricGrade.GradeA, "ROL-2026-00895-REM", lotPassed.Id, "A", -2.5m, -1.2m, locRemnant.LocationCode);
        rRemnant.IsRemnant = true;

        await db.FabricRolls.AddRangeAsync(twillRoll1, twillRoll2, twillRoll3, twillRoll4, rRemnant);

        // 11. Seed Customs Bond Register & Utilization Permit Ledger (WHS-03)
        var bondCredit = CustomsBondLedger.RecordCredit(
            "BOND-REG-2026-001",
            "UP-2026-BOND-8891",
            "GRN",
            "GRN-2026-0210",
            "FAB-TW-01",
            "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)",
            36750m,
            0m,
            22050.00m,
            defaultCompanyId,
            "Raw material inward credit under Customs Bond Utilization Permit UP-2026-BOND-8891");

        var bondDebit = CustomsBondLedger.RecordDebit(
            "BOND-REG-2026-001",
            "UP-2026-BOND-8891",
            "STORE_ISSUE_NOTE",
            "SIN-2026-0045",
            "FAB-TW-01",
            "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)",
            406.0m,
            36750m,
            defaultCompanyId,
            "Cutting floor requisition issue debit for Export Order Style ST-2026-CHINO-01");

        await db.CustomsBondLedgers.AddRangeAsync(bondCredit, bondDebit);

        // 12. Seed Store Issue Note (SIN-2026-0045) & Remnant Handling (WHS-04)
        var sin = new StoreIssueNote
        {
            IssueNumber = "SIN-2026-0045",
            IssueDate = DateTime.UtcNow.AddHours(-4),
            TargetDepartment = "Cutting Department (Table 01)",
            StyleNumber = "ST-2026-CHINO-01",
            BuyerOrderNumber = "ORD-2026-001",
            RequestedBy = "Mahfuz Rahman (Cutting Master)",
            IssuedBy = "Enamul Hoque (Warehouse In-Charge)",
            Status = StoreIssueStatus.IssuedToFloor,
            Remarks = "Fabric rolls released under approved shade grouping A. Remnant returned to remnant bin.",
            CompanyId = defaultCompanyId
        };
        sin.AddItem(r1.Id, r1.RollBarcode, r1.ShadeGroup, 102.5m, 102.5m, 0m);
        sin.AddItem(r2.Id, r2.RollBarcode, r2.ShadeGroup, 100.0m, 100.0m, 0m);
        sin.AddItem(r3.Id, r3.RollBarcode, r3.ShadeGroup, 98.5m, 98.5m, 0m);
        sin.AddItem(r4.Id, r4.RollBarcode, r4.ShadeGroup, 120.0m, 105.0m, 15.0m, "ROL-2026-00895-REM", locRemnant.LocationCode);

        await db.StoreIssueNotes.AddAsync(sin);

        // 13. Seed Inventory Cycle Count & Variance Adjustment (WHS-05)
        var cycleCount = new InventoryCycleCount
        {
            CycleCountNumber = "CC-2026-M10",
            AuditDate = DateTime.UtcNow.AddDays(-2),
            AuditorName = "Mahmud Hasan (Internal Auditor)",
            LocationZone = "RawMaterialFabric",
            Remarks = "Monthly routine physical audit across Aisle 03 twill fabric racks. Variances reconciled within +/- 0.3% standard tolerance.",
            CompanyId = defaultCompanyId
        };
        cycleCount.AddLine("FAB-TW-01", "ROL-2026-00891", loc1.LocationCode, 102.5m, 102.5m, "RoutineAudit");
        cycleCount.AddLine("FAB-TW-01", "ROL-2026-00892", loc2.LocationCode, 100.0m, 99.8m, "EndBitWastage");
        cycleCount.AuthorizeAdjustment("S. K. Anwar (DGM Supply Chain)");

        await db.InventoryCycleCounts.AddAsync(cycleCount);

        await db.SaveChangesAsync();
    }
}
