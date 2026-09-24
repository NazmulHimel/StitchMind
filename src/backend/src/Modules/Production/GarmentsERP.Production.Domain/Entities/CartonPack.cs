using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Production.Domain.Entities;

public enum CartonStatus
{
    Packed = 1,
    Inspected = 2,
    ReadyForDispatch = 3,
    Dispatched = 4
}

public class CartonPack : BaseEntity
{
    public string CartonBarcode { get; set; } = string.Empty;
    public Guid BuyerOrderId { get; set; }
    public BuyerOrder BuyerOrder { get; set; } = null!;
    public string StyleNumber { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string SizeBreakdownJson { get; set; } = "{}"; // e.g. {"S":10,"M":20,"L":20,"XL":10}
    public int TotalQuantityPcs { get; set; }
    public decimal GrossWeightKg { get; set; }
    public decimal NetWeightKg { get; set; }
    public CartonStatus Status { get; set; } = CartonStatus.Packed;
    public string? AssignedPallet { get; set; }
}
