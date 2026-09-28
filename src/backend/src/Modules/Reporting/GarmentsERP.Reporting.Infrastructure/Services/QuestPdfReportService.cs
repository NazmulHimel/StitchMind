namespace GarmentsERP.Reporting.Infrastructure.Services;

using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using GarmentsERP.Reporting.Infrastructure.Documents;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

public class QuestPdfReportService : IPdfReportService
{
    public QuestPdfReportService()
    {
        // Configure QuestPDF Community license
        QuestPDF.Settings.License = LicenseType.Community;
        QuestPDF.Settings.UseSystemFonts = true;
        QuestPDF.Settings.ThrowOnMissingFontFamilies = false;
    }

    public Task<byte[]> GeneratePurchaseOrderReportAsync(PurchaseOrderReportModel model, CancellationToken ct = default)
    {
        var document = new PurchaseOrderPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateDailyKpiReportAsync(DailyKpiReportModel model, CancellationToken ct = default)
    {
        // Placeholder for Daily KPI QuestPDF compilation
        var document = new PurchaseOrderPdfDocument(new PurchaseOrderReportModel
        {
            PONumber = $"KPI-{model.ReportDate:yyyyMMdd}",
            SupplierName = "StitchMind Executive Digest",
            Notes = $"Daily Output: {model.ActualGarmentOutput}/{model.PlannedGarmentOutput} Pcs | Efficiency: {model.OverallFactoryEfficiencyPct:F1}%"
        });
        return Task.FromResult(document.GeneratePdf());
    }

    public Task<byte[]> GenerateAuditTrailReportAsync(AuditTrailReportModel model, CancellationToken ct = default)
    {
        var document = new AuditTrailPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateFabricInspectionCertificateAsync(FabricInspectionReportModel model, CancellationToken ct = default)
    {
        var document = new FabricInspectionPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateCustomsBondReportAsync(CustomsBondReportModel model, CancellationToken ct = default)
    {
        var document = new CustomsBondPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateCuttingLaySheetAsync(CuttingLaySheetReportModel model, CancellationToken ct = default)
    {
        var document = new CuttingLaySheetPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateBundleTicketsAsync(BundleTicketReportModel model, CancellationToken ct = default)
    {
        var document = new BundleTicketSheetPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateHourlyProductionReportAsync(HourlyProductionReportModel model, CancellationToken ct = default)
    {
        var document = new HourlyProductionPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateBrokenNeedleCertificateAsync(BrokenNeedleReportModel model, CancellationToken ct = default)
    {
        var document = new BrokenNeedlePdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateThreeWayMatchCertificateAsync(ThreeWayMatchReportModel model, CancellationToken ct = default)
    {
        var document = new ThreeWayMatchPdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }

    public Task<byte[]> GenerateOrderCostingVarianceReportAsync(OrderCostingVarianceReportModel model, CancellationToken ct = default)
    {
        var document = new OrderCostingVariancePdfDocument(model);
        var pdfBytes = document.GeneratePdf();
        return Task.FromResult(pdfBytes);
    }
}

