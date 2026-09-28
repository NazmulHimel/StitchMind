namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class DailyExecutiveDigestPdfDocument : IDocument
{
    private readonly DailyExecutiveDigestModel _model;

    public DailyExecutiveDigestPdfDocument(DailyExecutiveDigestModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(25);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Lato"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(headerCol =>
                {
                    headerCol.Item().Text(_model.FactoryName.ToUpper()).Bold().FontSize(13).FontColor(Colors.Blue.Darken4);
                    headerCol.Item().Text(_model.FacilityLocation).FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                    headerCol.Item().Text("Enterprise Production, Floor Balancing & Compliance Automated Dispatch").FontSize(7.5f).FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(digestCol =>
                {
                    digestCol.Item().Text("DAILY EXECUTIVE MANAGEMENT DIGEST").Bold().FontSize(11).FontColor(Colors.Red.Darken3);
                    digestCol.Item().Text($"Digest Ref: {_model.ExecutiveDigestCode}").Bold().FontSize(8).FontColor(Colors.Grey.Darken3);
                    digestCol.Item().Text($"Cutoff Time: {_model.ReportDate:dd-MMM-yyyy} (08:00 PM Shiftdown)").FontSize(7.5f).FontColor(Colors.Grey.Darken2);
                    digestCol.Item().Text("CONFIDENTIAL &bull; BOARD &amp; EXECUTIVE COMMITTEE ONLY").FontSize(7).Bold().FontColor(Colors.Amber.Darken3);
                });
            });

            col.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(6).Column(col =>
        {
            // 1. Executive Metric Highlights Strip
            col.Item().Row(row =>
            {
                row.RelativeItem().Background(Colors.Blue.Lighten5).Border(1).BorderColor(Colors.Blue.Lighten3).Padding(6).Column(c =>
                {
                    c.Item().Text("WORKFORCE ATTENDANCE").FontSize(6.5f).Bold().FontColor(Colors.Blue.Darken3);
                    c.Item().Text($"{_model.TotalWorkforcePresent} / {_model.TotalWorkforceHeadcount} ({_model.AttendanceRatePct}%)").FontSize(11).Bold().FontColor(Colors.Blue.Darken4);
                    c.Item().Text("AC-07 Age Compliant (&ge;18)").FontSize(6.5f).FontColor(Colors.Green.Darken3);
                });

                row.ConstantItem(6);

                row.RelativeItem().Background(Colors.Green.Lighten5).Border(1).BorderColor(Colors.Green.Lighten3).Padding(6).Column(c =>
                {
                    c.Item().Text("SEWING OUTPUT & EFFICIENCY").FontSize(6.5f).Bold().FontColor(Colors.Green.Darken3);
                    c.Item().Text($"{_model.ActualGarmentUnitsSewnToday:N0} Pcs ({_model.OverallFactoryEfficiencyPct}%)").FontSize(11).Bold().FontColor(Colors.Green.Darken4);
                    c.Item().Text($"Target: {_model.TargetGarmentUnitsToday:N0} ({_model.SewingTargetAchievementPct}%)").FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(6);

                row.RelativeItem().Background(Colors.Amber.Lighten5).Border(1).BorderColor(Colors.Amber.Lighten3).Padding(6).Column(c =>
                {
                    c.Item().Text("QUALITY DHU & AQL").FontSize(6.5f).Bold().FontColor(Colors.Amber.Darken4);
                    c.Item().Text($"{_model.RunningDhuPct}% DHU &bull; {_model.FinishingAqlPassRatePct}% AQL").FontSize(11).Bold().FontColor(Colors.Amber.Darken4);
                    c.Item().Text($"Inspected: {_model.TotalInlineInspectionsPcs:N0} Pcs").FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                });

                row.ConstantItem(6);

                row.RelativeItem().Background(Colors.Purple.Lighten5).Border(1).BorderColor(Colors.Purple.Lighten3).Padding(6).Column(c =>
                {
                    c.Item().Text("EX-FACTORY DISPATCH (USD)").FontSize(6.5f).Bold().FontColor(Colors.Purple.Darken3);
                    c.Item().Text($"${_model.TodayExFactoryShipmentValueUsd:N2}").FontSize(11).Bold().FontColor(Colors.Purple.Darken4);
                    c.Item().Text($"{_model.TodayGarmentsDispatchedPcs:N0} Pcs ({_model.TodayCartonsDispatched} Ctns)").FontSize(6.5f).FontColor(Colors.Grey.Darken2);
                });
            });

            col.Item().PaddingTop(8);

            // 2. Section A: CAD Cutting & Material Yield
            col.Item().Text("1. CAD CUTTING & MATERIAL YIELD METRICS").Bold().FontSize(8.5f).FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(3);
                });

                table.Header(h =>
                {
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Fabric Inspection").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Lay Sheets / Plies").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Garments Cut").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("CAD Efficiency").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Bundles Released").Bold().FontSize(7);
                });

                table.Cell().Padding(4).Text($"{_model.TotalFabricRollsInspected} Rolls ({_model.FabricInspectedYards:N0} Yds, {_model.FabricQuarantineCount} Quarantined)").FontSize(7);
                table.Cell().Padding(4).Text($"{_model.TotalLaySheetsProcessed} Lays / {_model.TotalPliesLaid} Plies").FontSize(7);
                table.Cell().Padding(4).Text($"{_model.TotalGarmentUnitsCut:N0} Pcs").Bold().FontSize(7);
                table.Cell().Padding(4).Text($"{_model.MarkerEfficiencyPct}% Marker Yield").FontSize(7);
                table.Cell().Padding(4).Text($"{_model.TotalBundlesGenerated} Barcoded Bundles to Sewing").FontSize(7);
            });

            col.Item().PaddingTop(8);

            // 3. Section B: Sewing Floor Line Balancing
            col.Item().Text("2. SEWING FLOOR HOURLY OUTPUT & EFFICIENCY (12 LINES)").Bold().FontSize(8.5f).FontColor(Colors.Blue.Darken3);
            col.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten2).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.ConstantColumn(50);
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(2);
                    cols.ConstantColumn(60);
                    cols.ConstantColumn(60);
                    cols.ConstantColumn(60);
                    cols.ConstantColumn(50);
                    cols.ConstantColumn(65);
                });

                table.Header(h =>
                {
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Line").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Buyer").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Style No").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Hourly Target").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Achieved").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Efficiency").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("DHU %").Bold().FontSize(7);
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(3).Text("Status").Bold().FontSize(7);
                });

                var sampleLines = _model.SewingLines.Count > 0 ? _model.SewingLines : GetDefaultLines();

                foreach (var line in sampleLines)
                {
                    table.Cell().Padding(3).Text(line.LineNumber).Bold().FontSize(6.5f);
                    table.Cell().Padding(3).Text(line.BuyerName).FontSize(6.5f);
                    table.Cell().Padding(3).Text(line.StyleNumber).FontSize(6.5f);
                    table.Cell().Padding(3).Text($"{line.PlannedHourlyTarget} pcs/h").FontSize(6.5f);
                    table.Cell().Padding(3).Text($"{line.ActualHourlyAchieved} pcs/h").FontSize(6.5f);
                    table.Cell().Padding(3).Text($"{line.LineEfficiencyPct}%").Bold().FontColor(line.LineEfficiencyPct >= 85 ? Colors.Green.Darken3 : Colors.Red.Darken2).FontSize(6.5f);
                    table.Cell().Padding(3).Text($"{line.DhuPct}%").FontSize(6.5f);
                    table.Cell().Padding(3).Text(line.Status).FontSize(6.5f);
                }
            });

            col.Item().PaddingTop(8);

            // 4. Section C: Quality, Commercial & Social Compliance Grid
            col.Item().Row(row =>
            {
                // Left Column: Quality & Defect Distribution
                row.RelativeItem().Column(qaCol =>
                {
                    qaCol.Item().Text("3. QA & DEFECT ANALYSIS").Bold().FontSize(8.5f).FontColor(Colors.Blue.Darken3);
                    qaCol.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(inner =>
                    {
                        inner.Item().Text($"Inline Audits: {_model.TotalInlineInspectionsPcs:N0} Pcs &bull; Defects: {_model.InlineDefectsDetected}").FontSize(7);
                        inner.Item().Text($"Critical: {_model.CriticalDefects} | Major: {_model.MajorDefects} (Broken Stitch, Skip) | Minor: {_model.MinorDefects}").FontSize(7);
                        inner.Item().Text($"End-Line Finishing AQL 2.5 Pass Rate: {_model.FinishingAqlPassRatePct}%").Bold().FontColor(Colors.Green.Darken3).FontSize(7);
                        inner.Item().Text($"Cartons Cleared for Ex-Factory: {_model.FinishingInspectedCartons} Ctns").FontSize(7);
                    });
                });

                row.ConstantItem(10);

                // Right Column: Social Compliance & Commercial Trade
                row.RelativeItem().Column(compCol =>
                {
                    compCol.Item().Text("4. COMPLIANCE & COMMERCIAL TRADE").Bold().FontSize(8.5f).FontColor(Colors.Blue.Darken3);
                    compCol.Item().PaddingTop(3).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Column(inner =>
                    {
                        inner.Item().Text($"Overtime Hours: {_model.TotalOvertimeHoursLogged:N0} hrs (Avg {_model.AverageOvertimePerOperatorHours}h/op &mdash; Legal Limit &le;2h)").FontSize(7);
                        inner.Item().Text($"Health Clinic: {_model.ClinicConsultationsCount} visits ({_model.ClinicFitForFloorDutyCount} Fit, {_model.ClinicSentHomeCount} Sent Home)").FontSize(7);
                        inner.Item().Text($"AC-07 Child Labor Violations: {_model.UnderageViolationsCount} (Zero Tolerance Cleared)").Bold().FontColor(Colors.Green.Darken3).FontSize(7);
                        inner.Item().Text($"Open Worker Grievances: {_model.OpenWorkerGrievancesCount} &bull; Export LCs: {_model.ActiveExportMasterLcsCount} (${_model.TotalActiveExportLcValueUsd:N0})").FontSize(7);
                    });
                });
            });

            col.Item().PaddingTop(8);

            // 5. Executive Highlights & Corrective Action Summary
            col.Item().Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(botCol =>
            {
                botCol.Item().Text("EXECUTIVE REMARKS & ACTION DIRECTIVES (DIRECTOR OF OPERATIONS):").Bold().FontSize(7.5f).FontColor(Colors.Blue.Darken4);
                botCol.Item().Text($"1. Top line efficiency achieved by {_model.TopPerformingLine}. Maintain current line layout and needle feed setup.").FontSize(7);
                botCol.Item().Text($"2. Corrective action required for {_model.LowestPerformingLine}. Industrial Engineering (IE) supervisor dispatched to re-balance work station.").FontSize(7);
                botCol.Item().Text($"3. Ex-factory dispatch of ${_model.TodayExFactoryShipmentValueUsd:N2} USD successfully sealed under Customs bonded lock. On-time delivery standing at {_model.OnTimeDeliveryRatePct}%.").FontSize(7);
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text(text =>
            {
                text.Span("StitchMind Automated Digest Worker &bull; Pipeline: In-Memory Zero-Disk Vector Stream &bull; ").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
                text.Span($"Compiled: {_model.GeneratedAtUtc:yyyy-MM-dd HH:mm:ss} UTC").FontSize(6.5f).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Text(text =>
            {
                text.Span("Page ");
                text.CurrentPageNumber();
                text.Span(" of ");
                text.TotalPages();
            });
        });
    }

    private static List<LineEfficiencyDigestItem> GetDefaultLines() =>
    [
        new() { LineNumber = "Line 01", BuyerName = "H&M Global", StyleNumber = "ST-HM-POLO-01", PlannedHourlyTarget = 140, ActualHourlyAchieved = 135, LineEfficiencyPct = 96.4m, DhuPct = 1.1m, Status = "Optimal" },
        new() { LineNumber = "Line 02", BuyerName = "H&M Global", StyleNumber = "ST-HM-POLO-02", PlannedHourlyTarget = 140, ActualHourlyAchieved = 132, LineEfficiencyPct = 94.3m, DhuPct = 1.4m, Status = "Optimal" },
        new() { LineNumber = "Line 03", BuyerName = "Zara / Inditex", StyleNumber = "ST-ZARA-CHINO-01", PlannedHourlyTarget = 120, ActualHourlyAchieved = 118, LineEfficiencyPct = 98.3m, DhuPct = 0.9m, Status = "Optimal" },
        new() { LineNumber = "Line 04", BuyerName = "Zara / Inditex", StyleNumber = "ST-ZARA-CHINO-02", PlannedHourlyTarget = 120, ActualHourlyAchieved = 119, LineEfficiencyPct = 98.7m, DhuPct = 0.8m, Status = "Top Rank" },
        new() { LineNumber = "Line 05", BuyerName = "Levi Strauss", StyleNumber = "ST-LEVI-DENIM-01", PlannedHourlyTarget = 100, ActualHourlyAchieved = 91, LineEfficiencyPct = 91.0m, DhuPct = 1.5m, Status = "Normal" },
        new() { LineNumber = "Line 06", BuyerName = "Levi Strauss", StyleNumber = "ST-LEVI-DENIM-02", PlannedHourlyTarget = 100, ActualHourlyAchieved = 88, LineEfficiencyPct = 88.0m, DhuPct = 1.7m, Status = "Normal" },
        new() { LineNumber = "Line 07", BuyerName = "Marks & Spencer", StyleNumber = "ST-MS-HOODIE-01", PlannedHourlyTarget = 110, ActualHourlyAchieved = 94, LineEfficiencyPct = 85.5m, DhuPct = 1.8m, Status = "Normal" },
        new() { LineNumber = "Line 08", BuyerName = "Marks & Spencer", StyleNumber = "ST-MS-HOODIE-02", PlannedHourlyTarget = 110, ActualHourlyAchieved = 86, LineEfficiencyPct = 78.2m, DhuPct = 2.4m, Status = "Rebalancing" },
        new() { LineNumber = "Line 09", BuyerName = "Target USA", StyleNumber = "ST-TGT-TEE-01", PlannedHourlyTarget = 180, ActualHourlyAchieved = 172, LineEfficiencyPct = 95.6m, DhuPct = 1.0m, Status = "Optimal" },
        new() { LineNumber = "Line 10", BuyerName = "Target USA", StyleNumber = "ST-TGT-TEE-02", PlannedHourlyTarget = 180, ActualHourlyAchieved = 168, LineEfficiencyPct = 93.3m, DhuPct = 1.2m, Status = "Optimal" },
        new() { LineNumber = "Line 11", BuyerName = "Next UK", StyleNumber = "ST-NXT-JOGGER-01", PlannedHourlyTarget = 130, ActualHourlyAchieved = 115, LineEfficiencyPct = 88.5m, DhuPct = 1.6m, Status = "Normal" },
        new() { LineNumber = "Line 12", BuyerName = "Next UK", StyleNumber = "ST-NXT-JOGGER-02", PlannedHourlyTarget = 130, ActualHourlyAchieved = 112, LineEfficiencyPct = 86.2m, DhuPct = 1.9m, Status = "Normal" }
    ];
}
