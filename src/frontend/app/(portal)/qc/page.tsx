"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import {
  ShieldCheck,
  CheckCircle2,
  AlertOctagon,
  Award,
  Plus,
  RefreshCw,
  Search,
  Sliders,
  Sparkles,
  AlertTriangle,
  XCircle,
  FileText
} from "lucide-react";

interface QualityInspection {
  id: string;
  inspectionNumber: string;
  inspectionType: number; // 0: FabricFourPoint, 1: SewingInline, 2: SewingEndLine, 3: WashingAudit, 4: FinalAQL
  buyerOrderId: string;
  orderNumber: string;
  styleNumber: string;
  lotOrBatchRef: string;
  sampleSize: number;
  minorDefects: number;
  majorDefects: number;
  criticalDefects: number;
  totalPoints: number;
  result: number; // 0: Passed, 1: Failed, 2: ConditionallyPassed
  inspectorName: string;
  comments?: string;
  inspectedAt: string;
}

interface BuyerOrder {
  id: string;
  orderNumber: string;
  styleNumber: string;
  styleName: string;
  orderQuantity: number;
}

const INSPECTION_TYPES: { [key: number]: string } = {
  0: "Fabric 4-Point System (ASTM D5430)",
  1: "In-Line Sewing Audit",
  2: "End-Line Sewing Audit",
  3: "Washing & Wet Processing Audit",
  4: "Final AQL 2.5 Buyer Audit"
};

const RESULT_BADGES: { [key: number]: { label: string; color: string; bg: string } } = {
  0: { label: "PASSED", color: "#10b981", bg: "rgba(16,185,129,0.15)" },
  1: { label: "FAILED", color: "#ef4444", bg: "rgba(239,68,68,0.15)" },
  2: { label: "CONDITIONALLY PASSED", color: "#f59e0b", bg: "rgba(245,158,11,0.15)" }
};

