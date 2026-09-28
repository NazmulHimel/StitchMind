using System.Diagnostics;
using GarmentsERP.Reporting.Application.Interfaces;
using GarmentsERP.Reporting.Application.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace GarmentsERP.Reporting.Infrastructure.Services;

/// <summary>
/// Implements SCH-03: Secure SMTP / SES email dispatcher service with automated retry policies.
/// Handles in-memory attachment streams directly from QuestPDF buffers without touching disk.
/// </summary>
public class MailKitEmailDispatcher : IEmailDispatcher
{
    private readonly SmtpConfigurationOptions _options;
    private readonly ILogger<MailKitEmailDispatcher> _logger;

    public MailKitEmailDispatcher(
        IOptions<SmtpConfigurationOptions> options,
        ILogger<MailKitEmailDispatcher> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<EmailDispatchResult> SendReportEmailAsync(
        string to,
        string subject,
        string bodyHtml,
        Stream attachmentStream,
        string fileName,
        CancellationToken ct = default)
    {
        var recipients = to.Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return await SendBatchReportEmailAsync(recipients, subject, bodyHtml, attachmentStream, fileName, ct);
    }

    public async Task<EmailDispatchResult> SendBatchReportEmailAsync(
        IEnumerable<string> recipients,
        string subject,
        string bodyHtml,
        Stream attachmentStream,
        string fileName,
        CancellationToken ct = default)
    {
        var stopwatch = Stopwatch.StartNew();
        var recipientList = recipients.ToList();

        if (recipientList.Count == 0)
        {
            return EmailDispatchResult.Failure("No valid recipients provided.", 0, 0);
        }

        // Reset attachment stream position
        if (attachmentStream.CanSeek)
        {
            attachmentStream.Position = 0;
        }

        // If simulated delivery is enabled (development, staging, or automated CI tests)
        if (_options.EnableSimulatedDelivery)
        {
            var simulatedId = $"SIM-MSG-{Guid.NewGuid():N}";
            _logger.LogInformation(
                "[SIMULATED EMAIL DISPATCH] Sent to {Count} recipients ({Recipients}). Subject: '{Subject}'. Attachment: '{FileName}' ({Bytes} bytes). Simulated MessageId: {MessageId}",
                recipientList.Count,
                string.Join(", ", recipientList),
                subject,
                fileName,
                attachmentStream.Length,
                simulatedId);

            stopwatch.Stop();
            return EmailDispatchResult.Success(simulatedId, 1, stopwatch.ElapsedMilliseconds);
        }

        // Live SMTP / SES Dispatch with Automated Exponential Backoff Retry Policy
        int attempts = 0;
        int maxRetries = Math.Max(1, _options.MaxRetryAttempts);
        int delayMs = Math.Max(500, _options.InitialRetryDelayMs);
        string lastError = string.Empty;

        while (attempts < maxRetries)
        {
            attempts++;
            ct.ThrowIfCancellationRequested();

            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_options.SenderName, _options.SenderEmail));
                foreach (var rcpt in recipientList)
                {
                    message.To.Add(MailboxAddress.Parse(rcpt));
                }
                message.Subject = subject;

                var builder = new BodyBuilder
                {
                    HtmlBody = bodyHtml
                };

                if (attachmentStream.Length > 0)
                {
                    attachmentStream.Position = 0;
                    builder.Attachments.Add(fileName, attachmentStream, ContentType.Parse("application/pdf"));
                }

                message.Body = builder.ToMessageBody();

                using var client = new SmtpClient();
                client.Timeout = 15000; // 15s timeout

                var secureOption = _options.UseSsl 
                    ? SecureSocketOptions.Auto 
                    : SecureSocketOptions.None;

                await client.ConnectAsync(_options.Host, _options.Port, secureOption, ct);

                if (!string.IsNullOrEmpty(_options.UserName) && !string.IsNullOrEmpty(_options.Password))
                {
                    await client.AuthenticateAsync(_options.UserName, _options.Password, ct);
                }

                var response = await client.SendAsync(message, ct);
                await client.DisconnectAsync(true, ct);

                stopwatch.Stop();
                var messageId = message.MessageId ?? $"SMTP-{Guid.NewGuid():N}";

                _logger.LogInformation(
                    "Email successfully delivered on attempt {Attempt}/{MaxRetries} to {Count} recipients. SmtpResponse: {Response}. Duration: {Duration}ms",
                    attempts, maxRetries, recipientList.Count, response, stopwatch.ElapsedMilliseconds);

                return EmailDispatchResult.Success(messageId, attempts, stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                lastError = ex.Message;
                _logger.LogWarning(
                    ex,
                    "SMTP dispatch attempt {Attempt}/{MaxRetries} failed: {Error}. Retrying in {Delay}ms...",
                    attempts, maxRetries, ex.Message, delayMs);

                if (attempts < maxRetries)
                {
                    await Task.Delay(delayMs, ct);
                    delayMs *= 2; // Exponential backoff (1s, 2s, 4s...)
                }
            }
        }

        stopwatch.Stop();
        _logger.LogError("SMTP dispatch failed after {Attempts} attempts. Last error: {Error}", attempts, lastError);
        return EmailDispatchResult.Failure($"Failed after {attempts} attempts: {lastError}", attempts, stopwatch.ElapsedMilliseconds);
    }
}
