"use client";

import React, { useState } from "react";
import Link from "next/link";
import { usePathname } from "next/navigation";
import { useAuth, DEMO_ROLES } from "@/lib/auth-context";
import {
  ChevronDown, ChevronRight, LayoutDashboard, ShoppingCart, Disc,
  Warehouse, Scissors, Activity, ShieldCheck, DollarSign, Users,
  FileBarChart, Settings, FileText, Truck, CheckSquare, Layers,
  Droplet, Package, Box, Tag, Archive, Grid, ListOrdered, Ticket,
  GitCommit, Clock, TrendingUp, Search, CheckCircle, Award, BookOpen,
  ArrowDownCircle, FileSpreadsheet, UserCheck, Fingerprint, CreditCard,
  UserPlus, Key, LogOut, Sparkles, Building2, Landmark, Receipt, Scale, X
} from "lucide-react";

interface SidebarProps {
  isOpen: boolean;
  onClose: () => void;
}

const ICON_MAP: Record<string, any> = {
  LayoutDashboard, ShoppingCart, Disc, Warehouse, Scissors, Activity,
  ShieldCheck, DollarSign, Users, FileBarChart, Settings, FileText,
  Truck, CheckSquare, Layers, Droplet, Package, Box, Tag, Archive,
  Grid, ListOrdered, Ticket, GitCommit, Clock, TrendingUp, Search,
  CheckCircle, Award, BookOpen, ArrowDownCircle, FileSpreadsheet,
  UserCheck, Fingerprint, CreditCard, UserPlus, Key, Building2, Landmark, Receipt, Scale
};

