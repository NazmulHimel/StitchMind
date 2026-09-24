using GarmentsERP.Production.Application.Commands;
using GarmentsERP.Production.Application.DTOs;
using GarmentsERP.Production.Application.Queries;
using GarmentsERP.Production.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/quality")]
public class QualityControlController : ControllerBase
{
    private readonly IMediator _mediator;

    public QualityControlController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("inspections")]
    public async Task<IActionResult> GetInspections([FromQuery] InspectionType? type)
    {
        var result = await _mediator.Send(new GetQualityInspectionsQuery(type));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("inspections/create")]
    public async Task<IActionResult> CreateInspection([FromBody] CreateQualityInspectionDto dto)
    {
        var result = await _mediator.Send(new CreateQualityInspectionCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
