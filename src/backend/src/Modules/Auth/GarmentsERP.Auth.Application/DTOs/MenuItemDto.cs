namespace GarmentsERP.Auth.Application.DTOs;
public record MenuItemDto(
    string Module,
    string Label,
    string Icon,
    string Route,
    int SortOrder,
    IReadOnlyList<string> Permissions,
    IReadOnlyList<MenuItemDto> Children);
