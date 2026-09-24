using GarmentsERP.Inventory.Application.Interfaces;
using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;
using GarmentsERP.Inventory.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Inventory.Infrastructure.Repositories;

public class InventoryRepository(InventoryDbContext db) : IInventoryRepository
{
    // Inventory Items
    public async Task<InventoryItem?> GetItemByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.InventoryItems
            .Include(i => i.FabricRolls)
            .FirstOrDefaultAsync(i => i.Id == id, ct);
    }

    public async Task<InventoryItem?> GetItemByCodeAsync(string itemCode, CancellationToken ct = default)
    {
        return await db.InventoryItems
            .FirstOrDefaultAsync(i => i.ItemCode == itemCode.Trim().ToUpper(), ct);
    }

    public async Task<IReadOnlyList<InventoryItem>> GetItemsAsync(
        Guid companyId, ItemCategory? category = null, string? search = null, bool lowStockOnly = false, CancellationToken ct = default)
    {
        var query = db.InventoryItems.Where(i => i.CompanyId == companyId);

        if (category.HasValue)
        {
            query = query.Where(i => i.Category == category.Value);
        }

        if (lowStockOnly)
        {
            query = query.Where(i => i.CurrentStock <= i.ReorderPoint);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i => i.Name.ToLower().Contains(term) || i.ItemCode.ToLower().Contains(term));
        }

        return await query.OrderBy(i => i.Name).ToListAsync(ct);
    }

    public async Task AddItemAsync(InventoryItem item, CancellationToken ct = default)
    {
        await db.InventoryItems.AddAsync(item, ct);
    }

    public Task UpdateItemAsync(InventoryItem item, CancellationToken ct = default)
    {
        db.InventoryItems.Update(item);
        return Task.CompletedTask;
    }

    // Fabric Rolls
    public async Task<FabricRoll?> GetFabricRollByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.FabricRolls
            .Include(r => r.InventoryItem)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task<FabricRoll?> GetFabricRollByNumberAsync(string rollNumber, CancellationToken ct = default)
    {
        return await db.FabricRolls
            .Include(r => r.InventoryItem)
            .FirstOrDefaultAsync(r => r.RollNumber == rollNumber.Trim().ToUpper(), ct);
    }

    public async Task<IReadOnlyList<FabricRoll>> GetFabricRollsAsync(
        Guid companyId, Guid? itemId = null, string? dyeLot = null, RollStatus? status = null, FabricGrade? grade = null, CancellationToken ct = default)
    {
        var query = db.FabricRolls
            .Include(r => r.InventoryItem)
            .Where(r => r.CompanyId == companyId);

        if (itemId.HasValue)
        {
            query = query.Where(r => r.InventoryItemId == itemId.Value);
        }

        if (!string.IsNullOrWhiteSpace(dyeLot))
        {
            query = query.Where(r => r.DyeLotNumber == dyeLot.Trim().ToUpper());
        }

        if (status.HasValue)
        {
            query = query.Where(r => r.Status == status.Value);
        }

        if (grade.HasValue)
        {
            query = query.Where(r => r.Grade == grade.Value);
        }

        return await query.OrderByDescending(r => r.CreatedAt).ToListAsync(ct);
    }

    public async Task AddFabricRollAsync(FabricRoll roll, CancellationToken ct = default)
    {
        await db.FabricRolls.AddAsync(roll, ct);
    }

    public Task UpdateFabricRollAsync(FabricRoll roll, CancellationToken ct = default)
    {
        db.FabricRolls.Update(roll);
        return Task.CompletedTask;
    }

    // Transactions
    public async Task<IReadOnlyList<StockTransaction>> GetTransactionsAsync(
        Guid companyId, Guid? itemId = null, StockTransactionType? type = null, int take = 50, CancellationToken ct = default)
    {
        var query = db.StockTransactions
            .Include(t => t.InventoryItem)
            .Where(t => t.CompanyId == companyId);

        if (itemId.HasValue)
        {
            query = query.Where(t => t.InventoryItemId == itemId.Value);
        }

        if (type.HasValue)
        {
            query = query.Where(t => t.TransactionType == type.Value);
        }

        return await query.OrderByDescending(t => t.CreatedAt).Take(take).ToListAsync(ct);
    }

    public async Task AddTransactionAsync(StockTransaction transaction, CancellationToken ct = default)
    {
        await db.StockTransactions.AddAsync(transaction, ct);
    }

    // Thread Batches
    public async Task<ThreadBatch?> GetThreadBatchByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await db.ThreadBatches
            .Include(b => b.StageLogs)
            .FirstOrDefaultAsync(b => b.Id == id, ct);
    }

    public async Task<IReadOnlyList<ThreadBatch>> GetThreadBatchesAsync(
        Guid companyId, ThreadStage? stage = null, ThreadBatchStatus? status = null, CancellationToken ct = default)
    {
        var query = db.ThreadBatches
            .Include(b => b.StageLogs)
            .Where(b => b.CompanyId == companyId);

        if (stage.HasValue)
        {
            query = query.Where(b => b.CurrentStage == stage.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(b => b.Status == status.Value);
        }

        return await query.OrderByDescending(b => b.CreatedAt).ToListAsync(ct);
    }

    public async Task AddThreadBatchAsync(ThreadBatch batch, CancellationToken ct = default)
    {
        await db.ThreadBatches.AddAsync(batch, ct);
    }

    public Task UpdateThreadBatchAsync(ThreadBatch batch, CancellationToken ct = default)
    {
        db.ThreadBatches.Update(batch);
        return Task.CompletedTask;
    }

    // Metrics
    public async Task<(int TotalSKUs, decimal TotalStockValue, int LowStockItems, int ActiveBatches)> GetInventoryMetricsAsync(
        Guid companyId, CancellationToken ct = default)
    {
        var items = await db.InventoryItems
            .Where(i => i.CompanyId == companyId && i.IsActive)
            .ToListAsync(ct);

        var totalSKUs = items.Count;
        var totalStockValue = items.Sum(i => i.CurrentStock * i.StandardCost);
        var lowStock = items.Count(i => i.CurrentStock <= i.ReorderPoint);

        var activeBatches = await db.ThreadBatches
            .CountAsync(b => b.CompanyId == companyId && b.Status == ThreadBatchStatus.InProgress, ct);

        return (totalSKUs, totalStockValue, lowStock, activeBatches);
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await db.SaveChangesAsync(ct);
    }
}
