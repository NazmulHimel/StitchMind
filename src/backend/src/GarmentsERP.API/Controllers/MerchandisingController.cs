using GarmentsERP.Production.Infrastructure.Data;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Domain;
using GarmentsERP.Shared.Extensions;
using GarmentsERP.Shared.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MerchandisingController : ControllerBase
{
    private readonly ProductionDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IAuditTrailStore _auditTrailStore;

    public MerchandisingController(
        ProductionDbContext dbContext,
        ICurrentUserService currentUserService,
        IAuditTrailStore auditTrailStore)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _auditTrailStore = auditTrailStore;
    }

    /// <summary>
    /// Retrieves all active Buyer Styles with BOM item counts and Pre-Costing budget caps.
    /// Demonstrates data-level price masking for shop-floor users (AC-02 / SEC-03).
    /// </summary>
    [HttpGet("styles")]
    public async Task<IActionResult> GetStyles(CancellationToken ct)
    {
        var canViewCosts = _currentUserService.CanViewFinancialCosts;

        var orders = await _dbContext.BuyerOrders
            .Include(o => o.Buyer)
            .Include(o => o.ColorSizes)
            .Include(o => o.BOMItems)
            .Include(o => o.PreCosting)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync(ct);

        var response = orders.Select(o => new
        {
            o.Id,
            o.OrderNumber,
            BuyerName = o.Buyer?.Name ?? "Global Buyer",
            BuyerCode = o.Buyer?.Code ?? "BUYER",
            o.StyleNumber,
            o.StyleName,
            o.ProductCategory,
            o.FabricComposition,
            o.Season,
            o.Currency,
            o.OrderQuantity,
            o.DeliveryDate,
            Status = o.Status.ToString(),
            BOMCount = o.BOMItems.Count,
            ColorwaysCount = o.ColorSizes.Select(cs => cs.ColorName).Distinct().Count(),
            SizesCount = o.ColorSizes.Select(cs => cs.SizeLabel).Distinct().Count(),

            // Shop-floor Financial Price Masking (AC-02)
            IsFinancialDataMasked = !canViewCosts,
            UnitPrice = o.UnitPrice.MaskFinancialCost(canViewCosts),
            UnitPriceFormatted = o.UnitPrice.MaskFinancialFormatted(canViewCosts, o.Currency == "USD" ? "$" : "৳"),
            FOBPricePerDzn = o.FOBPricePerDzn.MaskFinancialCost(canViewCosts),
            FOBPricePerDznFormatted = o.FOBPricePerDzn.MaskFinancialFormatted(canViewCosts, o.Currency == "USD" ? "$" : "৳"),
            TotalOrderValue = o.TotalValue.MaskFinancialCost(canViewCosts),
            TotalOrderValueFormatted = o.TotalValue.MaskFinancialFormatted(canViewCosts, o.Currency == "USD" ? "$" : "৳"),
            MaterialsBudgetPerDzn = o.PreCosting?.TotalMaterialsBudget.MaskFinancialCost(canViewCosts),
            MaterialsBudgetFormatted = o.PreCosting?.TotalMaterialsBudget.MaskFinancialFormatted(canViewCosts, o.Currency == "USD" ? "$" : "৳"),
            IsBudgetApproved = o.PreCosting?.IsBudgetApproved ?? false
        }).ToList();

        return Ok(Result<object>.Success(new
        {
            CanViewFinancialCosts = canViewCosts,
            StylesCount = response.Count,
            Styles = response
        }));
    }

    /// <summary>
    /// Retrieves complete Style dossier including BOM, Color/Size breakdown, and Pre-Costing budget sheet.
    /// </summary>
    [HttpGet("styles/{id:guid}")]
    public async Task<IActionResult> GetStyleById(Guid id, CancellationToken ct)
    {
        var canViewCosts = _currentUserService.CanViewFinancialCosts;

        var order = await _dbContext.BuyerOrders
            .Include(o => o.Buyer)
            .Include(o => o.ColorSizes)
            .Include(o => o.BOMItems)
            .Include(o => o.PreCosting!)
                .ThenInclude(pc => pc.Lines)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order == null)
        {
            return NotFound(Result<string>.Failure("Style order not found."));
        }

        var result = new
        {
            order.Id,
            order.OrderNumber,
            Buyer = new
            {
                order.Buyer?.Id,
                order.Buyer?.Name,
                order.Buyer?.Code,
                order.Buyer?.Country,
                order.Buyer?.PaymentTerms
            },
            order.StyleNumber,
            order.StyleName,
            order.ProductCategory,
            order.FabricComposition,
            order.Season,
            order.Currency,
            order.OrderQuantity,
            order.DeliveryDate,
            Status = order.Status.ToString(),

            // Price Masking (AC-02)
            IsFinancialDataMasked = !canViewCosts,
            UnitPrice = order.UnitPrice.MaskFinancialCost(canViewCosts),
            UnitPriceFormatted = order.UnitPrice.MaskFinancialFormatted(canViewCosts),
            FOBPricePerDzn = order.FOBPricePerDzn.MaskFinancialCost(canViewCosts),
            FOBPricePerDznFormatted = order.FOBPricePerDzn.MaskFinancialFormatted(canViewCosts),
            TotalOrderValue = order.TotalValue.MaskFinancialCost(canViewCosts),
            TotalOrderValueFormatted = order.TotalValue.MaskFinancialFormatted(canViewCosts),

            // Color / Size Matrix
            ColorSizes = order.ColorSizes.Select(cs => new
            {
                cs.Id,
                cs.ColorName,
                cs.PantoneCode,
                cs.SizeLabel,
                cs.Quantity
            }).ToList(),

            // Bill of Materials (BOM)
            BOMItems = order.BOMItems.Select(b => new
            {
                b.Id,
                b.ItemCategory,
                b.ItemCode,
                b.ItemDescription,
                b.UnitOfMeasure,
                b.NetConsumptionPerDzn,
                b.WastagePercentage,
                b.GrossConsumptionPerDzn,
                b.TotalRequiredQty
            }).ToList(),

            // Pre-Costing Budget Sheet
            PreCosting = order.PreCosting == null ? null : new
            {
                order.PreCosting.Id,
                order.PreCosting.CostingVersion,
                order.PreCosting.CostingBasis,
                order.PreCosting.IsBudgetApproved,
                order.PreCosting.ApprovedAt,
                FOBPricePerDzn = order.PreCosting.FOBPricePerDzn.MaskFinancialCost(canViewCosts),
                TotalMaterialsBudget = order.PreCosting.TotalMaterialsBudget.MaskFinancialCost(canViewCosts),
                TotalLaborCostPerDzn = order.PreCosting.TotalLaborCostPerDzn.MaskFinancialCost(canViewCosts),
                TotalOverheadPerDzn = order.PreCosting.TotalOverheadPerDzn.MaskFinancialCost(canViewCosts),
                NetMarginPerDzn = order.PreCosting.NetMarginPerDzn.MaskFinancialCost(canViewCosts),
                Lines = order.PreCosting.Lines.Select(l => new
                {
                    l.Id,
                    l.Category,
                    l.ItemDescription,
                    l.ConsumptionPerDzn,
                    UnitRate = l.UnitRate.MaskFinancialCost(canViewCosts),
                    UnitRateFormatted = l.UnitRate.MaskFinancialFormatted(canViewCosts),
                    TotalBudgetPerDzn = l.TotalBudgetPerDzn.MaskFinancialCost(canViewCosts),
                    TotalBudgetFormatted = l.TotalBudgetPerDzn.MaskFinancialFormatted(canViewCosts)
                }).ToList()
            }
        };

        return Ok(Result<object>.Success(result));
    }

    /// <summary>
    /// Approves the Pre-Costing budget cap for an active style, locking maximum allowable material purchase rates (AC-03 / SEC-04).
    /// </summary>
    [HttpPost("styles/{id:guid}/pre-costing/approve")]
    public async Task<IActionResult> ApprovePreCostingBudget(Guid id, CancellationToken ct)
    {
        var order = await _dbContext.BuyerOrders
            .Include(o => o.PreCosting)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

        if (order?.PreCosting == null)
        {
            return NotFound(Result<string>.Failure("Pre-Costing sheet not found for style."));
        }

        var approverId = _currentUserService.UserId != Guid.Empty
            ? _currentUserService.UserId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");

        order.PreCosting.ApproveBudget(approverId);
        await _dbContext.SaveChangesAsync(ct);

        // Record immutable audit trail (SEC-05)
        var auditLog = AuditTrailLog.Create(
            entityName: "PreCosting",
            entityId: order.StyleNumber,
            actionType: "BUDGET_APPROVED",
            userId: approverId,
            userName: _currentUserService.UserName,
            userIpAddress: _currentUserService.IpAddress,
            oldValuesJson: "{\"IsBudgetApproved\": false}",
            newValuesJson: $"{{\"IsBudgetApproved\": true, \"ApprovedAt\": \"{DateTime.UtcNow:O}\", \"MaterialsBudget\": {order.PreCosting.TotalMaterialsBudget}}}"
        );
        await _auditTrailStore.RecordAuditLogAsync(auditLog, ct);

        return Ok(Result<string>.Success($"Pre-costing budget cap for Style {order.StyleNumber} has been approved and locked."));
    }
}
