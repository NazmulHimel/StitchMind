using MediatR;
using Microsoft.AspNetCore.Mvc;
using GarmentsERP.HR.Application.Commands;
using GarmentsERP.HR.Application.Queries;
using GarmentsERP.HR.Domain.Entities;
using GarmentsERP.HR.Domain.Enums;
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

    // 1. Digital Service Book & AC-07 Child Labor Age Gate
    [HttpGet("employees")]
    public async Task<IActionResult> GetEmployees([FromQuery] string? department)
    {
        var result = await _mediator.Send(new GetEmployeesQuery(department));
        return Ok(result);
    }

    [HttpGet("employees/{id}")]
    public async Task<IActionResult> GetEmployeeById(Guid id)
    {
        var result = await _mediator.Send(new GetEmployeeByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
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

    // 2. Attendance & Biometric TCP/IP Listener (HR-02)
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

    [HttpGet("biometric/devices")]
    public async Task<IActionResult> GetBiometricDevices()
    {
        var result = await _mediator.Send(new GetBiometricDevicesQuery());
        return Ok(result);
    }

    [HttpGet("biometric/punches")]
    public async Task<IActionResult> GetBiometricPunches([FromQuery] DateTime? date, [FromQuery] Guid? deviceId)
    {
        var result = await _mediator.Send(new GetBiometricPunchLogsQuery(date, deviceId));
        return Ok(result);
    }

    [HttpPost("biometric/sync")]
    public async Task<IActionResult> SyncDeviceLogs([FromBody] SyncBiometricDeviceLogsCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // 3. Work Shifts, Dynamic Rosters & Penalties (HR-03)
    [HttpGet("shifts")]
    public async Task<IActionResult> GetShifts()
    {
        var result = await _mediator.Send(new GetWorkShiftsQuery());
        return Ok(result);
    }

    [HttpGet("roster")]
    public async Task<IActionResult> GetShiftRosters([FromQuery] Guid? employeeId, [FromQuery] DateTime? date)
    {
        var result = await _mediator.Send(new GetShiftRostersQuery(employeeId, date));
        return Ok(result);
    }

    // 4. Leaves, Balances & Maternity Benefits (HR-04)
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

    [HttpGet("leaves/balance/{employeeId}")]
    public async Task<IActionResult> GetLeaveBalance(Guid employeeId, [FromQuery] int? year)
    {
        int targetYear = year ?? DateTime.UtcNow.Year;
        var result = await _mediator.Send(new GetLeaveBalanceQuery(employeeId, targetYear));
        return Ok(result);
    }

    [HttpPost("leaves/encashment")]
    public async Task<IActionResult> EncashEarnedLeave([FromBody] CalculateEarnedLeaveEncashmentCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("compliance/maternity")]
    public async Task<IActionResult> GetMaternityBenefits()
    {
        var result = await _mediator.Send(new GetMaternityBenefitsQuery());
        return Ok(result);
    }

    [HttpPost("compliance/maternity")]
    public async Task<IActionResult> CreateMaternityBenefit([FromBody] CalculateMaternityBenefitCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // 5. Dual-Stream Payroll & MFS Export (HR-05 & HR-06)
    [HttpGet("payroll")]
    public async Task<IActionResult> GetPayroll([FromQuery] int? month, [FromQuery] int? year)
    {
        var result = await _mediator.Send(new GetPayrollRecordsQuery(month, year));
        return Ok(result);
    }

    [HttpGet("payroll/sheets")]
    public async Task<IActionResult> GetPayrollSheets()
    {
        var result = await _mediator.Send(new GetPayrollSheetsQuery());
        return Ok(result);
    }

    [HttpGet("payroll/sheets/{id}")]
    public async Task<IActionResult> GetPayrollSheetById(Guid id)
    {
        var result = await _mediator.Send(new GetPayrollSheetByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("payroll/generate-dual-stream")]
    public async Task<IActionResult> GenerateDualStreamPayroll([FromBody] GenerateDualStreamPayrollCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
            return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("payroll/sheets/{id}/disburse")]
    public async Task<IActionResult> DisbursePayrollSheet(Guid id, [FromBody] DisbursePayrollRequest req)
    {
        var result = await _mediator.Send(new DisbursePayrollSheetCommand(id, req.ApprovedBy));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("payroll/export/{sheetId}/{channel}")]
    public async Task<IActionResult> ExportMfsBatch(Guid sheetId, PaymentDisbursementChannel channel)
    {
        var result = await _mediator.Send(new ExportMfsPayoutBatchQuery(sheetId, channel));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // 6. Clinic Logs, Grievance Box & Disciplinary Actions (HR-07)
    [HttpGet("incidents/clinic")]
    public async Task<IActionResult> GetClinicVisits([FromQuery] DateTime? fromDate, [FromQuery] Guid? employeeId)
    {
        var result = await _mediator.Send(new GetClinicVisitsQuery(fromDate, employeeId));
        return Ok(result);
    }

    [HttpPost("incidents/clinic")]
    public async Task<IActionResult> LogClinicVisit([FromBody] LogClinicVisitCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("incidents/grievances")]
    public async Task<IActionResult> GetWorkerGrievances([FromQuery] GrievanceStatus? status)
    {
        var result = await _mediator.Send(new GetWorkerGrievancesQuery(status));
        return Ok(result);
    }

    [HttpPost("incidents/grievances")]
    public async Task<IActionResult> SubmitWorkerGrievance([FromBody] SubmitWorkerGrievanceCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("incidents/grievances/{id}/resolve")]
    public async Task<IActionResult> ResolveWorkerGrievance(Guid id, [FromBody] ResolveGrievanceRequest req)
    {
        var result = await _mediator.Send(new ResolveWorkerGrievanceCommand(
            id,
            req.InvestigatedBy,
            req.WelfareCommitteeFindings,
            req.ResolutionSummary,
            req.IsResolved
        ));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpGet("incidents/disciplinary")]
    public async Task<IActionResult> GetDisciplinaryActions([FromQuery] Guid? employeeId)
    {
        var result = await _mediator.Send(new GetDisciplinaryActionsQuery(employeeId));
        return Ok(result);
    }

    [HttpPost("incidents/disciplinary")]
    public async Task<IActionResult> RecordDisciplinaryAction([FromBody] RecordDisciplinaryActionCommand command)
    {
        var result = await _mediator.Send(command);
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }
}

public record ApproveLeaveRequest(bool IsApproved, string? ApproverId, string? RejectionReason);
public record DisbursePayrollRequest(string ApprovedBy);
public record ResolveGrievanceRequest(string InvestigatedBy, string WelfareCommitteeFindings, string ResolutionSummary, bool IsResolved);
