using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Queries;

public record GetPurchaseOrdersQuery(
    Guid CompanyId,
    POStatus? Status = null,
    Guid? SupplierId = null,
    string? Search = null
) : IRequest<Result<IReadOnlyList<PurchaseOrderDto>>>;

public record GetPurchaseOrderByIdQuery(Guid Id) : IRequest<Result<PurchaseOrderDto>>;

public class PurchaseOrderQueryHandlers(IProcurementRepository repository)
    : IRequestHandler<GetPurchaseOrdersQuery, Result<IReadOnlyList<PurchaseOrderDto>>>,
      IRequestHandler<GetPurchaseOrderByIdQuery, Result<PurchaseOrderDto>>
{
    public async Task<Result<IReadOnlyList<PurchaseOrderDto>>> Handle(GetPurchaseOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await repository.GetPurchaseOrdersAsync(
            request.CompanyId,
            request.Status,
            request.SupplierId,
            request.Search,
            cancellationToken
        );

        var dtos = orders.Select(po => MapToDto(po)).ToList();
        return Result<IReadOnlyList<PurchaseOrderDto>>.Success(dtos);
    }

    public async Task<Result<PurchaseOrderDto>> Handle(GetPurchaseOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var po = await repository.GetPurchaseOrderByIdAsync(request.Id, cancellationToken);
        if (po == null) return Result<PurchaseOrderDto>.Failure("Purchase order not found.");

        return Result<PurchaseOrderDto>.Success(MapToDto(po));
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
            po.Supplier?.Name ?? "Supplier",
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
