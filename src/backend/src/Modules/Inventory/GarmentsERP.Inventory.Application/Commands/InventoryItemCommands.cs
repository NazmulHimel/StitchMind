using FluentValidation;
using GarmentsERP.Inventory.Application.DTOs;
using GarmentsERP.Inventory.Application.Interfaces;
using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Inventory.Application.Commands;

public record CreateInventoryItemCommand(
    string ItemCode,
    string Name,
    string Description,
    ItemCategory Category,
    StockUOM UnitOfMeasure,
    decimal InitialStock,
    decimal SafetyStock,
    decimal ReorderPoint,
    decimal StandardCost,
    string BinLocation,
    Guid CompanyId
) : IRequest<Result<InventoryItemDto>>;

public class CreateInventoryItemCommandValidator : AbstractValidator<CreateInventoryItemCommand>
{
    public CreateInventoryItemCommandValidator()
    {
        RuleFor(x => x.ItemCode).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.StandardCost).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SafetyStock).GreaterThanOrEqualTo(0);
        RuleFor(x => x.ReorderPoint).GreaterThanOrEqualTo(0);
    }
}

public class CreateInventoryItemCommandHandler(IInventoryRepository repository)
    : IRequestHandler<CreateInventoryItemCommand, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(CreateInventoryItemCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetItemByCodeAsync(request.ItemCode, cancellationToken);
        if (existing != null)
        {
            return Result<InventoryItemDto>.Failure($"Inventory item with code '{request.ItemCode}' already exists.");
        }

        var item = InventoryItem.Create(
            request.ItemCode,
            request.Name,
            request.Description,
            request.Category,
            request.UnitOfMeasure,
            request.InitialStock,
            request.SafetyStock,
            request.ReorderPoint,
            request.StandardCost,
            request.BinLocation,
            request.CompanyId
        );

        await repository.AddItemAsync(item, cancellationToken);

        if (request.InitialStock > 0)
        {
            var txNumber = $"TX-INIT-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";
            var tx = StockTransaction.Create(
                txNumber,
                item.Id,
                StockTransactionType.Receipt,
                request.InitialStock,
                request.StandardCost,
                "INITIAL-STOCK",
                item.BinLocation,
                Guid.Empty,
                request.CompanyId,
                notes: "Initial balance entry"
            );
            await repository.AddTransactionAsync(tx, cancellationToken);
        }

        await repository.SaveChangesAsync(cancellationToken);

        var dto = new InventoryItemDto(
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

        return Result<InventoryItemDto>.Success(dto);
    }
}

public record AdjustStockCommand(
    Guid ItemId,
    decimal AdjustedStockQuantity,
    string Reason,
    string Location,
    Guid PerformedBy,
    Guid CompanyId
) : IRequest<Result<InventoryItemDto>>;

public class AdjustStockCommandHandler(IInventoryRepository repository)
    : IRequestHandler<AdjustStockCommand, Result<InventoryItemDto>>
{
    public async Task<Result<InventoryItemDto>> Handle(AdjustStockCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetItemByIdAsync(request.ItemId, cancellationToken);
        if (item == null) return Result<InventoryItemDto>.Failure("Inventory item not found.");

        var diff = request.AdjustedStockQuantity - item.CurrentStock;
        item.AdjustStock(request.AdjustedStockQuantity);

        var txNumber = $"TX-ADJ-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";
        var tx = StockTransaction.Create(
            txNumber,
            item.Id,
            StockTransactionType.Adjustment,
            Math.Abs(diff),
            item.StandardCost,
            diff < 0 ? item.BinLocation : "ADJUSTMENT-SURPLUS",
            diff < 0 ? "ADJUSTMENT-LOSS" : item.BinLocation,
            request.PerformedBy,
            request.CompanyId,
            notes: $"Stock adjustment: {request.Reason}. Difference: {diff:+#,##0.00;-#,##0.00;0}"
        );

        await repository.AddTransactionAsync(tx, cancellationToken);
        await repository.UpdateItemAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var dto = new InventoryItemDto(
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

        return Result<InventoryItemDto>.Success(dto);
    }
}
