using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Shared.Common;
using MediatR;
namespace GarmentsERP.Auth.Application.Commands.RefreshToken;
public record RefreshTokenCommand(string AccessToken, string RefreshToken) : IRequest<Result<AuthResponseDto>>;
