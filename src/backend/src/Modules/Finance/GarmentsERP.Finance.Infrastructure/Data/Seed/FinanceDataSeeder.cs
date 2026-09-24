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

        logger.LogInformation("Finance data seeded successfully with {Count} accounts and 3 vouchers.", accounts.Count);
    }
}
