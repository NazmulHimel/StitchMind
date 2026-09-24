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
public class PurchaseOrdersController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PurchaseOrderDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPurchaseOrders(
        [FromQuery] POStatus? status,
        [FromQuery] Guid? supplierId,
        [FromQuery] string? search,
        CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetPurchaseOrdersQuery(companyId, status, supplierId, search);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<PurchaseOrderDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("metrics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ProcurementMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetProcurementMetricsQuery(companyId);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<ProcurementMetricsDto>.Ok(result.Value!));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPurchaseOrderById(Guid id, CancellationToken ct)
    {
        var query = new GetPurchaseOrderByIdQuery(id);
        var result = await sender.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error ?? "Purchase order not found."));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Value!));
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreatePurchaseOrder([FromBody] CreatePurchaseOrderRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var command = new CreatePurchaseOrderCommand(
            request.SupplierId,
            companyId,
            request.ExpectedDeliveryDate,
            request.Notes,
            request.Items
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to create purchase order."));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<PurchaseOrderDto>.Ok(result.Value!, "Purchase order created successfully."));
    }

    [HttpPost("{id:guid}/submit")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Submit(Guid id, CancellationToken ct)
    {
        var command = new SubmitPurchaseOrderCommand(id);
        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to submit purchase order."));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Value!, "Purchase order submitted for approval."));
    }

    [HttpPost("{id:guid}/approve")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Approve(Guid id, CancellationToken ct)
    {
        var approverId = currentUserService.UserId != Guid.Empty
            ? currentUserService.UserId
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        var command = new ApprovePurchaseOrderCommand(id, approverId);
        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to approve purchase order."));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Value!, "Purchase order approved successfully."));
    }

    [HttpPost("{id:guid}/cancel")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<PurchaseOrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken ct)
    {
        var command = new CancelPurchaseOrderCommand(id);
        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to cancel purchase order."));

        return Ok(ApiResponse<PurchaseOrderDto>.Ok(result.Value!, "Purchase order cancelled."));
    }
}
