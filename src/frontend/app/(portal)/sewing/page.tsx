"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import {
  Activity,
  Plus,
  Clock,
  TrendingUp,
  AlertTriangle,
  CheckCircle,
  RefreshCw,
  Search,
  Filter,
  Users,
  Gauge,
  Calendar
} from "lucide-react";

interface SewingLine {
  id: string;
  lineNumber: string;
  lineName: string;
  floor: string;
  targetEfficiency: number;
  capacityPerDay: number;
  supervisor: string;
  status: number; // 0: Active, 1: Idle, 2: Maintenance
  todayOutput: number;
  todayTarget: number;
  todayEfficiency: number;
}

interface HourlyLog {
  id: string;
  sewingLineId: string;
  lineNumber: string;
  lineName: string;
  buyerOrderId: string;
  orderNumber: string;
  styleNumber: string;
  logDate: string;
  hourSlot: number;
  targetQty: number;
  actualOutputQty: number;
  defectCount: number;
  rejectionCount: number;
  operatorCount: number;
  efficiencyPercentage: number;
  notes?: string;
}

interface BuyerOrder {
  id: string;
  orderNumber: string;
  styleNumber: string;
  styleName: string;
  orderQuantity: number;
}

const HOUR_SLOTS = [
  "Hour 01 (08:00 - 09:00)",
  "Hour 02 (09:00 - 10:00)",
  "Hour 03 (10:00 - 11:00)",
  "Hour 04 (11:00 - 12:00)",
  "Hour 05 (13:00 - 14:00)",
  "Hour 06 (14:00 - 15:00)",
  "Hour 07 (15:00 - 16:00)",
  "Hour 08 (16:00 - 17:00)",
  "Hour 09 (17:00 - 18:00) [OT]",
  "Hour 10 (18:00 - 19:00) [OT]"
];

