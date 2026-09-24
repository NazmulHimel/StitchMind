using FluentValidation;
using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Commands;

public record CreateGoodsReceiptNoteCommand(
    Guid PurchaseOrderId,
    Guid CompanyId,
    string DeliveryChallanNo,
    string? VehicleNo,
    string? Notes,
    List<CreateGRNItemRequest> Items
) : IRequest<Result<GoodsReceiptNoteDto>>;

public class CreateGoodsReceiptNoteCommandValidator : AbstractValidator<CreateGoodsReceiptNoteCommand>
{
    public CreateGoodsReceiptNoteCommandValidator()
    {
        RuleFor(x => x.PurchaseOrderId).NotEmpty();
        RuleFor(x => x.DeliveryChallanNo).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one received item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.PurchaseOrderItemId).NotEmpty();
            item.RuleFor(i => i.ReceivedQuantity).GreaterThan(0);
        });
    }
}

public class CreateGoodsReceiptNoteCommandHandler(IProcurementRepository repository)
    : IRequestHandler<CreateGoodsReceiptNoteCommand, Result<GoodsReceiptNoteDto>>
{
    public async Task<Result<GoodsReceiptNoteDto>> Handle(CreateGoodsReceiptNoteCommand request, CancellationToken cancellationToken)
    {
        var po = await repository.GetPurchaseOrderByIdAsync(request.PurchaseOrderId, cancellationToken);
        if (po == null) return Result<GoodsReceiptNoteDto>.Failure("Purchase order not found.");

        if (po.Status != POStatus.Approved && po.Status != POStatus.PartiallyReceived)
        {
            return Result<GoodsReceiptNoteDto>.Failure("Goods can only be received against Approved or Partially Received purchase orders.");
        }

        var randomSuffix = Random.Shared.Next(1000, 9999);
        var grnNumber = $"GRN-{DateTime.UtcNow:yyyyMM}-{randomSuffix}";

        var grn = GoodsReceiptNote.Create(
            grnNumber,
            po.Id,
            po.SupplierId,
            request.CompanyId,
            request.DeliveryChallanNo,
            request.VehicleNo,
            request.Notes
        );

        foreach (var itemReq in request.Items)
        {
            var poItem = po.Items.FirstOrDefault(i => i.Id == itemReq.PurchaseOrderItemId);
            if (poItem == null)
            {
                return Result<GoodsReceiptNoteDto>.Failure($"PO Item '{itemReq.PurchaseOrderItemId}' not found in PO.");
            }

            grn.AddItem(
                poItem.Id,
                itemReq.ItemCode,
                itemReq.Description,
                itemReq.ReceivedQuantity,
                itemReq.AcceptedQuantity,
                itemReq.RejectedQuantity,
                itemReq.RejectionReason
            );

            // Update PO Item received quantity with accepted quantity
            poItem.RecordReceived(itemReq.AcceptedQuantity);
        }

        // Auto inspect / complete if accepted quantities exist
        grn.CompleteInspection();
        po.UpdateReceivingStatus();

        await repository.AddGRNAsync(grn, cancellationToken);
        await repository.UpdatePurchaseOrderAsync(po, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

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

        var dto = new GoodsReceiptNoteDto(
            grn.Id,
            grn.GRNNumber,
            grn.PurchaseOrderId,
            po.PONumber,
            grn.SupplierId,
            po.Supplier?.Name ?? "Supplier",
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

        return Result<GoodsReceiptNoteDto>.Success(dto);
    }
}
