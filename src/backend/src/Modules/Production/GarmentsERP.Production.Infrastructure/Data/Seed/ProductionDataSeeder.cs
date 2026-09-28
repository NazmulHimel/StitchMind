using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Production.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace GarmentsERP.Production.Infrastructure.Data.Seed;

public static class ProductionDataSeeder
{
    public static async Task SeedAsync(ProductionDbContext context)
    {
        try
        {
            var dbCreator = context.Database.GetService<IRelationalDatabaseCreator>();
            if (dbCreator.Exists())
            {
                try { dbCreator.CreateTables(); } catch { }
            }
            else
            {
                dbCreator.Create();
                dbCreator.CreateTables();
            }
        }
        catch { }

        if (await context.Buyers.AnyAsync()) 
        {
            if (!await context.CartonPacks.AnyAsync())
            {
                var order = await context.BuyerOrders.FirstOrDefaultAsync();
                if (order != null)
                {
                    var c1 = new CartonPack
                    {
                        CartonBarcode = "CTN-2026-001-M",
                        BuyerOrderId = order.Id,
                        StyleNumber = order.StyleNumber,
                        Color = "Navy Blue",
                        SizeBreakdownJson = "{\"M\":24}",
                        TotalQuantityPcs = 24,
                        GrossWeightKg = 6.2m,
                        NetWeightKg = 5.8m,
                        Status = CartonStatus.Packed,
                        AssignedPallet = "PLT-01"
                    };

                    var c2 = new CartonPack
                    {
                        CartonBarcode = "CTN-2026-002-L",
                        BuyerOrderId = order.Id,
                        StyleNumber = order.StyleNumber,
                        Color = "Navy Blue",
                        SizeBreakdownJson = "{\"L\":24}",
                        TotalQuantityPcs = 24,
                        GrossWeightKg = 6.4m,
                        NetWeightKg = 6.0m,
                        Status = CartonStatus.Packed,
                        AssignedPallet = "PLT-01"
                    };

                    await context.CartonPacks.AddRangeAsync(c1, c2);
                    await context.SaveChangesAsync();
                }
            }
            return;
        }

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
            Name = "H&M Hennes & Mauritz AB",
            Code = "HM-GLOBAL",
            Country = "Sweden",
            ContactPerson = "Astrid Lindgren",
            Email = "astrid.l@hm.com",
            Phone = "+46 8 796 5500",
            PaymentTerms = "TT 30 Days",
            IsActive = true
        };

