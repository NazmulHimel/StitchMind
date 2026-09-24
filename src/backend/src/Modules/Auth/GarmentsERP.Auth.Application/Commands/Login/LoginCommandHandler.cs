using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Domain.Entities;
using GarmentsERP.Shared.Common;
using GarmentsERP.Shared.Constants;
using MediatR;

namespace GarmentsERP.Auth.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponseDto>>
{
    private readonly IAuthRepository _repo;
    private readonly IPasswordService _passwordSvc;
    private readonly IJwtTokenService _jwtSvc;
    private readonly IMenuBuilderService _menuSvc;

    public LoginCommandHandler(IAuthRepository repo, IPasswordService passwordSvc,
        IJwtTokenService jwtSvc, IMenuBuilderService menuSvc)
    {
        _repo = repo; _passwordSvc = passwordSvc; _jwtSvc = jwtSvc; _menuSvc = menuSvc;
    }

    public async Task<Result<AuthResponseDto>> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _repo.GetUserByEmailOrUserNameAsync(request.UserNameOrEmail, ct);

        if (user is null)
            return Result<AuthResponseDto>.Failure("Invalid username or password.");

        if (!user.IsActive)
            return Result<AuthResponseDto>.Failure("Account is deactivated. Please contact administrator.");

        if (user.IsLockedOut())
            return Result<AuthResponseDto>.Failure("Account is temporarily locked due to multiple failed attempts. Try again later.");

        if (!_passwordSvc.Verify(request.Password, user.PasswordHash))
        {
            user.RecordFailedLogin(AppConstants.MaxLoginAttempts, AppConstants.AccountLockoutMinutes);
            await _repo.UpdateUserAsync(user);
            await _repo.SaveChangesAsync(ct);
            return Result<AuthResponseDto>.Failure("Invalid username or password.");
        }

        user.RecordSuccessfulLogin();
        var permissions = await _repo.GetUserPermissionsAsync(user.Id, ct);
        var tokenResult = _jwtSvc.GenerateTokens(user, permissions);

        var refreshToken = GarmentsERP.Auth.Domain.Entities.RefreshToken.Create(user.Id, tokenResult.RefreshToken,
            request.RememberMe ? AppConstants.JwtRefreshTokenExpiryDays * 4 : AppConstants.JwtRefreshTokenExpiryDays,
            request.DeviceInfo, request.IpAddress);

        await _repo.AddRefreshTokenAsync(refreshToken, ct);
        await _repo.UpdateUserAsync(user);
        await _repo.SaveChangesAsync(ct);

        var roles = await _repo.GetUserRolesAsync(user.Id, ct);
        var menuItems = _menuSvc.BuildMenu(permissions);

        return Result<AuthResponseDto>.Success(new AuthResponseDto(
            user.Id, user.FullName, user.UserName, user.Email,
            tokenResult.AccessToken, tokenResult.RefreshToken, tokenResult.Expiry,
            permissions, menuItems));
    }
}