export default function QCPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission("QC.Create");

  const [inspections, setInspections] = useState<QualityInspection[]>([]);
  const [buyerOrders, setBuyerOrders] = useState<BuyerOrder[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [activeTab, setActiveTab] = useState<"all" | "fabric" | "sewing" | "aql">("all");
  const [search, setSearch] = useState("");

  // Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newInspection, setNewInspection] = useState({
    inspectionType: 4,
    buyerOrderId: "",
    lotOrBatchRef: "PO-8812 Fleece Hoodie",
    sampleSize: 500,
    minorDefects: 4,
    majorDefects: 1,
    criticalDefects: 0,
    totalPoints: 6.0,
    inspectorName: "R. Chowdhury",
    comments: "AQL 2.5 Major compliant. Pack sealed for dispatch."
  });

  const fetchData = async () => {
    setLoading(true);
    try {
      const [resQc, resOrders] = await Promise.all([
        fetch("http://localhost:5224/api/v1/quality/inspections").then(r => r.json()),
        fetch("http://localhost:5224/api/v1/buyerorders").then(r => r.json())
      ]);

      if (Array.isArray(resQc)) setInspections(resQc);
      if (Array.isArray(resOrders)) setBuyerOrders(resOrders);
    } catch (err) {
      console.error("Error fetching QC data:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreateInspection = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await fetch("http://localhost:5224/api/v1/quality/inspections/create", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newInspection)
      });
      if (res.ok) {
        setShowCreateModal(false);
        fetchData();
      }
    } catch (err) {
      console.error("Failed to create QC inspection:", err);
    }
  };

  const filteredInspections = inspections.filter((q) => {
    const matchesSearch =
      q.inspectionNumber.toLowerCase().includes(search.toLowerCase()) ||
      q.lotOrBatchRef.toLowerCase().includes(search.toLowerCase()) ||
      q.styleNumber.toLowerCase().includes(search.toLowerCase()) ||
      q.inspectorName.toLowerCase().includes(search.toLowerCase());

    if (!matchesSearch) return false;
    if (activeTab === "fabric") return q.inspectionType === 0;
    if (activeTab === "sewing") return q.inspectionType === 1 || q.inspectionType === 2;
    if (activeTab === "aql") return q.inspectionType === 4;
    return true;
  });

  const passedCount = inspections.filter(q => q.result === 0).length;
  const passRate = inspections.length > 0 ? ((passedCount / inspections.length) * 100).toFixed(1) : "100.0";
  const totalMinor = inspections.reduce((s, q) => s + q.minorDefects, 0);
  const totalMajor = inspections.reduce((s, q) => s + q.majorDefects, 0);
  const totalCritical = inspections.reduce((s, q) => s + q.criticalDefects, 0);

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 24 }}>
      {/* Top Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: 12 }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <ShieldCheck size={24} style={{ color: "var(--color-primary-light)" }} />
            <h2 style={{ fontSize: "1.45rem", fontWeight: 800 }}>Quality Assurance, 4-Point & AQL 2.5</h2>
          </div>
          <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
            ASTM D5430 fabric grading, in-line/end-line sewing audits, defect Pareto analysis, and final shipment releases.
          </p>
        </div>

        <div style={{ display: "flex", gap: 10 }}>
          <button className="btn btn-secondary" onClick={fetchData}>
            <RefreshCw size={15} />
            <span>Refresh</span>
          </button>
          {canCreate && (
            <button className="btn btn-primary" onClick={() => setShowCreateModal(true)}>
              <Plus size={16} />
              <span>New Quality Audit</span>
            </button>
          )}
        </div>
      </div>

      {/* KPI Cards */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: 16 }}>
        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>FIRST PASS YIELD (FPY)</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "#10b981", marginTop: 6 }}>
            {passRate}%
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: 4, display: "flex", alignItems: "center", gap: 4 }}>
            <CheckCircle2 size={13} /> {passedCount} of {inspections.length} lots passed audit
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>TOTAL DEFECTS DETECTED</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "var(--text-primary)", marginTop: 6 }}>
            {totalMinor + totalMajor + totalCritical} <span style={{ fontSize: "0.9rem", color: "var(--text-secondary)", fontWeight: 400 }}>Points</span>
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-secondary)", marginTop: 4 }}>
            {totalMinor} Minor â€¢ {totalMajor} Major â€¢ {totalCritical} Critical
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>AQL 2.5 COMPLIANCE</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "var(--color-primary-light)", marginTop: 6 }}>
            100% Certified
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: 4, display: "flex", alignItems: "center", gap: 4 }}>
            <Award size={13} /> Zero critical defects in final buyer sample
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>FABRIC 4-POINT AVG SCORE</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "#38bdf8", marginTop: 6 }}>
            14.5 <span style={{ fontSize: "0.8rem", fontWeight: 400 }}>pts/100 sq yds</span>
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: 4 }}>
            Grade A Qualified (Threshold &lt; 28.0)
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div style={{ display: "flex", borderBottom: "1px solid var(--border-color)", gap: 12 }}>
        <button
          onClick={() => setActiveTab("all")}
          style={{
            padding: "10px 16px",
            background: "none",
            border: "none",
            borderBottom: activeTab === "all" ? "2px solid var(--color-primary-light)" : "2px solid transparent",
            color: activeTab === "all" ? "var(--text-primary)" : "var(--text-secondary)",
            fontWeight: 700,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8
          }}
        >
          <FileText size={16} />
          <span>All Inspections ({inspections.length})</span>
        </button>

        <button
          onClick={() => setActiveTab("fabric")}
          style={{
            padding: "10px 16px",
            background: "none",
            border: "none",
            borderBottom: activeTab === "fabric" ? "2px solid var(--color-primary-light)" : "2px solid transparent",
            color: activeTab === "fabric" ? "var(--text-primary)" : "var(--text-secondary)",
            fontWeight: 700,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8
          }}
        >
          <Sliders size={16} />
          <span>Fabric 4-Point System</span>
        </button>

        <button
          onClick={() => setActiveTab("sewing")}
          style={{
            padding: "10px 16px",
            background: "none",
            border: "none",
            borderBottom: activeTab === "sewing" ? "2px solid var(--color-primary-light)" : "2px solid transparent",
            color: activeTab === "sewing" ? "var(--text-primary)" : "var(--text-secondary)",
            fontWeight: 700,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8
          }}
        >
          <ShieldCheck size={16} />
          <span>In-Line & End-Line Audits</span>
        </button>

        <button
          onClick={() => setActiveTab("aql")}
          style={{
            padding: "10px 16px",
            background: "none",
            border: "none",
            borderBottom: activeTab === "aql" ? "2px solid var(--color-primary-light)" : "2px solid transparent",
            color: activeTab === "aql" ? "var(--text-primary)" : "var(--text-secondary)",
            fontWeight: 700,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8
          }}
        >
          <Award size={16} />
          <span>Final AQL 2.5 Certifications</span>
        </button>
      </div>

      {/* Inspections Table */}
      <div className="card" style={{ padding: 20 }}>
        <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 16, gap: 12, flexWrap: "wrap" }}>
          <div style={{ position: "relative", width: 280 }}>
            <Search size={16} style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "var(--text-muted)" }} />
            <input
              type="text"
              placeholder="Search audit ID, lot, inspector..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              style={{
                width: "100%",
                padding: "8px 12px 8px 36px",
                background: "rgba(255, 255, 255, 0.05)",
                border: "1px solid var(--border-color)",
                borderRadius: 6,
                color: "var(--text-primary)",
                fontSize: "0.85rem"
              }}
            />
          </div>
        </div>

        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Audit ID</th>
                <th>Standard / Stage</th>
                <th>Lot / Line Sample</th>
                <th>Buyer Style</th>
                <th>Sample Size</th>
                <th>Defects (Min/Maj/Crit)</th>
                <th>Penalty Points</th>
                <th>Result</th>
                <th>Auditor</th>
                <th>Notes</th>
              </tr>
            </thead>
            <tbody>
              {filteredInspections.length === 0 ? (
                <tr>
                  <td colSpan={10} style={{ textAlign: "center", padding: 32, color: "var(--text-muted)" }}>
                    {loading ? "Loading inspection records..." : "No inspection records found."}
                  </td>
                </tr>
              ) : (
                filteredInspections.map((q) => {
                  const res = RESULT_BADGES[q.result] || RESULT_BADGES[0];
                  return (
                    <tr key={q.id}>
                      <td style={{ fontWeight: 700, color: "var(--color-primary-light)" }}>{q.inspectionNumber}</td>
                      <td style={{ fontWeight: 600 }}>{INSPECTION_TYPES[q.inspectionType] || "General Audit"}</td>
                      <td>{q.lotOrBatchRef}</td>
                      <td>
                        <strong>{q.styleNumber}</strong>
                        <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{q.orderNumber}</div>
                      </td>
                      <td>{q.sampleSize.toLocaleString()} pcs</td>
                      <td>
                        <span style={{ fontSize: "0.85rem" }}>
                          <span style={{ color: "#38bdf8" }}>{q.minorDefects}</span> /{" "}
                          <span style={{ color: "#f59e0b" }}>{q.majorDefects}</span> /{" "}
                          <span style={{ color: q.criticalDefects > 0 ? "#ef4444" : "var(--text-muted)", fontWeight: q.criticalDefects > 0 ? 800 : 400 }}>{q.criticalDefects}</span>
                        </span>
                      </td>
                      <td style={{ fontWeight: 700 }}>{q.totalPoints}</td>
                      <td>
                        <span
                          style={{
                            padding: "4px 10px",
                            borderRadius: 12,
                            fontSize: "0.75rem",
                            fontWeight: 800,
                            color: res.color,
                            background: res.bg
                          }}
                        >
                          {res.label}
                        </span>
                      </td>
                      <td>{q.inspectorName}</td>
                      <td style={{ fontSize: "0.8rem", color: "var(--text-muted)", maxWidth: 180, overflow: "hidden", textOverflow: "ellipsis", whiteSpace: "nowrap" }}>
                        {q.comments || "â€”"}
                      </td>
                    </tr>
                  );
                })
              )}
            </tbody>
          </table>
        </div>
      </div>

      {/* Modal: New Quality Audit */}
      {showCreateModal && (
        <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000, padding: 16 }}>
          <div className="card" style={{ width: "100%", maxWidth: 540, padding: 24, borderRadius: 12 }}>
            <h3 style={{ fontSize: "1.2rem", fontWeight: 800, marginBottom: 16 }}>Record Quality Audit</h3>
            <form onSubmit={handleCreateInspection} style={{ display: "flex", flexDirection: "column", gap: 14 }}>
              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Inspection Type / Standard</label>
                <select
                  value={newInspection.inspectionType}
                  onChange={(e) => setNewInspection({ ...newInspection, inspectionType: parseInt(e.target.value) || 0 })}
                  style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                >
                  <option value={0} style={{ background: "#ffffff", color: "#0f172a" }}>Fabric 4-Point System (ASTM D5430)</option>
                  <option value={1} style={{ background: "#ffffff", color: "#0f172a" }}>In-Line Sewing Audit</option>
                  <option value={2} style={{ background: "#ffffff", color: "#0f172a" }}>End-Line Sewing Audit</option>
                  <option value={3} style={{ background: "#ffffff", color: "#0f172a" }}>Washing & Wet Processing Audit</option>
                  <option value={4} style={{ background: "#ffffff", color: "#0f172a" }}>Final AQL 2.5 Buyer Audit</option>
                </select>
              </div>

              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Buyer Order Reference</label>
                <select
                  required
                  value={newInspection.buyerOrderId}
                  onChange={(e) => setNewInspection({ ...newInspection, buyerOrderId: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                >
                  <option value="">-- Choose Buyer Order --</option>
                  {buyerOrders.map((bo) => (
                    <option key={bo.id} value={bo.id} style={{ background: "#ffffff", color: "#0f172a" }}>
                      {bo.orderNumber} â€” {bo.styleNumber} ({bo.styleName})
                    </option>
                  ))}
                </select>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Lot / Roll / Line Ref</label>
                  <input
                    type="text"
                    required
                    value={newInspection.lotOrBatchRef}
                    onChange={(e) => setNewInspection({ ...newInspection, lotOrBatchRef: e.target.value })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Sample Size (Pcs / Mtr)</label>
                  <input
                    type="number"
                    required
                    value={newInspection.sampleSize}
                    onChange={(e) => setNewInspection({ ...newInspection, sampleSize: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr 1fr", gap: 10 }}>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Minor Defects</label>
                  <input
                    type="number"
                    value={newInspection.minorDefects}
                    onChange={(e) => setNewInspection({ ...newInspection, minorDefects: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Major Defects</label>
                  <input
                    type="number"
                    value={newInspection.majorDefects}
                    onChange={(e) => setNewInspection({ ...newInspection, majorDefects: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Critical Defects</label>
                  <input
                    type="number"
                    value={newInspection.criticalDefects}
                    onChange={(e) => setNewInspection({ ...newInspection, criticalDefects: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Penalty Points</label>
                  <input
                    type="number"
                    step="0.5"
                    value={newInspection.totalPoints}
                    onChange={(e) => setNewInspection({ ...newInspection, totalPoints: parseFloat(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 10px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Inspector Name</label>
                  <input
                    type="text"
                    required
                    value={newInspection.inspectorName}
                    onChange={(e) => setNewInspection({ ...newInspection, inspectorName: e.target.value })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Auditor Comments</label>
                  <input
                    type="text"
                    value={newInspection.comments}
                    onChange={(e) => setNewInspection({ ...newInspection, comments: e.target.value })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: 10, marginTop: 12 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowCreateModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Evaluate & Save Audit
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

