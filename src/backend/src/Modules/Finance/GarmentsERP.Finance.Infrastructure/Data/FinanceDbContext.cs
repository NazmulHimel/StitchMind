using GarmentsERP.Finance.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Finance.Infrastructure.Data;

public class FinanceDbContext : DbContext
{
    public FinanceDbContext(DbContextOptions<FinanceDbContext> options) : base(options)
    {
    }

    public DbSet<Account> Accounts => Set<Account>();
    public DbSet<JournalVoucher> JournalVouchers => Set<JournalVoucher>();
    public DbSet<JournalEntryLine> JournalEntryLines => Set<JournalEntryLine>();
    public DbSet<VendorBill> VendorBills => Set<VendorBill>();
    public DbSet<ThreeWayMatchLog> ThreeWayMatchLogs => Set<ThreeWayMatchLog>();
    public DbSet<MasterLetterOfCredit> MasterLetterOfCredits => Set<MasterLetterOfCredit>();
    public DbSet<BackToBackLetterOfCredit> BackToBackLetterOfCredits => Set<BackToBackLetterOfCredit>();
    public DbSet<ForeignBillDiscounting> ForeignBillDiscountings => Set<ForeignBillDiscounting>();
    public DbSet<OrderCostingVariance> OrderCostingVariances => Set<OrderCostingVariance>();
    public DbSet<CurrencyExchangeRate> CurrencyExchangeRates => Set<CurrencyExchangeRate>();
    public DbSet<FixedAsset> FixedAssets => Set<FixedAsset>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("finance");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(FinanceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
