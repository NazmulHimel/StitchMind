using GarmentsERP.Procurement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Procurement.Infrastructure.Data.Configurations;

public class PurchaseOrderConfiguration : IEntityTypeConfiguration<PurchaseOrder>
{
    public void Configure(EntityTypeBuilder<PurchaseOrder> builder)
    {
        builder.ToTable("PurchaseOrders");
        builder.HasKey(po => po.Id);

        builder.Property(po => po.PONumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(po => po.PONumber).IsUnique();

        builder.Property(po => po.TotalAmount).HasPrecision(18, 2);
        builder.Property(po => po.Notes).HasMaxLength(1000);

        builder.HasMany(po => po.Items)
            .WithOne(i => i.PurchaseOrder)
            .HasForeignKey(i => i.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(po => po.GoodsReceiptNotes)
            .WithOne(grn => grn.PurchaseOrder)
            .HasForeignKey(grn => grn.PurchaseOrderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class PurchaseOrderItemConfiguration : IEntityTypeConfiguration<PurchaseOrderItem>
{
    public void Configure(EntityTypeBuilder<PurchaseOrderItem> builder)
    {
        builder.ToTable("PurchaseOrderItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemCode).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(250);
        builder.Property(i => i.Quantity).HasPrecision(18, 4);
        builder.Property(i => i.UnitPrice).HasPrecision(18, 4);
        builder.Property(i => i.TotalPrice).HasPrecision(18, 2);
        builder.Property(i => i.ReceivedQuantity).HasPrecision(18, 4);
    }
}
