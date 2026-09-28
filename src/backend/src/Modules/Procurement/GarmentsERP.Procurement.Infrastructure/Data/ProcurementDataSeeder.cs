using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Procurement.Infrastructure.Data;

public static class ProcurementDataSeeder
{
    public static async Task SeedAsync(ProcurementDbContext db)
    {
        if (await db.Suppliers.AnyAsync()) return;

        var defaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // 1. Seed Suppliers
        var s1 = Supplier.Create("SUP-DEN-001", "Pioneer Denim Mills Ltd.", "orders@pioneerdenim.com", "+880 2 988 1234", "Habiganj Industrial Park, Sylhet, Bangladesh", MaterialCategory.Fabric, defaultCompanyId, 60, "TR-BD-782190");
        var s2 = Supplier.Create("SUP-FAB-002", "Beximco Textiles Ltd.", "sales@beximcotextiles.com", "+880 2 586 1189", "Kashimpur, Gazipur, Dhaka, Bangladesh", MaterialCategory.Fabric, defaultCompanyId, 45, "TR-BD-442109");
        var s3 = Supplier.Create("SUP-THD-003", "Coats Bangladesh Ltd.", "customercare@coats.com", "+880 2 890 1200", "Plot 23, Sector 1, DEPZ, Savar, Dhaka", MaterialCategory.Thread, defaultCompanyId, 30, "TR-BD-902143");
        var s4 = Supplier.Create("SUP-TRM-004", "YKK Bangladesh Pte. Ltd.", "orders@ykk.com.bd", "+880 2 770 1900", "Dhaka Export Processing Zone, Savar, Dhaka", MaterialCategory.Trims, defaultCompanyId, 30, "TR-BD-112345");
        var s5 = Supplier.Create("SUP-PKG-005", "Avery Dennison Bangladesh", "service@averydennison.com", "+880 2 887 0400", "Plot 7-10, Sector 2, CEPZ, Chattogram", MaterialCategory.Packaging, defaultCompanyId, 30, "TR-BD-667123");
        var s6 = Supplier.Create("SUP-YRN-006", "Square Yarns Ltd.", "yarnsales@squaregroup.com", "+880 2 883 3047", "Square Centre, 48 Mohakhali C/A, Dhaka", MaterialCategory.Thread, defaultCompanyId, 45, "TR-BD-334512");

        s1.SetRating(4.9m);
        s2.SetRating(4.8m);
        s3.SetRating(5.0m);
        s4.SetRating(4.9m);
        s5.SetRating(4.7m);
        s6.SetRating(4.8m);

        await db.Suppliers.AddRangeAsync(s1, s2, s3, s4, s5, s6);
        await db.SaveChangesAsync();

        // 2. Seed Purchase Orders
        var adminUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

        // PO 1: Pioneer Denim (Completed)
        var po1 = PurchaseOrder.Create("PO-202609-1001", s1.Id, defaultCompanyId, DateTime.UtcNow.AddDays(-5), "Bulk denim procurement for Levi's 501 Fall line");
        po1.AddItem("FAB-DNM-001", "12oz 100% Cotton Indigo Denim", MaterialCategory.Fabric, UOM.Yards, 10000, 3.80m);
        po1.AddItem("FAB-DNM-002", "10.5oz Stretch Blue Denim", MaterialCategory.Fabric, UOM.Yards, 5000, 3.50m);
        po1.Submit();
        po1.Approve(adminUserId);

        // PO 2: Coats Bangladesh (Approved / Partially Received)
        var po2 = PurchaseOrder.Create("PO-202609-1002", s3.Id, defaultCompanyId, DateTime.UtcNow.AddDays(7), "Sewing thread for export shipment");
        po2.AddItem("THD-EPIC-BLK", "Coats Epic 40/2 Spun Poly Thread (Black)", MaterialCategory.Thread, UOM.Cones, 500, 2.20m);
        po2.AddItem("THD-EPIC-WHT", "Coats Epic 40/2 Spun Poly Thread (White)", MaterialCategory.Thread, UOM.Cones, 500, 2.20m);
        po2.AddItem("THD-EPIC-203", "Coats Dual Duty 20/3 Heavy Topstitch", MaterialCategory.Thread, UOM.Cones, 300, 3.10m);
        po2.Submit();
        po2.Approve(adminUserId);

        // PO 3: YKK Bangladesh (Submitted)
        var po3 = PurchaseOrder.Create("PO-202609-1003", s4.Id, defaultCompanyId, DateTime.UtcNow.AddDays(14), "Metal zippers and buttons for Jeans production");
        po3.AddItem("TRM-ZIP-001", "5 Inch Antique Brass Zipper (Closed-end)", MaterialCategory.Trims, UOM.Pieces, 10000, 0.45m);
        po3.AddItem("TRM-BTN-002", "17mm Donut Tack Button (Gunmetal)", MaterialCategory.Trims, UOM.Pieces, 20000, 0.08m);
        po3.Submit();

        // PO 4: Avery Dennison (Draft)
        var po4 = PurchaseOrder.Create("PO-202609-1004", s5.Id, defaultCompanyId, DateTime.UtcNow.AddDays(20), "Hangtags, barcodes, polybags for Style #GL-992");
        po4.AddItem("PKG-TAG-001", "Recycled Craft Paper Hangtag with Cotton String", MaterialCategory.Packaging, UOM.Pieces, 25000, 0.04m);
        po4.AddItem("PKG-LBL-002", "Main Brand Damask Woven Label 40x25mm", MaterialCategory.Packaging, UOM.Pieces, 25000, 0.06m);

        await db.PurchaseOrders.AddRangeAsync(po1, po2, po3, po4);
        await db.SaveChangesAsync();

        // 3. Seed Goods Receipt Note for PO 1
        var grn1 = GoodsReceiptNote.Create("GRN-202609-1001", po1.Id, s1.Id, defaultCompanyId, "CH-PD-99812", "DM-HA-11-2309", "Delivered on 14-wheeler truck in good condition");
        var item1 = po1.Items.First();
        var item2 = po1.Items.Last();

        grn1.AddItem(item1.Id, item1.ItemCode, item1.Description, 10000, 9950, 50, "50 yds edge fraying on Roll #402");
        grn1.AddItem(item2.Id, item2.ItemCode, item2.Description, 5000, 5000, 0);
        grn1.MarkUnderInspection(adminUserId);
        grn1.CompleteInspection();

        item1.RecordReceived(9950);
        item2.RecordReceived(5000);
        po1.UpdateReceivingStatus();

        await db.GoodsReceiptNotes.AddAsync(grn1);

        // 4. Seed Phase 2 Production Demo Data (Style ST-2026-CHINO-01)
        var sApex = Supplier.Create("SUP-TEX-004", "Apex Textile Mills Ltd.", "commercial@apextextiles.com", "+880 2 841 8890", "Plot 12-14, DEPZ, Savar, Dhaka, Bangladesh", MaterialCategory.Fabric, defaultCompanyId, 90, "TR-BD-889102");
        sApex.SetRating(4.85m);
        var sSquare = Supplier.Create("SUP-FAB-007", "Square Fashions Ltd.", "marketing@squarefashions.com", "+880 2 883 3048", "Valuka, Mymensingh, Bangladesh", MaterialCategory.Fabric, defaultCompanyId, 60, "TR-BD-771234");
        sSquare.SetRating(4.90m);
        var sEnvoy = Supplier.Create("SUP-FAB-008", "Envoy Textiles Ltd.", "sales@envoytextiles.com", "+880 2 988 5678", "Bhaluka, Mymensingh, Bangladesh", MaterialCategory.Fabric, defaultCompanyId, 30, "TR-BD-551982");
        sEnvoy.SetRating(4.70m);

        await db.Suppliers.AddRangeAsync(sApex, sSquare, sEnvoy);
        await db.SaveChangesAsync();

        // 4.1 Material Requisition MR-2026-0089 (PRO-01)
        var mr = new MaterialRequisition
        {
            RequisitionNumber = "MR-2026-0089",
            StyleNumber = "ST-2026-CHINO-01",
            BuyerName = "H&M Global Sourcing (Stockholm)",
            Department = "Merchandising",
            RequisitionDate = DateTime.UtcNow.AddDays(-15),
            RequiredDate = DateTime.UtcNow.AddDays(25),
            Status = RequisitionStatus.Approved,
            Remarks = "Fabric and trims requisition for 24,000 pcs Slim-Fit Chino trouser",
            CompanyId = defaultCompanyId
        };
        mr.Items.Add(new MaterialRequisitionItem
        {
            MaterialRequisitionId = mr.Id,
            ItemCode = "FAB-TW-01",
            ItemDescription = "98/2 Cotton Spandex Twill 260 GSM (Navy, Olive, Khaki)",
            Category = MaterialCategory.Fabric,
            UnitOfMeasure = UOM.Yards,
            RequiredQuantity = 36750,
            EstimatedUnitPrice = 2.40m
        });
        mr.Items.Add(new MaterialRequisitionItem
        {
            MaterialRequisitionId = mr.Id,
            ItemCode = "PKT-TC-110",
            ItemDescription = "65/35 TC Pocketing Fabric 110 GSM",
            Category = MaterialCategory.Fabric,
            UnitOfMeasure = UOM.Yards,
            RequiredQuantity = 5200,
            EstimatedUnitPrice = 0.85m
        });
        await db.MaterialRequisitions.AddAsync(mr);

        // 4.2 Comparative Statement CS-2026-0042 (PRO-02 / AC-03)
        var cs = new ComparativeStatement
        {
            CSNumber = "CS-2026-0042",
            MaterialRequisitionId = mr.Id,
            ItemCode = "FAB-TW-01",
            ItemDescription = "98/2 Cotton Spandex Twill 260 GSM (Export Quality)",
            TotalQuantityRequired = 36750,
            PreCostingBudgetRate = 2.40m,
            CompanyId = defaultCompanyId
        };
        cs.AddSupplierBid(sApex.Id, sApex.Name, 2.3500m, 21, "90 Days Deferred B2B LC", 4.85m);
        cs.AddSupplierBid(sSquare.Id, sSquare.Name, 2.4500m, 14, "60 Days Deferred B2B LC", 4.90m);
        cs.AddSupplierBid(sEnvoy.Id, sEnvoy.Name, 2.3800m, 25, "At Sight Letter of Credit", 4.70m);
        cs.AwardContract(sApex.Id, "Awarded based on optimal pricing (-$0.05 budget savings) and favorable 90-day B2B LC terms.");
        await db.ComparativeStatements.AddAsync(cs);

        // 4.3 Purchase Order PO-2026-0145 (PRO-04 / Approved)
        var poChino = PurchaseOrder.Create("PO-2026-0145", sApex.Id, defaultCompanyId, DateTime.UtcNow.AddDays(14), "Fabric for Buyer Order ORD-2026-CHINO-001");
        poChino.AddItem("FAB-TW-01", "98/2 Cotton Spandex Twill 260 GSM", MaterialCategory.Fabric, UOM.Yards, 36750, 2.35m, 2.40m);
        poChino.Submit();
        poChino.Approve(adminUserId);

        // 4.4 Budget-Locked PO Demonstration (AC-03)
        var poLocked = PurchaseOrder.Create("PO-2026-0146", sSquare.Id, defaultCompanyId, DateTime.UtcNow.AddDays(10), "Surge replenishment attempt exceeding Pre-Costing cap");
        poLocked.AddItem("FAB-TW-01", "98/2 Cotton Spandex Twill 260 GSM", MaterialCategory.Fabric, UOM.Yards, 5000, 2.45m, 2.40m); // Exceeds $2.40!
        // poLocked will have IsBudgetLocked = true and Status = LockedBudgetExceeded automatically!

        await db.PurchaseOrders.AddRangeAsync(poChino, poLocked);

        // 4.5 Commercial Trade & Back-to-Back LC (PRO-05)
        var masterLc = new CommercialLC
        {
            LCNumber = "MLC-NORDIC-2026-4412",
            LCType = LCType.MasterLC,
            IssuingBank = "Swedbank Stockholm, Sweden",
            AdvisingBank = "Standard Chartered Bank, Dhaka",
            ApplicantName = "Nordic Fashion Retail AB",
            BeneficiaryName = "StitchMind Apparel Group Ltd.",
            LCAmount = 180000.00m,
            Currency = "USD",
            IssueDate = DateTime.UtcNow.AddDays(-30),
            ExpiryDate = DateTime.UtcNow.AddDays(90),
            ShipmentDeadline = DateTime.UtcNow.AddDays(60),
            Status = LCStatus.Opened,
            TenorTerms = "Sight LC",
            CompanyId = defaultCompanyId
        };
        await db.CommercialLCs.AddAsync(masterLc);
        await db.SaveChangesAsync();

        var b2bLc = new CommercialLC
        {
            LCNumber = "B2B-2026-0098",
            LCType = LCType.BackToBackLC,
            ParentMasterLCId = masterLc.Id,
            IssuingBank = "Standard Chartered Bank, Dhaka",
            AdvisingBank = "Eastern Bank PLC, Dhaka",
            ApplicantName = "StitchMind Apparel Group Ltd.",
            BeneficiaryName = "Apex Textile Mills Ltd.",
            LCAmount = 86362.50m, // 47.98% of $180,000 (compliant with 70% statutory limit)
            Currency = "USD",
            IssueDate = DateTime.UtcNow.AddDays(-20),
            ExpiryDate = DateTime.UtcNow.AddDays(70),
            ShipmentDeadline = DateTime.UtcNow.AddDays(40),
            Status = LCStatus.Opened,
            LinkedPurchaseOrderId = poChino.Id,
            TenorTerms = "90 Days Deferred Maturity B2B LC",
            CompanyId = defaultCompanyId
        };
        await db.CommercialLCs.AddAsync(b2bLc);

        // 4.6 Goods Receipt Note GRN-2026-0210 (PRO-06)
        var grnChino = GoodsReceiptNote.Create("GRN-2026-0210", poChino.Id, sApex.Id, defaultCompanyId, "CH-99412", "Dhaka Metro-U 11-4589", "Received 368 fabric rolls into WH1 Quarantine area");
        var poItem = poChino.Items.First();
        grnChino.AddItem(poItem.Id, poItem.ItemCode, poItem.Description, 36750, 36750, 0);
        grnChino.MarkUnderInspection(adminUserId);
        await db.GoodsReceiptNotes.AddAsync(grnChino);

        await db.SaveChangesAsync();
    }
}
