using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public class SewingLine : BaseEntity
{
    public string LineNumber { get; set; } = string.Empty; // e.g. Line-01
    public string LineName { get; set; } = string.Empty; // e.g. Polo Shirt Dedicated Line
    public string Floor { get; set; } = "Floor 2";
    public decimal TargetEfficiency { get; set; } = 85.0m;
    public int CapacityPerDay { get; set; } = 1500;
    public string Supervisor { get; set; } = string.Empty;
    public SewingLineStatus Status { get; set; } = SewingLineStatus.Active;

    public ICollection<HourlyProductionLog> ProductionLogs { get; set; } = new List<HourlyProductionLog>();
}
