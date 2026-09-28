using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

/// <summary>
/// SEW-05: Sewing Machine Asset Entity with MTTR / MTBF Analytics.
/// </summary>
public class SewingMachineAsset : BaseEntity
{
    public string MachineBarcode { get; set; } = string.Empty; // e.g. SN-084
    public string Brand { get; set; } = "Juki";
    public string MachineModel { get; set; } = "DDL-9000C-FMS";
    public string MachineType { get; set; } = "Single Needle Lockstitch (Direct Drive)";
    public string LineNumber { get; set; } = "LINE-04";
    public string StationNumber { get; set; } = "Station 12";
    public MachineStatus Status { get; set; } = MachineStatus.Operational;
    public decimal MTBFHours { get; set; } = 240.5m; // Mean Time Between Failures
    public decimal MTTRMinutes { get; set; } = 14.2m; // Mean Time To Repair
    public DateTime? LastServicedAtUtc { get; set; } = DateTime.UtcNow.AddDays(-12);
}
