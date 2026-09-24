using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Domain.Entities;
using GarmentsERP.Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace GarmentsERP.Auth.Infrastructure.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly AuthDbContext _context;

    public AuthRepository(AuthDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == id, ct);
    }

    public async Task<User?> GetUserByEmailAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLower();
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == normalized, ct);
    }

    public async Task<User?> GetUserByUserNameAsync(string userName, CancellationToken ct = default)
    {
        var normalized = userName.Trim().ToLower();
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.UserName == normalized, ct);
    }

    public async Task<User?> GetUserByEmailOrUserNameAsync(string emailOrUserName, CancellationToken ct = default)
    {
        var normalized = emailOrUserName.Trim().ToLower();
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == normalized || u.UserName == normalized, ct);
    }

    public async Task<bool> UserNameExistsAsync(string userName, CancellationToken ct = default)
    {
        var normalized = userName.Trim().ToLower();
        return await _context.Users.AnyAsync(u => u.UserName == normalized, ct);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken ct = default)
    {
        var normalized = email.Trim().ToLower();
        return await _context.Users.AnyAsync(u => u.Email == normalized, ct);
    }

    public async Task AddUserAsync(User user, CancellationToken ct = default)
    {
        await _context.Users.AddAsync(user, ct);
    }

    public Task UpdateUserAsync(User user, CancellationToken ct = default)
    {
        _context.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task<RefreshToken?> GetRefreshTokenAsync(string token, CancellationToken ct = default)
    {
        return await _context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token, ct);
    }

    public async Task AddRefreshTokenAsync(RefreshToken token, CancellationToken ct = default)
    {
        await _context.RefreshTokens.AddAsync(token, ct);
    }

    public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken ct = default)
    {
        var activeTokens = await _context.RefreshTokens
            .Where(r => r.UserId == userId && !r.IsRevoked)
            .ToListAsync(ct);

        foreach (var token in activeTokens)
        {
            token.Revoke();
        }
    }

    public async Task<IReadOnlyList<string>> GetUserPermissionsAsync(Guid userId, CancellationToken ct = default)
    {
        var roleIds = await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.RoleId)
            .ToListAsync(ct);

        if (roleIds.Count == 0)
            return [];

        // Check if user has SuperAdmin role
        var hasSuperAdmin = await _context.Roles
            .AnyAsync(r => roleIds.Contains(r.Id) && r.Name == "SuperAdmin", ct);

        if (hasSuperAdmin)
        {
            return await _context.Permissions
                .Select(p => p.Key)
                .ToListAsync(ct);
        }

        var permissions = await _context.RolePermissions
            .Where(rp => roleIds.Contains(rp.RoleId))
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToListAsync(ct);

        return permissions;
    }

    public async Task<IReadOnlyList<Role>> GetUserRolesAsync(Guid userId, CancellationToken ct = default)
    {
        return await _context.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<User>> GetUsersAsync(Guid companyId, int page, int pageSize, string? search, CancellationToken ct = default)
    {
        var query = _context.Users
            .Where(u => u.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || u.UserName.Contains(s) || u.Email.Contains(s));
        }

        return await query
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
    }

    public async Task<int> GetUserCountAsync(Guid companyId, string? search, CancellationToken ct = default)
    {
        var query = _context.Users.Where(u => u.CompanyId == companyId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(u => u.FullName.ToLower().Contains(s) || u.UserName.Contains(s) || u.Email.Contains(s));
        }

        return await query.CountAsync(ct);
    }

    public async Task<IReadOnlyList<Role>> GetRolesAsync(Guid companyId, CancellationToken ct = default)
    {
        return await _context.Roles
            .Where(r => r.CompanyId == companyId || r.IsSystem)
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .OrderBy(r => r.Name)
            .ToListAsync(ct);
    }

    public async Task<Role?> GetRoleByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await _context.Roles
            .Include(r => r.RolePermissions)
                .ThenInclude(rp => rp.Permission)
            .FirstOrDefaultAsync(r => r.Id == id, ct);
    }

    public async Task AddRoleAsync(Role role, CancellationToken ct = default)
    {
        await _context.Roles.AddAsync(role, ct);
    }

    public Task UpdateRoleAsync(Role role, CancellationToken ct = default)
    {
        _context.Roles.Update(role);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<Permission>> GetAllPermissionsAsync(CancellationToken ct = default)
    {
        return await _context.Permissions
            .OrderBy(p => p.Module)
            .ThenBy(p => p.Action)
            .ToListAsync(ct);
    }

    public async Task AssignRolesToUserAsync(Guid userId, IReadOnlyList<Guid> roleIds, CancellationToken ct = default)
    {
        var existing = await _context.UserRoles.Where(ur => ur.UserId == userId).ToListAsync(ct);
        _context.UserRoles.RemoveRange(existing);

        foreach (var roleId in roleIds)
        {
            await _context.UserRoles.AddAsync(new UserRole { UserId = userId, RoleId = roleId }, ct);
        }
    }

    public async Task AssignPermissionsToRoleAsync(Guid roleId, IReadOnlyList<Guid> permissionIds, CancellationToken ct = default)
    {
        var existing = await _context.RolePermissions.Where(rp => rp.RoleId == roleId).ToListAsync(ct);
        _context.RolePermissions.RemoveRange(existing);

        foreach (var permId in permissionIds)
        {
            await _context.RolePermissions.AddAsync(new RolePermission { RoleId = roleId, PermissionId = permId }, ct);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        return await _context.SaveChangesAsync(ct);
    }
}
