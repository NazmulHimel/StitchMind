using GarmentsERP.Procurement.Application.Commands;
using GarmentsERP.Procurement.Application.DTOs;
using GarmentsERP.Procurement.Application.Queries;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SuppliersController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet]
    [AllowAnonymous] // Allows demo access & frontend showcase
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<SupplierDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSuppliers(
        [FromQuery] MaterialCategory? category,
        [FromQuery] string? search,
        [FromQuery] bool? isActive,
        CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetSuppliersQuery(companyId, category, search, isActive);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<SupplierDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSupplierById(Guid id, CancellationToken ct)
    {
        var query = new GetSupplierByIdQuery(id);
        var result = await sender.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error ?? "Supplier not found."));

        return Ok(ApiResponse<SupplierDto>.Ok(result.Value!));
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<SupplierDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateSupplier([FromBody] CreateSupplierRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var command = new CreateSupplierCommand(
            request.Code,
            request.Name,
            request.Email,
            request.Phone,
            request.Address,
            request.Category,
            companyId,
            request.PaymentTermsDays,
            request.TaxRegistrationNo
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to create supplier."));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<SupplierDto>.Ok(result.Value!, "Supplier created successfully."));
    }
}
