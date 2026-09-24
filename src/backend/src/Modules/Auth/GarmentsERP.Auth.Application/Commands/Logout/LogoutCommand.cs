using GarmentsERP.Shared.Common;
using MediatR;
namespace GarmentsERP.Auth.Application.Commands.Logout;
public record LogoutCommand(string RefreshToken) : IRequest<Result>;
