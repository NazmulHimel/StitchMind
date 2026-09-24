using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Queries.GetUsers;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedList<UserDto>>>
{
    private readonly IAuthRepository _repo;

    public GetUsersQueryHandler(IAuthRepository repo)
    {
        _repo = repo;
    }

    public async Task<Result<PagedList<UserDto>>> Handle(GetUsersQuery request, CancellationToken ct)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize is < 1 or > 100 ? 20 : request.PageSize;

        var users = await _repo.GetUsersAsync(request.CompanyId, page, pageSize, request.Search, ct);
        var totalCount = await _repo.GetUserCountAsync(request.CompanyId, request.Search, ct);

        var dtoList = new List<UserDto>();
        foreach (var u in users)
        {
            var roles = await _repo.GetUserRolesAsync(u.Id, ct);
            dtoList.Add(new UserDto(
                u.Id,
                u.FullName,
                u.UserName,
                u.Email,
                u.Phone,
                u.AvatarPath,
                u.IsActive,
                u.LastLoginAt,
                roles.Select(r => r.Name).ToList()));
        }

        var pagedList = new PagedList<UserDto>(dtoList, page, pageSize, totalCount);
        return Result<PagedList<UserDto>>.Success(pagedList);
    }
}
