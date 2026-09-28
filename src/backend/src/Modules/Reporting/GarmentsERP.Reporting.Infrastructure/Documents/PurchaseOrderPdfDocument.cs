namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class PurchaseOrderPdfDocument : IDocument
{
    private readonly PurchaseOrderReportModel _model;

    public PurchaseOrderPdfDocument(PurchaseOrderReportModel model)
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
                col.Item().Text("Export Garments Manufacturing Division").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Plot 12, Gazipur Industrial Area, Dhaka, Bangladesh").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("PURCHASE ORDER").Bold().FontSize(18).FontColor(Colors.Blue.Darken2);
                col.Item().Text($"PO Number: {_model.PONumber}").Bold().FontSize(10);
                col.Item().Text($"Date: {_model.OrderDate:dd-MMM-yyyy}").FontSize(8);
                col.Item().Text($"Delivery: {_model.ExpectedDeliveryDate:dd-MMM-yyyy}").FontSize(8).FontColor(Colors.Red.Medium);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Supplier & Style Info Box
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("SUPPLIER DETAILS").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text(_model.SupplierName).Bold().FontSize(10);
                    c.Item().Text(_model.SupplierAddress).FontSize(8);
                    c.Item().Text($"Payment Terms: {_model.PaymentTerms}").FontSize(8);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("ORDER REFERENCE").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"Style Ref: {_model.StyleNumber}").Bold().FontSize(10);
                    c.Item().Text($"Buyer: {_model.BuyerName}").FontSize(8);
                    c.Item().Text($"Currency: {_model.Currency}").FontSize(8);
                });
            });

            col.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);  // Sl
                    columns.ConstantColumn(80);  // Item Code
                    columns.RelativeColumn(3);  // Description
                    columns.ConstantColumn(40);  // UOM
                    columns.ConstantColumn(60);  // Qty
                    columns.ConstantColumn(70);  // Unit Price
                    columns.ConstantColumn(80);  // Total Price
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("#").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Item Code").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Description").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("UOM").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Quantity").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Rate ($)").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Total ($)").Bold().FontColor(Colors.White);
                });

                for (var i = 0; i < _model.Items.Count; i++)
                {
                    var item = _model.Items[i];
                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Background(bg).Padding(5).Text((i + 1).ToString());
                    table.Cell().Background(bg).Padding(5).Text(item.ItemCode).Bold();
                    table.Cell().Background(bg).Padding(5).Text(item.Description);
                    table.Cell().Background(bg).Padding(5).Text(item.UnitOfMeasure);
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(item.Quantity.ToString("N0"));
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(item.UnitPrice.ToString("N4"));
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(item.TotalPrice.ToString("N2")).Bold();
                }

                // Summary Row
                table.Cell().ColumnSpan(5).BorderTop(1).Padding(5).AlignRight().Text("TOTAL ORDER VALUE:").Bold().FontSize(10);
                table.Cell().ColumnSpan(2).BorderTop(1).Padding(5).AlignRight().Text($"{_model.Currency} {_model.TotalAmount:N2}").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            });

            // Budget Lock Clearance Stamp
            col.Item().PaddingTop(20).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Green.Darken1).Background(Colors.Green.Lighten5).Padding(8).Column(c =>
                {
                    c.Item().Text("PRE-BUDGET VALIDATION VERIFIED").Bold().FontSize(8).FontColor(Colors.Green.Darken3);
                    c.Item().Text("All item rates comply with Merchandising Pre-Costing budget thresholds.").FontSize(8).FontColor(Colors.Green.Darken2);
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                {
                    c.Item().Text("AUTHORIZED COMMERCIAL SIGNATURE").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(20).Text("_________________________________").FontSize(8);
                    c.Item().Text("Chief Financial Officer / Managing Director").FontSize(8).FontColor(Colors.Grey.Darken1);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("Generated by StitchMind ERP — Confidential").FontSize(7).FontColor(Colors.Grey.Medium);
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
