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
        await db.SaveChangesAsync();
    }
}
