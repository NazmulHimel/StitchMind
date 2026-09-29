using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Commands.ForgotPassword;

public record ForgotPasswordCommand(string Email) : IRequest<Result>;
