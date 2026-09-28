using GarmentsERP.Finance.Application.Commands;
using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Interfaces;
using GarmentsERP.Finance.Domain.Entities;
using GarmentsERP.Finance.Domain.Enums;
using GarmentsERP.Shared.Common;
using Moq;
using Xunit;

namespace GarmentsERP.Finance.UnitTests;

public class FinanceUnitTests
{
    [Fact]
    public void Account_ShouldInitializeWithCorrectCategoryAndBalance()
    {
        var account = new Account
        {
            AccountCode = "1001",
            AccountName = "Cash in Hand",
            Category = AccountCategory.Asset,
            Currency = "BDT",
            OpeningBalance = 250000m,
            CurrentBalance = 250000m,
            IsActive = true
        };

        Assert.Equal("1001", account.AccountCode);
        Assert.Equal("Cash in Hand", account.AccountName);
        Assert.Equal(AccountCategory.Asset, account.Category);
        Assert.Equal(250000m, account.OpeningBalance);
        Assert.Equal(250000m, account.CurrentBalance);
        Assert.True(account.IsActive);
    }

    [Fact]
    public async Task CreateJournalVoucher_UnbalancedDebitCredit_ShouldReturnFailure()
    {
        var mockRepo = new Mock<IFinanceRepository>();
        var handler = new CreateJournalVoucherCommandHandler(mockRepo.Object);

        var account1Id = Guid.NewGuid();
        var account2Id = Guid.NewGuid();

        var dto = new CreateJournalVoucherDto(
            DateTime.UtcNow,
            VoucherType.Journal,
            "PO",
            "PO-001",
            "Test unbalanced voucher",
            new List<CreateJournalEntryLineDto>
            {
                new(account1Id, 5000m, 0m, "Debit line"),
                new(account2Id, 0m, 4000m, "Credit line (unbalanced)")
            }
        );

        var result = await handler.Handle(new CreateJournalVoucherCommand(dto), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("must balance", result.Error ?? "");
    }

    [Fact]
    public async Task CreateJournalVoucher_LessThanTwoLines_ShouldReturnFailure()
    {
        var mockRepo = new Mock<IFinanceRepository>();
        var handler = new CreateJournalVoucherCommandHandler(mockRepo.Object);

        var account1Id = Guid.NewGuid();

        var dto = new CreateJournalVoucherDto(
            DateTime.UtcNow,
            VoucherType.Journal,
            null,
            null,
            "Single line voucher",
            new List<CreateJournalEntryLineDto>
            {
                new(account1Id, 1000m, 0m, "Only one line")
            }
        );

        var result = await handler.Handle(new CreateJournalVoucherCommand(dto), CancellationToken.None);

        Assert.False(result.IsSuccess);
        Assert.Contains("at least 2 entry lines", result.Error ?? "");
    }

    [Fact]
    public async Task CreateJournalVoucher_Balanced_ShouldSucceed()
    {
        var mockRepo = new Mock<IFinanceRepository>();
        var account1Id = Guid.NewGuid();
        var account2Id = Guid.NewGuid();

        var createdVoucher = new JournalVoucher
        {
            VoucherNumber = "JV-2026-0001",
            VoucherDate = DateTime.UtcNow,
            Type = VoucherType.Journal,
            Narration = "Balanced Export Sale",
            TotalDebit = 10000m,
            TotalCredit = 10000m,
            Status = VoucherStatus.Draft,
            Lines = new List<JournalEntryLine>
            {
                new() { AccountId = account1Id, DebitAmount = 10000m, CreditAmount = 0m, LineNarration = "AR Debit" },
                new() { AccountId = account2Id, DebitAmount = 0m, CreditAmount = 10000m, LineNarration = "Sales Revenue Credit" }
            }
        };

        mockRepo.Setup(r => r.CreateJournalVoucherAsync(It.IsAny<JournalVoucher>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdVoucher);
        mockRepo.Setup(r => r.GetJournalVoucherByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdVoucher);

        var handler = new CreateJournalVoucherCommandHandler(mockRepo.Object);

        var dto = new CreateJournalVoucherDto(
            DateTime.UtcNow,
            VoucherType.Journal,
            "DISPATCH",
            "DC-001",
            "Balanced Export Sale",
            new List<CreateJournalEntryLineDto>
            {
                new(account1Id, 10000m, 0m, "AR Debit"),
                new(account2Id, 0m, 10000m, "Sales Revenue Credit")
            }
        );

        var result = await handler.Handle(new CreateJournalVoucherCommand(dto), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(10000m, result.Value?.TotalDebit);
        Assert.Equal(10000m, result.Value?.TotalCredit);
    }

    [Fact]
    public async Task PostJournalVoucher_ValidVoucher_ShouldUpdateStatusToPosted()
    {
        var mockRepo = new Mock<IFinanceRepository>();
        var voucherId = Guid.NewGuid();

        var postedVoucher = new JournalVoucher
        {
            VoucherNumber = "JV-2026-0001",
            VoucherDate = DateTime.UtcNow,
            Type = VoucherType.Journal,
            Narration = "Approved Payment",
            TotalDebit = 5000m,
            TotalCredit = 5000m,
            Status = VoucherStatus.Posted,
            PostedAt = DateTime.UtcNow,
            PostedBy = "Finance Director"
        };

        mockRepo.Setup(r => r.PostJournalVoucherAsync(voucherId, "Finance Director", It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<JournalVoucher>.Success(postedVoucher));

        var handler = new PostJournalVoucherCommandHandler(mockRepo.Object);

        var result = await handler.Handle(new PostJournalVoucherCommand(voucherId, "Finance Director"), CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(VoucherStatus.Posted, result.Value?.Status);
        Assert.Equal("Finance Director", result.Value?.PostedBy);
    }

    [Fact]
    public void TrialBalance_DebitsMustEqualCredits()
    {
        var items = new List<TrialBalanceItemDto>
        {
            new(Guid.NewGuid(), "1001", "Cash", AccountCategory.Asset, 50000m, 0m, 50000m),
            new(Guid.NewGuid(), "1002", "Bank", AccountCategory.Asset, 150000m, 0m, 150000m),
            new(Guid.NewGuid(), "2001", "AP", AccountCategory.Liability, 0m, 80000m, 80000m),
            new(Guid.NewGuid(), "3001", "Capital", AccountCategory.Equity, 0m, 120000m, 120000m)
        };

        decimal totalDebit = items.Sum(i => i.DebitAmount);
        decimal totalCredit = items.Sum(i => i.CreditAmount);

        Assert.Equal(200000m, totalDebit);
        Assert.Equal(200000m, totalCredit);
        Assert.Equal(totalDebit, totalCredit);
    }

    [Fact]
    public void BalanceSheet_AssetsMustEqualLiabilitiesPlusEquity()
    {
        decimal currentAssets = 37700000m;
        decimal nonCurrentAssets = 18000000m;
        decimal totalAssets = currentAssets + nonCurrentAssets; // 55,700,000

        decimal currentLiabilities = 10100000m;
        decimal nonCurrentLiabilities = 0m;
        decimal totalLiabilities = currentLiabilities + nonCurrentLiabilities; // 10,100,000

        decimal equityCapital = 45600000m;
        decimal netProfit = 0m;
        decimal totalEquity = equityCapital + netProfit; // 45,600,000

        decimal totalLiabilitiesAndEquity = totalLiabilities + totalEquity; // 55,700,000

        Assert.Equal(totalAssets, totalLiabilitiesAndEquity);
    }


    [Fact]
    public void ThreeWayMatch_PriceAndQuantityMatch_ShouldPassAndApproveForPayment()
    {
        // ACC-02 & AC-06: PO $2.3500, GRN 36,750 Yds, Bill $2.3500 for 36,750 Yds
        var bill = new VendorBill
        {
            BillNumber = "BILL-APEX-2026-091",
            SupplierName = "Apex Textile Mills Ltd.",
            BilledUnitPrice = 2.3500m,
            BilledQuantity = 36750.0m,
            BilledAmount = 86362.50m
        };

        var log = bill.ExecuteThreeWayMatch(poUnitPrice: 2.3500m, grnPassedQuantity: 36750.0m, evaluatedBy: "Audit Engine");

        Assert.Equal(BillMatchStatus.ApprovedForPayment, bill.MatchStatus);
        Assert.True(bill.IsApprovedForPayment);
        Assert.Equal(ThreeWayMatchResult.Passed, log.OverallMatchResult);
        Assert.True(log.IsPriceMatched);
        Assert.True(log.IsQuantityMatched);
        Assert.Equal(0m, log.PriceVariancePct);
        Assert.Equal(0m, log.QuantityVariance);
    }

    [Fact]
    public void ThreeWayMatch_PriceMarkupDetected_ShouldFailAndLockBill()
    {
        // ACC-02: PO $2.4000, Bill $2.6500 -> Price markup
        var bill = new VendorBill
        {
            BillNumber = "BILL-TEX-2026-042",
            BilledUnitPrice = 2.6500m,
            BilledQuantity = 15000.0m,
            BilledAmount = 39750.00m
        };

        var log = bill.ExecuteThreeWayMatch(poUnitPrice: 2.4000m, grnPassedQuantity: 15000.0m, evaluatedBy: "Audit Engine");

        Assert.Equal(BillMatchStatus.MatchFailedLocked, bill.MatchStatus);
        Assert.False(bill.IsApprovedForPayment);
        Assert.Equal(ThreeWayMatchResult.DiscrepancyFlagged, log.OverallMatchResult);
        Assert.False(log.IsPriceMatched);
        Assert.True(log.IsQuantityMatched);
        Assert.True(log.PriceVariancePct > 0m);
        Assert.Contains("Price markup detected", log.DiscrepancyNotes);
    }

    [Fact]
    public void ThreeWayMatch_QuantityInflationDetected_ShouldFailAndLockBill()
    {
        // ACC-02: GRN passed 30,000 Yds, Bill claims 35,000 Yds
        var bill = new VendorBill
        {
            BillNumber = "BILL-EXCESS-001",
            BilledUnitPrice = 2.0000m,
            BilledQuantity = 35000.0m,
            BilledAmount = 70000.00m
        };

        var log = bill.ExecuteThreeWayMatch(poUnitPrice: 2.0000m, grnPassedQuantity: 30000.0m, evaluatedBy: "Audit Engine");

        Assert.Equal(BillMatchStatus.MatchFailedLocked, bill.MatchStatus);
        Assert.False(bill.IsApprovedForPayment);
        Assert.Equal(ThreeWayMatchResult.DiscrepancyFlagged, log.OverallMatchResult);
        Assert.True(log.IsPriceMatched);
        Assert.False(log.IsQuantityMatched);
        Assert.Equal(5000.0m, log.QuantityVariance);
    }

    [Fact]
    public void StatutoryDeductions_TdsAndVds_ShouldBeAccurate()
    {
        // ACC-06: Apex bill $86,362.50 USD -> TDS 3% ($2,590.88), VDS 7.5% ($6,477.19), Net ($77,294.43)
        decimal grossAmount = 86362.50m;
        decimal tds = Math.Round(grossAmount * 0.03m, 2);
        decimal vds = Math.Round(grossAmount * 0.075m, 2);
        decimal netPayable = grossAmount - tds - vds;

        Assert.Equal(2590.88m, tds);
        Assert.Equal(6477.19m, vds);
        Assert.Equal(77294.43m, netPayable);
    }

    [Fact]
    public void MasterLC_AllocationHeadroom_ShouldEnforceSeventyPercentLimit()
    {
        // ACC-03: Export Master LC $180,000.00 USD, 70% max = $126,000.00
        var mlc = new MasterLetterOfCredit
        {
            MLCNumber = "MLC-2026-0891",
            TotalAmount = 180000.00m,
            MaxB2BAllocationPct = 70.0m,
            UtilizedB2BAmount = 0m
        };

        Assert.Equal(126000.00m, mlc.MaxB2BAllocationAmount);
        Assert.Equal(126000.00m, mlc.AvailableB2BAmount);

        // Allocate $86,362.50 for Apex B2B LC
        Assert.True(mlc.CanAllocateB2B(86362.50m));
        mlc.RecordB2BAllocation(86362.50m);
        Assert.Equal(39637.50m, mlc.AvailableB2BAmount);

        // Try allocating $50,000 (exceeds $39,637.50 available)
        Assert.False(mlc.CanAllocateB2B(50000.00m));
        Assert.Throws<InvalidOperationException>(() => mlc.RecordB2BAllocation(50000.00m));
    }

    [Fact]
    public void ForeignBillDiscounting_ShouldAdvanceNinetyPercentMinusCharges()
    {
        // ACC-03: FDBP $180,000 FOB, 90% advance ($162,000), charges $1,875 -> Net $160,125
        decimal fobValue = 180000.00m;
        decimal advancePct = 90.0m;
        decimal advancedAmount = Math.Round(fobValue * (advancePct / 100m), 2);
        decimal bankCharges = 1875.00m;
        decimal netProceeds = advancedAmount - bankCharges;

        Assert.Equal(162000.00m, advancedAmount);
        Assert.Equal(160125.00m, netProceeds);
    }

    [Fact]
    public void OrderCostingVariance_StyleChino_ShouldCalculateAccurateRealizedMargin()
    {
        // ACC-04: Style ST-2026-CHINO-01 (20,000 pcs @ $9.00 = $180,000 FOB)
        // Materials: $86,362.50 | Labor: $43,210.00 | Overheads: $29,900.00 | Total Cost: $159,472.50
        // Net Profit: $20,527.50 (11.40% margin)
        var costing = new OrderCostingVariance
        {
            StyleNumber = "ST-2026-CHINO-01",
            OrderQuantity = 20000,
            BuyerFOBPricePerPc = 9.0000m,
            BudgetRawMaterialsCost = 88500.00m,
            ActualRawMaterialsCost = 86362.50m,
            BudgetDirectLaborCost = 44000.00m,
            ActualDirectLaborCost = 43210.00m,
            BudgetOverheadsCost = 30000.00m,
            ActualOverheadsCost = 29900.00m
        };

        Assert.Equal(180000.00m, costing.BuyerFOBRevenue);
        Assert.Equal(162500.00m, costing.TotalBudgetCost);
        Assert.Equal(159472.50m, costing.TotalActualCost);
        Assert.Equal(3027.50m, costing.TotalCostVariance); // Favorable cost savings
        Assert.Equal(20527.50m, costing.NetRealizedProfit);
        Assert.Equal(11.40m, costing.NetProfitMarginPct);
    }

    [Fact]
    public void ForexGainLoss_ShouldComputeGainOnFavorableSettlementRate()
    {
        // ACC-05: Booked $180,000 at 117.50 BDT, Settled at 118.20 BDT
        decimal foreignAmount = 180000.00m;
        decimal bookingRate = 117.50m;
        decimal settlementRate = 118.20m;

        decimal forexGain = CurrencyExchangeRate.CalculateForexGainLoss(foreignAmount, bookingRate, settlementRate);

        Assert.Equal(126000.00m, forexGain); // +126,000 BDT Gain
    }

    [Fact]
    public void FixedAsset_StraightLineDepreciation_ShouldBeAccurate()
    {
        // ACC-06: Juki Machine cost $1,450, Salvage $150, 5 Years life
        var asset = new FixedAsset
        {
            AssetCode = "FA-SEW-001",
            OriginalCost = 1450.00m,
            SalvageValue = 150.00m,
            UsefulLifeYears = 5,
            AccumulatedDepreciation = 0m
        };

        Assert.Equal(1300.00m, asset.DepreciableBase);
        Assert.Equal(260.00m, asset.AnnualDepreciation);
        Assert.Equal(21.67m, asset.MonthlyDepreciation);

        asset.ApplyMonthlyDepreciation();
        Assert.Equal(21.67m, asset.AccumulatedDepreciation);
        Assert.Equal(1428.33m, asset.CurrentBookValue);
    }
}

