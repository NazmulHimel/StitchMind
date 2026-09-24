using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Domain.Entities;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.Finance.Application.Interfaces;

public interface IFinanceRepository
{
    // Accounts
    Task<List<Account>> GetAllAccountsAsync(CancellationToken cancellationToken = default);
    Task<Account?> GetAccountByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Account?> GetAccountByCodeAsync(string code, CancellationToken cancellationToken = default);
    Task<Account> CreateAccountAsync(Account account, CancellationToken cancellationToken = default);
    Task<Account> UpdateAccountAsync(Account account, CancellationToken cancellationToken = default);

    // Journal Vouchers
    Task<List<JournalVoucher>> GetAllJournalVouchersAsync(CancellationToken cancellationToken = default);
    Task<JournalVoucher?> GetJournalVoucherByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<JournalVoucher> CreateJournalVoucherAsync(JournalVoucher voucher, CancellationToken cancellationToken = default);
    Task<Result<JournalVoucher>> PostJournalVoucherAsync(Guid voucherId, string postedBy, CancellationToken cancellationToken = default);
    Task<Result<JournalVoucher>> CancelJournalVoucherAsync(Guid voucherId, CancellationToken cancellationToken = default);

    // Reports & Statements
    Task<GeneralLedgerDto> GetGeneralLedgerAsync(Guid? accountId, DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<TrialBalanceDto> GetTrialBalanceAsync(DateTime? asOfDate, CancellationToken cancellationToken = default);
    Task<IncomeStatementDto> GetIncomeStatementAsync(DateTime? fromDate, DateTime? toDate, CancellationToken cancellationToken = default);
    Task<BalanceSheetDto> GetBalanceSheetAsync(DateTime? asOfDate, CancellationToken cancellationToken = default);
    Task<FinanceMetricsDto> GetFinanceMetricsAsync(CancellationToken cancellationToken = default);
}
