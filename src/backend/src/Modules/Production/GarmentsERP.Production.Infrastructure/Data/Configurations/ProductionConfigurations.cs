using GarmentsERP.Production.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Production.Infrastructure.Data.Configurations;

public class BuyerConfiguration : IEntityTypeConfiguration<Buyer>
{
    public void Configure(EntityTypeBuilder<Buyer> builder)
    {
        builder.ToTable("Buyers");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.Name).HasMaxLength(150).IsRequired();
        builder.Property(b => b.Code).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Country).HasMaxLength(100);
        builder.Property(b => b.ContactPerson).HasMaxLength(100);
        builder.Property(b => b.Email).HasMaxLength(150);
        builder.Property(b => b.Phone).HasMaxLength(50);
        builder.Property(b => b.PaymentTerms).HasMaxLength(100);
        builder.HasIndex(b => b.Code).IsUnique();
    }
}

public class BuyerOrderConfiguration : IEntityTypeConfiguration<BuyerOrder>
{
    public void Configure(EntityTypeBuilder<BuyerOrder> builder)
    {
        builder.ToTable("BuyerOrders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(o => o.StyleNumber).HasMaxLength(100).IsRequired();
        builder.Property(o => o.StyleName).HasMaxLength(150).IsRequired();
        builder.Property(o => o.Season).HasMaxLength(50);
        builder.Property(o => o.UnitPrice).HasPrecision(18, 2);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.HasOne(o => o.Buyer)
               .WithMany(b => b.Orders)
               .HasForeignKey(o => o.BuyerId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CuttingOrderConfiguration : IEntityTypeConfiguration<CuttingOrder>
{
    public void Configure(EntityTypeBuilder<CuttingOrder> builder)
    {
        builder.ToTable("CuttingOrders");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.CuttingOrderNumber).HasMaxLength(50).IsRequired();
        builder.Property(c => c.StyleNumber).HasMaxLength(100).IsRequired();
        builder.Property(c => c.FabricType).HasMaxLength(150).IsRequired();
        builder.Property(c => c.MarkerLength).HasPrecision(18, 2);
        builder.Property(c => c.FabricWidth).HasPrecision(18, 2);
        builder.Property(c => c.EfficiencyPercentage).HasPrecision(18, 2);
        builder.Property(c => c.TableNumber).HasMaxLength(50);
        builder.HasIndex(c => c.CuttingOrderNumber).IsUnique();

        builder.HasOne(c => c.BuyerOrder)
               .WithMany(o => o.CuttingOrders)
               .HasForeignKey(c => c.BuyerOrderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class CutBundleConfiguration : IEntityTypeConfiguration<CutBundle>
{
    public void Configure(EntityTypeBuilder<CutBundle> builder)
    {
        builder.ToTable("CutBundles");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.BundleNumber).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Size).HasMaxLength(20).IsRequired();
        builder.Property(b => b.Color).HasMaxLength(50).IsRequired();
        builder.Property(b => b.Barcode).HasMaxLength(100).IsRequired();

        builder.HasOne(b => b.CuttingOrder)
               .WithMany(c => c.Bundles)
               .HasForeignKey(b => b.CuttingOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class SewingLineConfiguration : IEntityTypeConfiguration<SewingLine>
{
    public void Configure(EntityTypeBuilder<SewingLine> builder)
    {
        builder.ToTable("SewingLines");
        builder.HasKey(s => s.Id);
        builder.Property(s => s.LineNumber).HasMaxLength(50).IsRequired();
        builder.Property(s => s.LineName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.Floor).HasMaxLength(50);
        builder.Property(s => s.TargetEfficiency).HasPrecision(18, 2);
        builder.Property(s => s.Supervisor).HasMaxLength(100);
        builder.HasIndex(s => s.LineNumber).IsUnique();
    }
}

public class HourlyProductionLogConfiguration : IEntityTypeConfiguration<HourlyProductionLog>
{
    public void Configure(EntityTypeBuilder<HourlyProductionLog> builder)
    {
        builder.ToTable("HourlyProductionLogs");
        builder.HasKey(l => l.Id);

        builder.HasOne(l => l.SewingLine)
               .WithMany(s => s.ProductionLogs)
               .HasForeignKey(l => l.SewingLineId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.BuyerOrder)
               .WithMany(o => o.ProductionLogs)
               .HasForeignKey(l => l.BuyerOrderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}

public class QualityInspectionConfiguration : IEntityTypeConfiguration<QualityInspection>
{
    public void Configure(EntityTypeBuilder<QualityInspection> builder)
    {
        builder.ToTable("QualityInspections");
        builder.HasKey(q => q.Id);
        builder.Property(q => q.InspectionNumber).HasMaxLength(50).IsRequired();
        builder.Property(q => q.LotOrBatchRef).HasMaxLength(100).IsRequired();
        builder.Property(q => q.InspectorName).HasMaxLength(100).IsRequired();
        builder.Property(q => q.TotalPoints).HasPrecision(18, 2);
        builder.HasIndex(q => q.InspectionNumber).IsUnique();

        builder.HasOne(q => q.BuyerOrder)
               .WithMany(o => o.QualityInspections)
               .HasForeignKey(q => q.BuyerOrderId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
