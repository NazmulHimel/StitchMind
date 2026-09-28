using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Procurement.Domain.Entities;

public class PurchaseOrder : BaseEntity
{
    public string PONumber { get; private set; } = string.Empty;
    public Guid SupplierId { get; private set; }
    public Guid CompanyId { get; private set; }
    public DateTime OrderDate { get; private set; } = DateTime.UtcNow;
    public DateTime ExpectedDeliveryDate { get; private set; }
    public POStatus Status { get; private set; } = POStatus.Draft;
    public decimal TotalAmount { get; private set; } = 0;
    public string? Notes { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }

    public Guid? StyleId { get; private set; }
    public bool IsBudgetLocked { get; private set; } = false;
    public Guid? BudgetOverrideBy { get; private set; }
    public string? BudgetOverrideReason { get; private set; }
    public DateTime? BudgetOverrideAt { get; private set; }

    public Supplier Supplier { get; private set; } = null!;
    public ICollection<PurchaseOrderItem> Items { get; private set; } = [];
    public ICollection<GoodsReceiptNote> GoodsReceiptNotes { get; private set; } = [];

    private PurchaseOrder() { }

    public static PurchaseOrder Create(string poNumber, Guid supplierId, Guid companyId,
        DateTime expectedDeliveryDate, string? notes = null, Guid? styleId = null)
    {
        return new PurchaseOrder
        {
            PONumber = poNumber.Trim().ToUpper(),
            SupplierId = supplierId,
            CompanyId = companyId,
            StyleId = styleId,
            ExpectedDeliveryDate = expectedDeliveryDate,
            Notes = notes,
            Status = POStatus.Draft
        };
    }

    public void AddItem(string itemCode, string description, MaterialCategory category,
        UOM uom, decimal quantity, decimal unitPrice, decimal? budgetUnitPrice = null)
    {
        var item = PurchaseOrderItem.Create(Id, itemCode, description, category, uom, quantity, unitPrice, budgetUnitPrice);
        Items.Add(item);

        // AC-03: Pre-Budget Validation Lock
        if (budgetUnitPrice.HasValue && budgetUnitPrice.Value > 0 && unitPrice > budgetUnitPrice.Value)
        {
            IsBudgetLocked = true;
            Status = POStatus.LockedBudgetExceeded;
        }

        RecalculateTotal();
    }

    public void AuthorizeBudgetOverride(Guid cfoUserId, string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("A valid business justification is required for budget override.", nameof(reason));

        IsBudgetLocked = false;
        BudgetOverrideBy = cfoUserId;
        BudgetOverrideReason = reason.Trim();
        BudgetOverrideAt = DateTime.UtcNow;
        Status = POStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Submit()
    {
        if (IsBudgetLocked)
            throw new InvalidOperationException("Cannot submit Purchase Order: Hard Budget Lock active because item price exceeds pre-costing budget. CFO authorization required.");

        if (Status != POStatus.Draft) throw new InvalidOperationException("Only draft POs can be submitted.");
        Status = POStatus.Submitted;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve(Guid approvedByUserId)
    {
        if (IsBudgetLocked)
            throw new InvalidOperationException("Cannot approve Purchase Order: Hard Budget Lock active. CFO authorization required.");

        if (Status != POStatus.Submitted && Status != POStatus.Draft)
            throw new InvalidOperationException("PO must be submitted before approval.");

        Status = POStatus.Approved;
        ApprovedBy = approvedByUserId;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == POStatus.Completed) throw new InvalidOperationException("Completed POs cannot be cancelled.");
        Status = POStatus.Cancelled;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateReceivingStatus()
    {
        var totalQty = Items.Sum(i => i.Quantity);
        var totalReceived = Items.Sum(i => i.ReceivedQuantity);

        if (totalReceived >= totalQty && totalQty > 0)
        {
            Status = POStatus.Completed;
        }
        else if (totalReceived > 0)
        {
            Status = POStatus.PartiallyReceived;
        }
        UpdatedAt = DateTime.UtcNow;
    }

    private void RecalculateTotal()
    {
        TotalAmount = Items.Sum(i => i.TotalPrice);
        UpdatedAt = DateTime.UtcNow;
    }
}

public class PurchaseOrderItem : BaseEntity
{
    public Guid PurchaseOrderId { get; private set; }
    public string ItemCode { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public MaterialCategory Category { get; private set; }
    public UOM UnitOfMeasure { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }
    public decimal? BudgetUnitPrice { get; private set; }
    public decimal TotalPrice { get; private set; }
    public decimal ReceivedQuantity { get; private set; } = 0;

    public PurchaseOrder PurchaseOrder { get; private set; } = null!;

    private PurchaseOrderItem() { }

    public static PurchaseOrderItem Create(Guid poId, string itemCode, string description,
        MaterialCategory category, UOM uom, decimal quantity, decimal unitPrice, decimal? budgetUnitPrice = null)
    {
        return new PurchaseOrderItem
        {
            PurchaseOrderId = poId,
            ItemCode = itemCode.Trim().ToUpper(),
            Description = description.Trim(),
            Category = category,
            UnitOfMeasure = uom,
            Quantity = quantity,
            UnitPrice = unitPrice,
            BudgetUnitPrice = budgetUnitPrice,
            TotalPrice = quantity * unitPrice
        };
    }

    public void RecordReceived(decimal qty)
    {
        ReceivedQuantity += qty;
        UpdatedAt = DateTime.UtcNow;
    }
}