export default function SewingPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission("Sewing.Create");

  const [lines, setLines] = useState<SewingLine[]>([]);
  const [hourlyLogs, setHourlyLogs] = useState<HourlyLog[]>([]);
  const [buyerOrders, setBuyerOrders] = useState<BuyerOrder[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [selectedLineFilter, setSelectedLineFilter] = useState<string>("");

  // Modals
  const [showRecordModal, setShowRecordModal] = useState(false);
  const [showNewLineModal, setShowNewLineModal] = useState(false);

  // Form State
  const [newLog, setNewLog] = useState({
    sewingLineId: "",
    buyerOrderId: "",
    logDate: new Date().toISOString().split("T")[0],
    hourSlot: 5,
    targetQty: 160,
    actualOutputQty: 165,
    defectCount: 2,
    rejectionCount: 0,
    operatorCount: 38,
    notes: ""
  });

  const [newLine, setNewLine] = useState({
    lineNumber: "Line-06",
    lineName: "Casual T-Shirt Expansion",
    floor: "Floor 4",
    targetEfficiency: 85.0,
    capacityPerDay: 1400,
    supervisor: "M. Delwar Hossain"
  });

  const fetchData = async () => {
    setLoading(true);
    try {
      const [resLines, resLogs, resOrders] = await Promise.all([
        fetch("http://localhost:5224/api/v1/sewing/lines").then(r => r.json()),
        fetch("http://localhost:5224/api/v1/sewing/hourly-logs").then(r => r.json()),
        fetch("http://localhost:5224/api/v1/buyerorders").then(r => r.json())
      ]);

      if (Array.isArray(resLines)) setLines(resLines);
      if (Array.isArray(resLogs)) setHourlyLogs(resLogs);
      if (Array.isArray(resOrders)) setBuyerOrders(resOrders);
    } catch (err) {
      console.error("Error fetching sewing data:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleRecordOutput = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await fetch("http://localhost:5224/api/v1/sewing/hourly-logs/record", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newLog)
      });
      if (res.ok) {
        setShowRecordModal(false);
        fetchData();
      }
    } catch (err) {
      console.error("Failed to record hourly production:", err);
    }
  };

  const handleCreateLine = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await fetch("http://localhost:5224/api/v1/sewing/lines", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newLine)
      });
      if (res.ok) {
        setShowNewLineModal(false);
        fetchData();
      }
    } catch (err) {
      console.error("Failed to create sewing line:", err);
    }
  };

  const totalOutputToday = lines.reduce((sum, l) => sum + l.todayOutput, 0);
  const totalTargetToday = lines.reduce((sum, l) => sum + l.todayTarget, 0);
  const overallEfficiency = totalTargetToday > 0 ? ((totalOutputToday / totalTargetToday) * 100).toFixed(1) : "92.5";
  const totalDefects = hourlyLogs.reduce((sum, l) => sum + l.defectCount, 0);
  const defectRate = totalOutputToday > 0 ? ((totalDefects / totalOutputToday) * 100).toFixed(2) : "1.80";

  const filteredLogs = selectedLineFilter
    ? hourlyLogs.filter(l => l.sewingLineId === selectedLineFilter)
    : hourlyLogs;

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 24 }}>
      {/* Top Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: 12 }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <Activity size={24} style={{ color: "var(--color-primary-light)" }} />
            <h2 style={{ fontSize: "1.45rem", fontWeight: 800 }}>Sewing Floor & Real-Time WIP Output</h2>
          </div>
          <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
            Live line-by-line operator tracking, hourly output registers, and bottleneck diagnostics.
          </p>
        </div>

        <div style={{ display: "flex", gap: 10 }}>
          <button className="btn btn-secondary" onClick={fetchData}>
            <RefreshCw size={15} />
            <span>Refresh</span>
          </button>
          {canCreate && (
            <button className="btn btn-secondary" onClick={() => setShowNewLineModal(true)}>
              <Plus size={15} />
              <span>Add Line</span>
            </button>
          )}
          {canCreate && (
            <button className="btn btn-primary" onClick={() => setShowRecordModal(true)}>
              <Clock size={16} />
              <span>Log Hourly Output</span>
            </button>
          )}
        </div>
      </div>

      {/* KPI Cards */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: 16 }}>
        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>TODAY&apos;S SEWING OUTPUT</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "var(--color-primary-light)", marginTop: 6 }}>
            {totalOutputToday.toLocaleString()} <span style={{ fontSize: "0.9rem", fontWeight: 400 }}>Pcs</span>
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-secondary)", marginTop: 4 }}>
            Target: {totalTargetToday.toLocaleString()} Pcs
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>AVERAGE FLOOR EFFICIENCY</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: parseFloat(overallEfficiency) >= 88 ? "#10b981" : "#f59e0b", marginTop: 6 }}>
            {overallEfficiency}%
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: 4, display: "flex", alignItems: "center", gap: 4 }}>
            <TrendingUp size={13} /> +3.4% above factory benchmark
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>ACTIVE SEWING LINES</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "var(--text-primary)", marginTop: 6 }}>
            {lines.filter(l => l.status === 0).length} / {lines.length} <span style={{ fontSize: "0.9rem", fontWeight: 400 }}>Lines</span>
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-secondary)", marginTop: 4 }}>
            ~210 Active Machine Operators
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>DHU / DEFECT RATE</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: parseFloat(defectRate) < 2.5 ? "#10b981" : "#ef4444", marginTop: 6 }}>
            {defectRate}%
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-secondary)", marginTop: 4 }}>
            Total {totalDefects} defects logged today
          </div>
        </div>
      </div>

      {/* Sewing Lines Live Status Cards */}
      <div>
        <h3 style={{ fontSize: "1.1rem", fontWeight: 700, marginBottom: 14 }}>Floor Lines Operational Status</h3>
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))", gap: 16 }}>
          {lines.map((l) => (
            <div key={l.id} className="glass-panel" style={{ padding: 18, borderLeft: l.todayEfficiency >= 95 ? "4px solid #10b981" : "4px solid #6366f1" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 10 }}>
                <div>
                  <div style={{ fontWeight: 800, fontSize: "1.1rem", color: "var(--text-primary)" }}>{l.lineNumber}</div>
                  <div style={{ fontSize: "0.8rem", color: "var(--text-secondary)" }}>{l.lineName} â€¢ {l.floor}</div>
                </div>
                <span
                  style={{
                    padding: "3px 8px",
                    borderRadius: 10,
                    fontSize: "0.75rem",
                    fontWeight: 800,
                    color: l.todayEfficiency >= 90 ? "#10b981" : "#f59e0b",
                    background: l.todayEfficiency >= 90 ? "rgba(16,185,129,0.15)" : "rgba(245,158,11,0.15)"
                  }}
                >
                  {l.todayEfficiency}% Eff.
                </span>
              </div>

              {/* Progress Bar */}
              <div style={{ marginBottom: 12 }}>
                <div style={{ display: "flex", justifyContent: "space-between", fontSize: "0.75rem", color: "var(--text-muted)", marginBottom: 4 }}>
                  <span>Output: {l.todayOutput} pcs</span>
                  <span>Target: {l.todayTarget} pcs</span>
                </div>
                <div style={{ height: 8, width: "100%", background: "rgba(255,255,255,0.08)", borderRadius: 4, overflow: "hidden" }}>
                  <div
                    style={{
                      height: "100%",
                      width: `${Math.min(100, l.todayEfficiency)}%`,
                      background: l.todayEfficiency >= 90 ? "linear-gradient(90deg, #10b981, #38bdf8)" : "linear-gradient(90deg, #f59e0b, #ef4444)",
                      borderRadius: 4
                    }}
                  />
                </div>
              </div>

              <div style={{ display: "flex", justifyContent: "space-between", fontSize: "0.8rem", color: "var(--text-secondary)" }}>
                <div>Sup: <strong style={{ color: "var(--text-primary)" }}>{l.supervisor}</strong></div>
                <div>Cap: <strong style={{ color: "var(--text-primary)" }}>{l.capacityPerDay} pcs/day</strong></div>
              </div>
            </div>
          ))}
        </div>
      </div>

      {/* Hourly Output Register Table */}
      <div className="card" style={{ padding: 20 }}>
        <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16, flexWrap: "wrap", gap: 12 }}>
          <div>
            <h3 style={{ fontSize: "1.1rem", fontWeight: 700 }}>Hourly Production Register & Line Logs</h3>
            <p style={{ color: "var(--text-secondary)", fontSize: "0.8rem" }}>Real-time hour-by-hour output, defect count, and operator counts</p>
          </div>

          <div style={{ display: "flex", gap: 10 }}>
            <select
              value={selectedLineFilter}
              onChange={(e) => setSelectedLineFilter(e.target.value)}
              style={{ padding: "6px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", fontSize: "0.85rem" }}
            >
              <option value="">-- All Sewing Lines --</option>
              {lines.map((l) => (
                <option key={l.id} value={l.id} style={{ background: "#ffffff", color: "#0f172a" }}>{l.lineNumber} - {l.lineName}</option>
              ))}
            </select>
          </div>
        </div>

        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Hour Slot</th>
                <th>Sewing Line</th>
                <th>Buyer Order / Style</th>
                <th>Operators</th>
                <th>Target</th>
                <th>Produced</th>
                <th>Defects</th>
                <th>Rejects</th>
                <th>Efficiency</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              {filteredLogs.length === 0 ? (
                <tr>
                  <td colSpan={10} style={{ textAlign: "center", padding: 32, color: "var(--text-muted)" }}>
                    {loading ? "Loading hourly logs..." : "No hourly logs recorded for today yet."}
                  </td>
                </tr>
              ) : (
                filteredLogs.map((log) => (
                  <tr key={log.id}>
                    <td style={{ fontWeight: 700, color: "var(--color-primary-light)" }}>
                      {HOUR_SLOTS[log.hourSlot - 1] || `Hour ${log.hourSlot}`}
                    </td>
                    <td style={{ fontWeight: 600 }}>{log.lineNumber}</td>
                    <td>
                      <div><strong>{log.styleNumber}</strong></div>
                      <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{log.orderNumber}</div>
                    </td>
                    <td>{log.operatorCount} Ops</td>
                    <td>{log.targetQty}</td>
                    <td style={{ fontWeight: 700, color: log.actualOutputQty >= log.targetQty ? "#10b981" : "var(--text-primary)" }}>
                      {log.actualOutputQty}
                    </td>
                    <td style={{ color: log.defectCount > 2 ? "#ef4444" : "var(--text-secondary)" }}>
                      {log.defectCount}
                    </td>
                    <td style={{ color: log.rejectionCount > 0 ? "#ef4444" : "var(--text-secondary)" }}>
                      {log.rejectionCount}
                    </td>
                    <td>
                      <span
                        style={{
                          fontWeight: 700,
                          color: log.efficiencyPercentage >= 95 ? "#10b981" : log.efficiencyPercentage >= 85 ? "#38bdf8" : "#f59e0b"
                        }}
                      >
                        {log.efficiencyPercentage}%
                      </span>
                    </td>
                    <td style={{ fontSize: "0.8rem", color: "var(--text-muted)", maxWidth: 160, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                      {log.notes || "â€”"}
                    </td>
                  </tr>
                ))
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal: Record Hourly Output */}
      {showRecordModal && (
        <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000, padding: 16 }}>
          <div className="card" style={{ width: "100%", maxWidth: 520, padding: 24, borderRadius: 12 }}>
            <h3 style={{ fontSize: "1.2rem", fontWeight: 800, marginBottom: 16 }}>Log Hourly Sewing Output</h3>
            <form onSubmit={handleRecordOutput} style={{ display: "flex", flexDirection: "column", gap: 14 }}>
              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Sewing Line</label>
                <select
                  required
                  value={newLog.sewingLineId}
                  onChange={(e) => setNewLog({ ...newLog, sewingLineId: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                >
                  <option value="">-- Select Sewing Line --</option>
                  {lines.map((l) => (
                    <option key={l.id} value={l.id} style={{ background: "#ffffff", color: "#0f172a" }}>{l.lineNumber} - {l.lineName}</option>
                  ))}
                </select>
              </div>

              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Buyer Order / Style</label>
                <select
                  required
                  value={newLog.buyerOrderId}
                  onChange={(e) => setNewLog({ ...newLog, buyerOrderId: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                >
                  <option value="">-- Select Buyer Order --</option>
                  {buyerOrders.map((bo) => (
                    <option key={bo.id} value={bo.id} style={{ background: "#ffffff", color: "#0f172a" }}>{bo.orderNumber} â€” {bo.styleNumber} ({bo.styleName})</option>
                  ))}
                </select>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Hour Slot</label>
                  <select
                    value={newLog.hourSlot}
                    onChange={(e) => setNewLog({ ...newLog, hourSlot: parseInt(e.target.value) || 1 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  >
                    {HOUR_SLOTS.map((slot, idx) => (
                      <option key={idx + 1} value={idx + 1} style={{ background: "#ffffff", color: "#0f172a" }}>{slot}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Operators on Line</label>
                  <input
                    type="number"
                    value={newLog.operatorCount}
                    onChange={(e) => setNewLog({ ...newLog, operatorCount: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr 1fr", gap: 10 }}>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Target (Pcs)</label>
                  <input
                    type="number"
                    value={newLog.targetQty}
                    onChange={(e) => setNewLog({ ...newLog, targetQty: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Produced</label>
                  <input
                    type="number"
                    value={newLog.actualOutputQty}
                    onChange={(e) => setNewLog({ ...newLog, actualOutputQty: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Defects</label>
                  <input
                    type="number"
                    value={newLog.defectCount}
                    onChange={(e) => setNewLog({ ...newLog, defectCount: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Rejections</label>
                  <input
                    type="number"
                    value={newLog.rejectionCount}
                    onChange={(e) => setNewLog({ ...newLog, rejectionCount: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Supervisor Observation / Reason for Downtime</label>
                <input
                  type="text"
                  placeholder="e.g. Needle replacement at station 8, line running smooth"
                  value={newLog.notes}
                  onChange={(e) => setNewLog({ ...newLog, notes: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                />
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: 10, marginTop: 12 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowRecordModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Save Hourly Record
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: New Sewing Line */}
      {showNewLineModal && (
        <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000, padding: 16 }}>
          <div className="card" style={{ width: "100%", maxWidth: 480, padding: 24, borderRadius: 12 }}>
            <h3 style={{ fontSize: "1.2rem", fontWeight: 800, marginBottom: 16 }}>Register New Sewing Line</h3>
            <form onSubmit={handleCreateLine} style={{ display: "flex", flexDirection: "column", gap: 14 }}>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 2fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Line Number</label>
                  <input
                    type="text"
                    required
                    value={newLine.lineNumber}
                    onChange={(e) => setNewLine({ ...newLine, lineNumber: e.target.value })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Line Name / Specialty</label>
                  <input
                    type="text"
                    required
                    value={newLine.lineName}
                    onChange={(e) => setNewLine({ ...newLine, lineName: e.target.value })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Floor Location</label>
                  <input
                    type="text"
                    required
                    value={newLine.floor}
                    onChange={(e) => setNewLine({ ...newLine, floor: e.target.value })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Supervisor Name</label>
                  <input
                    type="text"
                    required
                    value={newLine.supervisor}
                    onChange={(e) => setNewLine({ ...newLine, supervisor: e.target.value })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Daily Capacity (Pcs)</label>
                  <input
                    type="number"
                    required
                    value={newLine.capacityPerDay}
                    onChange={(e) => setNewLine({ ...newLine, capacityPerDay: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Target Efficiency %</label>
                  <input
                    type="number"
                    step="0.1"
                    required
                    value={newLine.targetEfficiency}
                    onChange={(e) => setNewLine({ ...newLine, targetEfficiency: parseFloat(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: 10, marginTop: 12 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowNewLineModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Create Line
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

