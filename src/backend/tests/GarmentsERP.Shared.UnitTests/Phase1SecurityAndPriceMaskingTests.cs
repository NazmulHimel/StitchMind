using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Constants;
using GarmentsERP.Shared.Domain;
using GarmentsERP.Shared.Extensions;
using Xunit;

namespace GarmentsERP.Shared.UnitTests;

public class Phase1SecurityAndPriceMaskingTests
{
    [Fact]
    public void PriceMaskingExtensions_WhenUserCannotViewFinancials_ShouldMaskNumbersAndStrings()
    {
        // Arrange
        decimal sensitiveRate = 145.50m;
        decimal? sensitiveBudget = 25000.00m;
        string currency = "$";

        // Act
        decimal? maskedRate = sensitiveRate.MaskFinancialCost(canViewFinancialCosts: false);
        decimal? maskedBudget = sensitiveBudget.MaskFinancialCost(canViewFinancialCosts: false);
        string maskedDisplay = sensitiveRate.MaskFinancialFormatted(canViewFinancialCosts: false, currency);

        // Assert
        Assert.Null(maskedRate);
        Assert.Null(maskedBudget);
        Assert.Equal("[RESTRICTED]", maskedDisplay);
    }

    [Fact]
    public void PriceMaskingExtensions_WhenUserCanViewFinancials_ShouldPreserveFinancialValues()
    {
        // Arrange
        decimal sensitiveRate = 145.50m;
        string currency = "$";

        // Act
        decimal? unmaskedRate = sensitiveRate.MaskFinancialCost(canViewFinancialCosts: true);
        string unmaskedDisplay = sensitiveRate.MaskFinancialFormatted(canViewFinancialCosts: true, currency);

        // Assert
        Assert.Equal(145.50m, unmaskedRate);
        Assert.Equal("$145.50", unmaskedDisplay);
    }

    [Fact]
    public async Task InMemoryAuditTrailStore_ShouldRecordAndRetrieveAuditTrailLogs()
    {
        // Arrange
        var store = new InMemoryAuditTrailStore();
        var log = AuditTrailLog.Create(
            entityName: "PreCosting",
            entityId: Guid.NewGuid().ToString(),
            actionType: "APPROVE",
            userId: Guid.NewGuid(),
            userName: "cfo@stitchmind.com",
            userIpAddress: "192.168.1.10",
            oldValuesJson: "{\"IsBudgetApproved\":false}",
            newValuesJson: "{\"IsBudgetApproved\":true,\"TotalMaterialsBudget\":52800.00}"
        );

        // Act
        await store.RecordAuditLogAsync(log);
        var recentLogs = await store.GetAuditLogsAsync(take: 10);

        // Assert
        Assert.NotEmpty(recentLogs);
        var retrieved = recentLogs.First(l => l.EntityId == log.EntityId);
        Assert.Equal("PreCosting", retrieved.EntityName);
        Assert.Equal("APPROVE", retrieved.ActionType);
        Assert.Equal("cfo@stitchmind.com", retrieved.UserName);
        Assert.Contains("52800.00", retrieved.NewValuesJson);
    }

    [Fact]
    public void AppConstants_Actions_ShouldContainStandardSixRbacActions()
    {
        // Assert SEC-02 6-action matrix
        Assert.Contains(AppConstants.Actions.Create, AppConstants.Actions.All);
        Assert.Contains(AppConstants.Actions.Read, AppConstants.Actions.All);
        Assert.Contains(AppConstants.Actions.Update, AppConstants.Actions.All);
        Assert.Contains(AppConstants.Actions.Delete, AppConstants.Actions.All);
        Assert.Contains(AppConstants.Actions.Approve, AppConstants.Actions.All);
        Assert.Contains(AppConstants.Actions.Export, AppConstants.Actions.All);
        Assert.Equal(6, AppConstants.Actions.All.Length);
    }
}
