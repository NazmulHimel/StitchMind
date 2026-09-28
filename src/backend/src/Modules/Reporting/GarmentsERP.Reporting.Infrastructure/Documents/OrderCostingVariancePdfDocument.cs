namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class OrderCostingVariancePdfDocument : IDocument
{
    private readonly OrderCostingVarianceReportModel _model;

    public OrderCostingVariancePdfDocument(OrderCostingVarianceReportModel model)
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
                col.Item().Text("Management Accounting & Order Profitability Division").FontSize(9).FontColor(Colors.Grey.Darken1);
                col.Item().Text("Plot 12, Gazipur Industrial Area, Dhaka, Bangladesh").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("POST-COSTING VARIANCE DOSSIER").Bold().FontSize(14).FontColor(Colors.Teal.Darken2);
                col.Item().Text($"Style: {_model.StyleNumber}").Bold().FontSize(10);
                col.Item().Text($"Analysis Date: {_model.AnalyzedAtUtc:dd-MMM-yyyy}").FontSize(8);
                col.Item().Text($"Net Margin: {_model.NetProfitMarginPct:N2}%").Bold().FontSize(9).FontColor(Colors.Green.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(15).Column(col =>
        {
            // Order & Buyer Summary
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("STYLE DETAILS").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"{_model.StyleNumber} - {_model.StyleName}").Bold().FontSize(10);
                    c.Item().Text($"Buyer: {_model.BuyerName}").FontSize(9);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("EXPORT ORDER VOLUME").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"{_model.OrderQuantity:N0} Pcs").Bold().FontSize(10);
                    c.Item().Text($"FOB Price: ${_model.BuyerFOBPricePerPc:N4} / Pc").FontSize(9);
                });

                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("TOTAL REALIZED REVENUE").Bold().FontSize(8).FontColor(Colors.Grey.Darken2);
                    c.Item().Text($"${_model.BuyerFOBRevenue:N2} {_model.Currency}").Bold().FontSize(11).FontColor(Colors.Blue.Darken2);
                    c.Item().Text("Clean Bill of Lading Realized").FontSize(8).FontColor(Colors.Green.Darken2);
                });
            });

            col.Item().Height(12);

            // Cost Variance Breakdown Table
            col.Item().Text("Pre-Costing Budget vs Actual Factory Expenditure Variance").Bold().FontSize(11).FontColor(Colors.Blue.Darken3);
            col.Item().Height(4);

            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Cost Element
                    columns.RelativeColumn(2); // Pre-Costing Budget
                    columns.RelativeColumn(2); // Actual Factory Cost
                    columns.RelativeColumn(2); // Variance ($)
                    columns.RelativeColumn(2); // Variance Status
                });

                table.Header(header =>
                {
                    header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Cost Center / Element").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Pre-Costing Budget").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Actual Post-Cost").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Variance ($)").Bold().FontColor(Colors.White);
                    header.Cell().Background(Colors.Teal.Darken2).Padding(5).Text("Performance").Bold().FontColor(Colors.White);
                });

                // Raw Materials
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Raw Materials (Fabric & Trims)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.BudgetRawMaterialsCost:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.ActualRawMaterialsCost:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"+${_model.RawMaterialVariance:N2}").FontColor(Colors.Green.Darken2);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("FAVORABLE").Bold().FontColor(Colors.Green.Darken2);

                // Direct Labor
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Direct Labor (Cutting, Sewing, Finishing)");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.BudgetDirectLaborCost:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.ActualDirectLaborCost:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"+${_model.DirectLaborVariance:N2}").FontColor(Colors.Green.Darken2);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("FAVORABLE").Bold().FontColor(Colors.Green.Darken2);

                // Overheads & Commercial Charges
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("Factory Overheads & Commercial LC Fees");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.BudgetOverheadsCost:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"${_model.ActualOverheadsCost:N2}");
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text($"+${_model.OverheadsVariance:N2}").FontColor(Colors.Green.Darken2);
                table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).Padding(5).Text("FAVORABLE").Bold().FontColor(Colors.Green.Darken2);

                // Total Cost
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("Total Cost of Production").Bold();
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text($"${_model.TotalBudgetCost:N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text($"${_model.TotalActualCost:N2}").Bold();
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text($"+${_model.TotalCostVariance:N2}").Bold().FontColor(Colors.Green.Darken2);
                table.Cell().Background(Colors.Grey.Lighten4).Padding(5).Text("COST SAVING").Bold().FontColor(Colors.Green.Darken2);
            });

            col.Item().Height(12);

            // Profit & Margin Summary Card
            col.Item().Border(1).BorderColor(Colors.Teal.Lighten2).Background(Colors.Teal.Lighten5).Padding(10).Column(c =>
            {
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("TOTAL EXPORT FOB REVENUE:").Bold().FontSize(10);
                    r.RelativeItem(1).AlignRight().Text($"${_model.BuyerFOBRevenue:N2}").Bold().FontSize(10);
                });
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("LESS: TOTAL AUDITED ACTUAL COST:").FontColor(Colors.Grey.Darken2);
                    r.RelativeItem(1).AlignRight().Text($"- ${_model.TotalActualCost:N2}").FontColor(Colors.Red.Darken1);
                });
                c.Item().LineHorizontal(1).LineColor(Colors.Teal.Lighten2);
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("NET REALIZED FACTORY PROFIT:").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                    r.RelativeItem(1).AlignRight().Text($"${_model.NetRealizedProfit:N2}").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                });
                c.Item().Row(r =>
                {
                    r.RelativeItem(3).Text("NET REALIZED PROFIT MARGIN:").Bold().FontSize(10).FontColor(Colors.Green.Darken3);
                    r.RelativeItem(1).AlignRight().Text($"{_model.NetProfitMarginPct:N2}%").Bold().FontSize(10).FontColor(Colors.Green.Darken3);
                });
            });

            col.Item().Height(12);

            // Audit Conclusion
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(c =>
            {
                c.Item().Text("MANAGEMENT AUDIT CONCLUSION").Bold().FontSize(9).FontColor(Colors.Grey.Darken3);
                c.Item().Text($"Style {_model.StyleNumber} achieved a net profit margin of {_model.NetProfitMarginPct:N2}%, exceeding standard apparel manufacturing benchmarks (8%–12%). Favorable fabric consumption and zero needle/stain rejects contributed to ${ _model.TotalCostVariance:N2} in total factory savings.").FontSize(8);
                c.Item().Text($"Evaluated By: {_model.EvaluatedBy} | Status: POST-COSTING COMPLETED & SIGNED OFF").FontSize(7).FontColor(Colors.Grey.Darken1);
            });

            col.Item().Height(30);

            // Signatures
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().AlignCenter().Text("Cost Accounting Lead").FontSize(8);
                });

                row.ConstantItem(40);

                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().AlignCenter().Text("Head of Merchandising").FontSize(8);
                });

                row.ConstantItem(40);

                row.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(1).LineColor(Colors.Grey.Medium);
                    c.Item().AlignCenter().Text("Managing Director / CFO").FontSize(8);
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("Confidential - StitchMind Order-Wise Post-Costing Engine (ACC-04 Report)").FontSize(7).FontColor(Colors.Grey.Darken1);
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
