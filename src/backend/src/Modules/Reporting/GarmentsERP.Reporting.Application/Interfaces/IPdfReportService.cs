namespace GarmentsERP.Reporting.Application.Interfaces;

using GarmentsERP.Reporting.Application.Models;

public interface IPdfReportService
{
    Task<byte[]> GeneratePurchaseOrderReportAsync(PurchaseOrderReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateDailyKpiReportAsync(DailyKpiReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateAuditTrailReportAsync(AuditTrailReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateFabricInspectionCertificateAsync(FabricInspectionReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateCustomsBondReportAsync(CustomsBondReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateCuttingLaySheetAsync(CuttingLaySheetReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateBundleTicketsAsync(BundleTicketReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateHourlyProductionReportAsync(HourlyProductionReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateBrokenNeedleCertificateAsync(BrokenNeedleReportModel model, CancellationToken ct = default);
}
