namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class ThreeWayMatchPdfDocument : IDocument
{
    private readonly ThreeWayMatchReportModel _model;

    public ThreeWayMatchPdfDocument(ThreeWayMatchReportModel model)
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
                col.Item().Text("Commercial Finance & Trade Accounts Payable Division").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Plot 12, Gazipur Industrial Area, Dhaka, Bangladesh").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("THREE-WAY MATCH AUDIT").Bold().FontSize(16).FontColor(Colors.Indigo.Darken2);
                col.Item().Text($"Bill No: {_model.BillNumber}").Bold().FontSize(10);
                col.Item().Text($"Audit Date: {_model.EvaluatedAtUtc:dd-MMM-yyyy HH:mm}").FontSize(8);
                col.Item().Text($"Compliance: AC-06 Approved").FontSize(8).FontColor(Colors.Green.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Supplier & Reference Details
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("SUPPLIER (ACCOUNTS PAYABLE)").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text(_model.SupplierName).Bold().FontSize(11);
                    c.Item().Text($"Currency: {_model.Currency}").FontSize(9);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("PURCHASE ORDER").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text(_model.PONumber).Bold().FontSize(10);
                    c.Item().Text($"Approved PO Rate: ${_model.POUnitPrice:N4} / Yd").FontSize(9);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("QC GOODS RECEIPT (GRN)").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text(_model.GRNNumber).Bold().FontSize(10);
                    c.Item().Text($"QC Cleared Qty: {_model.GRNPassedQuantity:N0} Yds").FontSize(9);
                });
            });

            col.Item().Height(12);

            // Three-Way Match Verification Table
            col.Item().Text("Three-Way Matching Verification Ledger (AC-06)").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().Height(4);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Parameter
                    columns.RelativeColumn(2); // PO / GRN Benchmark
                    columns.RelativeColumn(2); // Vendor Invoice
                    columns.RelativeColumn(2); // Variance
                    columns.RelativeColumn(2); // Audit Result
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Verification Dimension").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Approved Base").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Billed Invoice").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Variance").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Match Result").Bold().FontColor(Colors.White);
                });

                // Row 1: Unit Price
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Unit Price Reconciliation (PO vs Bill)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.POUnitPrice:N4} / Yd");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.BilledUnitPrice:N4} / Yd");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_model.PriceVariancePct:+0.00;-0.00;0.00}%");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(_model.IsPriceMatched ? "PASSED" : "FAILED (LOCKED)").Bold().FontColor(_model.IsPriceMatched ? Colors.Green.Darken2 : Colors.Red.Darken2);

                // Row 2: Quantity
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Quantity Reconciliation (GRN vs Bill)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_model.GRNPassedQuantity:N0} Yds");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_model.BilledQuantity:N0} Yds");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"{_model.QuantityVariance:+0;-0;0} Yds");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text(_model.IsQuantityMatched ? "PASSED" : "FAILED (LOCKED)").Bold().FontColor(_model.IsQuantityMatched ? Colors.Green.Darken2 : Colors.Red.Darken2);

                // Row 3: Total Gross Amount
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Gross Invoice Value");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${(_model.POUnitPrice * _model.GRNPassedQuantity):N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.BilledAmount:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("$0.00");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("VERIFIED").Bold().FontColor(Colors.Green.Darken2);
            });

            col.Item().Height(12);

            // Statutory Withholding Deductions (ACC-06)
            col.Item().Text("Statutory Tax Withholding & Net Disbursement Schedule").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().Height(4);

            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("Gross Invoice Amount:").Bold();
                    r.RelativeItem(1).AlignRight().Text($"${_model.BilledAmount:N2}").Bold();
                });
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("Less: Tax Deducted at Source (TDS 3.0%):").FontColor(Colors.Grey.Darken2);
                    r.RelativeItem(1).AlignRight().Text($"- ${_model.TaxDeductedAtSourceTDS:N2}").FontColor(Colors.Red.Darken1);
                });
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("Less: VAT Deducted at Source (VDS 7.5%):").FontColor(Colors.Grey.Darken2);
                    r.RelativeItem(1).AlignRight().Text($"- ${_model.VatDeductedAtSourceVDS:N2}").FontColor(Colors.Red.Darken1);
                });
                c.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("NET PAYABLE DISBURSEMENT AUTHORIZED:").Bold().FontSize(10).FontColor(Colors.Green.Darken3);
                    r.RelativeItem(1).AlignRight().Text($"${_model.NetPayableAmount:N2}").Bold().FontSize(10).FontColor(Colors.Green.Darken3);
                });
            });

            col.Item().Height(12);

            // Match Result & Notes
            col.Item().Border(1).BorderColor(_model.OverallMatchResult == "PASSED" ? Colors.Green.Medium : Colors.Red.Medium)
                .Background(_model.OverallMatchResult == "PASSED" ? Colors.Green.Lighten5 : Colors.Red.Lighten5)
                .Padding(8).Column(c =>
                {
                    c.Item().Text($"RECONCILIATION RESULT: {_model.OverallMatchResult}").Bold().FontSize(10)
                        .FontColor(_model.OverallMatchResult == "PASSED" ? Colors.Green.Darken3 : Colors.Red.Darken3);
                    c.Item().Text(_model.DiscrepancyNotes ?? "No discrepancies detected. Authorized for payment.").FontSize(9);
                    c.Item().Text($"Audited By: {_model.EvaluatedBy} | Status: APPROVED FOR TREASURY DISBURSEMENT").FontSize(8).FontColor(Colors.Grey.Darken2);
                });

            col.Item().Height(30);

            // Signatures
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().AlignCenter().Text("Accounts Payable Officer").FontSize(8);
                });

                row.ConstantItem(40);

                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().AlignCenter().Text("Internal Audit Lead").FontSize(8);
                });

                row.ConstantItem(40);

                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().AlignCenter().Text("Chief Financial Officer (CFO)").FontSize(8);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("Confidential - StitchMind Commercial Finance System (AC-06 Verification Certificate)").FontSize(7).FontColor(Colors.Grey.Darken1);
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
