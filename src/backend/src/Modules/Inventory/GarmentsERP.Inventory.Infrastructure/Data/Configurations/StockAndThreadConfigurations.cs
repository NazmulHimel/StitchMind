using GarmentsERP.Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Inventory.Infrastructure.Data.Configurations;

public class StockTransactionConfiguration : IEntityTypeConfiguration<StockTransaction>
{
    public void Configure(EntityTypeBuilder<StockTransaction> builder)
    {
        builder.ToTable("StockTransactions");
        builder.HasKey(t => t.Id);

        builder.Property(t => t.TransactionNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(t => t.TransactionNumber).IsUnique();

        builder.Property(t => t.Quantity).HasPrecision(18, 4);
        builder.Property(t => t.UnitCost).HasPrecision(18, 4);
        builder.Property(t => t.TotalCost).HasPrecision(18, 2);

        builder.Property(t => t.ReferenceNumber).HasMaxLength(100);
        builder.Property(t => t.SourceLocation).IsRequired().HasMaxLength(100);
        builder.Property(t => t.DestinationLocation).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Notes).HasMaxLength(500);
    }
}

public class ThreadBatchConfiguration : IEntityTypeConfiguration<ThreadBatch>
{
    public void Configure(EntityTypeBuilder<ThreadBatch> builder)
    {
        builder.ToTable("ThreadBatches");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.BatchNumber).IsRequired().HasMaxLength(50);
        builder.HasIndex(b => b.BatchNumber).IsUnique();

        builder.Property(b => b.YarnCount).IsRequired().HasMaxLength(50);
        builder.Property(b => b.FiberComposition).IsRequired().HasMaxLength(100);
        builder.Property(b => b.ColorCode).IsRequired().HasMaxLength(50);
        builder.Property(b => b.ColorName).IsRequired().HasMaxLength(100);

        builder.Property(b => b.TargetQuantityKg).HasPrecision(18, 4);
        builder.Property(b => b.CurrentQuantityKg).HasPrecision(18, 4);
        builder.Property(b => b.TotalWastageKg).HasPrecision(18, 4);
        builder.Property(b => b.Notes).HasMaxLength(500);

        builder.HasMany(b => b.StageLogs)
            .WithOne(l => l.ThreadBatch)
            .HasForeignKey(l => l.ThreadBatchId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class ThreadStageLogConfiguration : IEntityTypeConfiguration<ThreadStageLog>
{
    public void Configure(EntityTypeBuilder<ThreadStageLog> builder)
    {
        builder.ToTable("ThreadStageLogs");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.InputQuantityKg).HasPrecision(18, 4);
        builder.Property(l => l.OutputQuantityKg).HasPrecision(18, 4);
        builder.Property(l => l.WasteQuantityKg).HasPrecision(18, 4);

        builder.Property(l => l.MachineId).IsRequired().HasMaxLength(50);
        builder.Property(l => l.OperatorName).IsRequired().HasMaxLength(100);
        builder.Property(l => l.Remarks).HasMaxLength(500);
    }
}
