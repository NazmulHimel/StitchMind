using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Inventory.Infrastructure.Data;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/warehouse")]
[AllowAnonymous]
public class WarehouseManagementController(InventoryDbContext db, ICurrentUserService currentUserService) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    /// <summary>
    /// WHS-01: Retrieves 3D Warehouse Coordinate Locations (Aisle, Rack, Tier, Bin) with occupancy capacity.
    /// </summary>
    [HttpGet("locations")]
    public async Task<IActionResult> GetLocations([FromQuery] WarehouseZone? zone, CancellationToken ct)
    {
        var query = db.WarehouseLocations.AsNoTracking();
        if (zone.HasValue)
        {
            query = query.Where(l => l.Zone == zone.Value);
        }

        var locations = await query.OrderBy(l => l.LocationCode).ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<WarehouseLocation>>.Ok(locations));
    }

    /// <summary>
    /// WHS-02: Retrieves fabric rolls with Barcode, Shade Group (A, B, C), Shrinkage %, and 3D Location Code.
    /// </summary>
    [HttpGet("fabric-rolls")]
    public async Task<IActionResult> GetFabricRolls(
        [FromQuery] string? shadeGroup,
        [FromQuery] bool? isRemnant,
        [FromQuery] RollStatus? status,
        CancellationToken ct)
    {
        var query = db.FabricRolls.AsNoTracking();

        if (!string.IsNullOrEmpty(shadeGroup))
        {
            query = query.Where(r => r.ShadeGroup == shadeGroup.Trim().ToUpper());
        }
        if (isRemnant.HasValue)
        {
            query = query.Where(r => r.IsRemnant == isRemnant.Value);
        }
        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        var rolls = await query.OrderBy(r => r.RollBarcode).ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<FabricRoll>>.Ok(rolls));
    }

    /// <summary>
    /// WHS-03: Retrieves statutory Customs Bond Register and Utilization Permit (UP) balance ledger.
    /// </summary>
    [HttpGet("bond-ledger")]
    public async Task<IActionResult> GetCustomsBondLedger([FromQuery] string? upNumber, CancellationToken ct)
    {
        var query = db.CustomsBondLedgers.AsNoTracking();
        if (!string.IsNullOrEmpty(upNumber))
        {
            query = query.Where(b => b.UtilizationPermitNo == upNumber.Trim().ToUpper());
        }

        var entries = await query.OrderByDescending(b => b.TransactionDate).ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<CustomsBondLedger>>.Ok(entries));
    }

    /// <summary>
    /// WHS-04: Retrieves Store Issue Notes (SIN) issued to cutting department with remnant returns.
    /// </summary>
    [HttpGet("store-issues")]
    public async Task<IActionResult> GetStoreIssueNotes(CancellationToken ct)
    {
        var notes = await db.StoreIssueNotes
            .Include(s => s.Items)
            .OrderByDescending(s => s.IssueDate)
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<StoreIssueNote>>.Ok(notes));
    }

    /// <summary>
    /// WHS-04: Creates and issues a new Store Issue Note (SIN) releasing fabric rolls to cutting floor.
    /// </summary>
    [HttpPost("store-issues")]
    public async Task<IActionResult> CreateStoreIssueNote([FromBody] CreateStoreIssueRequest request, CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var sin = new StoreIssueNote
        {
            IssueNumber = string.IsNullOrEmpty(request.IssueNumber)
                ? $"SIN-2026-{DateTime.UtcNow:MMdd}-{Random.Shared.Next(100, 999)}"
                : request.IssueNumber,
            IssueDate = DateTime.UtcNow,
            TargetDepartment = request.TargetDepartment,
            StyleNumber = request.StyleNumber,
            BuyerOrderNumber = request.BuyerOrderNumber,
            RequestedBy = request.RequestedBy,
            IssuedBy = request.IssuedBy ?? "Warehouse Dispatch Lead",
            Status = StoreIssueStatus.IssuedToFloor,
            Remarks = request.Remarks,
            CompanyId = companyId
        };

        foreach (var item in request.Items)
        {
            sin.AddItem(
                item.FabricRollId,
                item.RollBarcode,
                item.ShadeGroup,
                item.TotalRollLengthYards,
                item.IssuedYards,
                item.RemnantYards,
                item.RemnantBarcode,
                item.RemnantLocationCode ?? "WH1-A01-R01-T01-REM"
            );
        }

        await db.StoreIssueNotes.AddAsync(sin, ct);

        // Record debit against Customs Bond UP
        var latestBond = await db.CustomsBondLedgers
            .OrderByDescending(b => b.TransactionDate)
            .FirstOrDefaultAsync(ct);

        if (latestBond != null)
        {
            var debit = CustomsBondLedger.RecordDebit(
                latestBond.BondRegisterNumber,
                latestBond.UtilizationPermitNo,
                "STORE_ISSUE_NOTE",
                sin.IssueNumber,
                "FAB-TW-01",
                "Fabric floor issue debit for cutting allocation",
                sin.TotalIssuedYards,
                latestBond.ClosingBalance,
                companyId
            );
            await db.CustomsBondLedgers.AddAsync(debit, ct);
        }

        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<StoreIssueNote>.Ok(sin));
    }

    /// <summary>
    /// WHS-05: Retrieves physical inventory cycle count audits and variances.
    /// </summary>
    [HttpGet("cycle-counts")]
    public async Task<IActionResult> GetCycleCounts(CancellationToken ct)
    {
        var counts = await db.InventoryCycleCounts
            .Include(c => c.Lines)
            .OrderByDescending(c => c.AuditDate)
            .AsNoTracking()
            .ToListAsync(ct);

        return Ok(ApiResponse<IReadOnlyList<InventoryCycleCount>>.Ok(counts));
    }

    /// <summary>
    /// WHS-05: Authorizes inventory variance adjustment after physical audit review.
    /// </summary>
    [HttpPost("cycle-counts/{id:guid}/adjust")]
    public async Task<IActionResult> AuthorizeCycleCountAdjustment(
        Guid id,
        [FromBody] AuthorizeAdjustmentRequest request,
        CancellationToken ct)
    {
        var count = await db.InventoryCycleCounts
            .Include(c => c.Lines)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

        if (count == null)
        {
            return NotFound(ApiResponse<object>.Fail("Cycle count audit record not found."));
        }

        count.AuthorizeAdjustment(request.ManagerName ?? "S. K. Anwar (DGM Supply Chain)");
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<InventoryCycleCount>.Ok(count));
    }
}

public class CreateStoreIssueRequest
{
    public string? IssueNumber { get; set; }
    public string TargetDepartment { get; set; } = "Cutting Floor (Table 01)";
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string BuyerOrderNumber { get; set; } = "ORD-2026-001";
    public string RequestedBy { get; set; } = "Mahfuz Rahman (Cutting Master)";
    public string? IssuedBy { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public List<CreateStoreIssueItemRequest> Items { get; set; } = [];
}

public class CreateStoreIssueItemRequest
{
    public Guid FabricRollId { get; set; }
    public string RollBarcode { get; set; } = string.Empty;
    public string ShadeGroup { get; set; } = "A";
    public decimal TotalRollLengthYards { get; set; }
    public decimal IssuedYards { get; set; }
    public decimal RemnantYards { get; set; }
    public string? RemnantBarcode { get; set; }
    public string? RemnantLocationCode { get; set; }
}

public class AuthorizeAdjustmentRequest
{
    public string? ManagerName { get; set; }
}
