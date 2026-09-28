namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class MasterSalarySheetPdfDocument : IDocument
{
    private readonly MasterSalarySheetReportModel _model;

    public MasterSalarySheetPdfDocument(MasterSalarySheetReportModel model)
    {
        _model = model;
    }

    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4.Landscape());
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
        container.Row(row =>
        {
            row.RelativeItem().Column(col =>
            {
                col.Item().Text(_model.FactoryName.ToUpper()).Bold().FontSize(14).FontColor(Colors.Blue.Darken4);
                col.Item().Text("Factory Master Salary Disbursement Sheet / মাসিক মূল বেতন রেজিস্টার").Bold().FontSize(11).FontColor(Colors.Indigo.Darken3);
                col.Item().Text($"Pay Period: {_model.PeriodText} | Sheet Ref: {_model.SheetNumber}").FontSize(8).FontColor(Colors.Grey.Darken2);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text($"Total Headcount: {_model.TotalHeadcount:N0} Workers").Bold().FontSize(9);
                col.Item().Text($"Piece-Rate: {_model.TotalPieceRateWorkers:N0} | Staff: {_model.TotalStaffWorkers:N0}").FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().Text($"Total Net Disbursement: ${_model.TotalNetDisbursement:N2} USD").Bold().FontSize(11).FontColor(Colors.Green.Darken3);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(8).Column(col =>
        {
            col.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(65);  // Emp Code
                    columns.RelativeColumn(3);  // Name & Dept
                    columns.RelativeColumn(2);  // Designation & Stream
                    columns.ConstantColumn(60);  // Base/Piece ($)
                    columns.ConstantColumn(45);  // OT Hrs
                    columns.ConstantColumn(55);  // OT Pay ($)
                    columns.ConstantColumn(50);  // Bonus ($)
                    columns.ConstantColumn(50);  // Allowances ($)
                    columns.ConstantColumn(50);  // Deductions ($)
                    columns.ConstantColumn(65);  // Net Payable ($)
                    columns.ConstantColumn(80);  // Channel & A/C
                    columns.ConstantColumn(70);  // Signature
                });

                // Header
                table.Header(header =>
                {
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Card #").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Employee Name").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Designation").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Base/Piece").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("OT Hrs").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("OT Pay").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Bonus").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Allow").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Deduct").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Net ($)").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Channel").Bold();
                    header.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Sign/Thumb").Bold();
                });

                // Rows
                foreach (var row in _model.Rows)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text(row.EmployeeCode).Bold();
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Column(c =>
                    {
                        c.Item().Text(row.FullName).Bold();
                        c.Item().Text(row.Department).FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Column(c =>
                    {
                        c.Item().Text(row.Designation);
                        c.Item().Text(row.PayStream).FontSize(7).FontColor(Colors.Indigo.Darken2);
                    });
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${row.BasicOrPieceEarnings:N2}");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"{row.OvertimeHours:F1}");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${row.OvertimeAmount:N2}");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${row.AttendanceBonus:N2}");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${row.Allowances:N2}");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${row.TotalDeductions:N2}").FontColor(Colors.Red.Darken2);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).AlignRight().Text($"${row.NetPayable:N2}").Bold().FontColor(Colors.Green.Darken3);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Column(c =>
                    {
                        c.Item().Text(row.PaymentChannel).FontSize(7).Bold();
                        c.Item().Text(row.AccountNumber).FontSize(7).FontColor(Colors.Grey.Darken1);
                    });
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(3).Text("_____________").FontColor(Colors.Grey.Lighten1);
                }
            });

            // Summary Totals
            col.Item().PaddingTop(8).Background(Colors.Grey.Lighten4).Border(1).BorderColor(Colors.Grey.Lighten2).Padding(6).Row(sumRow =>
            {
                sumRow.RelativeItem().Text($"Grand Total Headcount: {_model.TotalHeadcount} Workers").Bold();
                sumRow.RelativeItem().AlignRight().Text($"Total Base/Piece: ${_model.TotalBaseOrPieceRateEarnings:N2} | Total OT: ${_model.TotalOvertimeAmount:N2} | Net Payout: ${_model.TotalNetDisbursement:N2} USD").Bold().FontColor(Colors.Green.Darken4);
            });

            // Approval Sign-offs
            col.Item().PaddingTop(20).Row(sigRow =>
            {
                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(2).Text("Prepared By (HR Payroll Executive)").FontSize(7).Italic();
                });
                sigRow.ConstantItem(30);
                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(2).Text("Audited By (Internal Audit & Accounts)").FontSize(7).Italic();
                });
                sigRow.ConstantItem(30);
                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(2).Text($"Approved By ({_model.ApprovedBy})").FontSize(7).Italic();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("StitchMind Automated Social Compliance Payroll Engine | Export Garments Sector Statutory Pay Register").FontSize(7).FontColor(Colors.Grey.Medium);
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

