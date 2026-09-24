"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiRequest } from "@/lib/api";
import { 
  GitBranch, Plus, ArrowRight, CheckCircle2, AlertCircle, 
  Settings, Clock, User, Cpu, Percent, RefreshCw, X 
} from "lucide-react";

interface StageLog {
  id: string;
  fromStageName: string;
  toStageName: string;
  inputQuantityKg: number;
  outputQuantityKg: number;
  wasteQuantityKg: number;
  machineId: string;
  operatorName: string;
  remarks?: string;
  createdAt: string;
}

interface ThreadBatch {
  id: string;
  batchNumber: string;
  yarnCount: string;
  fiberComposition: string;
  colorCode: string;
  colorName: string;
  targetQuantityKg: number;
  currentQuantityKg: number;
  totalWastageKg: number;
  currentStage: number;
  currentStageName: string;
  statusName: string;
  startDate: string;
  completedDate?: string;
  notes?: string;
  stageLogs: StageLog[];
}

const STAGES = [
  { id: 1, name: "Raw Fiber", code: "RawFiber" },
  { id: 2, name: "Spinning", code: "Spinning" },
  { id: 3, name: "Dyeing", code: "Dyeing" },
  { id: 4, name: "Winding", code: "Winding" },
  { id: 5, name: "Quality Testing", code: "QualityTesting" },
  { id: 6, name: "Packaged", code: "Packaged" }
];

