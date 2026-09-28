using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Finance.Domain.Entities;
using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Finance.Infrastructure.Data;
using GarmentsERP.Shared.Common;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Finance.Infrastructure.Repositories;

public class FinanceRepository : IFinanceRepository
{
    private readonly FinanceDbContext _context;

    public FinanceRepository(FinanceDbContext context)
    {
        _context = context;
    }

    public async Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<Account?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .Include(a => a.JournalLines)
            .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task<Account?> GetAccountByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return await _context.Accounts
            .FirstOrDefaultAsync(a => a.AccountCode == code, cancellationToken);
    }

    public async Task<Account> CreateAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Add(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<Account> UpdateAccountAsync(Account account, CancellationToken cancellationToken = default)
    {
        _context.Accounts.Update(account);
        await _context.SaveChangesAsync(cancellationToken);
        return account;
    }

    public async Task<List<JournalVoucher>> GetAllJournalVouchersAsync(CancellationToken cancellationToken = default)
    {
        return await _context.JournalVouchers
            .Include(v => v.Lines)
                .ThenInclude(l => l.Account)
            .OrderByDescending(v => v.VoucherDate)
            .ThenByDescending(v => v.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<JournalVoucher?> GetJournalVoucherByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.JournalVouchers
            .Include(v => v.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    public async Task<JournalVoucher> CreateJournalVoucherAsync(JournalVoucher voucher, CancellationToken cancellationToken = default)
    {
        _context.JournalVouchers.Add(voucher);
        await _context.SaveChangesAsync(cancellationToken);
        return voucher;
    }

    public async Task<Result<JournalVoucher>> PostJournalVoucherAsync(Guid voucherId, string postedBy, CancellationToken cancellationToken = default)
    {
        var voucher = await _context.JournalVouchers
            .Include(v => v.Lines)
                .ThenInclude(l => l.Account)
            .FirstOrDefaultAsync(v => v.Id == voucherId, cancellationToken);

        if (voucher == null)
            return Result<JournalVoucher>.Failure("Journal voucher not found.");

        if (voucher.Status == VoucherStatus.Posted)
            return Result<JournalVoucher>.Failure("Journal voucher is already posted.");

        if (voucher.Status == VoucherStatus.Cancelled)
            return Result<JournalVoucher>.Failure("Cancelled vouchers cannot be posted.");

        if (voucher.Lines == null || voucher.Lines.Count < 2)
            return Result<JournalVoucher>.Failure("Voucher must have at least 2 lines.");

        decimal totalDebit = voucher.Lines.Sum(l => l.DebitAmount);
        decimal totalCredit = voucher.Lines.Sum(l => l.CreditAmount);

        if (Math.Abs(totalDebit - totalCredit) > 0.001m)
            return Result<JournalVoucher>.Failure($"Debit ({totalDebit:N2}) and Credit ({totalCredit:N2}) must be equal.");

        // Update account balances
        foreach (var line in voucher.Lines)
        {
            var account = line.Account ?? await _context.Accounts.FindAsync(new object[] { line.AccountId }, cancellationToken);
            if (account != null)
            {
                if (account.Category == AccountCategory.Asset || account.Category == AccountCategory.Expense)
                {
                    account.CurrentBalance += (line.DebitAmount - line.CreditAmount);
                }
                else
                {
                    account.CurrentBalance += (line.CreditAmount - line.DebitAmount);
                }
            }
        }

        voucher.Status = VoucherStatus.Posted;
        voucher.PostedAt = DateTime.UtcNow;
        voucher.PostedBy = string.IsNullOrWhiteSpace(postedBy) ? "Admin" : postedBy;

        await _context.SaveChangesAsync(cancellationToken);
        return Result<JournalVoucher>.Success(voucher);
    }

    public async Task<Result<JournalVoucher>> CancelJournalVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default)
    {
        var voucher = await _context.JournalVouchers
            .Include(v => v.Lines)
            .FirstOrDefaultAsync(v => v.Id == voucherId, cancellationToken);

        if (voucher == null)
            return Result<JournalVoucher>.Failure("Journal voucher not found.");

        if (voucher.Status == VoucherStatus.Posted)
            return Result<JournalVoucher>.Failure("Posted vouchers cannot be cancelled directly. Please create a reversal journal voucher.");

        voucher.Status = VoucherStatus.Cancelled;
        await _context.SaveChangesAsync(cancellationToken);
        return Result<JournalVoucher>.Success(voucher);
    }

    public async Task<GeneralLedgerDto> GetGeneralLedgerAsync(Guid? accountId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        var accountsQuery = _context.Accounts.AsQueryable();
        if (accountId.HasValue)
        {
            accountsQuery = accountsQuery.Where(a => a.Id == accountId.Value);
        }

        var accounts = await accountsQuery.OrderBy(a => a.AccountCode).ToListAsync(cancellationToken);

        var voucherLinesQuery = _context.JournalEntryLines
            .Include(l => l.JournalVoucher)
            .Where(l => l.JournalVoucher.Status == VoucherStatus.Posted);

        if (fromDate.HasValue)
            voucherLinesQuery = voucherLinesQuery.Where(l => l.JournalVoucher.VoucherDate >= fromDate.Value);

        if (toDate.HasValue)
            voucherLinesQuery = voucherLinesQuery.Where(l => l.JournalVoucher.VoucherDate <= toDate.Value);

        var lines = await voucherLinesQuery
            .OrderBy(l => l.JournalVoucher.VoucherDate)
            .ThenBy(l => l.JournalVoucher.VoucherNumber)
            .ToListAsync(cancellationToken);

        var ledgerAccounts = new List<GeneralLedgerAccountDto>();

        foreach (var acc in accounts)
        {
            var accLines = lines.Where(l => l.AccountId == acc.Id).ToList();
            var entries = new List<GeneralLedgerEntryDto>();
            decimal running = acc.OpeningBalance;

            foreach (var line in accLines)
            {
                if (acc.Category == AccountCategory.Asset || acc.Category == AccountCategory.Expense)
                {
                    running += (line.DebitAmount - line.CreditAmount);
                }
                else
                {
                    running += (line.CreditAmount - line.DebitAmount);
                }

                entries.Add(new GeneralLedgerEntryDto(
                    line.JournalVoucherId,
                    line.JournalVoucher.VoucherNumber,
                    line.JournalVoucher.VoucherDate,
                    line.JournalVoucher.Type,
                    line.JournalVoucher.ReferenceNumber,
                    line.JournalVoucher.Narration,
                    line.LineNarration,
                    line.DebitAmount,
                    line.CreditAmount,
                    running
                ));
            }

            ledgerAccounts.Add(new GeneralLedgerAccountDto(
                acc.Id,
                acc.AccountCode,
                acc.AccountName,
                acc.Category,
                acc.OpeningBalance,
                running,
                entries
            ));
        }

        return new GeneralLedgerDto(fromDate, toDate, ledgerAccounts);
    }

    public async Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime? asOfDate, CancellationToken cancellationToken = default)
    {
        var targetDate = asOfDate ?? DateTime.UtcNow;
        var accounts = await _context.Accounts.Where(a => a.IsActive).OrderBy(a => a.AccountCode).ToListAsync(cancellationToken);

        var postedLines = await _context.JournalEntryLines
            .Include(l => l.JournalVoucher)
            .Where(l => l.JournalVoucher.Status == VoucherStatus.Posted && l.JournalVoucher.VoucherDate <= targetDate)
            .ToListAsync(cancellationToken);

        var items = new List<TrialBalanceItemDto>();
        decimal grandTotalDebit = 0;
        decimal grandTotalCredit = 0;

        foreach (var acc in accounts)
        {
            var accLines = postedLines.Where(l => l.AccountId == acc.Id).ToList();
            decimal totalDebit = accLines.Sum(l => l.DebitAmount);
            decimal totalCredit = accLines.Sum(l => l.CreditAmount);

            decimal net = 0;
            decimal debitCol = 0;
            decimal creditCol = 0;

            if (acc.Category == AccountCategory.Asset || acc.Category == AccountCategory.Expense)
            {
                net = acc.OpeningBalance + totalDebit - totalCredit;
                if (net >= 0)
                {
                    debitCol = net;
                }
                else
                {
                    creditCol = -net;
                }
            }
            else
            {
                net = acc.OpeningBalance + totalCredit - totalDebit;
                if (net >= 0)
                {
                    creditCol = net;
                }
                else
                {
                    debitCol = -net;
                }
            }

            grandTotalDebit += debitCol;
            grandTotalCredit += creditCol;

            items.Add(new TrialBalanceItemDto(
                acc.Id,
                acc.AccountCode,
                acc.AccountName,
                acc.Category,
                debitCol,
                creditCol,
                net
            ));
        }

        return new TrialBalanceDto(
            targetDate,
            items,
            grandTotalDebit,
            grandTotalCredit,
            Math.Abs(grandTotalDebit - grandTotalCredit) < 0.01m
        );
    }

    public async Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default)
    {
        var start = fromDate ?? new DateTime(DateTime.UtcNow.Year, 1, 1);
        var end = toDate ?? DateTime.UtcNow;

        var revenueAccounts = await _context.Accounts
            .Where(a => a.Category == AccountCategory.Revenue)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        var expenseAccounts = await _context.Accounts
            .Where(a => a.Category == AccountCategory.Expense)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        var postedLines = await _context.JournalEntryLines
            .Include(l => l.JournalVoucher)
            .Where(l => l.JournalVoucher.Status == VoucherStatus.Posted
                        && l.JournalVoucher.VoucherDate >= start
                        && l.JournalVoucher.VoucherDate <= end)
            .ToListAsync(cancellationToken);

        var revenues = new List<IncomeStatementItemDto>();
        decimal totalRevenue = 0;

        foreach (var rev in revenueAccounts)
        {
            var lines = postedLines.Where(l => l.AccountId == rev.Id).ToList();
            decimal amt = rev.OpeningBalance + lines.Sum(l => l.CreditAmount) - lines.Sum(l => l.DebitAmount);
            if (amt <= 0 && rev.CurrentBalance > 0)
            {
                amt = rev.CurrentBalance;
            }
            revenues.Add(new IncomeStatementItemDto(rev.AccountCode, rev.AccountName, amt));
            totalRevenue += amt;
        }

        var cogs = new List<IncomeStatementItemDto>();
        var opex = new List<IncomeStatementItemDto>();
        decimal totalCogs = 0;
        decimal totalOpex = 0;

        foreach (var exp in expenseAccounts)
        {
            var lines = postedLines.Where(l => l.AccountId == exp.Id).ToList();
            decimal amt = exp.OpeningBalance + lines.Sum(l => l.DebitAmount) - lines.Sum(l => l.CreditAmount);
            if (amt <= 0 && exp.CurrentBalance > 0) amt = exp.CurrentBalance;

            if (exp.AccountCode.StartsWith("5001") || exp.AccountCode.StartsWith("5002"))
            {
                cogs.Add(new IncomeStatementItemDto(exp.AccountCode, exp.AccountName, amt));
                totalCogs += amt;
            }
            else
            {
                opex.Add(new IncomeStatementItemDto(exp.AccountCode, exp.AccountName, amt));
                totalOpex += amt;
            }
        }

        decimal grossProfit = totalRevenue - totalCogs;
        decimal netProfit = grossProfit - totalOpex;

        return new IncomeStatementDto(
            start,
            end,
            revenues,
            totalRevenue,
            cogs,
            totalCogs,
            grossProfit,
            opex,
            totalOpex,
            netProfit
        );
    }

    public async Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime? asOfDate, CancellationToken cancellationToken = default)
    {
        var targetDate = asOfDate ?? DateTime.UtcNow;

        var assetAccounts = await _context.Accounts
            .Where(a => a.Category == AccountCategory.Asset)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        var liabilityAccounts = await _context.Accounts
            .Where(a => a.Category == AccountCategory.Liability)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        var equityAccounts = await _context.Accounts
            .Where(a => a.Category == AccountCategory.Equity)
            .OrderBy(a => a.AccountCode)
            .ToListAsync(cancellationToken);

        var currentAssets = new List<BalanceSheetItemDto>();
        var nonCurrentAssets = new List<BalanceSheetItemDto>();
        decimal totalCA = 0;
        decimal totalNCA = 0;

        foreach (var a in assetAccounts)
        {
            decimal bal = a.CurrentBalance;
            if (a.AccountCode.StartsWith("1001") || a.AccountCode.StartsWith("1002") || a.AccountCode.StartsWith("1003") || a.AccountCode.StartsWith("1004") || a.AccountCode.StartsWith("1005") || a.AccountCode.StartsWith("1006"))
            {
                currentAssets.Add(new BalanceSheetItemDto(a.AccountCode, a.AccountName, bal));
                totalCA += bal;
            }
            else
            {
                nonCurrentAssets.Add(new BalanceSheetItemDto(a.AccountCode, a.AccountName, bal));
                totalNCA += bal;
            }
        }

        var currentLiabilities = new List<BalanceSheetItemDto>();
        var nonCurrentLiabilities = new List<BalanceSheetItemDto>();
        decimal totalCL = 0;
        decimal totalNCL = 0;

        foreach (var l in liabilityAccounts)
        {
            decimal bal = l.CurrentBalance;
            if (l.AccountCode.StartsWith("2001") || l.AccountCode.StartsWith("2002") || l.AccountCode.StartsWith("2003"))
            {
                currentLiabilities.Add(new BalanceSheetItemDto(l.AccountCode, l.AccountName, bal));
                totalCL += bal;
            }
            else
            {
                nonCurrentLiabilities.Add(new BalanceSheetItemDto(l.AccountCode, l.AccountName, bal));
                totalNCL += bal;
            }
        }

        var equityItems = new List<BalanceSheetItemDto>();
        decimal totalEq = 0;

        foreach (var eq in equityAccounts)
        {
            decimal bal = eq.CurrentBalance;
            equityItems.Add(new BalanceSheetItemDto(eq.AccountCode, eq.AccountName, bal));
            totalEq += bal;
        }

        decimal totalAssets = totalCA + totalNCA;
        decimal totalLiabilities = totalCL + totalNCL;
        
        // In accounting, Current Period Profit = Total Assets - Total Liabilities - Share Capital & Retained Earnings
        decimal currentProfit = totalAssets - (totalLiabilities + totalEq);
        totalEq += currentProfit;

        decimal totalLiabilitiesAndEquity = totalLiabilities + totalEq;

        return new BalanceSheetDto(
            targetDate,
            currentAssets,
            totalCA,
            nonCurrentAssets,
            totalNCA,
            totalAssets,
            currentLiabilities,
            totalCL,
            nonCurrentLiabilities,
            totalNCL,
            totalLiabilities,
            equityItems,
            currentProfit,
            totalEq,
            totalLiabilitiesAndEquity,
            Math.Abs(totalAssets - totalLiabilitiesAndEquity) < 0.01m
        );
    }

    public async Task<FinanceMetricsDto> GetFinanceMetricsAsync(CancellationToken cancellationToken = default)
    {
        var accounts = await _context.Accounts.ToListAsync(cancellationToken);

        decimal cashAndBank = accounts
            .Where(a => a.AccountCode == "1001" || a.AccountCode == "1002")
            .Sum(a => a.CurrentBalance);

        decimal ar = accounts
            .Where(a => a.AccountCode == "1003")
            .Sum(a => a.CurrentBalance);

        decimal ap = accounts
            .Where(a => a.AccountCode == "2001" || a.AccountCode == "2002")
            .Sum(a => a.CurrentBalance);

        decimal rev = accounts
            .Where(a => a.Category == AccountCategory.Revenue)
            .Sum(a => a.CurrentBalance);

        decimal exp = accounts
            .Where(a => a.Category == AccountCategory.Expense)
            .Sum(a => a.CurrentBalance);

        decimal netProfit = rev - exp;

        int totalVouchers = await _context.JournalVouchers.CountAsync(cancellationToken);
        int draftVouchers = await _context.JournalVouchers.CountAsync(v => v.Status == VoucherStatus.Draft, cancellationToken);
        int activeAccounts = accounts.Count(a => a.IsActive);

        return new FinanceMetricsDto(
            cashAndBank,
            ar,
            ap,
            rev,
            exp,
            netProfit,
            totalVouchers,
            draftVouchers,
            activeAccounts
        );
    }

    // ACC-02: Automated Three-Way Matching
    public async Task<List<VendorBillDto>> GetVendorBillsAsync(CancellationToken cancellationToken = default)
    {
        var bills = await _context.VendorBills
            .Include(b => b.MatchLogs)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync(cancellationToken);

        return bills.Select(MapVendorBillDto).ToList();
    }

    public async Task<VendorBillDto?> GetVendorBillByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var bill = await _context.VendorBills
            .Include(b => b.MatchLogs)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        return bill != null ? MapVendorBillDto(bill) : null;
    }

    public async Task<VendorBillDto> CreateVendorBillAsync(CreateVendorBillDto dto, CancellationToken cancellationToken = default)
    {
        decimal billedAmount = Math.Round(dto.BilledUnitPrice * dto.BilledQuantity, 2);
        decimal tds = Math.Round(billedAmount * 0.03m, 2); // 3% TDS
        decimal vds = Math.Round(billedAmount * 0.075m, 2); // 7.5% VDS
        decimal netPayable = billedAmount - tds - vds;

        var bill = new VendorBill
        {
            BillNumber = dto.BillNumber,
            SupplierId = dto.SupplierId,
            SupplierName = dto.SupplierName,
            PurchaseOrderId = dto.PurchaseOrderId,
            PurchaseOrderNumber = dto.PurchaseOrderNumber,
            GRNId = dto.GRNId,
            GRNNumber = dto.GRNNumber,
            InvoiceDate = dto.InvoiceDate,
            DueDate = dto.DueDate,
            BilledUnitPrice = dto.BilledUnitPrice,
            BilledQuantity = dto.BilledQuantity,
            BilledAmount = billedAmount,
            Currency = dto.Currency,
            TaxDeductedAtSourceTDS = tds,
            VatDeductedAtSourceVDS = vds,
            NetPayableAmount = netPayable,
            Notes = dto.Notes,
            MatchStatus = BillMatchStatus.PendingMatch,
            IsApprovedForPayment = false
        };

        _context.VendorBills.Add(bill);
        await _context.SaveChangesAsync(cancellationToken);
        return MapVendorBillDto(bill);
    }

    public async Task<Result<ThreeWayMatchLogDto>> ExecuteThreeWayMatchAsync(
        Guid vendorBillId,
        decimal poUnitPrice,
        decimal grnPassedQuantity,
        string evaluatedBy,
        CancellationToken cancellationToken = default)
    {
        var bill = await _context.VendorBills
            .Include(b => b.MatchLogs)
            .FirstOrDefaultAsync(b => b.Id == vendorBillId, cancellationToken);

        if (bill == null)
            return Result<ThreeWayMatchLogDto>.Failure("Vendor bill record not found.");

        var log = bill.ExecuteThreeWayMatch(poUnitPrice, grnPassedQuantity, evaluatedBy);
        await _context.SaveChangesAsync(cancellationToken);

        var logDto = new ThreeWayMatchLogDto(
            log.Id,
            log.VendorBillId,
            log.POUnitPrice,
            log.BilledUnitPrice,
            log.PriceVariancePct,
            log.GRNPassedQuantity,
            log.BilledQuantity,
            log.QuantityVariance,
            log.IsPriceMatched,
            log.IsQuantityMatched,
            log.OverallMatchResult,
            log.OverallMatchResult.ToString(),
            log.DiscrepancyNotes,
            log.EvaluatedBy,
            log.EvaluatedAtUtc
        );

        return Result<ThreeWayMatchLogDto>.Success(logDto);
    }

    // ACC-03: Commercial Trade & LCs
    public async Task<List<MasterLetterOfCreditDto>> GetMasterLCsAsync(CancellationToken cancellationToken = default)
    {
        var lcs = await _context.MasterLetterOfCredits
            .Include(m => m.BackToBackLCs)
            .OrderByDescending(m => m.CreatedAt)
            .ToListAsync(cancellationToken);

        return lcs.Select(MapMasterLCDto).ToList();
    }

    public async Task<MasterLetterOfCreditDto?> GetMasterLCByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var lc = await _context.MasterLetterOfCredits
            .Include(m => m.BackToBackLCs)
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);

        return lc != null ? MapMasterLCDto(lc) : null;
    }

    public async Task<MasterLetterOfCreditDto> CreateMasterLCAsync(CreateMasterLCDto dto, CancellationToken cancellationToken = default)
    {
        var mlc = new MasterLetterOfCredit
        {
            MLCNumber = dto.MLCNumber,
            BuyerId = dto.BuyerId,
            BuyerName = dto.BuyerName,
            IssuingBank = dto.IssuingBank,
            AdvisingBank = dto.AdvisingBank,
            TotalAmount = dto.TotalAmount,
            Currency = dto.Currency,
            IssueDate = dto.IssueDate,
            ExpiryDate = dto.ExpiryDate,
            LatestShipmentDate = dto.LatestShipmentDate,
            MaxB2BAllocationPct = dto.MaxB2BAllocationPct,
            Status = LetterOfCreditStatus.Active
        };

        _context.MasterLetterOfCredits.Add(mlc);
        await _context.SaveChangesAsync(cancellationToken);
        return MapMasterLCDto(mlc);
    }

    public async Task<List<BackToBackLetterOfCreditDto>> GetBackToBackLCsAsync(Guid? masterLCId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.BackToBackLetterOfCredits.AsQueryable();
        if (masterLCId.HasValue && masterLCId.Value != Guid.Empty)
        {
            query = query.Where(b => b.MasterLCId == masterLCId.Value);
        }

        var b2bList = await query.OrderByDescending(b => b.CreatedAt).ToListAsync(cancellationToken);
        return b2bList.Select(MapBackToBackLCDto).ToList();
    }

    public async Task<Result<BackToBackLetterOfCreditDto>> CreateBackToBackLCAsync(CreateBackToBackLCDto dto, CancellationToken cancellationToken = default)
    {
        var mlc = await _context.MasterLetterOfCredits.FirstOrDefaultAsync(m => m.Id == dto.MasterLCId, cancellationToken);
        if (mlc == null)
            return Result<BackToBackLetterOfCreditDto>.Failure("Master Letter of Credit not found.");

        if (!mlc.CanAllocateB2B(dto.Amount))
        {
            return Result<BackToBackLetterOfCreditDto>.Failure($"Requested B2B LC amount (${dto.Amount:N2}) exceeds authorized Master LC allocation headroom (${mlc.AvailableB2BAmount:N2}).");
        }

        mlc.RecordB2BAllocation(dto.Amount);

        var b2b = new BackToBackLetterOfCredit
        {
            B2BLCNumber = dto.B2BLCNumber,
            MasterLCId = dto.MasterLCId,
            MasterLCNumber = mlc.MLCNumber,
            SupplierId = dto.SupplierId,
            SupplierName = dto.SupplierName,
            Amount = dto.Amount,
            Currency = dto.Currency,
            IssueDate = dto.IssueDate,
            MaturityDate = dto.MaturityDate,
            TenorDays = dto.TenorDays,
            Status = BackToBackLCStatus.Issued
        };

        _context.BackToBackLetterOfCredits.Add(b2b);
        await _context.SaveChangesAsync(cancellationToken);

        return Result<BackToBackLetterOfCreditDto>.Success(MapBackToBackLCDto(b2b));
    }

    public async Task<List<ForeignBillDiscountingDto>> GetForeignBillDiscountingsAsync(Guid? masterLCId = null, CancellationToken cancellationToken = default)
    {
        var query = _context.ForeignBillDiscountings.AsQueryable();
        if (masterLCId.HasValue && masterLCId.Value != Guid.Empty)
        {
            query = query.Where(f => f.MasterLCId == masterLCId.Value);
        }

        var items = await query.OrderByDescending(f => f.CreatedAt).ToListAsync(cancellationToken);
        return items.Select(f => new ForeignBillDiscountingDto(
            f.Id,
            f.FDBPNumber,
            f.MasterLCId,
            f.MasterLCNumber,
            f.BankName,
            f.BillOfLadingNumber,
            f.FOBValue,
            f.AdvancePercentage,
            f.AdvancedAmount,
            f.BankDiscountCharges,
            f.NetProceedsRealized,
            f.Currency,
            f.RealizationDate,
            f.Status,
            f.Remarks
        )).ToList();
    }

    public async Task<ForeignBillDiscountingDto> CreateForeignBillDiscountingAsync(CreateForeignBillDiscountingDto dto, CancellationToken cancellationToken = default)
    {
        var mlc = await _context.MasterLetterOfCredits.FirstOrDefaultAsync(m => m.Id == dto.MasterLCId, cancellationToken);
        string mlcNum = mlc?.MLCNumber ?? "MLC-2026-0891";

        decimal advanced = Math.Round(dto.FOBValue * (dto.AdvancePercentage / 100m), 2);
        decimal netProceeds = Math.Max(0m, advanced - dto.BankDiscountCharges);

        var fdbp = new ForeignBillDiscounting
        {
            FDBPNumber = dto.FDBPNumber,
            MasterLCId = dto.MasterLCId,
            MasterLCNumber = mlcNum,
            BankName = dto.BankName,
            BillOfLadingNumber = dto.BillOfLadingNumber,
            FOBValue = dto.FOBValue,
            AdvancePercentage = dto.AdvancePercentage,
            AdvancedAmount = advanced,
            BankDiscountCharges = dto.BankDiscountCharges,
            NetProceedsRealized = netProceeds,
            Currency = dto.Currency,
            RealizationDate = DateTime.UtcNow,
            Status = "Realized",
            Remarks = dto.Remarks
        };

        _context.ForeignBillDiscountings.Add(fdbp);
        await _context.SaveChangesAsync(cancellationToken);

        return new ForeignBillDiscountingDto(
            fdbp.Id,
            fdbp.FDBPNumber,
            fdbp.MasterLCId,
            fdbp.MasterLCNumber,
            fdbp.BankName,
            fdbp.BillOfLadingNumber,
            fdbp.FOBValue,
            fdbp.AdvancePercentage,
            fdbp.AdvancedAmount,
            fdbp.BankDiscountCharges,
            fdbp.NetProceedsRealized,
            fdbp.Currency,
            fdbp.RealizationDate,
            fdbp.Status,
            fdbp.Remarks
        );
    }

    // ACC-04: Order Costing Variance Engine
    public async Task<List<OrderCostingVarianceDto>> GetOrderCostingVariancesAsync(CancellationToken cancellationToken = default)
    {
        var variances = await _context.OrderCostingVariances
            .OrderByDescending(v => v.AnalyzedAtUtc)
            .ToListAsync(cancellationToken);

        return variances.Select(MapCostingVarianceDto).ToList();
    }

    public async Task<OrderCostingVarianceDto?> GetOrderCostingVarianceByStyleIdAsync(Guid styleId, CancellationToken cancellationToken = default)
    {
        var v = await _context.OrderCostingVariances.FirstOrDefaultAsync(x => x.StyleId == styleId, cancellationToken);
        return v != null ? MapCostingVarianceDto(v) : null;
    }

    public async Task<OrderCostingVarianceDto> SaveOrderCostingVarianceAsync(OrderCostingVariance variance, CancellationToken cancellationToken = default)
    {
        var existing = await _context.OrderCostingVariances.FirstOrDefaultAsync(x => x.StyleId == variance.StyleId, cancellationToken);
        if (existing != null)
        {
            existing.BuyerFOBPricePerPc = variance.BuyerFOBPricePerPc;
            existing.OrderQuantity = variance.OrderQuantity;
            existing.BudgetRawMaterialsCost = variance.BudgetRawMaterialsCost;
            existing.ActualRawMaterialsCost = variance.ActualRawMaterialsCost;
            existing.BudgetDirectLaborCost = variance.BudgetDirectLaborCost;
            existing.ActualDirectLaborCost = variance.ActualDirectLaborCost;
            existing.BudgetOverheadsCost = variance.BudgetOverheadsCost;
            existing.ActualOverheadsCost = variance.ActualOverheadsCost;
            existing.AnalyzedAtUtc = DateTime.UtcNow;
            existing.EvaluatedBy = variance.EvaluatedBy;
            await _context.SaveChangesAsync(cancellationToken);
            return MapCostingVarianceDto(existing);
        }

        _context.OrderCostingVariances.Add(variance);
        await _context.SaveChangesAsync(cancellationToken);
        return MapCostingVarianceDto(variance);
    }

    // ACC-05: Multi-Currency & Forex Ledger
    public async Task<List<CurrencyExchangeRateDto>> GetExchangeRatesAsync(CancellationToken cancellationToken = default)
    {
        var rates = await _context.CurrencyExchangeRates
            .Where(r => r.IsActive)
            .OrderBy(r => r.FromCurrency)
            .ToListAsync(cancellationToken);

        return rates.Select(r => new CurrencyExchangeRateDto(
            r.Id,
            r.FromCurrency,
            r.ToCurrency,
            r.ExchangeRate,
            r.EffectiveDate,
            r.Source,
            r.IsActive
        )).ToList();
    }

    public Task<ForexGainLossCalculationDto> CalculateForexGainLossAsync(
        decimal amount,
        string currency,
        decimal bookingRate,
        decimal settlementRate,
        CancellationToken cancellationToken = default)
    {
        decimal bookingBdt = Math.Round(amount * bookingRate, 2);
        decimal settlementBdt = Math.Round(amount * settlementRate, 2);
        decimal diff = settlementBdt - bookingBdt;

        return Task.FromResult(new ForexGainLossCalculationDto(
            amount,
            currency,
            bookingRate,
            settlementRate,
            bookingBdt,
            settlementBdt,
            Math.Abs(diff),
            diff >= 0m
        ));
    }

    // ACC-06: Fixed Asset Register & Depreciation
    public async Task<List<FixedAssetDto>> GetFixedAssetsAsync(CancellationToken cancellationToken = default)
    {
        var assets = await _context.FixedAssets
            .OrderBy(a => a.AssetCode)
            .ToListAsync(cancellationToken);

        return assets.Select(a => new FixedAssetDto(
            a.Id,
            a.AssetCode,
            a.AssetName,
            a.Department,
            a.PurchaseDate,
            a.OriginalCost,
            a.SalvageValue,
            a.UsefulLifeYears,
            a.DepreciationMethod,
            a.AnnualDepreciation,
            a.MonthlyDepreciation,
            a.AccumulatedDepreciation,
            a.CurrentBookValue,
            a.IsActive
        )).ToList();
    }

    public async Task<FixedAssetDto> CreateFixedAssetAsync(FixedAsset asset, CancellationToken cancellationToken = default)
    {
        _context.FixedAssets.Add(asset);
        await _context.SaveChangesAsync(cancellationToken);

        return new FixedAssetDto(
            asset.Id,
            asset.AssetCode,
            asset.AssetName,
            asset.Department,
            asset.PurchaseDate,
            asset.OriginalCost,
            asset.SalvageValue,
            asset.UsefulLifeYears,
            asset.DepreciationMethod,
            asset.AnnualDepreciation,
            asset.MonthlyDepreciation,
            asset.AccumulatedDepreciation,
            asset.CurrentBookValue,
            asset.IsActive
        );
    }

    // ACC-07: AP/AR Aging Analysis & Payment Disbursement Run
    public async Task<AgingAnalysisSummaryDto> GetAgingAnalysisAsync(string type, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow.Date;
        var items = new List<AgingItemDto>();

        if (string.Equals(type, "AccountsPayable", StringComparison.OrdinalIgnoreCase))
        {
            var bills = await _context.VendorBills
                .Where(b => b.MatchStatus != BillMatchStatus.Paid)
                .ToListAsync(cancellationToken);

            foreach (var b in bills)
            {
                int overdue = (now - b.DueDate.Date).Days;
                AgingBracket bracket = overdue switch
                {
                    <= 30 => AgingBracket.Current_0_30,
                    <= 60 => AgingBracket.Days_31_60,
                    <= 90 => AgingBracket.Days_61_90,
                    _ => AgingBracket.Days_90_Plus
                };

                items.Add(new AgingItemDto(
                    b.Id,
                    b.BillNumber,
                    b.SupplierName,
                    "Supplier",
                    b.InvoiceDate,
                    b.DueDate,
                    Math.Max(0, overdue),
                    b.BilledAmount,
                    b.NetPayableAmount,
                    bracket,
                    bracket.ToString()
                ));
            }
        }
        else
        {
            var mlcs = await _context.MasterLetterOfCredits.ToListAsync(cancellationToken);
            foreach (var mlc in mlcs)
            {
                int overdue = (now - mlc.LatestShipmentDate.Date).Days;
                AgingBracket bracket = overdue switch
                {
                    <= 30 => AgingBracket.Current_0_30,
                    <= 60 => AgingBracket.Days_31_60,
                    <= 90 => AgingBracket.Days_61_90,
                    _ => AgingBracket.Days_90_Plus
                };

                items.Add(new AgingItemDto(
                    mlc.Id,
                    mlc.MLCNumber,
                    mlc.BuyerName,
                    "Customer",
                    mlc.IssueDate,
                    mlc.LatestShipmentDate,
                    Math.Max(0, overdue),
                    mlc.TotalAmount,
                    mlc.TotalAmount - mlc.RealizedAmount,
                    bracket,
                    bracket.ToString()
                ));
            }
        }

        decimal total0_30 = items.Where(i => i.Bracket == AgingBracket.Current_0_30).Sum(i => i.BalanceAmount);
        decimal total31_60 = items.Where(i => i.Bracket == AgingBracket.Days_31_60).Sum(i => i.BalanceAmount);
        decimal total61_90 = items.Where(i => i.Bracket == AgingBracket.Days_61_90).Sum(i => i.BalanceAmount);
        decimal total90Plus = items.Where(i => i.Bracket == AgingBracket.Days_90_Plus).Sum(i => i.BalanceAmount);
        decimal grandTotal = total0_30 + total31_60 + total61_90 + total90Plus;

        return new AgingAnalysisSummaryDto(
            type,
            now,
            total0_30,
            total31_60,
            total61_90,
            total90Plus,
            grandTotal,
            items
        );
    }

    private static VendorBillDto MapVendorBillDto(VendorBill b)
    {
        return new VendorBillDto(
            b.Id,
            b.BillNumber,
            b.SupplierId,
            b.SupplierName,
            b.PurchaseOrderId,
            b.PurchaseOrderNumber,
            b.GRNId,
            b.GRNNumber,
            b.InvoiceDate,
            b.DueDate,
            b.BilledUnitPrice,
            b.BilledQuantity,
            b.BilledAmount,
            b.Currency,
            b.MatchStatus,
            b.MatchStatus.ToString(),
            b.IsApprovedForPayment,
            b.ApprovedBy,
            b.ApprovedAtUtc,
            b.TaxDeductedAtSourceTDS,
            b.VatDeductedAtSourceVDS,
            b.NetPayableAmount,
            b.Notes,
            b.MatchLogs.Select(l => new ThreeWayMatchLogDto(
                l.Id,
                l.VendorBillId,
                l.POUnitPrice,
                l.BilledUnitPrice,
                l.PriceVariancePct,
                l.GRNPassedQuantity,
                l.BilledQuantity,
                l.QuantityVariance,
                l.IsPriceMatched,
                l.IsQuantityMatched,
                l.OverallMatchResult,
                l.OverallMatchResult.ToString(),
                l.DiscrepancyNotes,
                l.EvaluatedBy,
                l.EvaluatedAtUtc
            )).ToList()
        );
    }

    private static MasterLetterOfCreditDto MapMasterLCDto(MasterLetterOfCredit m)
    {
        return new MasterLetterOfCreditDto(
            m.Id,
            m.MLCNumber,
            m.BuyerId,
            m.BuyerName,
            m.IssuingBank,
            m.AdvisingBank,
            m.Beneficiary,
            m.TotalAmount,
            m.Currency,
            m.IssueDate,
            m.ExpiryDate,
            m.LatestShipmentDate,
            m.MaxB2BAllocationPct,
            m.MaxB2BAllocationAmount,
            m.UtilizedB2BAmount,
            m.AvailableB2BAmount,
            m.Status,
            m.Status.ToString(),
            m.RealizedAmount,
            m.RealizedAtUtc,
            m.BackToBackLCs.Select(MapBackToBackLCDto).ToList()
        );
    }

    private static BackToBackLetterOfCreditDto MapBackToBackLCDto(BackToBackLetterOfCredit b)
    {
        return new BackToBackLetterOfCreditDto(
            b.Id,
            b.B2BLCNumber,
            b.MasterLCId,
            b.MasterLCNumber,
            b.SupplierId,
            b.SupplierName,
            b.Amount,
            b.Currency,
            b.IssueDate,
            b.MaturityDate,
            b.TenorDays,
            b.Status,
            b.Status.ToString(),
            b.SettledAmount,
            b.SettledAtUtc,
            b.SettlementReference
        );
    }

    private static OrderCostingVarianceDto MapCostingVarianceDto(OrderCostingVariance v)
    {
        return new OrderCostingVarianceDto(
            v.Id,
            v.StyleId,
            v.StyleNumber,
            v.StyleName,
            v.BuyerName,
            v.OrderQuantity,
            v.BuyerFOBPricePerPc,
            v.BuyerFOBRevenue,
            v.BudgetRawMaterialsCost,
            v.ActualRawMaterialsCost,
            v.RawMaterialVariance,
            v.BudgetDirectLaborCost,
            v.ActualDirectLaborCost,
            v.DirectLaborVariance,
            v.BudgetOverheadsCost,
            v.ActualOverheadsCost,
            v.OverheadsVariance,
            v.TotalBudgetCost,
            v.TotalActualCost,
            v.TotalCostVariance,
            v.NetRealizedProfit,
            v.NetProfitMarginPct,
            v.BudgetedProfit,
            v.BudgetedProfitMarginPct,
            v.Currency,
            v.AnalyzedAtUtc,
            v.EvaluatedBy
        );
    }
}

