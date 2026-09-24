using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Production.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Production.Infrastructure.Data.Seed;

public static class ProductionDataSeeder
{
    public static async Task SeedAsync(ProductionDbContext context)
    {
        if (await context.Buyers.AnyAsync()) return;

        // 1. Seed Buyers
        var zara = new Buyer
        {
            Name = "Inditex / Zara International",
            Code = "ZARA",
            Country = "Spain",
            ContactPerson = "Carlos Martinez",
            Email = "carlos.m@inditex.com",
            Phone = "+34 981 185 400",
            PaymentTerms = "LC 60 Days",
            IsActive = true
        };

        var hm = new Buyer
        {
            Name = "H&M Hennes & Mauritz",
            Code = "HM",
            Country = "Sweden",
            ContactPerson = "Astrid Lindgren",
            Email = "astrid.l@hm.com",
            Phone = "+46 8 796 55 00",
            PaymentTerms = "LC 90 Days",
            IsActive = true
        };

        var levis = new Buyer
        {
            Name = "Levi Strauss & Co.",
            Code = "LEVI",
            Country = "United States",
            ContactPerson = "Jonathan Vance",
            Email = "jvance@levi.com",
            Phone = "+1 415 501 6000",
            PaymentTerms = "TT in Advance",
            IsActive = true
        };

        await context.Buyers.AddRangeAsync(zara, hm, levis);
        await context.SaveChangesAsync();

        // 2. Seed Buyer Orders
        var order1 = new BuyerOrder
        {
            OrderNumber = "BO-2609-1001",
            BuyerId = zara.Id,
            StyleNumber = "ST-882",
            StyleName = "Men's Pique Polo Shirt",
            Season = "SS-2026",
            OrderQuantity = 5000,
            UnitPrice = 7.50m,
            OrderDate = DateTime.UtcNow.AddDays(-14),
            DeliveryDate = DateTime.UtcNow.AddDays(30),
            Status = OrderStatus.InProduction,
            Notes = "Custom collar tipping and contrast placket."
        };

        var order2 = new BuyerOrder
        {
            OrderNumber = "BO-2609-1002",
            BuyerId = hm.Id,
            StyleNumber = "ST-904",
            StyleName = "Organic Basic Crew Tee",
            Season = "SS-2026",
            OrderQuantity = 10000,
            UnitPrice = 4.20m,
            OrderDate = DateTime.UtcNow.AddDays(-10),
            DeliveryDate = DateTime.UtcNow.AddDays(25),
            Status = OrderStatus.InProduction,
            Notes = "GOTS organic certified cotton required."
        };

        var order3 = new BuyerOrder
        {
            OrderNumber = "BO-2609-1003",
            BuyerId = levis.Id,
            StyleNumber = "HD-112",
            StyleName = "Heavyweight Brushed Fleece Hoodie",
            Season = "AW-2026",
            OrderQuantity = 3500,
            UnitPrice = 12.80m,
            OrderDate = DateTime.UtcNow.AddDays(-5),
            DeliveryDate = DateTime.UtcNow.AddDays(45),
            Status = OrderStatus.Confirmed,
            Notes = "Kangaroo pocket with tonal drawcords."
        };

        await context.BuyerOrders.AddRangeAsync(order1, order2, order3);
        await context.SaveChangesAsync();

        // 3. Seed Cutting Orders & Bundles
        var cut1 = new CuttingOrder
        {
            CuttingOrderNumber = "CO-8801",
            BuyerOrderId = order1.Id,
            StyleNumber = "ST-882",
            FabricType = "100% Pique Cotton 220 GSM",
            TotalRequiredQty = 2500,
            TotalPlies = 80,
            MarkerLength = 6.85m,
            FabricWidth = 62.0m,
            EfficiencyPercentage = 88.6m,
            Status = CuttingStatus.CutCompleted,
            TableNumber = "Table-01",
            CuttingDate = DateTime.UtcNow.AddDays(-2)
        };

        var cut2 = new CuttingOrder
        {
            CuttingOrderNumber = "CO-8802",
            BuyerOrderId = order2.Id,
            StyleNumber = "ST-904",
            FabricType = "Single Jersey 160 GSM",
            TotalRequiredQty = 5000,
            TotalPlies = 120,
            MarkerLength = 5.40m,
            FabricWidth = 64.0m,
            EfficiencyPercentage = 91.2m,
            Status = CuttingStatus.InProgress,
            TableNumber = "Table-02",
            CuttingDate = DateTime.UtcNow.AddDays(-1)
        };

        await context.CuttingOrders.AddRangeAsync(cut1, cut2);
        await context.SaveChangesAsync();

        // Bundles for cut1
        var bundles = new List<CutBundle>
        {
            new() { BundleNumber = "CO-8801-01", CuttingOrderId = cut1.Id, Size = "S", Color = "Navy Blue", Quantity = 50, Barcode = "BND-CO-8801-001-S", SerialStart = 1, SerialEnd = 50, CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "CO-8801-02", CuttingOrderId = cut1.Id, Size = "M", Color = "Navy Blue", Quantity = 50, Barcode = "BND-CO-8801-002-M", SerialStart = 51, SerialEnd = 100, CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "CO-8801-03", CuttingOrderId = cut1.Id, Size = "L", Color = "Navy Blue", Quantity = 50, Barcode = "BND-CO-8801-003-L", SerialStart = 101, SerialEnd = 150, CurrentSection = BundleSection.Cutting },
            new() { BundleNumber = "CO-8801-04", CuttingOrderId = cut1.Id, Size = "XL", Color = "Navy Blue", Quantity = 50, Barcode = "BND-CO-8801-004-XL", SerialStart = 151, SerialEnd = 200, CurrentSection = BundleSection.Cutting }
        };
        await context.CutBundles.AddRangeAsync(bundles);

        // 4. Seed Sewing Lines
        var line1 = new SewingLine { LineNumber = "Line-01", LineName = "Polo Shirt Line", Floor = "Floor 2", TargetEfficiency = 88.0m, CapacityPerDay = 1400, Supervisor = "Rafiqul Islam", Status = SewingLineStatus.Active };
        var line2 = new SewingLine { LineNumber = "Line-02", LineName = "Crew Tee High-Speed", Floor = "Floor 2", TargetEfficiency = 92.0m, CapacityPerDay = 1800, Supervisor = "Kamrul Hassan", Status = SewingLineStatus.Active };
        var line3 = new SewingLine { LineNumber = "Line-03", LineName = "Hoodie & Heavywear", Floor = "Floor 3", TargetEfficiency = 82.0m, CapacityPerDay = 900, Supervisor = "Shahidul Alam", Status = SewingLineStatus.Active };
        var line4 = new SewingLine { LineNumber = "Line-04", LineName = "Woven Pant Line", Floor = "Floor 3", TargetEfficiency = 85.0m, CapacityPerDay = 1100, Supervisor = "Anisur Rahman", Status = SewingLineStatus.Active };
        var line5 = new SewingLine { LineNumber = "Line-05", LineName = "Ladies Fashion Line", Floor = "Floor 4", TargetEfficiency = 86.0m, CapacityPerDay = 1500, Supervisor = "Nasreen Akhter", Status = SewingLineStatus.Active };

        await context.SewingLines.AddRangeAsync(line1, line2, line3, line4, line5);
        await context.SaveChangesAsync();

        // 5. Seed Hourly Production Logs for Today
        var today = DateTime.UtcNow.Date;
        var logs = new List<HourlyProductionLog>
        {
            new() { SewingLineId = line1.Id, BuyerOrderId = order1.Id, LogDate = today, HourSlot = 1, TargetQty = 160, ActualOutputQty = 165, DefectCount = 2, RejectionCount = 0, OperatorCount = 38, Notes = "Smooth startup" },
            new() { SewingLineId = line1.Id, BuyerOrderId = order1.Id, LogDate = today, HourSlot = 2, TargetQty = 160, ActualOutputQty = 170, DefectCount = 1, RejectionCount = 0, OperatorCount = 38, Notes = "Good pace" },
            new() { SewingLineId = line1.Id, BuyerOrderId = order1.Id, LogDate = today, HourSlot = 3, TargetQty = 160, ActualOutputQty = 158, DefectCount = 3, RejectionCount = 1, OperatorCount = 38, Notes = "Needle broken at station 12" },
            new() { SewingLineId = line1.Id, BuyerOrderId = order1.Id, LogDate = today, HourSlot = 4, TargetQty = 160, ActualOutputQty = 168, DefectCount = 1, RejectionCount = 0, OperatorCount = 38 },
            new() { SewingLineId = line2.Id, BuyerOrderId = order2.Id, LogDate = today, HourSlot = 1, TargetQty = 200, ActualOutputQty = 205, DefectCount = 2, RejectionCount = 0, OperatorCount = 42 },
            new() { SewingLineId = line2.Id, BuyerOrderId = order2.Id, LogDate = today, HourSlot = 2, TargetQty = 200, ActualOutputQty = 198, DefectCount = 4, RejectionCount = 1, OperatorCount = 42 }
        };
        await context.HourlyProductionLogs.AddRangeAsync(logs);

        // 6. Seed Quality Inspections
        var qcs = new List<QualityInspection>
        {
            new() { InspectionNumber = "QC-IN-401", InspectionType = InspectionType.FabricFourPoint, BuyerOrderId = order1.Id, LotOrBatchRef = "LOT-901 Cotton Spandex", SampleSize = 3200, MinorDefects = 8, MajorDefects = 2, CriticalDefects = 0, TotalPoints = 14.5m, Result = InspectionResult.Passed, InspectorName = "M. Tariqul", Comments = "Passed 4-point ASTM D5430 (Points < 28/100 sq yds)", InspectedAt = DateTime.UtcNow.AddDays(-2) },
            new() { InspectionNumber = "QC-IN-402", InspectionType = InspectionType.SewingInline, BuyerOrderId = order1.Id, LotOrBatchRef = "Line 01 - ST-882", SampleSize = 240, MinorDefects = 4, MajorDefects = 1, CriticalDefects = 0, TotalPoints = 5.0m, Result = InspectionResult.Passed, InspectorName = "S. Mahmud", Comments = "Minor skip stitch repaired on line", InspectedAt = DateTime.UtcNow.AddDays(-1) },
            new() { InspectionNumber = "QC-IN-403", InspectionType = InspectionType.FinalAQL, BuyerOrderId = order2.Id, LotOrBatchRef = "PO-8812 Basic Crew", SampleSize = 500, MinorDefects = 5, MajorDefects = 2, CriticalDefects = 0, TotalPoints = 7.0m, Result = InspectionResult.Passed, InspectorName = "R. Chowdhury", Comments = "AQL 2.5 Major / 4.0 Minor compliant", InspectedAt = DateTime.UtcNow }
        };
        await context.QualityInspections.AddRangeAsync(qcs);

        await context.SaveChangesAsync();
    }
}
