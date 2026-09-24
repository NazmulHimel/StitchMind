using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Auth.Domain.Entities;
using GarmentsERP.Auth.Infrastructure.Data;
using GarmentsERP.Shared.Constants;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GarmentsERP.Auth.Infrastructure.Data.Seed;

public static class AuthDataSeeder
{
    public static readonly Guid DefaultCompanyId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    public static async Task SeedAsync(AuthDbContext context, IPasswordService passwordService, ILogger logger)
    {
        try
        {
            if (context.Database.IsSqlServer())
            {
                await context.Database.MigrateAsync();
            }
            else
            {
                await context.Database.EnsureCreatedAsync();
            }

            // 1. Seed Permissions
            var existingPermissions = await context.Permissions.ToListAsync();
            var standardPermissions = GenerateStandardPermissions();

            var newPermissions = new List<Permission>();
            foreach (var perm in standardPermissions)
            {
                if (!existingPermissions.Any(p => p.Module == perm.Module && p.Action == perm.Action))
                {
                    newPermissions.Add(perm);
                }
            }

            if (newPermissions.Count > 0)
            {
                await context.Permissions.AddRangeAsync(newPermissions);
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} new permissions.", newPermissions.Count);
            }

            var allPermissions = await context.Permissions.ToListAsync();

            // 2. Seed System Roles
            var rolesToSeed = new (string Name, string Description)[]
            {
                (AppConstants.Roles.SuperAdmin, "Complete system administrative access"),
                (AppConstants.Roles.Admin, "Factory and operational management access"),
                (AppConstants.Roles.StoreManager, "Warehouse, stock, and inventory management"),
                (AppConstants.Roles.ProductionManager, "Full production and plant floor oversight"),
                (AppConstants.Roles.CuttingSupervisor, "Fabric laying, marker, and cutting supervision"),
                (AppConstants.Roles.SewingSupervisor, "Sewing lines and floor operation supervision"),
                (AppConstants.Roles.QCSupervisor, "Quality control, fabric inspection, and AQL"),
                (AppConstants.Roles.AccountsManager, "General ledger, AP, AR, and financial reporting"),
                (AppConstants.Roles.HRManager, "Employee directory, attendance, and payroll"),
                (AppConstants.Roles.Viewer, "Read-only access across the ERP modules")
            };

            var existingRoles = await context.Roles.Where(r => r.CompanyId == DefaultCompanyId).ToListAsync();
            var seededRoles = new List<Role>();

            foreach (var (name, desc) in rolesToSeed)
            {
                var role = existingRoles.FirstOrDefault(r => r.Name == name);
                if (role is null)
                {
                    role = Role.Create(name, DefaultCompanyId, desc, isSystem: true);
                    await context.Roles.AddAsync(role);
                    seededRoles.Add(role);
                }
            }

            if (seededRoles.Count > 0)
            {
                await context.SaveChangesAsync();
                logger.LogInformation("Seeded {Count} system roles.", seededRoles.Count);
            }

            var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == AppConstants.Roles.SuperAdmin);
            if (superAdminRole is not null)
            {
                var existingRolePerms = await context.RolePermissions.Where(rp => rp.RoleId == superAdminRole.Id).ToListAsync();
                var missingPerms = allPermissions.Where(p => !existingRolePerms.Any(rp => rp.PermissionId == p.Id)).ToList();
                if (missingPerms.Count > 0)
                {
                    await context.RolePermissions.AddRangeAsync(missingPerms.Select(p => new RolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = p.Id
                    }));
                    await context.SaveChangesAsync();
                }
            }

            // 3. Seed Default SuperAdmin User
            var adminUser = await context.Users.FirstOrDefaultAsync(u => u.UserName == "admin");
            if (adminUser is null)
            {
                var passwordHash = passwordService.Hash("Admin@123456");
                adminUser = User.Create(
                    fullName: "System Super Administrator",
                    userName: "admin",
                    email: "admin@garmentserp.local",
                    passwordHash: passwordHash,
                    companyId: DefaultCompanyId,
                    phone: "+8801700000000"
                );

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                if (superAdminRole is not null)
                {
                    await context.UserRoles.AddAsync(new UserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = superAdminRole.Id
                    });
                    await context.SaveChangesAsync();
                }

                logger.LogInformation("Seeded default SuperAdmin user: admin (Password: Admin@123456)");
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while seeding the Auth database.");
        }
    }

    private static List<Permission> GenerateStandardPermissions()
    {
        var modules = new[]
        {
            AppConstants.Modules.Auth,
            AppConstants.Modules.Roles,
            AppConstants.Modules.Company,
            AppConstants.Modules.Suppliers,
            AppConstants.Modules.Procurement,
            AppConstants.Modules.Inventory,
            AppConstants.Modules.Thread,
            AppConstants.Modules.BuyerOrders,
            AppConstants.Modules.Production,
            AppConstants.Modules.Cutting,
            AppConstants.Modules.Sewing,
            AppConstants.Modules.Washing,
            AppConstants.Modules.QC,
            AppConstants.Modules.Dispatch,
            AppConstants.Modules.HR,
            AppConstants.Modules.Attendance,
            AppConstants.Modules.Payroll,
            AppConstants.Modules.Finance,
            AppConstants.Modules.Reports,
            AppConstants.Modules.Dashboard,
            AppConstants.Modules.AuditLog,
            AppConstants.Modules.Notifications,
            AppConstants.Modules.Settings
        };

        var actions = new[]
        {
            (AppConstants.Actions.Create, "Create new records"),
            (AppConstants.Actions.Read, "View and browse records"),
            (AppConstants.Actions.Update, "Modify and update records"),
            (AppConstants.Actions.Delete, "Delete or archive records"),
            (AppConstants.Actions.Export, "Export data to Excel or PDF")
        };

        var list = new List<Permission>();
        foreach (var mod in modules)
        {
            foreach (var (act, actDesc) in actions)
            {
                list.Add(Permission.Create(mod, act, $"{act} {mod} ({actDesc})"));
            }
        }

        return list;
    }
}
