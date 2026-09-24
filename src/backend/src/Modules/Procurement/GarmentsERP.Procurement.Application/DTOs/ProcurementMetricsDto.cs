namespace GarmentsERP.Procurement.Application.DTOs;

public record ProcurementMetricsDto(
    int TotalPurchaseOrders,
    decimal TotalSpend,
    int PendingDeliveries,
    int CompletedPOs,
    int TotalSuppliers,
    int ActiveSuppliers
);
