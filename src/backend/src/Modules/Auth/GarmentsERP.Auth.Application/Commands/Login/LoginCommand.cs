using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Commands.Login;

public record LoginCommand(string UserNameOrEmail, string Password, bool RememberMe,
    string? IpAddress, string? DeviceInfo) : IRequest<Result<AuthResponseDto>>;
