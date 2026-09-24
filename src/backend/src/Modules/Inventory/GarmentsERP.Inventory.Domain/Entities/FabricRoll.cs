using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

public class FabricRoll : BaseEntity
{
    public string RollNumber { get; private set; } = string.Empty;
    public Guid InventoryItemId { get; private set; }
    public string DyeLotNumber { get; private set; } = string.Empty;
    public decimal GSM { get; private set; }
    public decimal WidthInches { get; private set; }
    public decimal TotalLengthYards { get; private set; }
    public decimal AvailableLengthYards { get; private set; }
    public FabricGrade Grade { get; private set; } = FabricGrade.GradeA;
    public RollStatus Status { get; private set; } = RollStatus.Available;
    public string RackLocation { get; private set; } = string.Empty;
    public string? SupplierRollNo { get; private set; }
    public Guid? GoodsReceiptNoteId { get; private set; }
    public Guid CompanyId { get; private set; }

    public InventoryItem InventoryItem { get; private set; } = null!;

    private FabricRoll() { }

    public static FabricRoll Create(
        string rollNumber, Guid itemId, string dyeLot, decimal gsm, decimal width,
        decimal lengthYards, string rackLocation, Guid companyId,
        string? supplierRollNo = null, Guid? grnId = null, FabricGrade grade = FabricGrade.GradeA)
    {
        return new FabricRoll
        {
            RollNumber = rollNumber.Trim().ToUpper(),
            InventoryItemId = itemId,
            DyeLotNumber = dyeLot.Trim().ToUpper(),
            GSM = gsm,
            WidthInches = width,
            TotalLengthYards = lengthYards,
            AvailableLengthYards = lengthYards,
            Grade = grade,
            Status = RollStatus.Available,
            RackLocation = rackLocation.Trim().ToUpper(),
            SupplierRollNo = supplierRollNo?.Trim(),
            GoodsReceiptNoteId = grnId,
            CompanyId = companyId
        };
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
