namespace GarmentsERP.Reporting.Application.Models;

public class PurchaseOrderReportModel
{
    public string PONumber { get; set; } = string.Empty;
    public string SupplierName { get; set; } = string.Empty;
    public string SupplierAddress { get; set; } = string.Empty;
    public string PaymentTerms { get; set; } = string.Empty;
    public string StyleNumber { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime ExpectedDeliveryDate { get; set; }
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public string? B2BLCNumber { get; set; }
    public bool IsBudgetLocked { get; set; }
    public List<PurchaseOrderReportItem> Items { get; set; } = [];
    public decimal TotalAmount => Items.Sum(i => i.TotalPrice);
}

public class PurchaseOrderReportItem
{
    public string ItemCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string UnitOfMeasure { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal? BudgetUnitPrice { get; set; }
    public decimal TotalPrice => Quantity * UnitPrice;
}
