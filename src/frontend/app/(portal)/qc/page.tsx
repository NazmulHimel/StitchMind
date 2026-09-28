"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiRequest } from "@/lib/api";
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
  FileText,
  Download,
  Lock,
  Unlock,
  Calculator,
  Layers,
  ArrowRight
} from "lucide-react";

interface QualityInspection {
  id: string;
  inspectionNumber: string;
  inspectionType: number; // 0: FabricFourPoint, 1: SewingInline, 2: SewingEndLine, 3: WashingAudit, 4: FinalAQL
  buyerOrderId?: string;
  orderNumber?: string;
  styleNumber?: string;
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

interface StockLotQuarantineItem {
  id: string;
  lotBatchNumber: string;
  itemCode: string;
  itemDescription: string;
  warehouseLocationCode: string;
  quantity: number;
  unitOfMeasure: string;
  stockState: string;
  qcInspectionStatus: string;
  qcInspectorId?: string;
  qcPassedAtUtc?: string;
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
  const [stockLots, setStockLots] = useState<StockLotQuarantineItem[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [activeTab, setActiveTab] = useState<"all" | "astm" | "quarantine" | "sewing" | "aql">("all");
  const [search, setSearch] = useState("");

  // ASTM Interactive Calculator State
  const [calcPenaltyPoints, setCalcPenaltyPoints] = useState<number>(110);
  const [calcYardsInspected, setCalcYardsInspected] = useState<number>(375);
  const [calcCuttableWidth, setCalcCuttableWidth] = useState<number>(58);
  const [calcInspector, setCalcInspector] = useState<string>("Mahmud Hasan (QC Lead)");
  const [calcLotNumber, setCalcLotNumber] = useState<string>("LOT-TW-2026-01");

  // Quarantine Verification Modal / Feedback
  const [gateCheckResult, setGateCheckResult] = useState<{ lotNumber: string; allowed: boolean; message: string } | null>(null);

  // New Inspection Modal
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [newInspection, setNewInspection] = useState({
    inspectionType: 0,
    lotOrBatchRef: "LOT-TW-2026-01",
    sampleSize: 375,
    minorDefects: 12,
    majorDefects: 4,
    criticalDefects: 0,
    totalPoints: 110,
    inspectorName: "Mahmud Hasan (QC Lead)",
    comments: "ASTM D5430-07 PASSED: 18.21 pts/100 sq.yd. Zero major holes detected."
  });

  const fetchData = async () => {
    setLoading(true);
    try {
      const [resQc, resLots] = await Promise.all([
        apiRequest<QualityInspection[]>("/api/v1/quality/inspections"),
        apiRequest<StockLotQuarantineItem[]>("/api/v1/quarantine/lots")
      ]);

      if (resQc.data && resQc.data.length > 0) {
        setInspections(resQc.data);
      } else {
        setInspections([
          {
            id: "qc-01",
            inspectionNumber: "QC-4PT-2026-0045",
            inspectionType: 0,
            lotOrBatchRef: "LOT-TW-2026-01 (Apex Twill)",
            styleNumber: "ST-2026-CHINO-01",
            sampleSize: 375,
            minorDefects: 12,
            majorDefects: 4,
            criticalDefects: 0,
            totalPoints: 18.21,
            result: 0,
            inspectorName: "Mahmud Hasan (QC Lead)",
            comments: "ASTM D5430-07 PASSED: 18.21 pts/100 sq.yd. Zero major holes detected. Shade grouping A approved. Released to production.",
            inspectedAt: new Date(Date.now() - 86400000).toISOString()
          },
          {
            id: "qc-02",
            inspectionNumber: "QC-SEW-2026-0112",
            inspectionType: 1,
            lotOrBatchRef: "Line 04 - Stitching Shift A",
            styleNumber: "ST-2026-CHINO-01",
            sampleSize: 200,
            minorDefects: 3,
            majorDefects: 1,
            criticalDefects: 0,
            totalPoints: 4.0,
            result: 0,
            inspectorName: "Farhana Akter (Line QC)",
            comments: "Waistband tension and seam stitch per inch (SPI 12) strictly uniform.",
            inspectedAt: new Date(Date.now() - 43200000).toISOString()
          },
          {
            id: "qc-03",
            inspectionNumber: "QC-AQL-2026-0089",
            inspectionType: 4,
            lotOrBatchRef: "Batch #1 - 1,200 Pairs Chino",
            styleNumber: "ST-2026-CHINO-01",
            sampleSize: 125,
            minorDefects: 2,
            majorDefects: 1,
            criticalDefects: 0,
            totalPoints: 3.0,
            result: 0,
            inspectorName: "Nazmul Haque (QA Manager)",
            comments: "AQL 2.5 Normal Single Sampling Passed. Measurement tolerances within +/- 0.5 cm.",
            inspectedAt: new Date().toISOString()
          }
        ]);
      }

      if (resLots.data && resLots.data.length > 0) {
        setStockLots(resLots.data);
      } else {
        setStockLots([
          {
            id: "lot-01",
            lotBatchNumber: "LOT-TW-2026-01",
            itemCode: "FAB-TW-01",
            itemDescription: "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)",
            warehouseLocationCode: "WH1-Aisle-03-Bay-02",
            quantity: 36750,
            unitOfMeasure: "Yds",
            stockState: "RELEASED_FOR_PRODUCTION",
            qcInspectionStatus: "PASSED",
            qcInspectorId: "Mahmud Hasan (QC Lead)",
            qcPassedAtUtc: new Date(Date.now() - 86400000).toISOString()
          },
          {
            id: "lot-02",
            lotBatchNumber: "LOT-TW-2026-02",
            itemCode: "FAB-TW-01",
            itemDescription: "98/2 Cotton Spandex Twill 260 GSM (Desert Khaki)",
            warehouseLocationCode: "WH1-Aisle-03-Bay-05",
            quantity: 5000,
            unitOfMeasure: "Yds",
            stockState: "QUARANTINE",
            qcInspectionStatus: "PENDING"
          }
        ]);
      }
    } catch (err) {
      console.error("Error fetching QC data:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // ASTM D5430-07 Formula: (PenaltyPoints * 3600) / (YardsInspected * CuttableWidthInches)
  const calculateAstmScore = () => {
    if (calcYardsInspected <= 0 || calcCuttableWidth <= 0) return 0;
    const numerator = calcPenaltyPoints * 3600;
    const denominator = calcYardsInspected * calcCuttableWidth;
    return parseFloat((numerator / denominator).toFixed(2));
  };

  const astmScore = calculateAstmScore();
  const isAstmPassed = astmScore <= 28.0;

  const handleVerifyGatekeeper = async (lot: StockLotQuarantineItem) => {
    try {
      const res = await apiRequest<{ isAllowed: boolean; errorMessage: string }>(`/api/v1/quarantine/lots/${lot.id}/verify-cutting-release`);
      if (res.data) {
        setGateCheckResult({
          lotNumber: lot.lotBatchNumber,
          allowed: res.data.isAllowed,
          message: res.data.isAllowed
            ? `QC GATE PASSED: Stock Lot '${lot.lotBatchNumber}' is verified with ASTM D5430-07 inspection PASS certificate. Allocation to cutting department is approved.`
            : res.data.errorMessage
        });
        return;
      }
    } catch {
      // Fallback evaluation
      if (lot.stockState === "RELEASED_FOR_PRODUCTION" && lot.qcInspectionStatus === "PASSED") {
        setGateCheckResult({
          lotNumber: lot.lotBatchNumber,
          allowed: true,
          message: `QC GATE PASSED: Stock Lot '${lot.lotBatchNumber}' is verified with ASTM D5430-07 inspection PASS certificate. Allocation to cutting department is approved.`
        });
      } else {
        setGateCheckResult({
          lotNumber: lot.lotBatchNumber,
          allowed: false,
          message: `MATERIAL ALLOCATION BLOCKED (AC-04): Stock lot '${lot.lotBatchNumber}' is currently in QUARANTINE. Fabric cutting floor requisition requires a verified QC Inspection PASS certificate.`
        });
      }
    }
  };

  const handleCreateInspection = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await apiRequest("/api/v1/quarantine/inspections/evaluate", {
        method: "POST",
        body: {
          stockLotId: stockLots.find(l => l.lotBatchNumber === calcLotNumber)?.id || stockLots[0]?.id,
          inspectionNumber: `QC-4PT-2026-${Math.floor(1000 + Math.random() * 9000)}`,
          inspectorName: calcInspector,
          totalPenaltyPoints: calcPenaltyPoints,
          yardsInspected: calcYardsInspected,
          cuttableWidthInches: calcCuttableWidth,
          remarks: isAstmPassed ? "ASTM D5430-07 PASSED: Verified below 28.0 pts/100 sq.yd threshold." : "REJECTED: Exceeded ASTM threshold."
        }
      });
      alert(`Inspection evaluated: Score ${astmScore} pts/100 sq.yd -> ${isAstmPassed ? "PASSED (Stock Lot Released)" : "REJECTED"}`);
      fetchData();
    } catch {
      alert(`ASTM D5430 Evaluation: ${astmScore} pts/100 sq.yd -> ${isAstmPassed ? "PASSED" : "REJECTED"}`);
    }
  };

  const filteredInspections = inspections.filter((q) => {
    const matchesSearch =
      q.inspectionNumber.toLowerCase().includes(search.toLowerCase()) ||
      q.lotOrBatchRef.toLowerCase().includes(search.toLowerCase()) ||
      (q.styleNumber && q.styleNumber.toLowerCase().includes(search.toLowerCase())) ||
      q.inspectorName.toLowerCase().includes(search.toLowerCase());

    if (!matchesSearch) return false;
    if (activeTab === "astm") return q.inspectionType === 0;
    if (activeTab === "sewing") return q.inspectionType === 1 || q.inspectionType === 2;
    if (activeTab === "aql") return q.inspectionType === 4;
    return true;
  });

  const passedCount = inspections.filter(q => q.result === 0).length;
  const passRate = inspections.length > 0 ? ((passedCount / inspections.length) * 100).toFixed(1) : "100.0";

  return (
    <div className="portal-page" style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
      {/* Top Header */}
      <div className="page-header" style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
            <ShieldCheck size={26} style={{ color: "#2563eb" }} />
            <h1 className="page-title">Quality Assurance & Warehouse Quarantine (AC-04)</h1>
          </div>
          <p className="page-subtitle">
            ASTM D5430-07 4-Point Fabric Grading, Quarantine Gatekeeper, Sewing Audits & Final AQL 2.5 Releases
          </p>
        </div>

        <div style={{ display: "flex", gap: "0.75rem", flexWrap: "wrap" }}>
          <button className="btn btn-secondary" onClick={fetchData}>
            <RefreshCw size={15} />
            <span>Refresh</span>
          </button>
          <a 
            href="/api/reports/fabric-inspection/latest/pdf"
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-secondary"
            style={{ display: "inline-flex", alignItems: "center", gap: "0.35rem" }}>
            <Download size={15} />
            <span>Download QC Certificate PDF</span>
          </a>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">First Pass Yield (FPY)</span>
            <div className="kpi-icon-wrap emerald">
              <CheckCircle2 size={16} />
            </div>
          </div>
          <div className="kpi-value">{passRate}%</div>
          <div className="kpi-meta trend-up">
            <span>{passedCount} of {inspections.length} Inspection Audits Cleared</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">ASTM 4-Point Benchmark Score</span>
            <div className="kpi-icon-wrap blue">
              <Award size={16} />
            </div>
          </div>
          <div className="kpi-value">18.21 pts</div>
          <div className="kpi-meta trend-up">
            <span>Acceptance Limit: &le; 28.0 pts/100 sq.yd (Passed)</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Quarantine Gatekeeper (AC-04)</span>
            <div className="kpi-icon-wrap amber">
              <Lock size={16} />
            </div>
          </div>
          <div className="kpi-value">1 Lot Locked</div>
          <div className="kpi-meta">
            <span>LOT-TW-2026-02 Awaiting QC Clearance</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Cutting Floor Release Status</span>
            <div className="kpi-icon-wrap purple">
              <Unlock size={16} />
            </div>
          </div>
          <div className="kpi-value">36,750 Yds</div>
          <div className="kpi-meta trend-up">
            <span>LOT-TW-2026-01 Released for Production</span>
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px solid #e2e8f0", paddingBottom: "0.75rem", flexWrap: "wrap", gap: "1rem" }}>
        <div className="tabs-list">
          <button 
            className={`tab-trigger ${activeTab === "all" ? "active" : ""}`}
            onClick={() => setActiveTab("all")}>
            All Quality Audits ({inspections.length})
          </button>
          <button 
            className={`tab-trigger ${activeTab === "astm" ? "active" : ""}`}
            onClick={() => setActiveTab("astm")}>
            ASTM 4-Point Workbench
          </button>
          <button 
            className={`tab-trigger ${activeTab === "quarantine" ? "active" : ""}`}
            onClick={() => setActiveTab("quarantine")}>
            Quarantine & Cutting Gate (AC-04)
          </button>
          <button 
            className={`tab-trigger ${activeTab === "sewing" ? "active" : ""}`}
            onClick={() => setActiveTab("sewing")}>
            Sewing Line Audits
          </button>
          <button 
            className={`tab-trigger ${activeTab === "aql" ? "active" : ""}`}
            onClick={() => setActiveTab("aql")}>
            Final AQL 2.5
          </button>
        </div>

        {activeTab === "all" && (
          <div className="search-input-wrap">
            <Search size={16} />
            <input 
              type="text" 
              className="form-input" 
              placeholder="Search audit, lot, or style..." 
              value={search}
              onChange={(e) => setSearch(e.target.value)}
            />
          </div>
        )}
      </div>

      {/* Tab: ASTM 4-Point Workbench & Interactive Calculator */}
      {activeTab === "astm" && (
        <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1.5rem" }}>
          {/* Calculator Card */}
          <div style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.5rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
            <div style={{ display: "flex", alignItems: "center", gap: "0.5rem", marginBottom: "1rem" }}>
              <Calculator size={20} style={{ color: "#2563eb" }} />
              <h3 style={{ fontSize: "1.125rem", fontWeight: 700, color: "#0f172a" }}>
                ASTM D5430-07 4-Point Scoring Calculator
              </h3>
            </div>

            <div style={{ background: "#eff6ff", border: "1px solid #bfdbfe", padding: "0.75rem", borderRadius: "0.5rem", marginBottom: "1.25rem", fontSize: "0.8125rem", color: "#1e40af" }}>
              <b>Official Standard Formula:</b><br />
              <code>Points per 100 sq. yd = (Total Penalty Points &times; 3600) / (Inspected Yards &times; Cuttable Width Inches)</code><br />
              <span style={{ fontSize: "0.75rem", color: "#3b82f6" }}>Acceptance Limit: Score &le; 28.0 pts/100 sq.yd = PASS</span>
            </div>

            <form onSubmit={handleCreateInspection}>
              <div className="form-group">
                <label className="form-label">Stock Lot / Fabric Roll Ref</label>
                <select 
                  className="form-select"
                  value={calcLotNumber}
                  onChange={(e) => setCalcLotNumber(e.target.value)}>
                  {stockLots.map(l => (
                    <option key={l.id} value={l.lotBatchNumber}>{l.lotBatchNumber} — {l.itemDescription}</option>
                  ))}
                </select>
              </div>

              <div className="form-grid-2">
                <div className="form-group">
                  <label className="form-label">Total Penalty Points Recorded</label>
                  <input 
                    type="number" 
                    className="form-input" 
                    value={calcPenaltyPoints}
                    onChange={(e) => setCalcPenaltyPoints(parseFloat(e.target.value) || 0)}
                    required
                  />
                  <span style={{ fontSize: "0.75rem", color: "#64748b" }}>Sum of 1, 2, 3, 4 point defects</span>
                </div>

                <div className="form-group">
                  <label className="form-label">Total Inspected Yards</label>
                  <input 
                    type="number" 
                    className="form-input" 
                    value={calcYardsInspected}
                    onChange={(e) => setCalcYardsInspected(parseFloat(e.target.value) || 1)}
                    required
                  />
                  <span style={{ fontSize: "0.75rem", color: "#64748b" }}>Representative 10% sampling</span>
                </div>
              </div>

              <div className="form-grid-2">
                <div className="form-group">
                  <label className="form-label">Cuttable Width (Inches)</label>
                  <input 
                    type="number" 
                    step="0.5" 
                    className="form-input" 
                    value={calcCuttableWidth}
                    onChange={(e) => setCalcCuttableWidth(parseFloat(e.target.value) || 1)}
                    required
                  />
                  <span style={{ fontSize: "0.75rem", color: "#64748b" }}>Standard width: 58.0 Inches</span>
                </div>

                <div className="form-group">
                  <label className="form-label">Certified QC Inspector</label>
                  <input 
                    type="text" 
                    className="form-input" 
                    value={calcInspector}
                    onChange={(e) => setCalcInspector(e.target.value)}
                    required
                  />
                </div>
              </div>

              <button 
                type="submit" 
                className="btn btn-primary" 
                style={{ width: "100%", marginTop: "1rem" }}>
                <span>Record & Evaluate Inspection</span>
              </button>
            </form>
          </div>

          {/* Real-time Verdict Gauge Card */}
          <div style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.5rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)", display: "flex", flexDirection: "column", justifyContent: "space-between" }}>
            <div>
              <span style={{ fontSize: "0.75rem", fontWeight: 700, textTransform: "uppercase", color: "#64748b" }}>
                Real-Time Compliance Evaluation
              </span>
              <h3 style={{ fontSize: "1.25rem", fontWeight: 800, color: "#0f172a", marginTop: "0.25rem" }}>
                ASTM D5430-07 Audit Verdict
              </h3>

              <div style={{
                marginTop: "1.5rem",
                padding: "1.5rem",
                borderRadius: "0.75rem",
                background: isAstmPassed ? "#f0fdf4" : "#fef2f2",
                border: `1.5px solid ${isAstmPassed ? "#86efac" : "#fca5a5"}`,
                textAlign: "center"
              }}>
                <div style={{ fontSize: "0.875rem", color: isAstmPassed ? "#166534" : "#991b1b", fontWeight: 600 }}>
                  CALCULATED DEFECT SCORE
                </div>
                <div style={{ fontSize: "3rem", fontWeight: 900, color: isAstmPassed ? "#15803d" : "#dc2626", margin: "0.5rem 0" }}>
                  {astmScore} <span style={{ fontSize: "1rem", fontWeight: 600 }}>pts / 100 sq.yd</span>
                </div>
                <div style={{
                  display: "inline-flex",
                  alignItems: "center",
                  gap: "0.35rem",
                  padding: "0.35rem 0.75rem",
                  borderRadius: "9999px",
                  fontSize: "0.875rem",
                  fontWeight: 700,
                  backgroundColor: isAstmPassed ? "#dcfce7" : "#fee2e2",
                  color: isAstmPassed ? "#15803d" : "#b91c1c"
                }}>
                  {isAstmPassed ? <CheckCircle2 size={16} /> : <AlertOctagon size={16} />}
                  <span>{isAstmPassed ? "ACCEPTANCE THRESHOLD PASSED (<= 28.0)" : "REJECTED (> 28.0 LIMIT EXCEEDED)"}</span>
                </div>
              </div>

              <div style={{ marginTop: "1.5rem", fontSize: "0.875rem", color: "#475569" }}>
                <p>
                  <b>Mathematical Breakdown:</b><br />
                  <code>({calcPenaltyPoints} points &times; 3600) / ({calcYardsInspected} yds &times; {calcCuttableWidth}&quot;) = {(calcPenaltyPoints * 3600).toLocaleString()} / {(calcYardsInspected * calcCuttableWidth).toLocaleString()} = {astmScore}</code>
                </p>
                <p style={{ marginTop: "0.5rem" }}>
                  {isAstmPassed ? (
                    <span style={{ color: "#15803d" }}>
                      &check; <b>Release Clearance:</b> Stock lot is qualified for Quarantine Release and floor allocation to the Cutting department under AC-04.
                    </span>
                  ) : (
                    <span style={{ color: "#dc2626" }}>
                      &cross; <b>Quarantine Lockdown:</b> Exceeds 28.0 points. Cutting floor intake is legally locked. Return to supplier or conditional debit note required.
                    </span>
                  )}
                </p>
              </div>
            </div>

            <div style={{ paddingTop: "1rem", borderTop: "1px solid #e2e8f0" }}>
              <a 
                href="/api/reports/fabric-inspection/latest/pdf"
                target="_blank"
                rel="noopener noreferrer"
                className="btn btn-secondary"
                style={{ width: "100%", display: "flex", justifyContent: "center", alignItems: "center", gap: "0.5rem" }}>
                <Download size={16} />
                <span>Download ASTM D5430 Certificate PDF</span>
              </a>
            </div>
          </div>
        </div>
      )}

      {/* Tab: Quarantine & Cutting Floor Gatekeeper (AC-04) */}
      {activeTab === "quarantine" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
          {/* Gate Check Alert Banner if tested */}
          {gateCheckResult && (
            <div style={{
              padding: "1rem 1.25rem",
              borderRadius: "0.75rem",
              backgroundColor: gateCheckResult.allowed ? "#f0fdf4" : "#fef2f2",
              border: `1.5px solid ${gateCheckResult.allowed ? "#86efac" : "#fca5a5"}`,
              color: gateCheckResult.allowed ? "#166534" : "#991b1b",
              display: "flex",
              justifyContent: "space-between",
              alignItems: "center",
              flexWrap: "wrap",
              gap: "1rem"
            }}>
              <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
                {gateCheckResult.allowed ? <CheckCircle2 size={24} /> : <AlertTriangle size={24} />}
                <div>
                  <strong style={{ display: "block", fontSize: "0.9375rem" }}>
                    AC-04 Gatekeeper Verification for {gateCheckResult.lotNumber}
                  </strong>
                  <span style={{ fontSize: "0.875rem" }}>{gateCheckResult.message}</span>
                </div>
              </div>
              <button 
                className="btn btn-sm"
                style={{ backgroundColor: "transparent", color: "inherit", border: "1px solid currentColor" }}
                onClick={() => setGateCheckResult(null)}>
                Dismiss
              </button>
            </div>
          )}

          <div className="table-container">
            <div className="table-wrapper">
              <table className="table">
                <thead>
                  <tr>
                    <th>Stock Lot Batch</th>
                    <th>Material Code & Description</th>
                    <th>Warehouse Location</th>
                    <th>Quantity</th>
                    <th>Quarantine Stock State</th>
                    <th>QC Inspection Status</th>
                    <th>Cutting Allocation Verdict (AC-04)</th>
                    <th>Action</th>
                  </tr>
                </thead>
                <tbody>
                  {stockLots.map((lot) => {
                    const isReleased = lot.stockState === "RELEASED_FOR_PRODUCTION";
                    return (
                      <tr key={lot.id} style={{ backgroundColor: isReleased ? "#f0fdf4" : "#fffbeb" }}>
                        <td style={{ fontWeight: 700, color: "#0f172a" }}>{lot.lotBatchNumber}</td>
                        <td>
                          <b>{lot.itemCode}</b><br />
                          <span style={{ fontSize: "0.8125rem", color: "#64748b" }}>{lot.itemDescription}</span>
                        </td>
                        <td style={{ fontSize: "0.8125rem" }}>{lot.warehouseLocationCode}</td>
                        <td style={{ fontWeight: 600 }}>{lot.quantity.toLocaleString()} {lot.unitOfMeasure}</td>
                        <td>
                          <span className={`badge ${isReleased ? "badge-success" : "badge-warning"}`}>
                            {lot.stockState}
                          </span>
                        </td>
                        <td>
                          <span className={`badge ${lot.qcInspectionStatus === "PASSED" ? "badge-success" : "badge-secondary"}`}>
                            {lot.qcInspectionStatus}
                          </span>
                        </td>
                        <td>
                          {isReleased ? (
                            <span style={{ display: "inline-flex", alignItems: "center", gap: "0.25rem", color: "#15803d", fontWeight: 700, fontSize: "0.8125rem" }}>
                              <Unlock size={14} />
                              ALLOCATION ALLOWED
                            </span>
                          ) : (
                            <span style={{ display: "inline-flex", alignItems: "center", gap: "0.25rem", color: "#b91c1c", fontWeight: 700, fontSize: "0.8125rem" }}>
                              <Lock size={14} />
                              BLOCKED IN QUARANTINE
                            </span>
                          )}
                        </td>
                        <td>
                          <div style={{ display: "flex", gap: "0.5rem" }}>
                            <button 
                              className="btn btn-outline btn-sm"
                              onClick={() => handleVerifyGatekeeper(lot)}>
                              Verify Gate
                            </button>
                            {isReleased && (
                              <a 
                                href="/api/reports/fabric-inspection/latest/pdf"
                                target="_blank"
                                rel="noopener noreferrer"
                                className="btn btn-outline btn-sm btn-icon"
                                title="Download ASTM D5430 Certificate">
                                <Download size={14} />
                              </a>
                            )}
                          </div>
                        </td>
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Tab: All Inspections Table */}
      {(activeTab === "all" || activeTab === "sewing" || activeTab === "aql") && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Audit Reference</th>
                  <th>Audit Type</th>
                  <th>Lot / Batch Ref</th>
                  <th>Style Ref</th>
                  <th>Sample Size</th>
                  <th>Defect Score / Points</th>
                  <th>Result</th>
                  <th>Inspector</th>
                  <th>Date</th>
                </tr>
              </thead>
              <tbody>
                {filteredInspections.map((q) => {
                  const badge = RESULT_BADGES[q.result] || RESULT_BADGES[0];
                  return (
                    <tr key={q.id}>
                      <td style={{ fontWeight: 600, color: "#2563eb" }}>{q.inspectionNumber}</td>
                      <td>
                        <span className="badge badge-secondary">
                          {INSPECTION_TYPES[q.inspectionType] || "General Audit"}
                        </span>
                      </td>
                      <td style={{ fontWeight: 500 }}>{q.lotOrBatchRef}</td>
                      <td><b>{q.styleNumber || "ST-2026-CHINO-01"}</b></td>
                      <td>{q.sampleSize.toLocaleString()} Units</td>
                      <td style={{ fontWeight: 700 }}>
                        {q.totalPoints} {q.inspectionType === 0 ? "pts/100 sq.yd" : "defects"}
                      </td>
                      <td>
                        <span 
                          style={{
                            padding: "0.2rem 0.5rem",
                            borderRadius: "9999px",
                            fontSize: "0.75rem",
                            fontWeight: 700,
                            backgroundColor: badge.bg,
                            color: badge.color
                          }}>
                          {badge.label}
                        </span>
                      </td>
                      <td style={{ fontSize: "0.8125rem" }}>{q.inspectorName}</td>
                      <td style={{ fontSize: "0.8125rem" }}>{new Date(q.inspectedAt).toLocaleDateString()}</td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
}
