using GarmentsERP.Inventory.Domain.Enums;

namespace GarmentsERP.Inventory.Application.DTOs;

public record StockTransactionDto(
    Guid Id,
    string TransactionNumber,
    Guid InventoryItemId,
    string ItemCode,
    string ItemName,
    StockTransactionType TransactionType,
    string TransactionTypeName,
    decimal Quantity,
    decimal UnitCost,
    decimal TotalCost,
    string? ReferenceNumber,
    string SourceLocation,
    string DestinationLocation,
    Guid PerformedBy,
    string? Notes,
    DateTime CreatedAt
);

public record ThreadStageLogDto(
    Guid Id,
    Guid ThreadBatchId,
    ThreadStage FromStage,
    string FromStageName,
    ThreadStage ToStage,
    string ToStageName,
    decimal InputQuantityKg,
    decimal OutputQuantityKg,
    decimal WasteQuantityKg,
    string MachineId,
    string OperatorName,
    string? Remarks,
    DateTime CreatedAt
);

public record ThreadBatchDto(
    Guid Id,
    string BatchNumber,
    string YarnCount,
    string FiberComposition,
    string ColorCode,
    string ColorName,
    decimal TargetQuantityKg,
    decimal CurrentQuantityKg,
    decimal TotalWastageKg,
    ThreadStage CurrentStage,
    string CurrentStageName,
    ThreadBatchStatus Status,
    string StatusName,
    DateTime StartDate,
    DateTime? CompletedDate,
    Guid? SupervisorId,
    string? Notes,
    IReadOnlyList<ThreadStageLogDto> StageLogs,
    DateTime CreatedAt
);

public record CreateThreadBatchRequest(
    string YarnCount,
    string FiberComposition,
    string ColorCode,
    string ColorName,
    decimal TargetQuantityKg,
    string? Notes = null
);

public record AdvanceThreadStageRequest(
    ThreadStage NextStage,
    decimal OutputQuantityKg,
    decimal WasteQuantityKg,
    string MachineId,
    string OperatorName,
    string? Remarks = null
);

public record InventoryMetricsDto(
    int TotalSKUs,
    decimal TotalStockValue,
    int LowStockItems,
    int ActiveBatches
);