        var levis = new Buyer
        {
            Name = "Levi Strauss & Co.",
            Code = "LEVIS-USA",
            Country = "United States",
            ContactPerson = "Michael Vance",
            Email = "mvance@levi.com",
            Phone = "+1 415 501 6000",
            PaymentTerms = "LC 90 Days",
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
            StyleName = "Slim Fit Pique Polo Shirt",
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

        // Master Style Baseline ST-2026-CHINO-01 (MOD-01)
        var orderChino = new BuyerOrder
        {
            OrderNumber = "BO-2026-0890",
            BuyerId = hm.Id,
            StyleNumber = "ST-2026-CHINO-01",
            StyleName = "Mens Slim Fit Chino Pant",
            ProductCategory = "Bottoms (Woven)",
            FabricComposition = "98% Cotton, 2% Spandex Twill, 260 GSM",
            Season = "Spring / Summer 2026",
            Currency = "USD",
            OrderQuantity = 24000,
            UnitPrice = 7.50m,
            OrderDate = DateTime.UtcNow.AddDays(-20),
            DeliveryDate = DateTime.UtcNow.AddDays(40),
            Status = OrderStatus.InProduction,
            Notes = "Export contract LC-2026-0098. 0.00% budget lock enforcement active."
        };

        // Matrix breakdown (12,000 Khaki Beige + 12,000 Navy Blue)
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Khaki Beige", PantoneCode = "16-1108 TCX", SizeLabel = "30/32", Quantity = 2400 });
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Khaki Beige", PantoneCode = "16-1108 TCX", SizeLabel = "32/32", Quantity = 4800 });
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Khaki Beige", PantoneCode = "16-1108 TCX", SizeLabel = "34/32", Quantity = 2400 });
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Khaki Beige", PantoneCode = "16-1108 TCX", SizeLabel = "36/32", Quantity = 2400 });
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Navy Blue", PantoneCode = "19-4024 TCX", SizeLabel = "30/32", Quantity = 2400 });
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Navy Blue", PantoneCode = "19-4024 TCX", SizeLabel = "32/32", Quantity = 4800 });
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Navy Blue", PantoneCode = "19-4024 TCX", SizeLabel = "34/32", Quantity = 2400 });
        orderChino.ColorSizes.Add(new StyleColorSize { ColorName = "Navy Blue", PantoneCode = "19-4024 TCX", SizeLabel = "36/32", Quantity = 2400 });

        // BOM Items
        orderChino.BOMItems.Add(new BOMItem
        {
            ItemCategory = "Fabric",
            ItemCode = "FAB-TW-01",
            ItemDescription = "98% Cotton 2% Spandex Twill 260 GSM (Navy/Khaki)",
            UnitOfMeasure = "Yds",
            NetConsumptionPerDzn = 18.5m,
            WastagePercentage = 5.0m,
            TotalRequiredQty = 38850.0m
        });
        orderChino.BOMItems.Add(new BOMItem
        {
            ItemCategory = "Thread",
            ItemCode = "THR-TC-01",
            ItemDescription = "T/C 40/2 Spun Polyester Cone 5000M",
            UnitOfMeasure = "Cones",
            NetConsumptionPerDzn = 2.5m,
            WastagePercentage = 3.0m,
            TotalRequiredQty = 5150.0m
        });
        orderChino.BOMItems.Add(new BOMItem
        {
            ItemCategory = "Trims",
            ItemCode = "ZIP-YKK-05",
            ItemDescription = "YKK #5 Brass Zipper 7 inch Closed-end",
            UnitOfMeasure = "Pcs",
            NetConsumptionPerDzn = 12.0m,
            WastagePercentage = 2.0m,
            TotalRequiredQty = 24480.0m
        });
        orderChino.BOMItems.Add(new BOMItem
        {
            ItemCategory = "Trims",
            ItemCode = "BTN-COROZO-20",
            ItemDescription = "20L 4-Hole Natural Corozo Button",
            UnitOfMeasure = "Pcs",
            NetConsumptionPerDzn = 24.0m,
            WastagePercentage = 3.0m,
            TotalRequiredQty = 49440.0m
        });

        // Pre-Costing budget cap sheet ($53.40 Materials / $18.00 CM / $6.00 OH / $90.00 FOB)
        var preCosting = new PreCosting
        {
            CostingVersion = 1,
            CostingBasis = "PerDozen",
            FOBPricePerDzn = 90.00m,
            TotalMaterialsBudget = 53.40m,
            TotalLaborCostPerDzn = 18.00m,
            TotalOverheadPerDzn = 6.00m,
            NetMarginPerDzn = 12.60m,
            IsBudgetApproved = true,
            ApprovedBy = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ApprovedAt = DateTime.UtcNow.AddDays(-19)
        };

        preCosting.Lines.Add(new CostingLine { Category = "Fabric", ItemDescription = "Cotton Twill Fabric (Max Cap $2.20/Yd)", ConsumptionPerDzn = 19.425m, UnitRate = 2.20m });
        preCosting.Lines.Add(new CostingLine { Category = "Thread", ItemDescription = "Sewing Thread Cones", ConsumptionPerDzn = 2.575m, UnitRate = 1.20m });
        preCosting.Lines.Add(new CostingLine { Category = "Trims", ItemDescription = "YKK Zippers", ConsumptionPerDzn = 12.24m, UnitRate = 0.35m });
        preCosting.Lines.Add(new CostingLine { Category = "Trims", ItemDescription = "Corozo Buttons", ConsumptionPerDzn = 24.72m, UnitRate = 0.05m });
        preCosting.Lines.Add(new CostingLine { Category = "Labor", ItemDescription = "Cost of Making (CM) Sewing & Finishing", ConsumptionPerDzn = 1.0m, UnitRate = 18.00m });
        preCosting.Lines.Add(new CostingLine { Category = "Overhead", ItemDescription = "Factory Utilities & Depreciation", ConsumptionPerDzn = 1.0m, UnitRate = 6.00m });

        orderChino.PreCosting = preCosting;

        await context.BuyerOrders.AddRangeAsync(order1, order2, order3, orderChino);
        await context.SaveChangesAsync();

        // 3. Seed CAD Markers (CUT-01)
        var markerChino = new CADMarker
        {
            MarkerName = "MKR-CHINO-SZ32-34-A",
            StyleNumber = "ST-2026-CHINO-01",
            CADSoftware = "Gerber AccuMark v16",
            CuttableWidthInches = 58.0m,
            MarkerLengthYards = 8.42m,
            GarmentsPerMarker = 4,
            SizeDistributionRatio = "32/32: 2 pcs, 34/32: 2 pcs",
            MarkerEfficiencyPct = 87.20m,
            PlannedConsumptionPerPcYds = 2.1050m,
            FileFormat = "ASTM/AAMA DXF"
        };
        var markerPolo = new CADMarker
        {
            MarkerName = "MKR-POLO-S-XL",
            StyleNumber = "ST-882",
            CADSoftware = "Lectra Diamino Expert",
            CuttableWidthInches = 62.0m,
            MarkerLengthYards = 6.85m,
            GarmentsPerMarker = 4,
            SizeDistributionRatio = "S: 1, M: 1, L: 1, XL: 1",
            MarkerEfficiencyPct = 88.60m,
            PlannedConsumptionPerPcYds = 1.7125m,
            FileFormat = "HPGL Plotter"
        };
        await context.CADMarkers.AddRangeAsync(markerChino, markerPolo);

        // 3b. Seed Relaxation Sessions (CUT-02)
        var roll1Guid = Guid.Parse("33333333-3333-3333-3333-333333333301");
        var roll2Guid = Guid.Parse("33333333-3333-3333-3333-333333333302");
        var roll3Guid = Guid.Parse("33333333-3333-3333-3333-333333333303");
        var roll4Guid = Guid.Parse("33333333-3333-3333-3333-333333333304");

        var relax1 = RelaxationSession.StartSession(roll1Guid, "ROL-2026-00891", "LOT-TW-2026-01", "98/2 Cotton Spandex Twill (260 GSM)", 24, 24.5m, 65.0m, "Cutter-Master-01", DateTime.UtcNow.AddHours(-26));
        var relax2 = RelaxationSession.StartSession(roll2Guid, "ROL-2026-00892", "LOT-TW-2026-01", "98/2 Cotton Spandex Twill (260 GSM)", 24, 24.5m, 65.0m, "Cutter-Master-01", DateTime.UtcNow.AddHours(-25.5));
        var relax3 = RelaxationSession.StartSession(roll3Guid, "ROL-2026-00893", "LOT-TW-2026-01", "98/2 Cotton Spandex Twill (260 GSM)", 24, 24.2m, 66.0m, "Cutter-Master-01", DateTime.UtcNow.AddHours(-24.5));
        var relax4 = RelaxationSession.StartSession(roll4Guid, "ROL-2026-00894", "LOT-TW-2026-01", "98/2 Cotton Spandex Twill (260 GSM)", 24, 24.8m, 64.0m, "Cutter-Master-02", DateTime.UtcNow.AddHours(-4.0)); // LOCKED! 20 hrs remaining

        await context.RelaxationSessions.AddRangeAsync(relax1, relax2, relax3, relax4);

        // 3c. Seed Cutting Orders & Bundles (CUT-03 & CUT-04)
        var cutChino = new CuttingOrder
        {
            CuttingOrderNumber = "CUT-2026-0001",
            BuyerOrderId = orderChino.Id,
            StyleNumber = "ST-2026-CHINO-01",
            FabricType = "98/2 Cotton Spandex Twill (260 GSM)",
            TotalRequiredQty = 240,
            TotalPlies = 60,
            MarkerLength = 8.42m,
            FabricWidth = 58.0m,
            EfficiencyPercentage = 87.20m,
            Status = CuttingStatus.CutCompleted,
            TableNumber = "Table-03",
            CuttingDate = DateTime.UtcNow.AddHours(-18),
            MarkerId = markerChino.Id,
            MarkerName = markerChino.MarkerName,
            ShadeGroup = "Shade A",
            GarmentsPerMarker = 4,
            TotalGarmentsPlanned = 240,
            ActualFabricConsumedYds = 508.5m,
            EndLossRemnantYds = 3.5m,
            CuttingWastageVariancePct = 0.68m,
            AllocatedRollBarcodes = "ROL-2026-00891, ROL-2026-00892, ROL-2026-00893",
            ApprovedBy = "Tariqul Islam (Cutting In-charge)"
        };

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
            CuttingDate = DateTime.UtcNow.AddDays(-2),
            ShadeGroup = "Shade A",
            TotalGarmentsPlanned = 2500,
            ApprovedBy = "Tariqul Islam"
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
            CuttingDate = DateTime.UtcNow.AddDays(-1),
            ShadeGroup = "Shade B",
            TotalGarmentsPlanned = 5000,
            ApprovedBy = "Tariqul Islam"
        };

