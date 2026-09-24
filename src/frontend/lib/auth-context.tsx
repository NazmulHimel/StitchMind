"use client";

import React, { createContext, useContext, useEffect, useState } from "react";
import { AuthResponse, MenuItem, UserProfile } from "./types";
import { apiRequest } from "./api";

interface AuthContextType {
  user: UserProfile | null;
  menuItems: MenuItem[];
  permissions: string[];
  isAuthenticated: boolean;
  isLoading: boolean;
  login: (userNameOrEmail: string, password: string, rememberMe?: boolean) => Promise<{ success: boolean; message?: string }>;
  logout: () => void;
  switchDemoRole: (role: string) => void;
  hasPermission: (permission: string) => boolean;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Standard default menu tree for immediate offline/development preview
const DEFAULT_MENU_TREE: MenuItem[] = [
  { module: "Dashboard", label: "Dashboard", icon: "LayoutDashboard", route: "/dashboard", sortOrder: 1, permissions: [], children: [] },
  {
    module: "Procurement",
    label: "Material Procurement",
    icon: "ShoppingCart",
    route: "/procurement",
    sortOrder: 2,
    permissions: ["Procurement.Read"],
    children: [
      { module: "Procurement", label: "Purchase Orders", icon: "FileText", route: "/procurement", sortOrder: 1, permissions: ["Procurement.Read"], children: [] },
      { module: "Suppliers", label: "Suppliers Directory", icon: "Truck", route: "/procurement/suppliers", sortOrder: 2, permissions: ["Suppliers.Read"], children: [] },
      { module: "Procurement", label: "Goods Receipt (GRN)", icon: "CheckSquare", route: "/procurement/grn", sortOrder: 3, permissions: ["Procurement.Read"], children: [] }
    ]
  },
  {
    module: "Thread",
    label: "Thread Production",
    icon: "Disc",
    route: "/thread",
    sortOrder: 3,
    permissions: ["Thread.Read"],
    children: [
      { module: "Thread", label: "Yarn Spinning Batch", icon: "Layers", route: "/thread", sortOrder: 1, permissions: ["Thread.Read"], children: [] },
      { module: "Thread", label: "Dyeing Process", icon: "Droplet", route: "/thread/dyeing", sortOrder: 2, permissions: ["Thread.Read"], children: [] },
      { module: "Thread", label: "Winding & Packaging", icon: "Package", route: "/thread/winding", sortOrder: 3, permissions: ["Thread.Read"], children: [] }
    ]
  },
  {
    module: "Inventory",
    label: "Stock & Warehouse",
    icon: "Warehouse",
    route: "/inventory",
    sortOrder: 4,
    permissions: ["Inventory.Read"],
    children: [
      { module: "Inventory", label: "Fabric Inventory", icon: "Box", route: "/inventory", sortOrder: 1, permissions: ["Inventory.Read"], children: [] },
      { module: "Inventory", label: "Accessories & Trims", icon: "Tag", route: "/inventory/trims", sortOrder: 2, permissions: ["Inventory.Read"], children: [] },
      { module: "Inventory", label: "Finished Garments", icon: "Archive", route: "/inventory/finished", sortOrder: 3, permissions: ["Inventory.Read"], children: [] }
    ]
  },
  {
    module: "Cutting",
    label: "Cutting Department",
    icon: "Scissors",
    route: "/cutting",
    sortOrder: 5,
    permissions: ["Cutting.Read"],
    children: [
      { module: "Cutting", label: "Marker & Lay Planning", icon: "Grid", route: "/cutting", sortOrder: 1, permissions: ["Cutting.Read"], children: [] },
      { module: "Cutting", label: "Cutting Table Orders", icon: "ListOrdered", route: "/cutting/orders", sortOrder: 2, permissions: ["Cutting.Read"], children: [] },
      { module: "Cutting", label: "Bundle Ticket Tracking", icon: "Ticket", route: "/cutting/bundles", sortOrder: 3, permissions: ["Cutting.Create"], children: [] }
    ]
  },
  {
    module: "Sewing",
    label: "Sewing Floor",
    icon: "Activity",
    route: "/sewing",
    sortOrder: 6,
    permissions: ["Sewing.Read"],
    children: [
      { module: "Sewing", label: "Line Allocation", icon: "GitCommit", route: "/sewing", sortOrder: 1, permissions: ["Sewing.Read"], children: [] },
      { module: "Sewing", label: "Hourly Output Monitor", icon: "Clock", route: "/sewing/hourly", sortOrder: 2, permissions: ["Sewing.Read"], children: [] },
      { module: "Sewing", label: "Line WIP Status", icon: "TrendingUp", route: "/sewing/wip", sortOrder: 3, permissions: ["Sewing.Read"], children: [] }
    ]
  },
  {
    module: "QC",
    label: "Quality Control (QC)",
    icon: "ShieldCheck",
    route: "/qc",
    sortOrder: 7,
    permissions: ["QC.Read"],
    children: [
      { module: "QC", label: "4-Point Fabric Inspection", icon: "Search", route: "/qc", sortOrder: 1, permissions: ["QC.Read"], children: [] },
      { module: "QC", label: "In-Line Sewing Audit", icon: "CheckCircle", route: "/qc/inline", sortOrder: 2, permissions: ["QC.Read"], children: [] },
      { module: "QC", label: "AQL 2.5 Final Inspection", icon: "Award", route: "/qc/aql", sortOrder: 3, permissions: ["QC.Read"], children: [] }
    ]
  },
  {
    module: "Finance",
    label: "Accounting & Finance",
    icon: "Landmark",
    route: "/finance",
    sortOrder: 8,
    permissions: ["Finance.Read"],
    children: [
      { module: "Finance", label: "Financial Overview", icon: "TrendingUp", route: "/finance", sortOrder: 1, permissions: ["Finance.Read"], children: [] },
      { module: "Finance", label: "Chart of Accounts", icon: "BookOpen", route: "/finance", sortOrder: 2, permissions: ["Finance.Read"], children: [] },
      { module: "Finance", label: "Journal Vouchers", icon: "Receipt", route: "/finance", sortOrder: 3, permissions: ["Finance.Read"], children: [] },
      { module: "Finance", label: "Financial Statements", icon: "Scale", route: "/finance", sortOrder: 4, permissions: ["Finance.Read"], children: [] }
    ]
  },
  {
    module: "HR",
    label: "HR & Payroll",
    icon: "Users",
    route: "/hr",
    sortOrder: 9,
    permissions: ["HR.Read"],
    children: [
      { module: "HR", label: "Employee Roster", icon: "UserCheck", route: "/hr", sortOrder: 1, permissions: ["HR.Read"], children: [] },
      { module: "HR", label: "Biometric Attendance", icon: "Fingerprint", route: "/hr/attendance", sortOrder: 2, permissions: ["HR.Read"], children: [] },
      { module: "HR", label: "Payroll Processing", icon: "CreditCard", route: "/hr/payroll", sortOrder: 3, permissions: ["HR.Read"], children: [] }
    ]
  },
  {
    module: "Reports",
    label: "Enterprise Reporting",
    icon: "FileBarChart",
    route: "/reports",
    sortOrder: 10,
    permissions: ["Reports.Read"],
    children: []
  },
  {
    module: "Settings",
    label: "System Security",
    icon: "Settings",
    route: "/settings",
    sortOrder: 11,
    permissions: ["Auth.Read"],
    children: [
      { module: "Auth", label: "User Directory", icon: "UserPlus", route: "/settings", sortOrder: 1, permissions: ["Auth.Read"], children: [] },
      { module: "Roles", label: "Roles & Permissions", icon: "Key", route: "/settings/roles", sortOrder: 2, permissions: ["Roles.Read"], children: [] }
    ]
  }
];

export const DEMO_ROLES: Record<string, { fullName: string; permissions: string[]; roles: string[] }> = {
  SuperAdmin: {
    fullName: "Chief Executive (Super Admin)",
    roles: ["SuperAdmin"],
    permissions: ["Auth.SuperAdmin", "*"]
  },
  CuttingSupervisor: {
    fullName: "Rashidul Islam (Cutting Head)",
    roles: ["CuttingSupervisor"],
    permissions: ["Cutting.Read", "Cutting.Create", "Cutting.Update", "Inventory.Read"]
  },
  SewingSupervisor: {
    fullName: "Farhana Akter (Line 4 Master)",
    roles: ["SewingSupervisor"],
    permissions: ["Sewing.Read", "Sewing.Create", "Sewing.Update", "Cutting.Read"]
  },
  QCSupervisor: {
    fullName: "Mahmud Hasan (Quality Lead)",
    roles: ["QCSupervisor"],
    permissions: ["QC.Read", "QC.Create", "QC.Update", "Sewing.Read", "Cutting.Read"]
  },
  StoreManager: {
    fullName: "Anwar Hossain (Store Keeper)",
    roles: ["StoreManager"],
    permissions: ["Inventory.Read", "Inventory.Create", "Inventory.Update", "Procurement.Read"]
  },
  AccountsManager: {
    fullName: "Tariqul Islam (Chief Accountant)",
    roles: ["AccountsManager"],
    permissions: ["Finance.Read", "Finance.Create", "Finance.Update", "Reports.Read", "Procurement.Read"]
  },
  HRManager: {
    fullName: "Nusrat Jahan (HR & Compliance)",
    roles: ["HRManager"],
    permissions: ["HR.Read", "HR.Create", "HR.Update", "Reports.Read"]
  }
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [menuItems, setMenuItems] = useState<MenuItem[]>([]);
  const [permissions, setPermissions] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  const filterMenusByPermissions = (perms: string[]): MenuItem[] => {
    const isSuper = perms.includes("Auth.SuperAdmin") || perms.includes("*");
    if (isSuper) return DEFAULT_MENU_TREE;

    const set = new Set(perms.map(p => p.toLowerCase()));

    return DEFAULT_MENU_TREE.filter(item => {
      if (item.permissions.length === 0) return true;
      const hasParent = item.permissions.some(p => set.has(p.toLowerCase()));
      const hasChild = item.children.some(c => c.permissions.some(cp => set.has(cp.toLowerCase())));
      return hasParent || hasChild;
    }).map(item => ({
      ...item,
      children: item.children.filter(c => c.permissions.length === 0 || c.permissions.some(cp => set.has(cp.toLowerCase())))
    }));
  };

  useEffect(() => {
    // Restore session on load
    const savedUser = localStorage.getItem("garments_user");
    const savedPerms = localStorage.getItem("garments_perms");

    if (savedUser && savedPerms) {
      try {
        const parsedUser = JSON.parse(savedUser);
        const parsedPerms = JSON.parse(savedPerms);
        setUser(parsedUser);
        setPermissions(parsedPerms);
        setMenuItems(filterMenusByPermissions(parsedPerms));
      } catch {
        localStorage.clear();
      }
    } else {
      // Default to SuperAdmin session for instant evaluation
      switchDemoRole("SuperAdmin");
    }
    setIsLoading(false);
  }, []);

  const login = async (userNameOrEmail: string, password: string, rememberMe = false) => {
    setIsLoading(true);
    const res = await apiRequest<AuthResponse>("/api/v1/auth/login", {
      method: "POST",
      body: JSON.stringify({ userNameOrEmail, password, rememberMe }),
    });

    if (res.success && res.data) {
      const data = res.data;
      const userProfile: UserProfile = {
        id: data.userId,
        fullName: data.fullName,
        userName: data.userName,
        email: data.email,
        isActive: true,
        roles: ["User"],
      };

      localStorage.setItem("garments_access_token", data.accessToken);
      localStorage.setItem("garments_refresh_token", data.refreshToken);
      localStorage.setItem("garments_user", JSON.stringify(userProfile));
      localStorage.setItem("garments_perms", JSON.stringify(data.permissions));

      setUser(userProfile);
      setPermissions(data.permissions);
      setMenuItems(data.menuItems && data.menuItems.length > 0 ? data.menuItems : filterMenusByPermissions(data.permissions));
      setIsLoading(false);
      return { success: true };
    }

    setIsLoading(false);
    return { success: false, message: res.message || res.errors?.[0] || "Invalid credentials" };
  };

  const logout = () => {
    localStorage.removeItem("garments_access_token");
    localStorage.removeItem("garments_refresh_token");
    localStorage.removeItem("garments_user");
    localStorage.removeItem("garments_perms");
    setUser(null);
    setPermissions([]);
    setMenuItems([]);
  };

  const switchDemoRole = (roleKey: string) => {
    const roleData = DEMO_ROLES[roleKey] || DEMO_ROLES.SuperAdmin;
    const demoUser: UserProfile = {
      id: "11111111-2222-3333-4444-555555555555",
      fullName: roleData.fullName,
      userName: roleKey.toLowerCase(),
      email: `${roleKey.toLowerCase()}@garmentserp.local`,
      isActive: true,
      roles: roleData.roles
    };

    localStorage.setItem("garments_user", JSON.stringify(demoUser));
    localStorage.setItem("garments_perms", JSON.stringify(roleData.permissions));

    setUser(demoUser);
    setPermissions(roleData.permissions);
    setMenuItems(filterMenusByPermissions(roleData.permissions));
  };

  const hasPermission = (permission: string) => {
    if (!user) return false;
    if (permissions.includes("Auth.SuperAdmin") || permissions.includes("*")) return true;
    return permissions.some(p => p.toLowerCase() === permission.toLowerCase());
  };

  return (
    <AuthContext.Provider
      value={{
        user,
        menuItems,
        permissions,
        isAuthenticated: !!user,
        isLoading,
        login,
        logout,
        switchDemoRole,
        hasPermission
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider");
  }
  return context;
};
