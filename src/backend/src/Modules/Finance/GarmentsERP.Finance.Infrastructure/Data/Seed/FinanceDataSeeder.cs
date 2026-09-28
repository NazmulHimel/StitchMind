using GarmentsERP.Finance.Domain.Entities;
using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Finance.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace GarmentsERP.Finance.Infrastructure.Data.Seed;

public static class FinanceDataSeeder
{
    public static async Task SeedAsync(FinanceDbContext context, ILogger logger)
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

        if (await context.Accounts.AnyAsync())
        {
            logger.LogInformation("Finance data already exists. Skipping seed.");
            return;
        }

        logger.LogInformation("Seeding standard Garments Industry Chart of Accounts and Initial Vouchers...");

        // Standard Chart of Accounts
        var accounts = new List<Account>
        {
            // Assets (1000 - 1999)
            new() { AccountCode = "1001", AccountName = "Cash in Hand", Category = AccountCategory.Asset, Currency = "BDT", OpeningBalance = 250000m, CurrentBalance = 250000m, Description = "Factory petty cash fund" },
            new() { AccountCode = "1002", AccountName = "Standard Chartered Bank (Principal A/C)", Category = AccountCategory.Asset, Currency = "BDT", OpeningBalance = 15400000m, CurrentBalance = 15400000m, Description = "Operating & export proceeds bank account" },
            new() { AccountCode = "1003", AccountName = "Accounts Receivable - Export Buyers", Category = AccountCategory.Asset, Currency = "BDT", OpeningBalance = 8250000m, CurrentBalance = 8250000m, Description = "Buyer LC & invoice receivables (H&M, Zara, Target)" },
            new() { AccountCode = "1004", AccountName = "Raw Material Inventory - Fabric & Yarn", Category = AccountCategory.Asset, Currency = "BDT", OpeningBalance = 6200000m, CurrentBalance = 6200000m, Description = "Single Jersey, Twill, Denim raw stock" },
            new() { AccountCode = "1005", AccountName = "Work-in-Progress (WIP) Inventory", Category = AccountCategory.Asset, Currency = "BDT", OpeningBalance = 3100000m, CurrentBalance = 3100000m, Description = "Cutting, sewing, finishing line WIP" },
            new() { AccountCode = "1006", AccountName = "Finished Goods Inventory", Category = AccountCategory.Asset, Currency = "BDT", OpeningBalance = 4500000m, CurrentBalance = 4500000m, Description = "Packed cartons awaiting export dispatch" },
            new() { AccountCode = "1501", AccountName = "Plant & Automatic Sewing Machinery", Category = AccountCategory.Asset, Currency = "BDT", OpeningBalance = 18000000m, CurrentBalance = 18000000m, Description = "Juki & Brother industrial machines" },

            // Liabilities (2000 - 2999)
            new() { AccountCode = "2001", AccountName = "Accounts Payable - Yarn & Dyeing Suppliers", Category = AccountCategory.Liability, Currency = "BDT", OpeningBalance = 5800000m, CurrentBalance = 5800000m, Description = "Supplier invoices (Square Textiles, Envoy)" },
            new() { AccountCode = "2002", AccountName = "Accrued Wages & Operator Salaries", Category = AccountCategory.Liability, Currency = "BDT", OpeningBalance = 3450000m, CurrentBalance = 3450000m, Description = "Monthly factory payroll liability" },
            new() { AccountCode = "2003", AccountName = "Factory Utilities Payable (Gas & Electricity)", Category = AccountCategory.Liability, Currency = "BDT", OpeningBalance = 850000m, CurrentBalance = 850000m, Description = "DESCO & Titas Gas monthly accruals" },

            // Equity (3000 - 3999)
            new() { AccountCode = "3001", AccountName = "Shareholder Paid-Up Capital", Category = AccountCategory.Equity, Currency = "BDT", OpeningBalance = 30000000m, CurrentBalance = 30000000m, Description = "Founding equity investment" },
            new() { AccountCode = "3002", AccountName = "Retained Earnings", Category = AccountCategory.Equity, Currency = "BDT", OpeningBalance = 15600000m, CurrentBalance = 15600000m, Description = "Prior years cumulative retained profits" },

            // Revenue (4000 - 4999)
            new() { AccountCode = "4001", AccountName = "Export Apparel Sales (FOB)", Category = AccountCategory.Revenue, Currency = "BDT", OpeningBalance = 0m, CurrentBalance = 12500000m, Description = "Direct export garment shipments" },
            new() { AccountCode = "4002", AccountName = "Local Fabric Waste & Scrap Sales", Category = AccountCategory.Revenue, Currency = "BDT", OpeningBalance = 0m, CurrentBalance = 320000m, Description = "Cutting wastage / jhut sales revenue" },

            // Expenses (5000 - 5999)
            new() { AccountCode = "5001", AccountName = "Direct Material Cost - Fabric & Trims", Category = AccountCategory.Expense, Currency = "BDT", OpeningBalance = 0m, CurrentBalance = 5800000m, Description = "COGS: Fabric, zippers, buttons, labels" },
            new() { AccountCode = "5002", AccountName = "Direct Labor - Cutting & Sewing Wages", Category = AccountCategory.Expense, Currency = "BDT", OpeningBalance = 0m, CurrentBalance = 3200000m, Description = "COGS: Factory floor operator & helper wages" },
            new() { AccountCode = "5003", AccountName = "Factory Overhead - Boiler Steam & Power", Category = AccountCategory.Expense, Currency = "BDT", OpeningBalance = 0m, CurrentBalance = 780000m, Description = "Electricity, steam generator, water treatment" },
            new() { AccountCode = "5004", AccountName = "Quality Assurance & Testing Laboratory", Category = AccountCategory.Expense, Currency = "BDT", OpeningBalance = 0m, CurrentBalance = 240000m, Description = "Intertek/SGS fabric shrinkage & colorfastness tests" },
            new() { AccountCode = "5005", AccountName = "General & Administrative Expenses", Category = AccountCategory.Expense, Currency = "BDT", OpeningBalance = 0m, CurrentBalance = 410000m, Description = "Office rent, internet, audit, compliance fees" }
        };

