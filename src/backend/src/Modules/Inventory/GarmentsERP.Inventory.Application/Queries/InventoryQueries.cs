using GarmentsERP.Inventory.Application.DTOs;
using GarmentsERP.Inventory.Application.Interfaces;
using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Inventory.Application.Queries;

// 1. Inventory Items Query
public record GetInventoryItemsQuery(
    Guid CompanyId,
    ItemCategory? Category = null,
    string? Search = null,
    bool LowStockOnly = false
) : IRequest<Result<IReadOnlyList<InventoryItemDto>>>;

public record GetInventoryItemByIdQuery(Guid Id) : IRequest<Result<InventoryItemDto>>;

public class InventoryItemQueryHandlers(IInventoryRepository repository)
    : IRequestHandler<GetInventoryItemsQuery, Result<IReadOnlyList<InventoryItemDto>>>,
      IRequestHandler<GetInventoryItemByIdQuery, Result<InventoryItemDto>>
{
    public async Task<Result<IReadOnlyList<InventoryItemDto>>> Handle(GetInventoryItemsQuery request, CancellationToken cancellationToken)
    {
        var items = await repository.GetItemsAsync(
            request.CompanyId,
            request.Category,
            request.Search,
            request.LowStockOnly,
            cancellationToken
        );

        var dtos = items.Select(i => MapToDto(i)).ToList();
        return Result<IReadOnlyList<InventoryItemDto>>.Success(dtos);
    }

    public async Task<Result<InventoryItemDto>> Handle(GetInventoryItemByIdQuery request, CancellationToken cancellationToken)
    {
        var item = await repository.GetItemByIdAsync(request.Id, cancellationToken);
        if (item == null) return Result<InventoryItemDto>.Failure("Item not found.");

        return Result<InventoryItemDto>.Success(MapToDto(item));
    }

    private static InventoryItemDto MapToDto(InventoryItem item)
    {
        return new InventoryItemDto(
            item.Id,
            item.ItemCode,
            item.Name,
            item.Description,
            item.Category,
            item.Category.ToString(),
            item.UnitOfMeasure,
            item.UnitOfMeasure.ToString(),
            item.CurrentStock,
            item.SafetyStock,
            item.ReorderPoint,
            item.StandardCost,
            item.CurrentStock * item.StandardCost,
            item.BinLocation,
            item.CurrentStock <= item.ReorderPoint,
            item.IsActive,
            item.CompanyId,
            item.CreatedAt
        );
    }
}

// 2. Fabric Rolls Query
public record GetFabricRollsQuery(
    Guid CompanyId,
    Guid? ItemId = null,
    string? DyeLot = null,
    RollStatus? Status = null,
    FabricGrade? Grade = null
) : IRequest<Result<IReadOnlyList<FabricRollDto>>>;

public record GetFabricRollByIdQuery(Guid Id) : IRequest<Result<FabricRollDto>>;

public class FabricRollQueryHandlers(IInventoryRepository repository)
    : IRequestHandler<GetFabricRollsQuery, Result<IReadOnlyList<FabricRollDto>>>,
      IRequestHandler<GetFabricRollByIdQuery, Result<FabricRollDto>>
{
    public async Task<Result<IReadOnlyList<FabricRollDto>>> Handle(GetFabricRollsQuery request, CancellationToken cancellationToken)
    {
        var rolls = await repository.GetFabricRollsAsync(
            request.CompanyId,
            request.ItemId,
            request.DyeLot,
            request.Status,
            request.Grade,
            cancellationToken
        );

        var dtos = rolls.Select(r => new FabricRollDto(
            r.Id,
            r.RollNumber,
            r.InventoryItemId,
            r.InventoryItem?.ItemCode ?? "FABRIC",
            r.InventoryItem?.Name ?? "Fabric",
            r.DyeLotNumber,
            r.GSM,
            r.WidthInches,
            r.TotalLengthYards,
            r.AvailableLengthYards,
            r.Grade,
            r.Grade.ToString(),
            r.Status,
            r.Status.ToString(),
            r.RackLocation,
            r.SupplierRollNo,
            r.GoodsReceiptNoteId,
            r.CompanyId,
            r.CreatedAt
        )).ToList();

        return Result<IReadOnlyList<FabricRollDto>>.Success(dtos);
    }

    public async Task<Result<FabricRollDto>> Handle(GetFabricRollByIdQuery request, CancellationToken cancellationToken)
    {
        var r = await repository.GetFabricRollByIdAsync(request.Id, cancellationToken);
        if (r == null) return Result<FabricRollDto>.Failure("Fabric roll not found.");

        var dto = new FabricRollDto(
            r.Id,
            r.RollNumber,
            r.InventoryItemId,
            r.InventoryItem?.ItemCode ?? "FABRIC",
            r.InventoryItem?.Name ?? "Fabric",
            r.DyeLotNumber,
            r.GSM,
            r.WidthInches,
            r.TotalLengthYards,
            r.AvailableLengthYards,
            r.Grade,
            r.Grade.ToString(),
            r.Status,
            r.Status.ToString(),
            r.RackLocation,
            r.SupplierRollNo,
            r.GoodsReceiptNoteId,
            r.CompanyId,
            r.CreatedAt
        );

        return Result<FabricRollDto>.Success(dto);
    }
}

