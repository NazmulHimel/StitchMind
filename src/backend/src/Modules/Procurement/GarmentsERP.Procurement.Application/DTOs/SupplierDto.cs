using GarmentsERP.Procurement.Domain.Enums;

namespace GarmentsERP.Procurement.Application.DTOs;

public record SupplierDto(
    Guid Id,
    string Code,
    string Name,
    string Email,
    string Phone,
    string Address,
    MaterialCategory Category,
    string CategoryName,
    int PaymentTermsDays,
    string? TaxRegistrationNo,
    decimal Rating,
    bool IsActive,
    Guid CompanyId,
    DateTime CreatedAt
);

public record CreateSupplierRequest(
    string Code,
    string Name,
    string Email,
    string Phone,
    string Address,
    MaterialCategory Category,
    int PaymentTermsDays,
    string? TaxRegistrationNo
);

public record UpdateSupplierRequest(
    string Name,
    string Email,
    string Phone,
    string Address,
    MaterialCategory Category,
    int PaymentTermsDays,
    string? TaxRegistrationNo
);
