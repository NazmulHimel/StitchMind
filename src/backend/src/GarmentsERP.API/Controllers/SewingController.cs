using GarmentsERP.Production.Application.Commands;
using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SewingController : ControllerBase
{
    private readonly IMediator _mediator;

    public SewingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("lines")]
    public async Task<IActionResult> GetSewingLines()
    {
        var result = await _mediator.Send(new GetSewingLinesQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("lines")]
    public async Task<IActionResult> CreateSewingLine([FromBody] CreateSewingLineDto dto)
    {
        var result = await _mediator.Send(new CreateSewingLineCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("hourly-logs")]
    public async Task<IActionResult> GetHourlyLogs([FromQuery] DateTime? date, [FromQuery] Guid? lineId)
    {
        var result = await _mediator.Send(new GetHourlyProductionLogsQuery(date, lineId));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("hourly-logs/record")]
    public async Task<IActionResult> RecordHourlyOutput([FromBody] RecordHourlyOutputDto dto)
    {
        var result = await _mediator.Send(new RecordHourlyOutputCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // IE-01: Operation Bulletins
    [HttpGet("operation-bulletins")]
    public async Task<IActionResult> GetOperationBulletins([FromQuery] string? styleNumber)
    {
        var result = await _mediator.Send(new GetOperationBulletinsQuery(styleNumber));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("operation-bulletins")]
    public async Task<IActionResult> CreateOperationBulletin([FromBody] CreateOperationBulletinDto dto)
    {
        var result = await _mediator.Send(new CreateOperationBulletinCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // SEW-01: Pitch Diagram & Real-time Line Balancing Calculation
    [HttpGet("line-balancing")]
    public async Task<IActionResult> GetLineBalancing([FromQuery] string styleNumber = "ST-2026-CHINO-01", [FromQuery] int workstations = 50)
    {
        var result = await _mediator.Send(new CalculateLineBalancingQuery(styleNumber, workstations));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // SEW-02: Sub-300ms Barcode Bundle Scanning API
    [HttpPost("bundle-scan")]
    public async Task<IActionResult> RecordBundleScan([FromBody] RecordBundleScanDto dto)
    {
        var result = await _mediator.Send(new RecordBundleScanCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("bundle-scans")]
    public async Task<IActionResult> GetBundleScans([FromQuery] string? lineNumber, [FromQuery] string? barcode)
    {
        var result = await _mediator.Send(new GetBundleScansQuery(lineNumber, barcode));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // SEW-04: Digital Andon Board Live Feed
    [HttpGet("andon-feed")]
    public async Task<IActionResult> GetAndonBoardFeed([FromQuery] string lineNumber = "LINE-04")
    {
        var result = await _mediator.Send(new GetAndonBoardFeedQuery(lineNumber));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // SEW-05: Machine Assets & MTTR/MTBF
    [HttpGet("machines")]
    public async Task<IActionResult> GetSewingMachines([FromQuery] string? lineNumber, [FromQuery] GarmentsERP.Production.Domain.Enums.MachineStatus? status)
    {
        var result = await _mediator.Send(new GetSewingMachinesQuery(lineNumber, status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("machines")]
    public async Task<IActionResult> CreateSewingMachine([FromBody] CreateSewingMachineAssetDto dto)
    {
        var result = await _mediator.Send(new CreateSewingMachineAssetCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // SEW-05: Machine Breakdown Tickets
    [HttpGet("breakdown-tickets")]
    public async Task<IActionResult> GetBreakdownTickets([FromQuery] string? lineNumber, [FromQuery] bool? isResolved)
    {
        var result = await _mediator.Send(new GetBreakdownTicketsQuery(lineNumber, isResolved));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("breakdown-tickets")]
    public async Task<IActionResult> CreateBreakdownTicket([FromBody] CreateMachineBreakdownTicketDto dto)
    {
        var result = await _mediator.Send(new CreateMachineBreakdownTicketCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("breakdown-tickets/{id}/resolve")]
    public async Task<IActionResult> ResolveBreakdownTicket(Guid id, [FromBody] ResolveBreakdownTicketDto dto)
    {
        var result = await _mediator.Send(new ResolveBreakdownTicketCommand(id, dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    // SEW-06: Broken Needle Logs & Compliance Gate
    [HttpGet("broken-needle-logs")]
    public async Task<IActionResult> GetBrokenNeedleLogs([FromQuery] string? lineNumber, [FromQuery] GarmentsERP.Production.Domain.Enums.NeedleComplianceStatus? status)
    {
        var result = await _mediator.Send(new GetBrokenNeedleLogsQuery(lineNumber, status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("broken-needle-logs/{id}")]
    public async Task<IActionResult> GetBrokenNeedleLogById(Guid id)
    {
        var result = await _mediator.Send(new GetBrokenNeedleLogByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    [HttpPost("broken-needle-logs")]
    public async Task<IActionResult> CreateBrokenNeedleLog([FromBody] CreateBrokenNeedleLogDto dto)
    {
        var result = await _mediator.Send(new CreateBrokenNeedleLogCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("broken-needle-logs/{id}/verify-and-issue")]
    public async Task<IActionResult> VerifyNeedleFragments(Guid id, [FromBody] VerifyNeedleFragmentsDto dto)
    {
        var result = await _mediator.Send(new VerifyNeedleFragmentsCommand(id, dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
