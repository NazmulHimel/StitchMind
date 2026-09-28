using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Infrastructure.Data;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/quarantine")]
public class QuarantineController(
    InventoryDbContext db,
    ICurrentUserService currentUserService,
    IAuditTrailStore? auditTrailStore = null) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet("lots")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStockLots([FromQuery] string? state, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var query = db.StockLots.Where(l => l.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(state))
        {
            query = query.Where(l => l.StockState == state.Trim());
        }

        var lots = await query.OrderByDescending(l => l.CreatedAt).ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<StockLot>>.Ok(lots));
    }

    /// <summary>
    /// AC-04 Mandatory Quarantine Gatekeeper: Blocks fabric cutting floor allocation if QC inspection has not passed.
    /// </summary>
    [HttpPost("lots/{id:guid}/verify-cutting-release")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyCuttingRelease(Guid id, CancellationToken ct)
    {
        var lot = await db.StockLots.FirstOrDefaultAsync(l => l.Id == id, ct);
        if (lot == null) return NotFound(ApiResponse<object>.Fail("Stock lot not found."));

        var (isAllowed, errorMessage) = lot.VerifyAvailableForCutting();
        if (!isAllowed)
        {
            return BadRequest(ApiResponse<object>.Fail(errorMessage));
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            lot.Id,
            lot.LotBatchNumber,
            lot.StockState,
            lot.QCInspectionStatus,
            lot.QCPassedAtUtc,
            Status = "RELEASED_FOR_PRODUCTION",
            Message = "Fabric roll lot is verified and authorized for cutting table spreading."
        }));
    }

    [HttpGet("inspections")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInspections(CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var inspections = await db.FabricInspections
            .Include(i => i.StockLot)
            .Where(i => i.CompanyId == companyId)
            .OrderByDescending(i => i.InspectionDate)
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<FabricInspection>>.Ok(inspections));
    }

    /// <summary>
    /// Evaluates ASTM D5430-07 4-Point System scoring and executes Quarantine Release or Rejection.
    /// </summary>
    [HttpPost("inspections/evaluate")]
    [AllowAnonymous]
    public async Task<IActionResult> EvaluateInspection([FromBody] EvaluateFabricInspectionRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var lot = await db.StockLots.FirstOrDefaultAsync(l => l.Id == request.StockLotId, ct);
        if (lot == null) return NotFound(ApiResponse<object>.Fail("Stock lot not found."));

        var inspectorName = !string.IsNullOrWhiteSpace(request.InspectorName)
            ? request.InspectorName
            : currentUserService.UserName ?? "Mahmud Hasan (QC Lead)";

        var inspection = new FabricInspection
        {
            InspectionNumber = $"QC-4PT-{DateTime.UtcNow:yyyyMM}-{Random.Shared.Next(1000, 9999)}",
            StockLotId = lot.Id,
            InspectorName = inspectorName,
            InspectionDate = DateTime.UtcNow,
            TotalRollsInLot = request.TotalRollsInLot,
            RollsInspected = request.RollsInspected,
            CompanyId = companyId
        };

        inspection.EvaluateInspection(request.TotalPenaltyPoints, request.TotalYardsInspected, request.CuttableWidthInches, lot);

        await db.FabricInspections.AddAsync(inspection, ct);
        await db.SaveChangesAsync(ct);

        if (auditTrailStore != null)
        {
            var audit = GarmentsERP.Shared.Domain.AuditTrailLog.Create(
                entityName: "FabricInspection",
                entityId: inspection.InspectionNumber,
                actionType: inspection.OverallResult == "PASSED" ? "QUARANTINE_RELEASE" : "QUARANTINE_REJECT",
                userId: currentUserService.UserId,
                userName: inspectorName,
                userIpAddress: currentUserService.IpAddress ?? "127.0.0.1",
                oldValuesJson: "{\"StockState\":\"QUARANTINE\",\"QCStatus\":\"PENDING\"}",
                newValuesJson: $"{{\"StockState\":\"{lot.StockState}\",\"ASTMScore\":{inspection.AveragePointsPer100SqYds},\"Result\":\"{inspection.OverallResult}\"}}"
            );
            await auditTrailStore.RecordAuditLogAsync(audit, ct);
        }

        return Ok(ApiResponse<FabricInspection>.Ok(inspection, $"Inspection evaluated: Result = {inspection.OverallResult}. Lot status updated to {lot.StockState}."));
    }
}

public class EvaluateFabricInspectionRequest
{
    public Guid StockLotId { get; set; }
    public string? InspectorName { get; set; }
    public int TotalRollsInLot { get; set; }
    public int RollsInspected { get; set; }
    public decimal TotalYardsInspected { get; set; }
    public decimal CuttableWidthInches { get; set; } = 58.00m;
    public decimal TotalPenaltyPoints { get; set; }
}
