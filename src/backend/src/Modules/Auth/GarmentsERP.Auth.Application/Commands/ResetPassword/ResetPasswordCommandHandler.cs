using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GarmentsERP.Auth.Application.Commands.ResetPassword;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, Result>
{
    private readonly IAuthRepository _repo;
    private readonly IOtpService _otpService;
    private readonly IPasswordService _passwordService;
    private readonly ILogger<ResetPasswordCommandHandler> _logger;

    public ResetPasswordCommandHandler(
        IAuthRepository repo,
        IOtpService otpService,
        IPasswordService passwordService,
        ILogger<ResetPasswordCommandHandler> logger)
    {
        _repo = repo;
        _otpService = otpService;
        _passwordService = passwordService;
        _logger = logger;
    }

    public async Task<Result> Handle(ResetPasswordCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        // 1. Verify OTP status
        var isOtpVerified = await _otpService.IsOtpVerifiedAsync(email, ct);
        if (!isOtpVerified)
        {
            var isValidOtp = await _otpService.VerifyOtpAsync(email, request.Otp.Trim(), ct);
            if (!isValidOtp)
            {
                return Result.Failure("Invalid or expired OTP code.");
            }
        }

        // 2. Validate passwords match
        if (request.NewPassword != request.ConfirmPassword)
        {
            return Result.Failure("Passwords do not match.");
        }

        // 3. Validate password complexity
        if (!_passwordService.MeetsComplexity(request.NewPassword))
        {
            return Result.Failure("Password must be at least 8 characters and include uppercase, lowercase, digit, and special character.");
        }

        // 4. Retrieve user account
        var user = await _repo.GetUserByEmailAsync(email, ct);
        if (user is null)
        {
            return Result.Failure("User account not found.");
        }

        if (!user.IsActive)
        {
            return Result.Failure("Account is deactivated. Please contact administrator.");
        }

        // 5. Update user password
        var newHash = _passwordService.Hash(request.NewPassword);
        user.ChangePassword(newHash);

        await _repo.UpdateUserAsync(user, ct);
        await _repo.RevokeAllUserTokensAsync(user.Id, ct);
        await _repo.SaveChangesAsync(ct);

        // 6. Clear OTP session
        await _otpService.ClearOtpAsync(email, ct);

        _logger.LogInformation("Password successfully reset for user: {Email}", email);
        return Result.Success();
    }
}
