using FluentValidation;
using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Commands;

public record CreatePurchaseOrderCommand(
    Guid SupplierId,
    Guid CompanyId,
    DateTime ExpectedDeliveryDate,
    string? Notes,
    List<CreatePurchaseOrderItemRequest> Items
) : IRequest<Result<PurchaseOrderDto>>;

public class CreatePurchaseOrderCommandValidator : AbstractValidator<CreatePurchaseOrderCommand>
{
    public CreatePurchaseOrderCommandValidator()
    {
        RuleFor(x => x.SupplierId).NotEmpty();
        RuleFor(x => x.ExpectedDeliveryDate).GreaterThan(DateTime.UtcNow.Date.AddDays(-1));
        RuleFor(x => x.Items).NotEmpty().WithMessage("At least one purchase order item is required.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.ItemCode).NotEmpty();
            item.RuleFor(i => i.Description).NotEmpty();
            item.RuleFor(i => i.Quantity).GreaterThan(0);
            item.RuleFor(i => i.UnitPrice).GreaterThan(0);
        });
    }
}

public class CreatePurchaseOrderCommandHandler(IProcurementRepository repository)
    : IRequestHandler<CreatePurchaseOrderCommand, Result<PurchaseOrderDto>>
{
    public async Task<Result<PurchaseOrderDto>> Handle(CreatePurchaseOrderCommand request, CancellationToken cancellationToken)
    {
        var supplier = await repository.GetSupplierByIdAsync(request.SupplierId, cancellationToken);
        if (supplier == null)
        {
            return Result<PurchaseOrderDto>.Failure("Supplier not found.");
        }

        // Generate sequential PO Number like PO-2026-XXXX
        var randomSuffix = Random.Shared.Next(1000, 9999);
        var poNumber = $"PO-{DateTime.UtcNow:yyyyMM}-{randomSuffix}";

        var po = PurchaseOrder.Create(
            poNumber,
            supplier.Id,
            request.CompanyId,
            request.ExpectedDeliveryDate,
            request.Notes
        );

        foreach (var item in request.Items)
        {
            po.AddItem(item.ItemCode, item.Description, item.Category, item.UnitOfMeasure, item.Quantity, item.UnitPrice);
        }

        await repository.AddPurchaseOrderAsync(po, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

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

        var dto = new PurchaseOrderDto(
            po.Id,
            po.PONumber,
            supplier.Id,
            supplier.Name,
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

        return Result<PurchaseOrderDto>.Success(dto);
    }
}
