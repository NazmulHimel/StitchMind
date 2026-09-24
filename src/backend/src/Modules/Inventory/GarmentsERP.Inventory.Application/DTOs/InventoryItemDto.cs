using GarmentsERP.Inventory.Domain.Enums;

namespace GarmentsERP.Inventory.Application.DTOs;

public record InventoryItemDto(
    Guid Id,
    string ItemCode,
    string Name,
    string Description,
    ItemCategory Category,
    string CategoryName,
    StockUOM UnitOfMeasure,
    string UOMName,
    decimal CurrentStock,
    decimal SafetyStock,
    decimal ReorderPoint,
    decimal StandardCost,
    decimal TotalValue,
    string BinLocation,
    bool IsLowStock,
    bool IsActive,
    Guid CompanyId,
    DateTime CreatedAt
);

public record CreateInventoryItemRequest(
    string ItemCode,
    string Name,
    string Description,
    ItemCategory Category,
    StockUOM UnitOfMeasure,
    decimal InitialStock,
    decimal SafetyStock,
    decimal ReorderPoint,
    decimal StandardCost,
    string BinLocation
);

public record AdjustStockRequest(
    Guid ItemId,
    decimal AdjustedStockQuantity,
    string Reason,
    string Location
);
