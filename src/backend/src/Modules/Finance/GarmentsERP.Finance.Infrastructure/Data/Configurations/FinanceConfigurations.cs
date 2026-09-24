using GarmentsERP.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GarmentsERP.Finance.Infrastructure.Data.Configurations;

public class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("Accounts", "finance");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AccountCode).HasMaxLength(50).IsRequired();
        builder.HasIndex(a => a.AccountCode).IsUnique();

        builder.Property(a => a.AccountName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Category).IsRequired();
        builder.Property(a => a.Currency).HasMaxLength(10).HasDefaultValue("BDT");
        builder.Property(a => a.OpeningBalance).HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(a => a.CurrentBalance).HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(a => a.Description).HasMaxLength(500);

        builder.HasMany(a => a.JournalLines)
            .WithOne(l => l.Account)
            .HasForeignKey(l => l.AccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class JournalVoucherConfiguration : IEntityTypeConfiguration<JournalVoucher>
{
    public void Configure(EntityTypeBuilder<JournalVoucher> builder)
    {
        builder.ToTable("JournalVouchers", "finance");
        builder.HasKey(v => v.Id);

        builder.Property(v => v.VoucherNumber).HasMaxLength(50).IsRequired();
        builder.HasIndex(v => v.VoucherNumber).IsUnique();

        builder.Property(v => v.Type).IsRequired();
        builder.Property(v => v.ReferenceType).HasMaxLength(50);
        builder.Property(v => v.ReferenceNumber).HasMaxLength(100);
        builder.Property(v => v.Narration).HasMaxLength(1000).IsRequired();
        builder.Property(v => v.TotalDebit).HasPrecision(18, 4);
        builder.Property(v => v.TotalCredit).HasPrecision(18, 4);
        builder.Property(v => v.Status).IsRequired();
        builder.Property(v => v.PostedBy).HasMaxLength(100);

        builder.HasMany(v => v.Lines)
            .WithOne(l => l.JournalVoucher)
            .HasForeignKey(l => l.JournalVoucherId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public class JournalEntryLineConfiguration : IEntityTypeConfiguration<JournalEntryLine>
{
    public void Configure(EntityTypeBuilder<JournalEntryLine> builder)
    {
        builder.ToTable("JournalEntryLines", "finance");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.DebitAmount).HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(l => l.CreditAmount).HasPrecision(18, 4).HasDefaultValue(0);
        builder.Property(l => l.LineNarration).HasMaxLength(500);
    }
}
