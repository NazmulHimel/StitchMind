using GarmentsERP.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Inventory.Infrastructure.Data.Configurations;

public class InventoryItemConfiguration : IEntityTypeConfiguration<InventoryItem>
{
    public void Configure(EntityTypeBuilder<InventoryItem> builder)
    {
        builder.ToTable("InventoryItems");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.ItemCode).IsRequired().HasMaxLength(50);
        builder.HasIndex(i => i.ItemCode).IsUnique();

        builder.Property(i => i.Name).IsRequired().HasMaxLength(200);
        builder.Property(i => i.Description).HasMaxLength(500);
        builder.Property(i => i.BinLocation).IsRequired().HasMaxLength(50);

        builder.Property(i => i.CurrentStock).HasPrecision(18, 4);
        builder.Property(i => i.SafetyStock).HasPrecision(18, 4);
        builder.Property(i => i.ReorderPoint).HasPrecision(18, 4);
        builder.Property(i => i.StandardCost).HasPrecision(18, 4);

        builder.HasMany(i => i.FabricRolls)
            .WithOne(r => r.InventoryItem)
            .HasForeignKey(r => r.InventoryItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(i => i.Transactions)
            .WithOne(t => t.InventoryItem)
            .HasForeignKey(t => t.InventoryItemId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class FabricRollConfiguration : IEntityTypeConfiguration<FabricRoll>
{
    public void Configure(EntityTypeBuilder<FabricRoll> builder)
    {
        builder.ToTable("FabricRolls");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.RollNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(r => r.RollNumber).IsUnique();

        builder.Property(r => r.DyeLotNumber).IsRequired().HasMaxLength(50);
        builder.Property(r => r.RackLocation).IsRequired().HasMaxLength(50);
        builder.Property(r => r.SupplierRollNo).HasMaxLength(50);

        builder.Property(r => r.GSM).HasPrecision(10, 2);
        builder.Property(r => r.WidthInches).HasPrecision(10, 2);
        builder.Property(r => r.TotalLengthYards).HasPrecision(18, 4);
        builder.Property(r => r.AvailableLengthYards).HasPrecision(18, 4);
    }
}
