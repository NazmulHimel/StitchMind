using GarmentsERP.Procurement.Application.Interfaces;
using GarmentsERP.Procurement.Domain.Entities;
using GarmentsERP.Procurement.Domain.Enums;
using GarmentsERP.Procurement.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Procurement.Infrastructure.Repositories;

public class ProcurementRepository(ProcurementDbContext db) : IProcurementRepository
{
    // Suppliers
    public async Task<Supplier?> GetSupplierByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.Suppliers
            .Include(s => s.PurchaseOrders)
            .FirstOrDefaultAsync(s => s.Id == id, ct);
    }

    public async Task<Supplier?> GetSupplierByCodeAsync(string code, CancellationToken ct = default)
    {
        return await db.Suppliers
            .FirstOrDefaultAsync(s => s.Code == code.Trim().ToUpper(), ct);
    }

    public async Task<IReadOnlyList<Supplier>> GetSuppliersAsync(
        Guid companyId, MaterialCategory? category = null, string? search = null, bool? isActive = null, CancellationToken ct = default)
    {
        var query = db.Suppliers.Where(s => s.CompanyId == companyId);

        if (category.HasValue)
        {
            query = query.Where(s => s.Category == category.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(s => s.IsActive == isActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(term) || s.Code.ToLower().Contains(term) || s.Email.ToLower().Contains(term));
        }

        return await query.OrderByDescending(s => s.CreatedAt).ToListAsync(ct);
    }

    public async Task AddSupplierAsync(Supplier supplier, CancellationToken ct = default)
    {
        await db.Suppliers.AddAsync(supplier, ct);
    }

    public Task UpdateSupplierAsync(Supplier supplier, CancellationToken ct = default)
    {
        db.Suppliers.Update(supplier);
        return Task.CompletedTask;
    }

    public Task DeleteSupplierAsync(Supplier supplier, CancellationToken ct = default)
    {
        db.Suppliers.Remove(supplier);
        return Task.CompletedTask;
    }

    // Purchase Orders
    public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.Id == id, ct);
    }

    public async Task<PurchaseOrder?> GetPurchaseOrderByNumberAsync(string poNumber, CancellationToken ct = default)
    {
        return await db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .FirstOrDefaultAsync(po => po.PONumber == poNumber.Trim().ToUpper(), ct);
    }

    public async Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(
        Guid companyId, POStatus? status = null, Guid? supplierId = null, string? search = null, CancellationToken ct = default)
    {
        var query = db.PurchaseOrders
            .Include(po => po.Supplier)
            .Include(po => po.Items)
            .Where(po => po.CompanyId == companyId);

        if (status.HasValue)
        {
            query = query.Where(po => po.Status == status.Value);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(po => po.SupplierId == supplierId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(po => po.PONumber.ToLower().Contains(term) || (po.Notes != null && po.Notes.ToLower().Contains(term)));
        }

        return await query.OrderByDescending(po => po.CreatedAt).ToListAsync(ct);
    }

    public async Task AddPurchaseOrderAsync(PurchaseOrder po, CancellationToken ct = default)
    {
        await db.PurchaseOrders.AddAsync(po, ct);
    }

    public Task UpdatePurchaseOrderAsync(PurchaseOrder po, CancellationToken ct = default)
    {
        db.PurchaseOrders.Update(po);
        return Task.CompletedTask;
    }

    // Goods Receipt Notes
    public async Task<GoodsReceiptNote?> GetGRNByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.GoodsReceiptNotes
            .Include(grn => grn.PurchaseOrder)
            .Include(grn => grn.Supplier)
            .Include(grn => grn.Items)
            .FirstOrDefaultAsync(grn => grn.Id == id, ct);
    }

    public async Task<GoodsReceiptNote?> GetGRNByNumberAsync(string grnNumber, CancellationToken ct = default)
    {
        return await db.GoodsReceiptNotes
            .Include(grn => grn.PurchaseOrder)
            .Include(grn => grn.Supplier)
            .Include(grn => grn.Items)
            .FirstOrDefaultAsync(grn => grn.GRNNumber == grnNumber.Trim().ToUpper(), ct);
    }

    public async Task<IReadOnlyList<GoodsReceiptNote>> GetGRNsAsync(
        Guid companyId, Guid? poId = null, Guid? supplierId = null, GRNStatus? status = null, CancellationToken ct = default)
    {
        var query = db.GoodsReceiptNotes
            .Include(grn => grn.PurchaseOrder)
            .Include(grn => grn.Supplier)
            .Include(grn => grn.Items)
            .Where(grn => grn.CompanyId == companyId);

        if (poId.HasValue)
        {
            query = query.Where(grn => grn.PurchaseOrderId == poId.Value);
        }

        if (supplierId.HasValue)
        {
            query = query.Where(grn => grn.SupplierId == supplierId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(grn => grn.Status == status.Value);
        }

        return await query.OrderByDescending(grn => grn.CreatedAt).ToListAsync(ct);
    }

    public async Task AddGRNAsync(GoodsReceiptNote grn, CancellationToken ct = default)
    {
        await db.GoodsReceiptNotes.AddAsync(grn, ct);
    }

    public Task UpdateGRNAsync(GoodsReceiptNote grn, CancellationToken ct = default)
    {
        db.GoodsReceiptNotes.Update(grn);
        return Task.CompletedTask;
    }

    // Metrics
    public async Task<(int TotalPOs, decimal TotalSpend, int PendingDeliveries, int CompletedPOs)> GetProcurementMetricsAsync(
        Guid companyId, CancellationToken ct = default)
    {
        var orders = await db.PurchaseOrders
            .Where(po => po.CompanyId == companyId)
            .ToListAsync(ct);

        var totalPOs = orders.Count;
        var totalSpend = orders.Where(po => po.Status != POStatus.Cancelled).Sum(po => po.TotalAmount);
        var pending = orders.Count(po => po.Status == POStatus.Approved || po.Status == POStatus.PartiallyReceived);
        var completed = orders.Count(po => po.Status == POStatus.Completed);

        return (totalPOs, totalSpend, pending, completed);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await db.SaveChangesAsync(ct);
    }
}
