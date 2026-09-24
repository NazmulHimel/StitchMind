using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Procurement.Domain.Entities;

public class GoodsReceiptNote : BaseEntity
{
    public string GRNNumber { get; private set; } = string.Empty;
    public Guid PurchaseOrderId { get; private set; }
    public Guid SupplierId { get; private set; }
    public Guid CompanyId { get; private set; }
    public DateTime ReceivedDate { get; private set; } = DateTime.UtcNow;
    public string DeliveryChallanNo { get; private set; } = string.Empty;
    public string? VehicleNo { get; private set; }
    public Guid? InspectedBy { get; private set; }
    public GRNStatus Status { get; private set; } = GRNStatus.Draft;
    public string? Notes { get; private set; }

    public PurchaseOrder PurchaseOrder { get; private set; } = null!;
    public Supplier Supplier { get; private set; } = null!;
    public ICollection<GoodsReceiptNoteItem> Items { get; private set; } = [];

    private GoodsReceiptNote() { }

    public static GoodsReceiptNote Create(string grnNumber, Guid poId, Guid supplierId,
        Guid companyId, string challanNo, string? vehicleNo = null, string? notes = null)
    {
        return new GoodsReceiptNote
        {
            GRNNumber = grnNumber.Trim().ToUpper(),
            PurchaseOrderId = poId,
            SupplierId = supplierId,
            CompanyId = companyId,
            DeliveryChallanNo = challanNo.Trim(),
            VehicleNo = vehicleNo?.Trim(),
            Notes = notes,
            Status = GRNStatus.Draft
        };
    }

    public void AddItem(Guid poItemId, string itemCode, string description,
        decimal receivedQty, decimal acceptedQty, decimal rejectedQty, string? rejectionReason = null)
    {
        var item = GoodsReceiptNoteItem.Create(Id, poItemId, itemCode, description, receivedQty, acceptedQty, rejectedQty, rejectionReason);
        Items.Add(item);
    }

    public void MarkUnderInspection(Guid inspectedByUserId)
    {
        InspectedBy = inspectedByUserId;
        Status = GRNStatus.UnderInspection;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteInspection()
    {
        var hasRejections = Items.Any(i => i.RejectedQuantity > 0);
        Status = hasRejections ? GRNStatus.Accepted : GRNStatus.Accepted; // Accepted with notes
        UpdatedAt = DateTime.UtcNow;
    }
}

public class GoodsReceiptNoteItem : BaseEntity
{
    public Guid GoodsReceiptNoteId { get; private set; }
    public Guid PurchaseOrderItemId { get; private set; }
    public string ItemCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal ReceivedQuantity { get; private set; }
    public decimal AcceptedQuantity { get; private set; }
    public decimal RejectedQuantity { get; private set; }
    public string? RejectionReason { get; private set; }

    public GoodsReceiptNote GoodsReceiptNote { get; private set; } = null!;

    private GoodsReceiptNoteItem() { }

    public static GoodsReceiptNoteItem Create(Guid grnId, Guid poItemId, string itemCode,
        string description, decimal receivedQty, decimal acceptedQty, decimal rejectedQty, string? reason = null)
    {
        return new GoodsReceiptNoteItem
        {
            GoodsReceiptNoteId = grnId,
            PurchaseOrderItemId = poItemId,
            ItemCode = itemCode.Trim().ToUpper(),
            Description = description.Trim(),
            ReceivedQuantity = receivedQty,
            AcceptedQuantity = acceptedQty,
            RejectedQuantity = rejectedQty,
            RejectionReason = reason
        };
    }
}