        await context.Accounts.AddRangeAsync(accounts);
        await context.SaveChangesAsync();

        var bankAcc = accounts.First(a => a.AccountCode == "1002");
        var arAcc = accounts.First(a => a.AccountCode == "1003");
        var apAcc = accounts.First(a => a.AccountCode == "2001");
        var exportSalesAcc = accounts.First(a => a.AccountCode == "4001");
        var fabricExpAcc = accounts.First(a => a.AccountCode == "5001");

        // Seed Initial Balanced Journal Vouchers
        var jv1 = new JournalVoucher
        {
            VoucherNumber = "JV-2026-0001",
            VoucherDate = DateTime.UtcNow.AddDays(-15),
            Type = VoucherType.Journal,
            ReferenceType = "DISPATCH",
            ReferenceNumber = "DC-2026-001",
            Narration = "Export Invoice Booking for H&M Spring Polo Shirt Order (50,000 Pcs)",
            TotalDebit = 6500000m,
            TotalCredit = 6500000m,
            Status = VoucherStatus.Posted,
            PostedAt = DateTime.UtcNow.AddDays(-15),
            PostedBy = "Chief Financial Officer",
            Lines = new List<JournalEntryLine>
            {
                new() { AccountId = arAcc.Id, DebitAmount = 6500000m, CreditAmount = 0m, LineNarration = "Receivable from H&M Global Procurement" },
                new() { AccountId = exportSalesAcc.Id, DebitAmount = 0m, CreditAmount = 6500000m, LineNarration = "Revenue recognition on Bill of Lading BL-99214" }
            }
        };

