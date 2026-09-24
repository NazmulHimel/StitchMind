using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Commands;

public record SubmitPurchaseOrderCommand(Guid Id) : IRequest<Result<PurchaseOrderDto>>;
public record ApprovePurchaseOrderCommand(Guid Id, Guid ApprovedByUserId) : IRequest<Result<PurchaseOrderDto>>;
public record CancelPurchaseOrderCommand(Guid Id) : IRequest<Result<PurchaseOrderDto>>;

public class PurchaseOrderStatusCommandHandlers(IProcurementRepository repository)
    : IRequestHandler<SubmitPurchaseOrderCommand, Result<PurchaseOrderDto>>,
      IRequestHandler<ApprovePurchaseOrderCommand, Result<PurchaseOrderDto>>,
      IRequestHandler<CancelPurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(SubmitPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await repository.GetPurchaseOrderByIdAsync(request.Id, cancellationToken);
        if (po == null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");

        try
        {
            po.Submit();
            await repository.UpdatePurchaseOrderAsync(po, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return Result<PurchaseOrderDto>.Success(MapToDto(po));
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderDto>> Handle(ApprovePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await repository.GetPurchaseOrderByIdAsync(request.Id, cancellationToken);
        if (po == null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");

        try
        {
            po.Approve(request.ApprovedByUserId);
            await repository.UpdatePurchaseOrderAsync(po, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return Result<PurchaseOrderDto>.Success(MapToDto(po));
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.Message);
        }
    }

    public async Task<Result<PurchaseOrderDto>> Handle(CancelPurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var po = await repository.GetPurchaseOrderByIdAsync(request.Id, cancellationToken);
        if (po == null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");

        try
        {
            po.Cancel();
            await repository.UpdatePurchaseOrderAsync(po, cancellationToken);
            await repository.SaveChangesAsync(cancellationToken);
            return Result<PurchaseOrderDto>.Success(MapToDto(po));
        }
        catch (Exception ex)
        {
            return Result<PurchaseOrderDto>.Failure(ex.Message);
        }
    }

    private static PurchaseOrderDto MapToDto(Domain.Entities.PurchaseOrder po)
    {
        var itemDtos = po.Items.Select(i => new PurchaseOrderItemDto(
            i.Id,
            i.PurchaseOrderId,
            i.ItemCode,
            i.Description,
            i.Category,
            i.Category.ToString(),
            i.UnitOfMeasure,
            i.UnitOfMeasure.ToString(),
            i.Quantity,
            i.UnitPrice,
            i.TotalPrice,
            i.ReceivedQuantity
        )).ToList();

        return new PurchaseOrderDto(
            po.Id,
            po.PONumber,
            po.SupplierId,
            po.Supplier?.Name ?? "Unknown Supplier",
            po.CompanyId,
            po.OrderDate,
            po.ExpectedDeliveryDate,
            po.Status,
            po.Status.ToString(),
            po.TotalAmount,
            po.Notes,
            po.ApprovedBy,
            po.ApprovedAt,
            itemDtos,
            po.CreatedAt
        );
    }
}
