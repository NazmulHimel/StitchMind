using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Queries.GetMenuItems;

public record GetMenuItemsQuery(Guid UserId) : IRequest<Result<IReadOnlyList<MenuItemDto>>>;
