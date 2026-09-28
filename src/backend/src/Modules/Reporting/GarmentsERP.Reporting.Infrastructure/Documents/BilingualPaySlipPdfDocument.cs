namespace GarmentsERP.Reporting.Infrastructure.Documents;

using GarmentsERP.Reporting.Application.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

public class BilingualPaySlipPdfDocument : IDocument
{
    private readonly BilingualPaySlipReportModel _model;

    public BilingualPaySlipPdfDocument(BilingualPaySlipReportModel model)
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
                col.Item().Text(_model.FactoryAddress).FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().Text($"Factory Reg / DIFE License: {_model.FactoryLicense}").FontSize(8).FontColor(Colors.Grey.Darken1);
            });

            row.RelativeItem().AlignRight().Column(col =>
            {
                col.Item().Text("MONTHLY PAY SLIP / বেতন রশিদ").Bold().FontSize(14).FontColor(Colors.Indigo.Darken3);
                col.Item().Text($"Pay Period: {_model.MonthYearText}").Bold().FontSize(9);
                col.Item().Text($"Slip Ref: {_model.PaySlipNumber}").FontSize(8).FontColor(Colors.Grey.Darken2);
                col.Item().Text("Social Compliance Audit Ready").FontSize(7).FontColor(Colors.Green.Darken2).Bold();
            });
        });
    }

    private void ComposeContent(IContainer container)
    {
        container.PaddingVertical(10).Column(col =>
        {
            // Employee Profile Grid
            col.Item().Border(1).BorderColor(Colors.Grey.Lighten2).Background(Colors.Grey.Lighten4).Padding(8).Column(empCol =>
            {
                empCol.Item().Row(r =>
                {
                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Employee Name / নাম: ").Bold(); t.Span($"{_model.FullNameEnglish} ({_model.FullNameBangla})"); });
                        c.Item().Text(t => { t.Span("Employee Code / কার্ড নং: ").Bold(); t.Span(_model.EmployeeCode); });
                        c.Item().Text(t => { t.Span("Designation / পদবী: ").Bold(); t.Span($"{_model.DesignationEnglish} ({_model.DesignationBangla})"); });
                    });

                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Department / বিভাগ: ").Bold(); t.Span($"{_model.DepartmentEnglish} ({_model.DepartmentBangla})"); });
                        c.Item().Text(t => { t.Span("Assigned Line / লাইন: ").Bold(); t.Span(_model.AssignedLine); });
                        c.Item().Text(t => { t.Span("Badge Number / ব্যাজ: ").Bold(); t.Span(_model.BiometricBadgeNumber); });
                    });

                    r.RelativeItem().Column(c =>
                    {
                        c.Item().Text(t => { t.Span("Pay Stream / ধরণ: ").Bold(); t.Span(_model.PayStream); });
                        c.Item().Text(t => { t.Span("Joining Date / যোগদান: ").Bold(); t.Span($"{_model.DateOfJoining:dd-MMM-yyyy}"); });
                        c.Item().Text(t => { t.Span("Disbursement / মাধ্যম: ").Bold(); t.Span($"{_model.DisbursementChannel} ({_model.MfsOrAccountNumber})"); });
                    });
                });
            });

            // Attendance & Piece-rate Summary Strip
            col.Item().PaddingTop(8).Border(1).BorderColor(Colors.Blue.Lighten3).Background(Colors.Blue.Lighten5).Padding(6).Row(attRow =>
            {
                attRow.RelativeItem().Text(t => { t.Span("Working Days / মোট দিন: ").Bold(); t.Span($"{_model.TotalWorkingDays}"); });
                attRow.RelativeItem().Text(t => { t.Span("Present / উপস্থিত: ").Bold(); t.Span($"{_model.DaysPresent}"); });
                attRow.RelativeItem().Text(t => { t.Span("Absent / অনুপস্থিত: ").Bold(); t.Span($"{_model.DaysAbsent}"); });
                attRow.RelativeItem().Text(t => { t.Span("Late / বিলম্ব: ").Bold(); t.Span($"{_model.DaysLate}"); });
                if (_model.CompletedGarmentUnits > 0)
                {
                    attRow.RelativeItem().Text(t => { t.Span("Garments Sewn: ").Bold(); t.Span($"{_model.CompletedGarmentUnits:N0} pcs"); });
                }
            });

            // Earnings & Deductions Dual-Table
            col.Item().PaddingTop(10).Row(tableRow =>
            {
                // Left Column: Gross Earnings
                tableRow.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(earnCol =>
                {
                    earnCol.Item().Text("EARNINGS / উপার্জন বিবরণী").Bold().FontSize(10).FontColor(Colors.Blue.Darken3);
                    earnCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    if (_model.CompletedGarmentUnits > 0)
                    {
                        earnCol.Item().PaddingVertical(2).Row(r =>
                        {
                            r.RelativeItem().Text($"Piece-Rate Production ({_model.CompletedGarmentUnits:N0} @ ${_model.PieceRatePerUnit:F4})");
                            r.ConstantItem(75).AlignRight().Text($"${_model.PieceRateEarnings:N2}").Bold();
                        });
                    }
                    else
                    {
                        earnCol.Item().PaddingVertical(2).Row(r =>
                        {
                            r.RelativeItem().Text("Basic Salary / মূল বেতন");
                            r.ConstantItem(75).AlignRight().Text($"${_model.BasicSalary:N2}").Bold();
                        });
                        earnCol.Item().PaddingVertical(2).Row(r =>
                        {
                            r.RelativeItem().Text("House Rent & Medical / বাড়ি ভাড়া ও চিকিৎসা");
                            r.ConstantItem(75).AlignRight().Text($"${_model.Allowances:N2}");
                        });
                    }

                    earnCol.Item().PaddingVertical(2).Row(r =>
                    {
                        r.RelativeItem().Text($"Overtime Pay ({_model.OvertimeHours:F1} hrs @ ${_model.OvertimeRatePerHour:F2}/hr)");
                        r.ConstantItem(75).AlignRight().Text($"${_model.OvertimeAmount:N2}");
                    });

                    earnCol.Item().PaddingVertical(2).Row(r =>
                    {
                        r.RelativeItem().Text("Attendance Bonus (100% on-time)");
                        r.ConstantItem(75).AlignRight().Text($"${_model.AttendanceBonus:N2}");
                    });

                    earnCol.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    earnCol.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text("GROSS EARNINGS / মোট উপার্জন").Bold();
                        r.ConstantItem(75).AlignRight().Text($"${_model.GrossEarnings:N2}").Bold().FontColor(Colors.Blue.Darken3);
                    });
                });

                // Right Column: Statutory & Factory Deductions
                tableRow.RelativeItem().Border(1).BorderColor(Colors.Grey.Lighten2).Padding(8).Column(dedCol =>
                {
                    dedCol.Item().Text("DEDUCTIONS / কর্তন বিবরণী").Bold().FontSize(10).FontColor(Colors.Red.Darken3);
                    dedCol.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    dedCol.Item().PaddingVertical(2).Row(r =>
                    {
                        r.RelativeItem().Text("Absenteeism Deduction / অনুপস্থিতি কর্তন");
                        r.ConstantItem(75).AlignRight().Text($"${_model.AbsentDeduction:N2}");
                    });

                    dedCol.Item().PaddingVertical(2).Row(r =>
                    {
                        r.RelativeItem().Text("Late Penalty Deduction / বিলম্ব জরিমানা");
                        r.ConstantItem(75).AlignRight().Text($"${_model.LatePenaltyDeduction:N2}");
                    });

                    dedCol.Item().PaddingVertical(2).Row(r =>
                    {
                        r.RelativeItem().Text("Tax & Statutory Withholding (TDS)");
                        r.ConstantItem(75).AlignRight().Text($"${_model.TaxDeduction:N2}");
                    });

                    dedCol.Item().PaddingVertical(2).Row(r =>
                    {
                        r.RelativeItem().Text("Advance Salary Recovery / অগ্রিম সমন্বয়");
                        r.ConstantItem(75).AlignRight().Text($"${_model.AdvanceDeduction:N2}");
                    });

                    dedCol.Item().PaddingTop(6).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                    dedCol.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL DEDUCTIONS / মোট কর্তন").Bold();
                        r.ConstantItem(75).AlignRight().Text($"${_model.TotalDeductions:N2}").Bold().FontColor(Colors.Red.Darken3);
                    });
                });
            });

            // Net Disbursement Banner
            col.Item().PaddingTop(10).Border(1.5f).BorderColor(Colors.Green.Darken2).Background(Colors.Green.Lighten5).Padding(10).Row(netRow =>
            {
                netRow.RelativeItem().Column(c =>
                {
                    c.Item().Text("NET PAYABLE AMOUNT / প্রদেয় মোট টাকা").Bold().FontSize(11).FontColor(Colors.Green.Darken3);
                    c.Item().Text($"Payment Channel: {_model.DisbursementChannel} | Wallet A/C: {_model.MfsOrAccountNumber} | Ref: {_model.PaymentReference}").FontSize(8).FontColor(Colors.Grey.Darken2);
                });

                netRow.ConstantItem(150).AlignRight().Column(c =>
                {
                    c.Item().Text($"${_model.NetPayable:N2} USD").Bold().FontSize(16).FontColor(Colors.Green.Darken4);
                    c.Item().Text("DIRECT MFS DISBURSED").FontSize(7).FontColor(Colors.Green.Darken2).Bold();
                });
            });

            // Signatures & Compliance Block
            col.Item().PaddingTop(25).Row(sigRow =>
            {
                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(3).Text("Prepared By (HR Payroll Executive)").FontSize(8).Italic();
                });

                sigRow.ConstantItem(25);

                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(3).Text("Audited By (Compliance Officer)").FontSize(8).Italic();
                });

                sigRow.ConstantItem(25);

                sigRow.RelativeItem().Column(c =>
                {
                    c.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Medium);
                    c.Item().PaddingTop(3).Text("Worker Signature / কর্মী স্বাক্ষর").FontSize(8).Italic();
                });
            });
        });
    }

    private void ComposeFooter(IContainer container)
    {
        container.Row(row =>
        {
            row.RelativeItem().Text("StitchMind Automated Social Compliance Payroll Engine | International Labor Standards (ILO) & Bangladesh Labor Act Compliant").FontSize(7).FontColor(Colors.Grey.Medium);
            row.RelativeItem().AlignRight().Text(x =>
            {
                x.Span("Generated: ").FontSize(7).FontColor(Colors.Grey.Medium);
                x.Span($"{_model.GeneratedAt:dd-MMM-yyyy HH:mm} UTC").FontSize(7).FontColor(Colors.Grey.Medium);
            });
        });
    }
}
