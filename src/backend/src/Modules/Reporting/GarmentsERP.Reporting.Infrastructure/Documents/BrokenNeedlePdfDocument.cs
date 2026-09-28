namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class BrokenNeedlePdfDocument : IDocument
{
    private readonly BrokenNeedleReportModel _model;

    public BrokenNeedlePdfDocument(BrokenNeedleReportModel model)
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
            page.DefaultTextStyle(x => x.FontSize(9).FontFamily("Lato"));

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
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("STITCHMIND GARMENTS MANUFACTURING LTD.").Bold().FontSize(13).FontColor(Colors.Red.Darken3);
                    c.Item().Text("BROKEN NEEDLE REPLACEMENT & FRAGMENT VERIFICATION CERTIFICATE").Bold().FontSize(10).FontColor(Colors.Grey.Darken3);
                    c.Item().Text("Zero Metal Contamination Social Compliance Standard (Buyer Gatekeeper)").FontSize(8).FontColor(Colors.Grey.Darken1);
                });

                row.RelativeItem().AlignRight().Column(c =>
                {
                    c.Item().Text($"Log No: {_model.CertificateNumber}").Bold().FontSize(11).FontColor(Colors.Red.Darken2);
                    c.Item().Text($"Date: {_model.IncidentTimeUtc:dd-MMM-yyyy HH:mm} UTC").FontSize(8);
                    c.Item().Text($"Status: {_model.ComplianceStatus}").Bold().FontSize(8).FontColor(_model.AllFragmentsRecovered ? Colors.Green.Darken2 : Colors.Red.Darken2);
                });
            });

            col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(Colors.Red.Darken2);
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Section 1: Machine & Operator Identity
            col.Item().Text("1. INCIDENT & MACHINE IDENTIFICATION").Bold().FontSize(10).FontColor(Colors.Grey.Darken3);
            col.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten5).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Sewing Line: {_model.LineNumber}").Bold();
                    c.Item().Text($"Machine Barcode: {_model.MachineBarcode}").Bold();
                    c.Item().Text($"Model: {_model.MachineModel}");
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Operator Badge: {_model.OperatorBadgeId}").Bold();
                    c.Item().Text($"Operator Name: {_model.OperatorName}");
                    c.Item().Text($"Downtime: {_model.DowntimeMinutes} Minutes");
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Breakage Cause:").Bold();
                    c.Item().Text(_model.BreakageReason).FontSize(8).FontColor(Colors.Grey.Darken2);
                });
            });

            // Section 2: 3-Point Fragment Recovery Gate
            col.Item().PaddingTop(15).Text("2. MANDATORY 3-POINT FRAGMENT VERIFICATION GATE (100% RECOVERY REQUIREMENT)").Bold().FontSize(10).FontColor(Colors.Grey.Darken3);
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(cols =>
                {
                    cols.RelativeColumn(2);
                    cols.RelativeColumn(1.5f);
                    cols.RelativeColumn(3);
                    cols.RelativeColumn(3.5f);
                });

                table.Header(h =>
                {
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Fragment Part").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Found Status").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Fragment Dimension").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(5).Text("Verification Tape Mount").Bold();
                });

                // Tip
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Needle Tip (Point)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(_model.TipFound ? "RECOVERED [✓]" : "MISSING [✗]").Bold().FontColor(_model.TipFound ? Colors.Green.Darken2 : Colors.Red.Darken2);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("2.5 mm point section");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(_model.TipFound ? "Mounted on cell #1" : "QUARANTINE ENFORCED");

                // Shank
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Needle Shank (Top)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(_model.ShankFound ? "RECOVERED [✓]" : "MISSING [✗]").Bold().FontColor(_model.ShankFound ? Colors.Green.Darken2 : Colors.Red.Darken2);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("18.0 mm shank bar");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(_model.ShankFound ? "Mounted on cell #2" : "QUARANTINE ENFORCED");

                // Eye
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("Needle Eye & Blade");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(_model.EyeFound ? "RECOVERED [✓]" : "MISSING [✗]").Bold().FontColor(_model.EyeFound ? Colors.Green.Darken2 : Colors.Red.Darken2);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text("12.5 mm blade segment");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten3).Padding(5).Text(_model.EyeFound ? "Mounted on cell #3" : "QUARANTINE ENFORCED");
            });

            // Section 3: Compliance Decision Card
            col.Item().PaddingTop(15).Row(row =>
            {
                var bg = _model.AllFragmentsRecovered ? Colors.Green.Lighten5 : Colors.Red.Lighten5;
                var border = _model.AllFragmentsRecovered ? Colors.Green.Darken1 : Colors.Red.Darken1;

                row.RelativeItem().Border(1.5f).BorderColor(border).Background(bg).Padding(10).Column(c =>
                {
                    c.Item().Text(_model.AllFragmentsRecovered
                        ? "100% FRAGMENT VERIFICATION PASSED — REPLACEMENT AUTHORIZED"
                        : "COMPLIANCE VIOLATION — REPLACEMENT BLOCKED & BUNDLE QUARANTINED")
                        .Bold().FontSize(11).FontColor(_model.AllFragmentsRecovered ? Colors.Green.Darken3 : Colors.Red.Darken3);

                    c.Item().PaddingTop(4).Text(_model.Remarks).FontSize(8.5f).FontColor(Colors.Grey.Darken3);
                    c.Item().PaddingTop(4).Text($"Metal Detector Audit: {_model.MetalDetectorCheckResult}").FontSize(8.5f).Bold();
                    c.Item().Text($"Replacement Needle Issued: {_model.ReplacementNeedleType} (Qty: 1 pc)").FontSize(8.5f);
                });
            });

            // Section 4: Physical Fragment Adhesive Card Placeholder
            col.Item().PaddingTop(15).Text("3. PHYSICAL FRAGMENT RETENTION STICKER CARD (TAPE AREA)").Bold().FontSize(9).FontColor(Colors.Grey.Darken3);
            col.Item().PaddingTop(4).Border(1).BorderColor(Colors.Grey.Lighten1).Background(Colors.Grey.Lighten5).Height(80).AlignCenter().AlignMiddle().Text("AFFIX TRANSPARENT ADHESIVE TAPE WITH RECOVERED NEEDLE FRAGMENTS HERE").FontSize(9).FontColor(Colors.Grey.Lighten1);

            // Signatures
            col.Item().PaddingTop(25).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                    c.Item().PaddingTop(3).Text($"Operator: {_model.OperatorName} ({_model.OperatorBadgeId})").FontSize(8).AlignCenter();
                });

                row.Spacing(20);

                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                    c.Item().PaddingTop(3).Text($"Verified by Supervisor: {_model.SupervisorBadge}").FontSize(8).AlignCenter();
                });

                row.Spacing(20);

                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Darken1);
                    c.Item().PaddingTop(3).Text("Compliance QA Gatekeeper Sign").FontSize(8).AlignCenter();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("StitchMind Social Compliance — Form BNL-SOP-084-REV3 | Certified Archival Copy").FontSize(7).FontColor(Colors.Grey.Darken1);
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
