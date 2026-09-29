namespace GarmentsERP.Auth.Application.Interfaces;

public record OtpResult(bool Success, string? Otp, int SecondsRemaining = 0, string? ErrorMessage = null)
{
    public static OtpResult Ok(string otp) => new(true, otp);
    public static OtpResult Cooldown(int secondsRemaining, string message) => new(false, null, secondsRemaining, message);
    public static OtpResult Fail(string message) => new(false, null, 0, message);
}

public interface IOtpService
{
    Task<OtpResult> GenerateOtpAsync(string email, CancellationToken ct = default);
    Task<bool> VerifyOtpAsync(string email, string otp, CancellationToken ct = default);
    Task<bool> IsOtpVerifiedAsync(string email, CancellationToken ct = default);
    Task ClearOtpAsync(string email, CancellationToken ct = default);
    Task<int> GetRemainingCooldownSecondsAsync(string email, CancellationToken ct = default);
}
