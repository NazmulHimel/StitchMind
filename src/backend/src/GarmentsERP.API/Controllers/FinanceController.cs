using GarmentsERP.Finance.Application.Commands;
using GarmentsERP.Finance.Application.DTOs;
using GarmentsERP.Finance.Application.Queries;
using GarmentsERP.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FinanceController : ControllerBase
{
    private readonly IMediator _mediator;

    public FinanceController(IMediator mediator)
    {
        _mediator = mediator;
    }

    // Accounts
    [HttpGet("accounts")]
    public async Task<IActionResult> GetAccounts()
    {
        var result = await _mediator.Send(new GetAccountsQuery());
        return Ok(result);
    }

    [HttpGet("accounts/{id:guid}")]
    public async Task<IActionResult> GetAccountById(Guid id)
    {
        var result = await _mediator.Send(new GetAccountByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("accounts")]
    public async Task<IActionResult> CreateAccount([FromBody] CreateAccountDto dto)
    {
        var result = await _mediator.Send(new CreateAccountCommand(dto));
        if (!result.IsSuccess) return BadRequest(result);
        return Created($"/api/finance/accounts/{result.Value?.Id}", result);
    }

    // Journal Vouchers
    [HttpGet("vouchers")]
    public async Task<IActionResult> GetJournalVouchers()
    {
        var result = await _mediator.Send(new GetJournalVouchersQuery());
        return Ok(result);
    }

    [HttpGet("vouchers/{id:guid}")]
    public async Task<IActionResult> GetJournalVoucherById(Guid id)
    {
        var result = await _mediator.Send(new GetJournalVoucherByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("vouchers")]
    public async Task<IActionResult> CreateJournalVoucher([FromBody] CreateJournalVoucherDto dto)
    {
        var result = await _mediator.Send(new CreateJournalVoucherCommand(dto));
        if (!result.IsSuccess) return BadRequest(result);
        return Created($"/api/finance/vouchers/{result.Value?.Id}", result);
    }

    [HttpPost("vouchers/{id:guid}/post")]
    public async Task<IActionResult> PostJournalVoucher(Guid id, [FromQuery] string? postedBy)
    {
        var result = await _mediator.Send(new PostJournalVoucherCommand(id, postedBy ?? "Admin"));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    [HttpPost("vouchers/{id:guid}/cancel")]
    public async Task<IActionResult> CancelJournalVoucher(Guid id)
    {
        var result = await _mediator.Send(new CancelJournalVoucherCommand(id));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // Reports & Statements
    [HttpGet("ledger")]
    public async Task<IActionResult> GetGeneralLedger([FromQuery] Guid? accountId, [FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _mediator.Send(new GetGeneralLedgerQuery(accountId, fromDate, toDate));
        return Ok(result);
    }

    [HttpGet("trial-balance")]
    public async Task<IActionResult> GetTrialBalance([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetTrialBalanceQuery(asOfDate));
        return Ok(result);
    }

    [HttpGet("income-statement")]
    public async Task<IActionResult> GetIncomeStatement([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var result = await _mediator.Send(new GetIncomeStatementQuery(fromDate, toDate));
        return Ok(result);
    }

    [HttpGet("balance-sheet")]
    public async Task<IActionResult> GetBalanceSheet([FromQuery] DateTime? asOfDate)
    {
        var result = await _mediator.Send(new GetBalanceSheetQuery(asOfDate));
        return Ok(result);
    }

    [HttpGet("metrics")]
    public async Task<IActionResult> GetMetrics()
    {
        var result = await _mediator.Send(new GetFinanceMetricsQuery());
        return Ok(result);
    }
}
