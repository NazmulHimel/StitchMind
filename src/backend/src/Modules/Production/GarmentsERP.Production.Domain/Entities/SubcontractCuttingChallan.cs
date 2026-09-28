using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// CUT-06: Subcontract Cutting Delivery & Return Shortage Reconciliation Ledger.
/// Ensures outsourced cutting processes do not leak fabric or result in unaccounted panel shortages.
/// </summary>
public class SubcontractCuttingChallan : BaseEntity
{
    public string ChallanNumber { get; set; } = string.Empty; // e.g. SCC-2026-0012
    public string SubcontractorName { get; set; } = "Apex Precision Cutting Mills";
    public string StyleNumber { get; set; } = "ST-2026-CHINO-01";
    public string OrderNumber { get; set; } = "BO-2026-0890";
    public decimal FabricSentYds { get; set; } = 1200.0m;
    public int ExpectedCutPanels { get; set; } = 1440;
    public int ReceivedCutPanels { get; set; }
    public int DefectivePanels { get; set; }
    public int ShortageCount { get; set; }
    public decimal ShortagePercentage { get; set; }
    public SubcontractStatus Status { get; set; } = SubcontractStatus.Dispatched;
    public DateTime DispatchedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ReceivedAtUtc { get; set; }
    public string ReconciledBy { get; set; } = string.Empty;
    public string Remarks { get; set; } = string.Empty;

    public void Reconcile(int receivedPanels, int defectivePanels, string reconciledBy, string? remarks = null)
    {
        ReceivedCutPanels = receivedPanels;
        DefectivePanels = defectivePanels;
        ShortageCount = Math.Max(0, ExpectedCutPanels - (receivedPanels + defectivePanels));
        ShortagePercentage = ExpectedCutPanels > 0 
            ? Math.Round(((decimal)ShortageCount / ExpectedCutPanels) * 100m, 2) 
            : 0m;

        ReceivedAtUtc = DateTime.UtcNow;
        ReconciledBy = reconciledBy;
        if (!string.IsNullOrEmpty(remarks))
        {
            Remarks = remarks;
        }

        Status = (ShortagePercentage > 1.0m)
            ? SubcontractStatus.VarianceUnderReview
            : SubcontractStatus.Reconciled;
    }
}
