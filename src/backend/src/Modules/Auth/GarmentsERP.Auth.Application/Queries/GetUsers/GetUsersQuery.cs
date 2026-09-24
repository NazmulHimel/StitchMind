using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Shared.Common;
using MediatR;

namespace GarmentsERP.Auth.Application.Queries.GetUsers;

public record GetUsersQuery(Guid CompanyId, int Page = 1, int PageSize = 20, string? Search = null)
    : IRequest<Result<PagedList<UserDto>>>;
