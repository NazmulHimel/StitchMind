using GarmentsERP.Inventory.Domain.Entities;
using GarmentsERP.Inventory.Domain.Enums;

namespace GarmentsERP.Inventory.Application.Interfaces;

public interface IInventoryRepository
{
    // Inventory Items
    Task<InventoryItem?> GetItemByIdAsync(Guid id, CancellationToken ct = default);
    Task<InventoryItem?> GetItemByCodeAsync(string itemCode, CancellationToken ct = default);
    Task<IReadOnlyList<InventoryItem>> GetItemsAsync(Guid companyId, ItemCategory? category = null, string? search = null, bool lowStockOnly = false, CancellationToken ct = default);
    Task AddItemAsync(InventoryItem item, CancellationToken ct = default);
    Task UpdateItemAsync(InventoryItem item, CancellationToken ct = default);

    // Fabric Rolls
    Task<FabricRoll?> GetFabricRollByIdAsync(Guid id, CancellationToken ct = default);
    Task<FabricRoll?> GetFabricRollByNumberAsync(string rollNumber, CancellationToken ct = default);
    Task<IReadOnlyList<FabricRoll>> GetFabricRollsAsync(Guid companyId, Guid? itemId = null, string? dyeLot = null, RollStatus? status = null, FabricGrade? grade = null, CancellationToken ct = default);
    Task AddFabricRollAsync(FabricRoll roll, CancellationToken ct = default);
    Task UpdateFabricRollAsync(FabricRoll roll, CancellationToken ct = default);

    // Transactions
    Task<IReadOnlyList<StockTransaction>> GetTransactionsAsync(Guid companyId, Guid? itemId = null, StockTransactionType? type = null, int take = 50, CancellationToken ct = default);
    Task AddTransactionAsync(StockTransaction transaction, CancellationToken ct = default);

    // Thread Batches
    Task<ThreadBatch?> GetThreadBatchByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<ThreadBatch>> GetThreadBatchesAsync(Guid companyId, ThreadStage? stage = null, ThreadBatchStatus? status = null, CancellationToken ct = default);
    Task AddThreadBatchAsync(ThreadBatch batch, CancellationToken ct = default);
    Task UpdateThreadBatchAsync(ThreadBatch batch, CancellationToken ct = default);

    // Metrics
    Task<(int TotalSKUs, decimal TotalStockValue, int LowStockItems, int ActiveBatches)> GetInventoryMetricsAsync(Guid companyId, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
