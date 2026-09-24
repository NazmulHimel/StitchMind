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
public class FabricRollsController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<FabricRollDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolls(
        [FromQuery] Guid? itemId,
        [FromQuery] string? dyeLot,
        [FromQuery] RollStatus? status,
        [FromQuery] FabricGrade? grade,
        CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetFabricRollsQuery(companyId, itemId, dyeLot, status, grade);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<FabricRollDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FabricRollDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRollById(Guid id, CancellationToken ct)
    {
        var query = new GetFabricRollByIdQuery(id);
        var result = await sender.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error ?? "Fabric roll not found."));

        return Ok(ApiResponse<FabricRollDto>.Ok(result.Value!));
    }

    [HttpPost]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FabricRollDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateRoll([FromBody] CreateFabricRollRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var command = new CreateFabricRollCommand(
            request.InventoryItemId,
            request.DyeLotNumber,
            request.GSM,
            request.WidthInches,
            request.TotalLengthYards,
            request.RackLocation,
            companyId,
            request.SupplierRollNo,
            Grade: request.Grade
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to create fabric roll."));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<FabricRollDto>.Ok(result.Value!, "Fabric roll registered into warehouse."));
    }

    [HttpPost("issue")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<FabricRollDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> IssueRoll([FromBody] IssueFabricRollRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var userId = currentUserService.UserId != Guid.Empty
            ? currentUserService.UserId
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        var command = new IssueFabricRollCommand(
            request.RollId,
            request.YardsToIssue,
            request.CuttingOrderRef,
            userId,
            companyId
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to issue fabric from roll."));

        return Ok(ApiResponse<FabricRollDto>.Ok(result.Value!, "Fabric issued to cutting order successfully."));
    }
}
