using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Domain.Entities;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Constants;
using MediatR;
namespace GarmentsERP.Auth.Application.Commands.RefreshToken;
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponseDto>>
{
    private readonly IAuthRepository _repo;
    private readonly IJwtTokenService _jwtSvc;
    private readonly IMenuBuilderService _menuSvc;
    public RefreshTokenCommandHandler(IAuthRepository repo, IJwtTokenService jwtSvc, IMenuBuilderService menuSvc)
        => (_repo, _jwtSvc, _menuSvc) = (repo, jwtSvc, menuSvc);

    public async Task<Result<AuthResponseDto>> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var existing = await _repo.GetRefreshTokenAsync(request.RefreshToken, ct);
        if (existing is null || !existing.IsActive())
            return Result<AuthResponseDto>.Failure("Invalid or expired refresh token.");

        var user = await _repo.GetUserByIdAsync(existing.UserId, ct);
        if (user is null || !user.IsActive)
            return Result<AuthResponseDto>.Failure("User not found or deactivated.");

        existing.Revoke();
        var permissions = await _repo.GetUserPermissionsAsync(user.Id, ct);
        var tokenResult = _jwtSvc.GenerateTokens(user, permissions);
        var newRefresh = GarmentsERP.Auth.Domain.Entities.RefreshToken.Create(user.Id, tokenResult.RefreshToken,
            AppConstants.JwtRefreshTokenExpiryDays, existing.DeviceInfo, existing.IpAddress);

        await _repo.AddRefreshTokenAsync(newRefresh, ct);
        await _repo.SaveChangesAsync(ct);
        var menuItems = _menuSvc.BuildMenu(permissions);
        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            user.Id, user.FullName, user.UserName, user.Email,
            tokenResult.AccessToken, tokenResult.RefreshToken, tokenResult.Expiry,
            permissions, menuItems));
    }
}

