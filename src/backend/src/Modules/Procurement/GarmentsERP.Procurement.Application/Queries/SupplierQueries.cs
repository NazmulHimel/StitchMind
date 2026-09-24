using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Procurement.Application.Queries;

public record GetSuppliersQuery(
    Guid CompanyId,
    MaterialCategory? Category = null,
    string? Search = null,
    bool? IsActive = null
) : IRequest<Result<IReadOnlyList<SupplierDto>>>;

public record GetSupplierByIdQuery(Guid Id) : IRequest<Result<SupplierDto>>;

public class SupplierQueryHandlers(IProcurementRepository repository)
    : IRequestHandler<GetSuppliersQuery, Result<IReadOnlyList<SupplierDto>>>,
      IRequestHandler<GetSupplierByIdQuery, Result<SupplierDto>>
{
    public async Task<Result<IReadOnlyList<SupplierDto>>> Handle(GetSuppliersQuery request, CancellationToken cancellationToken)
    {
        var suppliers = await repository.GetSuppliersAsync(
            request.CompanyId,
            request.Category,
            request.Search,
            request.IsActive,
            cancellationToken
        );

        var dtos = suppliers.Select(s => new SupplierDto(
            s.Id,
            s.Code,
            s.Name,
            s.Email,
            s.Phone,
            s.Address,
            s.Category,
            s.Category.ToString(),
            s.PaymentTermsDays,
            s.TaxRegistrationNo,
            s.Rating,
            s.IsActive,
            s.CompanyId,
            s.CreatedAt
        )).ToList();

        return Result<IReadOnlyList<SupplierDto>>.Success(dtos);
    }

    public async Task<Result<SupplierDto>> Handle(GetSupplierByIdQuery request, CancellationToken cancellationToken)
    {
        var s = await repository.GetSupplierByIdAsync(request.Id, cancellationToken);
        if (s == null) return Result<SupplierDto>.Failure("Supplier not found.");

        var dto = new SupplierDto(
            s.Id,
            s.Code,
            s.Name,
            s.Email,
            s.Phone,
            s.Address,
            s.Category,
            s.Category.ToString(),
            s.PaymentTermsDays,
            s.TaxRegistrationNo,
            s.Rating,
            s.IsActive,
            s.CompanyId,
            s.CreatedAt
        );

        return Result<SupplierDto>.Success(dto);
    }
}
