using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// SEW-06: Broken Needle Exchange Compliance Policy & Fragment Verification Log.
/// Enforces mandatory 100% fragment recovery (Tip, Shank, Eye) and supervisor sign-off
/// before a replacement sewing needle can be dispensed.
/// </summary>
public class BrokenNeedleLog : BaseEntity
{
    public string LogNumber { get; set; } = string.Empty; // e.g. BNL-2026-0089
    public string LineNumber { get; set; } = "LINE-04";
    public string MachineBarcode { get; set; } = "SN-084";
    public string OperatorBadgeId { get; set; } = "OP-1042";
    public string OperatorName { get; set; } = "Nasreen Sultana";
    public string BreakageReason { get; set; } = "Deflection on heavy multi-ply waistband join";
    
    // Fragment Verification Gate
    public bool TipFound { get; set; } = true;
    public bool ShankFound { get; set; } = true;
    public bool EyeFound { get; set; } = true;
    public bool AllFragmentsRecovered { get; set; } = true;
    
    public string SupervisorVerificationBadge { get; set; } = "SUP-012";
    public bool ReplacementNeedleIssued { get; set; } = true;
    public string ReplacementNeedleType { get; set; } = "Groz-Beckert DPx5 110/18";
    public DateTime LoggedAtUtc { get; set; } = DateTime.UtcNow;
    public int DowntimeMinutes { get; set; } = 4;
    public NeedleComplianceStatus Status { get; set; } = NeedleComplianceStatus.VerifiedAndReplaced;
    public string Remarks { get; set; } = "All 3 fragments mounted on compliance card. Metal detector clearance completed.";

    public bool VerifyAndIssueReplacement(string supervisorBadge, bool tip, bool shank, bool eye, string needleType, out string message)
    {
        TipFound = tip;
        ShankFound = shank;
        EyeFound = eye;
        AllFragmentsRecovered = tip && shank && eye;
        SupervisorVerificationBadge = supervisorBadge;
        ReplacementNeedleType = needleType;

        if (!AllFragmentsRecovered)
        {
            ReplacementNeedleIssued = false;
            Status = NeedleComplianceStatus.FragmentSearchActive;
            message = "COMPLIANCE BLOCK: Incomplete needle fragments! Tip, Shank, and Eye must all be recovered or garment bundle quarantined for 9-point metal detector audit.";
            return false;
        }

        ReplacementNeedleIssued = true;
        Status = NeedleComplianceStatus.VerifiedAndReplaced;
        message = $"All 3 needle fragments verified by Supervisor {supervisorBadge}. Replacement needle '{needleType}' authorized for issue.";
        return true;
    }
}
