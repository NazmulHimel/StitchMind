using FluentValidation;
using GarmentsERP.Inventory.Application.DTOs;
using GarmentsERP.Inventory.Application.Interfaces;
using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Inventory.Application.Commands;

public record CreateFabricRollCommand(
    Guid InventoryItemId,
    string DyeLotNumber,
    decimal GSM,
    decimal WidthInches,
    decimal TotalLengthYards,
    string RackLocation,
    Guid CompanyId,
    string? SupplierRollNo = null,
    Guid? GoodsReceiptNoteId = null,
    FabricGrade Grade = FabricGrade.GradeA
) : IRequest<Result<FabricRollDto>>;

public class CreateFabricRollCommandValidator : AbstractValidator<CreateFabricRollCommand>
{
    public CreateFabricRollCommandValidator()
    {
        RuleFor(x => x.InventoryItemId).NotEmpty();
        RuleFor(x => x.DyeLotNumber).NotEmpty();
        RuleFor(x => x.GSM).GreaterThan(0);
        RuleFor(x => x.WidthInches).GreaterThan(0);
        RuleFor(x => x.TotalLengthYards).GreaterThan(0);
        RuleFor(x => x.RackLocation).NotEmpty();
    }
}

public class CreateFabricRollCommandHandler(IInventoryRepository repository)
    : IRequestHandler<CreateFabricRollCommand, Result<FabricRollDto>>
{
    public async Task<Result<FabricRollDto>> Handle(CreateFabricRollCommand request, CancellationToken cancellationToken)
    {
        var item = await repository.GetItemByIdAsync(request.InventoryItemId, cancellationToken);
        if (item == null) return Result<FabricRollDto>.Failure("Fabric inventory item not found.");

        var rollNo = $"ROL-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";

        var roll = FabricRoll.Create(
            rollNo,
            item.Id,
            request.DyeLotNumber,
            request.GSM,
            request.WidthInches,
            request.TotalLengthYards,
            request.RackLocation,
            request.CompanyId,
            request.SupplierRollNo,
            request.GoodsReceiptNoteId,
            request.Grade
        );

        // Add yards to inventory stock
        item.AddStock(request.TotalLengthYards);

        await repository.AddFabricRollAsync(roll, cancellationToken);
        await repository.UpdateItemAsync(item, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var dto = new FabricRollDto(
            roll.Id,
            roll.RollNumber,
            roll.InventoryItemId,
            item.ItemCode,
            item.Name,
            roll.DyeLotNumber,
            roll.GSM,
            roll.WidthInches,
            roll.TotalLengthYards,
            roll.AvailableLengthYards,
            roll.Grade,
            roll.Grade.ToString(),
            roll.Status,
            roll.Status.ToString(),
            roll.RackLocation,
            roll.SupplierRollNo,
            roll.GoodsReceiptNoteId,
            roll.CompanyId,
            roll.CreatedAt
        );

        return Result<FabricRollDto>.Success(dto);
    }
}

public record IssueFabricRollCommand(
    Guid RollId,
    decimal YardsToIssue,
    string CuttingOrderRef,
    Guid PerformedBy,
    Guid CompanyId
) : IRequest<Result<FabricRollDto>>;

public class IssueFabricRollCommandHandler(IInventoryRepository repository)
    : IRequestHandler<IssueFabricRollCommand, Result<FabricRollDto>>
{
    public async Task<Result<FabricRollDto>> Handle(IssueFabricRollCommand request, CancellationToken cancellationToken)
    {
        var roll = await repository.GetFabricRollByIdAsync(request.RollId, cancellationToken);
        if (roll == null) return Result<FabricRollDto>.Failure("Fabric roll not found.");

        try
        {
            roll.IssueYards(request.YardsToIssue);

            // Deduct from item stock
            var item = await repository.GetItemByIdAsync(roll.InventoryItemId, cancellationToken);
            if (item != null)
            {
                item.DeductStock(request.YardsToIssue);
                await repository.UpdateItemAsync(item, cancellationToken);

                var txNumber = $"TX-ISS-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}";
                var tx = StockTransaction.Create(
                    txNumber,
                    item.Id,
                    StockTransactionType.IssueToCutting,
                    request.YardsToIssue,
                    item.StandardCost,
                    roll.RackLocation,
                    "CUTTING-TABLE-01",
                    request.PerformedBy,
                    request.CompanyId,
                    refNumber: request.CuttingOrderRef,
                    notes: $"Issued {request.YardsToIssue} yds from Roll {roll.RollNumber} for cutting order {request.CuttingOrderRef}"
                );
                await repository.AddTransactionAsync(tx, cancellationToken);
            }

            await repository.UpdateFabricRollAsync(roll, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);

            var dto = new FabricRollDto(
                roll.Id,
                roll.RollNumber,
                roll.InventoryItemId,
                item?.ItemCode ?? "FABRIC",
                item?.Name ?? "Fabric",
                roll.DyeLotNumber,
                roll.GSM,
                roll.WidthInches,
                roll.TotalLengthYards,
                roll.AvailableLengthYards,
                roll.Grade,
                roll.Grade.ToString(),
                roll.Status,
                roll.Status.ToString(),
                roll.RackLocation,
                roll.SupplierRollNo,
                roll.GoodsReceiptNoteId,
                roll.CompanyId,
                roll.CreatedAt
            );

            return Result<FabricRollDto>.Success(dto);
        }
        catch (Exception ex)
        {
            return Result<FabricRollDto>.Failure(ex.Message);
        }
    }
}
