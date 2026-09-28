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

    // ACC-02: Automated Three-Way Matching (AC-06)
    Task<List<VendorBillDto>> GetVendorBillsAsync(CancellationToken cancellationToken = default);
    Task<VendorBillDto?> GetVendorBillByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<VendorBillDto> CreateVendorBillAsync(CreateVendorBillDto dto, CancellationToken cancellationToken = default);
    Task<Result<ThreeWayMatchLogDto>> ExecuteThreeWayMatchAsync(Guid vendorBillId, decimal poUnitPrice, decimal grnPassedQuantity, string evaluatedBy, CancellationToken cancellationToken = default);

    // ACC-03: Commercial Trade & LCs
    Task<List<MasterLetterOfCreditDto>> GetMasterLCsAsync(CancellationToken cancellationToken = default);
    Task<MasterLetterOfCreditDto?> GetMasterLCByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<MasterLetterOfCreditDto> CreateMasterLCAsync(CreateMasterLCDto dto, CancellationToken cancellationToken = default);
    Task<List<BackToBackLetterOfCreditDto>> GetBackToBackLCsAsync(Guid? masterLCId = null, CancellationToken cancellationToken = default);
    Task<Result<BackToBackLetterOfCreditDto>> CreateBackToBackLCAsync(CreateBackToBackLCDto dto, CancellationToken cancellationToken = default);
    Task<List<ForeignBillDiscountingDto>> GetForeignBillDiscountingsAsync(Guid? masterLCId = null, CancellationToken cancellationToken = default);
    Task<ForeignBillDiscountingDto> CreateForeignBillDiscountingAsync(CreateForeignBillDiscountingDto dto, CancellationToken cancellationToken = default);

    // ACC-04: Order Costing Variance Engine
    Task<List<OrderCostingVarianceDto>> GetOrderCostingVariancesAsync(CancellationToken cancellationToken = default);
    Task<OrderCostingVarianceDto?> GetOrderCostingVarianceByStyleIdAsync(Guid styleId, CancellationToken cancellationToken = default);
    Task<OrderCostingVarianceDto> SaveOrderCostingVarianceAsync(OrderCostingVariance variance, CancellationToken cancellationToken = default);

    // ACC-05: Multi-Currency & Forex Ledger
    Task<List<CurrencyExchangeRateDto>> GetExchangeRatesAsync(CancellationToken cancellationToken = default);
    Task<ForexGainLossCalculationDto> CalculateForexGainLossAsync(decimal amount, string currency, decimal bookingRate, decimal settlementRate, CancellationToken cancellationToken = default);

    // ACC-06: Fixed Asset Register & Depreciation
    Task<List<FixedAssetDto>> GetFixedAssetsAsync(CancellationToken cancellationToken = default);
    Task<FixedAssetDto> CreateFixedAssetAsync(FixedAsset asset, CancellationToken cancellationToken = default);

    // ACC-07: AP/AR Aging Analysis & Payment Disbursement
    Task<AgingAnalysisSummaryDto> GetAgingAnalysisAsync(string type, CancellationToken cancellationToken = default);
}

