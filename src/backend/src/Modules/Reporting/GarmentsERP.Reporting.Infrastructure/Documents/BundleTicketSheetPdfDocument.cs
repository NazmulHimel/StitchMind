namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class BundleTicketSheetPdfDocument : IDocument
{
    private readonly BundleTicketReportModel _model;

    public BundleTicketSheetPdfDocument(BundleTicketReportModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(20);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(8).FontFamily("Lato"));

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
                col.Item().Text("STITCHMIND INDUSTRIAL EXECUTION — THERMAL BUNDLE TICKETS").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                col.Item().Text($"Cut Order: {_model.CuttingOrderNumber} | Style: {_model.StyleNumber} | Buyer: {_model.BuyerName}").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text($"Color: {_model.ColorName} | Shade: {_model.ShadeGroup}").Bold().FontSize(9);
                col.Item().Text($"Total Tickets: {_model.Tickets.Count} Bundles").FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Table(table =>
        {
            table.ColumnsDefinition(cols =>
            {
                cols.RelativeColumn();
                cols.RelativeColumn();
            });

            foreach (var ticket in _model.Tickets)
            {
                table.Cell().Padding(5).Border(1).BorderColor(Colors.Black).Background(Colors.Grey.Lighten5).Padding(8).Column(ticketCol =>
                {
                    ticketCol.Item().Row(r =>
                    {
                        r.RelativeItem().Text("STITCHMIND FACTORY").Bold().FontSize(9).FontColor(Colors.Blue.Darken3);
                        r.RelativeItem().AlignRight().Text($"CUT: {_model.CuttingOrderNumber}").Bold().FontSize(8);
                    });

                    ticketCol.Item().PaddingTop(2).Row(r =>
                    {
                        r.RelativeItem().Text($"Style: {_model.StyleNumber}").FontSize(8);
                        r.RelativeItem().AlignRight().Text($"Buyer: {_model.BuyerName}").FontSize(8);
                    });

                    ticketCol.Item().Row(r =>
                    {
                        r.RelativeItem().Text($"Color: {_model.ColorName}").FontSize(8);
                        r.RelativeItem().AlignRight().Text($"Shade: {_model.ShadeGroup}").Bold().FontSize(8);
                    });

                    ticketCol.Item().BorderTop(0.5f).BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten1).PaddingVertical(3).Row(r =>
                    {
                        r.RelativeItem().Text(ticket.ComponentName).Bold().FontSize(10).FontColor(Colors.Teal.Darken3);
                        r.RelativeItem().AlignRight().Text($"SIZE: {ticket.Size}").Bold().FontSize(10);
                    });

                    ticketCol.Item().PaddingTop(2).Row(r =>
                    {
                        r.RelativeItem().Text($"Bundle: {ticket.BundleSequence:D2}/{ticket.TotalBundles:D2}").Bold().FontSize(8);
                        r.RelativeItem().AlignRight().Text($"Qty: {ticket.Quantity} Pcs ({ticket.SerialStart:D3}-{ticket.SerialEnd:D3})").Bold().FontSize(8);
                    });

                    ticketCol.Item().PaddingTop(4).Column(barcodeCol =>
                    {
                        barcodeCol.Item().Background(Colors.Grey.Lighten3).Padding(4).AlignCenter().Text($"||| {ticket.BundleBarcode} |||").Bold().FontSize(10).FontFamily("Consolas");
                        barcodeCol.Item().AlignCenter().Text(ticket.BundleBarcode).FontSize(7).FontFamily("Consolas");
                    });
                });
            }
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(4).Row(row =>
        {
            row.RelativeItem().Text("Attach ticket to cut component stack prior to sewing line input").FontSize(7).FontColor(Colors.Grey.Darken2);
            row.RelativeItem().AlignRight().Text(x =>
            {
                x.Span("Sheet ");
                x.CurrentPageNumber();
                x.Span(" of ");
                x.TotalPages();
            });
        });
    }
}
