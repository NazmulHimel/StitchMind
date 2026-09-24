using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Auth.Domain.Entities;

public class Permission : BaseEntity
{
    public string Module { get; private set; } = string.Empty;
    public string Action { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;

    // Computed key e.g. "Procurement.Create"
    public string Key => $"{Module}.{Action}";

    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Permission() { }

    public static Permission Create(string module, string action, string displayName)
        => new() { Module = module.Trim(), Action = action.Trim(), DisplayName = displayName.Trim() };
}
