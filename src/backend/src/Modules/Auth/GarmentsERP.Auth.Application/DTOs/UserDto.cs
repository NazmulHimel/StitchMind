namespace GarmentsERP.Auth.Application.DTOs;
public record UserDto(
    Guid Id, string FullName, string UserName, string Email,
    string? Phone, string? AvatarPath, bool IsActive,
    DateTime? LastLoginAt, IReadOnlyList<string> Roles);
