namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class FabricInspectionPdfDocument : IDocument
{
    private readonly FabricInspectionReportModel _model;

    public FabricInspectionPdfDocument(FabricInspectionReportModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(30);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

            page.Header().Element(ComposeHeader);
            page.Content().Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    private void ComposeHeader(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text("STITCHMIND APPAREL GROUP").Bold().FontSize(16).FontColor(Colors.Blue.Darken3);
                col.Item().Text("Quality Assurance & Fabric Inspection Laboratory").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Central Raw Material Testing Facility, Gazipur, Bangladesh").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("INSPECTION CERTIFICATE").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                col.Item().Text($"Standard: ASTM D5430-07 (4-Point)").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().Text($"Cert No: {_model.InspectionNumber}").Bold().FontSize(9);
                col.Item().Text($"Date: {_model.InspectionDate:dd-MMM-yyyy}").FontSize(8);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Material & Batch Information Box
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("MATERIAL & STOCK LOT IDENTIFICATION").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"Lot/Batch: {_model.LotBatchNumber}").Bold().FontSize(10);
                    c.Item().Text($"Item Code: {_model.ItemCode}").FontSize(8);
                    c.Item().Text($"Description: {_model.ItemDescription}").FontSize(8);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("INSPECTION AUDIT PARTICULARS").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"Inspector: {_model.InspectorName}").Bold().FontSize(9);
                    c.Item().Text($"Total Rolls in Lot: {_model.TotalRollsInLot} Rolls").FontSize(8);
                    c.Item().Text($"Rolls Inspected: {_model.RollsInspected} Rolls (10% Sampling)").FontSize(8);
                });
            });

            // Mathematical 4-Point Metrics Table
            col.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3);  // Metric Parameter
                    columns.ConstantColumn(120); // Value
                    columns.RelativeColumn(3);  // Benchmark / Norm
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Test Parameter").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Observed Result").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("ASTM Specification Threshold").Bold().FontColor(Colors.White);
                });

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Total Yards Inspected");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{_model.TotalYardsInspected:N0} Yds").Bold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Standard 10% Representative Sampling");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Cuttable Fabric Width");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{_model.CuttableWidthInches:N1} Inches").Bold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Nominal +/- 0.5 Inch Tolerance");

                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Total Penalty Points Recorded");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text($"{_model.TotalPenaltyPoints:N0} Points").Bold();
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Sum of 1, 2, 3, 4 Point Defects");

                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Calculated Points / 100 Sq. Yds").Bold();
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text($"{_model.PointsPer100SqYards:N2} pts").Bold().FontSize(10).FontColor(_model.OverallResult == "PASSED" ? Colors.Green.Darken2 : Colors.Red.Darken2);
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text($"Acceptance Limit: <= {_model.AcceptanceThreshold:N1} pts/100 sq.yd").Bold();
            });

            // Mathematical Formula Callout
            col.Item().PaddingTop(10).Border(1).BorderColor(Colors.Blue.Lighten3).Background(Colors.Blue.Lighten5).Padding(6).Column(c =>
            {
                c.Item().Text("ASTM D5430-07 Formula Reference:").Bold().FontSize(8).FontColor(Colors.Blue.Darken3);
                c.Item().Text("Points per 100 sq. yd = (Total Penalty Points x 3600) / (Inspected Yards x Cuttable Width Inches)").FontSize(8).FontColor(Colors.Blue.Darken2);
            });

            // Decision Box
            var isPassed = _model.OverallResult == "PASSED";
            var resultColor = isPassed ? Colors.Green.Darken2 : Colors.Red.Darken2;
            var resultBg = isPassed ? Colors.Green.Lighten5 : Colors.Red.Lighten5;

            col.Item().PaddingTop(15).Border(1.5f).BorderColor(resultColor).Background(resultBg).Padding(10).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"QUALITY INSPECTION VERDICT: {_model.OverallResult}").Bold().FontSize(12).FontColor(resultColor);
                    c.Item().Text(_model.Remarks).FontSize(8).FontColor(Colors.Grey.Darken3);
                });
            });

            // Signatures & Warehouse Release Clearance
            col.Item().PaddingTop(25).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                {
                    c.Item().Text("INSPECTED & AUDITED BY").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(25).Text("_________________________________").FontSize(8);
                    c.Item().Text(_model.InspectorName).Bold().FontSize(8);
                    c.Item().Text("Certified Textile QC Inspector").FontSize(7).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                {
                    c.Item().Text("WAREHOUSE QUARANTINE RELEASE AUTHORIZATION (AC-04)").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(25).Text("_________________________________").FontSize(8);
                    c.Item().Text("Quality Assurance Manager / DGM").Bold().FontSize(8);
                    c.Item().Text(isPassed ? "Status: RELEASED FOR CUTTING ALLOCATION" : "Status: QUARANTINED / RETURN TO SUPPLIER").FontSize(7).FontColor(resultColor);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("StitchMind QA Inspection Division — ASTM D5430-07 Digital Certification").FontSize(7).FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }
}
