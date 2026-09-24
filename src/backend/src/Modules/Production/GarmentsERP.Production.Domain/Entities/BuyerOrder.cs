using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public class BuyerOrder : BaseEntity
{
    public string OrderNumber { get; set; } = string.Empty;
    public Guid BuyerId { get; set; }
    public Buyer? Buyer { get; set; }
    public string StyleNumber { get; set; } = string.Empty;
    public string StyleName { get; set; } = string.Empty;
    public string Season { get; set; } = "SS-2026";
    public int OrderQuantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalValue => OrderQuantity * UnitPrice;
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime DeliveryDate { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Draft;
    public string? Notes { get; set; }

    public ICollection<CuttingOrder> CuttingOrders { get; set; } = new List<CuttingOrder>();
    public ICollection<HourlyProductionLog> ProductionLogs { get; set; } = new List<HourlyProductionLog>();
    public ICollection<QualityInspection> QualityInspections { get; set; } = new List<QualityInspection>();
}
