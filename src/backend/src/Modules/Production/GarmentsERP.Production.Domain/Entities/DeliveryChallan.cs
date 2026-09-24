using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public enum ChallanStatus
{
    Prepared = 1,
    Inspected = 2,
    Dispatched = 3,
    Delivered = 4
}

public class DeliveryChallan : BaseEntity
{
    public string ChallanNumber { get; set; } = string.Empty; // e.g. CH-2026-0081
    public Guid BuyerId { get; set; }
    public Buyer Buyer { get; set; } = null!;
    public string VehicleNumber { get; set; } = string.Empty;
    public string DriverName { get; set; } = string.Empty;
    public string DriverPhone { get; set; } = string.Empty;
    public string DestinationAddress { get; set; } = string.Empty;
    public int TotalCartons { get; set; }
    public int TotalGarmentPcs { get; set; }
    public decimal TotalWeightKg { get; set; }
    public DateTime DispatchDate { get; set; }
    public ChallanStatus Status { get; set; } = ChallanStatus.Prepared;
    public string? GatePassNumber { get; set; }
    public string? Remarks { get; set; }

    public ICollection<DeliveryChallanItem> Items { get; set; } = new List<DeliveryChallanItem>();
}

public class DeliveryChallanItem : BaseEntity
{
    public Guid DeliveryChallanId { get; set; }
    public DeliveryChallan DeliveryChallan { get; set; } = null!;
    public string CartonBarcode { get; set; } = string.Empty;
    public string StyleNumber { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public int QuantityPcs { get; set; }
}
