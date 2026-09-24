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
public class CuttingController : ControllerBase
{
    private readonly IMediator _mediator;

    public CuttingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("orders")]
    public async Task<IActionResult> GetCuttingOrders([FromQuery] CuttingStatus? status)
    {
        var result = await _mediator.Send(new GetCuttingOrdersQuery(status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateCuttingOrder([FromBody] CreateCuttingOrderDto dto)
    {
        var result = await _mediator.Send(new CreateCuttingOrderCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("bundles")]
    public async Task<IActionResult> GetCutBundles([FromQuery] Guid? cuttingOrderId, [FromQuery] BundleSection? section)
    {
        var result = await _mediator.Send(new GetCutBundlesQuery(cuttingOrderId, section));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost("orders/{id}/generate-bundles")]
    public async Task<IActionResult> GenerateBundles(Guid id, [FromQuery] int bundleSize = 50)
    {
        var result = await _mediator.Send(new GenerateBundlesCommand(id, bundleSize));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("bundles/{id}/transfer")]
    public async Task<IActionResult> TransferBundle(Guid id, [FromBody] BundleSection nextSection)
    {
        var result = await _mediator.Send(new UpdateBundleSectionCommand(id, nextSection));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
