using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// SEW-05: Machine Breakdown Ticket & Downtime Tracking.
/// </summary>
public class MachineBreakdownTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty; // e.g. TKT-2026-0045
    public string MachineBarcode { get; set; } = "SN-084";
    public string LineNumber { get; set; } = "LINE-04";
    public string ReportedIssue { get; set; } = "Thread trimmer solenoid jammed / needle thread tension loose";
    public DateTime ReportedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ResolvedAtUtc { get; set; }
    public int DowntimeMinutes { get; set; } = 15;
    public string MechanicName { get; set; } = "Kamal Hossain";
    public string RootCause { get; set; } = "Lint accumulation in bobbin case hook assembly. Cleaned & lubricated.";
    public bool IsResolved { get; set; } = true;
}
