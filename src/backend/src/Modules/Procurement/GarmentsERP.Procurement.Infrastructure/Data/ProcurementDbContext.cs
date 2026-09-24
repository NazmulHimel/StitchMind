using GarmentsERP.Procurement.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Procurement.Infrastructure.Data;

public class ProcurementDbContext : DbContext
{
    public const string Schema = "procurement";

    public ProcurementDbContext(DbContextOptions<ProcurementDbContext> options) : base(options)
    {
    }

    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<PurchaseOrder> PurchaseOrders => Set<PurchaseOrder>();
    public DbSet<PurchaseOrderItem> PurchaseOrderItems => Set<PurchaseOrderItem>();
    public DbSet<GoodsReceiptNote> GoodsReceiptNotes => Set<GoodsReceiptNote>();
    public DbSet<GoodsReceiptNoteItem> GoodsReceiptNoteItems => Set<GoodsReceiptNoteItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProcurementDbContext).Assembly);
    }
}
