using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Domain.Entities;
using GarmentsERP.Shared.Common;
using MediatR;
namespace GarmentsERP.Auth.Application.Commands.RegisterUser;
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<Guid>>
{
    private readonly IAuthRepository _repo;
    private readonly IPasswordService _passwordSvc;
    public RegisterUserCommandHandler(IAuthRepository repo, IPasswordService passwordSvc)
        => (_repo, _passwordSvc) = (repo, passwordSvc);
    public async Task<Result<Guid>> Handle(RegisterUserCommand request, CancellationToken ct)
    {
        if (await _repo.UserNameExistsAsync(request.UserName, ct))
            return Result<Guid>.Failure("Username already taken.");
        if (await _repo.EmailExistsAsync(request.Email, ct))
            return Result<Guid>.Failure("Email already registered.");
        if (!_passwordSvc.MeetsComplexity(request.Password))
            return Result<Guid>.Failure("Password does not meet complexity requirements.");

        var hash = _passwordSvc.Hash(request.Password);
        var user = User.Create(request.FullName, request.UserName, request.Email,
            hash, request.CompanyId, request.Phone);
        user.CreatedBy = request.CreatedBy;

        await _repo.AddUserAsync(user, ct);
        await _repo.AssignRolesToUserAsync(user.Id, request.RoleIds, ct);
        await _repo.SaveChangesAsync(ct);
        return Result<Guid>.Success(user.Id);
    }
}
