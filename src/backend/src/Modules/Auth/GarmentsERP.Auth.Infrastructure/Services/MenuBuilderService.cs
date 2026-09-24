using GarmentsERP.Auth.Application.DTOs;
using GarmentsERP.Auth.Application.Interfaces;
using GarmentsERP.Shared.Constants;

namespace GarmentsERP.Auth.Infrastructure.Services;

public class MenuBuilderService : IMenuBuilderService
{
    public IReadOnlyList<MenuItemDto> BuildMenu(IEnumerable<string> permissions)
    {
        var permissionSet = new HashSet<string>(permissions, StringComparer.OrdinalIgnoreCase);
        var isSuperAdmin = permissionSet.Contains($"{AppConstants.Modules.Auth}.SuperAdmin") || permissionSet.Contains("*");

        var allMenuItems = GetStandardMenuItems();
        var userMenu = new List<MenuItemDto>();

        foreach (var item in allMenuItems)
        {
            var filteredChildren = new List<MenuItemDto>();
            foreach (var child in item.Children)
            {
                if (isSuperAdmin || child.Permissions.Count == 0 || child.Permissions.Any(p => permissionSet.Contains(p)))
                {
                    filteredChildren.Add(child);
                }
            }

            // Include module if user has permission for the module itself or any child item
            var hasModulePermission = isSuperAdmin || item.Permissions.Count == 0 || item.Permissions.Any(p => permissionSet.Contains(p));
            if (hasModulePermission || filteredChildren.Count > 0)
            {
                userMenu.Add(item with { Children = filteredChildren });
            }
        }

        return userMenu.OrderBy(m => m.SortOrder).ToList();
    }

