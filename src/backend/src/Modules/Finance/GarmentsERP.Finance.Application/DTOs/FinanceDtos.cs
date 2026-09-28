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

// ACC-02: Three-Way Matching DTOs
public record ThreeWayMatchLogDto(
    Guid Id,
    Guid VendorBillId,
    decimal POUnitPrice,
    decimal BilledUnitPrice,
    decimal PriceVariancePct,
    decimal GRNPassedQuantity,
    decimal BilledQuantity,
    decimal QuantityVariance,
    bool IsPriceMatched,
    bool IsQuantityMatched,
    ThreeWayMatchResult OverallMatchResult,
    string OverallMatchResultName,
    string? DiscrepancyNotes,
    string EvaluatedBy,
    DateTime EvaluatedAtUtc
);

public record VendorBillDto(
    Guid Id,
    string BillNumber,
    Guid SupplierId,
    string SupplierName,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid GRNId,
    string GRNNumber,
    DateTime InvoiceDate,
    DateTime DueDate,
    decimal BilledUnitPrice,
    decimal BilledQuantity,
    decimal BilledAmount,
    string Currency,
    BillMatchStatus MatchStatus,
    string MatchStatusName,
    bool IsApprovedForPayment,
    string? ApprovedBy,
    DateTime? ApprovedAtUtc,
    decimal TaxDeductedAtSourceTDS,
    decimal VatDeductedAtSourceVDS,
    decimal NetPayableAmount,
    string? Notes,
    List<ThreeWayMatchLogDto> MatchLogs
);

public record CreateVendorBillDto(
    string BillNumber,
    Guid SupplierId,
    string SupplierName,
    Guid PurchaseOrderId,
    string PurchaseOrderNumber,
    Guid GRNId,
    string GRNNumber,
    DateTime InvoiceDate,
    DateTime DueDate,
    decimal BilledUnitPrice,
    decimal BilledQuantity,
    string Currency,
    string? Notes
);

public record ExecuteThreeWayMatchRequest(
    decimal POUnitPrice,
    decimal GRNPassedQuantity,
    string EvaluatedBy
);

// ACC-03: Commercial Trade & LC DTOs
public record MasterLetterOfCreditDto(
    Guid Id,
    string MLCNumber,
    Guid BuyerId,
    string BuyerName,
    string IssuingBank,
    string AdvisingBank,
    string Beneficiary,
    decimal TotalAmount,
    string Currency,
    DateTime IssueDate,
    DateTime ExpiryDate,
    DateTime LatestShipmentDate,
    decimal MaxB2BAllocationPct,
    decimal MaxB2BAllocationAmount,
    decimal UtilizedB2BAmount,
    decimal AvailableB2BAmount,
    LetterOfCreditStatus Status,
    string StatusName,
    decimal RealizedAmount,
    DateTime? RealizedAtUtc,
    List<BackToBackLetterOfCreditDto> BackToBackLCs
);

public record CreateMasterLCDto(
    string MLCNumber,
    Guid BuyerId,
    string BuyerName,
    string IssuingBank,
    string AdvisingBank,
    decimal TotalAmount,
    string Currency,
    DateTime IssueDate,
    DateTime ExpiryDate,
    DateTime LatestShipmentDate,
    decimal MaxB2BAllocationPct
);

public record BackToBackLetterOfCreditDto(
    Guid Id,
    string B2BLCNumber,
    Guid MasterLCId,
    string MasterLCNumber,
    Guid SupplierId,
    string SupplierName,
    decimal Amount,
    string Currency,
    DateTime IssueDate,
    DateTime MaturityDate,
    int TenorDays,
    BackToBackLCStatus Status,
    string StatusName,
    decimal SettledAmount,
    DateTime? SettledAtUtc,
    string? SettlementReference
);