        var jv2 = new JournalVoucher
        {
            VoucherNumber = "JV-2026-0002",
            VoucherDate = DateTime.UtcNow.AddDays(-8),
            Type = VoucherType.Payment,
            ReferenceType = "PO",
            ReferenceNumber = "PO-2026-0042",
            Narration = "Bank TT Payment to Square Textiles Ltd for 100% Combed Cotton Yarn",
            TotalDebit = 2800000m,
            TotalCredit = 2800000m,
            Status = VoucherStatus.Posted,
            PostedAt = DateTime.UtcNow.AddDays(-8),
            PostedBy = "Treasury Manager",
            Lines = new List<JournalEntryLine>
            {
                new() { AccountId = apAcc.Id, DebitAmount = 2800000m, CreditAmount = 0m, LineNarration = "Settlement of supplier invoice INV-SQ-881" },
                new() { AccountId = bankAcc.Id, DebitAmount = 0m, CreditAmount = 2800000m, LineNarration = "Payment debited from SCB Principal Account" }
            }
        };

        var jv3 = new JournalVoucher
        {
            VoucherNumber = "JV-2026-0003",
            VoucherDate = DateTime.UtcNow.AddDays(-2),
            Type = VoucherType.Journal,
            ReferenceType = "GRN",
            ReferenceNumber = "GRN-2026-018",
            Narration = "Fabric Issue to Cutting Section for Zara Mens Denim Jacket Order",
            TotalDebit = 1950000m,
            TotalCredit = 1950000m,
            Status = VoucherStatus.Draft,
            Lines = new List<JournalEntryLine>
            {
                new() { AccountId = fabricExpAcc.Id, DebitAmount = 1950000m, CreditAmount = 0m, LineNarration = "Raw material consumption in Cutting Lot #12" },
                new() { AccountId = accounts.First(a => a.AccountCode == "1004").Id, DebitAmount = 0m, CreditAmount = 1950000m, LineNarration = "Inventory deduction from Central Fabric Warehouse" }
            }
        };

        await context.JournalVouchers.AddRangeAsync(jv1, jv2, jv3);
        await context.SaveChangesAsync();

        // ACC-03: Seed Trade Finance Master LC & B2B LC
        if (!await context.MasterLetterOfCredits.AnyAsync())
        {
            var mlc = new MasterLetterOfCredit
            {
                MLCNumber = "MLC-2026-0891",
                BuyerId = Guid.Parse("11111111-2222-3333-4444-555555555555"),
                BuyerName = "H&M Hennes & Mauritz GBC AB",
                IssuingBank = "Swedbank Sweden",
                AdvisingBank = "Standard Chartered Bank Dhaka",
                Beneficiary = "StitchMind Garments Industries Ltd.",
                TotalAmount = 180000.00m,
                Currency = "USD",
                IssueDate = DateTime.UtcNow.AddDays(-35),
                ExpiryDate = DateTime.UtcNow.AddDays(85),
                LatestShipmentDate = DateTime.UtcNow.AddDays(40),
                MaxB2BAllocationPct = 70.0m,
                UtilizedB2BAmount = 86362.50m,
                Status = LetterOfCreditStatus.PartiallyUtilized,
                RealizedAmount = 180000.00m,
                RealizedAtUtc = DateTime.UtcNow.AddDays(-5)
            };
            await context.MasterLetterOfCredits.AddAsync(mlc);
            await context.SaveChangesAsync();

            var b2b = new BackToBackLetterOfCredit
            {
                B2BLCNumber = "B2B-2026-0098",
                MasterLCId = mlc.Id,
                MasterLCNumber = mlc.MLCNumber,
                SupplierId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                SupplierName = "Apex Textile Mills Ltd.",
                Amount = 86362.50m,
                Currency = "USD",
                IssueDate = DateTime.UtcNow.AddDays(-25),
                MaturityDate = DateTime.UtcNow.AddDays(65),
                TenorDays = 90,
                Status = BackToBackLCStatus.Settled,
                SettledAmount = 86362.50m,
                SettledAtUtc = DateTime.UtcNow.AddDays(-3),
                SettlementReference = "SCB-DEBIT-TXN-9981"
            };
            await context.BackToBackLetterOfCredits.AddAsync(b2b);

            var fdbp = new ForeignBillDiscounting
            {
                FDBPNumber = "FDBP-2026-0034",
                MasterLCId = mlc.Id,
                MasterLCNumber = mlc.MLCNumber,
                BankName = "Standard Chartered Bank Dhaka",
                BillOfLadingNumber = "BL-SWED-2026-7890",
                FOBValue = 180000.00m,
                AdvancePercentage = 90.0m,
                AdvancedAmount = 162000.00m,
                BankDiscountCharges = 1875.00m,
                NetProceedsRealized = 160125.00m,
                Currency = "USD",
                RealizationDate = DateTime.UtcNow.AddDays(-5),
                Status = "Realized",
                Remarks = "Clean Bill of Lading accepted; 90% advance credited to SCB Principal A/C"
            };
            await context.ForeignBillDiscountings.AddAsync(fdbp);
            await context.SaveChangesAsync();
        }

