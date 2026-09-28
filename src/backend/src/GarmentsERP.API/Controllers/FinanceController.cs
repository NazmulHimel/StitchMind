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

    // ACC-02: Three-Way Matching & Vendor Bills (AC-06)
    [HttpGet("bills")]
    public async Task<IActionResult> GetVendorBills()
    {
        var result = await _mediator.Send(new GetVendorBillsQuery());
        return Ok(result);
    }

    [HttpGet("bills/{id:guid}")]
    public async Task<IActionResult> GetVendorBillById(Guid id)
    {
        var result = await _mediator.Send(new GetVendorBillByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("bills")]
    public async Task<IActionResult> CreateVendorBill([FromBody] CreateVendorBillDto dto)
    {
        var result = await _mediator.Send(new CreateVendorBillCommand(dto));
        if (!result.IsSuccess) return BadRequest(result);
        return Created($"/api/finance/bills/{result.Value?.Id}", result);
    }

    [HttpPost("bills/{id:guid}/three-way-match")]
    public async Task<IActionResult> ExecuteThreeWayMatch(Guid id, [FromBody] ExecuteThreeWayMatchRequest request)
    {
        var result = await _mediator.Send(new ExecuteThreeWayMatchCommand(id, request.POUnitPrice, request.GRNPassedQuantity, request.EvaluatedBy));
        if (!result.IsSuccess) return BadRequest(result);
        return Ok(result);
    }

    // ACC-03: Commercial Trade & LCs
    [HttpGet("master-lcs")]
    public async Task<IActionResult> GetMasterLCs()
    {
        var result = await _mediator.Send(new GetMasterLCsQuery());
        return Ok(result);
    }

    [HttpGet("master-lcs/{id:guid}")]
    public async Task<IActionResult> GetMasterLCById(Guid id)
    {
        var result = await _mediator.Send(new GetMasterLCByIdQuery(id));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    [HttpPost("master-lcs")]
    public async Task<IActionResult> CreateMasterLC([FromBody] CreateMasterLCDto dto)
    {
        var result = await _mediator.Send(new CreateMasterLCCommand(dto));
        if (!result.IsSuccess) return BadRequest(result);
        return Created($"/api/finance/master-lcs/{result.Value?.Id}", result);
    }

    [HttpGet("b2b-lcs")]
    public async Task<IActionResult> GetBackToBackLCs([FromQuery] Guid? masterLCId)
    {
        var result = await _mediator.Send(new GetBackToBackLCsQuery(masterLCId));
        return Ok(result);
    }

    [HttpPost("b2b-lcs")]
    public async Task<IActionResult> CreateBackToBackLC([FromBody] CreateBackToBackLCDto dto)
    {
        var result = await _mediator.Send(new CreateBackToBackLCCommand(dto));
        if (!result.IsSuccess) return BadRequest(result);
        return Created($"/api/finance/b2b-lcs/{result.Value?.Id}", result);
    }

    [HttpGet("fdbp")]
    public async Task<IActionResult> GetForeignBillDiscountings([FromQuery] Guid? masterLCId)
    {
        var result = await _mediator.Send(new GetForeignBillDiscountingsQuery(masterLCId));
        return Ok(result);
    }

    [HttpPost("fdbp")]
    public async Task<IActionResult> CreateForeignBillDiscounting([FromBody] CreateForeignBillDiscountingDto dto)
    {
        var result = await _mediator.Send(new CreateForeignBillDiscountingCommand(dto));
        if (!result.IsSuccess) return BadRequest(result);
        return Created($"/api/finance/fdbp/{result.Value?.Id}", result);
    }

    // ACC-04: Order Costing Variances
    [HttpGet("costing-variances")]
    public async Task<IActionResult> GetOrderCostingVariances()
    {
        var result = await _mediator.Send(new GetOrderCostingVariancesQuery());
        return Ok(result);
    }

    [HttpGet("costing-variances/{styleId:guid}")]
    public async Task<IActionResult> GetOrderCostingVarianceByStyleId(Guid styleId)
    {
        var result = await _mediator.Send(new GetOrderCostingVarianceByStyleIdQuery(styleId));
        if (!result.IsSuccess) return NotFound(result);
        return Ok(result);
    }

    // ACC-05: Multi-Currency & Forex
    [HttpGet("exchange-rates")]
    public async Task<IActionResult> GetExchangeRates()
    {
        var result = await _mediator.Send(new GetExchangeRatesQuery());
        return Ok(result);
    }

    [HttpPost("calculate-forex")]
    public async Task<IActionResult> CalculateForexGainLoss([FromBody] CalculateForexRequest req)
    {
        var result = await _mediator.Send(new CalculateForexGainLossQuery(req.Amount, req.Currency, req.BookingRate, req.SettlementRate));
        return Ok(result);
    }

    // ACC-06: Fixed Assets Register
    [HttpGet("fixed-assets")]
    public async Task<IActionResult> GetFixedAssets()
    {
        var result = await _mediator.Send(new GetFixedAssetsQuery());
        return Ok(result);
    }

    // ACC-07: AP/AR Aging Analysis
    [HttpGet("aging-analysis")]
    public async Task<IActionResult> GetAgingAnalysis([FromQuery] string? type)
    {
        var result = await _mediator.Send(new GetAgingAnalysisQuery(type ?? "AccountsPayable"));
        return Ok(result);
    }
}

public record CalculateForexRequest(
    decimal Amount,
    string Currency,
    decimal BookingRate,
    decimal SettlementRate
);

