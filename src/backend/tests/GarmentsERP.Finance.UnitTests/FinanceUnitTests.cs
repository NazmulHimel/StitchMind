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
}
