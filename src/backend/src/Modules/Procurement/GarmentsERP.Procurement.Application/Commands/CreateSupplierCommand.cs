using FluentValidation;
using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Commands;

public record CreateSupplierCommand(
    string Code,
    string Name,
    string Email,
    string Phone,
    string Address,
    MaterialCategory Category,
    Guid CompanyId,
    int PaymentTermsDays = 30,
    string? TaxRegistrationNo = null
) : IRequest<Result<SupplierDto>>;

public class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Phone).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(500);
        RuleFor(x => x.PaymentTermsDays).GreaterThanOrEqualTo(0);
    }
}

public class CreateSupplierCommandHandler(IProcurementRepository repository)
    : IRequestHandler<CreateSupplierCommand, Result<SupplierDto>>
{
    public async Task<Result<SupplierDto>> Handle(CreateSupplierCommand request, CancellationToken cancellationToken)
    {
        var existing = await repository.GetSupplierByCodeAsync(request.Code, cancellationToken);
        if (existing != null)
        {
            return Result<SupplierDto>.Failure($"Supplier code '{request.Code}' already exists.");
        }

        var supplier = Supplier.Create(
            request.Code,
            request.Name,
            request.Email,
            request.Phone,
            request.Address,
            request.Category,
            request.CompanyId,
            request.PaymentTermsDays,
            request.TaxRegistrationNo
        );

        await repository.AddSupplierAsync(supplier, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        var dto = new SupplierDto(
            supplier.Id,
            supplier.Code,
            supplier.Name,
            supplier.Email,
            supplier.Phone,
            supplier.Address,
            supplier.Category,
            supplier.Category.ToString(),
            supplier.PaymentTermsDays,
            supplier.TaxRegistrationNo,
            supplier.Rating,
            supplier.IsActive,
            supplier.CompanyId,
            supplier.CreatedAt
        );

        return Result<SupplierDto>.Success(dto);
    }
}