// 3. Thread Batches Query
public record GetThreadBatchesQuery(
    Guid CompanyId,
    ThreadStage? Stage = null,
    ThreadBatchStatus? Status = null
) : IRequest<Result<IReadOnlyList<ThreadBatchDto>>>;

public record GetThreadBatchByIdQuery(Guid Id) : IRequest<Result<ThreadBatchDto>>;

public class ThreadBatchQueryHandlers(IInventoryRepository repository)
    : IRequestHandler<GetThreadBatchesQuery, Result<IReadOnlyList<ThreadBatchDto>>>,
      IRequestHandler<GetThreadBatchByIdQuery, Result<ThreadBatchDto>>
{
    public async Task<Result<IReadOnlyList<ThreadBatchDto>>> Handle(GetThreadBatchesQuery request, CancellationToken cancellationToken)
    {
        var batches = await repository.GetThreadBatchesAsync(
            request.CompanyId,
            request.Stage,
            request.Status,
            cancellationToken
        );

        var dtos = batches.Select(b => MapToDto(b)).ToList();
        return Result<IReadOnlyList<ThreadBatchDto>>.Success(dtos);
    }

    public async Task<Result<ThreadBatchDto>> Handle(GetThreadBatchByIdQuery request, CancellationToken cancellationToken)
    {
        var b = await repository.GetThreadBatchByIdAsync(request.Id, cancellationToken);
        if (b == null) return Result<ThreadBatchDto>.Failure("Thread batch not found.");

        return Result<ThreadBatchDto>.Success(MapToDto(b));
    }

    private static ThreadBatchDto MapToDto(ThreadBatch b)
    {
        var logs = b.StageLogs.Select(l => new ThreadStageLogDto(
            l.Id,
            l.ThreadBatchId,
            l.FromStage,
            l.FromStage.ToString(),
            l.ToStage,
            l.ToStage.ToString(),
            l.InputQuantityKg,
            l.OutputQuantityKg,
            l.WasteQuantityKg,
            l.MachineId,
            l.OperatorName,
            l.Remarks,
            l.CreatedAt
        )).ToList();

        return new ThreadBatchDto(
            b.Id,
            b.BatchNumber,
            b.YarnCount,
            b.FiberComposition,
            b.ColorCode,
            b.ColorName,
            b.TargetQuantityKg,
            b.CurrentQuantityKg,
            b.TotalWastageKg,
            b.CurrentStage,
            b.CurrentStage.ToString(),
            b.Status,
            b.Status.ToString(),
            b.StartDate,
            b.CompletedDate,
            b.SupervisorId,
            b.Notes,
            logs,
            b.CreatedAt
        );
    }
}

// 4. Stock Transactions Query
public record GetStockTransactionsQuery(
    Guid CompanyId,
    Guid? ItemId = null,
    StockTransactionType? Type = null,
    int Take = 50
) : IRequest<Result<IReadOnlyList<StockTransactionDto>>>;

public class StockTransactionQueryHandler(IInventoryRepository repository)
    : IRequestHandler<GetStockTransactionsQuery, Result<IReadOnlyList<StockTransactionDto>>>
{
    public async Task<Result<IReadOnlyList<StockTransactionDto>>> Handle(GetStockTransactionsQuery request, CancellationToken cancellationToken)
    {
        var txs = await repository.GetTransactionsAsync(
            request.CompanyId,
            request.ItemId,
            request.Type,
            request.Take,
            cancellationToken
        );

        var dtos = txs.Select(t => new StockTransactionDto(
            t.Id,
            t.TransactionNumber,
            t.InventoryItemId,
            t.InventoryItem?.ItemCode ?? "N/A",
            t.InventoryItem?.Name ?? "Item",
            t.TransactionType,
            t.TransactionType.ToString(),
            t.Quantity,
            t.UnitCost,
            t.TotalCost,
            t.ReferenceNumber,
            t.SourceLocation,
            t.DestinationLocation,
            t.PerformedBy,
            t.Notes,
            t.CreatedAt
        )).ToList();

        return Result<IReadOnlyList<StockTransactionDto>>.Success(dtos);
    }
}

// 5. Inventory Metrics Query
public record GetInventoryMetricsQuery(Guid CompanyId) : IRequest<Result<InventoryMetricsDto>>;

public class InventoryMetricsQueryHandler(IInventoryRepository repository)
    : IRequestHandler<GetInventoryMetricsQuery, Result<InventoryMetricsDto>>
{
    public async Task<Result<InventoryMetricsDto>> Handle(GetInventoryMetricsQuery request, CancellationToken cancellationToken)
    {
        var (totalSKUs, totalValue, lowStock, activeBatches) =
            await repository.GetInventoryMetricsAsync(request.CompanyId, cancellationToken);

        var dto = new InventoryMetricsDto(totalSKUs, totalValue, lowStock, activeBatches);
        return Result<InventoryMetricsDto>.Success(dto);
    }
}
