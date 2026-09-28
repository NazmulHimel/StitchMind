using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

public class FabricRoll : BaseEntity
{
    public string RollNumber { get; set; } = string.Empty;
    public string RollBarcode { get; set; } = string.Empty;
    public Guid InventoryItemId { get; set; }
    public Guid? StockLotId { get; set; }
    public string DyeLotNumber { get; set; } = string.Empty;
    public decimal GSM { get; set; }
    public decimal MeasuredGSM { get; set; }
    public decimal WidthInches { get; set; }
    public decimal CuttableWidthInches { get; set; }
    public decimal TotalLengthYards { get; set; }
    public decimal AvailableLengthYards { get; set; }
    public string ShadeGroup { get; set; } = "A"; // "A", "B", "C"
    public decimal ShrinkageLengthPct { get; set; } = -2.5m;
    public decimal ShrinkageWidthPct { get; set; } = -1.2m;
    public FabricGrade Grade { get; set; } = FabricGrade.GradeA;
    public RollStatus Status { get; set; } = RollStatus.Available;
    public string RackLocation { get; set; } = string.Empty;
    public string LocationCode { get; set; } = string.Empty; // 3D Coordinate: WH1-A03-R02-T01-B04
    public string? SupplierRollNo { get; set; }
    public Guid? GoodsReceiptNoteId { get; set; }
    public bool IsRemnant { get; set; } = false;
    public Guid? ParentRollId { get; set; }
    public Guid CompanyId { get; set; }

    public InventoryItem InventoryItem { get; set; } = null!;
    public StockLot? StockLot { get; set; }

    public FabricRoll() { }

    public static FabricRoll Create(
        string rollNumber, Guid itemId, string dyeLot, decimal gsm, decimal width,
        decimal lengthYards, string rackLocation, Guid companyId,
        string? supplierRollNo = null, Guid? grnId = null, FabricGrade grade = FabricGrade.GradeA,
        string? rollBarcode = null, Guid? stockLotId = null, string shadeGroup = "A",
        decimal shrinkageLengthPct = -2.5m, decimal shrinkageWidthPct = -1.2m,
        string? locationCode = null)
    {
        var roll = new FabricRoll
        {
            RollNumber = rollNumber.Trim().ToUpper(),
            RollBarcode = rollBarcode ?? $"ROL-{rollNumber.Trim().ToUpper()}",
            InventoryItemId = itemId,
            StockLotId = stockLotId,
            DyeLotNumber = dyeLot.Trim().ToUpper(),
            GSM = gsm,
            MeasuredGSM = gsm,
            WidthInches = width,
            CuttableWidthInches = width,
            TotalLengthYards = lengthYards,
            AvailableLengthYards = lengthYards,
            ShadeGroup = shadeGroup.Trim().ToUpper(),
            ShrinkageLengthPct = shrinkageLengthPct,
            ShrinkageWidthPct = shrinkageWidthPct,
            Grade = grade,
            Status = RollStatus.Available,
            RackLocation = rackLocation.Trim().ToUpper(),
            LocationCode = locationCode ?? rackLocation.Trim().ToUpper(),
            SupplierRollNo = supplierRollNo?.Trim(),
            GoodsReceiptNoteId = grnId,
            CompanyId = companyId
        };
        return roll;
    }

    public void IssueYards(decimal yards)
    {
        if (yards <= 0) throw new ArgumentOutOfRangeException(nameof(yards), "Yards to issue must be greater than zero.");
        if (yards > AvailableLengthYards) throw new InvalidOperationException($"Cannot issue {yards} yds. Only {AvailableLengthYards} yds available on Roll {RollNumber}.");

        AvailableLengthYards -= yards;
        if (AvailableLengthYards == 0)
        {
            Status = RollStatus.Consumed;
        }
        else
        {
            Status = RollStatus.InCutting;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateGrade(FabricGrade grade)
    {
        Grade = grade;
        if (grade == FabricGrade.Rejected)
        {
            Status = RollStatus.Quarantined;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    public void Relocate(string newRack)
    {
        RackLocation = newRack.Trim().ToUpper();
        UpdatedAt = DateTime.UtcNow;
    }
}
