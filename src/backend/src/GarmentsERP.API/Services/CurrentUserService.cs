using System.Security.Claims;
using GarmentsERP.Shared.Constants;
using GarmentsERP.Shared.Interfaces;

namespace GarmentsERP.API.Services;

public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public Guid UserId
    {
        get
        {
            var idClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(idClaim, out var id) ? id : Guid.Empty;
        }
    }

    public string UserName => User?.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty;

    public Guid CompanyId
    {
        get
        {
            var compClaim = User?.FindFirst("CompanyId")?.Value;
            return Guid.TryParse(compClaim, out var id) ? id : Guid.Empty;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public string? IpAddress => _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();

    public bool CanViewFinancialCosts =>
        HasPermission(AppConstants.CustomClaims.ViewFinancialCosts) ||
        HasPermission("ViewFinancialCosts") ||
        HasPermission("*") ||
        HasPermission($"{AppConstants.Modules.Auth}.SuperAdmin");

    public IReadOnlyList<string> Permissions
    {
        get
        {
            if (User is null) return [];
            return User.FindAll("permission").Select(c => c.Value).ToList();
        }
    }

    public bool HasPermission(string permission)
    {
        if (!IsAuthenticated) return false;
        var perms = Permissions;
        return perms.Contains($"{AppConstants.Modules.Auth}.SuperAdmin") ||
               perms.Contains("*") ||
               perms.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }
}
