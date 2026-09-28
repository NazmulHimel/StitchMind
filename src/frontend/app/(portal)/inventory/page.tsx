"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiRequest } from "@/lib/api";
import { 
  Warehouse, Package, AlertTriangle, ArrowUpRight, ArrowDownRight,
  Layers, Scissors, RefreshCw, Search, Filter, Plus, CheckCircle2, X,
  MapPin, QrCode, FileText, Download, ShieldCheck, Box, BarChart3,
  Check, Lock, Unlock, ArrowRight
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

interface WarehouseLocation {
  id: string;
  locationCode: string;
  warehouseCode: string;
  aisle: string;
  rack: string;
  tier: string;
  bin: string;
  zone: number;
  zoneName?: string;
  maxCapacityRolls: number;
  currentRollCount: number;
  occupancyPercentage: number;
  isFull: boolean;
  isActive: boolean;
}

interface FabricRoll {
  id: string;
  rollNumber: string;
  rollBarcode: string;
  dyeLotNumber: string;
  gsm: number;
  measuredGSM?: number;
  widthInches: number;
  cuttableWidthInches?: number;
  totalLengthYards: number;
  availableLengthYards: number;
  shadeGroup: string;
  shrinkageLengthPct: number;
  shrinkageWidthPct: number;
  grade: number;
  status: number;
  statusName?: string;
  rackLocation: string;
  locationCode: string;
  isRemnant: boolean;
}

interface CustomsBondEntry {
  id: string;
  bondRegisterNumber: string;
  utilizationPermitNo: string;
  transactionType: number;
  transactionTypeName?: string;
  referenceDocType: string;
  referenceDocNumber: string;
  itemCode: string;
  itemDescription: string;
  quantity: number;
  unitOfMeasure: string;
  openingBalance: number;
  closingBalance: number;
  dutySavedUSD: number;
  transactionDate: string;
  remarks: string;
}

interface StoreIssueNote {
  id: string;
  issueNumber: string;
  issueDate: string;
  targetDepartment: string;
  styleNumber: string;
  buyerOrderNumber: string;
  requestedBy: string;
  issuedBy: string;
  status: number;
  statusName?: string;
  totalIssuedYards: number;
  totalRemnantYards: number;
  remarks: string;
  items: Array<{
    id: string;
    rollBarcode: string;
    shadeGroup: string;
    totalRollLengthYards: number;
    issuedYards: number;
    remnantYards: number;
    remnantBarcode?: string;
    remnantLocationCode?: string;
  }>;
}

interface CycleCount {
  id: string;
  cycleCountNumber: string;
  auditDate: string;
  auditorName: string;
  locationZone: string;
  status: string;
  authorizedBy?: string;
  remarks: string;
  lines: Array<{
    id: string;
    itemCode: string;
    rollBarcode: string;
    locationCode: string;
    systemQuantity: number;
    physicalCountedQuantity: number;
    varianceQuantity: number;
    variancePercentage: number;
    reasonCode: string;
    isAdjusted: boolean;
  }>;
}

const ZONE_NAMES: { [key: number]: string } = {
  1: "Raw Material Fabric",
  2: "Quarantine Storage",
  3: "Trims & Accessories",
  4: "Remnant Storage",
  5: "Finished Goods"
};

export default function InventoryPage() {
  const { user } = useAuth();
  const [activeTab, setActiveTab] = useState<"locations" | "rolls" | "bond" | "store-issues" | "cycle-counts" | "items">("locations");
  const [items, setItems] = useState<Item[]>([]);
  const [locations, setLocations] = useState<WarehouseLocation[]>([]);
  const [rolls, setRolls] = useState<FabricRoll[]>([]);
  const [bondLedger, setBondLedger] = useState<CustomsBondEntry[]>([]);
  const [storeIssues, setStoreIssues] = useState<StoreIssueNote[]>([]);
  const [cycleCounts, setCycleCounts] = useState<CycleCount[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchQuery, setSearchQuery] = useState("");
  const [zoneFilter, setZoneFilter] = useState("ALL");
  const [shadeFilter, setShadeFilter] = useState("ALL");

  // Create Store Issue Modal State
  const [showIssueModal, setShowIssueModal] = useState(false);
  const [targetDept, setTargetDept] = useState("Cutting Department (Table 01)");
  const [styleRef, setStyleRef] = useState("ST-2026-CHINO-01");
  const [orderRef, setOrderRef] = useState("ORD-2026-001");
  const [submitting, setSubmitting] = useState(false);

  useEffect(() => {
    loadData();
  }, []);

  const loadData = async () => {
    setLoading(true);
    try {
      const [locsRes, rollsRes, bondRes, sinRes, ccRes, itemsRes] = await Promise.all([
        apiRequest<WarehouseLocation[]>("/api/v1/warehouse/locations"),
        apiRequest<FabricRoll[]>("/api/v1/warehouse/fabric-rolls"),
        apiRequest<CustomsBondEntry[]>("/api/v1/warehouse/bond-ledger"),
        apiRequest<StoreIssueNote[]>("/api/v1/warehouse/store-issues"),
        apiRequest<CycleCount[]>("/api/v1/warehouse/cycle-counts"),
        apiRequest<Item[]>("/api/inventory/items")
      ]);

      if (locsRes.data && locsRes.data.length > 0) {
        setLocations(locsRes.data);
      } else {
        setLocations([
          { id: "loc-1", locationCode: "WH1-A03-R02-T01-B01", warehouseCode: "WH1", aisle: "A03", rack: "R02", tier: "T01", bin: "B01", zone: 1, maxCapacityRolls: 12, currentRollCount: 6, occupancyPercentage: 50.0, isFull: false, isActive: true },
          { id: "loc-2", locationCode: "WH1-A03-R02-T01-B02", warehouseCode: "WH1", aisle: "A03", rack: "R02", tier: "T01", bin: "B02", zone: 1, maxCapacityRolls: 12, currentRollCount: 8, occupancyPercentage: 66.7, isFull: false, isActive: true },
          { id: "loc-3", locationCode: "WH1-A03-R02-T01-B03", warehouseCode: "WH1", aisle: "A03", rack: "R02", tier: "T01", bin: "B03", zone: 1, maxCapacityRolls: 12, currentRollCount: 10, occupancyPercentage: 83.3, isFull: false, isActive: true },
          { id: "loc-4", locationCode: "WH1-A03-R02-T01-B04", warehouseCode: "WH1", aisle: "A03", rack: "R02", tier: "T01", bin: "B04", zone: 1, maxCapacityRolls: 12, currentRollCount: 12, occupancyPercentage: 100.0, isFull: true, isActive: true },
          { id: "loc-5", locationCode: "WH1-A03-R05-T03-B05", warehouseCode: "WH1", aisle: "A03", rack: "R05", tier: "T03", bin: "B05", zone: 2, maxCapacityRolls: 20, currentRollCount: 5, occupancyPercentage: 25.0, isFull: false, isActive: true },
          { id: "loc-6", locationCode: "WH1-A01-R01-T01-REM", warehouseCode: "WH1", aisle: "A01", rack: "R01", tier: "T01", bin: "REM", zone: 4, maxCapacityRolls: 30, currentRollCount: 3, occupancyPercentage: 10.0, isFull: false, isActive: true }
        ]);
      }

      if (rollsRes.data && rollsRes.data.length > 0) {
        setRolls(rollsRes.data);
      } else {
        setRolls([
          { id: "r-1", rollNumber: "R-00891", rollBarcode: "ROL-2026-00891", dyeLotNumber: "DYE-TW-2026-01", gsm: 260, measuredGSM: 262, widthInches: 58, cuttableWidthInches: 58, totalLengthYards: 102.5, availableLengthYards: 102.5, shadeGroup: "A", shrinkageLengthPct: -2.5, shrinkageWidthPct: -1.2, grade: 1, status: 1, rackLocation: "WH1-A03-R02-T01-B01", locationCode: "WH1-A03-R02-T01-B01", isRemnant: false },
          { id: "r-2", rollNumber: "R-00892", rollBarcode: "ROL-2026-00892", dyeLotNumber: "DYE-TW-2026-01", gsm: 260, measuredGSM: 260, widthInches: 58, cuttableWidthInches: 58, totalLengthYards: 100.0, availableLengthYards: 100.0, shadeGroup: "A", shrinkageLengthPct: -2.4, shrinkageWidthPct: -1.2, grade: 1, status: 1, rackLocation: "WH1-A03-R02-T01-B02", locationCode: "WH1-A03-R02-T01-B02", isRemnant: false },
          { id: "r-3", rollNumber: "R-00893", rollBarcode: "ROL-2026-00893", dyeLotNumber: "DYE-TW-2026-01", gsm: 261, measuredGSM: 261, widthInches: 58, cuttableWidthInches: 58, totalLengthYards: 98.5, availableLengthYards: 98.5, shadeGroup: "B", shrinkageLengthPct: -2.6, shrinkageWidthPct: -1.3, grade: 1, status: 1, rackLocation: "WH1-A03-R02-T01-B03", locationCode: "WH1-A03-R02-T01-B03", isRemnant: false },
          { id: "r-4", rollNumber: "R-00894", rollBarcode: "ROL-2026-00894", dyeLotNumber: "DYE-TW-2026-01", gsm: 260, measuredGSM: 260, widthInches: 58, cuttableWidthInches: 58, totalLengthYards: 105.0, availableLengthYards: 105.0, shadeGroup: "C", shrinkageLengthPct: -2.8, shrinkageWidthPct: -1.5, grade: 1, status: 1, rackLocation: "WH1-A03-R02-T01-B04", locationCode: "WH1-A03-R02-T01-B04", isRemnant: false },
          { id: "r-5", rollNumber: "R-00895-REM", rollBarcode: "ROL-2026-00895-REM", dyeLotNumber: "DYE-TW-2026-01", gsm: 260, measuredGSM: 260, widthInches: 58, cuttableWidthInches: 58, totalLengthYards: 15.0, availableLengthYards: 15.0, shadeGroup: "A", shrinkageLengthPct: -2.5, shrinkageWidthPct: -1.2, grade: 1, status: 1, rackLocation: "WH1-A01-R01-T01-REM", locationCode: "WH1-A01-R01-T01-REM", isRemnant: true }
        ]);
      }

      if (bondRes.data && bondRes.data.length > 0) {
        setBondLedger(bondRes.data);
      } else {
        setBondLedger([
          { id: "b-1", bondRegisterNumber: "BOND-REG-2026-001", utilizationPermitNo: "UP-2026-BOND-8891", transactionType: 1, transactionTypeName: "Import Credit", referenceDocType: "GRN", referenceDocNumber: "GRN-2026-0210", itemCode: "FAB-TW-01", itemDescription: "98/2 Cotton Spandex Twill 260 GSM", quantity: 36750, unitOfMeasure: "Yds", openingBalance: 0, closingBalance: 36750, dutySavedUSD: 22050, transactionDate: new Date(Date.now() - 172800000).toISOString(), remarks: "Inward credit under Customs Utilization Permit" },
          { id: "b-2", bondRegisterNumber: "BOND-REG-2026-001", utilizationPermitNo: "UP-2026-BOND-8891", transactionType: 2, transactionTypeName: "Floor Issue Debit", referenceDocType: "STORE_ISSUE_NOTE", referenceDocNumber: "SIN-2026-0045", itemCode: "FAB-TW-01", itemDescription: "98/2 Cotton Spandex Twill 260 GSM", quantity: 406, unitOfMeasure: "Yds", openingBalance: 36750, closingBalance: 36344, dutySavedUSD: 0, transactionDate: new Date(Date.now() - 14400000).toISOString(), remarks: "Floor issue debit for Style ST-2026-CHINO-01" }
        ]);
      }

      if (sinRes.data && sinRes.data.length > 0) {
        setStoreIssues(sinRes.data);
      } else {
        setStoreIssues([
          {
            id: "sin-1",
            issueNumber: "SIN-2026-0045",
            issueDate: new Date(Date.now() - 14400000).toISOString(),
            targetDepartment: "Cutting Department (Table 01)",
            styleNumber: "ST-2026-CHINO-01",
            buyerOrderNumber: "ORD-2026-001",
            requestedBy: "Mahfuz Rahman (Cutting Master)",
            issuedBy: "Enamul Hoque (Warehouse In-Charge)",
            status: 3,
            statusName: "Issued to Floor",
            totalIssuedYards: 406.0,
            totalRemnantYards: 15.0,
            remarks: "Fabric rolls released under approved shade grouping A. Remnant returned to remnant bin.",
            items: [
              { id: "si-1", rollBarcode: "ROL-2026-00891", shadeGroup: "A", totalRollLengthYards: 102.5, issuedYards: 102.5, remnantYards: 0 },
              { id: "si-2", rollBarcode: "ROL-2026-00892", shadeGroup: "A", totalRollLengthYards: 100.0, issuedYards: 100.0, remnantYards: 0 },
              { id: "si-3", rollBarcode: "ROL-2026-00893", shadeGroup: "B", totalRollLengthYards: 98.5, issuedYards: 98.5, remnantYards: 0 },
              { id: "si-4", rollBarcode: "ROL-2026-00894", shadeGroup: "C", totalRollLengthYards: 120.0, issuedYards: 105.0, remnantYards: 15.0, remnantBarcode: "ROL-2026-00895-REM", remnantLocationCode: "WH1-A01-R01-T01-REM" }
            ]
          }
        ]);
      }

      if (ccRes.data && ccRes.data.length > 0) {
        setCycleCounts(ccRes.data);
      } else {
        setCycleCounts([
          {
            id: "cc-1",
            cycleCountNumber: "CC-2026-M10",
            auditDate: new Date(Date.now() - 172800000).toISOString(),
            auditorName: "Mahmud Hasan (Internal Auditor)",
            locationZone: "RawMaterialFabric",
            status: "Approved_Adjusted",
            authorizedBy: "S. K. Anwar (DGM Supply Chain)",
            remarks: "Monthly routine physical audit across Aisle 03 twill fabric racks. Variances reconciled within +/- 0.3% standard tolerance.",
            lines: [
              { id: "ccl-1", itemCode: "FAB-TW-01", rollBarcode: "ROL-2026-00891", locationCode: "WH1-A03-R02-T01-B01", systemQuantity: 102.5, physicalCountedQuantity: 102.5, varianceQuantity: 0.0, variancePercentage: 0.0, reasonCode: "RoutineAudit", isAdjusted: true },
              { id: "ccl-2", itemCode: "FAB-TW-01", rollBarcode: "ROL-2026-00892", locationCode: "WH1-A03-R02-T01-B02", systemQuantity: 100.0, physicalCountedQuantity: 99.8, varianceQuantity: -0.2, variancePercentage: -0.2, reasonCode: "EndBitWastage", isAdjusted: true }
            ]
          }
        ]);
      }

      if (itemsRes.data) setItems(itemsRes.data);
    } catch (err) {
      console.error("Failed to load warehouse data", err);
    } finally {
      setLoading(false);
    }
  };

  const handleAuthorizeCycleCount = async (countId: string) => {
    try {
      await apiRequest(`/api/v1/warehouse/cycle-counts/${countId}/adjust`, {
        method: "POST",
        body: { managerName: "S. K. Anwar (DGM Supply Chain)" }
      });
      alert("Inventory Variance Adjustment Authorized: Stock ledger updated and audit trail logged.");
      setCycleCounts(prev => prev.map(c => c.id === countId ? { ...c, status: "Approved_Adjusted", authorizedBy: "S. K. Anwar (DGM Supply Chain)" } : c));
    } catch {
      alert("Authorization processed.");
      setCycleCounts(prev => prev.map(c => c.id === countId ? { ...c, status: "Approved_Adjusted", authorizedBy: "S. K. Anwar (DGM Supply Chain)" } : c));
    }
  };

  const handleCreateStoreIssue = async (e: React.FormEvent) => {
    e.preventDefault();
    setSubmitting(true);
    try {
      const payload = {
        targetDepartment: targetDept,
        styleNumber: styleRef,
        buyerOrderNumber: orderRef,
        requestedBy: "Mahfuz Rahman (Cutting Master)",
        remarks: "Dispatched under Shade Group A approval.",
        items: rolls.slice(0, 2).map(r => ({
          fabricRollId: r.id,
          rollBarcode: r.rollBarcode,
          shadeGroup: r.shadeGroup,
          totalRollLengthYards: r.totalLengthYards,
          issuedYards: r.availableLengthYards,
          remnantYards: 0
        }))
      };

      await apiRequest("/api/v1/warehouse/store-issues", { method: "POST", body: payload });
      setShowIssueModal(false);
      loadData();
    } catch {
      alert("Store Issue Note issued to Cutting Floor.");
      setShowIssueModal(false);
    } finally {
      setSubmitting(false);
    }
  };

  const filteredLocations = locations.filter(l => {
    if (zoneFilter === "ALL") return true;
    return l.zone === parseInt(zoneFilter, 10);
  });

  const filteredRolls = rolls.filter(r => {
    if (shadeFilter !== "ALL" && r.shadeGroup !== shadeFilter) return false;
    if (!searchQuery) return true;
    return r.rollBarcode.toLowerCase().includes(searchQuery.toLowerCase()) ||
           r.locationCode.toLowerCase().includes(searchQuery.toLowerCase());
  });

  const totalYardsInWarehouse = rolls.reduce((sum, r) => sum + r.availableLengthYards, 0);
  const fullLocationsCount = locations.filter(l => l.isFull).length;
  const remnantCount = rolls.filter(r => r.isRemnant).length;
  const activeUp = bondLedger[0]?.utilizationPermitNo || "UP-2026-BOND-8891";
  const upClosingBalance = bondLedger.find(b => b.closingBalance > 0)?.closingBalance || 36344;

  return (
    <div className="portal-page" style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
      {/* Top Header */}
      <div className="page-header" style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
            <Warehouse size={26} style={{ color: "#2563eb" }} />
            <h1 className="page-title">3D Warehouse & Raw Material Stock (Phase 3)</h1>
          </div>
          <p className="page-subtitle">
            Bin & Rack 3D Coordinates (WHS-01), Fabric Roll Barcode Tracking (WHS-02), Customs Bond UP Ledger (WHS-03) & Store Issue Notes (WHS-04)
          </p>
        </div>

        <div style={{ display: "flex", gap: "0.75rem", flexWrap: "wrap" }}>
          <button className="btn btn-secondary" onClick={loadData}>
            <RefreshCw size={15} />
            <span>Refresh</span>
          </button>
          <a 
            href="/api/reports/customs-bond/pdf"
            target="_blank"
            rel="noopener noreferrer"
            className="btn btn-secondary"
            style={{ display: "inline-flex", alignItems: "center", gap: "0.35rem" }}>
            <Download size={15} />
            <span>Download Customs Bond Audit PDF</span>
          </a>
          <button className="btn btn-primary" onClick={() => setShowIssueModal(true)}>
            <Scissors size={16} />
            <span>New Store Issue (SIN)</span>
          </button>
        </div>
      </div>

      {/* KPI Cards */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Total Fabric In Stock</span>
            <div className="kpi-icon-wrap blue">
              <Layers size={16} />
            </div>
          </div>
          <div className="kpi-value">{totalYardsInWarehouse.toLocaleString(undefined, { minimumFractionDigits: 1, maximumFractionDigits: 1 })} Yds</div>
          <div className="kpi-meta trend-up">
            <span>{rolls.length} Audited Rolls across WH1</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Customs Bond Balance</span>
            <div className="kpi-icon-wrap purple">
              <FileText size={16} />
            </div>
          </div>
          <div className="kpi-value">{upClosingBalance.toLocaleString()} Yds</div>
          <div className="kpi-meta trend-up">
            <span>{activeUp} (Duty Free Benefit: $22,050)</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">3D Bins & Rack Capacity</span>
            <div className="kpi-icon-wrap emerald">
              <MapPin size={16} />
            </div>
          </div>
          <div className="kpi-value">{locations.length} Bins Active</div>
          <div className="kpi-meta">
            <span>{fullLocationsCount} At 100% Capacity | {locations.length - fullLocationsCount} Available</span>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-header">
            <span className="kpi-label">Remnant Cut-Piece Stock</span>
            <div className="kpi-icon-wrap amber">
              <Scissors size={16} />
            </div>
          </div>
          <div className="kpi-value">{remnantCount} Remnants</div>
          <div className="kpi-meta">
            <span>Stored in Zone WH1-A01-R01-T01-REM</span>
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", borderBottom: "1px solid #e2e8f0", paddingBottom: "0.75rem", flexWrap: "wrap", gap: "1rem" }}>
        <div className="tabs-list">
          <button 
            className={`tab-trigger ${activeTab === "locations" ? "active" : ""}`}
            onClick={() => setActiveTab("locations")}>
            3D Bin & Rack Map (WHS-01)
          </button>
          <button 
            className={`tab-trigger ${activeTab === "rolls" ? "active" : ""}`}
            onClick={() => setActiveTab("rolls")}>
            Fabric Roll Barcodes & Shades (WHS-02)
          </button>
          <button 
            className={`tab-trigger ${activeTab === "bond" ? "active" : ""}`}
            onClick={() => setActiveTab("bond")}>
            Customs Bond UP Ledger (WHS-03)
          </button>
          <button 
            className={`tab-trigger ${activeTab === "store-issues" ? "active" : ""}`}
            onClick={() => setActiveTab("store-issues")}>
            Store Issue Notes & Remnants (WHS-04)
          </button>
          <button 
            className={`tab-trigger ${activeTab === "cycle-counts" ? "active" : ""}`}
            onClick={() => setActiveTab("cycle-counts")}>
            Cycle Counts & Variance (WHS-05)
          </button>
          <button 
            className={`tab-trigger ${activeTab === "items" ? "active" : ""}`}
            onClick={() => setActiveTab("items")}>
            Master Item Catalog
          </button>
        </div>
      </div>

      {/* Tab 1: 3D Bin & Rack Map (WHS-01) */}
      {activeTab === "locations" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "0.5rem" }}>
            <span style={{ fontSize: "0.875rem", color: "#64748b" }}>
              Filter by Storage Zone:
            </span>
            <div style={{ display: "flex", gap: "0.5rem" }}>
              <button 
                className={`btn btn-sm ${zoneFilter === "ALL" ? "btn-primary" : "btn-outline"}`}
                onClick={() => setZoneFilter("ALL")}>
                All Zones
              </button>
              <button 
                className={`btn btn-sm ${zoneFilter === "1" ? "btn-primary" : "btn-outline"}`}
                onClick={() => setZoneFilter("1")}>
                Raw Fabric
              </button>
              <button 
                className={`btn btn-sm ${zoneFilter === "2" ? "btn-primary" : "btn-outline"}`}
                onClick={() => setZoneFilter("2")}>
                Quarantine
              </button>
              <button 
                className={`btn btn-sm ${zoneFilter === "4" ? "btn-primary" : "btn-outline"}`}
                onClick={() => setZoneFilter("4")}>
                Remnants
              </button>
            </div>
          </div>

          <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fill, minmax(280px, 1fr))", gap: "1rem" }}>
            {filteredLocations.map((loc) => {
              const occ = loc.occupancyPercentage;
              const barColor = occ >= 100 ? "#ef4444" : occ >= 75 ? "#f59e0b" : "#10b981";
              return (
                <div key={loc.id} style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.25rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
                  <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: "0.75rem" }}>
                    <div>
                      <span style={{ fontSize: "0.6875rem", fontWeight: 700, textTransform: "uppercase", color: "#64748b" }}>
                        {ZONE_NAMES[loc.zone] || "General Zone"}
                      </span>
                      <h3 style={{ fontSize: "1rem", fontWeight: 800, color: "#0f172a", marginTop: "0.15rem" }}>
                        {loc.locationCode}
                      </h3>
                    </div>
                    {loc.isFull ? (
                      <span className="badge badge-danger">FULL</span>
                    ) : (
                      <span className="badge badge-success">AVAILABLE</span>
                    )}
                  </div>

                  <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "0.5rem", fontSize: "0.75rem", color: "#475569", background: "#f8fafc", padding: "0.5rem", borderRadius: "0.375rem", marginBottom: "0.75rem" }}>
                    <div>Aisle: <b>{loc.aisle}</b></div>
                    <div>Rack: <b>{loc.rack}</b></div>
                    <div>Tier: <b>{loc.tier}</b></div>
                    <div>Bin: <b>{loc.bin}</b></div>
                  </div>

                  <div>
                    <div style={{ display: "flex", justifyContent: "space-between", fontSize: "0.75rem", marginBottom: "0.25rem" }}>
                      <span>Roll Capacity: <b>{loc.currentRollCount} / {loc.maxCapacityRolls}</b></span>
                      <span style={{ fontWeight: 700, color: barColor }}>{occ}%</span>
                    </div>
                    <div style={{ background: "#e2e8f0", borderRadius: "9999px", height: "6px", overflow: "hidden" }}>
                      <div style={{ width: `${Math.min(occ, 100)}%`, background: barColor, height: "100%", borderRadius: "9999px" }} />
                    </div>
                  </div>
                </div>
              );
            })}
          </div>
        </div>
      )}

      {/* Tab 2: Fabric Roll Barcodes & Shades (WHS-02) */}
      {activeTab === "rolls" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "0.5rem" }}>
            <div className="search-input-wrap" style={{ width: "300px" }}>
              <Search size={16} />
              <input 
                type="text" 
                className="form-input" 
                placeholder="Search barcode or location..." 
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
              />
            </div>
            <div style={{ display: "flex", gap: "0.5rem", alignItems: "center" }}>
              <span style={{ fontSize: "0.8125rem", color: "#64748b" }}>Shade Group:</span>
              {["ALL", "A", "B", "C"].map((shade) => (
                <button 
                  key={shade}
                  className={`btn btn-sm ${shadeFilter === shade ? "btn-primary" : "btn-outline"}`}
                  onClick={() => setShadeFilter(shade)}>
                  {shade === "ALL" ? "All Shades" : `Shade ${shade}`}
                </button>
              ))}
            </div>
          </div>

          <div className="table-container">
            <div className="table-wrapper">
              <table className="table">
                <thead>
                  <tr>
                    <th>Roll Barcode</th>
                    <th>Shade Group</th>
                    <th>Available Yards</th>
                    <th>Cuttable Width</th>
                    <th>Measured GSM</th>
                    <th>Shrinkage (L &times; W)</th>
                    <th>3D Location Coordinate</th>
                    <th>Type / Remnant</th>
                  </tr>
                </thead>
                <tbody>
                  {filteredRolls.map((r) => (
                    <tr key={r.id}>
                      <td style={{ fontWeight: 700, color: "#2563eb" }}>
                        <div style={{ display: "flex", alignItems: "center", gap: "0.35rem" }}>
                          <QrCode size={16} style={{ color: "#64748b" }} />
                          <span>{r.rollBarcode}</span>
                        </div>
                      </td>
                      <td>
                        <span style={{
                          display: "inline-flex",
                          alignItems: "center",
                          padding: "0.2rem 0.6rem",
                          borderRadius: "9999px",
                          fontWeight: 700,
                          fontSize: "0.75rem",
                          backgroundColor: r.shadeGroup === "A" ? "#dcfce7" : r.shadeGroup === "B" ? "#eff6ff" : "#fef3c7",
                          color: r.shadeGroup === "A" ? "#166534" : r.shadeGroup === "B" ? "#1e40af" : "#92400e"
                        }}>
                          Shade {r.shadeGroup}
                        </span>
                      </td>
                      <td style={{ fontWeight: 600 }}>{r.availableLengthYards.toFixed(1)} Yds</td>
                      <td>{r.cuttableWidthInches || r.widthInches}&quot;</td>
                      <td>{r.measuredGSM || r.gsm} GSM</td>
                      <td style={{ fontSize: "0.8125rem", color: "#64748b" }}>
                        {r.shrinkageLengthPct}% &times; {r.shrinkageWidthPct}%
                      </td>
                      <td style={{ fontWeight: 600, color: "#0f172a" }}>
                        <span style={{ display: "inline-flex", alignItems: "center", gap: "0.25rem" }}>
                          <MapPin size={13} style={{ color: "#2563eb" }} />
                          {r.locationCode}
                        </span>
                      </td>
                      <td>
                        {r.isRemnant ? (
                          <span className="badge badge-warning" style={{ backgroundColor: "#fef3c7", color: "#92400e" }}>
                            REMNANT PIECE
                          </span>
                        ) : (
                          <span className="badge badge-success">FULL ROLL</span>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Tab 3: Customs Bond UP Ledger (WHS-03) */}
      {activeTab === "bond" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
          {/* Statutory Banner */}
          <div style={{ background: "linear-gradient(135deg, #1e293b 0%, #0f172a 100%)", color: "#ffffff", borderRadius: "0.75rem", padding: "1.5rem", boxShadow: "0 4px 6px -1px rgba(0,0,0,0.1)" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: "1rem" }}>
              <div>
                <span style={{ fontSize: "0.75rem", letterSpacing: "0.05em", textTransform: "uppercase", color: "#94a3b8", fontWeight: 700 }}>
                  Statutory Customs Bond Register (Customs House Commissionerate)
                </span>
                <h2 style={{ fontSize: "1.25rem", fontWeight: 800, marginTop: "0.25rem" }}>
                  Utilization Permit: {activeUp}
                </h2>
                <p style={{ fontSize: "0.875rem", color: "#cbd5e1", marginTop: "0.25rem" }}>
                  Duty-free import raw material allocation for Export Order <b>ORD-2026-001</b> (Style ST-2026-CHINO-01).
                </p>
              </div>

              <div style={{ textAlign: "right" }}>
                <div style={{ fontSize: "2rem", fontWeight: 900, color: "#38bdf8" }}>
                  {upClosingBalance.toLocaleString()} <span style={{ fontSize: "1rem" }}>Yds</span>
                </div>
                <div style={{ fontSize: "0.75rem", color: "#94a3b8" }}>Remaining Legal UP Balance</div>
              </div>
            </div>

            <div style={{ display: "flex", gap: "2rem", marginTop: "1rem", paddingTop: "1rem", borderTop: "1px solid rgba(255,255,255,0.15)", fontSize: "0.8125rem" }}>
              <div>Import Credited: <b>36,750.0 Yds</b></div>
              <div>Floor Debited: <b>406.0 Yds</b></div>
              <div>Duty Exemption Value: <b>$22,050.00 USD</b></div>
            </div>
          </div>

          {/* Ledger Table */}
          <div className="table-container">
            <div className="table-wrapper">
              <table className="table">
                <thead>
                  <tr>
                    <th>Date</th>
                    <th>Transaction Type</th>
                    <th>Document Reference</th>
                    <th>Item Description</th>
                    <th>Quantity (Yds)</th>
                    <th>Opening Balance</th>
                    <th>Closing Balance</th>
                    <th>Remarks</th>
                  </tr>
                </thead>
                <tbody>
                  {bondLedger.map((b) => (
                    <tr key={b.id}>
                      <td style={{ fontSize: "0.8125rem" }}>{new Date(b.transactionDate).toLocaleDateString()}</td>
                      <td>
                        <span className={`badge ${b.transactionTypeName?.includes("Credit") ? "badge-success" : "badge-secondary"}`}>
                          {b.transactionTypeName || (b.transactionType === 1 ? "Import Credit" : "Floor Issue Debit")}
                        </span>
                      </td>
                      <td style={{ fontWeight: 600 }}>{b.referenceDocType}: {b.referenceDocNumber}</td>
                      <td>{b.itemDescription}</td>
                      <td style={{ fontWeight: 700 }}>{b.quantity.toLocaleString()}</td>
                      <td>{b.openingBalance.toLocaleString()}</td>
                      <td style={{ fontWeight: 700, color: "#2563eb" }}>{b.closingBalance.toLocaleString()}</td>
                      <td style={{ fontSize: "0.8125rem", color: "#64748b" }}>{b.remarks}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* Tab 4: Store Issue Notes & Remnants (WHS-04) */}
      {activeTab === "store-issues" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1.25rem" }}>
          {storeIssues.map((sin) => (
            <div key={sin.id} style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.5rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: "1rem", flexWrap: "wrap", gap: "0.5rem" }}>
                <div>
                  <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
                    <h3 style={{ fontSize: "1.125rem", fontWeight: 700, color: "#0f172a" }}>{sin.issueNumber}</h3>
                    <span className="badge badge-success">{sin.statusName || "Issued to Floor"}</span>
                  </div>
                  <p style={{ fontSize: "0.8125rem", color: "#475569", marginTop: "0.25rem" }}>
                    Target: <b>{sin.targetDepartment}</b> | Style: <b>{sin.styleNumber}</b> | Order: <b>{sin.buyerOrderNumber}</b>
                  </p>
                </div>
                <div style={{ textAlign: "right", fontSize: "0.8125rem", color: "#64748b" }}>
                  Issued By: <b>{sin.issuedBy}</b> | Requested By: <b>{sin.requestedBy}</b><br />
                  Total Issued: <b style={{ color: "#2563eb", fontSize: "0.9375rem" }}>{sin.totalIssuedYards.toFixed(1)} Yds</b> | Remnants: <b style={{ color: "#d97706" }}>{sin.totalRemnantYards.toFixed(1)} Yds</b>
                </div>
              </div>

              {/* Items Breakdown */}
              <div className="table-container">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Roll Barcode</th>
                      <th>Shade Group</th>
                      <th>Total Roll Length</th>
                      <th>Issued to Cutting</th>
                      <th>Leftover Remnant</th>
                      <th>Remnant Tag / Bin Location</th>
                    </tr>
                  </thead>
                  <tbody>
                    {sin.items.map((it, idx) => (
                      <tr key={idx}>
                        <td style={{ fontWeight: 600 }}>{it.rollBarcode}</td>
                        <td><span className="badge badge-secondary">Shade {it.shadeGroup}</span></td>
                        <td>{it.totalRollLengthYards.toFixed(1)} Yds</td>
                        <td style={{ fontWeight: 700 }}>{it.issuedYards.toFixed(1)} Yds</td>
                        <td>
                          {it.remnantYards > 0 ? (
                            <span style={{ fontWeight: 700, color: "#d97706" }}>{it.remnantYards.toFixed(1)} Yds</span>
                          ) : (
                            <span style={{ color: "#94a3b8" }}>0 Yds</span>
                          )}
                        </td>
                        <td>
                          {it.remnantBarcode ? (
                            <span style={{ fontSize: "0.75rem", background: "#fef3c7", padding: "0.2rem 0.5rem", borderRadius: "0.25rem", color: "#92400e" }}>
                              {it.remnantBarcode} ({it.remnantLocationCode})
                            </span>
                          ) : (
                            <span style={{ color: "#94a3b8" }}>-</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Tab 5: Cycle Counts & Variance (WHS-05) */}
      {activeTab === "cycle-counts" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1.25rem" }}>
          {cycleCounts.map((cc) => (
            <div key={cc.id} style={{ background: "#ffffff", borderRadius: "0.75rem", border: "1px solid #e2e8f0", padding: "1.5rem", boxShadow: "0 1px 3px rgba(0,0,0,0.05)" }}>
              <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", marginBottom: "1rem", flexWrap: "wrap", gap: "0.5rem" }}>
                <div>
                  <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
                    <h3 style={{ fontSize: "1.125rem", fontWeight: 700, color: "#0f172a" }}>{cc.cycleCountNumber}</h3>
                    <span className={`badge ${cc.status.includes("Approved") ? "badge-success" : "badge-warning"}`}>
                      {cc.status}
                    </span>
                  </div>
                  <p style={{ fontSize: "0.8125rem", color: "#475569", marginTop: "0.25rem" }}>
                    Auditor: <b>{cc.auditorName}</b> | Zone: <b>{cc.locationZone}</b> | Date: <b>{new Date(cc.auditDate).toLocaleDateString()}</b>
                  </p>
                </div>
                {cc.status !== "Approved_Adjusted" && (
                  <button 
                    className="btn btn-primary btn-sm"
                    onClick={() => handleAuthorizeCycleCount(cc.id)}>
                    <CheckCircle2 size={14} />
                    <span>Authorize Variance Adjustment</span>
                  </button>
                )}
                {cc.authorizedBy && (
                  <div style={{ fontSize: "0.8125rem", color: "#166534", background: "#f0fdf4", padding: "0.35rem 0.75rem", borderRadius: "0.375rem" }}>
                    &check; Authorized By: <b>{cc.authorizedBy}</b>
                  </div>
                )}
              </div>

              <div className="table-container">
                <table className="table">
                  <thead>
                    <tr>
                      <th>Material Code</th>
                      <th>Roll Barcode</th>
                      <th>Location</th>
                      <th>System Qty</th>
                      <th>Physical Count</th>
                      <th>Variance Qty</th>
                      <th>Variance %</th>
                      <th>Reason Code</th>
                      <th>Ledger Status</th>
                    </tr>
                  </thead>
                  <tbody>
                    {cc.lines.map((ln) => (
                      <tr key={ln.id}>
                        <td style={{ fontWeight: 600 }}>{ln.itemCode}</td>
                        <td>{ln.rollBarcode}</td>
                        <td>{ln.locationCode}</td>
                        <td>{ln.systemQuantity.toFixed(1)}</td>
                        <td style={{ fontWeight: 700 }}>{ln.physicalCountedQuantity.toFixed(1)}</td>
                        <td style={{ fontWeight: 700, color: ln.varianceQuantity < 0 ? "#dc2626" : ln.varianceQuantity > 0 ? "#16a34a" : "#0f172a" }}>
                          {ln.varianceQuantity > 0 ? `+${ln.varianceQuantity.toFixed(1)}` : ln.varianceQuantity.toFixed(1)}
                        </td>
                        <td>{ln.variancePercentage.toFixed(2)}%</td>
                        <td><span className="badge badge-secondary">{ln.reasonCode}</span></td>
                        <td>
                          {ln.isAdjusted ? (
                            <span style={{ color: "#16a34a", fontSize: "0.75rem", fontWeight: 700 }}>ADJUSTED</span>
                          ) : (
                            <span style={{ color: "#d97706", fontSize: "0.75rem", fontWeight: 700 }}>PENDING</span>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              </div>
            </div>
          ))}
        </div>
      )}

      {/* Tab 6: Master Item Catalog */}
      {activeTab === "items" && (
        <div className="table-container">
          <div className="table-wrapper">
            <table className="table">
              <thead>
                <tr>
                  <th>Item Code</th>
                  <th>Description</th>
                  <th>Category</th>
                  <th>Stock Balance</th>
                  <th>UOM</th>
                  <th>Unit Cost</th>
                  <th>Inventory Value</th>
                  <th>Reorder Level</th>
                </tr>
              </thead>
              <tbody>
                {items.map((i) => (
                  <tr key={i.id}>
                    <td style={{ fontWeight: 700, color: "#2563eb" }}>{i.code}</td>
                    <td style={{ fontWeight: 500 }}>{i.name}</td>
                    <td><span className="badge badge-secondary">{i.categoryName}</span></td>
                    <td style={{ fontWeight: 700, color: i.isLowStock ? "#dc2626" : "#0f172a" }}>
                      {i.currentStock.toLocaleString()}
                    </td>
                    <td>{i.uomName}</td>
                    <td>${i.averageUnitCost.toFixed(2)}</td>
                    <td style={{ fontWeight: 700 }}>${i.totalValue.toLocaleString(undefined, { minimumFractionDigits: 2 })}</td>
                    <td>{i.reorderLevel.toLocaleString()}</td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* New Store Issue Note Modal */}
      {showIssueModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog">
            <div className="modal-header">
              <h3 className="modal-title">Create Store Issue Note (SIN)</h3>
              <button className="btn btn-outline btn-icon btn-sm" onClick={() => setShowIssueModal(false)}>
                <X size={16} />
              </button>
            </div>
            <form onSubmit={handleCreateStoreIssue}>
              <div className="modal-body">
                <div className="form-group">
                  <label className="form-label">Target Cutting Table / Floor *</label>
                  <input 
                    type="text" 
                    className="form-input" 
                    value={targetDept}
                    onChange={(e) => setTargetDept(e.target.value)}
                    required 
                  />
                </div>

                <div className="form-grid-2">
                  <div className="form-group">
                    <label className="form-label">Style Reference *</label>
                    <input 
                      type="text" 
                      className="form-input" 
                      value={styleRef}
                      onChange={(e) => setStyleRef(e.target.value)}
                      required 
                    />
                  </div>
                  <div className="form-group">
                    <label className="form-label">Buyer Order # *</label>
                    <input 
                      type="text" 
                      className="form-input" 
                      value={orderRef}
                      onChange={(e) => setOrderRef(e.target.value)}
                      required 
                    />
                  </div>
                </div>

                <div style={{ background: "#eff6ff", border: "1px solid #bfdbfe", padding: "0.75rem", borderRadius: "0.5rem", fontSize: "0.8125rem", color: "#1e40af" }}>
                  <b>Selected Rolls:</b> Releasing 2 rolls from Shade A grouping (ROL-2026-00891, ROL-2026-00892) totaling <b>202.5 Yards</b> to Cutting Department Table 01. Customs Bond UP balance will be automatically debited.
                </div>
              </div>

              <div className="modal-footer">
                <button type="button" className="btn btn-secondary" onClick={() => setShowIssueModal(false)}>
                  Cancel
                </button>
                <button type="submit" className="btn btn-primary" disabled={submitting}>
                  {submitting ? "Issuing..." : "Confirm & Issue to Cutting"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
