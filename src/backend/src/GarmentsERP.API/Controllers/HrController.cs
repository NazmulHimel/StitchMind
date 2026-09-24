using MediatR;
using Microsoft.AspNetCore.Mvc;
using GarmentsERP.HR.Application.Commands;
using GarmentsERP.HR.Application.Queries;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.Shared.Common;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HrController : ControllerBase
{
    private readonly IMediator _mediator;

    public HrController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? department)
    {
        var result = await _mediator.Send(new GetEmployeesQuery(department));
        return Ok(result);
    }

    [HttpPost("employees")]
    public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Created($"/api/hr/employees/{result.Value?.Id}", result);
    }

    [HttpGet("attendance")]
    public async Task<IActionResult> GetAttendance([FromQuery] DateTime? date, [FromQuery] Guid? employeeId)
    {
        var result = await _mediator.Send(new GetAttendanceLogsQuery(date, employeeId));
        return Ok(result);
    }

    [HttpPost("attendance/punch")]
    public async Task<IActionResult> RecordPunch([FromBody] RecordAttendancePunchCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("leaves")]
    public async Task<IActionResult> GetLeaves([FromQuery] LeaveStatus? status)
    {
        var result = await _mediator.Send(new GetLeaveRequestsQuery(status));
        return Ok(result);
    }

    [HttpPost("leaves")]
    public async Task<IActionResult> RequestLeave([FromBody] RequestLeaveCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("leaves/{id}/approve")]
    public async Task<IActionResult> ApproveLeave(Guid id, [FromBody] ApproveLeaveRequest req)
    {
        var result = await _mediator.Send(new ApproveLeaveCommand(id, req.IsApproved, req.ApproverId, req.RejectionReason));
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("payroll")]
    public async Task<IActionResult> GetPayroll([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetPayrollRecordsQuery(month, year));
        return Ok(result);
    }

    [HttpPost("payroll/generate")]
    public async Task<IActionResult> GeneratePayroll([FromBody] GenerateMonthlyPayrollCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }
}

public record ApproveLeaveRequest(bool IsApproved, string? ApproverId, string? RejectionReason);