export const Sidebar: React.FC<SidebarProps> = ({ isOpen, onClose }) => {
  const pathname = usePathname();
  const { user, menuItems, logout, switchDemoRole } = useAuth();
  const [expandedModules, setExpandedModules] = useState<Record<string, boolean>>({
    "Material Procurement": true,
    "Cutting Department": false,
    "Sewing Floor": false,
    "Stock & Warehouse": false,
    "Quality Control (QC)": false,
    "Thread Production": false
  });
  const [showRoleSwitcher, setShowRoleSwitcher] = useState(false);

  const toggleExpand = (module: string) => {
    setExpandedModules(prev => ({ ...prev, [module]: !prev[module] }));
  };

  const renderIcon = (iconName: string, size = 18) => {
    const IconComponent = ICON_MAP[iconName] || LayoutDashboard;
    return <IconComponent size={size} className="nav-icon" />;
  };

  return (
    <>
      {/* Mobile Backdrop */}
      {isOpen && (
        <div
          onClick={onClose}
          style={{
            position: "fixed",
            inset: 0,
            background: "rgba(15, 23, 42, 0.5)",
            backdropFilter: "blur(4px)",
            zIndex: 45
          }}
        />
      )}

      <aside className={`sidebar ${isOpen ? "open" : ""}`}>
        {/* Brand Header */}
        <div className="sidebar-header" style={{ justifyContent: "space-between" }}>
          <div style={{ display: "flex", alignItems: "center", gap: 12 }}>
            <div className="sidebar-brand-icon">
              <Sparkles size={18} />
            </div>
            <div className="sidebar-brand-text">
              <span>Stitch<span className="gradient-text">Mind</span></span>
              <span className="sub">Enterprise Apparel OS</span>
            </div>
          </div>
          <button
            onClick={onClose}
            className="menu-toggle-btn"
            style={{ display: isOpen ? "flex" : "none" }}
          >
            <X size={18} />
          </button>
        </div>

        {/* Navigation Modules */}
        <div className="sidebar-nav" style={{ paddingTop: "0.75rem" }}>
          {menuItems.map((item) => {
            const hasChildren = item.children && item.children.length > 0;
            const isExpanded = expandedModules[item.label] ?? false;
            const isMainActive = pathname === item.route || (hasChildren && item.children?.some(c => c.route === pathname));

            return (
              <div key={item.label} style={{ marginBottom: 2 }}>
                {hasChildren ? (
                  <button
                    type="button"
                    className={`nav-item ${isMainActive ? "active" : ""}`}
                    onClick={() => toggleExpand(item.label)}
                    style={{
                      width: "100%",
                      backgroundColor: isMainActive ? "#f1f5f9" : "transparent",
                      color: isMainActive ? "#0f172a" : "#334155",
                      border: "none",
                      textAlign: "left",
                      display: "flex",
                      alignItems: "center",
                      justifyContent: "space-between",
                      cursor: "pointer",
                      padding: 0
                    }}
                  >
                    <div className="nav-item-content">
                      {renderIcon(item.icon)}
                      <span style={{ fontWeight: isMainActive ? 600 : 500 }}>{item.label}</span>
                    </div>
                    <div style={{ color: "#64748b", paddingRight: 10, display: "flex", alignItems: "center" }}>
                      {isExpanded ? <ChevronDown size={14} /> : <ChevronRight size={14} />}
                    </div>
                  </button>
                ) : (
                  <Link
                    href={item.route}
                    className={`nav-item ${isMainActive ? "active" : ""}`}
                    onClick={onClose}
                    style={{
                      backgroundColor: isMainActive ? "#f1f5f9" : "transparent",
                      color: isMainActive ? "#0f172a" : "#334155",
                      display: "flex",
                      alignItems: "center"
                    }}
                  >
                    <div className="nav-item-content">
                      {renderIcon(item.icon)}
                      <span style={{ fontWeight: isMainActive ? 600 : 500 }}>{item.label}</span>
                    </div>
                  </Link>
                )}

                {/* Submenu Items */}
                {hasChildren && isExpanded && (
                  <div className="nav-child-list">
                    {item.children.map(child => {
                      const isChildActive = pathname === child.route;
                      return (
                        <Link
                          key={child.label}
                          href={child.route}
                          className={`nav-child-item ${isChildActive ? "active" : ""}`}
                          onClick={onClose}
                          style={{
                            backgroundColor: isChildActive ? "#eff6ff" : "transparent",
                            color: isChildActive ? "#2563eb" : "#475569",
                            fontWeight: isChildActive ? 600 : 400
                          }}
                        >
                          {renderIcon(child.icon, 14)}
                          <span>{child.label}</span>
                        </Link>
                      );
                    })}
                  </div>
                )}
              </div>
            );
          })}
        </div>

        {/* Demo Fast Role Switcher for instant permission testing */}
        <div style={{ padding: "8px 12px", borderTop: "1px solid #e2e8f0", background: "#f8fafc" }}>
          <button
            onClick={() => setShowRoleSwitcher(!showRoleSwitcher)}
            style={{
              width: "100%",
              padding: "6px 10px",
              background: "#ffffff",
              border: "1px solid #e2e8f0",
              borderRadius: "6px",
              color: "#0f172a",
              fontSize: "0.75rem",
              fontWeight: 600,
              cursor: "pointer",
              display: "flex",
              alignItems: "center",
              justifyContent: "space-between"
            }}
          >
            <span>Role Switcher (Demo)</span>
            <ChevronDown size={12} style={{ transform: showRoleSwitcher ? "rotate(180deg)" : "none", transition: "0.2s" }} />
          </button>

          {showRoleSwitcher && (
            <div style={{ marginTop: 6, display: "flex", flexDirection: "column", gap: 3, maxHeight: 160, overflowY: "auto" }}>
              {Object.keys(DEMO_ROLES).map(rk => (
                <button
                  key={rk}
                  onClick={() => {
                    switchDemoRole(rk);
                    setShowRoleSwitcher(false);
                  }}
                  style={{
                    padding: "6px 8px",
                    background: user?.roles.includes(rk) ? "#0f172a" : "#ffffff",
                    color: user?.roles.includes(rk) ? "#ffffff" : "#334155",
                    border: "1px solid #e2e8f0",
                    borderRadius: 4,
                    fontSize: "0.75rem",
                    textAlign: "left",
                    cursor: "pointer",
                    fontWeight: user?.roles.includes(rk) ? 600 : 400
                  }}
                >
                  {rk}
                </button>
              ))}
            </div>
          )}
        </div>

        {/* User Card & Logout */}
        <div className="sidebar-footer">
          <div className="user-mini-card">
            <div className="avatar">
              {user?.fullName?.charAt(0) || "U"}
            </div>
            <div className="user-info">
              <div className="user-name">{user?.fullName || "Guest User"}</div>
              <span className="user-role-badge">
                {user?.roles?.[0] || "SuperAdmin"}
              </span>
            </div>
            <button
              onClick={logout}
              title="Logout"
              style={{
                background: "none",
                border: "none",
                color: "#64748b",
                cursor: "pointer",
                padding: 6,
                borderRadius: 4,
                display: "flex",
                alignItems: "center",
                justifyContent: "center"
              }}
            >
              <LogOut size={16} />
            </button>
          </div>
        </div>
      </aside>
    </>
  );
};
