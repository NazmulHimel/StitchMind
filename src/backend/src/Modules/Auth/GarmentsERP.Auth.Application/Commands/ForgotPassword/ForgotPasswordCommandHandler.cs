using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace GarmentsERP.Auth.Application.Commands.ForgotPassword;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, Result>
{
    private readonly IAuthRepository _repo;
    private readonly IOtpService _otpService;
    private readonly IAuthEmailService _emailService;
    private readonly ILogger<ForgotPasswordCommandHandler> _logger;

    public ForgotPasswordCommandHandler(
        IAuthRepository repo,
        IOtpService otpService,
        IAuthEmailService emailService,
        ILogger<ForgotPasswordCommandHandler> logger)
    {
        _repo = repo;
        _otpService = otpService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result> Handle(ForgotPasswordCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await _repo.GetUserByEmailAsync(email, ct);

        // Prevent user enumeration attacks: return generic success even if user not found
        if (user is null || !user.IsActive)
        {
            _logger.LogInformation("Password reset requested for non-existent or inactive email: {Email}", email);
            return Result.Success();
        }

        var otpResult = await _otpService.GenerateOtpAsync(user.Email, ct);
        if (!otpResult.Success)
        {
            return Result.Failure(otpResult.ErrorMessage ?? "Please wait before requesting another code.");
        }

        await _emailService.SendPasswordResetOtpAsync(user.Email, user.FullName, otpResult.Otp!, ct);

        _logger.LogInformation("Password reset OTP generated and dispatched for user: {Email}", user.Email);
        return Result.Success();
    }
}
