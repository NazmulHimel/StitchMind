using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

public class StockTransaction : BaseEntity
{
    public string TransactionNumber { get; private set; } = string.Empty;
    public Guid InventoryItemId { get; private set; }
    public StockTransactionType TransactionType { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitCost { get; private set; }
    public decimal TotalCost { get; private set; }
    public string? ReferenceNumber { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public string SourceLocation { get; private set; } = string.Empty;
    public string DestinationLocation { get; private set; } = string.Empty;
    public Guid PerformedBy { get; private set; }
    public string? Notes { get; private set; }
    public Guid CompanyId { get; private set; }

    public InventoryItem InventoryItem { get; private set; } = null!;

    private StockTransaction() { }

    public static StockTransaction Create(
        string txNumber, Guid itemId, StockTransactionType type, decimal quantity,
        decimal unitCost, string sourceLoc, string destLoc, Guid performedBy,
        Guid companyId, string? refNumber = null, Guid? refId = null, string? notes = null)
    {
        return new StockTransaction
        {
            TransactionNumber = txNumber.Trim().ToUpper(),
            InventoryItemId = itemId,
            TransactionType = type,
            Quantity = quantity,
            UnitCost = unitCost,
            TotalCost = quantity * unitCost,
            SourceLocation = sourceLoc.Trim(),
            DestinationLocation = destLoc.Trim(),
            PerformedBy = performedBy,
            CompanyId = companyId,
            ReferenceNumber = refNumber?.Trim(),
            ReferenceId = refId,
            Notes = notes
        };
    }
}