public record CreateBackToBackLCDto(
    string B2BLCNumber,
    Guid MasterLCId,
    Guid SupplierId,
    string SupplierName,
    decimal Amount,
    string Currency,
    DateTime IssueDate,
    DateTime MaturityDate,
    int TenorDays
);

public record ForeignBillDiscountingDto(
    Guid Id,
    string FDBPNumber,
    Guid MasterLCId,
    string MasterLCNumber,
    string BankName,
    string BillOfLadingNumber,
    decimal FOBValue,
    decimal AdvancePercentage,
    decimal AdvancedAmount,
    decimal BankDiscountCharges,
    decimal NetProceedsRealized,
    string Currency,
    DateTime RealizationDate,
    string Status,
    string? Remarks
);

public record CreateForeignBillDiscountingDto(
    string FDBPNumber,
    Guid MasterLCId,
    string BankName,
    string BillOfLadingNumber,
    decimal FOBValue,
    decimal AdvancePercentage,
    decimal BankDiscountCharges,
    string Currency,
    string? Remarks
);

// ACC-04: Order Costing Variance DTOs
public record OrderCostingVarianceDto(
    Guid Id,
    Guid StyleId,
    string StyleNumber,
    string StyleName,
    string BuyerName,
    int OrderQuantity,
    decimal BuyerFOBPricePerPc,
    decimal BuyerFOBRevenue,
    decimal BudgetRawMaterialsCost,
    decimal ActualRawMaterialsCost,
    decimal RawMaterialVariance,
    decimal BudgetDirectLaborCost,
    decimal ActualDirectLaborCost,
    decimal DirectLaborVariance,
    decimal BudgetOverheadsCost,
    decimal ActualOverheadsCost,
    decimal OverheadsVariance,
    decimal TotalBudgetCost,
    decimal TotalActualCost,
    decimal TotalCostVariance,
    decimal NetRealizedProfit,
    decimal NetProfitMarginPct,
    decimal BudgetedProfit,
    decimal BudgetedProfitMarginPct,
    string Currency,
    DateTime AnalyzedAtUtc,
    string EvaluatedBy
);

// ACC-05: Multi-currency & Forex DTOs
public record CurrencyExchangeRateDto(
    Guid Id,
    string FromCurrency,
    string ToCurrency,
    decimal ExchangeRate,
    DateTime EffectiveDate,
    string Source,
    bool IsActive
);

public record ForexGainLossCalculationDto(
    decimal ForeignAmount,
    string ForeignCurrency,
    decimal BookingRate,
    decimal SettlementRate,
    decimal BookingBdtAmount,
    decimal SettlementBdtAmount,
    decimal ForexGainLossBdt,
    bool IsGain
);

// ACC-06: Fixed Asset Register DTOs
public record FixedAssetDto(
    Guid Id,
    string AssetCode,
    string AssetName,
    string Department,
    DateTime PurchaseDate,
    decimal OriginalCost,
    decimal SalvageValue,
    int UsefulLifeYears,
    string DepreciationMethod,
    decimal AnnualDepreciation,
    decimal MonthlyDepreciation,
    decimal AccumulatedDepreciation,
    decimal CurrentBookValue,
    bool IsActive
);

// ACC-07: AP/AR Aging Analysis DTOs
public record AgingItemDto(
    Guid ReferenceId,
    string ReferenceNumber,
    string PartyName,
    string PartyType, // "Customer" (AR) or "Supplier" (AP)
    DateTime InvoiceDate,
    DateTime DueDate,
    int DaysOverdue,
    decimal TotalAmount,
    decimal BalanceAmount,
    AgingBracket Bracket,
    string BracketName
);

public record AgingAnalysisSummaryDto(
    string AnalysisType, // "AccountsPayable" or "AccountsReceivable"
    DateTime AsOfDate,
    decimal TotalCurrent_0_30,
    decimal TotalDays_31_60,
    decimal TotalDays_61_90,
    decimal TotalDays_90_Plus,
    decimal GrandTotalOutstanding,
    List<AgingItemDto> Items
);

