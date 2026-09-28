using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GarmentsERP.API.Controllers;

[ApiController]
[Route("api/v1/commercial")]
public class CommercialTradeController(
    IProcurementRepository procurementRepo,
    ICurrentUserService currentUserService,
    IAuditTrailStore? auditTrailStore = null) : ControllerBase
{
    private Guid GetEffectiveCompanyId()
    {
        return currentUserService.CompanyId != Guid.Empty
            ? currentUserService.CompanyId
            : Guid.Parse("11111111-1111-1111-1111-111111111111");
    }

    [HttpGet("requisitions")]
    [AllowAnonymous]
    public async Task<IActionResult> GetRequisitions(CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var requisitions = await procurementRepo.GetMaterialRequisitionsAsync(companyId, ct);
        return Ok(ApiResponse<IReadOnlyList<MaterialRequisition>>.Ok(requisitions));
    }

    [HttpGet("comparative-statements")]
    [AllowAnonymous]
    public async Task<IActionResult> GetComparativeStatements(CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var statements = await procurementRepo.GetComparativeStatementsAsync(companyId, ct);
        return Ok(ApiResponse<IReadOnlyList<ComparativeStatement>>.Ok(statements));
    }

    [HttpPost("comparative-statements/{id:guid}/award")]
    [AllowAnonymous]
    public async Task<IActionResult> AwardSupplier(Guid id, [FromBody] AwardSupplierRequest request, CancellationToken ct)
    {
        var cs = await procurementRepo.GetComparativeStatementByIdAsync(id, ct);
        if (cs == null) return NotFound(ApiResponse<object>.Fail("Comparative Statement not found."));

        cs.AwardContract(request.SupplierId, request.Notes);
        await procurementRepo.SaveChangesAsync(ct);

        if (auditTrailStore != null)
        {
            var audit = GarmentsERP.Shared.Domain.AuditTrailLog.Create(
                entityName: "ComparativeStatement",
                entityId: cs.CSNumber,
                actionType: "AWARD_CONTRACT",
                userId: currentUserService.UserId,
                userName: currentUserService.UserName ?? "cfo@stitchmind.com",
                userIpAddress: currentUserService.IpAddress ?? "127.0.0.1",
                oldValuesJson: "{\"Status\":\"Draft\"}",
                newValuesJson: $"{{\"AwardedSupplier\":\"{cs.AwardedSupplierName}\",\"AwardedRate\":{cs.AwardedRate}}}"
            );
            await auditTrailStore.RecordAuditLogAsync(audit, ct);
        }

        return Ok(ApiResponse<ComparativeStatement>.Ok(cs, "Contract successfully awarded to supplier."));
    }

    [HttpGet("lcs")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCommercialLCs(CancellationToken ct)
    {
        var companyId = GetEffectiveCompanyId();
        var lcs = await procurementRepo.GetCommercialLCsAsync(companyId, ct);
        return Ok(ApiResponse<IReadOnlyList<CommercialLC>>.Ok(lcs));
    }

    [HttpPost("purchaseorders/{id:guid}/override-budget")]
    [AllowAnonymous]
    public async Task<IActionResult> AuthorizeBudgetOverride(Guid id, [FromBody] BudgetOverrideRequest request, CancellationToken ct)
    {
        var po = await procurementRepo.GetPurchaseOrderByIdAsync(id, ct);
        if (po == null) return NotFound(ApiResponse<object>.Fail("Purchase order not found."));

        var cfoId = currentUserService.UserId != Guid.Empty
            ? currentUserService.UserId
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        po.AuthorizeBudgetOverride(cfoId, request.Reason);
        await procurementRepo.UpdatePurchaseOrderAsync(po, ct);
        await procurementRepo.SaveChangesAsync(ct);

        if (auditTrailStore != null)
        {
            var audit = GarmentsERP.Shared.Domain.AuditTrailLog.Create(
                entityName: "PurchaseOrder",
                entityId: po.PONumber,
                actionType: "BUDGET_OVERRIDE",
                userId: cfoId,
                userName: currentUserService.UserName ?? "cfo@stitchmind.com",
                userIpAddress: currentUserService.IpAddress ?? "127.0.0.1",
                oldValuesJson: "{\"IsBudgetLocked\":true,\"Status\":\"LockedBudgetExceeded\"}",
                newValuesJson: $"{{\"IsBudgetLocked\":false,\"Status\":\"Submitted\",\"Reason\":\"{request.Reason}\"}}"
            );
            await auditTrailStore.RecordAuditLogAsync(audit, ct);
        }

        return Ok(ApiResponse<object>.Ok(new
        {
            po.Id,
            po.PONumber,
            po.Status,
            po.IsBudgetLocked,
            po.BudgetOverrideReason,
            po.BudgetOverrideAt
        }, "CFO budget override successfully authorized. PO unlocked for approval."));
    }
}

public class AwardSupplierRequest
{
    public Guid SupplierId { get; set; }
    public string? Notes { get; set; }
}

public class BudgetOverrideRequest
{
    public string Reason { get; set; } = string.Empty;
}
