using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Queries.GetCurrentUser;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, Result<UserDto>>
{
    private readonly IAuthRepository _repo;

    public GetCurrentUserQueryHandler(IAuthRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<UserDto>> Handle(GetCurrentUserQuery request, CancellationToken ct)
    {
        var user = await _repo.GetUserByIdAsync(request.UserId, ct);
        if (user is null || !user.IsActive)
            return Result<UserDto>.Failure("User not found or inactive.");

        var roles = await _repo.GetUserRolesAsync(user.Id, ct);
        var roleNames = roles.Select(r => r.Name).ToList();

        var dto = new UserDto(
            user.Id,
            user.FullName,
            user.UserName,
            user.Email,
            user.Phone,
            user.AvatarPath,
            user.IsActive,
            user.LastLoginAt,
            roleNames);

        return Result<UserDto>.Success(dto);
    }
}
