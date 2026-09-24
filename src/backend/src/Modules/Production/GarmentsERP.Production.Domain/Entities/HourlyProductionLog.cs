using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public class HourlyProductionLog : BaseEntity
{
    public Guid SewingLineId { get; set; }
    public SewingLine? SewingLine { get; set; }
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder? BuyerOrder { get; set; }
    public DateTime LogDate { get; set; } = DateTime.UtcNow.Date;
    public int HourSlot { get; set; } // 1 to 10 (e.g. 1 = 08:00-09:00)
    public int TargetQty { get; set; }
    public int ActualOutputQty { get; set; }
    public int DefectCount { get; set; }
    public int RejectionCount { get; set; }
    public int OperatorCount { get; set; }
    public decimal EfficiencyPercentage => TargetQty > 0 ? Math.Round(((decimal)ActualOutputQty / TargetQty) * 100, 2) : 0;
    public string? Notes { get; set; }
}
