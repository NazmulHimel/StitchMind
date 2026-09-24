using FluentValidation;
namespace GarmentsERP.Auth.Application.Commands.RegisterUser;
public class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MinimumLength(2).MaximumLength(100);
        RuleFor(x => x.UserName).NotEmpty().MinimumLength(3).MaximumLength(50)
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, and hyphens.");
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8)
            .Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
        RuleFor(x => x.RoleIds).NotEmpty().WithMessage("At least one role must be assigned.");
        RuleFor(x => x.CompanyId).NotEmpty();
    }
}
