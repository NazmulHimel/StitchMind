using System.Collections.Concurrent;
using System.Security.Cryptography;
using GarmentsERP.Auth.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace GarmentsERP.Auth.Infrastructure.Services;

public class OtpService : IOtpService
{
    private readonly ConcurrentDictionary<string, OtpRecord> _cache = new(StringComparer.OrdinalIgnoreCase);
    private readonly ILogger<OtpService> _logger;
    private const int MaxFailedAttempts = 5;
    
    // OTP is valid for exactly 2 minutes
    private static readonly TimeSpan DefaultExpiry = TimeSpan.FromMinutes(2);

    public OtpService(ILogger<OtpService> logger)
    {
        _logger = logger;
    }

    public Task<OtpResult> GenerateOtpAsync(string email, CancellationToken ct = default)
    {
        // Check if an unexpired OTP is already active for this email within the 2-minute window
        if (_cache.TryGetValue(email, out var existing) && DateTime.UtcNow < existing.ExpiresAtUtc)
        {
            var remainingSeconds = (int)Math.Ceiling((existing.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds);
            _logger.LogWarning("[OTP SERVICE] Cooldown active for {Email}. {Seconds}s remaining before another OTP can be requested.",
                email, remainingSeconds);

            return Task.FromResult(OtpResult.Cooldown(
                remainingSeconds,
                $"An OTP has already been sent to this email. Please wait {remainingSeconds} seconds before requesting a new code."));
        }

        // Generate secure 6-digit OTP: 100000 to 999999
        var otp = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();
        var record = new OtpRecord(
            Otp: otp,
            ExpiresAtUtc: DateTime.UtcNow.Add(DefaultExpiry),
            FailedAttempts: 0,
            IsVerified: false
        );

        _cache[email] = record;
        _logger.LogInformation("[OTP SERVICE] Generated 2-minute OTP {Otp} for {Email} (Expires: {Expiry})",
            otp, email, record.ExpiresAtUtc);

        return Task.FromResult(OtpResult.Ok(otp));
    }

    public Task<bool> VerifyOtpAsync(string email, string otp, CancellationToken ct = default)
    {
        if (!_cache.TryGetValue(email, out var record))
        {
            _logger.LogWarning("[OTP SERVICE] No OTP found for {Email}", email);
            return Task.FromResult(false);
        }

        if (DateTime.UtcNow > record.ExpiresAtUtc)
        {
            _cache.TryRemove(email, out _);
            _logger.LogWarning("[OTP SERVICE] OTP for {Email} has expired (exceeded 2 minutes)", email);
            return Task.FromResult(false);
        }

        if (record.FailedAttempts >= MaxFailedAttempts)
        {
            _cache.TryRemove(email, out _);
            _logger.LogWarning("[OTP SERVICE] Too many failed OTP attempts for {Email}", email);
            return Task.FromResult(false);
        }

        if (record.Otp != otp)
        {
            var updated = record with { FailedAttempts = record.FailedAttempts + 1 };
            _cache[email] = updated;
            _logger.LogWarning("[OTP SERVICE] Invalid OTP attempt for {Email}. Attempt {Count}/{Max}",
                email, updated.FailedAttempts, MaxFailedAttempts);
            return Task.FromResult(false);
        }

        // Successfully matched
        _cache[email] = record with { IsVerified = true };
        _logger.LogInformation("[OTP SERVICE] OTP successfully verified for {Email}", email);
        return Task.FromResult(true);
    }

    public Task<bool> IsOtpVerifiedAsync(string email, CancellationToken ct = default)
    {
        if (!_cache.TryGetValue(email, out var record))
        {
            return Task.FromResult(false);
        }

        if (DateTime.UtcNow > record.ExpiresAtUtc)
        {
            _cache.TryRemove(email, out _);
            return Task.FromResult(false);
        }

        return Task.FromResult(record.IsVerified);
    }

    public Task ClearOtpAsync(string email, CancellationToken ct = default)
    {
        _cache.TryRemove(email, out _);
        return Task.CompletedTask;
    }

    public Task<int> GetRemainingCooldownSecondsAsync(string email, CancellationToken ct = default)
    {
        if (_cache.TryGetValue(email, out var record) && DateTime.UtcNow < record.ExpiresAtUtc)
        {
            var remaining = (int)Math.Ceiling((record.ExpiresAtUtc - DateTime.UtcNow).TotalSeconds);
            return Task.FromResult(remaining);
        }

        return Task.FromResult(0);
    }

    private record OtpRecord(string Otp, DateTime ExpiresAtUtc, int FailedAttempts, bool IsVerified);
}
