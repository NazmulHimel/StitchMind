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
}