export default function ThreadProductionPage() {
  const { hasPermission } = useAuth();
  const [batches, setBatches] = useState<ThreadBatch[]>([]);
  const [loading, setLoading] = useState(true);
  
  const [showNewBatchModal, setShowNewBatchModal] = useState(false);
  const [showAdvanceModal, setShowAdvanceModal] = useState(false);
  const [selectedBatch, setSelectedBatch] = useState<ThreadBatch | null>(null);

  // New batch form
  const [newCount, setNewCount] = useState("40/2 Ne");
  const [newFiber, setNewFiber] = useState("100% Spun Polyester");
  const [newColorCode, setNewColorCode] = useState("BLK-001");
  const [newColorName, setNewColorName] = useState("Jet Black");
  const [newTargetKg, setNewTargetKg] = useState(1000);
  const [newNotes, setNewNotes] = useState("");

  // Advance form
  const [nextStage, setNextStage] = useState(2);
  const [outputKg, setOutputKg] = useState(970);
  const [wasteKg, setWasteKg] = useState(30);
  const [machineId, setMachineId] = useState("SPIN-01");
  const [operatorName, setOperatorName] = useState("Kabir Hossain");
  const [remarks, setRemarks] = useState("Stage completed smoothly");

  const fetchData = async () => {
    setLoading(true);
    try {
      const res = await apiRequest<ThreadBatch[]>("/api/v1/threadproduction/batches");
      if (res.success && res.data) setBatches(res.data);
    } catch (err) {
      console.error("Failed to load thread batches", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreateBatch = async (e: React.FormEvent) => {
    e.preventDefault();
    const res = await apiRequest("/api/v1/threadproduction/batches", {
      method: "POST",
      body: JSON.stringify({
        yarnCount: newCount,
        fiberComposition: newFiber,
        colorCode: newColorCode,
        colorName: newColorName,
        targetQuantityKg: newTargetKg,
        notes: newNotes
      })
    });

    if (res.success) {
      setShowNewBatchModal(false);
      fetchData();
    }
  };

  const handleAdvanceStage = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedBatch) return;

    const res = await apiRequest(`/api/v1/threadproduction/batches/${selectedBatch.id}/advance-stage`, {
      method: "POST",
      body: JSON.stringify({
        nextStage: nextStage,
        outputQuantityKg: outputKg,
        wasteQuantityKg: wasteKg,
        machineId: machineId,
        operatorName: operatorName,
        remarks: remarks
      })
    });

    if (res.success) {
      setShowAdvanceModal(false);
      setSelectedBatch(null);
      fetchData();
    }
  };

  const totalBatches = batches.length;
  const inProgress = batches.filter(b => b.statusName === "InProgress").length;
  const totalVolumeKg = batches.reduce((acc, b) => acc + b.currentQuantityKg, 0);
  const totalWastageKg = batches.reduce((acc, b) => acc + b.totalWastageKg, 0);

  return (
    <div>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 24, flexWrap: "wrap", gap: 12 }}>
        <div>
          <h2 style={{ fontSize: "1.4rem", fontWeight: 800 }}>Thread & Yarn Manufacturing Pipeline</h2>
          <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
            Raw Fiber processing, Ring Spinning, HT Dyeing, Cone Winding, and Quality Inspection.
          </p>
        </div>

        <div style={{ display: "flex", gap: 10 }}>
          <button className="btn btn-secondary" onClick={fetchData} title="Refresh data">
            <RefreshCw size={15} />
            <span>Refresh</span>
          </button>
          <button className="btn btn-primary" onClick={() => setShowNewBatchModal(true)}>
            <Plus size={16} />
            <span>New Production Batch</span>
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <span className="kpi-label">Active Spinning Batches</span>
          <span className="kpi-value" style={{ color: "var(--color-primary)" }}>{inProgress}</span>
          <span className="kpi-meta">{totalBatches} Total Batches Initiated</span>
        </div>

        <div className="kpi-card">
          <span className="kpi-label">Current Floor Volume</span>
          <span className="kpi-value" style={{ color: "var(--color-info)" }}>{totalVolumeKg.toLocaleString()} Kg</span>
          <span className="kpi-meta">Yarn In-Production</span>
        </div>

        <div className="kpi-card">
          <span className="kpi-label">Wastage Tracked</span>
          <span className="kpi-value" style={{ color: "var(--color-warning)" }}>{totalWastageKg.toLocaleString()} Kg</span>
          <span className="kpi-meta">Across All Processing Stages</span>
        </div>

        <div className="kpi-card">
          <span className="kpi-label">Average Yield Efficiency</span>
          <span className="kpi-value" style={{ color: "var(--color-success)" }}>
            {totalVolumeKg > 0 ? ((totalVolumeKg / (totalVolumeKg + totalWastageKg)) * 100).toFixed(1) : "96.5"}%
          </span>
          <span className="kpi-meta">Fiber to Cone Ratio</span>
        </div>
      </div>

      {/* Batches Pipeline View */}
      <div style={{ display: "flex", flexDirection: "column", gap: 20 }}>
        {batches.map((batch) => (
          <div key={batch.id} className="glass-panel" style={{ padding: 24 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 18, flexWrap: "wrap", gap: 12 }}>
              <div>
                <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                  <h3 style={{ fontSize: "1.15rem", fontWeight: 700, color: "var(--text-accent)" }}>{batch.batchNumber}</h3>
                  <span className={`badge ${batch.statusName === "Completed" ? "badge-success" : "badge-info"}`}>
                    {batch.statusName}
                  </span>
                </div>
                <div style={{ fontSize: "0.85rem", color: "var(--text-secondary)", marginTop: 4 }}>
                  <b>{batch.yarnCount}</b> • {batch.fiberComposition} • Color: <b>{batch.colorName}</b> (<code>{batch.colorCode}</code>)
                </div>
              </div>

              <div style={{ display: "flex", gap: 10, alignItems: "center" }}>
                <div style={{ textAlign: "right", marginRight: 10 }}>
                  <div style={{ fontSize: "0.78rem", color: "var(--text-muted)" }}>Current Output</div>
                  <div style={{ fontSize: "1.05rem", fontWeight: 700 }}>{batch.currentQuantityKg.toLocaleString()} / {batch.targetQuantityKg.toLocaleString()} Kg</div>
                </div>

                {batch.currentStage < 6 && (
                  <button 
                    className="btn btn-primary"
                    onClick={() => {
                      setSelectedBatch(batch);
                      setNextStage(batch.currentStage + 1);
                      setOutputKg(Math.round(batch.currentQuantityKg * 0.98));
                      setWasteKg(Math.round(batch.currentQuantityKg * 0.02));
                      setShowAdvanceModal(true);
                    }}>
                    <ArrowRight size={15} />
                    <span>Advance Stage</span>
                  </button>
                )}
              </div>
            </div>

            {/* Stages Stepper */}
            <div style={{ display: "grid", gridTemplateColumns: "repeat(6, 1fr)", gap: 8, margin: "20px 0 16px" }}>
              {STAGES.map((s, idx) => {
                const isPassed = s.id < batch.currentStage;
                const isCurrent = s.id === batch.currentStage;
                return (
                  <div key={s.id} style={{
                    padding: "10px 8px",
                    borderRadius: 8,
                    textAlign: "center",
                    backgroundColor: isCurrent ? "rgba(99, 102, 241, 0.18)" : isPassed ? "rgba(16, 185, 129, 0.1)" : "rgba(255,255,255,0.03)",
                    border: isCurrent ? "1px solid var(--color-primary)" : isPassed ? "1px solid rgba(16, 185, 129, 0.3)" : "1px solid var(--border-color)",
                  }}>
                    <div style={{ fontSize: "0.7rem", color: isCurrent ? "var(--color-primary)" : isPassed ? "var(--color-success)" : "var(--text-muted)", fontWeight: 700 }}>
                      STAGE 0{s.id}
                    </div>
                    <div style={{ fontSize: "0.82rem", fontWeight: 600, marginTop: 2, color: isCurrent ? "var(--text-primary)" : isPassed ? "var(--color-success)" : "var(--text-muted)" }}>
                      {s.name}
                    </div>
                  </div>
                );
              })}
            </div>

            {/* Stage Logs History */}
            {batch.stageLogs && batch.stageLogs.length > 0 && (
              <div style={{ background: "rgba(0,0,0,0.2)", borderRadius: 8, padding: 12, marginTop: 12 }}>
                <div style={{ fontSize: "0.8rem", fontWeight: 700, marginBottom: 8, color: "var(--text-secondary)" }}>Process Logs:</div>
                <div style={{ display: "flex", flexDirection: "column", gap: 6, fontSize: "0.8rem" }}>
                  {batch.stageLogs.map((log) => (
                    <div key={log.id} style={{ display: "flex", justifyContent: "space-between", color: "var(--text-muted)" }}>
                      <span>
                        • <b>{log.toStageName}</b>: Output <b>{log.outputQuantityKg} Kg</b> (Wastage: {log.wasteQuantityKg} Kg) on Machine <code>{log.machineId}</code> by {log.operatorName}
                      </span>
                      <span>{new Date(log.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
        ))}
      </div>

      {/* New Batch Modal */}
      {showNewBatchModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 550 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 18 }}>
              <h3 style={{ fontSize: "1.2rem", fontWeight: 700 }}>Initiate New Thread Production Batch</h3>
              <button onClick={() => setShowNewBatchModal(false)} style={{ background: "none", border: "none", color: "var(--text-muted)", cursor: "pointer" }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleCreateBatch}>
              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14, marginBottom: 14 }}>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Yarn Count</label>
                  <input 
                    type="text" 
                    placeholder="e.g. 40/2 Ne" 
                    required 
                    value={newCount} 
                    onChange={(e) => setNewCount(e.target.value)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Fiber Composition</label>
                  <input 
                    type="text" 
                    placeholder="e.g. 100% Spun Polyester" 
                    required 
                    value={newFiber} 
                    onChange={(e) => setNewFiber(e.target.value)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 2fr", gap: 14, marginBottom: 14 }}>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Color Code</label>
                  <input 
                    type="text" 
                    placeholder="e.g. BLK-001" 
                    required 
                    value={newColorCode} 
                    onChange={(e) => setNewColorCode(e.target.value)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Color Name</label>
                  <input 
                    type="text" 
                    placeholder="e.g. Jet Black" 
                    required 
                    value={newColorName} 
                    onChange={(e) => setNewColorName(e.target.value)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
              </div>

              <div style={{ marginBottom: 14 }}>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Target Quantity (Kg)</label>
                <input 
                  type="number" 
                  min={50} 
                  required 
                  value={newTargetKg} 
                  onChange={(e) => setNewTargetKg(parseFloat(e.target.value) || 0)}
                  style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                />
              </div>

              <div style={{ marginBottom: 20 }}>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Order Notes / Customer Contract</label>
                <input 
                  type="text" 
                  placeholder="e.g. High tenacity thread for export order"
                  value={newNotes} 
                  onChange={(e) => setNewNotes(e.target.value)}
                  style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                />
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: 10 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowNewBatchModal(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">Start Production Batch</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Advance Stage Modal */}
      {showAdvanceModal && selectedBatch && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 520 }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 18 }}>
              <h3 style={{ fontSize: "1.2rem", fontWeight: 700 }}>Advance Batch: {selectedBatch.batchNumber}</h3>
              <button onClick={() => setShowAdvanceModal(false)} style={{ background: "none", border: "none", color: "var(--text-muted)", cursor: "pointer" }}>
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleAdvanceStage}>
              <div style={{ marginBottom: 14 }}>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Target Stage</label>
                <select 
                  value={nextStage} 
                  onChange={(e) => setNextStage(parseInt(e.target.value))}
                  style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }}>
                  {STAGES.filter(s => s.id > selectedBatch.currentStage).map((s) => (
                    <option key={s.id} value={s.id}>{s.name} (Stage 0{s.id})</option>
                  ))}
                </select>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14, marginBottom: 14 }}>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Output Quantity (Kg)</label>
                  <input 
                    type="number" 
                    step="0.1" 
                    required 
                    value={outputKg} 
                    onChange={(e) => setOutputKg(parseFloat(e.target.value) || 0)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Wastage / Fly Loss (Kg)</label>
                  <input 
                    type="number" 
                    step="0.1" 
                    required 
                    value={wasteKg} 
                    onChange={(e) => setWasteKg(parseFloat(e.target.value) || 0)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 14, marginBottom: 14 }}>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Machine ID</label>
                  <input 
                    type="text" 
                    required 
                    value={machineId} 
                    onChange={(e) => setMachineId(e.target.value)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
                <div>
                  <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Operator Name</label>
                  <input 
                    type="text" 
                    required 
                    value={operatorName} 
                    onChange={(e) => setOperatorName(e.target.value)}
                    style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                  />
                </div>
              </div>

              <div style={{ marginBottom: 20 }}>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, marginBottom: 4 }}>Quality Remarks</label>
                <input 
                  type="text" 
                  value={remarks} 
                  onChange={(e) => setRemarks(e.target.value)}
                  style={{ width: "100%", padding: "10px", borderRadius: 8, background: "var(--bg-input)", border: "1px solid var(--border-color)", color: "var(--text-primary)" }} 
                />
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: 10 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowAdvanceModal(false)}>Cancel</button>
                <button type="submit" className="btn btn-primary">Complete Stage & Advance</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
