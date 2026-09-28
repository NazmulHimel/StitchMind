using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

/// <summary>
/// WHS-04: Store Issue Note (SIN) & Remnant / Cut-Piece Inventory Management.
/// Authorizes and records raw fabric rolls released from central warehouse to cutting room lay tables.
/// Handles remnant split accounting for end-of-roll leftover yardage.
/// </summary>
public class StoreIssueNote : BaseEntity
{
    public string IssueNumber { get; set; } = string.Empty;
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
    public string TargetDepartment { get; set; } = "Cutting Floor (Table 01)";
    public string StyleNumber { get; set; } = string.Empty;
    public string BuyerOrderNumber { get; set; } = string.Empty;
    public string RequestedBy { get; set; } = string.Empty;
    public string IssuedBy { get; set; } = string.Empty;
    public StoreIssueStatus Status { get; set; } = StoreIssueStatus.Draft;
    public decimal TotalIssuedYards { get; set; }
    public decimal TotalRemnantYards { get; set; }
    public string Remarks { get; set; } = string.Empty;
    public Guid CompanyId { get; set; }

    public ICollection<StoreIssueNoteItem> Items { get; set; } = new List<StoreIssueNoteItem>();

    public void AddItem(
        Guid fabricRollId,
        string rollBarcode,
        string shadeGroup,
        decimal totalRollLength,
        decimal issuedYards,
        decimal remnantYards = 0,
        string? remnantBarcode = null,
        string? remnantLocation = "WH1-A01-R01-T01-REM")
    {
        if (issuedYards <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(issuedYards), "Issued yards must be greater than zero.");
        }

        if (issuedYards + remnantYards > totalRollLength)
        {
            throw new InvalidOperationException($"Sum of issued yards ({issuedYards}) and remnant ({remnantYards}) exceeds total roll length ({totalRollLength}).");
        }

        var item = new StoreIssueNoteItem
        {
            StoreIssueNoteId = Id,
            FabricRollId = fabricRollId,
            RollBarcode = rollBarcode,
            ShadeGroup = shadeGroup,
            TotalRollLengthYards = totalRollLength,
            IssuedYards = issuedYards,
            RemnantYards = remnantYards,
            RemnantBarcode = remnantBarcode ?? (remnantYards > 0 ? $"{rollBarcode}-REM" : null),
            RemnantLocationCode = remnantYards > 0 ? remnantLocation : null
        };

        Items.Add(item);
        TotalIssuedYards = Items.Sum(i => i.IssuedYards);
        TotalRemnantYards = Items.Sum(i => i.RemnantYards);
        UpdatedAt = DateTime.UtcNow;
    }

    public void ApproveAndIssue(string authorizedBy)
    {
        if (Items.Count == 0)
        {
            throw new InvalidOperationException("Cannot issue empty Store Issue Note.");
        }

        IssuedBy = authorizedBy;
        Status = StoreIssueStatus.IssuedToFloor;
        UpdatedAt = DateTime.UtcNow;
    }
}

public class StoreIssueNoteItem : BaseEntity
{
    public Guid StoreIssueNoteId { get; set; }
    public Guid FabricRollId { get; set; }
    public string RollBarcode { get; set; } = string.Empty;
    public string ShadeGroup { get; set; } = "A";
    public decimal TotalRollLengthYards { get; set; }
    public decimal IssuedYards { get; set; }
    public decimal RemnantYards { get; set; }
    public string? RemnantBarcode { get; set; }
    public string? RemnantLocationCode { get; set; }

    public StoreIssueNote StoreIssueNote { get; set; } = null!;
}
