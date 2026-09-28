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
    public DbSet<MaterialRequisition> MaterialRequisitions => Set<MaterialRequisition>();
    public DbSet<MaterialRequisitionItem> MaterialRequisitionItems => Set<MaterialRequisitionItem>();
    public DbSet<ComparativeStatement> ComparativeStatements => Set<ComparativeStatement>();
    public DbSet<ComparativeStatementLine> ComparativeStatementLines => Set<ComparativeStatementLine>();
    public DbSet<CommercialLC> CommercialLCs => Set<CommercialLC>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProcurementDbContext).Assembly);
    }
}
