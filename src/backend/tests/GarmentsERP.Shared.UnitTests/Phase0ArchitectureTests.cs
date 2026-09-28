using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Services;
using GarmentsERP.Shared.Domain;
using GarmentsERP.Shared.Extensions;
using GarmentsERP.Shared.Interceptors;
using GarmentsERP.Shared.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace GarmentsERP.Shared.UnitTests;

// Test entity inheriting from BaseEntity
public class SampleGarmentItem : BaseEntity
{
    public string ItemCode { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
}

// Test DbContext incorporating AuditAndSoftDeleteInterceptor
public class TestDbContext : DbContext
{
    private readonly AuditAndSoftDeleteInterceptor _interceptor;

    public TestDbContext(DbContextOptions<TestDbContext> options, AuditAndSoftDeleteInterceptor interceptor)
        : base(options)
    {
        _interceptor = interceptor;
    }

    public DbSet<SampleGarmentItem> Items => Set<SampleGarmentItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.AddInterceptors(_interceptor);
        base.OnConfiguring(optionsBuilder);
    }
}

// Mock ICurrentUserService
public class MockCurrentUserService : ICurrentUserService
{
    public Guid UserId { get; set; } = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public string UserName => "test.merchandiser@stitchmind.com";
    public Guid CompanyId => Guid.Parse("22222222-2222-2222-2222-222222222222");
    public bool IsAuthenticated => true;
    public string? IpAddress => "127.0.0.1";
    public bool CanViewFinancialCosts { get; set; } = true;
    public IReadOnlyList<string> Permissions => ["Procurement.View", "Procurement.Create"];
    public bool HasPermission(string permission) => true;
}

public class Phase0ArchitectureTests
{
    [Fact]
    public async Task AuditAndSoftDeleteInterceptor_ShouldStampAuditFieldsAndHandleSoftDelete()
    {
        // Arrange
        var currentUserService = new MockCurrentUserService();
        var interceptor = new AuditAndSoftDeleteInterceptor(currentUserService);

        var options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        await using var context = new TestDbContext(options, interceptor);

        // Act 1: Add entity
        var item = new SampleGarmentItem
        {
            ItemCode = "FAB-TWILL-01",
            Description = "100% Cotton Chino Fabric"
        };
        context.Items.Add(item);
        await context.SaveChangesAsync();

        // Assert 1: CreatedAt and CreatedBy are set
        Assert.NotEqual(default, item.CreatedAt);
        Assert.Equal(currentUserService.UserId, item.CreatedBy);
        Assert.False(item.IsDeleted);

        // Act 2: Update entity
        item.Description = "Updated 100% Cotton Chino 240 GSM";
        context.Items.Update(item);
        await context.SaveChangesAsync();

        // Assert 2: UpdatedAt and UpdatedBy are stamped
        Assert.NotNull(item.UpdatedAt);
        Assert.Equal(currentUserService.UserId, item.UpdatedBy);

        // Act 3: Soft Delete
        context.Items.Remove(item);
        await context.SaveChangesAsync();

        // Assert 3: Entity is marked IsDeleted without being physically removed from db
        Assert.True(item.IsDeleted);
        Assert.NotNull(item.DeletedAt);
        Assert.Equal(currentUserService.UserId, item.DeletedBy);
    }

    [Fact]
    public async Task AddEnterpriseEventBus_WithInMemoryConfiguration_ShouldRegisterBusServices()
    {
        // Arrange
        var services = new ServiceCollection();
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "EventBus:Provider", "InMemory" }
        };
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        // Act
        services.AddEnterpriseEventBus(configuration);
        await using var provider = services.BuildServiceProvider();

        // Assert: MassTransit IBusRegistrationConfigurator or IBusControl can be resolved
        var busControl = provider.GetService<MassTransit.IBusControl>();
        Assert.NotNull(busControl);
    }

    [Fact]
    public async Task QuestPdfReportService_ShouldGenerateValidPurchaseOrderPdf()
    {
        // Arrange
        var reportService = new QuestPdfReportService();
        var model = new PurchaseOrderReportModel
        {
            PONumber = "PO-2026-0145",
            SupplierName = "Pacific Cotton Mills Ltd.",
            SupplierAddress = "Plot 12-14, DEPZ, Savar, Dhaka, Bangladesh",
            PaymentTerms = "Back-to-Back LC at 90 Days Sight",
            StyleNumber = "ST-2026-CHINO-01",
            BuyerName = "H&M Global Sourcing",
            OrderDate = DateTime.UtcNow,
            ExpectedDeliveryDate = DateTime.UtcNow.AddDays(21),
            Currency = "USD",
            IsBudgetLocked = true,
            Notes = "ASTM 4-Point inspection required before cutting.",
            Items =
            [
                new()
                {
                    ItemCode = "FAB-TWILL-240",
                    Description = "100% Cotton Twill 240 GSM (Navy Blue)",
                    UnitOfMeasure = "Yds",
                    Quantity = 15000,
                    UnitPrice = 2.25m,
                    BudgetUnitPrice = 2.20m
                }
            ]
        };

        // Act
        var pdfBytes = await reportService.GeneratePurchaseOrderReportAsync(model);

        // Assert: Valid PDF byte signature (%PDF-)
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 1000, "Generated PDF should be at least 1KB");
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes.Take(4).ToArray());
        Assert.Equal("%PDF", header);
    }

    [Fact]
    public async Task QuestPdfReportService_ShouldGenerateValidDailyKpiPdf()
    {
        // Arrange
        var reportService = new QuestPdfReportService();
        var model = new DailyKpiReportModel
        {
            ReportDate = DateTime.UtcNow,
            ShiftName = "Day Shift (A)",
            PlannedGarmentOutput = 12000,
            ActualGarmentOutput = 11450,
            OverallFactoryEfficiencyPct = 95.4m,
            RunningDHUPct = 1.2m,
            PresentOperators = 480,
            TotalOperators = 500,
            LineOutputs =
            [
                new() { LineNumber = "Line 04", StyleNumber = "ST-2026-CHINO-01", HourlyTarget = 150, ActualOutput = 148, EfficiencyPct = 98.7m, Status = "Optimal" }
            ]
        };

        // Act
        var pdfBytes = await reportService.GenerateDailyKpiReportAsync(model);

        // Assert: Valid PDF byte signature (%PDF-)
        Assert.NotNull(pdfBytes);
        Assert.True(pdfBytes.Length > 1000, "Generated PDF should be at least 1KB");
        var header = System.Text.Encoding.ASCII.GetString(pdfBytes.Take(4).ToArray());
        Assert.Equal("%PDF", header);
    }
}
