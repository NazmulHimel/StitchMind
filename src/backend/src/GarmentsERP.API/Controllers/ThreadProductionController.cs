using GarmentsERP.Inventory.Application.Commands;
using GarmentsERP.Inventory.Application.DTOs;
using GarmentsERP.Inventory.Application.Queries;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using GarmentsERP.Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ThreadProductionController(ISender sender, ICurrentUserService currentUserService, InventoryDbContext db) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet("recipes")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRecipes(CancellationToken ct)
    {
        var recipes = await db.ThreadDyeingRecipes
            .Include(r => r.Ingredients)
            .OrderBy(r => r.RecipeCode)
            .ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<GarmentsERP.Inventory.Domain.Entities.ThreadDyeingRecipe>>.Ok(recipes));
    }

    [HttpGet("tensile-tests")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTensileTests(CancellationToken ct)
    {
        var tests = await db.ThreadTensileTests
            .OrderByDescending(t => t.TestDate)
            .ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<GarmentsERP.Inventory.Domain.Entities.ThreadTensileTest>>.Ok(tests));
    }

    [HttpGet("batches")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<ThreadBatchDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBatches(
        [FromQuery] ThreadStage? stage,
        [FromQuery] ThreadBatchStatus? status,
        CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = new GetThreadBatchesQuery(companyId, stage, status);
        var result = await sender.Send(query, ct);

        return Ok(ApiResponse<IReadOnlyList<ThreadBatchDto>>.Ok(result.Value ?? []));
    }

    [HttpGet("batches/{id:guid}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ThreadBatchDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBatchById(Guid id, CancellationToken ct)
    {
        var query = new GetThreadBatchByIdQuery(id);
        var result = await sender.Send(query, ct);

        if (!result.IsSuccess)
            return NotFound(ApiResponse<object>.Fail(result.Error ?? "Thread batch not found."));

        return Ok(ApiResponse<ThreadBatchDto>.Ok(result.Value!));
    }

    [HttpPost("batches")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ThreadBatchDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBatch([FromBody] CreateThreadBatchRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var supervisorId = currentUserService.UserId != Guid.Empty
            ? currentUserService.UserId
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        var command = new CreateThreadBatchCommand(
            request.YarnCount,
            request.FiberComposition,
            request.ColorCode,
            request.ColorName,
            request.TargetQuantityKg,
            companyId,
            supervisorId,
            request.Notes
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to create thread batch."));

        return StatusCode(StatusCodes.Status201Created, ApiResponse<ThreadBatchDto>.Ok(result.Value!, "Thread production batch initiated."));
    }

    [HttpPost("batches/{id:guid}/advance-stage")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ThreadBatchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> AdvanceStage(Guid id, [FromBody] AdvanceThreadStageRequest request, CancellationToken ct)
    {
        var command = new AdvanceThreadStageCommand(
            id,
            request.NextStage,
            request.OutputQuantityKg,
            request.WasteQuantityKg,
            request.MachineId,
            request.OperatorName,
            request.Remarks
        );

        var result = await sender.Send(command, ct);

        if (!result.IsSuccess)
            return BadRequest(ApiResponse<object>.Fail(result.Error ?? "Failed to advance thread production stage."));

        return Ok(ApiResponse<ThreadBatchDto>.Ok(result.Value!, $"Thread batch advanced to {request.NextStage}."));
    }
}