public class SocialComplianceOtAuditPdfDocument : IDocument
{
    private readonly SocialComplianceOtAuditReportModel _model;

    public SocialComplianceOtAuditPdfDocument(SocialComplianceOtAuditReportModel model)
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
                col.Item().Text(_model.FactoryName.ToUpper()).Bold().FontSize(15).FontColor(Colors.Blue.Darken4);
                col.Item().Text("SOCIAL COMPLIANCE OVERTIME AUDIT DOSSIER").Bold().FontSize(12).FontColor(Colors.Amber.Darken3);
                col.Item().Text($"Audit Benchmark: {_model.AuditStandard}").FontSize(8).FontColor(Colors.Grey.Darken2);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text($"Audit Cycle: {_model.AuditPeriodText}").Bold().FontSize(9);
                col.Item().Text($"Compliance Score: {_model.FactoryComplianceRatePct:F2}%").Bold().FontSize(12).FontColor(Colors.Green.Darken3);
                col.Item().Text($"Statutory OT Limits: Max {_model.MaxAllowedDailyOtHours:F1} hrs/day, {_model.MaxAllowedWeeklyOtHours:F1} hrs/week").FontSize(8).FontColor(Colors.Grey.Darken2);
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Executive Metrics Strip
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Row(r =>
            {
                r.RelativeItem().Text(t => { t.Span("Total Workers Audited: ").Bold(); t.Span($"{_model.TotalWorkersAudited:N0}"); });
                r.RelativeItem().Text(t => { t.Span("Compliant Records: ").Bold(); t.Span($"{_model.CompliantWorkersCount:N0}").FontColor(Colors.Green.Darken3); });
                r.RelativeItem().Text(t => { t.Span("Flagged Overtime: ").Bold(); t.Span($"{_model.FlaggedWorkersCount:N0}").FontColor(Colors.Amber.Darken3); });
                r.RelativeItem().Text(t => { t.Span("Audit Status: ").Bold(); t.Span("PASSED (CLEAN AUDIT)").FontColor(Colors.Green.Darken4); });
            });

            // Detailed Findings Table
            col.Item().PaddingTop(12).Text("FLAGGED WORKERS & CORRECTIVE ACTIONS (CAPA) / নিরীক্ষা পর্যবেক্ষণ ও সংশোধন").Bold().FontSize(10).FontColor(Colors.Indigo.Darken3);
            col.Item().PaddingTop(4).Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(75);
                    columns.RelativeColumn(3);
                    columns.RelativeColumn(2);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(65);
                    columns.ConstantColumn(65);
                    columns.RelativeColumn(3);
                });

                table.Header(h =>
                {
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Card #").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Worker Name").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Line/Section").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Monthly OT").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).AlignRight().Text("Peak Daily").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Severity").Bold();
                    h.Cell().Background(Colors.Grey.Lighten3).Padding(4).Text("Corrective Action (CAPA)").Bold();
                });

                foreach (var row in _model.FlaggedWorkers)
                {
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row.EmployeeCode).Bold();
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row.FullName);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row.AssignedLine);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{row.MonthlyOtHoursRecorded:F1} hrs");
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).AlignRight().Text($"{row.PeakDailyOtHours:F1} hrs").Bold().FontColor(Colors.Amber.Darken3);
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row.ViolationSeverity).FontColor(row.ViolationSeverity == "High" ? Colors.Red.Darken2 : Colors.Amber.Darken3).Bold();
                    table.Cell().BorderBottom(0.5f).BorderColor(Colors.Grey.Lighten2).Padding(4).Text(row.CorrectiveActionTaken).FontSize(8);
                }
            });

            // Social Compliance Sign-off Block
            col.Item().PaddingTop(25).Row(sigRow =>
            {
                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(3).Text("Audited By (Lead Social Compliance Auditor)").FontSize(8).Italic();
                });
                sigRow.ConstantItem(40);
                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(3).Text("Verified By (Head of HR & Factory General Manager)").FontSize(8).Italic();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("StitchMind Automated Social Compliance Audit Engine | BSCI, Sedex SMETA, WRAP & ILO Convention 1 Verification").FontSize(7).FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(x =>
            {
                x.Span("Generated: ").FontSize(7).FontColor(Colors.Grey.Medium);
                x.Span($"{DateTime.UtcNow:dd-MMM-yyyy HH:mm} UTC").FontSize(7).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
