using GarmentsERP.Inventory.Application.Commands;
using GarmentsERP.Inventory.Application.DTOs;
using GarmentsERP.Inventory.Application.Queries;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class InventoryController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet("items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<InventoryItemDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItems(
        [FromQuery] ItemCategory? category,
        [FromQuery] string? search,
        [FromQuery] bool lowStockOnly,
        CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetInventoryItemsQuery(companyId, category, search, lowStockOnly);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<InventoryItemDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("items/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetItemById(Guid id, CancellationToken ct)
    {
        var query = new GetInventoryItemByIdQuery(id);
        var result = await sender.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error ?? "Inventory item not found."));

        return Ok(ApiResponse<InventoryItemDto>.Ok(result.Value!));
    }

    [HttpPost("items")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateItem([FromBody] CreateInventoryItemRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var command = new CreateInventoryItemCommand(
            request.ItemCode,
            request.Name,
            request.Description,
            request.Category,
            request.UnitOfMeasure,
            request.InitialStock,
            request.SafetyStock,
            request.ReorderPoint,
            request.StandardCost,
            request.BinLocation,
            companyId
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to create inventory item."));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<InventoryItemDto>.Ok(result.Value!, "Inventory item created successfully."));
    }

    [HttpPost("adjust")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InventoryItemDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AdjustStock([FromBody] AdjustStockRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var userId = currentUserService.UserId != Guid.Empty
            ? currentUserService.UserId
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        var command = new AdjustStockCommand(
            request.ItemId,
            request.AdjustedStockQuantity,
            request.Reason,
            request.Location,
            userId,
            companyId
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to adjust stock."));

        return Ok(ApiResponse<InventoryItemDto>.Ok(result.Value!, "Stock adjusted successfully."));
    }

    [HttpGet("transactions")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<StockTransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransactions(
        [FromQuery] Guid? itemId,
        [FromQuery] StockTransactionType? type,
        [FromQuery] int take = 50,
        CancellationToken ct = default)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetStockTransactionsQuery(companyId, itemId, type, take);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<StockTransactionDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("metrics")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<InventoryMetricsDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMetrics(CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetInventoryMetricsQuery(companyId);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<InventoryMetricsDto>.Ok(result.Value!));
    }
}
