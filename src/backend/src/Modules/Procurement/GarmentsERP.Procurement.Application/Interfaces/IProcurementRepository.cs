using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;

namespace GarmentsERP.Procurement.Application.Interfaces;

public interface IProcurementRepository
{
    // Suppliers
    Task<Supplier?> GetSupplierByIdAsync(Guid id, CancellationToken ct = default);
    Task<Supplier?> GetSupplierByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetSuppliersAsync(Guid companyId, MaterialCategory? category = null, string? search = null, bool? isActive = null, CancellationToken ct = default);
    Task AddSupplierAsync(Supplier supplier, CancellationToken ct = default);
    Task UpdateSupplierAsync(Supplier supplier, CancellationToken ct = default);
    Task DeleteSupplierAsync(Supplier supplier, CancellationToken ct = default);

    // Purchase Orders
    Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken ct = default);
    Task<PurchaseOrder?> GetPurchaseOrderByNumberAsync(string poNumber, CancellationToken ct = default);
    Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(Guid companyId, POStatus? status = null, Guid? supplierId = null, string? search = null, CancellationToken ct = default);
    Task AddPurchaseOrderAsync(PurchaseOrder po, CancellationToken ct = default);
    Task UpdatePurchaseOrderAsync(PurchaseOrder po, CancellationToken ct = default);

    // Goods Receipt Notes
    Task<GoodsReceiptNote?> GetGRNByIdAsync(Guid id, CancellationToken ct = default);
    Task<GoodsReceiptNote?> GetGRNByNumberAsync(string grnNumber, CancellationToken ct = default);
    Task<IReadOnlyList<GoodsReceiptNote>> GetGRNsAsync(Guid companyId, Guid? poId = null, Guid? supplierId = null, GRNStatus? status = null, CancellationToken ct = default);
    Task AddGRNAsync(GoodsReceiptNote grn, CancellationToken ct = default);
    Task UpdateGRNAsync(GoodsReceiptNote grn, CancellationToken ct = default);

    // Material Requisitions (PRO-01)
    Task<IReadOnlyList<MaterialRequisition>> GetMaterialRequisitionsAsync(Guid companyId, CancellationToken ct = default);
    Task<MaterialRequisition?> GetMaterialRequisitionByIdAsync(Guid id, CancellationToken ct = default);
    Task AddMaterialRequisitionAsync(MaterialRequisition requisition, CancellationToken ct = default);

    // Comparative Statements (PRO-02)
    Task<IReadOnlyList<ComparativeStatement>> GetComparativeStatementsAsync(Guid companyId, CancellationToken ct = default);
    Task<ComparativeStatement?> GetComparativeStatementByIdAsync(Guid id, CancellationToken ct = default);
    Task AddComparativeStatementAsync(ComparativeStatement cs, CancellationToken ct = default);

    // Commercial LCs (PRO-05)
    Task<IReadOnlyList<CommercialLC>> GetCommercialLCsAsync(Guid companyId, CancellationToken ct = default);
    Task<CommercialLC?> GetCommercialLCByIdAsync(Guid id, CancellationToken ct = default);
    Task AddCommercialLCAsync(CommercialLC lc, CancellationToken ct = default);

    // Metrics
    Task<(int TotalPOs, decimal TotalSpend, int PendingDeliveries, int CompletedPOs)> GetProcurementMetricsAsync(Guid companyId, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
