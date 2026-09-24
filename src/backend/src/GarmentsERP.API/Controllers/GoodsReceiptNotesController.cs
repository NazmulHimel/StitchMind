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
public class GoodsReceiptNotesController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<GoodsReceiptNoteDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGoodsReceiptNotes(
        [FromQuery] Guid? purchaseOrderId,
        [FromQuery] Guid? supplierId,
        [FromQuery] GRNStatus? status,
        CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetGoodsReceiptNotesQuery(companyId, purchaseOrderId, supplierId, status);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<GoodsReceiptNoteDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptNoteDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGoodsReceiptNoteById(Guid id, CancellationToken ct)
    {
        var query = new GetGoodsReceiptNoteByIdQuery(id);
        var result = await sender.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error ?? "Goods receipt note not found."));

        return Ok(ApiResponse<GoodsReceiptNoteDto>.Ok(result.Value!));
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<GoodsReceiptNoteDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGoodsReceiptNote([FromBody] CreateGoodsReceiptNoteRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var command = new CreateGoodsReceiptNoteCommand(
            request.PurchaseOrderId,
            companyId,
            request.DeliveryChallanNo,
            request.VehicleNo,
            request.Notes,
            request.Items
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to create goods receipt note."));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<GoodsReceiptNoteDto>.Ok(result.Value!, "Goods receipt note recorded and inspected successfully."));
    }
}
