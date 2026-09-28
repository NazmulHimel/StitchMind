using GarmentsERP.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Inventory.Infrastructure.Data;

public class InventoryDbContext : DbContext
{
    public const string Schema = "inventory";

    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<InventoryItem> InventoryItems => Set<InventoryItem>();
    public DbSet<FabricRoll> FabricRolls => Set<FabricRoll>();
    public DbSet<StockTransaction> StockTransactions => Set<StockTransaction>();
    public DbSet<ThreadBatch> ThreadBatches => Set<ThreadBatch>();
    public DbSet<ThreadStageLog> ThreadStageLogs => Set<ThreadStageLog>();
    public DbSet<StockLot> StockLots => Set<StockLot>();
    public DbSet<FabricInspection> FabricInspections => Set<FabricInspection>();
    public DbSet<ThreadDyeingRecipe> ThreadDyeingRecipes => Set<ThreadDyeingRecipe>();
    public DbSet<RecipeIngredient> RecipeIngredients => Set<RecipeIngredient>();
    public DbSet<ThreadTensileTest> ThreadTensileTests => Set<ThreadTensileTest>();
    public DbSet<WarehouseLocation> WarehouseLocations => Set<WarehouseLocation>();
    public DbSet<CustomsBondLedger> CustomsBondLedgers => Set<CustomsBondLedger>();
    public DbSet<StoreIssueNote> StoreIssueNotes => Set<StoreIssueNote>();
    public DbSet<StoreIssueNoteItem> StoreIssueNoteItems => Set<StoreIssueNoteItem>();
    public DbSet<InventoryCycleCount> InventoryCycleCounts => Set<InventoryCycleCount>();
    public DbSet<InventoryCycleCountLine> InventoryCycleCountLines => Set<InventoryCycleCountLine>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
