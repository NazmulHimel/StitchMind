using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

/// <summary>
/// Physical stock batch item under Warehouse & Quarantine control (AC-04 / PRO-06).
/// All goods inward from GRN are initially placed in 'QUARANTINE'.
/// Physical cutting floor issue is strictly locked until a verified QC 4-Point inspection PASS certificate is recorded.
/// </summary>
public class StockLot : BaseEntity
{
    public const string StateQuarantine = "QUARANTINE";
    public const string StateReleased = "RELEASED_FOR_PRODUCTION";
    public const string StateRejected = "REJECTED";

    public const string QcStatusPending = "PENDING";
    public const string QcStatusPassed = "PASSED";
    public const string QcStatusFailed = "FAILED";

    public string LotBatchNumber { get; set; } = string.Empty;
    public Guid? GoodsReceiptNoteId { get; set; }
    public Guid? BOMItemId { get; set; }
    public string ItemCode { get; set; } = string.Empty;
    public string ItemDescription { get; set; } = string.Empty;
    public string WarehouseLocationCode { get; set; } = "WH1-Aisle-03-Bay-02";
    public decimal Quantity { get; set; }
    public string UnitOfMeasure { get; set; } = "Yds";
    public string StockState { get; set; } = StateQuarantine;
    public string QCInspectionStatus { get; set; } = QcStatusPending;
    public DateTime? QCPassedAtUtc { get; set; }
    public string? QCInspectorId { get; set; }
    public Guid CompanyId { get; set; }

    public ICollection<FabricRoll> Rolls { get; set; } = new List<FabricRoll>();

    /// <summary>
    /// AC-04 Quarantine Gatekeeper: Validates whether raw materials are legally permitted for cutting floor usage.
    /// </summary>
    public (bool IsAllowed, string ErrorMessage) VerifyAvailableForCutting()
    {
        if (StockState == StateRejected || QCInspectionStatus == QcStatusFailed)
        {
            return (
                false,
                $"MATERIAL ALLOCATION BLOCKED: Stock lot '{LotBatchNumber}' is REJECTED by Quality Control."
            );
        }

        if (StockState == StateQuarantine || QCInspectionStatus != QcStatusPassed)
        {
            return (
                false,
                $"MATERIAL ALLOCATION BLOCKED: Stock lot '{LotBatchNumber}' is currently in QUARANTINE. " +
                "Fabric cutting floor requisition requires a verified QC Inspection PASS certificate."
            );
        }

        return (true, string.Empty);
    }

    /// <summary>
    /// Releases the quarantined stock lot to production upon passing the ASTM 4-Point quality audit.
    /// </summary>
    public void ReleaseToProduction(string inspectorId)
    {
        StockState = StateReleased;
        QCInspectionStatus = QcStatusPassed;
        QCInspectorId = inspectorId;
        QCPassedAtUtc = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Rejects the stock lot if defect points exceed the ASTM threshold.
    /// </summary>
    public void Reject(string inspectorId, string reason)
    {
        StockState = StateRejected;
        QCInspectionStatus = QcStatusFailed;
        QCInspectorId = inspectorId;
        UpdatedAt = DateTime.UtcNow;
    }
}
