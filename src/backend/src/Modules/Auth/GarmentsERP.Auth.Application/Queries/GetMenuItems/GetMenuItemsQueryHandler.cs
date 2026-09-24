using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Queries.GetMenuItems;

public class GetMenuItemsQueryHandler : IRequestHandler<GetMenuItemsQuery, Result<IReadOnlyList<MenuItemDto>>>
{
    private readonly IAuthRepository _repo;
    private readonly IMenuBuilderService _menuSvc;

    public GetMenuItemsQueryHandler(IAuthRepository repo, IMenuBuilderService menuSvc)
    {
        _repo = repo;
        _menuSvc = menuSvc;
    }

    public async Task<Result<IReadOnlyList<MenuItemDto>>> Handle(GetMenuItemsQuery request, CancellationToken ct)
    {
        var permissions = await _repo.GetUserPermissionsAsync(request.UserId, ct);
        var menu = _menuSvc.BuildMenu(permissions);
        return Result<IReadOnlyList<MenuItemDto>>.Success(menu);
    }
}
