namespace GarmentsERP.Reporting.Application.Models;

public class BrokenNeedleReportModel
{
    public string CertificateNumber { get; set; } = "BNL-2026-0089";
    public string LineNumber { get; set; } = "LINE-04";
    public string MachineBarcode { get; set; } = "SN-084";
    public string MachineModel { get; set; } = "Juki DDL-9000C-FMS (Direct Drive)";
    public string OperatorBadgeId { get; set; } = "OP-1042";
    public string OperatorName { get; set; } = "Nasreen Sultana";
    public string BreakageReason { get; set; } = "Deflection on heavy multi-ply waistband join";
    public bool TipFound { get; set; } = true;
    public bool ShankFound { get; set; } = true;
    public bool EyeFound { get; set; } = true;
    public bool AllFragmentsRecovered { get; set; } = true;
    public string SupervisorBadge { get; set; } = "SUP-012";
    public string SupervisorName { get; set; } = "Mohammad Al-Amin";
    public bool ReplacementNeedleIssued { get; set; } = true;
    public string ReplacementNeedleType { get; set; } = "Groz-Beckert DPx5 110/18";
    public DateTime IncidentTimeUtc { get; set; } = DateTime.UtcNow;
    public int DowntimeMinutes { get; set; } = 4;
    public string ComplianceStatus { get; set; } = "VerifiedAndReplaced";
    public string MetalDetectorCheckResult { get; set; } = "PASSED - 9-Point Ferrous 0.8mm Calibration Clear";
    public string Remarks { get; set; } = "All 3 fragments mounted on compliance card. Metal detector clearance completed.";
}
