using GarmentsERP.Procurement.Domain.Enums;

namespace GarmentsERP.Procurement.Application.DTOs;

public record PurchaseOrderItemDto(
    Guid Id,
    Guid PurchaseOrderId,
    string ItemCode,
    string Description,
    MaterialCategory Category,
    string CategoryName,
    UOM UnitOfMeasure,
    string UOMName,
    decimal Quantity,
    decimal UnitPrice,
    decimal TotalPrice,
    decimal ReceivedQuantity
);

public record PurchaseOrderDto(
    Guid Id,
    string PONumber,
    Guid SupplierId,
    string SupplierName,
    Guid CompanyId,
    DateTime OrderDate,
    DateTime ExpectedDeliveryDate,
    POStatus Status,
    string StatusName,
    decimal TotalAmount,
    string? Notes,
    Guid? ApprovedBy,
    DateTime? ApprovedAt,
    IReadOnlyList<PurchaseOrderItemDto> Items,
    DateTime CreatedAt
);

public record CreatePurchaseOrderItemRequest(
    string ItemCode,
    string Description,
    MaterialCategory Category,
    UOM UnitOfMeasure,
    decimal Quantity,
    decimal UnitPrice
);

public record CreatePurchaseOrderRequest(
    Guid SupplierId,
    DateTime ExpectedDeliveryDate,
    string? Notes,
    List<CreatePurchaseOrderItemRequest> Items
);
