"use client";

import React from "react";
import Link from "next/link";
import { useAuth } from "@/lib/auth-context";
import {
  Activity, Scissors, ShoppingCart, ShieldCheck, DollarSign,
  Users, TrendingUp, TrendingDown, Clock, ArrowUpRight,
  Sparkles, CheckCircle2, Download, Plus, Layers, Warehouse,
  Package, AlertCircle
} from "lucide-react";

export default function DashboardPage() {
  const { user } = useAuth();

  const kpis = [
    {
      title: "Today's Garment Output",
      value: "6,170 Pcs",
      meta: "+12.4% vs Daily Target",
      isUp: true,
      icon: Activity,
      colorClass: "blue"
    },
    {
      title: "Real-Time Factory OEE",
      value: "87.4%",
      meta: "+2.1% across 6 lines",
      isUp: true,
      icon: Scissors,
      colorClass: "emerald"
    },
    {
      title: "AQL Defect Rate (DHU)",
      value: "1.8%",
      meta: "Within 2.5% AQL Limit",
      isUp: true,
      icon: ShieldCheck,
      colorClass: "purple"
    },
    {
      title: "Active Purchase Orders",
      value: "18 Orders",
      meta: "4 pending receipt",
      isUp: false,
      icon: ShoppingCart,
      colorClass: "amber"
    },
    {
      title: "Raw Material Inventory",
      value: "$142,850.00",
      meta: "48,200 Yds Fabric in Stock",
      isUp: true,
      icon: Warehouse,
      colorClass: "emerald"
    },
    {
      title: "Floor Attendance",
      value: "648 / 665",
      meta: "97.4% shift present",
      isUp: true,
      icon: Users,
      colorClass: "blue"
    }
  ];

  const lines = [
    { line: "Line 01", style: "ST-882 Polo Shirt (Pique)", target: 1200, actual: 1280, efficiency: "106%", status: "Exceeded" },
    { line: "Line 02", style: "ST-904 Basic Crew Tee", target: 1500, actual: 1470, efficiency: "98%", status: "On Track" },
    { line: "Line 03", style: "HD-112 Fleece Hoodie", target: 800, actual: 820, efficiency: "102%", status: "Exceeded" },
    { line: "Line 04", style: "ST-2026-CHINO-01 (Slim Fit Chino)", target: 960, actual: 1008, efficiency: "105%", status: "Balanced" },
    { line: "Line 05", style: "SW-301 Ladies V-Neck", target: 1400, actual: 1420, efficiency: "101%", status: "Exceeded" },
    { line: "Line 06", style: "JK-701 Windbreaker Jacket", target: 600, actual: 590, efficiency: "98%", status: "On Track" }
  ];

  const activities = [
    { time: "8 mins ago", title: "3-Way Match Verified", desc: "Apex Textile invoice matched with PO-2026-0145 ($86,362.50 USD).", type: "finance" },
    { time: "25 mins ago", title: "Cutting Lay Plan #01 Complete", desc: "Bundles #BND-001 to BND-010 printed for Line 04 (ST-2026-CHINO-01).", type: "cutting" },
    { time: "40 mins ago", title: "Fabric 4-Point Audit Passed", desc: "Lot LOT-TW-2026-01 (368 rolls) passed at 3.85 pts/100 sq.yds. Quarantine released.", type: "qc" },
    { time: "1 hour ago", title: "GRN-2026-0210 Intake", desc: "36,750 yds 98/2 Cotton Spandex Twill placed in Quarantine.", type: "procurement" }
  ];

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
      {/* Welcome Bar */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
        <div>
          <h1 style={{ fontSize: "1.5rem", fontWeight: 700, color: "#0f172a", letterSpacing: "-0.02em" }}>
            Welcome back, {user?.fullName || "Production Manager"}
          </h1>
          <p style={{ fontSize: "0.875rem", color: "#64748b", marginTop: "0.25rem" }}>
            Factory Shift A is currently active. All 6 sewing lines and 2 cutting tables are operational.
          </p>
        </div>

        <div style={{ display: "flex", gap: "0.5rem" }}>
          <Link href="/reports" className="btn btn-outline">
            <Download size={16} />
            <span>Daily PDF Report</span>
          </Link>
          <Link href="/procurement" className="btn btn-primary">
            <Plus size={16} />
            <span>Create Purchase Order</span>
          </Link>
        </div>
      </div>

      {/* KPI Cards Grid (Matches Procurement Card System) */}
      <div className="kpi-grid" style={{ gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))" }}>
        {kpis.map((kpi, idx) => {
          const Icon = kpi.icon;
          return (
            <div key={idx} className="kpi-card">
              <div className="kpi-header">
                <span className="kpi-label">{kpi.title}</span>
                <div className={`kpi-icon-wrap ${kpi.colorClass}`}>
                  <Icon size={16} />
                </div>
              </div>
              <div className="kpi-value">{kpi.value}</div>
              <div className={`kpi-meta ${kpi.isUp ? "trend-up" : ""}`}>
                {kpi.isUp ? <TrendingUp size={12} /> : <TrendingDown size={12} />}
                <span>{kpi.meta}</span>
              </div>
            </div>
          );
        })}
      </div>

      {/* Manufacturing Floor Tables & Activities */}
      <div style={{ display: "grid", gridTemplateColumns: "2fr 1fr", gap: "1.5rem", alignItems: "start" }}>
        {/* Sewing Line Performance Table */}
        <div className="card">
          <div className="card-header">
            <div>
              <h3 className="card-title">Real-Time Sewing Line Output</h3>
              <p className="card-desc">Target vs Actual production count per line</p>
            </div>
            <Link href="/sewing" className="btn btn-outline btn-sm">
              <span>View All Lines</span>
              <ArrowUpRight size={14} />
            </Link>
          </div>

          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Line</th>
                  <th>Style & Article</th>
                  <th>Target</th>
                  <th>Actual</th>
                  <th>Efficiency</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {lines.map((l, idx) => (
                  <tr key={idx}>
                    <td style={{ fontWeight: 600, color: "#2563eb" }}>{l.line}</td>
                    <td style={{ fontWeight: 500 }}>{l.style}</td>
                    <td>{l.target.toLocaleString()} pcs</td>
                    <td style={{ fontWeight: 600 }}>{l.actual.toLocaleString()} pcs</td>
                    <td style={{ fontWeight: 700, color: "#0f172a" }}>{l.efficiency}</td>
                    <td>
                      <span className={`badge ${
                        l.status === "Exceeded" ? "badge-success" :
                        l.status === "On Track" ? "badge-info" : "badge-warning"
                      }`}>
                        {l.status}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>

        {/* Live Plant Activity Feed */}
        <div className="card">
          <div className="card-header">
            <div>
              <h3 className="card-title">Factory Events & Log</h3>
              <p className="card-desc">Audit trail across active departments</p>
            </div>
          </div>

          <div className="card-body" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            {activities.map((act, idx) => (
              <div
                key={idx}
                style={{
                  display: "flex",
                  gap: "0.75rem",
                  paddingBottom: idx === activities.length - 1 ? 0 : "0.875rem",
                  borderBottom: idx === activities.length - 1 ? "none" : "1px solid #e2e8f0"
                }}
              >
                <div
                  style={{
                    width: 32,
                    height: 32,
                    borderRadius: "6px",
                    background: "#eff6ff",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    color: "#2563eb",
                    flexShrink: 0
                  }}
                >
                  <Clock size={16} />
                </div>
                <div>
                  <span style={{ fontSize: "0.8125rem", fontWeight: 600, color: "#0f172a" }}>
                    {act.title}
                  </span>
                  <p style={{ fontSize: "0.75rem", color: "#64748b", marginTop: "0.125rem" }}>
                    {act.desc}
                  </p>
                  <span style={{ fontSize: "0.6875rem", color: "#94a3b8", marginTop: "0.25rem", display: "inline-block" }}>
                    {act.time}
                  </span>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
