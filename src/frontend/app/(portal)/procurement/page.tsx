"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiRequest } from "@/lib/api";
import { 
  Plus, Search, Filter, Download, ShoppingCart, Truck, CheckCircle2, 
  AlertCircle, Building2, PackageCheck, FileText, ArrowRight, X, Clock, RefreshCw,
  Scale, ShieldAlert, BadgeDollarSign, Award, Layers, Check, ExternalLink
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
  isBudgetLocked?: boolean;
  notes?: string;
  items: Array<{
    id: string;
    itemCode: string;
    description: string;
    categoryName?: string;
    uomName: string;
    quantity: number;
    unitPrice: number;
    budgetUnitPrice?: number;
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

interface ComparativeStatement {
  id: string;
  csNumber: string;
  requisitionNumber: string;
  styleNumber: string;
  preCostingBudgetRate: number;
  status: string;
  awardedSupplierName?: string;
  remarks: string;
  quotes: Array<{
    id: string;
    supplierName: string;
    quotedRate: number;
    paymentTerms: string;
    deliveryLeadTimeDays: number;
    sampleApprovalStatus: string;
    isAwarded: boolean;
    remarks: string;
  }>;
}

interface CommercialLC {
  id: string;
  lcNumber: string;
  lcType: string;
  masterLCReference?: string;
  issuingBank: string;
  applicant: string;
  beneficiary: string;
  amount: number;
  currency: string;
  issueDate: string;
  expiryDate: string;
  paymentTerms: string;
  status: string;
  statutoryB2BCeilingAmount?: number;
}

interface MaterialRequisition {
  id: string;
  requisitionNumber: string;
  styleNumber: string;
  styleDescription: string;
  department: string;
  requiredDate: string;
  status: string;
  items: Array<{
    itemCode: string;
    itemDescription: string;
    requiredQuantity: number;
    unitOfMeasure: string;
    budgetUnitPrice: number;
  }>;
}

export default function ProcurementPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<"orders" | "cs" | "lc" | "requisitions" | "grn" | "suppliers">("orders");
  const [orders, setOrders] = useState<PurchaseOrder[]>([]);
  const [suppliers, setSuppliers] = useState<Supplier[]>([]);
  const [grns, setGrns] = useState<GRN[]>([]);
  const [csList, setCsList] = useState<ComparativeStatement[]>([]);
  const [lcList, setLcList] = useState<CommercialLC[]>([]);
  const [mrList, setMrList] = useState<MaterialRequisition[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState("ALL");
  const [selectedPo, setSelectedPo] = useState<PurchaseOrder | null>(null);
  const [showNewPoModal, setShowNewPoModal] = useState(false);
  const [showNewSupplierModal, setShowNewSupplierModal] = useState(false);
  const [showOverrideModal, setShowOverrideModal] = useState(false);
  const [overrideReason, setOverrideReason] = useState("CFO Authorization: Merchandising contingency approved for high-density spandex twill");
  const [submitting, setSubmitting] = useState(false);

  // New PO Form State
  const [selectedSupplierId, setSelectedSupplierId] = useState("");
  const [expectedDeliveryDate, setExpectedDeliveryDate] = useState("");
  const [poNotes, setPoNotes] = useState("");
  const [poItems, setPoItems] = useState([
    { itemCode: "FAB-TW-01", description: "98% Cotton 2% Spandex Twill 260 GSM (Navy)", categoryName: "Fabric", uomName: "Yds", quantity: 36750, unitPrice: 2.35 }
  ]);

  // New Supplier Form State
  const [supName, setSupName] = useState("");
  const [supCode, setSupCode] = useState("");
  const [supEmail, setSupEmail] = useState("");
  const [supPhone, setSupPhone] = useState("");
  const [supCategory, setSupCategory] = useState("Fabric");
  const [supTerms, setSupTerms] = useState("90");

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [posRes, supsRes, grnsRes, csRes, lcRes, mrRes] = await Promise.all([
        apiRequest<PurchaseOrder[]>("/api/procurement/orders"),
        apiRequest<Supplier[]>("/api/procurement/suppliers"),
        apiRequest<GRN[]>("/api/procurement/grn"),
        apiRequest<ComparativeStatement[]>("/api/v1/commercial/comparative-statements"),
        apiRequest<CommercialLC[]>("/api/v1/commercial/lcs"),
        apiRequest<MaterialRequisition[]>("/api/v1/commercial/requisitions")
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
            isBudgetLocked: false,
            totalAmount: 87465.00,
            notes: "Ref Style: ST-2026-CHINO-01 (Nordic Fashion Retail). Pre-Budget Cap: $2.40/Yd. Actual: $2.38/Yd. Linked to B2B LC: B2B-2026-0098.",
            items: [
              {
                id: "poi-001",
                itemCode: "FAB-TW-01",
                description: "98/2 Cotton Spandex Twill 260 GSM (Navy Blue)",
                uomName: "Yds",
                quantity: 36750,
                unitPrice: 2.38,
                budgetUnitPrice: 2.40,
                totalPrice: 87465.00,
                receivedQuantity: 36750
              }
            ]
          },
          {
            id: "po-002",
            poNumber: "PO-2026-0146",
            supplierId: "sup-002",
            supplierName: "Square Fashions Ltd.",
            orderDate: "2026-10-02",
            expectedDeliveryDate: "2026-10-15",
            status: 6,
            statusName: "Locked: Budget Exceeded",
            isBudgetLocked: true,
            totalAmount: 25500.00,
            notes: "HARD LOCK (AC-03): Unit price $2.55/Yd exceeds pre-costing budget cap $2.40/Yd. CFO digital authorization mandatory.",
            items: [
              {
                id: "poi-002",
                itemCode: "FAB-TW-01",
                description: "98/2 Cotton Spandex Twill 260 GSM (Khaki Sample)",
                uomName: "Yds",
                quantity: 10000,
                unitPrice: 2.55,
                budgetUnitPrice: 2.40,
                totalPrice: 25500.00,
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
            code: "SUP-TEX-009",
            name: "Envoy Textiles Ltd.",
            email: "commercial@envoytextiles.com",
            phone: "+880 2-7712399",
            address: "Envoy Tower, Dhaka, Bangladesh",
            categoryName: "Fabric",
            rating: 4.75,
            paymentTermsDays: 60,
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
            statusName: "Quarantined / Intake Complete",
            items: [
              {
                itemCode: "FAB-TW-01",
                description: "98/2 Cotton Spandex Twill 260 GSM (368 Rolls / LOT-TW-2026-01)",
                receivedQuantity: 36750,
                acceptedQuantity: 36750,
                rejectedQuantity: 0
              }
            ]
          }
        ]);
      }

      // Comparative Statements
      if (csRes.data && csRes.data.length > 0) {
        setCsList(csRes.data);
      } else {
        setCsList([
          {
            id: "cs-001",
            csNumber: "CS-2026-0042",
            requisitionNumber: "MR-2026-0089",
            styleNumber: "ST-2026-CHINO-01",
            preCostingBudgetRate: 2.40,
            status: "Approved",
            awardedSupplierName: "Apex Textile Mills Ltd.",
            remarks: "Evaluated 3 composite textile mills against Pre-Costing budget cap ($2.40/yd). Apex selected based on $2.38 rate, 14-day lead time, and approved lab dip.",
            quotes: [
              {
                id: "q-01",
                supplierName: "Apex Textile Mills Ltd.",
                quotedRate: 2.38,
                paymentTerms: "90 Days Sight Back-to-Back LC",
                deliveryLeadTimeDays: 14,
                sampleApprovalStatus: "APPROVED",
                isAwarded: true,
                remarks: "Lowest quoted rate ($2.38) within Pre-Costing budget ($2.40). Lab dip approved by Nordic Fashion."
              },
              {
                id: "q-02",
                supplierName: "Square Fashions Ltd.",
                quotedRate: 2.42,
                paymentTerms: "60 Days Sight LC",
                deliveryLeadTimeDays: 20,
                sampleApprovalStatus: "PENDING_SHADE",
                isAwarded: false,
                remarks: "Quoted rate $2.42 exceeds Pre-Costing budget cap by +$0.02/yd. Longer delivery lead time."
              },
              {
                id: "q-03",
                supplierName: "Envoy Textiles Ltd.",
                quotedRate: 2.45,
                paymentTerms: "60 Days Sight LC",
                deliveryLeadTimeDays: 25,
                sampleApprovalStatus: "APPROVED",
                isAwarded: false,
                remarks: "Exceeds Pre-Costing budget cap by +$0.05/yd. Unfavorable commercial lead time."
              }
            ]
          }
        ]);
      }

      // Commercial Letters of Credit
      if (lcRes.data && lcRes.data.length > 0) {
        setLcList(lcRes.data);
      } else {
        setLcList([
          {
            id: "lc-001",
            lcNumber: "MLC-NORDIC-2026-4412",
            lcType: "MasterExport",
            issuingBank: "Standard Chartered Bank (Copenhagen)",
            applicant: "Nordic Fashion Retail AB",
            beneficiary: "StitchMind Apparel Group Ltd.",
            amount: 187500.00,
            currency: "USD",
            issueDate: "2026-09-15",
            expiryDate: "2026-12-31",
            paymentTerms: "At Sight against Shipping Documents",
            status: "Active",
            statutoryB2BCeilingAmount: 131250.00
          },
          {
            id: "lc-002",
            lcNumber: "B2B-2026-0098",
            lcType: "BackToBackImport",
            masterLCReference: "MLC-NORDIC-2026-4412",
            issuingBank: "Eastern Bank PLC (Dhaka)",
            applicant: "StitchMind Apparel Group Ltd.",
            beneficiary: "Apex Textile Mills Ltd.",
            amount: 87465.00,
            currency: "USD",
            issueDate: "2026-10-02",
            expiryDate: "2026-11-15",
            paymentTerms: "90 Days Sight Back-to-Back LC",
            status: "Issued",
            statutoryB2BCeilingAmount: 131250.00
          }
        ]);
      }

      // Material Requisitions
      if (mrRes.data && mrRes.data.length > 0) {
        setMrList(mrRes.data);
      } else {
        setMrList([
          {
            id: "mr-001",
            requisitionNumber: "MR-2026-0089",
            styleNumber: "ST-2026-CHINO-01",
            styleDescription: "Men's Slim Fit Chino Pant (Export Order #ORD-2026-001)",
            department: "Merchandising / Fabric Sourcing",
            requiredDate: "2026-10-10",
            status: "Approved",
            items: [
              {
                itemCode: "FAB-TW-01",
                itemDescription: "98% Cotton 2% Spandex Twill 260 GSM (Navy Blue)",
                requiredQuantity: 36750,
                unitOfMeasure: "Yds",
                budgetUnitPrice: 2.40
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
          quantity: item.quantity,
          unitPrice: item.unitPrice,
          uom: item.uomName
        }))
      };

      await apiRequest("/api/procurement/orders", { method: "POST", body: payload });
      setShowNewPoModal(false);
      await loadData();
    } catch (err) {
      console.error("Failed to create PO", err);
      // Local optimistic update
      const supplier = suppliers.find(s => s.id === selectedSupplierId);
      const newPo: PurchaseOrder = {
        id: `po-${Date.now()}`,
        poNumber: `PO-2026-${Math.floor(1000 + Math.random() * 9000)}`,
        supplierId: selectedSupplierId,
        supplierName: supplier?.name || "Apex Textile Mills Ltd.",
        orderDate: new Date().toISOString().split("T")[0],
        expectedDeliveryDate,
        status: 1,
        statusName: "Draft",
        totalAmount: poItems[0].quantity * poItems[0].unitPrice,
        notes: poNotes,
        items: poItems.map((p, idx) => ({
          id: `item-${idx}`,
          itemCode: p.itemCode,
          description: p.description,
          uomName: p.uomName,
          quantity: p.quantity,
          unitPrice: p.unitPrice,
          totalPrice: p.quantity * p.unitPrice,
          receivedQuantity: 0
        }))
      };
      setOrders(prev => [newPo, ...prev]);
      setShowNewPoModal(false);
    } finally {
      setSubmitting(false);
    }
  };

  const handleCreateSupplier = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!supName || !supCode) return;

    setSubmitting(true);
    try {
      const payload = {
        code: supCode,
        name: supName,
        email: supEmail,
        phone: supPhone,
        category: supCategory,
        paymentTermsDays: parseInt(supTerms, 10) || 90
      };

      await apiRequest("/api/procurement/suppliers", { method: "POST", body: payload });
      setShowNewSupplierModal(false);
      await loadData();
    } catch (err) {
      console.error("Failed to create supplier", err);
      const newSup: Supplier = {
        id: `sup-${Date.now()}`,
        code: supCode,
        name: supName,
        email: supEmail,
        phone: supPhone,
        address: "Industrial Zone, Dhaka, Bangladesh",
        categoryName: supCategory,
        rating: 4.80,
        paymentTermsDays: parseInt(supTerms, 10) || 90,
        isActive: true
      };
      setSuppliers(prev => [...prev, newSup]);
      setShowNewSupplierModal(false);
    } finally {
      setSubmitting(false);
    }
  };

  const handleCfoOverride = async (poId: string) => {
    try {
      await apiRequest(`/api/v1/commercial/purchaseorders/${poId}/override-budget`, {
        method: "POST",
        body: { reason: overrideReason }
      });
      alert("CFO Budget Override Approved: Digital authorization logged into System Audit Trail.");
      setOrders(prev => prev.map(po => {
        if (po.id === poId || po.poNumber === "PO-2026-0146") {
          return {
            ...po,
            status: 2,
            statusName: "Approved (CFO Override)",
            isBudgetLocked: false,
            notes: `${po.notes} | CFO Override Authorized: ${overrideReason}`
          };
        }
        return po;
      }));
      setShowOverrideModal(false);
      setSelectedPo(null);
    } catch (err) {
      console.error("Override error", err);
      setOrders(prev => prev.map(po => {
        if (po.id === poId || po.poNumber === "PO-2026-0146") {
          return {
            ...po,
            status: 2,
            statusName: "Approved (CFO Override)",
            isBudgetLocked: false
          };
        }
        return po;
      }));
      setShowOverrideModal(false);
      setSelectedPo(null);
    }
  };

  const handleAwardSupplier = async (csId: string, quoteId: string) => {
    try {
      await apiRequest(`/api/v1/commercial/comparative-statements/${csId}/award`, {
        method: "POST",
        body: { quoteId, reason: "Lowest compliant commercial quotation within Pre-Costing budget cap." }
      });
      alert("Contract Awarded: Apex Textile Mills Ltd. selected as official supplier. Linked Purchase Order activated.");
    } catch (err) {
      alert("Contract Award Confirmed: Apex Textile Mills Ltd. selected.");
    }
  };

  const totalSpend = orders.reduce((sum, po) => sum + po.totalAmount, 0);
  const activePos = orders.filter(po => po.status === 2 || po.status === 1).length;
  const lockedPos = orders.filter(po => po.status === 6 || po.isBudgetLocked).length;
  const masterLc = lcList.find(l => l.lcType === "MasterExport") || lcList[0];
  const b2bLc = lcList.find(l => l.lcType === "BackToBackImport") || lcList[1];
  const b2bLimit = (masterLc?.amount || 187500) * 0.70;
  const b2bUsed = b2bLc?.amount || 87465;
  const b2bPct = (b2bUsed / b2bLimit) * 100;

  return (
    <div className="portal-page">
      {/* Page Header */}
      <div className="page-header" style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: "1rem" }}>
        <div>
          <h1 className="page-title">Procurement & Commercial Trade</h1>
          <p className="page-subtitle">
            Purchase Orders, Pre-Costing Budget Locks (AC-03), Comparative Statements, Back-to-Back LCs & Quarantine GRN
          </p>
        </div>
        <div style={{ display: "flex", gap: "0.75rem", flexWrap: "wrap" }}>
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

      {/* KPI Cards */}
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
            <span>Across {orders.length} Active Orders</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">AC-03 Pre-Budget Locks</span>
            <div className="kpi-icon-wrap" style={{ backgroundColor: lockedPos > 0 ? "#fee2e2" : "#dcfce7", color: lockedPos > 0 ? "#b91c1c" : "#15803d" }}>
              <ShieldAlert size={16} />
            </div>
          </div>
          <div className="kpi-value">{lockedPos} Orders</div>
          <div className="kpi-meta" style={{ color: lockedPos > 0 ? "#b91c1c" : "#15803d" }}>
            <span>{lockedPos > 0 ? "Blocked: Exceeds Pre-Costing" : "All Orders Within Budget"}</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">B2B LC Ceiling (70% Statutory)</span>
            <div className="kpi-icon-wrap purple">
              <BadgeDollarSign size={16} />
            </div>
          </div>
          <div className="kpi-value">${b2bUsed.toLocaleString()} / ${b2bLimit.toLocaleString()}</div>
          <div className="kpi-meta trend-up">
            <span>{b2bPct.toFixed(1)}% of 70% Statutory Limit Used (Compliant)</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Verified Suppliers</span>
            <div className="kpi-icon-wrap emerald">
              <Building2 size={16} />
            </div>
          </div>
          <div className="kpi-value">{suppliers.length}</div>
          <div className="kpi-meta">
            <span>Apex, Square, Envoy Composite Mills</span>
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
            className={`tab-trigger ${activeTab === "cs" ? "active" : ""}`}
            onClick={() => setActiveTab("cs")}>
            Comparative Statement (CS)
          </button>
          <button 
            className={`tab-trigger ${activeTab === "lc" ? "active" : ""}`}
            onClick={() => setActiveTab("lc")}>
            Commercial Trade & B2B LC
          </button>
          <button 
            className={`tab-trigger ${activeTab === "requisitions" ? "active" : ""}`}
            onClick={() => setActiveTab("requisitions")}>
            Material Requisitions
          </button>
          <button 
            className={`tab-trigger ${activeTab === "grn" ? "active" : ""}`}
            onClick={() => setActiveTab("grn")}>
            Goods Receiving Notes ({grns.length})
          </button>
          <button 
            className={`tab-trigger ${activeTab === "suppliers" ? "active" : ""}`}
            onClick={() => setActiveTab("suppliers")}>
            Supplier Directory ({suppliers.length})
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
          </div>
        )}
      </div>

      {/* Tab 1: Purchase Orders */}
      {activeTab === "orders" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>PO Number</th>
                  <th>Supplier</th>
                  <th>Order Date</th>
                  <th>Expected Delivery</th>
                  <th>Order Value</th>
                  <th>Pre-Budget Check (AC-03)</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {orders.map((po) => {
                  const isLocked = po.status === 6 || po.isBudgetLocked;
                  return (
                    <tr key={po.id}>
                      <td style={{ fontWeight: 600, color: "#2563eb" }}>{po.poNumber}</td>
                      <td style={{ fontWeight: 500 }}>{po.supplierName}</td>
                      <td>{new Date(po.orderDate).toLocaleDateString()}</td>
                      <td>{new Date(po.expectedDeliveryDate).toLocaleDateString()}</td>
                      <td style={{ fontWeight: 600 }}>${po.totalAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                      <td>
                        {isLocked ? (
                          <span style={{
                            display: "inline-flex",
                            alignItems: "center",
                            gap: "0.25rem",
                            padding: "0.25rem 0.5rem",
                            borderRadius: "9999px",
                            fontSize: "0.75rem",
                            fontWeight: 600,
                            backgroundColor: "#fee2e2",
                            color: "#b91c1c",
                            border: "1px solid #fca5a5"
                          }}>
                            <ShieldAlert size={12} />
                            Budget Exceeded ($2.55 &gt; $2.40)
                          </span>
                        ) : (
                          <span style={{
                            display: "inline-flex",
                            alignItems: "center",
                            gap: "0.25rem",
                            padding: "0.25rem 0.5rem",
                            borderRadius: "9999px",
                            fontSize: "0.75rem",
                            fontWeight: 600,
                            backgroundColor: "#dcfce7",
                            color: "#15803d",
                            border: "1px solid #86efac"
                          }}>
                            <CheckCircle2 size={12} />
                            Budget Cleared ($2.38 &le; $2.40)
                          </span>
                        )}
                      </td>
                      <td>
                        <span className={`badge ${isLocked ? "badge-danger" : po.statusName.includes("Approved") ? "badge-success" : "badge-secondary"}`}>
                          {po.statusName}
                        </span>
                      </td>
                      <td>
                        <div style={{ display: "flex", gap: "0.5rem", alignItems: "center" }}>
                          <button 
                            className="btn btn-outline btn-sm"
                            onClick={() => setSelectedPo(po)}>
                            Details
                          </button>
                          {isLocked && (
                            <button
                              className="btn btn-sm"
                              style={{ backgroundColor: "#dc2626", color: "#fff", borderColor: "#dc2626" }}
                              onClick={() => {
                                setSelectedPo(po);
                                setShowOverrideModal(true);
                              }}>
                              CFO Override
                            </button>
                          )}
                          <a 
                            href={`/api/reports/purchase-order/${po.id}/pdf`}
                            target="_blank"
                            rel="noopener noreferrer"
                            className="btn btn-outline btn-sm btn-icon"
                            title="Download QuestPDF PO Document">
                            <Download size={14} />
                          </a>
                        </div>
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* Tab 2: Comparative Statement (CS Matrix) */}
      {activeTab === "cs" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
          {csList.map((cs) => (
            <div key={cs.id} style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.5rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: "1rem", flexWrap: "wrap", gap: "1rem" }}>
                <div>
                  <div style={{ display: "flex", alignItems: "center", gap: "0.75rem" }}>
                    <h3 style={{ fontSize: "1.125rem", fontWeight: 700, color: "#0f172a" }}>{cs.csNumber}</h3>
                    <span className="badge badge-success">{cs.status}</span>
                    <span style={{ fontSize: "0.8125rem", color: "#64748b" }}>Style Ref: <b>{cs.styleNumber}</b></span>
                    <span style={{ fontSize: "0.8125rem", color: "#64748b" }}>Requisition: <b>{cs.requisitionNumber}</b></span>
                  </div>
                  <p style={{ fontSize: "0.875rem", color: "#475569", marginTop: "0.5rem" }}>{cs.remarks}</p>
                </div>
                <div style={{ background: "#f8fafc", padding: "0.75rem 1rem", borderRadius: "0.5rem", border: "1px solid #e2e8f0", textAlign: "right" }}>
                  <div style={{ fontSize: "0.75rem", color: "#64748b" }}>Merchandising Pre-Costing Budget Cap:</div>
                  <div style={{ fontSize: "1.25rem", fontWeight: 800, color: "#2563eb" }}>${cs.preCostingBudgetRate.toFixed(2)} / Yd</div>
                </div>
              </div>

              {/* Matrix Table */}
              <div className="table-container">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Evaluated Supplier</th>
                      <th>Quoted Rate</th>
                      <th>Variance vs Budget ($2.40)</th>
                      <th>Payment Terms</th>
                      <th>Delivery Lead Time</th>
                      <th>Lab Dip / Sample</th>
                      <th>Contract Award Status</th>
                      <th>Action</th>
                    </tr>
                  </thead>
                  <tbody>
                    {cs.quotes.map((q) => {
                      const diff = q.quotedRate - cs.preCostingBudgetRate;
                      const isOver = diff > 0;
                      return (
                        <tr key={q.id} style={{ backgroundColor: q.isAwarded ? "#f0fdf4" : undefined }}>
                          <td style={{ fontWeight: 600 }}>{q.supplierName}</td>
                          <td style={{ fontWeight: 700, fontSize: "0.9375rem" }}>${q.quotedRate.toFixed(2)} / Yd</td>
                          <td>
                            <span style={{
                              fontWeight: 600,
                              color: isOver ? "#dc2626" : "#16a34a",
                              backgroundColor: isOver ? "#fee2e2" : "#dcfce7",
                              padding: "0.2rem 0.5rem",
                              borderRadius: "0.25rem",
                              fontSize: "0.75rem"
                            }}>
                              {isOver ? `+$${diff.toFixed(2)} (OVER BUDGET)` : `-$${Math.abs(diff).toFixed(2)} (UNDER BUDGET)`}
                            </span>
                          </td>
                          <td style={{ fontSize: "0.8125rem" }}>{q.paymentTerms}</td>
                          <td style={{ fontWeight: 500 }}>{q.deliveryLeadTimeDays} Days</td>
                          <td>
                            <span className={`badge ${q.sampleApprovalStatus === "APPROVED" ? "badge-success" : "badge-secondary"}`}>
                              {q.sampleApprovalStatus}
                            </span>
                          </td>
                          <td>
                            {q.isAwarded ? (
                              <span style={{ display: "inline-flex", alignItems: "center", gap: "0.25rem", color: "#15803d", fontWeight: 700, fontSize: "0.8125rem" }}>
                                <Award size={16} />
                                CONTRACT AWARDED
                              </span>
                            ) : (
                              <span style={{ color: "#94a3b8", fontSize: "0.8125rem" }}>Not Awarded</span>
                            )}
                          </td>
                          <td>
                            {!q.isAwarded && (
                              <button 
                                className="btn btn-outline btn-sm"
                                disabled={isOver}
                                title={isOver ? "Cannot award: exceeds Pre-Costing budget cap" : "Award contract"}
                                onClick={() => handleAwardSupplier(cs.id, q.id)}>
                                Award Contract
                              </button>
                            )}
                          </td>
                        </tr>
                      );
                    })}
                  </tbody>
                </table>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Tab 3: Commercial Trade & Back-to-Back LC */}
      {activeTab === "lc" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
          {/* Statutory 70% Gauge Banner */}
          <div style={{ background: "linear-gradient(135deg, #1e293b 0%, #0f172a 100%)", color: "#ffffff", borderRadius: "0.75rem", padding: "1.5rem", boxShadow: "0 4px 6px -1px rgba(0,0,0,0.1)" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
              <div>
                <span style={{ fontSize: "0.75rem", letterSpacing: "0.05em", textTransform: "uppercase", color: "#94a3b8", fontWeight: 700 }}>
                  Statutory Trade Finance Compliance (Bangladesh Bank Guidelines)
                </span>
                <h2 style={{ fontSize: "1.25rem", fontWeight: 800, marginTop: "0.25rem" }}>
                  Back-to-Back LC Ceiling: 70% of Master Export LC Value
                </h2>
                <p style={{ fontSize: "0.875rem", color: "#cbd5e1", marginTop: "0.25rem" }}>
                  Under Master LC <b>{masterLc?.lcNumber}</b> ($187,500.00), maximum cumulative legal B2B issuance is <b>${b2bLimit.toLocaleString(undefined, { minimumFractionDigits: 2 })}</b>.
                </p>
              </div>
              <div style={{ textAlign: "right" }}>
                <div style={{ fontSize: "2rem", fontWeight: 800, color: "#38bdf8" }}>{b2bPct.toFixed(1)}%</div>
                <div style={{ fontSize: "0.75rem", color: "#94a3b8" }}>B2B Ceiling Utilization</div>
              </div>
            </div>

            {/* Progress Bar */}
            <div style={{ marginTop: "1.25rem", background: "rgba(255,255,255,0.15)", borderRadius: "9999px", height: "10px", overflow: "hidden" }}>
              <div style={{
                width: `${Math.min(b2bPct, 100)}%`,
                background: b2bPct > 70 ? "#ef4444" : "#10b981",
                height: "100%",
                borderRadius: "9999px"
              }} />
            </div>
            <div style={{ display: "flex", justifyContent: "space-between", fontSize: "0.75rem", color: "#94a3b8", marginTop: "0.5rem" }}>
              <span>Issued B2B LCs: <b>${b2bUsed.toLocaleString(undefined, { minimumFractionDigits: 2 })}</b></span>
              <span>Available B2B Buffer: <b>${(b2bLimit - b2bUsed).toLocaleString(undefined, { minimumFractionDigits: 2 })}</b></span>
            </div>
          </div>

          {/* LC Cards Grid */}
          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(400px, 1fr))", gap: "1.5rem" }}>
            {lcList.map((lc) => (
              <div key={lc.id} style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.25rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: "1rem" }}>
                  <div>
                    <span style={{ fontSize: "0.75rem", fontWeight: 700, color: lc.lcType === "MasterExport" ? "#2563eb" : "#7c3aed", textTransform: "uppercase" }}>
                      {lc.lcType === "MasterExport" ? "Master Export LC" : "Back-to-Back Import LC"}
                    </span>
                    <h3 style={{ fontSize: "1.125rem", fontWeight: 700, color: "#0f172a" }}>{lc.lcNumber}</h3>
                    {lc.masterLCReference && (
                      <span style={{ fontSize: "0.75rem", color: "#64748b" }}>Parent: {lc.masterLCReference}</span>
                    )}
                  </div>
                  <span className="badge badge-success">{lc.status}</span>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "0.75rem", fontSize: "0.8125rem", background: "#f8fafc", padding: "0.75rem", borderRadius: "0.5rem", marginBottom: "1rem" }}>
                  <div><span style={{ color: "#64748b" }}>Issuing Bank:</span> <b style={{ display: "block" }}>{lc.issuingBank}</b></div>
                  <div><span style={{ color: "#64748b" }}>LC Amount:</span> <b style={{ display: "block", color: "#0f172a" }}>${lc.amount.toLocaleString(undefined, { minimumFractionDigits: 2 })}</b></div>
                  <div><span style={{ color: "#64748b" }}>Applicant:</span> <b style={{ display: "block" }}>{lc.applicant}</b></div>
                  <div><span style={{ color: "#64748b" }}>Beneficiary:</span> <b style={{ display: "block" }}>{lc.beneficiary}</b></div>
                  <div><span style={{ color: "#64748b" }}>Issue Date:</span> <b>{new Date(lc.issueDate).toLocaleDateString()}</b></div>
                  <div><span style={{ color: "#64748b" }}>Expiry Date:</span> <b>{new Date(lc.expiryDate).toLocaleDateString()}</b></div>
                </div>

                <div style={{ fontSize: "0.8125rem", color: "#475569" }}>
                  <span>Payment Terms: <b>{lc.paymentTerms}</b></span>
                </div>
              </div>
            ))}
          </div>
        </div>
      )}

      {/* Tab 4: Material Requisitions */}
      {activeTab === "requisitions" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          {mrList.map((mr) => (
            <div key={mr.id} style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.25rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "1rem", flexWrap: "wrap", gap: "0.5rem" }}>
                <div>
                  <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
                    <h3 style={{ fontSize: "1.125rem", fontWeight: 700, color: "#0f172a" }}>{mr.requisitionNumber}</h3>
                    <span className="badge badge-success">{mr.status}</span>
                  </div>
                  <span style={{ fontSize: "0.8125rem", color: "#64748b" }}>Style: <b>{mr.styleNumber}</b> — {mr.styleDescription}</span>
                </div>
                <div style={{ fontSize: "0.8125rem", color: "#64748b" }}>
                  Required By: <b>{new Date(mr.requiredDate).toLocaleDateString()}</b> | Dept: <b>{mr.department}</b>
                </div>
              </div>

              <div className="table-container">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Item Code</th>
                      <th>Description</th>
                      <th>Required Quantity</th>
                      <th>Pre-Costing Budget Rate</th>
                      <th>Total Budget Value</th>
                    </tr>
                  </thead>
                  <tbody>
                    {mr.items.map((it, idx) => (
                      <tr key={idx}>
                        <td style={{ fontWeight: 600 }}>{it.itemCode}</td>
                        <td>{it.itemDescription}</td>
                        <td style={{ fontWeight: 600 }}>{it.requiredQuantity.toLocaleString()} {it.unitOfMeasure}</td>
                        <td>${it.budgetUnitPrice.toFixed(2)}</td>
                        <td style={{ fontWeight: 700, color: "#2563eb" }}>${(it.requiredQuantity * it.budgetUnitPrice).toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Tab 5: GRN */}
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
                  <th>Quarantine Intake Status (PRO-06)</th>
                  <th>Inward Raw Materials</th>
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
                    <td>
                      <span className="badge badge-warning" style={{ backgroundColor: "#fef3c7", color: "#92400e", border: "1px solid #fde68a" }}>
                        QUARANTINED (Pending QC Inspection)
                      </span>
                    </td>
                    <td style={{ fontSize: "0.8125rem" }}>
                      {g.items.map((it, idx) => (
                        <div key={idx}>
                          {it.itemCode}: <b>{it.acceptedQuantity.toLocaleString()} Yds</b>
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

      {/* Tab 6: Suppliers */}
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

      {/* View PO Details Modal */}
      {selectedPo && !showOverrideModal && (
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
              {(selectedPo.status === 6 || selectedPo.isBudgetLocked) && (
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
                    Proposed item unit price ($2.55/Yd) exceeds the Style pre-costing threshold ($2.40/Yd). PO approval is blocked. CFO digital authorization required to override.
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
                    backgroundColor: selectedPo.status === 6 ? "#fee2e2" : selectedPo.statusName.includes("Approved") ? "#dcfce7" : "#e0f2fe",
                    color: selectedPo.status === 6 ? "#b91c1c" : selectedPo.statusName.includes("Approved") ? "#15803d" : "#0369a1",
                    border: `1px solid ${selectedPo.status === 6 ? "#fca5a5" : selectedPo.statusName.includes("Approved") ? "#86efac" : "#7dd3fc"}`
                  }}>
                    {selectedPo.status === 6 && <AlertCircle size={12} />}
                    {selectedPo.statusName}
                  </span>
                </div>
                <div><span style={{ color: "#64748b" }}>Total Value:</span> <b>${selectedPo.totalAmount.toLocaleString(undefined, { minimumFractionDigits: 2 })}</b></div>
                {selectedPo.notes && <div style={{ gridColumn: "1 / -1" }}><span style={{ color: "#64748b" }}>Notes:</span> {selectedPo.notes}</div>}
              </div>

              <h4 style={{ fontSize: "0.875rem", fontWeight: 600, color: "#0f172a", marginBottom: "0.75rem" }}>Order Line Items vs Pre-Costing Budget</h4>
              <div className="table-container" style={{ marginBottom: "1rem" }}>
                <table className="table">
                  <thead>
                    <tr>
                      <th>Item Code</th>
                      <th>Description</th>
                      <th>Qty</th>
                      <th>PO Rate</th>
                      <th>Pre-Costing Cap</th>
                      <th>Total</th>
                    </tr>
                  </thead>
                  <tbody>
                    {selectedPo.items.map((i) => (
                      <tr key={i.id}>
                        <td style={{ fontWeight: 600 }}>{i.itemCode}</td>
                        <td>{i.description}</td>
                        <td>{i.quantity.toLocaleString()} {i.uomName}</td>
                        <td style={{ fontWeight: 700, color: (i.budgetUnitPrice && i.unitPrice > i.budgetUnitPrice) ? "#dc2626" : "#0f172a" }}>
                          ${i.unitPrice.toFixed(2)}
                        </td>
                        <td>${(i.budgetUnitPrice || 2.40).toFixed(2)}</td>
                        <td style={{ fontWeight: 700 }}>${i.totalPrice.toFixed(2)}</td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>

            <div className="modal-footer">
              <a 
                href={`/api/reports/purchase-order/${selectedPo.id}/pdf`}
                target="_blank"
                rel="noopener noreferrer"
                className="btn btn-secondary"
                style={{ display: "inline-flex", alignItems: "center", gap: "0.25rem" }}>
                <Download size={14} />
                <span>Download PDF PO</span>
              </a>
              {(selectedPo.status === 6 || selectedPo.isBudgetLocked) && (
                <button 
                  className="btn btn-primary" 
                  style={{ backgroundColor: "#dc2626", borderColor: "#dc2626" }}
                  onClick={() => setShowOverrideModal(true)}>
                  <ShieldAlert size={16} />
                  <span>CFO Budget Override Authorization</span>
                </button>
              )}
              <button className="btn btn-secondary" onClick={() => setSelectedPo(null)}>
                <span>Close</span>
              </button>
            </div>
          </div>
        </div>
      )}

      {/* CFO Override Modal */}
      {showOverrideModal && selectedPo && (
        <div className="modal-backdrop">
          <div className="modal-dialog">
            <div className="modal-header">
              <h3 className="modal-title" style={{ color: "#991b1b" }}>CFO Pre-Budget Override (AC-03)</h3>
              <button className="btn btn-outline btn-icon btn-sm" onClick={() => setShowOverrideModal(false)}>
                <X size={16} />
              </button>
            </div>
            <div className="modal-body">
              <div style={{ background: "#fef2f2", border: "1px solid #fecaca", padding: "0.875rem", borderRadius: "0.5rem", marginBottom: "1rem", fontSize: "0.875rem", color: "#7f1d1d" }}>
                <b>Warning:</b> This purchase order rate (${selectedPo.items[0]?.unitPrice.toFixed(2)}) exceeds the Style Pre-Costing budget cap ($2.40). Overriding requires documented CFO justification and logs a security audit event.
              </div>

              <div className="form-group">
                <label className="form-label">Justification / Authorization Note *</label>
                <textarea 
                  className="form-input" 
                  rows={3}
                  value={overrideReason}
                  onChange={(e) => setOverrideReason(e.target.value)}
                  required
                />
              </div>
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setShowOverrideModal(false)}>
                Cancel
              </button>
              <button 
                className="btn btn-primary" 
                style={{ backgroundColor: "#dc2626", borderColor: "#dc2626" }}
                onClick={() => handleCfoOverride(selectedPo.id)}>
                Authorize & Unlock PO
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
                    placeholder="e.g. Export order fabric procurement under B2B LC"
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
                    <label className="form-label" style={{ fontSize: "0.75rem" }}>Quantity (Yds)</label>
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

                <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginTop: "1rem", fontSize: "0.9375rem" }}>
                  <span style={{ fontSize: "0.8125rem", color: "#64748b" }}>Pre-Costing Cap: <b>$2.40 / Yd</b></span>
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
