using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;
namespace GarmentsERP.Auth.Application.Commands.Logout;
public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IAuthRepository _repo;
    public LogoutCommandHandler(IAuthRepository repo) => _repo = repo;
    public async Task<Result> Handle(LogoutCommand request, CancellationToken ct)
    {
        var token = await _repo.GetRefreshTokenAsync(request.RefreshToken, ct);
        if (token is not null) { token.Revoke(); await _repo.SaveChangesAsync(ct); }
        return Result.Success();
    }
}
