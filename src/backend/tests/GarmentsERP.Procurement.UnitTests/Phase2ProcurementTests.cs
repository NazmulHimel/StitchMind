using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;
using Xunit;

namespace GarmentsERP.Procurement.UnitTests;

public class Phase2ProcurementTests
{
    [Fact]
    public void PurchaseOrder_WhenItemExceedsBudgetRate_ShouldTriggerHardBudgetLock_AC03()
    {
        // Arrange: Pre-costing budget cap is $2.40/yd for Cotton Twill
        var po = PurchaseOrder.Create("PO-2026-TEST-01", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(14));

        // Act: Vendor quoted $2.45/yd
        po.AddItem("FAB-TW-01", "Cotton Spandex Twill", MaterialCategory.Fabric, UOM.Yards, 10000, unitPrice: 2.45m, budgetUnitPrice: 2.40m);

        // Assert: Hard lock engaged automatically
        Assert.True(po.IsBudgetLocked);
        Assert.Equal(POStatus.LockedBudgetExceeded, po.Status);
    }

    [Fact]
    public void PurchaseOrder_WhenBudgetLocked_SubmitAndApproveShouldThrow_AC03()
    {
        // Arrange
        var po = PurchaseOrder.Create("PO-2026-TEST-02", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(14));
        po.AddItem("FAB-TW-01", "Cotton Spandex Twill", MaterialCategory.Fabric, UOM.Yards, 10000, unitPrice: 2.45m, budgetUnitPrice: 2.40m);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => po.Submit());
        Assert.Throws<InvalidOperationException>(() => po.Approve(Guid.NewGuid()));
    }

    [Fact]
    public void PurchaseOrder_CFOAuthorizeBudgetOverride_ShouldUnlockAndAllowWorkflowTransition()
    {
        // Arrange
        var cfoUserId = Guid.NewGuid();
        var po = PurchaseOrder.Create("PO-2026-TEST-03", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(14));
        po.AddItem("FAB-TW-01", "Cotton Spandex Twill", MaterialCategory.Fabric, UOM.Yards, 10000, unitPrice: 2.45m, budgetUnitPrice: 2.40m);
        Assert.True(po.IsBudgetLocked);

        // Act: CFO authorizes override with formal justification
        po.AuthorizeBudgetOverride(cfoUserId, "Urgent production requirement approved due to global raw cotton price surge");

        // Assert
        Assert.False(po.IsBudgetLocked);
        Assert.Equal(POStatus.Submitted, po.Status);
        Assert.Equal(cfoUserId, po.BudgetOverrideBy);
        Assert.NotNull(po.BudgetOverrideAt);

        // Now approval can proceed
        po.Approve(Guid.NewGuid());
        Assert.Equal(POStatus.Approved, po.Status);
    }

    [Fact]
    public void CommercialLC_StatutoryB2BCeiling_ShouldEnforceSeventyPercentLimit()
    {
        // Arrange: Master LC = $180,000 USD
        decimal masterLCAmount = 180000.00m;

        var b2bValid = new CommercialLC
        {
            LCNumber = "B2B-2026-0098",
            LCType = LCType.BackToBackLC,
            LCAmount = 86362.50m // 47.98%
        };

        var b2bExcessive = new CommercialLC
        {
            LCNumber = "B2B-2026-EXCESS",
            LCType = LCType.BackToBackLC,
            LCAmount = 135000.00m // 75.00%
        };

        // Act & Assert
        Assert.True(b2bValid.ValidateStatutoryB2BCeiling(masterLCAmount));
        Assert.True(b2bValid.CalculateCeilingPercentage(masterLCAmount) <= 70.00m);

        Assert.False(b2bExcessive.ValidateStatutoryB2BCeiling(masterLCAmount));
        Assert.True(b2bExcessive.CalculateCeilingPercentage(masterLCAmount) > 70.00m);
    }

    [Fact]
    public void ComparativeStatement_AwardContract_ShouldSelectVendorAndSetStatus()
    {
        // Arrange
        var cs = new ComparativeStatement
        {
            CSNumber = "CS-2026-0042",
            ItemCode = "FAB-TW-01",
            PreCostingBudgetRate = 2.40m
        };

        var supApex = Guid.NewGuid();
        var supSquare = Guid.NewGuid();

        cs.AddSupplierBid(supApex, "Apex Textile Mills", 2.35m, 21, "90 Days B2B LC", 4.85m);
        cs.AddSupplierBid(supSquare, "Square Fashions", 2.45m, 14, "60 Days B2B LC", 4.90m);

        // Act
        cs.AwardContract(supApex, "Best rate under budget");

        // Assert
        Assert.Equal(CSStatus.Awarded, cs.Status);
        Assert.Equal(supApex, cs.AwardedSupplierId);
        Assert.Equal("Apex Textile Mills", cs.AwardedSupplierName);
        Assert.Equal(2.35m, cs.AwardedRate);
        Assert.True(cs.Lines.First(l => l.SupplierId == supApex).IsSelected);
        Assert.False(cs.Lines.First(l => l.SupplierId == supSquare).IsSelected);
    }
}
