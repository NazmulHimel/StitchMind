"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import {
  Scissors,
  Plus,
  Ticket,
  Printer,
  Layers,
  Sparkles,
  ArrowRight,
  Barcode,
  Search,
  CheckCircle2,
  Clock,
  RefreshCw,
  AlertCircle
} from "lucide-react";

interface CutBundle {
  id: string;
  bundleNumber: string;
  cuttingOrderId: string;
  size: string;
  color: string;
  quantity: number;
  barcode: string;
  serialStart: number;
  serialEnd: number;
  currentSection: number; // 0: Cutting, 1: Sewing, 2: QC, 3: Washing, 4: Finishing, 5: Packed
  isCompleted: boolean;
}

interface CuttingOrder {
  id: string;
  cuttingOrderNumber: string;
  buyerOrderId: string;
  orderNumber: string;
  styleNumber: string;
  fabricType: string;
  totalRequiredQty: number;
  totalPlies: number;
  markerLength: number;
  fabricWidth: number;
  efficiencyPercentage: number;
  status: number; // 0: Draft, 1: SpreadingLay, 2: InProgress, 3: CutCompleted, 4: Bundled
  tableNumber: string;
  cuttingDate: string;
  bundles: CutBundle[];
}

interface BuyerOrder {
  id: string;
  orderNumber: string;
  styleNumber: string;
  styleName: string;
  orderQuantity: number;
}

const SECTION_NAMES = ["Cutting", "Sewing", "Quality Control", "Washing", "Finishing", "Packed"];
const STATUS_LABELS: { [key: number]: { label: string; color: string; bg: string } } = {
  0: { label: "Draft", color: "var(--text-muted)", bg: "rgba(255,255,255,0.05)" },
  1: { label: "Spreading Lay", color: "#f59e0b", bg: "rgba(245,158,11,0.15)" },
  2: { label: "In Progress", color: "#38bdf8", bg: "rgba(56,189,248,0.15)" },
  3: { label: "Cut Completed", color: "#10b981", bg: "rgba(16,185,129,0.15)" },
  4: { label: "Bundled", color: "#a855f7", bg: "rgba(168,85,247,0.15)" }
};