        // ACC-02: Seed Vendor Bills & Automated Three-Way Matching
        if (!await context.VendorBills.AnyAsync())
        {
            var bill1 = new VendorBill
            {
                BillNumber = "BILL-APEX-2026-091",
                SupplierId = Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"),
                SupplierName = "Apex Textile Mills Ltd.",
                PurchaseOrderId = Guid.Parse("99999999-8888-7777-6666-555555555555"),
                PurchaseOrderNumber = "PO-2026-0145",
                GRNId = Guid.Parse("12121212-3434-5656-7878-909090909090"),
                GRNNumber = "GRN-2026-0210",
                InvoiceDate = DateTime.UtcNow.AddDays(-10),
                DueDate = DateTime.UtcNow.AddDays(20),
                BilledUnitPrice = 2.3500m,
                BilledQuantity = 36750.0m,
                BilledAmount = 86362.50m,
                Currency = "USD",
                TaxDeductedAtSourceTDS = 2590.88m, // 3% TDS
                VatDeductedAtSourceVDS = 6477.19m, // 7.5% VDS
                NetPayableAmount = 77294.43m,
                Notes = "100% Cotton Twill 240 GSM for H&M Chino Pant (AC-06 compliant)"
            };

            // Execute 3-Way Match (Pass)
            bill1.ExecuteThreeWayMatch(poUnitPrice: 2.3500m, grnPassedQuantity: 36750.0m, evaluatedBy: "Automated Reconciliation Engine");

            var bill2 = new VendorBill
            {
                BillNumber = "BILL-TEX-2026-042",
                SupplierId = Guid.Parse("bbbbbbbb-cccc-dddd-eeee-ffffffffffff"),
                SupplierName = "Square Fashion Yarns Ltd.",
                PurchaseOrderId = Guid.Parse("88888888-7777-6666-5555-444444444444"),
                PurchaseOrderNumber = "PO-2026-0150",
                GRNId = Guid.Parse("23232323-4545-6767-8989-010101010101"),
                GRNNumber = "GRN-2026-0215",
                InvoiceDate = DateTime.UtcNow.AddDays(-5),
                DueDate = DateTime.UtcNow.AddDays(25),
                BilledUnitPrice = 2.6500m, // PO was 2.4000 -> Markup!
                BilledQuantity = 15000.0m,
                BilledAmount = 39750.00m,
                Currency = "USD",
                TaxDeductedAtSourceTDS = 1192.50m,
                VatDeductedAtSourceVDS = 2981.25m,
                NetPayableAmount = 35576.25m,
                Notes = "Price discrepancy flag test bill"
            };

            // Execute 3-Way Match (Fail -> Lock)
            bill2.ExecuteThreeWayMatch(poUnitPrice: 2.4000m, grnPassedQuantity: 15000.0m, evaluatedBy: "Automated Reconciliation Engine");

            await context.VendorBills.AddRangeAsync(bill1, bill2);
            await context.SaveChangesAsync();
        }

