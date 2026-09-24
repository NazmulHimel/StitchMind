using GarmentsERP.Auth.Application.DTOs;

namespace GarmentsERP.Auth.Application.Interfaces;

public interface IMenuBuilderService
{
    IReadOnlyList<MenuItemDto> BuildMenu(IEnumerable<string> permissions);
}
