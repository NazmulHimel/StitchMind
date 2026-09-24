using GarmentsERP.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Auth.Infrastructure.Data.Configurations;

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Module).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Action).HasMaxLength(50).IsRequired();
        builder.Property(p => p.DisplayName).HasMaxLength(150).IsRequired();

        builder.HasIndex(p => new { p.Module, p.Action }).IsUnique();

        builder.HasQueryFilter(p => !p.IsDeleted);

        builder.HasMany(p => p.RolePermissions)
            .WithOne(rp => rp.Permission)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
