using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Queries;

public record GetGoodsReceiptNotesQuery(
    Guid CompanyId,
    Guid? PurchaseOrderId = null,
    Guid? SupplierId = null,
    GRNStatus? Status = null
) : IRequest<Result<IReadOnlyList<GoodsReceiptNoteDto>>>;

public record GetGoodsReceiptNoteByIdQuery(Guid Id) : IRequest<Result<GoodsReceiptNoteDto>>;

public class GoodsReceiptNoteQueryHandlers(IProcurementRepository repository)
    : IRequestHandler<GetGoodsReceiptNotesQuery, Result<IReadOnlyList<GoodsReceiptNoteDto>>>,
      IRequestHandler<GetGoodsReceiptNoteByIdQuery, Result<GoodsReceiptNoteDto>>
{
    public async Task<Result<IReadOnlyList<GoodsReceiptNoteDto>>> Handle(GetGoodsReceiptNotesQuery request, CancellationToken cancellationToken)
    {
        var grns = await repository.GetGRNsAsync(
            request.CompanyId,
            request.PurchaseOrderId,
            request.SupplierId,
            request.Status,
            cancellationToken
        );

        var dtos = grns.Select(grn => MapToDto(grn)).ToList();
        return Result<IReadOnlyList<GoodsReceiptNoteDto>>.Success(dtos);
    }

    public async Task<Result<GoodsReceiptNoteDto>> Handle(GetGoodsReceiptNoteByIdQuery request, CancellationToken cancellationToken)
    {
        var grn = await repository.GetGRNByIdAsync(request.Id, cancellationToken);
        if (grn == null) return Result<GoodsReceiptNoteDto>.Failure("Goods receipt note not found.");

        return Result<GoodsReceiptNoteDto>.Success(MapToDto(grn));
    }

    private static GoodsReceiptNoteDto MapToDto(Domain.Entities.GoodsReceiptNote grn)
    {
        var itemDtos = grn.Items.Select(i => new GoodsReceiptNoteItemDto(
            i.Id,
            i.GoodsReceiptNoteId,
            i.PurchaseOrderItemId,
            i.ItemCode,
            i.Description,
            i.ReceivedQuantity,
            i.AcceptedQuantity,
            i.RejectedQuantity,
            i.RejectionReason
        )).ToList();

        return new GoodsReceiptNoteDto(
            grn.Id,
            grn.GRNNumber,
            grn.PurchaseOrderId,
            grn.PurchaseOrder?.PONumber ?? "N/A",
            grn.SupplierId,
            grn.Supplier?.Name ?? "Supplier",
            grn.CompanyId,
            grn.ReceivedDate,
            grn.DeliveryChallanNo,
            grn.VehicleNo,
            grn.InspectedBy,
            grn.Status,
            grn.Status.ToString(),
            grn.Notes,
            itemDtos,
            grn.CreatedAt
        );
    }
}
