namespace GarmentsERP.Shared.Constants;

public static class AppConstants
{
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    public const int JwtAccessTokenExpiryMinutes = 15;
    public const int JwtRefreshTokenExpiryDays = 7;
    public const int MaxLoginAttempts = 5;
    public const int AccountLockoutMinutes = 30;
    public const int MaxFileSizeMB = 20;

    public static class Roles
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string AccountsManager = "AccountsManager";
        public const string StoreManager = "StoreManager";
        public const string ProductionManager = "ProductionManager";
        public const string CuttingSupervisor = "CuttingSupervisor";
        public const string SewingSupervisor = "SewingSupervisor";
        public const string QCSupervisor = "QCSupervisor";
        public const string HRManager = "HRManager";
        public const string DispatchOfficer = "DispatchOfficer";
        public const string Viewer = "Viewer";
    }

    public static class Modules
    {
        public const string Auth         = "Auth";
        public const string Roles        = "Roles";
        public const string Company      = "Company";
        public const string Suppliers    = "Suppliers";
        public const string Procurement  = "Procurement";
        public const string Inventory    = "Inventory";
        public const string Thread       = "Thread";
        public const string BuyerOrders  = "BuyerOrders";
        public const string Production   = "Production";
        public const string Cutting      = "Cutting";
        public const string Sewing       = "Sewing";
        public const string Washing      = "Washing";
        public const string QC           = "QC";
        public const string Dispatch     = "Dispatch";
        public const string HR           = "HR";
        public const string Attendance   = "Attendance";
        public const string Payroll      = "Payroll";
        public const string Finance      = "Finance";
        public const string Reports      = "Reports";
        public const string Dashboard    = "Dashboard";
        public const string AuditLog     = "AuditLog";
        public const string Notifications= "Notifications";
        public const string Settings     = "Settings";
    }

    public static class Actions
    {
        public const string Create  = "Create";
        public const string Read    = "Read";
        public const string Update  = "Update";
        public const string Delete  = "Delete";
        public const string Approve = "Approve";
        public const string Export  = "Export";

        public static readonly string[] All = [Create, Read, Update, Delete, Approve, Export];
    }

    public static class CustomClaims
    {
        public const string ViewFinancialCosts = "Permission:ViewFinancialCosts";
        public const string Department         = "Department";
        public const string CompanyId          = "CompanyId";
    }
}
