"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiRequest } from "@/lib/api";
import { 
  Warehouse, Package, AlertTriangle, ArrowUpRight, ArrowDownRight,
  Layers, Scissors, RefreshCw, Search, Filter, Plus, CheckCircle2, X
} from "lucide-react";

interface Item {
  id: string;
  code: string;
  name: string;
  categoryName: string;
  uomName: string;
  reorderLevel: number;
  currentStock: number;
  averageUnitCost: number;
  totalValue: number;
  isLowStock: boolean;
}

interface FabricRoll {
  id: string;
  itemCode: string;
  barcode: string;
  shade: string;
  dyeLotNumber: string;
  originalLengthYards: number;
  availableLengthYards: number;
  widthInches: number;
  statusName: string;
}

interface StockTransaction {
  id: string;
  itemCode: string;
  itemName: string;
  transactionTypeName: string;
  quantity: number;
  referenceType: string;
  referenceNumber: string;
  createdAt: string;
}

export default function InventoryPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<"items" | "rolls" | "transactions">("items");
  const [items, setItems] = useState<Item[]>([]);
  const [rolls, setRolls] = useState<FabricRoll[]>([]);
  const [transactions, setTransactions] = useState<StockTransaction[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [categoryFilter, setCategoryFilter] = useState("ALL");

  // Modals
  const [selectedItem, setSelectedItem] = useState<Item | null>(null);
  const [showAdjustModal, setShowAdjustModal] = useState(false);
  const [adjustQty, setAdjustQty] = useState(0);
  const [adjustReason, setAdjustReason] = useState("Physical Count Verification");

  const [selectedRoll, setSelectedRoll] = useState<FabricRoll | null>(null);
  const [showIssueRollModal, setShowIssueRollModal] = useState(false);
  const [issueYards, setIssueYards] = useState(0);
  const [cuttingRef, setCuttingRef] = useState("");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [itemsRes, rollsRes, txRes] = await Promise.all([
        apiRequest<Item[]>("/api/inventory/items"),
        apiRequest<FabricRoll[]>("/api/inventory/fabric-rolls"),
        apiRequest<StockTransaction[]>("/api/inventory/transactions")
      ]);
      if (itemsRes.data) setItems(itemsRes.data);
      if (rollsRes.data) setRolls(rollsRes.data);
      if (txRes.data) setTransactions(txRes.data);
    } catch (err) {
      console.error("Failed to load inventory data", err);
    } finally {
      setLoading(false);
    }
  };

  const handleAdjustStock = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedItem) return;
    try {
      await apiRequest(`/api/inventory/items/${selectedItem.id}/adjust`, {
        method: "POST",
        body: {
          newQuantity: adjustQty,
          reason: adjustReason,
          adjustedBy: user?.id
        }
      });
      setShowAdjustModal(false);
      loadData();
    } catch (err) {
      console.error("Failed to adjust stock", err);
    }
  };

  const handleIssueFabricRoll = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedRoll) return;
    try {
      await apiRequest(`/api/inventory/fabric-rolls/${selectedRoll.id}/issue`, {
        method: "POST",
        body: {
          yardsToIssue: issueYards,
          cuttingOrderRef: cuttingRef,
          issuedBy: user?.id
        }
      });
      setShowIssueRollModal(false);
      loadData();
    } catch (err) {
      console.error("Failed to issue fabric roll", err);
    }
  };

  const totalStockValue = items.reduce((sum, item) => sum + item.totalValue, 0);
  const lowStockCount = items.filter(i => i.isLowStock).length;
  const totalFabricYards = rolls.reduce((sum, r) => sum + r.availableLengthYards, 0);

  const filteredItems = items.filter(item => {
    const matchesSearch = item.name.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          item.code.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesCat = categoryFilter === "ALL" || item.categoryName === categoryFilter;
    return matchesSearch && matchesCat;
  });

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
      {/* Header */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
        <div>
          <h1 style={{ fontSize: "1.5rem", fontWeight: 700, color: "#0f172a", letterSpacing: "-0.02em" }}>Warehouse & Raw Material Inventory</h1>
          <p style={{ fontSize: "0.875rem", color: "#64748b", marginTop: "0.25rem" }}>Live stock tracking, fabric roll barcodes, and bin location movements</p>
        </div>
        <div style={{ display: "flex", gap: "0.5rem" }}>
          <button className="btn btn-outline" onClick={loadData}>
            <RefreshCw size={16} className={loading ? "animate-spin" : ""} />
            <span>Refresh</span>
          </button>
        </div>
      </div>

      {/* KPI Cards (Shadcn Light Design) */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Total Inventory Valuation</span>
            <div className="kpi-icon-wrap blue">
              <Warehouse size={16} />
            </div>
          </div>
          <div className="kpi-value">${totalStockValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
          <div className="kpi-meta trend-up">
            <CheckCircle2 size={12} />
            <span>Across {items.length} Active SKUs</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Fabric Rolls in Warehouse</span>
            <div className="kpi-icon-wrap purple">
              <Layers size={16} />
            </div>
          </div>
          <div className="kpi-value">{rolls.length} Rolls</div>
          <div className="kpi-meta">
            <span>{totalFabricYards.toLocaleString()} Total Available Yds</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Low Stock Alerts</span>
            <div className="kpi-icon-wrap amber">
              <AlertTriangle size={16} />
            </div>
          </div>
          <div className="kpi-value" style={{ color: lowStockCount > 0 ? "#ef4444" : "#0f172a" }}>{lowStockCount}</div>
          <div className="kpi-meta">
            <span>Below Reorder Threshold</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Stock Movements Logged</span>
            <div className="kpi-icon-wrap emerald">
              <Package size={16} />
            </div>
          </div>
          <div className="kpi-value">{transactions.length}</div>
          <div className="kpi-meta">
            <span>Receipts & Issues</span>
          </div>
        </div>
      </div>

      {/* Tabs & Filter Bar */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px solid #e2e8f0", paddingBottom: "0.75rem", flexWrap: "wrap", gap: "1rem" }}>
        <div className="tabs-list">
          <button 
            className={`tab-trigger ${activeTab === "items" ? "active" : ""}`}
            onClick={() => setActiveTab("items")}>
            Raw Material SKUs ({items.length})
          </button>
          <button 
            className={`tab-trigger ${activeTab === "rolls" ? "active" : ""}`}
            onClick={() => setActiveTab("rolls")}>
            Fabric Rolls & Dye Lots ({rolls.length})
          </button>
          <button 
            className={`tab-trigger ${activeTab === "transactions" ? "active" : ""}`}
            onClick={() => setActiveTab("transactions")}>
            Ledger & Stock Movements ({transactions.length})
          </button>
        </div>

        {activeTab === "items" && (
          <div style={{ display: "flex", gap: "0.5rem", alignItems: "center" }}>
            <div className="search-input-wrap">
              <Search size={16} />
              <input 
                type="text" 
                className="form-input" 
                placeholder="Search SKU or item name..." 
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
            <select 
              className="form-select" 
              style={{ width: "160px" }}
              value={categoryFilter}
              onChange={(e) => setCategoryFilter(e.target.value)}>
              <option value="ALL">All Categories</option>
              <option value="Fabric">Fabric</option>
              <option value="Yarn">Yarn</option>
              <option value="Accessories">Accessories</option>
              <option value="Chemicals">Chemicals</option>
            </select>
          </div>
        )}
      </div>

      {/* Tab 1: Item SKUs */}
      {activeTab === "items" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Item Code</th>
                  <th>Item Description</th>
                  <th>Category</th>
                  <th>Current Stock</th>
                  <th>Avg Unit Cost</th>
                  <th>Total Valuation</th>
                  <th>Status</th>
                  <th style={{ textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredItems.map(item => (
                  <tr key={item.id}>
                    <td style={{ fontWeight: 600, color: "#2563eb" }}>{item.code}</td>
                    <td style={{ fontWeight: 500 }}>{item.name}</td>
                    <td><span className="badge badge-secondary">{item.categoryName}</span></td>
                    <td style={{ fontWeight: 600 }}>{item.currentStock.toLocaleString()} {item.uomName}</td>
                    <td>${item.averageUnitCost.toFixed(2)}</td>
                    <td style={{ fontWeight: 600 }}>${item.totalValue.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                    <td>
                      {item.isLowStock ? (
                        <span className="badge badge-danger">Low Stock (Min {item.reorderLevel})</span>
                      ) : (
                        <span className="badge badge-success">Sufficient</span>
                      )}
                    </td>
                    <td style={{ textAlign: "right" }}>
                      <button 
                        className="btn btn-outline btn-sm"
                        onClick={() => {
                          setSelectedItem(item);
                          setAdjustQty(item.currentStock);
                          setShowAdjustModal(true);
                        }}>
                        <span>Adjust Stock</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 2: Fabric Rolls */}
      {activeTab === "rolls" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Roll Barcode</th>
                  <th>Fabric Item Code</th>
                  <th>Shade / Color</th>
                  <th>Dye Lot Number</th>
                  <th>Width</th>
                  <th>Available Yds</th>
                  <th>Status</th>
                  <th style={{ textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {rolls.map(roll => (
                  <tr key={roll.id}>
                    <td style={{ fontWeight: 600, color: "#0f172a" }}>{roll.barcode}</td>
                    <td style={{ fontWeight: 500 }}>{roll.itemCode}</td>
                    <td>{roll.shade}</td>
                    <td><span className="badge badge-secondary">{roll.dyeLotNumber}</span></td>
                    <td>{roll.widthInches}&quot;</td>
                    <td style={{ fontWeight: 600 }}>{roll.availableLengthYards.toLocaleString()} / {roll.originalLengthYards.toLocaleString()} Yds</td>
                    <td>
                      <span className={`badge ${
                        roll.statusName === "Available" ? "badge-success" :
                        roll.statusName === "In Use" ? "badge-warning" : "badge-secondary"
                      }`}>
                        {roll.statusName}
                      </span>
                    </td>
                    <td style={{ textAlign: "right" }}>
                      <button 
                        className="btn btn-outline btn-sm"
                        disabled={roll.availableLengthYards <= 0}
                        onClick={() => {
                          setSelectedRoll(roll);
                          setIssueYards(roll.availableLengthYards);
                          setShowIssueRollModal(true);
                        }}>
                        <Scissors size={14} />
                        <span>Issue to Cutting</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 3: Transactions */}
      {activeTab === "transactions" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Timestamp</th>
                  <th>Item Code</th>
                  <th>Item Description</th>
                  <th>Transaction Type</th>
                  <th>Quantity</th>
                  <th>Reference</th>
                </tr>
              </thead>
              <tbody>
                {transactions.map(tx => (
                  <tr key={tx.id}>
                    <td>{new Date(tx.createdAt).toLocaleString()}</td>
                    <td style={{ fontWeight: 600 }}>{tx.itemCode}</td>
                    <td>{tx.itemName}</td>
                    <td>
                      <span className={`badge ${
                        tx.transactionTypeName === "Receive" ? "badge-success" :
                        tx.transactionTypeName === "Issue" ? "badge-warning" : "badge-info"
                      }`}>
                        {tx.transactionTypeName}
                      </span>
                    </td>
                    <td style={{ fontWeight: 600, color: tx.quantity > 0 ? "#047857" : "#b91c1c" }}>
                      {tx.quantity > 0 ? `+${tx.quantity.toLocaleString()}` : tx.quantity.toLocaleString()}
                    </td>
                    <td>{tx.referenceType}: {tx.referenceNumber}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Adjust Stock Modal */}
      {showAdjustModal && selectedItem && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 500 }}>
            <div className="modal-header">
              <h3 className="modal-title">Adjust Stock Level</h3>
              <button className="btn btn-outline btn-icon btn-sm" onClick={() => setShowAdjustModal(false)}>
                <X size={16} />
              </button>
            </div>

            <form onSubmit={handleAdjustStock}>
              <div className="modal-body">
                <div style={{ background: "#f8fafc", padding: "1rem", borderRadius: "0.5rem", border: "1px solid #e2e8f0", marginBottom: "1rem", fontSize: "0.875rem" }}>
                  <div><span style={{ color: "#64748b" }}>SKU:</span> <b>{selectedItem.code}</b></div>
                  <div style={{ marginTop: "0.25rem" }}><span style={{ color: "#64748b" }}>Item Name:</span> <b>{selectedItem.name}</b></div>
                  <div style={{ marginTop: "0.25rem" }}><span style={{ color: "#64748b" }}>Current On-Hand:</span> <b>{selectedItem.currentStock.toLocaleString()} {selectedItem.uomName}</b></div>
                </div>

                <div className="form-group">
                  <label className="form-label">New Verified Quantity ({selectedItem.uomName}) *</label>
                  <input
                    type="number"
                    className="form-input"
                    value={adjustQty}
                    onChange={(e) => setAdjustQty(parseFloat(e.target.value) || 0)}
                    required
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Adjustment Reason *</label>
                  <input
                    type="text"
                    className="form-input"
                    value={adjustReason}
                    onChange={(e) => setAdjustReason(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowAdjustModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Save Stock Adjustment
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Issue Fabric Roll Modal */}
      {showIssueRollModal && selectedRoll && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 500 }}>
            <div className="modal-header">
              <h3 className="modal-title">Issue Fabric Roll to Cutting</h3>
              <button className="btn btn-outline btn-icon btn-sm" onClick={() => setShowIssueRollModal(false)}>
                <X size={16} />
              </button>
            </div>

            <form onSubmit={handleIssueFabricRoll}>
              <div className="modal-body">
                <div style={{ background: "#f8fafc", padding: "1rem", borderRadius: "0.5rem", border: "1px solid #e2e8f0", marginBottom: "1rem", fontSize: "0.875rem" }}>
                  <div><span style={{ color: "#64748b" }}>Barcode:</span> <b>{selectedRoll.barcode}</b></div>
                  <div style={{ marginTop: "0.25rem" }}><span style={{ color: "#64748b" }}>Dye Lot:</span> <b>{selectedRoll.dyeLotNumber}</b> (Shade: {selectedRoll.shade})</div>
                  <div style={{ marginTop: "0.25rem" }}><span style={{ color: "#64748b" }}>Available Yards:</span> <b>{selectedRoll.availableLengthYards.toLocaleString()} Yds</b></div>
                </div>

                <div className="form-group">
                  <label className="form-label">Yards to Issue *</label>
                  <input
                    type="number"
                    step="0.1"
                    max={selectedRoll.availableLengthYards}
                    min={1}
                    className="form-input"
                    value={issueYards}
                    onChange={(e) => setIssueYards(parseFloat(e.target.value) || 0)}
                    required
                  />
                </div>

                <div className="form-group">
                  <label className="form-label">Cutting Order Reference *</label>
                  <input
                    type="text"
                    placeholder="e.g. CUT-2026-001"
                    className="form-input"
                    value={cuttingRef}
                    onChange={(e) => setCuttingRef(e.target.value)}
                    required
                  />
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowIssueRollModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary">
                  Confirm Issue
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
