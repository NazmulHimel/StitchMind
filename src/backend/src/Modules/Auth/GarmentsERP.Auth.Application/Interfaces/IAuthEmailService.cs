namespace GarmentsERP.Auth.Application.Interfaces;

public interface IAuthEmailService
{
    Task SendPasswordResetOtpAsync(string toEmail, string fullName, string otp, CancellationToken ct = default);
}
