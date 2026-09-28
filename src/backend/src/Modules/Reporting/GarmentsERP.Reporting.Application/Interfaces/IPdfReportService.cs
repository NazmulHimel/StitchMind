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
    Task<byte[]> GenerateThreeWayMatchCertificateAsync(ThreeWayMatchReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateOrderCostingVarianceReportAsync(OrderCostingVarianceReportModel model, CancellationToken ct = default);

    // HR, Compliance & Dual-Stream Payroll Reports (HR-RPT)
    Task<byte[]> GenerateBilingualPaySlipReportAsync(BilingualPaySlipReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateMasterSalarySheetReportAsync(MasterSalarySheetReportModel model, CancellationToken ct = default);
    Task<byte[]> GenerateSocialComplianceOtAuditReportAsync(SocialComplianceOtAuditReportModel model, CancellationToken ct = default);

    // Background Scheduler Executive Digest (SCH-02 & SCH-04)
    Task<byte[]> GenerateDailyExecutiveDigestReportAsync(DailyExecutiveDigestModel model, CancellationToken ct = default);
    Task<MemoryStream> RenderDailyExecutiveDigestToStreamAsync(DailyExecutiveDigestModel model, CancellationToken ct = default);
}
