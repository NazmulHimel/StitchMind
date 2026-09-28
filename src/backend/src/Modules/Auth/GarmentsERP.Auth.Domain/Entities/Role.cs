using GarmentsERP.Shared.Domain;

namespace GarmentsERP.Auth.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsSystem { get; private set; } = false;
    public Guid CompanyId { get; private set; }
    public bool CanViewFinancialCosts { get; private set; } = true;

    public ICollection<UserRole> UserRoles { get; private set; } = [];
    public ICollection<RolePermission> RolePermissions { get; private set; } = [];

    private Role() { }

    public static Role Create(string name, Guid companyId, string? description = null, bool isSystem = false, bool canViewFinancialCosts = true)
        => new() { Name = name.Trim(), Description = description, CompanyId = companyId, IsSystem = isSystem, CanViewFinancialCosts = canViewFinancialCosts };

    public void Update(string name, string? description)
    {
        Name = name.Trim();
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetFinancialCostsVisibility(bool canView)
    {
        CanViewFinancialCosts = canView;
        UpdatedAt = DateTime.UtcNow;
    }
}