    private static List<MenuItemDto> GetStandardMenuItems()
    {
        return
        [
            new MenuItemDto(
                AppConstants.Modules.Dashboard,
                "Dashboard",
                "LayoutDashboard",
                "/dashboard",
                1,
                [],
                []
            ),
            new MenuItemDto(
                AppConstants.Modules.Procurement,
                "Material Procurement",
                "ShoppingCart",
                "/procurement",
                2,
                [$"{AppConstants.Modules.Procurement}.Read"],
                [
                    new(AppConstants.Modules.Procurement, "Purchase Orders", "FileText", "/procurement/orders", 1, [$"{AppConstants.Modules.Procurement}.Read"], []),
                    new(AppConstants.Modules.Suppliers, "Suppliers", "Truck", "/procurement/suppliers", 2, [$"{AppConstants.Modules.Suppliers}.Read"], []),
                    new(AppConstants.Modules.Procurement, "Goods Receipt Note (GRN)", "CheckSquare", "/procurement/grn", 3, [$"{AppConstants.Modules.Procurement}.Read"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.Thread,
                "Thread Production",
                "Disc",
                "/thread",
                3,
                [$"{AppConstants.Modules.Thread}.Read"],
                [
                    new(AppConstants.Modules.Thread, "Yarn Spinning Batch", "Layers", "/thread/spinning", 1, [$"{AppConstants.Modules.Thread}.Read"], []),
                    new(AppConstants.Modules.Thread, "Dyeing Process", "Droplet", "/thread/dyeing", 2, [$"{AppConstants.Modules.Thread}.Read"], []),
                    new(AppConstants.Modules.Thread, "Winding & Packaging", "Package", "/thread/winding", 3, [$"{AppConstants.Modules.Thread}.Read"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.Inventory,
                "Stock Management",
                "Warehouse",
                "/inventory",
                4,
                [$"{AppConstants.Modules.Inventory}.Read"],
                [
                    new(AppConstants.Modules.Inventory, "Fabric Stock", "Box", "/inventory/fabrics", 1, [$"{AppConstants.Modules.Inventory}.Read"], []),
                    new(AppConstants.Modules.Inventory, "Accessories & Trims", "Tag", "/inventory/trims", 2, [$"{AppConstants.Modules.Inventory}.Read"], []),
                    new(AppConstants.Modules.Inventory, "Finished Goods", "Archive", "/inventory/finished-goods", 3, [$"{AppConstants.Modules.Inventory}.Read"], []),
                    new(AppConstants.Modules.Inventory, "Stock Transfer", "Repeat", "/inventory/transfer", 4, [$"{AppConstants.Modules.Inventory}.Update"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.Cutting,
                "Cutting Department",
                "Scissors",
                "/cutting",
                5,
                [$"{AppConstants.Modules.Cutting}.Read"],
                [
                    new(AppConstants.Modules.Cutting, "Fabric Laying & Marker", "Grid", "/cutting/markers", 1, [$"{AppConstants.Modules.Cutting}.Read"], []),
                    new(AppConstants.Modules.Cutting, "Cutting Orders", "ListOrdered", "/cutting/orders", 2, [$"{AppConstants.Modules.Cutting}.Read"], []),
                    new(AppConstants.Modules.Cutting, "Bundle Ticket Generation", "Ticket", "/cutting/bundles", 3, [$"{AppConstants.Modules.Cutting}.Create"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.Sewing,
                "Sewing Department",
                "Activity",
                "/sewing",
                6,
                [$"{AppConstants.Modules.Sewing}.Read"],
                [
                    new(AppConstants.Modules.Sewing, "Line Allocation", "GitCommit", "/sewing/lines", 1, [$"{AppConstants.Modules.Sewing}.Read"], []),
                    new(AppConstants.Modules.Sewing, "Hourly Production", "Clock", "/sewing/hourly-output", 2, [$"{AppConstants.Modules.Sewing}.Read"], []),
                    new(AppConstants.Modules.Sewing, "WIP Tracking", "TrendingUp", "/sewing/wip", 3, [$"{AppConstants.Modules.Sewing}.Read"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.QC,
                "Quality Control (QC)",
                "ShieldCheck",
                "/qc",
                7,
                [$"{AppConstants.Modules.QC}.Read"],
                [
                    new(AppConstants.Modules.QC, "Fabric Inspection (4-Point)", "Search", "/qc/fabric-inspection", 1, [$"{AppConstants.Modules.QC}.Read"], []),
                    new(AppConstants.Modules.QC, "In-Line Sewing QC", "CheckCircle", "/qc/inline", 2, [$"{AppConstants.Modules.QC}.Read"], []),
                    new(AppConstants.Modules.QC, "End-Line & AQL Inspection", "Award", "/qc/aql-inspection", 3, [$"{AppConstants.Modules.QC}.Read"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.Finance,
                "Accounting & Finance",
                "DollarSign",
                "/finance",
                8,
                [$"{AppConstants.Modules.Finance}.Read"],
                [
                    new(AppConstants.Modules.Finance, "Chart of Accounts", "BookOpen", "/finance/coa", 1, [$"{AppConstants.Modules.Finance}.Read"], []),
                    new(AppConstants.Modules.Finance, "Accounts Payable (AP)", "ArrowDownCircle", "/finance/ap", 2, [$"{AppConstants.Modules.Finance}.Read"], []),
                    new(AppConstants.Modules.Finance, "Accounts Receivable (AR)", "ArrowUpCircle", "/finance/ar", 3, [$"{AppConstants.Modules.Finance}.Read"], []),
                    new(AppConstants.Modules.Finance, "General Ledger & Journals", "FileSpreadsheet", "/finance/gl", 4, [$"{AppConstants.Modules.Finance}.Read"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.HR,
                "HR & Payroll",
                "Users",
                "/hr",
                9,
                [$"{AppConstants.Modules.HR}.Read"],
                [
                    new(AppConstants.Modules.HR, "Employee Directory", "UserCheck", "/hr/employees", 1, [$"{AppConstants.Modules.HR}.Read"], []),
                    new(AppConstants.Modules.Attendance, "Biometric Attendance", "Fingerprint", "/hr/attendance", 2, [$"{AppConstants.Modules.Attendance}.Read"], []),
                    new(AppConstants.Modules.Payroll, "Monthly Salary & Wages", "CreditCard", "/hr/payroll", 3, [$"{AppConstants.Modules.Payroll}.Read"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.Reports,
                "Reports & Analytics",
                "FileBarChart",
                "/reports",
                10,
                [$"{AppConstants.Modules.Reports}.Read"],
                [
                    new(AppConstants.Modules.Reports, "Daily Production Report", "FileText", "/reports/daily-production", 1, [$"{AppConstants.Modules.Reports}.Read"], []),
                    new(AppConstants.Modules.Reports, "Material Consumption Report", "PieChart", "/reports/consumption", 2, [$"{AppConstants.Modules.Reports}.Read"], []),
                    new(AppConstants.Modules.Reports, "Financial Profit & Loss", "DollarSign", "/reports/pnl", 3, [$"{AppConstants.Modules.Reports}.Read"], [])
                ]
            ),
            new MenuItemDto(
                AppConstants.Modules.Settings,
                "System & Security",
                "Settings",
                "/settings",
                11,
                [$"{AppConstants.Modules.Auth}.Read", $"{AppConstants.Modules.Roles}.Read"],
                [
                    new(AppConstants.Modules.Auth, "User Management", "UserPlus", "/settings/users", 1, [$"{AppConstants.Modules.Auth}.Read"], []),
                    new(AppConstants.Modules.Roles, "Role & Permissions", "Key", "/settings/roles", 2, [$"{AppConstants.Modules.Roles}.Read"], []),
                    new(AppConstants.Modules.Company, "Company Profile", "Building2", "/settings/company", 3, [$"{AppConstants.Modules.Company}.Read"], []),
                    new(AppConstants.Modules.AuditLog, "Audit Trails", "History", "/settings/audit", 4, [$"{AppConstants.Modules.AuditLog}.Read"], [])
                ]
            )
        ];
    }
}
