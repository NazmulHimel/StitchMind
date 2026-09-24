using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Inventory.Domain.Entities;

public class InventoryItem : BaseEntity
{
    public string ItemCode { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public ItemCategory Category { get; private set; }
    public StockUOM UnitOfMeasure { get; private set; }
    public decimal CurrentStock { get; private set; } = 0;
    public decimal SafetyStock { get; private set; } = 0;
    public decimal ReorderPoint { get; private set; } = 0;
    public decimal StandardCost { get; private set; } = 0;
    public string BinLocation { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public Guid CompanyId { get; private set; }

    public ICollection<FabricRoll> FabricRolls { get; private set; } = [];
    public ICollection<StockTransaction> Transactions { get; private set; } = [];

    private InventoryItem() { }

    public static InventoryItem Create(
        string code, string name, string description, ItemCategory category,
        StockUOM uom, decimal initialStock, decimal safetyStock, decimal reorderPoint,
        decimal standardCost, string binLocation, Guid companyId)
    {
        return new InventoryItem
        {
            ItemCode = code.Trim().ToUpper(),
            Name = name.Trim(),
            Description = description.Trim(),
            Category = category,
            UnitOfMeasure = uom,
            CurrentStock = Math.Max(0, initialStock),
            SafetyStock = Math.Max(0, safetyStock),
            ReorderPoint = Math.Max(0, reorderPoint),
            StandardCost = Math.Max(0, standardCost),
            BinLocation = binLocation.Trim().ToUpper(),
            CompanyId = companyId
        };
    }

    public void AddStock(decimal quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
        CurrentStock += quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void DeductStock(decimal quantity)
    {
        if (quantity < 0) throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity cannot be negative.");
        if (CurrentStock < quantity) throw new InvalidOperationException($"Insufficient stock for item {ItemCode}. Available: {CurrentStock}, Requested: {quantity}");
        CurrentStock -= quantity;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AdjustStock(decimal newStockQuantity)
    {
        CurrentStock = Math.Max(0, newStockQuantity);
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateDetails(string name, string description, decimal safetyStock, decimal reorderPoint, decimal standardCost, string binLocation)
    {
        Name = name.Trim();
        Description = description.Trim();
        SafetyStock = Math.Max(0, safetyStock);
        ReorderPoint = Math.Max(0, reorderPoint);
        StandardCost = Math.Max(0, standardCost);
        BinLocation = binLocation.Trim().ToUpper();
        UpdatedAt = DateTime.UtcNow;
    }
}
