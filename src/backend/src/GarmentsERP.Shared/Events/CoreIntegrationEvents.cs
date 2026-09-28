namespace GarmentsERP.Shared.Events;

/// <summary>
/// Published when a new Purchase Order is created or submitted.
/// </summary>
public class PurchaseOrderCreatedIntegrationEvent : BaseIntegrationEvent
{
    public Guid PurchaseOrderId { get; init; }
    public string PurchaseOrderNumber { get; init; } = string.Empty;
    public Guid SupplierId { get; init; }
    public string SupplierName { get; init; } = string.Empty;
    public decimal TotalAmount { get; init; }
    public string Currency { get; init; } = "USD";
    public string Status { get; init; } = string.Empty;
}

/// <summary>
/// Published when a Purchase Order exceeds pre-costing budget (AC-03 Hard Lock Rule).
/// Triggers CFO alerts and notification workflows.
/// </summary>
public class PurchaseOrderBudgetLockedIntegrationEvent : BaseIntegrationEvent
{
    public Guid PurchaseOrderId { get; init; }
    public string PurchaseOrderNumber { get; init; } = string.Empty;
    public Guid ItemId { get; init; }
    public string ItemName { get; init; } = string.Empty;
    public decimal BudgetUnitPrice { get; init; }
    public decimal QuotedUnitPrice { get; init; }
    public decimal VariancePercent { get; init; }
}

/// <summary>
/// Published when a Goods Receipt Note (GRN) is received at factory gate.
/// Directly moves stock into Quarantine Bin pending ASTM 4-point QA (AC-04).
/// </summary>
public class QuarantineStockReceivedIntegrationEvent : BaseIntegrationEvent
{
    public Guid GoodsReceiptNoteId { get; init; }
    public string GoodsReceiptNoteNumber { get; init; } = string.Empty;
    public Guid PurchaseOrderId { get; init; }
    public Guid InventoryItemId { get; init; }
    public string ItemDescription { get; init; } = string.Empty;
    public decimal ReceivedQuantity { get; init; }
    public string QuarantineBinCoordinate { get; init; } = "Q-01-A";
}

/// <summary>
/// Published when ASTM D5430-07 4-Point fabric inspection is completed by QA auditor.
/// </summary>
public class FabricInspectionCompletedIntegrationEvent : BaseIntegrationEvent
{
    public Guid InspectionId { get; init; }
    public string RollBarcode { get; init; } = string.Empty;
    public decimal PenaltyPointsPer100SqYards { get; init; }
    public bool IsAccepted { get; init; }
    public string AssignedShadeGroup { get; init; } = string.Empty; // Group A, B, C
}

/// <summary>
/// Published when IoT sewing station or operator triggers an Andon defect/stoppage alert.
/// Pushes to floor LED monitors and mechanics via SignalR/RabbitMQ (AC-05).
/// </summary>
public class AndonAlertTriggeredIntegrationEvent : BaseIntegrationEvent
{
    public string LineCode { get; init; } = string.Empty;
    public string WorkstationNumber { get; init; } = string.Empty;
    public string DefectCategory { get; init; } = string.Empty;
    public string Severity { get; init; } = "High"; // Low, Medium, High, Critical
    public string OperatorName { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Published when monthly dual-stream payroll calculation finishes (AC-07).
/// </summary>
public class DualPayrollCalculatedIntegrationEvent : BaseIntegrationEvent
{
    public int Year { get; init; }
    public int Month { get; init; }
    public int TotalWorkersProcessed { get; init; }
    public decimal TotalFormalBankDisbursement { get; init; }
    public decimal TotalFloorCashDisbursement { get; init; }
    public decimal GrossPayrollTotal { get; init; }
}