        // ACC-04: Seed Order Costing Variance (Style ST-2026-CHINO-01)
        if (!await context.OrderCostingVariances.AnyAsync())
        {
            var costingVariance = new OrderCostingVariance
            {
                StyleId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                StyleNumber = "ST-2026-CHINO-01",
                StyleName = "Slim Fit Cotton Chino Pant",
                BuyerName = "H&M Hennes & Mauritz GBC AB",
                OrderQuantity = 20000,
                BuyerFOBPricePerPc = 9.0000m, // Revenue = $180,000.00
                BudgetRawMaterialsCost = 88500.00m,
                ActualRawMaterialsCost = 86362.50m, // Favorable +$2,137.50
                BudgetDirectLaborCost = 44000.00m,
                ActualDirectLaborCost = 43210.00m, // Favorable +$790.00
                BudgetOverheadsCost = 30000.00m,
                ActualOverheadsCost = 29900.00m, // Favorable +$100.00
                Currency = "USD",
                AnalyzedAtUtc = DateTime.UtcNow,
                EvaluatedBy = "Cost Accounting Lead"
            };
            await context.OrderCostingVariances.AddAsync(costingVariance);
            await context.SaveChangesAsync();
        }

        // ACC-05: Seed Currency Exchange Rates
        if (!await context.CurrencyExchangeRates.AnyAsync())
        {
            var rates = new List<CurrencyExchangeRate>
            {
                new() { FromCurrency = "USD", ToCurrency = "BDT", ExchangeRate = 117.50m, EffectiveDate = DateTime.UtcNow.Date, Source = "Bangladesh Bank Reference Rate", IsActive = true },
                new() { FromCurrency = "EUR", ToCurrency = "BDT", ExchangeRate = 127.80m, EffectiveDate = DateTime.UtcNow.Date, Source = "Bangladesh Bank Reference Rate", IsActive = true },
                new() { FromCurrency = "GBP", ToCurrency = "BDT", ExchangeRate = 150.25m, EffectiveDate = DateTime.UtcNow.Date, Source = "Bangladesh Bank Reference Rate", IsActive = true }
            };
            await context.CurrencyExchangeRates.AddRangeAsync(rates);
            await context.SaveChangesAsync();
        }

        // ACC-06: Seed Fixed Assets Register
        if (!await context.FixedAssets.AnyAsync())
        {
            var assets = new List<FixedAsset>
            {
                new()
                {
                    AssetCode = "FA-SEW-001",
                    AssetName = "Juki DDL-9000C Direct Drive High Speed Lockstitch Machine",
                    Department = "Sewing Floor Line 04",
                    PurchaseDate = DateTime.UtcNow.AddYears(-1),
                    OriginalCost = 1450.00m,
                    SalvageValue = 150.00m,
                    UsefulLifeYears = 5,
                    DepreciationMethod = "StraightLine",
                    AccumulatedDepreciation = 260.00m,
                    IsActive = true
                },
                new()
                {
                    AssetCode = "FA-CUT-002",
                    AssetName = "Eastman Straight Knife Automatic Cloth Cutting Machine 10-Inch",
                    Department = "Cutting Section Table 02",
                    PurchaseDate = DateTime.UtcNow.AddYears(-1),
                    OriginalCost = 2800.00m,
                    SalvageValue = 300.00m,
                    UsefulLifeYears = 5,
                    DepreciationMethod = "StraightLine",
                    AccumulatedDepreciation = 500.00m,
                    IsActive = true
                }
            };
            await context.FixedAssets.AddRangeAsync(assets);
            await context.SaveChangesAsync();
        }

        logger.LogInformation("Finance data seeded successfully with accounts, vouchers, trade LCs, vendor bills, costing variance, and fixed assets.");
    }
}
