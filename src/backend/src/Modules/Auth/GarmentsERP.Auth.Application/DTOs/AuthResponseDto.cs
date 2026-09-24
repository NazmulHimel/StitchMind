namespace GarmentsERP.Auth.Application.DTOs;
public record AuthResponseDto(
    Guid UserId,
    string FullName,
    string UserName,
    string Email,
    string AccessToken,
    string RefreshToken,
    DateTime AccessTokenExpiry,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<MenuItemDto> MenuItems);
