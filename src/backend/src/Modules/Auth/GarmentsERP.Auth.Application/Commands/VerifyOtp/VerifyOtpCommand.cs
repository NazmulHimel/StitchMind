using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Commands.VerifyOtp;

public record VerifyOtpCommand(string Email, string Otp) : IRequest<Result<bool>>;
