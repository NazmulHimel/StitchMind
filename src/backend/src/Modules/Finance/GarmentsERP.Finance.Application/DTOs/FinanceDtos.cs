using GarmentsERP.Finance.Domain.Enums;

namespace GarmentsERP.Finance.Application.DTOs;

public record AccountDto(
    Guid Id,
    string AccountCode,
    string AccountName,
    AccountCategory Category,
    string CategoryName,
    string Currency,
    decimal OpeningBalance,
    decimal CurrentBalance,
    bool IsActive,
    string? Description
);

public record CreateAccountDto(
    string AccountCode,
    string AccountName,
    AccountCategory Category,
    string Currency,
    decimal OpeningBalance,
    string? Description
);

public record JournalEntryLineDto(
    Guid Id,
    Guid JournalVoucherId,
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountCategory AccountCategory,
    decimal DebitAmount,
    decimal CreditAmount,
    string? LineNarration
);

public record CreateJournalEntryLineDto(
    Guid AccountId,
    decimal DebitAmount,
    decimal CreditAmount,
    string? LineNarration
);

public record JournalVoucherDto(
    Guid Id,
    string VoucherNumber,
    DateTime VoucherDate,
    VoucherType Type,
    string TypeName,
    string? ReferenceType,
    string? ReferenceNumber,
    string Narration,
    decimal TotalDebit,
    decimal TotalCredit,
    VoucherStatus Status,
    string StatusName,
    DateTime? PostedAt,
    string? PostedBy,
    List<JournalEntryLineDto> Lines
);

public record CreateJournalVoucherDto(
    DateTime VoucherDate,
    VoucherType Type,
    string? ReferenceType,
    string? ReferenceNumber,
    string Narration,
    List<CreateJournalEntryLineDto> Lines
);

public record TrialBalanceItemDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountCategory Category,
    decimal DebitAmount,
    decimal CreditAmount,
    decimal NetBalance
);

public record TrialBalanceDto(
    DateTime AsOfDate,
    List<TrialBalanceItemDto> Items,
    decimal TotalDebit,
    decimal TotalCredit,
    bool IsBalanced
);

public record IncomeStatementItemDto(
    string AccountCode,
    string AccountName,
    decimal Amount
);

public record IncomeStatementDto(
    DateTime FromDate,
    DateTime ToDate,
    List<IncomeStatementItemDto> Revenues,
    decimal TotalRevenue,
    List<IncomeStatementItemDto> CostOfGoodsSold,
    decimal TotalCOGS,
    decimal GrossProfit,
    List<IncomeStatementItemDto> OperatingExpenses,
    decimal TotalOperatingExpenses,
    decimal NetProfit
);

public record BalanceSheetItemDto(
    string AccountCode,
    string AccountName,
    decimal Amount
);

public record BalanceSheetDto(
    DateTime AsOfDate,
    List<BalanceSheetItemDto> CurrentAssets,
    decimal TotalCurrentAssets,
    List<BalanceSheetItemDto> NonCurrentAssets,
    decimal TotalNonCurrentAssets,
    decimal TotalAssets,
    List<BalanceSheetItemDto> CurrentLiabilities,
    decimal TotalCurrentLiabilities,
    List<BalanceSheetItemDto> NonCurrentLiabilities,
    decimal TotalNonCurrentLiabilities,
    decimal TotalLiabilities,
    List<BalanceSheetItemDto> EquityItems,
    decimal CurrentPeriodProfit,
    decimal TotalEquity,
    decimal TotalLiabilitiesAndEquity,
    bool IsBalanced
);

public record GeneralLedgerEntryDto(
    Guid VoucherId,
    string VoucherNumber,
    DateTime VoucherDate,
    VoucherType VoucherType,
    string? ReferenceNumber,
    string Narration,
    string? LineNarration,
    decimal Debit,
    decimal Credit,
    decimal RunningBalance
);

public record GeneralLedgerAccountDto(
    Guid AccountId,
    string AccountCode,
    string AccountName,
    AccountCategory Category,
    decimal OpeningBalance,
    decimal ClosingBalance,
    List<GeneralLedgerEntryDto> Entries
);

public record GeneralLedgerDto(
    DateTime? FromDate,
    DateTime? ToDate,
    List<GeneralLedgerAccountDto> Accounts
);

public record FinanceMetricsDto(
    decimal CashAndBankBalance,
    decimal TotalAccountsReceivable,
    decimal TotalAccountsPayable,
    decimal TotalRevenueYtd,
    decimal TotalExpensesYtd,
    decimal NetProfitYtd,
    int TotalVouchersCount,
    int PendingDraftVouchersCount,
    int ActiveAccountsCount
);