export default function CuttingPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission("Cutting.Create");

  const [activeTab, setActiveTab] = useState<"orders" | "marker" | "bundles">("orders");
  const [orders, setOrders] = useState<CuttingOrder[]>([]);
  const [buyerOrders, setBuyerOrders] = useState<BuyerOrder[]>([]);
  const [bundles, setBundles] = useState<CutBundle[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [search, setSearch] = useState("");

  // Modals
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showBundleModal, setShowBundleModal] = useState(false);
  const [selectedOrderForBundles, setSelectedOrderForBundles] = useState<CuttingOrder | null>(null);
  const [bundleSize, setBundleSize] = useState(50);

  // Form State
  const [newOrder, setNewOrder] = useState({
    buyerOrderId: "",
    styleNumber: "ST-882",
    fabricType: "100% Pique Cotton 220 GSM",
    totalRequiredQty: 2500,
    totalPlies: 80,
    markerLength: 6.85,
    fabricWidth: 62.0,
    efficiencyPercentage: 88.6,
    tableNumber: "Table-01"
  });

  const fetchData = async () => {
    setLoading(true);
    try {
      const [resOrders, resBuyerOrders, resBundles] = await Promise.all([
        fetch("http://localhost:5224/api/v1/cutting/orders").then(r => r.json()),
        fetch("http://localhost:5224/api/v1/buyerorders").then(r => r.json()),
        fetch("http://localhost:5224/api/v1/cutting/bundles").then(r => r.json())
      ]);

      if (Array.isArray(resOrders)) setOrders(resOrders);
      if (Array.isArray(resBuyerOrders)) setBuyerOrders(resBuyerOrders);
      if (Array.isArray(resBundles)) setBundles(resBundles);
    } catch (err) {
      console.error("Error fetching cutting data:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleCreateOrder = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await fetch("http://localhost:5224/api/v1/cutting/orders", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(newOrder)
      });
      if (res.ok) {
        setShowCreateModal(false);
        fetchData();
      }
    } catch (err) {
      console.error("Failed to create cutting order:", err);
    }
  };

  const handleGenerateBundles = async (orderId: string) => {
    try {
      const res = await fetch(`http://localhost:5224/api/v1/cutting/orders/${orderId}/generate-bundles?bundleSize=${bundleSize}`, {
        method: "POST"
      });
      if (res.ok) {
        setShowBundleModal(false);
        fetchData();
      }
    } catch (err) {
      console.error("Failed to generate bundles:", err);
    }
  };

  const handleTransferBundle = async (bundleId: string, currentSection: number) => {
    const nextSection = (currentSection + 1) % 6;
    try {
      await fetch(`http://localhost:5224/api/v1/cutting/bundles/${bundleId}/transfer`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify(nextSection)
      });
      fetchData();
    } catch (err) {
      console.error("Failed to transfer bundle:", err);
    }
  };

  const filteredOrders = orders.filter(
    o => o.cuttingOrderNumber.toLowerCase().includes(search.toLowerCase()) ||
         o.styleNumber.toLowerCase().includes(search.toLowerCase()) ||
         o.fabricType.toLowerCase().includes(search.toLowerCase())
  );

  const totalCutQty = orders.reduce((sum, o) => sum + o.totalRequiredQty, 0);
  const avgEfficiency = orders.length > 0 ? (orders.reduce((sum, o) => sum + o.efficiencyPercentage, 0) / orders.length).toFixed(1) : "0.0";
  const totalPliesCount = orders.reduce((sum, o) => sum + o.totalPlies, 0);

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: 24 }}>
      {/* Top Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: 12 }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
            <Scissors size={24} style={{ color: "var(--color-primary-light)" }} />
            <h2 style={{ fontSize: "1.45rem", fontWeight: 800 }}>Cutting Hall, CAD Nesting & Bundles</h2>
          </div>
          <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
            Computerized marker optimization, ply spreading, cutting orders, and bundle ticket generation.
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
              <span>Create Cutting Order</span>
            </button>
          )}
        </div>
      </div>

      {/* KPI Cards */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(220px, 1fr))", gap: 16 }}>
        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>ACTIVE CUTTING ORDERS</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "var(--text-primary)", marginTop: 6 }}>
            {orders.length} <span style={{ fontSize: "0.9rem", color: "var(--text-secondary)", fontWeight: 400 }}>Orders</span>
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: 4, display: "flex", alignItems: "center", gap: 4 }}>
            <CheckCircle2 size={13} /> {orders.filter(o => o.status === 3).length} Orders Cut Completed
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>TOTAL REQUIRED CUT PCS</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "var(--color-primary-light)", marginTop: 6 }}>
            {totalCutQty.toLocaleString()} <span style={{ fontSize: "0.9rem", fontWeight: 400 }}>Pcs</span>
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-secondary)", marginTop: 4 }}>
            Spread across {totalPliesCount} Fabric Plies
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>CAD MARKER AVG EFFICIENCY</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "#f59e0b", marginTop: 6 }}>
            {avgEfficiency}%
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: 4, display: "flex", alignItems: "center", gap: 4 }}>
            <Sparkles size={13} /> High fabric utilization rate
          </div>
        </div>

        <div className="card" style={{ padding: 18 }}>
          <span style={{ fontSize: "0.8rem", color: "var(--text-muted)", fontWeight: 600 }}>TOTAL CUT BUNDLES</span>
          <div style={{ fontSize: "1.8rem", fontWeight: 800, color: "#a855f7", marginTop: 6 }}>
            {bundles.length} <span style={{ fontSize: "0.9rem", fontWeight: 400 }}>Bundles</span>
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-secondary)", marginTop: 4 }}>
            With QR / Barcode serial tracking
          </div>
        </div>
      </div>

      {/* Tabs */}
      <div style={{ display: "flex", borderBottom: "1px solid var(--border-color)", gap: 12 }}>
        <button
          onClick={() => setActiveTab("orders")}
          style={{
            padding: "10px 16px",
            background: "none",
            border: "none",
            borderBottom: activeTab === "orders" ? "2px solid var(--color-primary-light)" : "2px solid transparent",
            color: activeTab === "orders" ? "var(--text-primary)" : "var(--text-secondary)",
            fontWeight: 700,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8
          }}
        >
          <Scissors size={16} />
          <span>Cutting Orders & Lay Plans</span>
        </button>

        <button
          onClick={() => setActiveTab("marker")}
          style={{
            padding: "10px 16px",
            background: "none",
            border: "none",
            borderBottom: activeTab === "marker" ? "2px solid var(--color-primary-light)" : "2px solid transparent",
            color: activeTab === "marker" ? "var(--text-primary)" : "var(--text-secondary)",
            fontWeight: 700,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8
          }}
        >
          <Layers size={16} />
          <span>CAD Marker Optimization</span>
        </button>

        <button
          onClick={() => setActiveTab("bundles")}
          style={{
            padding: "10px 16px",
            background: "none",
            border: "none",
            borderBottom: activeTab === "bundles" ? "2px solid var(--color-primary-light)" : "2px solid transparent",
            color: activeTab === "bundles" ? "var(--text-primary)" : "var(--text-secondary)",
            fontWeight: 700,
            cursor: "pointer",
            display: "flex",
            alignItems: "center",
            gap: 8
          }}
        >
          <Barcode size={16} />
          <span>Cut Bundles & Section Routing ({bundles.length})</span>
        </button>
      </div>

      {/* Tab 1: Cutting Orders */}
      {activeTab === "orders" && (
        <div className="card" style={{ padding: 20 }}>
          <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 16, gap: 12, flexWrap: "wrap" }}>
            <div style={{ position: "relative", width: 280 }}>
              <Search size={16} style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "var(--text-muted)" }} />
              <input
                type="text"
                placeholder="Search order, style, fabric..."
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
                  <th>Cutting Order</th>
                  <th>Buyer Order</th>
                  <th>Style Article</th>
                  <th>Fabric Spec</th>
                  <th>Target Pcs</th>
                  <th>Lay Plies</th>
                  <th>Marker Eff.</th>
                  <th>Table</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredOrders.length === 0 ? (
                  <tr>
                    <td colSpan={10} style={{ textAlign: "center", padding: 32, color: "var(--text-muted)" }}>
                      {loading ? "Loading cutting orders..." : "No cutting orders found."}
                    </td>
                  </tr>
                ) : (
                  filteredOrders.map((o) => {
                    const st = STATUS_LABELS[o.status] || STATUS_LABELS[0];
                    return (
                      <tr key={o.id}>
                        <td style={{ fontWeight: 700, color: "var(--color-primary-light)" }}>{o.cuttingOrderNumber}</td>
                        <td style={{ fontWeight: 600 }}>{o.orderNumber}</td>
                        <td>{o.styleNumber}</td>
                        <td style={{ fontSize: "0.85rem", color: "var(--text-secondary)" }}>{o.fabricType}</td>
                        <td style={{ fontWeight: 700 }}>{o.totalRequiredQty.toLocaleString()}</td>
                        <td>{o.totalPlies} Plies</td>
                        <td>
                          <span style={{ fontWeight: 700, color: o.efficiencyPercentage >= 88 ? "#10b981" : "#f59e0b" }}>
                            {o.efficiencyPercentage}%
                          </span>
                        </td>
                        <td>
                          <span style={{ padding: "3px 8px", background: "rgba(255,255,255,0.05)", borderRadius: 4, fontSize: "0.8rem" }}>
                            {o.tableNumber}
                          </span>
                        </td>
                        <td>
                          <span
                            style={{
                              padding: "4px 10px",
                              borderRadius: 12,
                              fontSize: "0.75rem",
                              fontWeight: 700,
                              color: st.color,
                              background: st.bg
                            }}
                          >
                            {st.label}
                          </span>
                        </td>
                        <td>
                          <div style={{ display: "flex", gap: 6 }}>
                            <button
                              className="btn btn-secondary"
                              style={{ padding: "4px 8px", fontSize: "0.75rem" }}
                              onClick={() => {
                                setSelectedOrderForBundles(o);
                                setShowBundleModal(true);
                              }}
                            >
                              <Ticket size={13} />
                              <span>Bundles ({o.bundles?.length || 0})</span>
                            </button>
                          </div>
                        </td>
                      </tr>
                    );
                  })
                )}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 2: CAD Marker Optimization */}
      {activeTab === "marker" && (
        <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(320px, 1fr))", gap: 16 }}>
          {orders.map((o) => (
            <div key={o.id} className="glass-panel" style={{ padding: 20 }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: 12 }}>
                <div>
                  <h3 style={{ fontSize: "1.05rem", fontWeight: 700 }}>{o.cuttingOrderNumber} â€” {o.styleNumber}</h3>
                  <p style={{ color: "var(--text-secondary)", fontSize: "0.8rem" }}>{o.fabricType}</p>
                </div>
                <span
                  style={{
                    padding: "4px 10px",
                    borderRadius: 12,
                    fontSize: "0.8rem",
                    fontWeight: 800,
                    color: o.efficiencyPercentage >= 88 ? "#10b981" : "#f59e0b",
                    background: o.efficiencyPercentage >= 88 ? "rgba(16,185,129,0.15)" : "rgba(245,158,11,0.15)"
                  }}
                >
                  {o.efficiencyPercentage}% CAD Eff.
                </span>
              </div>

              {/* Marker Schematic Simulation Bar */}
              <div style={{ marginBottom: 16 }}>
                <div style={{ display: "flex", justifyContent: "space-between", fontSize: "0.75rem", color: "var(--text-muted)", marginBottom: 4 }}>
                  <span>Marker Length: {o.markerLength}m</span>
                  <span>Fabric Width: {o.fabricWidth}&quot;</span>
                </div>
                <div style={{ height: 28, width: "100%", background: "rgba(255,255,255,0.05)", borderRadius: 6, overflow: "hidden", display: "flex", border: "1px solid var(--border-color)" }}>
                  <div style={{ width: `${o.efficiencyPercentage}%`, background: "linear-gradient(90deg, #38bdf8, #6366f1)", display: "flex", alignItems: "center", justifyContent: "center", color: "#fff", fontSize: "0.75rem", fontWeight: 700 }}>
                    Nesting Area ({o.efficiencyPercentage}%)
                  </div>
                  <div style={{ width: `${100 - o.efficiencyPercentage}%`, background: "rgba(239, 68, 68, 0.2)", display: "flex", alignItems: "center", justifyContent: "center", color: "#fca5a5", fontSize: "0.7rem", fontWeight: 600 }}>
                    End Loss
                  </div>
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10, fontSize: "0.85rem", color: "var(--text-secondary)" }}>
                <div>Plies Spread: <strong style={{ color: "var(--text-primary)" }}>{o.totalPlies} Layers</strong></div>
                <div>Allocated Table: <strong style={{ color: "var(--text-primary)" }}>{o.tableNumber}</strong></div>
                <div>Target Output: <strong style={{ color: "var(--text-primary)" }}>{o.totalRequiredQty} Pcs</strong></div>
                <div>Bundle Count: <strong style={{ color: "var(--text-primary)" }}>{o.bundles?.length || 0} Sets</strong></div>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Tab 3: Cut Bundles Tracker */}
      {activeTab === "bundles" && (
        <div className="card" style={{ padding: 20 }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: 16 }}>
            <h3 style={{ fontSize: "1.05rem", fontWeight: 700 }}>Live Bundle Ticket & Section Routing</h3>
            <span style={{ fontSize: "0.85rem", color: "var(--text-muted)" }}>Click &apos;Advance Section&apos; to route bundle along factory floor</span>
          </div>

          <div className="table-container">
            <table className="data-table">
              <thead>
                <tr>
                  <th>Bundle Ticket</th>
                  <th>Barcode</th>
                  <th>Size</th>
                  <th>Color</th>
                  <th>Quantity</th>
                  <th>Serial Range</th>
                  <th>Current Floor Section</th>
                  <th>Action</th>
                </tr>
              </thead>
              <tbody>
                {bundles.map((b) => (
                  <tr key={b.id}>
                    <td style={{ fontWeight: 700, color: "var(--text-accent)" }}>{b.bundleNumber}</td>
                    <td>
                      <span style={{ fontFamily: "monospace", padding: "2px 6px", background: "rgba(255,255,255,0.05)", borderRadius: 4, fontSize: "0.8rem" }}>
                        {b.barcode}
                      </span>
                    </td>
                    <td><strong style={{ color: "#38bdf8" }}>{b.size}</strong></td>
                    <td>{b.color}</td>
                    <td style={{ fontWeight: 700 }}>{b.quantity} pcs</td>
                    <td style={{ fontSize: "0.85rem", color: "var(--text-muted)" }}>#{b.serialStart} - #{b.serialEnd}</td>
                    <td>
                      <span
                        style={{
                          padding: "4px 10px",
                          borderRadius: 12,
                          fontSize: "0.75rem",
                          fontWeight: 700,
                          background: b.currentSection === 5 ? "rgba(16,185,129,0.15)" : "rgba(99,102,241,0.15)",
                          color: b.currentSection === 5 ? "#10b981" : "var(--color-primary-light)"
                        }}
                      >
                        {SECTION_NAMES[b.currentSection]}
                      </span>
                    </td>
                    <td>
                      <button
                        className="btn btn-secondary"
                        style={{ padding: "4px 8px", fontSize: "0.75rem" }}
                        onClick={() => handleTransferBundle(b.id, b.currentSection)}
                      >
                        <span>Transfer</span>
                        <ArrowRight size={13} />
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Modal: Create Cutting Order */}
      {showCreateModal && (
        <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000, padding: 16 }}>
          <div className="card" style={{ width: "100%", maxWidth: 540, padding: 24, borderRadius: 12 }}>
            <h3 style={{ fontSize: "1.2rem", fontWeight: 800, marginBottom: 16 }}>New Cutting Order & Lay Plan</h3>
            <form onSubmit={handleCreateOrder} style={{ display: "flex", flexDirection: "column", gap: 14 }}>
              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Select Buyer Order</label>
                <select
                  required
                  value={newOrder.buyerOrderId}
                  onChange={(e) => {
                    const sel = buyerOrders.find(bo => bo.id === e.target.value);
                    setNewOrder({
                      ...newOrder,
                      buyerOrderId: e.target.value,
                      styleNumber: sel ? sel.styleNumber : newOrder.styleNumber
                    });
                  }}
                  style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                >
                  <option value="">-- Choose Buyer Order --</option>
                  {buyerOrders.map((bo) => (
                    <option key={bo.id} value={bo.id} style={{ background: "#ffffff", color: "#0f172a" }}>
                      {bo.orderNumber} â€” {bo.styleNumber} ({bo.styleName}) [{bo.orderQuantity} Pcs]
                    </option>
                  ))}
                </select>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Target Cut Quantity</label>
                  <input
                    type="number"
                    required
                    value={newOrder.totalRequiredQty}
                    onChange={(e) => setNewOrder({ ...newOrder, totalRequiredQty: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Total Fabric Plies</label>
                  <input
                    type="number"
                    required
                    value={newOrder.totalPlies}
                    onChange={(e) => setNewOrder({ ...newOrder, totalPlies: parseInt(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Fabric Specification</label>
                <input
                  type="text"
                  required
                  value={newOrder.fabricType}
                  onChange={(e) => setNewOrder({ ...newOrder, fabricType: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                />
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr 1fr", gap: 12 }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Marker Length (m)</label>
                  <input
                    type="number"
                    step="0.01"
                    required
                    value={newOrder.markerLength}
                    onChange={(e) => setNewOrder({ ...newOrder, markerLength: parseFloat(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Fabric Width (&quot;)</label>
                  <input
                    type="number"
                    step="0.1"
                    required
                    value={newOrder.fabricWidth}
                    onChange={(e) => setNewOrder({ ...newOrder, fabricWidth: parseFloat(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>CAD Eff. %</label>
                  <input
                    type="number"
                    step="0.1"
                    required
                    value={newOrder.efficiencyPercentage}
                    onChange={(e) => setNewOrder({ ...newOrder, efficiencyPercentage: parseFloat(e.target.value) || 0 })}
                    style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
                  />
                </div>
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: 10, marginTop: 12 }}>
                <button type="button" className="btn btn-secondary" onClick={() => setShowCreateModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Save & Generate Lay Plan
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Modal: Generate Bundles */}
      {showBundleModal && selectedOrderForBundles && (
        <div style={{ position: "fixed", inset: 0, background: "rgba(0,0,0,0.75)", display: "flex", alignItems: "center", justifyContent: "center", zIndex: 1000, padding: 16 }}>
          <div className="card" style={{ width: "100%", maxWidth: 480, padding: 24, borderRadius: 12 }}>
            <h3 style={{ fontSize: "1.2rem", fontWeight: 800, marginBottom: 12 }}>
              Generate Barcode Bundles for {selectedOrderForBundles.cuttingOrderNumber}
            </h3>
            <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginBottom: 16 }}>
              Total Order Qty: <strong>{selectedOrderForBundles.totalRequiredQty} Pcs</strong>. Each bundle will have unique QR barcodes and serial sequences.
            </p>

            <div style={{ marginBottom: 20 }}>
              <label style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Bundle Size (Pcs per Bundle)</label>
              <input
                type="number"
                value={bundleSize}
                onChange={(e) => setBundleSize(parseInt(e.target.value) || 20)}
                style={{ width: "100%", padding: "8px 12px", background: "rgba(255,255,255,0.05)", border: "1px solid var(--border-color)", borderRadius: 6, color: "var(--text-primary)", marginTop: 4 }}
              />
              <span style={{ fontSize: "0.75rem", color: "var(--color-primary-light)", marginTop: 4, display: "block" }}>
                Will create {Math.ceil(selectedOrderForBundles.totalRequiredQty / bundleSize)} Barcode Bundle Tickets
              </span>
            </div>

            <div style={{ display: "flex", justifyContent: "flex-end", gap: 10 }}>
              <button className="btn btn-secondary" onClick={() => setShowBundleModal(false)}>
                Close
              </button>
              <button className="btn btn-primary" onClick={() => handleGenerateBundles(selectedOrderForBundles.id)}>
                <Barcode size={16} />
                <span>Generate & Print Tickets</span>
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}

