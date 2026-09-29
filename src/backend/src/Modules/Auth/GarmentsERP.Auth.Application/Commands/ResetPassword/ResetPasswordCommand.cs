using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Commands.ResetPassword;

public record ResetPasswordCommand(
    string Email,
    string Otp,
    string NewPassword,
    string ConfirmPassword) : IRequest<Result>;
