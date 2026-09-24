"use client";

import React from "react";
import { Settings, Shield, UserPlus, Key } from "lucide-react";

export default function SettingsPage() {
  const roles = [
    { name: "SuperAdmin", desc: "Unrestricted master system access across companies", users: 1, permissions: "All (23 modules * 5 actions)" },
    { name: "StoreManager", desc: "Warehouse stock management, rolls, trims, raw materials", users: 3, permissions: "Inventory.*, Procurement.Read" },
    { name: "CuttingSupervisor", desc: "Fabric lay planning, marker creation, and bundle tickets", users: 4, permissions: "Cutting.*, Inventory.Read" },
    { name: "SewingSupervisor", desc: "Sewing lines, operator allocation, hourly outputs", users: 8, permissions: "Sewing.*, Cutting.Read" },
    { name: "QCSupervisor", desc: "Fabric 4-point inspection, inline audits, AQL 2.5", users: 5, permissions: "QC.*, Sewing.Read" }
  ];

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: "1.4rem", fontWeight: 800 }}>Role-Based Access Control (RBAC) & Users</h2>
        <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
          Manage system users, define roles, and assign fine-grained permissions per garment module.
        </p>
      </div>

      <div className="card" style={{ padding: 24 }}>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Role Name</th>
                <th>Description</th>
                <th>Active Users</th>
                <th>Permissions Scope</th>
              </tr>
            </thead>
            <tbody>
              {roles.map((ro) => (
                <tr key={ro.name}>
                  <td style={{ fontWeight: 700, color: "var(--text-accent)" }}>{ro.name}</td>
                  <td>{ro.desc}</td>
                  <td style={{ fontWeight: 600 }}>{ro.users} Users</td>
                  <td><span className="badge badge-info">{ro.permissions}</span></td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
