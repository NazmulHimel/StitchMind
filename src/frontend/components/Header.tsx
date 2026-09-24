"use client";

import React from "react";
import { usePathname } from "next/navigation";
import { useAuth } from "@/lib/auth-context";
import { Menu, Search, Bell, Shield, Building, Sparkles } from "lucide-react";

interface HeaderProps {
  onToggleSidebar: () => void;
}

export const Header: React.FC<HeaderProps> = ({ onToggleSidebar }) => {
  const pathname = usePathname();
  const { user } = useAuth();

  const getPageTitle = (path: string) => {
    if (path.includes("/procurement")) return "Material Procurement & PO";
    if (path.includes("/thread")) return "Thread Production & Spinning";
    if (path.includes("/inventory")) return "Stock & Warehouse Management";
    if (path.includes("/cutting")) return "Cutting Department & Marker Planning";
    if (path.includes("/sewing")) return "Sewing Floor & Real-Time WIP";
    if (path.includes("/qc")) return "Quality Control (4-Point & AQL)";
    if (path.includes("/finance")) return "Accounting & Financial Ledger";
    if (path.includes("/hr")) return "Human Resources & Payroll";
    if (path.includes("/reports")) return "QuestPDF Enterprise Reports";
    if (path.includes("/settings")) return "System Administration & RBAC";
    return "Enterprise Manufacturing Dashboard";
  };

  return (
    <header className="topbar">
      <div className="topbar-left">
        <button className="menu-toggle-btn" onClick={onToggleSidebar} aria-label="Toggle Navigation">
          <Menu size={20} />
        </button>

        <div>
          <h1 style={{ fontSize: "1.05rem", fontWeight: 700, letterSpacing: "-0.01em" }}>
            {getPageTitle(pathname)}
          </h1>
          <div style={{ fontSize: "0.72rem", color: "var(--text-muted)", display: "flex", alignItems: "center", gap: 6 }}>
            <span>StitchMind</span>
            <span>/</span>
            <span style={{ color: "var(--text-accent)" }}>{pathname.replace("/", "") || "dashboard"}</span>
          </div>
        </div>
      </div>

      <div className="search-box">
        <Search size={16} />
        <input
          type="text"
          className="search-input"
          placeholder="Search Style, PO, Bundle, Roll, or Order..."
        />
      </div>

      <div className="topbar-right">
        {/* Plant Connection Status */}
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: 6,
            padding: "4px 10px",
            background: "rgba(16, 185, 129, 0.12)",
            border: "1px solid rgba(16, 185, 129, 0.3)",
            borderRadius: "var(--radius-sm)",
            fontSize: "0.75rem",
            color: "var(--success)",
            fontWeight: 600
          }}
        >
          <span style={{ width: 6, height: 6, borderRadius: "50%", background: "var(--success)", display: "inline-block" }} />
          <span>Plant Floor: Online</span>
        </div>

        {/* Company Badge */}
        <div
          style={{
            display: "flex",
            alignItems: "center",
            gap: 6,
            padding: "4px 10px",
            background: "var(--bg-surface-elevated)",
            border: "1px solid var(--border-subtle)",
            borderRadius: "var(--radius-sm)",
            fontSize: "0.75rem",
            color: "var(--text-secondary)"
          }}
        >
          <Building size={14} style={{ color: "var(--primary)" }} />
          <span>Apex Textiles Ltd.</span>
        </div>

        {/* Notification Bell */}
        <button className="topbar-btn" title="System Alerts">
          <Bell size={18} />
          <span className="badge-dot" />
        </button>
      </div>
    </header>
  );
};