        await context.CuttingOrders.AddRangeAsync(cutChino, cut1, cut2);
        await context.SaveChangesAsync();

        // 10 Serialized Component Bundles for CUT-2026-0001 (CUT-04)
        var chinoBundles = new List<CutBundle>
        {
            new() { BundleNumber = "BND-2026-0001-01", Barcode = "BND-2026-0001-01", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "32/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "FRONT PANEL", Quantity = 24, SerialStart = 1, SerialEnd = 24, BundleSequence = 1, TotalBundlesInOrder = 10, CurrentFloorStation = "SEWING_INPUT", CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "BND-2026-0001-02", Barcode = "BND-2026-0001-02", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "32/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "BACK PANEL", Quantity = 24, SerialStart = 1, SerialEnd = 24, BundleSequence = 2, TotalBundlesInOrder = 10, CurrentFloorStation = "SEWING_INPUT", CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "BND-2026-0001-03", Barcode = "BND-2026-0001-03", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "32/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "WAISTBAND", Quantity = 24, SerialStart = 1, SerialEnd = 24, BundleSequence = 3, TotalBundlesInOrder = 10, CurrentFloorStation = "FUSING_MACHINE", CurrentSection = BundleSection.Cutting },
            new() { BundleNumber = "BND-2026-0001-04", Barcode = "BND-2026-0001-04", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "32/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "FLY & POCKET", Quantity = 24, SerialStart = 1, SerialEnd = 24, BundleSequence = 4, TotalBundlesInOrder = 10, CurrentFloorStation = "CUTTING_OUTPUT", CurrentSection = BundleSection.Cutting },
            new() { BundleNumber = "BND-2026-0001-05", Barcode = "BND-2026-0001-05", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "34/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "FRONT PANEL", Quantity = 24, SerialStart = 25, SerialEnd = 48, BundleSequence = 5, TotalBundlesInOrder = 10, CurrentFloorStation = "SEWING_INPUT", CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "BND-2026-0001-06", Barcode = "BND-2026-0001-06", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "34/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "BACK PANEL", Quantity = 24, SerialStart = 25, SerialEnd = 48, BundleSequence = 6, TotalBundlesInOrder = 10, CurrentFloorStation = "SEWING_INPUT", CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "BND-2026-0001-07", Barcode = "BND-2026-0001-07", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "34/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "WAISTBAND", Quantity = 24, SerialStart = 25, SerialEnd = 48, BundleSequence = 7, TotalBundlesInOrder = 10, CurrentFloorStation = "FUSING_MACHINE", CurrentSection = BundleSection.Cutting },
            new() { BundleNumber = "BND-2026-0001-08", Barcode = "BND-2026-0001-08", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "34/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "FLY & POCKET", Quantity = 24, SerialStart = 25, SerialEnd = 48, BundleSequence = 8, TotalBundlesInOrder = 10, CurrentFloorStation = "CUTTING_OUTPUT", CurrentSection = BundleSection.Cutting },
            new() { BundleNumber = "BND-2026-0001-09", Barcode = "BND-2026-0001-09", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "32/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "FRONT PANEL", Quantity = 24, SerialStart = 49, SerialEnd = 72, BundleSequence = 9, TotalBundlesInOrder = 10, CurrentFloorStation = "CUTTING_OUTPUT", CurrentSection = BundleSection.Cutting },
            new() { BundleNumber = "BND-2026-0001-10", Barcode = "BND-2026-0001-10", CuttingOrderId = cutChino.Id, StyleNumber = "ST-2026-CHINO-01", Size = "34/32", Color = "Khaki Beige", ShadeGroup = "A", ComponentName = "BACK PANEL", Quantity = 24, SerialStart = 73, SerialEnd = 96, BundleSequence = 10, TotalBundlesInOrder = 10, CurrentFloorStation = "CUTTING_OUTPUT", CurrentSection = BundleSection.Cutting }
        };

        // Bundles for cut1
        var bundles = new List<CutBundle>
        {
            new() { BundleNumber = "CO-8801-01", Barcode = "BND-CO-8801-001-S", CuttingOrderId = cut1.Id, Size = "S", Color = "Navy Blue", Quantity = 50, SerialStart = 1, SerialEnd = 50, CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "CO-8801-02", Barcode = "BND-CO-8801-002-M", CuttingOrderId = cut1.Id, Size = "M", Color = "Navy Blue", Quantity = 50, SerialStart = 51, SerialEnd = 100, CurrentSection = BundleSection.Sewing },
            new() { BundleNumber = "CO-8801-03", Barcode = "BND-CO-8801-003-L", CuttingOrderId = cut1.Id, Size = "L", Color = "Navy Blue", Quantity = 50, SerialStart = 101, SerialEnd = 150, CurrentSection = BundleSection.Cutting },
            new() { BundleNumber = "CO-8801-04", Barcode = "BND-CO-8801-004-XL", CuttingOrderId = cut1.Id, Size = "XL", Color = "Navy Blue", Quantity = 50, SerialStart = 151, SerialEnd = 200, CurrentSection = BundleSection.Cutting }
        };
        await context.CutBundles.AddRangeAsync(chinoBundles);
        await context.CutBundles.AddRangeAsync(bundles);

        // 3d. Seed Fusing Machine Parameters Log (CUT-05)
        var fusingLog = new FusingMachineLog
        {
            CutOrderNumber = "CUT-2026-0001",
            MachineId = "FUS-02",
            ComponentName = "Waistband",
            InterliningType = "Fusible Microdot Woven 70 GSM",
            TemperatureC = 150.0m,
            PressureBar = 3.5m,
            SpeedSeconds = 12.0m,
            OperatorName = "Mizanur Rahman",
            IsApproved = true,
            Remarks = "Adhesion peel test passed (1.45 kg/inch). No surface bubbling.",
            LoggedAtUtc = DateTime.UtcNow.AddHours(-15)
        };
        await context.FusingMachineLogs.AddAsync(fusingLog);

        // 3e. Seed Subcontract Cutting Delivery & Reconciliation (CUT-06)
        var subChallan = new SubcontractCuttingChallan
        {
            ChallanNumber = "SCC-2026-0012",
            SubcontractorName = "Apex Precision Cutting Mills",
            StyleNumber = "ST-2026-CHINO-01",
            OrderNumber = "BO-2026-0890",
            FabricSentYds = 1200.0m,
            ExpectedCutPanels = 1440,
            ReceivedCutPanels = 1432,
            DefectivePanels = 4,
            ShortageCount = 4,
            ShortagePercentage = 0.28m,
            Status = SubcontractStatus.Reconciled,
            DispatchedAtUtc = DateTime.UtcNow.AddDays(-3),
            ReceivedAtUtc = DateTime.UtcNow.AddDays(-1),
            ReconciledBy = "Tariqul Islam",
            Remarks = "Shortage within 0.5% standard subcontract tolerance. Reconciled."
        };
        await context.SubcontractCuttingChallans.AddAsync(subChallan);
        await context.SaveChangesAsync();

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

        // 7. Seed Cartons
        var c1_initial = new CartonPack
        {
            CartonBarcode = "CTN-2026-001-M",
            BuyerOrderId = order1.Id,
            StyleNumber = order1.StyleNumber,
            Color = "Navy Blue",
            SizeBreakdownJson = "{\"M\":24}",
            TotalQuantityPcs = 24,
            GrossWeightKg = 6.2m,
            NetWeightKg = 5.8m,
            Status = CartonStatus.Packed,
            AssignedPallet = "PLT-01"
        };

        var c2_initial = new CartonPack
        {
            CartonBarcode = "CTN-2026-002-L",
            BuyerOrderId = order1.Id,
            StyleNumber = order1.StyleNumber,
            Color = "Navy Blue",
            SizeBreakdownJson = "{\"L\":24}",
            TotalQuantityPcs = 24,
            GrossWeightKg = 6.4m,
            NetWeightKg = 6.0m,
            Status = CartonStatus.Packed,
            AssignedPallet = "PLT-01"
        };

        await context.CartonPacks.AddRangeAsync(c1_initial, c2_initial);
        await context.SaveChangesAsync();
    }
}
