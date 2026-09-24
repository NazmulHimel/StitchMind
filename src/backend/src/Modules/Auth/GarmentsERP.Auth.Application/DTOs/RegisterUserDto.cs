namespace GarmentsERP.Auth.Application.DTOs;
public record RegisterUserDto(
    string FullName, string UserName, string Email,
    string Password, string ConfirmPassword,
    string? Phone, Guid CompanyId, IReadOnlyList<Guid> RoleIds);
