using GarmentsERP.Finance.Application.Queries;
using GarmentsERP.HR.Application.Queries;
using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Production.Application.Interfaces;
using GarmentsERP.Production.Application.Queries;
using GarmentsERP.Production.Domain.Entities;
using GarmentsERP.Shared.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReportsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IProductionRepository _productionRepo;
    private readonly IProcurementRepository _procurementRepo;

    public ReportsController(
        IMediator mediator,
        IProductionRepository productionRepo,
        IProcurementRepository procurementRepo)
    {
        _mediator = mediator;
        _productionRepo = productionRepo;
        _procurementRepo = procurementRepo;
    }

    [HttpGet("financial")]
    public async Task<IActionResult> GetFinancialReport([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
    {
        var trialBalance = await _mediator.Send(new GetTrialBalanceQuery(toDate));
        var incomeStatement = await _mediator.Send(new GetIncomeStatementQuery(fromDate, toDate));
        var balanceSheet = await _mediator.Send(new GetBalanceSheetQuery(toDate));
        var metrics = await _mediator.Send(new GetFinanceMetricsQuery());

        return Ok(Result<object>.Success(new
        {
            ReportTitle = "Comprehensive Garments Financial Statement Report",
            GeneratedAt = DateTime.UtcNow,
            FromDate = fromDate,
            ToDate = toDate ?? DateTime.UtcNow,
            TrialBalance = trialBalance.Value,
            IncomeStatement = incomeStatement.Value,
            BalanceSheet = balanceSheet.Value,
            Metrics = metrics.Value
        }));
    }

    [HttpGet("production-summary")]
    public async Task<IActionResult> GetProductionSummaryReport()
    {
        var orders = await _mediator.Send(new GetBuyerOrdersQuery());
        var cutting = await _mediator.Send(new GetCuttingOrdersQuery());
        var sewing = await _mediator.Send(new GetHourlyProductionLogsQuery());
        var qc = await _mediator.Send(new GetQualityInspectionsQuery());
        var dispatches = (await _productionRepo.GetDeliveryChallansAsync(default(ChallanStatus?), default)).ToList();

        return Ok(Result<object>.Success(new
        {
            ReportTitle = "Enterprise Garment Factory Production & Dispatch Summary",
            GeneratedAt = DateTime.UtcNow,
            TotalOrders = orders.Value?.Count ?? 0,
            CuttingBatches = cutting.Value?.Count ?? 0,
            SewingLogsCount = sewing.Value?.Count ?? 0,
            QcInspectionsCount = qc.Value?.Count ?? 0,
            TotalDispatchedChallans = dispatches.Count,
            Orders = orders.Value,
            CuttingOrders = cutting.Value,
            HourlyProduction = sewing.Value,
            QualityInspections = qc.Value,
            Dispatches = dispatches
        }));
    }

    [HttpGet("payroll-register")]
    public async Task<IActionResult> GetPayrollRegisterReport([FromQuery] int? month, [FromQuery] int? year)
    {
        var targetMonth = month ?? DateTime.UtcNow.Month;
        var targetYear = year ?? DateTime.UtcNow.Year;

        var payroll = await _mediator.Send(new GetPayrollRecordsQuery(targetMonth, targetYear));
        var employees = await _mediator.Send(new GetEmployeesQuery());
        var leaves = await _mediator.Send(new GetLeaveRequestsQuery());

        decimal totalBasic = payroll.Value?.Sum(p => p.BasicSalary) ?? 0;
        decimal totalOvertime = payroll.Value?.Sum(p => p.OvertimeAmount) ?? 0;
        decimal totalDeductions = payroll.Value?.Sum(p => p.TotalDeductions) ?? 0;
        decimal totalNet = payroll.Value?.Sum(p => p.NetPayable) ?? 0;

        return Ok(Result<object>.Success(new
        {
            ReportTitle = $"Factory Workforce Monthly Payroll & Attendance Register ({targetYear}-{targetMonth:D2})",
            Month = targetMonth,
            Year = targetYear,
            GeneratedAt = DateTime.UtcNow,
            TotalEmployees = employees.Value?.Count ?? 0,
            TotalBasicPay = totalBasic,
            TotalOvertimePay = totalOvertime,
            TotalDeductions = totalDeductions,
            TotalNetDisbursed = totalNet,
            PayrollRecords = payroll.Value,
            Employees = employees.Value,
            LeaveSummary = leaves.Value
        }));
    }

    [HttpGet("procurement-summary")]
    public async Task<IActionResult> GetProcurementSummaryReport()
    {
        var companyId = Guid.Empty;
        var pos = await _procurementRepo.GetPurchaseOrdersAsync(companyId);
        var grns = await _procurementRepo.GetGRNsAsync(companyId);
        var suppliers = await _procurementRepo.GetSuppliersAsync(companyId);

        return Ok(Result<object>.Success(new
        {
            ReportTitle = "Supply Chain & Procurement Valuation Summary",
            GeneratedAt = DateTime.UtcNow,
            TotalSuppliers = suppliers?.Count ?? 0,
            TotalPurchaseOrders = pos?.Count ?? 0,
            TotalGRNs = grns?.Count ?? 0,
            PurchaseOrders = pos,
            GoodsReceiptNotes = grns,
            Suppliers = suppliers
        }));
    }
}
