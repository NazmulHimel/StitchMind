using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Commands.VerifyOtp;

public class VerifyOtpCommandHandler : IRequestHandler<VerifyOtpCommand, Result<bool>>
{
    private readonly IOtpService _otpService;

    public VerifyOtpCommandHandler(IOtpService otpService)
    {
        _otpService = otpService;
    }

    public async Task<Result<bool>> Handle(VerifyOtpCommand request, CancellationToken ct)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var isValid = await _otpService.VerifyOtpAsync(email, request.Otp.Trim(), ct);

        if (!isValid)
        {
            return Result<bool>.Failure("Invalid or expired verification code. Please check and try again.");
        }

        return Result<bool>.Success(true);
    }
}
