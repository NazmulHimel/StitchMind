namespace GarmentsERP.Shared.Interfaces;

public interface ICurrentUserService
{
    Guid UserId { get; }
    string UserName { get; }
    Guid CompanyId { get; }
    bool IsAuthenticated { get; }
    string? IpAddress { get; }
    bool CanViewFinancialCosts { get; }
    IReadOnlyList<string> Permissions { get; }
    bool HasPermission(string permission);
}
