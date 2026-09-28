namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class CuttingLaySheetPdfDocument : IDocument
{
    private readonly CuttingLaySheetReportModel _model;

    public CuttingLaySheetPdfDocument(CuttingLaySheetReportModel model)
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
                col.Item().Text("STITCHMIND APPAREL GROUP").Bold().FontSize(16).FontColor(Colors.Teal.Darken3);
                col.Item().Text("Cutting Department & CAD Automation Division").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Factory Unit 1 — Lay Planning & Spreading Floor").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("CUTTING LAY SHEET").Bold().FontSize(16).FontColor(Colors.Teal.Darken2);
                col.Item().Text($"Cut Order: {_model.CuttingOrderNumber}").Bold().FontSize(9);
                col.Item().Text($"Table: {_model.TableNumber} | Shade: {_model.ShadeGroup}").FontSize(8);
                col.Item().Text($"Cut Date: {_model.CuttingDate:dd-MMM-yyyy}").FontSize(8);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(12).Column(col =>
        {
            // Order & CAD Particulars Box
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"Style No: {_model.StyleNumber}").Bold();
                    c.Item().Text($"Buyer: {_model.BuyerName}");
                    c.Item().Text($"Buyer Order: {_model.OrderNumber}");
                    c.Item().Text($"Fabric Type: {_model.FabricType}");
                });
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"CAD Marker: {_model.MarkerName}").Bold();
                    c.Item().Text($"Marker Efficiency: {_model.MarkerEfficiencyPct:F2}% (Target >= 85.0%)");
                    c.Item().Text($"Marker Length: {_model.MarkerLengthYards:F2} Yds ({_model.GarmentsPerMarker} Pcs/Marker)");
                    c.Item().Text($"Total Plies: {_model.TotalPlies} Plies | Planned: {_model.TotalGarmentsPlanned} Garments").Bold();
                });
            });

            col.Item().PaddingTop(10);

            // Fabric Consumption & Wastage KPI Box
            col.Item().Border(1).BorderColor(Colors.Teal.Lighten3).Background(Colors.Teal.Lighten5).Padding(8).Column(kpiCol =>
            {
                kpiCol.Item().Text("FABRIC RECONCILIATION & WASTAGE VARIANCE").Bold().FontColor(Colors.Teal.Darken3);
                kpiCol.Item().PaddingTop(4).Row(r =>
                {
                    r.RelativeItem().Text($"Planned Total: {_model.PlannedFabricTotalYds:F2} Yds");
                    r.RelativeItem().Text($"Actual Used: {_model.ActualFabricConsumedYds:F2} Yds").Bold();
                    r.RelativeItem().Text($"End-loss Remnant: {_model.EndLossRemnantYds:F2} Yds");
                    r.RelativeItem().Text($"Wastage Variance: {_model.CuttingWastageVariancePct:F2}%").Bold().FontColor(_model.CuttingWastageVariancePct <= 2.0m ? Colors.Green.Darken2 : Colors.Red.Darken2);
                });
            });

            col.Item().PaddingTop(10);

            // Allocated Rolls & Relaxation Status
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Column(rollCol =>
            {
                rollCol.Item().Text("ALLOCATED FABRIC ROLLS (MANDATORY 24-HOUR RELAXATION VERIFIED)").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                rollCol.Item().Text(_model.AllocatedRollBarcodes).FontSize(8).FontFamily("Consolas");
            });

            col.Item().PaddingTop(12);

            // Bundles Component Table
            col.Item().Text("COMPONENT BUNDLE BREAKDOWN & SERIAL ALLOCATION").Bold().FontSize(10).FontColor(Colors.Teal.Darken3);
            col.Item().PaddingTop(4);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(110);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(70);
                    columns.ConstantColumn(60);
                    columns.RelativeColumn(2);
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Teal.Darken2).Padding(4).Text("Bundle Barcode").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(4).Text("Component").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(4).Text("Size").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(4).Text("Qty (Pcs)").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(4).Text("Serial Range").Bold().FontColor(Colors.White);
                });

                foreach (var b in _model.Bundles)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(b.BundleBarcode).FontFamily("Consolas");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(b.ComponentName).Bold();
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(b.Size);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(b.Quantity.ToString()).Bold();
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(b.SerialRange);
                }
            });

            col.Item().PaddingTop(25);

            // Signatures
            col.Item().Row(r =>
            {
                r.RelativeItem().Column(c =>
                {
                    c.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(20).Text("");
                    c.Item().Text("Prepared By: Cutting Master").FontSize(8).Bold();
                });
                r.ConstantItem(20);
                r.RelativeItem().Column(c =>
                {
                    c.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(20).Text("");
                    c.Item().Text("Verified By: QC Floor Auditor").FontSize(8).Bold();
                });
                r.ConstantItem(20);
                r.RelativeItem().Column(c =>
                {
                    c.Item().BorderBottom(1).BorderColor(Colors.Grey.Medium).PaddingBottom(20).Text("");
                    c.Item().Text($"Approved By: {_model.ApprovedBy}").FontSize(8).Bold();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("StitchMind ERP — Cutting Execution & Spreading Lay Engine").FontSize(8).FontColor(Colors.Grey.Darken1);
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
