"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiRequest } from "@/lib/api";
import { 
  Plus, Search, Filter, Download, ShoppingCart, Truck, CheckCircle2, 
  AlertCircle, Building2, PackageCheck, FileText, ArrowRight, X, Clock, RefreshCw
} from "lucide-react";

interface PurchaseOrder {
  id: string;
  poNumber: string;
  supplierId: string;
  supplierName: string;
  orderDate: string;
  expectedDeliveryDate: string;
  status: number;
  statusName: string;
  totalAmount: number;
  notes?: string;
  items: Array<{
    id: string;
    itemCode: string;
    description: string;
    categoryName: string;
    uomName: string;
    quantity: number;
    unitPrice: number;
    totalPrice: number;
    receivedQuantity: number;
  }>;
}

interface Supplier {
  id: string;
  code: string;
  name: string;
  email: string;
  phone: string;
  address: string;
  categoryName: string;
  rating: number;
  paymentTermsDays: number;
  isActive: boolean;
}

interface GRN {
  id: string;
  grnNumber: string;
  poNumber: string;
  supplierName: string;
  receivedDate: string;
  deliveryChallanNo: string;
  statusName: string;
  items: Array<{
    itemCode: string;
    description: string;
    receivedQuantity: number;
    acceptedQuantity: number;
    rejectedQuantity: number;
  }>;
}

