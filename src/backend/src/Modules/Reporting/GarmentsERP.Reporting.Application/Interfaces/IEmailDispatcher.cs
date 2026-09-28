using GarmentsERP.Reporting.Application.Models;

namespace GarmentsERP.Reporting.Application.Interfaces;

public interface IEmailDispatcher
{
    Task<EmailDispatchResult> SendReportEmailAsync(
        string to,
        string subject,
        string bodyHtml,
        Stream attachmentStream,
        string fileName,
        CancellationToken ct = default);

    Task<EmailDispatchResult> SendBatchReportEmailAsync(
        IEnumerable<string> recipients,
        string subject,
        string bodyHtml,
        Stream attachmentStream,
        string fileName,
        CancellationToken ct = default);
}
