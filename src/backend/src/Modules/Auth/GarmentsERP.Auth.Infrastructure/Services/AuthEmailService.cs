using GarmentsERP.Auth.Application.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace GarmentsERP.Auth.Infrastructure.Services;

public class AuthEmailService : IAuthEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthEmailService> _logger;

    public AuthEmailService(IConfiguration configuration, ILogger<AuthEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otp, CancellationToken ct = default)
    {
        var subject = "StitchMind - Password Reset Verification Code";
        var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='utf-8'>
  <style>
    body {{ font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; }}
    .container {{ max-width: 520px; margin: 0 auto; background: #ffffff; border-radius: 16px; border: 1px solid #e2e8f0; padding: 36px; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05); }}
    .header {{ text-align: center; margin-bottom: 24px; }}
    .brand {{ font-size: 24px; font-weight: 800; color: #0f172a; margin: 0; }}
    .brand span {{ color: #2563eb; }}
    .title {{ font-size: 18px; font-weight: 700; color: #1e293b; margin: 16px 0 8px; }}
    .text {{ color: #475569; font-size: 14px; line-height: 1.6; margin: 8px 0; }}
    .otp-box {{ background: #eff6ff; border: 2px dashed #93c5fd; border-radius: 12px; padding: 20px; text-align: center; margin: 24px 0; }}
    .otp-code {{ font-size: 36px; font-weight: 800; letter-spacing: 8px; color: #1d4ed8; font-family: 'Courier New', monospace; }}
    .expiry {{ font-size: 12px; color: #64748b; margin-top: 8px; }}
    .warning {{ background: #fffbeb; border: 1px solid #fef3c7; border-radius: 8px; padding: 12px; font-size: 12px; color: #92400e; margin: 16px 0; }}
    .footer {{ border-top: 1px solid #e2e8f0; padding-top: 20px; margin-top: 28px; text-align: center; font-size: 12px; color: #94a3b8; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='header'>
      <h1 class='brand'>Stitch<span>Mind</span></h1>
      <p style='color: #64748b; font-size: 13px; margin: 4px 0 0;'>Intelligent Apparel ERP Platform</p>
    </div>
    <div class='title'>Password Reset Request</div>
    <p class='text'>Hello {System.Net.WebUtility.HtmlEncode(fullName)},</p>
    <p class='text'>We received a request to reset your password. Use the verification code below to complete the process:</p>
    <div class='otp-box'>
      <div class='otp-code'>{otp}</div>
      <div class='expiry'>This code expires in <strong>2 minutes</strong></div>
    </div>
    <div class='warning'>
      <strong>Security notice:</strong> If you did not request a password reset, please ignore this email or contact your administrator immediately. Never share this code with anyone.
    </div>
    <div class='footer'>
      &copy; {DateTime.UtcNow.Year} StitchMind Garments ERP. All rights reserved.
    </div>
  </div>
</body>
</html>";

        // Always log for development visibility and debugging
        _logger.LogInformation("==================================================================");
        _logger.LogInformation("[STITCHMIND AUTH] Password Reset OTP for {Email} is: {Otp}", toEmail, otp);
        _logger.LogInformation("==================================================================");

        var smtpSection = _configuration.GetSection("SmtpConfiguration");
        var host = smtpSection["Host"];
        if (string.IsNullOrWhiteSpace(host) || host.Contains("internal") || host == "localhost")
        {
            // Simulated / local development delivery
            _logger.LogInformation("[STITCHMIND AUTH] Simulated email delivery to {Email} with OTP: {Otp}", toEmail, otp);
            return;
        }

        try
        {
            var message = new MimeMessage();
            var senderEmail = smtpSection["SenderEmail"] ?? "security@stitchmind.com";
            var senderName = smtpSection["SenderName"] ?? "StitchMind Security";

            message.From.Add(new MailboxAddress(senderName, senderEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
            message.Body = bodyBuilder.ToMessageBody();

            using var client = new SmtpClient();
            client.Timeout = 10000;

            var port = int.TryParse(smtpSection["Port"], out var p) ? p : 587;
            var useSsl = bool.TryParse(smtpSection["UseSsl"], out var s) && s;

            await client.ConnectAsync(host, port, useSsl ? SecureSocketOptions.Auto : SecureSocketOptions.StartTlsWhenAvailable, ct);

            var user = smtpSection["UserName"];
            var pass = smtpSection["Password"];
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(pass))
            {
                await client.AuthenticateAsync(user, pass, ct);
            }

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("[STITCHMIND AUTH] Live email with OTP successfully delivered to {Email}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "[STITCHMIND AUTH] Could not deliver email via live SMTP server. Logged OTP for dev use.");
        }
    }
}
