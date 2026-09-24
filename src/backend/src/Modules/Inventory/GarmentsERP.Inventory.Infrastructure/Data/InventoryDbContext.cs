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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InventoryDbContext).Assembly);
    }
}
