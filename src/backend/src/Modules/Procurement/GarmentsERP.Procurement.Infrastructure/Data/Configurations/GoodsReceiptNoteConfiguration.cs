using GarmentsERP.Procurement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Procurement.Infrastructure.Data.Configurations;

public class GoodsReceiptNoteConfiguration : IEntityTypeConfiguration<GoodsReceiptNote>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNote> builder)
    {
        builder.ToTable("GoodsReceiptNotes");
        builder.HasKey(grn => grn.Id);

        builder.Property(grn => grn.GRNNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(grn => grn.GRNNumber).IsUnique();

        builder.Property(grn => grn.DeliveryChallanNo).IsRequired().HasMaxLength(100);
        builder.Property(grn => grn.VehicleNo).HasMaxLength(50);
        builder.Property(grn => grn.Notes).HasMaxLength(1000);

        builder.HasMany(grn => grn.Items)
            .WithOne(i => i.GoodsReceiptNote)
            .HasForeignKey(i => i.GoodsReceiptNoteId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class GoodsReceiptNoteItemConfiguration : IEntityTypeConfiguration<GoodsReceiptNoteItem>
{
    public void Configure(EntityTypeBuilder<GoodsReceiptNoteItem> builder)
    {
        builder.ToTable("GoodsReceiptNoteItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemCode).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Description).IsRequired().HasMaxLength(250);
        builder.Property(i => i.ReceivedQuantity).HasPrecision(18, 4);
        builder.Property(i => i.AcceptedQuantity).HasPrecision(18, 4);
        builder.Property(i => i.RejectedQuantity).HasPrecision(18, 4);
        builder.Property(i => i.RejectionReason).HasMaxLength(500);
    }
}
