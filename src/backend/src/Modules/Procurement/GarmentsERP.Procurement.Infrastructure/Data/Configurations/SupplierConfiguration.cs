using GarmentsERP.Procurement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Procurement.Infrastructure.Data.Configurations;

public class SupplierConfiguration : IEntityTypeConfiguration<Supplier>
{
    public void Configure(EntityTypeBuilder<Supplier> builder)
    {
        builder.ToTable("Suppliers");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(s => s.Code).IsUnique();

        builder.Property(s => s.Name).IsRequired().HasMaxLength(200);
        builder.Property(s => s.Email).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Phone).IsRequired().HasMaxLength(50);
        builder.Property(s => s.Address).IsRequired().HasMaxLength(500);
        builder.Property(s => s.Rating).HasPrecision(3, 2);

        builder.HasMany(s => s.PurchaseOrders)
            .WithOne(po => po.Supplier)
            .HasForeignKey(po => po.SupplierId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
