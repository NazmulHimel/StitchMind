using GarmentsERP.Production.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public class CutBundle : BaseEntity
{
    public string BundleNumber { get; set; } = string.Empty; // e.g. BND-8801-01
    public Guid CuttingOrderId { get; set; }
    public CuttingOrder? CuttingOrder { get; set; }
    public string Size { get; set; } = "M";
    public string Color { get; set; } = "Navy Blue";
    public int Quantity { get; set; }
    public string Barcode { get; set; } = string.Empty;
    public int SerialStart { get; set; }
    public int SerialEnd { get; set; }
    public BundleSection CurrentSection { get; set; } = BundleSection.Cutting;
    public bool IsCompleted { get; set; } = false;
    public DateTime? CompletedAt { get; set; }
}
