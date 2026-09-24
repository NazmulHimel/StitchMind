using GarmentsERP.Procurement.Domain.Enums;

namespace GarmentsERP.Procurement.Application.DTOs;

public record GoodsReceiptNoteItemDto(
    Guid Id,
    Guid GoodsReceiptNoteId,
    Guid PurchaseOrderItemId,
    string ItemCode,
    string Description,
    decimal ReceivedQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity,
    string? RejectionReason
);

public record GoodsReceiptNoteDto(
    Guid Id,
    string GRNNumber,
    Guid PurchaseOrderId,
    string PONumber,
    Guid SupplierId,
    string SupplierName,
    Guid CompanyId,
    DateTime ReceivedDate,
    string DeliveryChallanNo,
    string? VehicleNo,
    Guid? InspectedBy,
    GRNStatus Status,
    string StatusName,
    string? Notes,
    IReadOnlyList<GoodsReceiptNoteItemDto> Items,
    DateTime CreatedAt
);

public record CreateGRNItemRequest(
    Guid PurchaseOrderItemId,
    string ItemCode,
    string Description,
    decimal ReceivedQuantity,
    decimal AcceptedQuantity,
    decimal RejectedQuantity,
    string? RejectionReason
);

public record CreateGoodsReceiptNoteRequest(
    Guid PurchaseOrderId,
    string DeliveryChallanNo,
    string? VehicleNo,
    string? Notes,
    List<CreateGRNItemRequest> Items
);
