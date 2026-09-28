namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class CustomsBondPdfDocument : IDocument
{
    private readonly CustomsBondReportModel _model;

    public CustomsBondPdfDocument(CustomsBondReportModel model)
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
                col.Item().Text("Customs Bonded Warehouse & Compliance Division").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Customs House Commissionerate, Dhaka, Bangladesh").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("CUSTOMS BOND LEDGER").Bold().FontSize(16).FontColor(Colors.Blue.Darken2);
                col.Item().Text($"UP No: {_model.UtilizationPermitNo}").Bold().FontSize(9);
                col.Item().Text($"Bond Reg: {_model.BondRegisterNumber}").FontSize(8);
                col.Item().Text($"Audit Date: {_model.ReportDate:dd-MMM-yyyy}").FontSize(8);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // UP Particulars Box
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("UTILIZATION PERMIT (UP) PARTICULARS").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"UP Number: {_model.UtilizationPermitNo}").Bold().FontSize(10);
                    c.Item().Text($"Buyer: {_model.BuyerName}").FontSize(8);
                    c.Item().Text($"Style Ref: {_model.StyleNumber}").FontSize(8);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("RAW MATERIAL ALLOCATION").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"Item Code: {_model.ItemCode}").Bold().FontSize(10);
                    c.Item().Text($"Description: {_model.ItemDescription}").FontSize(8);
                    c.Item().Text($"Duty Exemption Benefit: ${_model.DutySavedUSD:N2} USD").Bold().FontSize(8).FontColor(Colors.Green.Darken2);
                });
            });

            // Summary Gauge Row
            col.Item().PaddingTop(12).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Blue.Lighten3).Background(Colors.Blue.Lighten5).Padding(8).Column(c =>
                {
                    c.Item().Text("TOTAL CREDITED").Bold().FontSize(8).FontColor(Colors.Blue.Darken3);
                    c.Item().Text($"{_model.TotalCreditedYards:N2} Yds").Bold().FontSize(12).FontColor(Colors.Blue.Darken3);
                });

                row.ConstantItem(10);

                row.RelativeItem().Border(1).BorderColor(Colors.Orange.Lighten3).Background(Colors.Orange.Lighten5).Padding(8).Column(c =>
                {
                    c.Item().Text("TOTAL DEBITED (FLOOR ISSUES)").Bold().FontSize(8).FontColor(Colors.Orange.Darken3);
                    c.Item().Text($"{_model.TotalDebitedYards:N2} Yds").Bold().FontSize(12).FontColor(Colors.Orange.Darken3);
                });

                row.ConstantItem(10);

                row.RelativeItem().Border(1).BorderColor(Colors.Green.Lighten3).Background(Colors.Green.Lighten5).Padding(8).Column(c =>
                {
                    c.Item().Text("REMAINING UP BALANCE").Bold().FontSize(8).FontColor(Colors.Green.Darken3);
                    c.Item().Text($"{_model.CurrentBalanceYards:N2} Yds").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                });
            });

            // Transactions Table
            col.Item().PaddingTop(15).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(65);  // Date
                    columns.ConstantColumn(110); // Trans Type
                    columns.ConstantColumn(80);  // Doc Type
                    columns.ConstantColumn(90);  // Doc Ref
                    columns.ConstantColumn(70);  // Quantity
                    columns.ConstantColumn(75);  // Balance
                    columns.RelativeColumn(3);   // Remarks
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Date").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Transaction").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Doc Type").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Reference #").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Qty (Yds)").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).AlignRight().Text("Balance (Yds)").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken3).Padding(5).Text("Remarks").Bold().FontColor(Colors.White);
                });

                for (var i = 0; i < _model.Transactions.Count; i++)
                {
                    var t = _model.Transactions[i];
                    var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;

                    table.Cell().Background(bg).Padding(5).Text(t.TransactionDate.ToString("dd-MMM-yy"));
                    table.Cell().Background(bg).Padding(5).Text(t.TransactionType).Bold();
                    table.Cell().Background(bg).Padding(5).Text(t.ReferenceDocType);
                    table.Cell().Background(bg).Padding(5).Text(t.ReferenceDocNumber).Bold();
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(t.Quantity.ToString("N1"));
                    table.Cell().Background(bg).Padding(5).AlignRight().Text(t.BalanceAfterTransaction.ToString("N1")).Bold();
                    table.Cell().Background(bg).Padding(5).Text(t.Remarks);
                }
            });

            // Signatures
            col.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                {
                    c.Item().Text("CUSTOMS BOND OFFICER").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(25).Text("_________________________________").FontSize(8);
                    c.Item().Text("Superintendent / Inspector").Bold().FontSize(8);
                    c.Item().Text("National Board of Revenue (NBR)").FontSize(7).FontColor(Colors.Grey.Darken1);
                });

                row.ConstantItem(20);

                row.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
                {
                    c.Item().Text("FACTORY GENERAL MANAGER / COMPLIANCE").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().PaddingTop(25).Text("_________________________________").FontSize(8);
                    c.Item().Text("DGM Supply Chain & Bond Compliance").Bold().FontSize(8);
                    c.Item().Text("StitchMind Apparel Group Ltd.").FontSize(7).FontColor(Colors.Grey.Darken1);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.BorderTop(1).BorderColor(Colors.Grey.Lighten2).PaddingTop(5).Row(row =>
        {
            row.RelativeItem().Text("StitchMind ERP — Statutory Customs Bond Ledger Register").FontSize(7).FontColor(Colors.Grey.Medium);
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
