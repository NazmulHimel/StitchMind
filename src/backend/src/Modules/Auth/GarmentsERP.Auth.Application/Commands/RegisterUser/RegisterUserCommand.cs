using GarmentsERP.Shared.Common;
using MediatR;
namespace GarmentsERP.Auth.Application.Commands.RegisterUser;
public record RegisterUserCommand(string FullName, string UserName, string Email,
    string Password, string? Phone, Guid CompanyId, IReadOnlyList<Guid> RoleIds, Guid CreatedBy)
    : IRequest<Result<Guid>>;
