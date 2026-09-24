using GarmentsERP.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Production.Infrastructure.Data;

public class ProductionDbContext : DbContext
{
    public ProductionDbContext(DbContextOptions<ProductionDbContext> options) : base(options)
    {
    }

    public DbSet<Buyer> Buyers => Set<Buyer>();
    public DbSet<BuyerOrder> BuyerOrders => Set<BuyerOrder>();
    public DbSet<CuttingOrder> CuttingOrders => Set<CuttingOrder>();
    public DbSet<CutBundle> CutBundles => Set<CutBundle>();
    public DbSet<SewingLine> SewingLines => Set<SewingLine>();
    public DbSet<HourlyProductionLog> HourlyProductionLogs => Set<HourlyProductionLog>();
    public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
    public DbSet<CartonPack> CartonPacks => Set<CartonPack>();
    public DbSet<DeliveryChallan> DeliveryChallans => Set<DeliveryChallan>();
    public DbSet<DeliveryChallanItem> DeliveryChallanItems => Set<DeliveryChallanItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("production");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProductionDbContext).Assembly);
    }
}