export default function ProcurementPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<"orders" | "suppliers" | "grn">("orders");
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [grns, setGrns] = useState<GRN[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("ALL");
  const [selectedPo, setSelectedPo] = useState<PurchaseOrder | null>(null);
  const [showNewPoModal, setShowNewPoModal] = useState(false);
  const [showNewSupplierModal, setShowNewSupplierModal] = useState(false);
  const [submitting, setSubmitting] = useState(false);

  // New PO Form State
  const [selectedSupplierId, setSelectedSupplierId] = useState("");
  const [expectedDeliveryDate, setExpectedDeliveryDate] = useState("");
  const [poNotes, setPoNotes] = useState("");
  const [poItems, setPoItems] = useState([
    { itemCode: "FAB-CTN-101", description: "100% Combed Cotton Single Jersey 180 GSM", categoryName: "Fabric", uomName: "Kgs", quantity: 5000, unitPrice: 4.80 }
  ]);

  // New Supplier Form State
  const [supName, setSupName] = useState("");
  const [supCode, setSupCode] = useState("");
  const [supEmail, setSupEmail] = useState("");
  const [supPhone, setSupPhone] = useState("");
  const [supCategory, setSupCategory] = useState("Fabric");
  const [supTerms, setSupTerms] = useState("30");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [posRes, supsRes, grnsRes] = await Promise.all([
        apiRequest<PurchaseOrder[]>("/api/procurement/orders"),
        apiRequest<Supplier[]>("/api/procurement/suppliers"),
        apiRequest<GRN[]>("/api/procurement/grn")
      ]);
      if (posRes.data && posRes.data.length > 0) {
        setOrders(posRes.data);
      } else {
        setOrders([
          {
            id: "po-001",
            poNumber: "PO-2026-0145",
            supplierId: "sup-001",
            supplierName: "Apex Textile Mills Ltd.",
            orderDate: "2026-10-01",
            expectedDeliveryDate: "2026-10-10",
            status: 2,
            statusName: "Approved",
            totalAmount: 86362.50,
            notes: "Ref Style: ST-2026-CHINO-01 (Nordic Fashion Retail). Pre-Budget Cap: $2.40/Yd. Actual: $2.35/Yd (Under Budget).",
            items: [
              {
                id: "poi-001",
                itemCode: "FAB-TW-01",
                description: "98% Cotton 2% Spandex Twill 260 GSM (Navy/Khaki/Olive)",
                categoryName: "Fabric",
                uomName: "Yards",
                quantity: 36750,
                unitPrice: 2.35,
                totalPrice: 86362.50,
                receivedQuantity: 36750
              }
            ]
          },
          {
            id: "po-002",
            poNumber: "PO-2026-LOCK-01",
            supplierId: "sup-002",
            supplierName: "Square Fashions Ltd.",
            orderDate: "2026-10-02",
            expectedDeliveryDate: "2026-10-15",
            status: 6,
            statusName: "Locked: Budget Exceeded",
            totalAmount: 24500.00,
            notes: "HARD LOCK: Unit price $2.45/Yd exceeds pre-costing budget of $2.40/Yd. Requires CFO digital override.",
            items: [
              {
                id: "poi-002",
                itemCode: "FAB-TW-01",
                description: "Cotton Spandex Twill 260 GSM",
                categoryName: "Fabric",
                uomName: "Yards",
                quantity: 10000,
                unitPrice: 2.45,
                totalPrice: 24500.00,
                receivedQuantity: 0
              }
            ]
          }
        ]);
      }

      if (supsRes.data && supsRes.data.length > 0) {
        setSuppliers(supsRes.data);
        if (!selectedSupplierId) setSelectedSupplierId(supsRes.data[0].id);
      } else {
        const defaultSups = [
          {
            id: "sup-001",
            code: "SUP-TEX-004",
            name: "Apex Textile Mills Ltd.",
            email: "sales@apextextiles.com",
            phone: "+880 2-9884512",
            address: "Plot 12, Gazipur Industrial Area, Dhaka",
            categoryName: "Fabric",
            rating: 4.85,
            paymentTermsDays: 90,
            isActive: true
          },
          {
            id: "sup-002",
            code: "SUP-TEX-007",
            name: "Square Fashions Ltd.",
            email: "yarn@squaregroup.com",
            phone: "+880 2-8833441",
            address: "Square Centre, Uttara, Dhaka",
            categoryName: "Fabric",
            rating: 4.90,
            paymentTermsDays: 60,
            isActive: true
          },
          {
            id: "sup-003",
            code: "SUP-TRM-012",
            name: "YKK Bangladesh Ltd.",
            email: "orders@ykk.com.bd",
            phone: "+880 2-7789011",
            address: "DEPZ, Savar, Dhaka",
            categoryName: "Trims",
            rating: 4.95,
            paymentTermsDays: 30,
            isActive: true
          }
        ];
        setSuppliers(defaultSups);
        if (!selectedSupplierId) setSelectedSupplierId(defaultSups[0].id);
      }

      if (grnsRes.data && grnsRes.data.length > 0) {
        setGrns(grnsRes.data);
      } else {
        setGrns([
          {
            id: "grn-001",
            grnNumber: "GRN-2026-0210",
            poNumber: "PO-2026-0145",
            supplierName: "Apex Textile Mills Ltd.",
            receivedDate: "2026-10-09",
            deliveryChallanNo: "CH-99412",
            statusName: "Quarantine / Accepted",
            items: [
              {
                itemCode: "FAB-TW-01",
                description: "98/2 Cotton Spandex Twill (368 Rolls)",
                receivedQuantity: 36750,
                acceptedQuantity: 36750,
                rejectedQuantity: 0
              }
            ]
          }
        ]);
      }
    } catch (err) {
      console.error("Failed to load procurement data", err);
    } finally {
      setLoading(false);
    }
  };

  const handleCreatePo = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedSupplierId || !expectedDeliveryDate) return;

    setSubmitting(true);
    try {
      const payload = {
        supplierId: selectedSupplierId,
        orderDate: new Date().toISOString(),
        expectedDeliveryDate: new Date(expectedDeliveryDate).toISOString(),
        notes: poNotes,
        items: poItems.map(item => ({
          itemCode: item.itemCode,
          description: item.description,
          categoryName: item.categoryName,
          uomName: item.uomName,
          quantity: Number(item.quantity),
          unitPrice: Number(item.unitPrice)
        }))
      };

      const res = await apiRequest<PurchaseOrder>("/api/procurement/orders", {
        method: "POST",
        body: payload
      });

      if (res.data) {
        setShowNewPoModal(false);
        setPoNotes("");
        loadData();
      }
    } catch (err) {
      console.error("Error creating PO", err);
    } finally {
      setSubmitting(false);
    }
  };

  const handleApprovePo = async (poId: string) => {
    try {
      await apiRequest(`/api/procurement/orders/${poId}/approve`, {
        method: "POST",
        body: { approverId: user?.id }
      });
      loadData();
      if (selectedPo?.id === poId) setSelectedPo(null);
    } catch (err) {
      console.error("Failed to approve PO", err);
    }
  };

  const handleCreateSupplier = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const payload = {
        name: supName,
        code: supCode,
        email: supEmail,
        phone: supPhone,
        categoryName: supCategory,
        paymentTermsDays: parseInt(supTerms) || 30
      };

      const res = await apiRequest<Supplier>("/api/procurement/suppliers", {
        method: "POST",
        body: payload
      });

      if (res.data) {
        setShowNewSupplierModal(false);
        setSupName("");
        setSupCode("");
        setSupEmail("");
        setSupPhone("");
        loadData();
      }
    } catch (err) {
      console.error("Error creating supplier", err);
    } finally {
      setSubmitting(false);
    }
  };

  const filteredOrders = orders.filter(po => {
    const matchesSearch = po.poNumber.toLowerCase().includes(searchQuery.toLowerCase()) ||
                          po.supplierName.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === "ALL" || po.statusName.toUpperCase() === statusFilter.toUpperCase();
    return matchesSearch && matchesStatus;
  });

  const totalSpend = orders.reduce((sum, o) => sum + o.totalAmount, 0);
  const activePos = orders.filter(o => o.statusName === "Approved" || o.statusName === "Partially Received").length;
  const completedPos = orders.filter(o => o.statusName === "Received" || o.statusName === "Completed").length;

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
      {/* Header & Actions */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
        <div>
          <h1 style={{ fontSize: "1.5rem", fontWeight: 700, color: "#0f172a", letterSpacing: "-0.02em" }}>Procurement & Sourcing</h1>
          <p style={{ fontSize: "0.875rem", color: "#64748b", marginTop: "0.25rem" }}>Manage vendor catalog, purchase orders, and goods receiving notes</p>
        </div>
        <div style={{ display: "flex", gap: "0.5rem" }}>
          <button className="btn btn-outline" onClick={loadData}>
            <RefreshCw size={16} className={loading ? "animate-spin" : ""} />
            <span>Refresh</span>
          </button>
          <button className="btn btn-secondary" onClick={() => setShowNewSupplierModal(true)}>
            <Building2 size={16} />
            <span>Add Supplier</span>
          </button>
          <button className="btn btn-primary" onClick={() => setShowNewPoModal(true)}>
            <Plus size={16} />
            <span>Create Purchase Order</span>
          </button>
        </div>
      </div>

      {/* KPI Cards (Shadcn Light Design with clean vertical stack) */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Total Procurement Value</span>
            <div className="kpi-icon-wrap blue">
              <ShoppingCart size={16} />
            </div>
          </div>
          <div className="kpi-value">${totalSpend.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</div>
          <div className="kpi-meta trend-up">
            <CheckCircle2 size={12} />
            <span>Across {orders.length} Purchase Orders</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Active Orders in Pipeline</span>
            <div className="kpi-icon-wrap amber">
              <Clock size={16} />
            </div>
          </div>
          <div className="kpi-value">{activePos}</div>
          <div className="kpi-meta">
            <span>Approved & Pending Delivery</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Completed & Received</span>
            <div className="kpi-icon-wrap emerald">
              <PackageCheck size={16} />
            </div>
          </div>
          <div className="kpi-value">{completedPos}</div>
          <div className="kpi-meta trend-up">
            <span>Goods Inspected & Stocked</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Verified Suppliers</span>
            <div className="kpi-icon-wrap purple">
              <Building2 size={16} />
            </div>
          </div>
          <div className="kpi-value">{suppliers.length}</div>
          <div className="kpi-meta">
            <span>{suppliers.filter(s => s.isActive).length} Active Vendors</span>
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px solid #e2e8f0", paddingBottom: "0.75rem", flexWrap: "wrap", gap: "1rem" }}>
        <div className="tabs-list">
          <button 
            className={`tab-trigger ${activeTab === "orders" ? "active" : ""}`}
            onClick={() => setActiveTab("orders")}>
            Purchase Orders ({orders.length})
          </button>
          <button 
            className={`tab-trigger ${activeTab === "suppliers" ? "active" : ""}`}
            onClick={() => setActiveTab("suppliers")}>
            Supplier Directory ({suppliers.length})
          </button>
          <button 
            className={`tab-trigger ${activeTab === "grn" ? "active" : ""}`}
            onClick={() => setActiveTab("grn")}>
            Goods Receiving Notes ({grns.length})
          </button>
        </div>

        {activeTab === "orders" && (
          <div style={{ display: "flex", gap: "0.5rem", alignItems: "center" }}>
            <div className="search-input-wrap">
              <Search size={16} />
              <input 
                type="text" 
                className="form-input" 
                placeholder="Search PO or supplier..." 
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
            <select 
              className="form-select" 
              style={{ width: "160px" }}
              value={statusFilter}
              onChange={(e) => setStatusFilter(e.target.value)}>
              <option value="ALL">All Statuses</option>
              <option value="DRAFT">Draft</option>
              <option value="SUBMITTED">Submitted</option>
              <option value="APPROVED">Approved</option>
              <option value="RECEIVED">Received</option>
            </select>
          </div>
        )}
      </div>

      {/* Tab 1: Orders */}
      {activeTab === "orders" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>PO Number</th>
                  <th>Supplier Name</th>
                  <th>Order Date</th>
                  <th>Expected Delivery</th>
                  <th>Items Qty</th>
                  <th>Total Amount</th>
                  <th>Status</th>
                  <th style={{ textAlign: "right" }}>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredOrders.length === 0 ? (
                  <tr>
                    <td colSpan={8} style={{ textAlign: "center", padding: "2.5rem 1rem", color: "#64748b" }}>
                      No purchase orders found. Click &quot;Create Purchase Order&quot; to generate one.
                    </td>
                  </tr>
                ) : (
                  filteredOrders.map(po => {
                    const totalItems = po.items.reduce((sum, it) => sum + it.quantity, 0);
                    return (
                      <tr key={po.id}>
                        <td style={{ fontWeight: 600, color: "#2563eb" }}>{po.poNumber}</td>
                        <td style={{ fontWeight: 500 }}>{po.supplierName}</td>
                        <td>{new Date(po.orderDate).toLocaleDateString()}</td>
                        <td>{new Date(po.expectedDeliveryDate).toLocaleDateString()}</td>
                        <td>{totalItems.toLocaleString()} units ({po.items.length} items)</td>
                        <td style={{ fontWeight: 600 }}>${po.totalAmount.toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</td>
                        <td>
                          <span style={{
                            display: "inline-flex",
                            alignItems: "center",
                            gap: "0.25rem",
                            padding: "0.25rem 0.625rem",
                            borderRadius: "9999px",
                            fontSize: "0.75rem",
                            fontWeight: 600,
                            backgroundColor: po.status === 6 || po.statusName.includes("Locked") ? "#fee2e2" :
                                             po.statusName === "Approved" ? "#dcfce7" :
                                             po.statusName === "Received" ? "#e0f2fe" :
                                             po.statusName === "Partially Received" ? "#fef3c7" : "#f1f5f9",
                            color: po.status === 6 || po.statusName.includes("Locked") ? "#b91c1c" :
                                   po.statusName === "Approved" ? "#15803d" :
                                   po.statusName === "Received" ? "#0369a1" :
                                   po.statusName === "Partially Received" ? "#b45309" : "#475569",
                            border: `1px solid ${
                              po.status === 6 || po.statusName.includes("Locked") ? "#fca5a5" :
                              po.statusName === "Approved" ? "#86efac" :
                              po.statusName === "Received" ? "#7dd3fc" :
                              po.statusName === "Partially Received" ? "#fde68a" : "#cbd5e1"
                            }`
                          }}>
                            {(po.status === 6 || po.statusName.includes("Locked")) && <AlertCircle size={12} />}
                            {po.statusName}
                          </span>
                        </td>
                        <td style={{ textAlign: "right" }}>
                          <button 
                            className="btn btn-outline btn-sm"
                            onClick={() => setSelectedPo(po)}>
                            <span>View Details</span>
                          </button>
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

      {/* Tab 2: Suppliers */}
      {activeTab === "suppliers" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Vendor Code</th>
                  <th>Supplier Name</th>
                  <th>Category</th>
                  <th>Contact Email</th>
                  <th>Phone Number</th>
                  <th>Payment Terms</th>
                  <th>Rating</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {suppliers.map(sup => (
                  <tr key={sup.id}>
                    <td style={{ fontWeight: 600, color: "#0f172a" }}>{sup.code}</td>
                    <td style={{ fontWeight: 500 }}>{sup.name}</td>
                    <td><span className="badge badge-secondary">{sup.categoryName}</span></td>
                    <td>{sup.email}</td>
                    <td>{sup.phone}</td>
                    <td>{sup.paymentTermsDays} Days Net</td>
                    <td>
                      <span style={{ color: "#d97706", fontWeight: 600 }}>★ {sup.rating.toFixed(1)}</span>
                    </td>
                    <td>
                      <span className={`badge ${sup.isActive ? "badge-success" : "badge-secondary"}`}>
                        {sup.isActive ? "Active" : "Inactive"}
                      </span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 3: GRN */}
      {activeTab === "grn" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>GRN Number</th>
                  <th>PO Reference</th>
                  <th>Supplier</th>
                  <th>Received Date</th>
                  <th>Challan No</th>
                  <th>Status</th>
                  <th>Accepted Items</th>
                </tr>
              </thead>
              <tbody>
                {grns.map(g => (
                  <tr key={g.id}>
                    <td style={{ fontWeight: 600, color: "#2563eb" }}>{g.grnNumber}</td>
                    <td style={{ fontWeight: 500 }}>{g.poNumber}</td>
                    <td>{g.supplierName}</td>
                    <td>{new Date(g.receivedDate).toLocaleDateString()}</td>
                    <td>{g.deliveryChallanNo}</td>
                    <td><span className="badge badge-success">{g.statusName}</span></td>
                    <td style={{ fontSize: "0.8125rem" }}>
                      {g.items.map((it, idx) => (
                        <div key={idx}>
                          {it.itemCode}: <b>{it.acceptedQuantity}</b> accepted {it.rejectedQuantity > 0 && <span style={{ color: "#ef4444" }}>({it.rejectedQuantity} rejected)</span>}
                        </div>
                      ))}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* View PO Details Modal (Solid Shadcn Modal) */}
      {selectedPo && (
        <div className="modal-backdrop">
          <div className="modal-dialog modal-lg">
            <div className="modal-header">
              <div>
                <h3 className="modal-title">Purchase Order Details: {selectedPo.poNumber}</h3>
                <p style={{ fontSize: "0.8125rem", color: "#64748b", marginTop: "0.125rem" }}>Supplier: {selectedPo.supplierName}</p>
              </div>
              <button className="btn btn-outline btn-icon btn-sm" onClick={() => setSelectedPo(null)}>
                <X size={16} />
              </button>
            </div>

            <div className="modal-body">
              {selectedPo.status === 6 && (
                <div style={{
                  marginBottom: "1rem",
                  padding: "0.875rem 1rem",
                  backgroundColor: "#fef2f2",
                  border: "1px solid #fca5a5",
                  borderRadius: "0.5rem",
                  color: "#991b1b",
                  fontSize: "0.875rem",
                  display: "flex",
                  gap: "0.75rem",
                  alignItems: "center"
                }}>
                  <AlertCircle size={20} style={{ flexShrink: 0 }} />
                  <div>
                    <strong style={{ display: "block", marginBottom: "0.25rem" }}>AC-03 Pre-Budget Hard Lock Active</strong>
                    Proposed item unit price exceeds the Style pre-costing threshold. PO approval is blocked. CFO digital authorization required to override.
                  </div>
                </div>
              )}

              <div className="form-grid-2" style={{ marginBottom: "1.25rem", fontSize: "0.875rem", background: "#f8fafc", padding: "1rem", borderRadius: "0.5rem", border: "1px solid #e2e8f0" }}>
                <div><span style={{ color: "#64748b" }}>Order Date:</span> <b>{new Date(selectedPo.orderDate).toLocaleDateString()}</b></div>
                <div><span style={{ color: "#64748b" }}>Expected Delivery:</span> <b>{new Date(selectedPo.expectedDeliveryDate).toLocaleDateString()}</b></div>
                <div><span style={{ color: "#64748b" }}>Status:</span> 
                  <span style={{
                    display: "inline-flex",
                    alignItems: "center",
                    gap: "0.25rem",
                    marginLeft: "0.5rem",
                    padding: "0.15rem 0.5rem",
                    borderRadius: "9999px",
                    fontSize: "0.75rem",
                    fontWeight: 600,
                    backgroundColor: selectedPo.status === 6 ? "#fee2e2" : selectedPo.statusName === "Approved" ? "#dcfce7" : "#e0f2fe",
                    color: selectedPo.status === 6 ? "#b91c1c" : selectedPo.statusName === "Approved" ? "#15803d" : "#0369a1",
                    border: `1px solid ${selectedPo.status === 6 ? "#fca5a5" : selectedPo.statusName === "Approved" ? "#86efac" : "#7dd3fc"}`
                  }}>
                    {selectedPo.status === 6 && <AlertCircle size={12} />}
                    {selectedPo.statusName}
                  </span>
                </div>
                <div><span style={{ color: "#64748b" }}>Total Value:</span> <b>${selectedPo.totalAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}</b></div>
                {selectedPo.notes && <div style={{ gridColumn: "1 / -1" }}><span style={{ color: "#64748b" }}>Notes:</span> {selectedPo.notes}</div>}
              </div>

              <h4 style={{ fontSize: "0.875rem", fontWeight: 600, color: "#0f172a", marginBottom: "0.75rem" }}>Order Line Items</h4>
              <div className="table-container" style={{ marginBottom: "1rem" }}>
                <table className="table">
                  <thead>
                    <tr>
                      <th>Item Code</th>
                      <th>Description</th>
                      <th>Qty</th>
                      <th>Unit Price</th>
                      <th>Total</th>
                      <th>Received</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedPo.items.map((i) => (
                      <tr key={i.id}>
                        <td style={{ fontWeight: 600 }}>{i.itemCode}</td>
                        <td>{i.description}</td>
                        <td>{i.quantity.toLocaleString()} {i.uomName}</td>
                        <td>${i.unitPrice.toFixed(2)}</td>
                        <td style={{ fontWeight: 700 }}>${i.totalPrice.toFixed(2)}</td>
                        <td style={{ color: i.receivedQuantity > 0 ? "#059669" : "#64748b" }}>{i.receivedQuantity.toLocaleString()}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="modal-footer">
              {selectedPo.status === 6 && (
                <button 
                  className="btn btn-primary" 
                  style={{ backgroundColor: "#dc2626", borderColor: "#dc2626" }}
                  onClick={() => alert("CFO Budget Override Authorization: Enter CFO PIN to unlock PO.")}>
                  <AlertCircle size={16} />
                  <span>Request CFO Budget Override</span>
                </button>
              )}
              {(selectedPo.statusName === "Draft" || selectedPo.statusName === "Submitted") && (
                <button className="btn btn-primary" onClick={() => handleApprovePo(selectedPo.id)}>
                  <CheckCircle2 size={16} />
                  <span>Approve Purchase Order</span>
                </button>
              )}
              <button className="btn btn-secondary" onClick={() => setSelectedPo(null)}>
                <span>Close</span>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Create New PO Modal */}
      {showNewPoModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog modal-lg">
            <div className="modal-header">
              <h3 className="modal-title">Create New Purchase Order</h3>
              <button className="btn btn-outline btn-icon btn-sm" onClick={() => setShowNewPoModal(false)}>
                <X size={16} />
              </button>
            </div>

            <form onSubmit={handleCreatePo}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">Select Supplier *</label>
                    <select 
                      className="form-select"
                      value={selectedSupplierId} 
                      onChange={(e) => setSelectedSupplierId(e.target.value)} 
                      required>
                      {suppliers.map((s) => (
                        <option key={s.id} value={s.id}>{s.name} ({s.code})</option>
                      ))}
                    </select>
                  </div>

                  <div className="form-group">
                    <label className="form-label">Expected Delivery Date *</label>
                    <input 
                      type="date" 
                      className="form-input"
                      value={expectedDeliveryDate} 
                      onChange={(e) => setExpectedDeliveryDate(e.target.value)} 
                      required 
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label className="form-label">Remarks & Order Notes</label>
                  <input 
                    type="text" 
                    className="form-input"
                    placeholder="e.g. Export order fabric procurement"
                    value={poNotes} 
                    onChange={(e) => setPoNotes(e.target.value)} 
                  />
                </div>

                <h4 style={{ fontSize: "0.875rem", fontWeight: 600, color: "#0f172a", margin: "1rem 0 0.5rem" }}>Order Item Details</h4>
                <div style={{ display: "grid", gridTemplateColumns: "1.5fr 2fr 1fr 1fr", gap: "0.75rem", background: "#f8fafc", padding: "1rem", borderRadius: "0.5rem", border: "1px solid #e2e8f0" }}>
                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label" style={{ fontSize: "0.75rem" }}>Item Code</label>
                    <input 
                      type="text" 
                      className="form-input"
                      value={poItems[0].itemCode} 
                      onChange={(e) => setPoItems([{ ...poItems[0], itemCode: e.target.value }])}
                    />
                  </div>
                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label" style={{ fontSize: "0.75rem" }}>Description</label>
                    <input 
                      type="text" 
                      className="form-input"
                      value={poItems[0].description} 
                      onChange={(e) => setPoItems([{ ...poItems[0], description: e.target.value }])}
                    />
                  </div>
                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label" style={{ fontSize: "0.75rem" }}>Quantity (Kgs)</label>
                    <input 
                      type="number" 
                      className="form-input"
                      value={poItems[0].quantity} 
                      onChange={(e) => setPoItems([{ ...poItems[0], quantity: parseFloat(e.target.value) || 0 }])}
                    />
                  </div>
                  <div className="form-group" style={{ marginBottom: 0 }}>
                    <label className="form-label" style={{ fontSize: "0.75rem" }}>Unit Price ($)</label>
                    <input 
                      type="number" 
                      step="0.01" 
                      className="form-input"
                      value={poItems[0].unitPrice} 
                      onChange={(e) => setPoItems([{ ...poItems[0], unitPrice: parseFloat(e.target.value) || 0 }])}
                    />
                  </div>
                </div>

                <div style={{ display: "flex", justifyContent: "flex-end", marginTop: "1rem", fontSize: "0.9375rem" }}>
                  <span>Total Order Estimate: <b>${(poItems[0].quantity * poItems[0].unitPrice).toLocaleString(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 })}</b></span>
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowNewPoModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? "Creating..." : "Create Purchase Order"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Create New Supplier Modal */}
      {showNewSupplierModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog">
            <div className="modal-header">
              <h3 className="modal-title">Register New Supplier</h3>
              <button className="btn btn-outline btn-icon btn-sm" onClick={() => setShowNewSupplierModal(false)}>
                <X size={16} />
              </button>
            </div>

            <form onSubmit={handleCreateSupplier}>
              <div className="modal-body">
                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">Vendor Code *</label>
                    <input 
                      type="text" 
                      className="form-input"
                      placeholder="e.g. SUP-005"
                      value={supCode} 
                      onChange={(e) => setSupCode(e.target.value)} 
                      required 
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Supplier Name *</label>
                    <input 
                      type="text" 
                      className="form-input"
                      placeholder="e.g. Pacific Mills Ltd."
                      value={supName} 
                      onChange={(e) => setSupName(e.target.value)} 
                      required 
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">Email Address *</label>
                    <input 
                      type="email" 
                      className="form-input"
                      placeholder="sales@vendor.com"
                      value={supEmail} 
                      onChange={(e) => setSupEmail(e.target.value)} 
                      required 
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Phone Number</label>
                    <input 
                      type="text" 
                      className="form-input"
                      placeholder="+880 1711-000000"
                      value={supPhone} 
                      onChange={(e) => setSupPhone(e.target.value)} 
                    />
                  </div>
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">Category</label>
                    <select 
                      className="form-select"
                      value={supCategory} 
                      onChange={(e) => setSupCategory(e.target.value)}>
                      <option value="Fabric">Fabric Mill</option>
                      <option value="Yarn">Yarn Spinner</option>
                      <option value="Accessories">Accessories & Trims</option>
                      <option value="Chemicals">Dyes & Chemicals</option>
                    </select>
                  </div>
                  <div className="form-group">
                    <label className="form-label">Payment Terms (Days)</label>
                    <input 
                      type="number" 
                      className="form-input"
                      value={supTerms} 
                      onChange={(e) => setSupTerms(e.target.value)} 
                    />
                  </div>
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowNewSupplierModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? "Registering..." : "Save Supplier"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
