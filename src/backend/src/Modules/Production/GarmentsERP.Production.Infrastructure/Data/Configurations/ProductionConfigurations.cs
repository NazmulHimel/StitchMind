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
        builder.Property(o => o.ProductCategory).HasMaxLength(100);
        builder.Property(o => o.FabricComposition).HasMaxLength(200);
        builder.Property(o => o.Currency).HasMaxLength(10);
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.HasOne(o => o.Buyer)
               .WithMany(b => b.Orders)
               .HasForeignKey(o => o.BuyerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.ColorSizes)
               .WithOne(cs => cs.BuyerOrder)
               .HasForeignKey(cs => cs.BuyerOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.BOMItems)
               .WithOne(b => b.BuyerOrder)
               .HasForeignKey(b => b.BuyerOrderId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(o => o.PreCosting)
               .WithOne(pc => pc.BuyerOrder)
               .HasForeignKey<PreCosting>(pc => pc.BuyerOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class StyleColorSizeConfiguration : IEntityTypeConfiguration<StyleColorSize>
{
    public void Configure(EntityTypeBuilder<StyleColorSize> builder)
    {
        builder.ToTable("StyleColorSizes");
        builder.HasKey(cs => cs.Id);
        builder.Property(cs => cs.ColorName).HasMaxLength(100).IsRequired();
        builder.Property(cs => cs.PantoneCode).HasMaxLength(50);
        builder.Property(cs => cs.SizeLabel).HasMaxLength(50).IsRequired();
    }
}

public class BOMItemConfiguration : IEntityTypeConfiguration<BOMItem>
{
    public void Configure(EntityTypeBuilder<BOMItem> builder)
    {
        builder.ToTable("BOMItems");
        builder.HasKey(b => b.Id);
        builder.Property(b => b.ItemCategory).HasMaxLength(100).IsRequired();
        builder.Property(b => b.ItemCode).HasMaxLength(50).IsRequired();
        builder.Property(b => b.ItemDescription).HasMaxLength(250).IsRequired();
        builder.Property(b => b.UnitOfMeasure).HasMaxLength(50).IsRequired();
        builder.Property(b => b.NetConsumptionPerDzn).HasPrecision(18, 4);
        builder.Property(b => b.WastagePercentage).HasPrecision(5, 2);
        builder.Property(b => b.TotalRequiredQty).HasPrecision(18, 4);
    }
}

public class PreCostingConfiguration : IEntityTypeConfiguration<PreCosting>
{
    public void Configure(EntityTypeBuilder<PreCosting> builder)
    {
        builder.ToTable("PreCostings");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.CostingBasis).HasMaxLength(50);
        builder.Property(p => p.FOBPricePerDzn).HasPrecision(18, 4);
        builder.Property(p => p.TotalMaterialsBudget).HasPrecision(18, 4);
        builder.Property(p => p.TotalLaborCostPerDzn).HasPrecision(18, 4);
        builder.Property(p => p.TotalOverheadPerDzn).HasPrecision(18, 4);
        builder.Property(p => p.NetMarginPerDzn).HasPrecision(18, 4);

        builder.HasMany(p => p.Lines)
               .WithOne(l => l.PreCosting)
               .HasForeignKey(l => l.PreCostingId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}

public class CostingLineConfiguration : IEntityTypeConfiguration<CostingLine>
{
    public void Configure(EntityTypeBuilder<CostingLine> builder)
    {
        builder.ToTable("CostingLines");
        builder.HasKey(l => l.Id);
        builder.Property(l => l.Category).HasMaxLength(100).IsRequired();
        builder.Property(l => l.ItemDescription).HasMaxLength(250).IsRequired();
        builder.Property(l => l.ConsumptionPerDzn).HasPrecision(18, 4);
        builder.Property(l => l.UnitRate).HasPrecision(18, 4);
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
