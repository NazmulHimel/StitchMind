using GarmentsERP.Auth.Domain.Entities;

namespace GarmentsERP.Auth.Application.Interfaces;

public interface IAuthRepository
{
    Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default);
    Task<User?> GetUserByUserNameAsync(string userName, CancellationToken ct = default);
    Task<User?> GetUserByEmailOrUserNameAsync(string emailOrUserName, CancellationToken ct = default);
    Task<bool> UserNameExistsAsync(string userName, CancellationToken ct = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken ct = default);
    Task AddUserAsync(User user, CancellationToken ct = default);
    Task UpdateUserAsync(User user, CancellationToken ct = default);

    Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default);
    Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default);
    Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default);

    Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetUsersAsync(Guid companyId, int page, int pageSize, string? search, CancellationToken ct = default);
    Task<int> GetUserCountAsync(Guid companyId, string? search, CancellationToken ct = default);

    Task<IReadOnlyList<Role>> GetRolesAsync(Guid companyId, CancellationToken ct = default);
    Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken ct = default);
    Task AddRoleAsync(Role role, CancellationToken ct = default);
    Task UpdateRoleAsync(Role role, CancellationToken ct = default);

    Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default);
    Task AssignRolesToUserAsync(Guid userId, IReadOnlyList<Guid> roleIds, CancellationToken ct = default);
    Task AssignPermissionsToRoleAsync(Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken ct = default);

    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
