using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;
using Xunit;

namespace GarmentsERP.Procurement.UnitTests;

public class PurchaseOrderTests
{
    [Fact]
    public void Create_ShouldInitialize_InDraftStatusWithZeroTotal()
    {
        var supplierId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var expectedDelivery = DateTime.UtcNow.AddDays(10);

        var po = PurchaseOrder.Create("PO-2026-001", supplierId, companyId, expectedDelivery, "Sample order");

        Assert.Equal("PO-2026-001", po.PONumber);
        Assert.Equal(POStatus.Draft, po.Status);
        Assert.Equal(0, po.TotalAmount);
        Assert.Empty(po.Items);
    }

    [Fact]
    public void AddItem_ShouldCalculateTotalAmountCorrectly()
    {
        var po = PurchaseOrder.Create("PO-2026-002", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));

        po.AddItem("FAB-01", "Denim 12oz", MaterialCategory.Fabric, UOM.Yards, 1000, 4.00m);
        po.AddItem("THD-01", "Poly Thread 40/2", MaterialCategory.Thread, UOM.Cones, 50, 2.50m);

        // 1000 * 4.00 = 4000; 50 * 2.50 = 125; Total = 4125
        Assert.Equal(2, po.Items.Count);
        Assert.Equal(4125m, po.TotalAmount);
    }

    [Fact]
    public void Submit_And_Approve_ShouldTransitionStatus()
    {
        var po = PurchaseOrder.Create("PO-2026-003", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        var approverId = Guid.NewGuid();

        po.Submit();
        Assert.Equal(POStatus.Submitted, po.Status);

        po.Approve(approverId);
        Assert.Equal(POStatus.Approved, po.Status);
        Assert.Equal(approverId, po.ApprovedBy);
        Assert.NotNull(po.ApprovedAt);
    }

    [Fact]
    public void ReceivingStatus_ShouldUpdateToPartiallyReceived_And_Completed()
    {
        var po = PurchaseOrder.Create("PO-2026-004", Guid.NewGuid(), Guid.NewGuid(), DateTime.UtcNow.AddDays(7));
        po.AddItem("BTN-01", "Brass Button", MaterialCategory.Trims, UOM.Pieces, 1000, 0.10m);
        po.Submit();
        po.Approve(Guid.NewGuid());

        var item = po.Items.First();
        item.RecordReceived(400);
        po.UpdateReceivingStatus();
        Assert.Equal(POStatus.PartiallyReceived, po.Status);

        item.RecordReceived(600);
        po.UpdateReceivingStatus();
        Assert.Equal(POStatus.Completed, po.Status);
    }
}

public class GoodsReceiptNoteTests
{
    [Fact]
    public void CreateGRN_WithInspection_ShouldRecordQuantities()
    {
        var poId = Guid.NewGuid();
        var supplierId = Guid.NewGuid();
        var companyId = Guid.NewGuid();
        var poItemId = Guid.NewGuid();

        var grn = GoodsReceiptNote.Create("GRN-2026-001", poId, supplierId, companyId, "CH-8891", "TRUCK-01");
        grn.AddItem(poItemId, "FAB-01", "Indigo Denim", 500, 480, 20, "20 yds oil stain");
        grn.CompleteInspection();

        Assert.Equal(GRNStatus.Accepted, grn.Status);
        Assert.Single(grn.Items);
        Assert.Equal(480m, grn.Items.First().AcceptedQuantity);
        Assert.Equal(20m, grn.Items.First().RejectedQuantity);
    }
}
