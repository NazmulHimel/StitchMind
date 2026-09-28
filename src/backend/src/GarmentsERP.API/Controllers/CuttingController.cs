using GarmentsERP.Production.Application.Commands;
using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Queries;
using GarmentsERP.Production.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[AllowAnonymous]
public class CuttingController(IMediator mediator) : ControllerBase
{
    // Orders & Lay Planning (CUT-03)
    [HttpGet("orders")]
    public async Task<IActionResult> GetCuttingOrders([FromQuery] CuttingStatus? status)
    {
        var result = await mediator.Send(new GetCuttingOrdersQuery(status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateCuttingOrder([FromBody] CreateCuttingOrderDto dto)
    {
        var result = await mediator.Send(new CreateCuttingOrderCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // CAD Markers (CUT-01)
    [HttpGet("markers")]
    public async Task<IActionResult> GetCADMarkers()
    {
        var result = await mediator.Send(new GetCADMarkersQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("markers")]
    public async Task<IActionResult> CreateCADMarker([FromBody] CreateCADMarkerDto dto)
    {
        var result = await mediator.Send(new CreateCADMarkerCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // Relaxation Time-Lock Gate (CUT-02)
    [HttpGet("relaxation-sessions")]
    public async Task<IActionResult> GetRelaxationSessions([FromQuery] RelaxationStatus? status)
    {
        var result = await mediator.Send(new GetRelaxationSessionsQuery(status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("relaxation-sessions/unroll")]
    public async Task<IActionResult> UnrollFabricRoll([FromBody] StartRelaxationSessionDto dto)
    {
        var result = await mediator.Send(new StartRelaxationSessionCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("orders/{id}/authorize-spreading")]
    public async Task<IActionResult> AuthorizeSpreading(Guid id)
    {
        var result = await mediator.Send(new AuthorizeCuttingSpreadingCommand(id));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(new { allowed = false, message = result.Error });
    }

    // Bundles & Thermal Barcodes (CUT-04)
    [HttpGet("bundles")]
    public async Task<IActionResult> GetCutBundles([FromQuery] Guid? cuttingOrderId, [FromQuery] BundleSection? section)
    {
        var result = await mediator.Send(new GetCutBundlesQuery(cuttingOrderId, section));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("orders/{id}/generate-bundles")]
    public async Task<IActionResult> GenerateBundles(Guid id, [FromQuery] int bundleSize = 50)
    {
        var result = await mediator.Send(new GenerateBundlesCommand(id, bundleSize));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("orders/{id}/generate-thermal-bundles")]
    public async Task<IActionResult> GenerateThermalBundles(Guid id, [FromQuery] int bundleSize = 24)
    {
        var result = await mediator.Send(new GenerateThermalBundlesCommand(id, bundleSize));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("bundles/{id}/transfer")]
    public async Task<IActionResult> TransferBundle(Guid id, [FromBody] BundleSection nextSection)
    {
        var result = await mediator.Send(new UpdateBundleSectionCommand(id, nextSection));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // Fusing Machine Parameters (CUT-05)
    [HttpGet("fusing-logs")]
    public async Task<IActionResult> GetFusingLogs([FromQuery] string? cutOrderNumber)
    {
        var result = await mediator.Send(new GetFusingLogsQuery(cutOrderNumber));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("fusing-logs")]
    public async Task<IActionResult> CreateFusingLog([FromBody] CreateFusingMachineLogDto dto)
    {
        var result = await mediator.Send(new CreateFusingLogCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // Subcontract Cutting & Shortage Reconciliation (CUT-06)
    [HttpGet("subcontract-challans")]
    public async Task<IActionResult> GetSubcontractChallans([FromQuery] SubcontractStatus? status)
    {
        var result = await mediator.Send(new GetSubcontractChallansQuery(status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("subcontract-challans")]
    public async Task<IActionResult> CreateSubcontractChallan([FromBody] CreateSubcontractCuttingChallanDto dto)
    {
        var result = await mediator.Send(new CreateSubcontractChallanCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("subcontract-challans/{id}/reconcile")]
    public async Task<IActionResult> ReconcileSubcontract(Guid id, [FromBody] ReconcileSubcontractDto dto)
    {
        var result = await mediator.Send(new ReconcileSubcontractCommand(id, dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
