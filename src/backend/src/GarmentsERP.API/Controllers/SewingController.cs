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
}
