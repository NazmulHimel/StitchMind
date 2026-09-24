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
public class BuyerOrdersController : ControllerBase
{
    private readonly IMediator _mediator;

    public BuyerOrdersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetBuyerOrders([FromQuery] OrderStatus? status)
    {
        var result = await _mediator.Send(new GetBuyerOrdersQuery(status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetProductionMetrics()
    {
        var result = await _mediator.Send(new GetProductionMetricsQuery());
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPost]
    public async Task<IActionResult> CreateBuyerOrder([FromBody] CreateBuyerOrderDto dto)
    {
        var result = await _mediator.Send(new CreateBuyerOrderCommand(dto));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    [HttpPut("{id}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] OrderStatus status)
    {
        var result = await _mediator.Send(new UpdateOrderStatusCommand(id, status));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }
}
