"use client";

import React, { useState, useEffect } from "react";
import { useAuth } from "@/lib/auth-context";
import { apiRequest } from "@/lib/api";
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
  AlertCircle,
  FileText,
  Download,
  ShieldAlert,
  ShieldCheck,
  Thermometer,
  Droplets,
  Sliders,
  Check,
  X,
  Truck,
  Percent,
  Cpu
} from "lucide-react";

interface CutBundle {
  id: string;
  bundleNumber: string;
  cuttingOrderId: string;
  styleNumber?: string;
  size: string;
  color: string;
  shadeGroup?: string;
  componentName?: string;
  quantity: number;
  barcode: string;
  serialStart: number;
  serialEnd: number;
  bundleSequence?: number;
  totalBundlesInOrder?: number;
  currentFloorStation?: string;
  currentSection: number; // 0: Cutting, 1: Sewing, 2: QC, 3: Washing, 4: Finishing, 5: Packed
  isCompleted: boolean;
}

interface CuttingOrder {
  id: string;
  cuttingOrderNumber: string;
  buyerOrderId: string;
  orderNumber?: string;
  styleNumber: string;
  fabricType: string;
  totalRequiredQty: number;
  totalPlies: number;
  markerLength: number;
  fabricWidth: number;
  efficiencyPercentage: number;
  status: number; // 0: Draft, 1: SpreadingLay, 2: InProgress, 3: CutCompleted, 4: Bundled, 5: Relaxing, 6: ReadyForCut, 7: IssuedToSewing
  tableNumber: string;
  cuttingDate: string;
  markerName?: string;
  shadeGroup?: string;
  totalGarmentsPlanned?: number;
  actualFabricConsumedYds?: number;
  endLossRemnantYds?: number;
  cuttingWastageVariancePct?: number;
  allocatedRollBarcodes?: string;
  approvedBy?: string;
  bundles?: CutBundle[];
}

interface CADMarker {
  id: string;
  markerName: string;
  styleNumber: string;
  cadSoftware: string;
  cuttableWidthInches: number;
  markerLengthYards: number;
  garmentsPerMarker: number;
  sizeDistributionRatio: string;
  markerEfficiencyPct: number;
  plannedConsumptionPerPcYds: number;
  fileFormat: string;
}

interface RelaxationSession {
  id: string;
  fabricRollId: string;
  rollBarcode: string;
  dyeLotNumber: string;
  fabricType: string;
  unrolledAtUtc: string;
  mandatoryDurationHours: number;
  unlockedAtUtc: string;
  isUnlocked: boolean;
  remainingHours: number;
  remainingMinutes: number;
  ambientTemperatureC: number;
  relativeHumidityPct: number;
  operatorId: string;
  statusName: string;
}

interface FusingMachineLog {
  id: string;
  cutOrderNumber: string;
  machineId: string;
  componentName: string;
  interliningType: string;
  temperatureC: number;
  pressureBar: number;
  speedSeconds: number;
  operatorName: string;
  isApproved: boolean;
  remarks: string;
  loggedAtUtc: string;
}

interface SubcontractChallan {
  id: string;
  challanNumber: string;
  subcontractorName: string;
  styleNumber: string;
  orderNumber: string;
  fabricSentYds: number;
  expectedCutPanels: number;
  receivedCutPanels: number;
  defectivePanels: number;
  shortageCount: number;
  shortagePercentage: number;
  statusName: string;
  dispatchedAtUtc: string;
  receivedAtUtc?: string;
  reconciledBy?: string;
  remarks: string;
}

const STATUS_LABELS: { [key: number]: { label: string; color: string; bg: string } } = {
  0: { label: "Draft", color: "var(--text-muted)", bg: "rgba(255,255,255,0.05)" },
  1: { label: "Spreading Lay", color: "#f59e0b", bg: "rgba(245,158,11,0.15)" },
  2: { label: "In Progress", color: "#38bdf8", bg: "rgba(56,189,248,0.15)" },
  3: { label: "Cut Completed", color: "#10b981", bg: "rgba(16,185,129,0.15)" },
  4: { label: "Bundled", color: "#a855f7", bg: "rgba(168,85,247,0.15)" },
  5: { label: "Relaxing Lock", color: "#ef4444", bg: "rgba(239,68,68,0.15)" },
  6: { label: "Ready For Cut", color: "#06b6d4", bg: "rgba(6,182,212,0.15)" },
  7: { label: "Issued to Sewing", color: "#8b5cf6", bg: "rgba(139,92,246,0.15)" }
};

export default function CuttingPage() {
  const { hasPermission } = useAuth();
  const canCreate = hasPermission("Cutting.Create");

  const [activeTab, setActiveTab] = useState<"orders" | "relaxation" | "markers" | "bundles" | "fusing" | "subcontract">("orders");
  const [orders, setOrders] = useState<CuttingOrder[]>([]);
  const [markers, setMarkers] = useState<CADMarker[]>([]);
  const [relaxationSessions, setRelaxationSessions] = useState<RelaxationSession[]>([]);
  const [bundles, setBundles] = useState<CutBundle[]>([]);
  const [fusingLogs, setFusingLogs] = useState<FusingMachineLog[]>([]);
  const [subcontractChallans, setSubcontractChallans] = useState<SubcontractChallan[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [search, setSearch] = useState("");

  // Modals & Feedback
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showUnrollModal, setShowUnrollModal] = useState(false);
  const [showZplModal, setShowZplModal] = useState(false);
  const [selectedBundleForZpl, setSelectedBundleForZpl] = useState<CutBundle | null>(null);
  const [spreadingGateFeedback, setSpreadingGateFeedback] = useState<{ allowed: boolean; message: string; cutOrder: string } | null>(null);
  const [authorizingOrderId, setAuthorizingOrderId] = useState<string | null>(null);

  // New Cut Order Form
  const [newOrder, setNewOrder] = useState({
    styleNumber: "ST-2026-CHINO-01",
    fabricType: "98/2 Cotton Spandex Twill (260 GSM)",
    totalRequiredQty: 240,
    totalPlies: 60,
    markerLength: 8.42,
    fabricWidth: 58.0,
    efficiencyPercentage: 87.2,
    tableNumber: "Table-03"
  });

  // Unrolling Form
  const [unrollForm, setUnrollForm] = useState({
    rollBarcode: "ROL-2026-00895",
    dyeLotNumber: "LOT-TW-2026-01",
    fabricType: "98/2 Cotton Spandex Twill (260 GSM)",
    durationHours: 24,
    ambientTemp: 24.5,
    humidity: 65.0,
    operatorId: "Cutter-Master-01"
  });

  const fetchData = async () => {
    setLoading(true);
    try {
      const [resOrders, resMarkers, resRelax, resBundles, resFusing, resSub] = await Promise.all([
        apiRequest<CuttingOrder[]>("/api/v1/cutting/orders"),
        apiRequest<CADMarker[]>("/api/v1/cutting/markers"),
        apiRequest<RelaxationSession[]>("/api/v1/cutting/relaxation-sessions"),
        apiRequest<CutBundle[]>("/api/v1/cutting/bundles"),
        apiRequest<FusingMachineLog[]>("/api/v1/cutting/fusing-logs"),
        apiRequest<SubcontractChallan[]>("/api/v1/cutting/subcontract-challans")
      ]);

      if (resOrders.data && resOrders.data.length > 0) {
        setOrders(resOrders.data);
      } else {
        setOrders([
          {
            id: "cut-chino",
            cuttingOrderNumber: "CUT-2026-0001",
            buyerOrderId: "ord-chino",
            orderNumber: "BO-2026-0890",
            styleNumber: "ST-2026-CHINO-01",
            fabricType: "98/2 Cotton Spandex Twill (260 GSM)",
            totalRequiredQty: 240,
            totalPlies: 60,
            markerLength: 8.42,
            fabricWidth: 58.0,
            efficiencyPercentage: 87.20,
            status: 3, // Cut Completed
            tableNumber: "Table-03",
            cuttingDate: new Date().toISOString(),
            markerName: "MKR-CHINO-SZ32-34-A",
            shadeGroup: "Shade A",
            totalGarmentsPlanned: 240,
            actualFabricConsumedYds: 508.5,
            endLossRemnantYds: 3.5,
            cuttingWastageVariancePct: 0.68,
            allocatedRollBarcodes: "ROL-2026-00891, ROL-2026-00892, ROL-2026-00893",
            approvedBy: "Tariqul Islam (Cutting In-charge)"
          },
          {
            id: "cut-polo",
            cuttingOrderNumber: "CO-8801",
            buyerOrderId: "ord-polo",
            orderNumber: "BO-2609-1001",
            styleNumber: "ST-882",
            fabricType: "100% Pique Cotton 220 GSM",
            totalRequiredQty: 2500,
            totalPlies: 80,
            markerLength: 6.85,
            fabricWidth: 62.0,
            efficiencyPercentage: 88.60,
            status: 3,
            tableNumber: "Table-01",
            cuttingDate: new Date(Date.now() - 86400000 * 2).toISOString(),
            shadeGroup: "Shade A",
            totalGarmentsPlanned: 2500,
            approvedBy: "Tariqul Islam"
          },
          {
            id: "cut-tee",
            cuttingOrderNumber: "CO-8802",
            buyerOrderId: "ord-tee",
            orderNumber: "BO-2609-1002",
            styleNumber: "ST-904",
            fabricType: "Single Jersey 160 GSM",
            totalRequiredQty: 5000,
            totalPlies: 120,
            markerLength: 5.40,
            fabricWidth: 64.0,
            efficiencyPercentage: 91.20,
            status: 2, // In Progress
            tableNumber: "Table-02",
            cuttingDate: new Date(Date.now() - 86400000).toISOString(),
            shadeGroup: "Shade B",
            totalGarmentsPlanned: 5000,
            approvedBy: "Tariqul Islam"
          }
        ]);
      }

      if (resMarkers.data && resMarkers.data.length > 0) {
        setMarkers(resMarkers.data);
      } else {
        setMarkers([
          {
            id: "mkr-1",
            markerName: "MKR-CHINO-SZ32-34-A",
            styleNumber: "ST-2026-CHINO-01",
            cadSoftware: "Gerber AccuMark v16",
            cuttableWidthInches: 58.0,
            markerLengthYards: 8.42,
            garmentsPerMarker: 4,
            sizeDistributionRatio: "32/32: 2 pcs, 34/32: 2 pcs",
            markerEfficiencyPct: 87.20,
            plannedConsumptionPerPcYds: 2.1050,
            fileFormat: "ASTM/AAMA DXF"
          },
          {
            id: "mkr-2",
            markerName: "MKR-POLO-S-XL",
            styleNumber: "ST-882",
            cadSoftware: "Lectra Diamino Expert",
            cuttableWidthInches: 62.0,
            markerLengthYards: 6.85,
            garmentsPerMarker: 4,
            sizeDistributionRatio: "S: 1, M: 1, L: 1, XL: 1",
            markerEfficiencyPct: 88.60,
            plannedConsumptionPerPcYds: 1.7125,
            fileFormat: "HPGL Plotter"
          }
        ]);
      }

      if (resRelax.data && resRelax.data.length > 0) {
        setRelaxationSessions(resRelax.data);
      } else {
        setRelaxationSessions([
          {
            id: "rel-1",
            fabricRollId: "roll-1",
            rollBarcode: "ROL-2026-00891",
            dyeLotNumber: "LOT-TW-2026-01",
            fabricType: "98/2 Cotton Spandex Twill (260 GSM)",
            unrolledAtUtc: new Date(Date.now() - 3600000 * 26).toISOString(),
            mandatoryDurationHours: 24,
            unlockedAtUtc: new Date(Date.now() - 3600000 * 2).toISOString(),
            isUnlocked: true,
            remainingHours: 0,
            remainingMinutes: 0,
            ambientTemperatureC: 24.5,
            relativeHumidityPct: 65.0,
            operatorId: "Cutter-Master-01",
            statusName: "SpreadingAuthorized"
          },
          {
            id: "rel-2",
            fabricRollId: "roll-2",
            rollBarcode: "ROL-2026-00892",
            dyeLotNumber: "LOT-TW-2026-01",
            fabricType: "98/2 Cotton Spandex Twill (260 GSM)",
            unrolledAtUtc: new Date(Date.now() - 3600000 * 25.5).toISOString(),
            mandatoryDurationHours: 24,
            unlockedAtUtc: new Date(Date.now() - 3600000 * 1.5).toISOString(),
            isUnlocked: true,
            remainingHours: 0,
            remainingMinutes: 0,
            ambientTemperatureC: 24.5,
            relativeHumidityPct: 65.0,
            operatorId: "Cutter-Master-01",
            statusName: "SpreadingAuthorized"
          },
          {
            id: "rel-3",
            fabricRollId: "roll-3",
            rollBarcode: "ROL-2026-00893",
            dyeLotNumber: "LOT-TW-2026-01",
            fabricType: "98/2 Cotton Spandex Twill (260 GSM)",
            unrolledAtUtc: new Date(Date.now() - 3600000 * 24.5).toISOString(),
            mandatoryDurationHours: 24,
            unlockedAtUtc: new Date(Date.now() - 3600000 * 0.5).toISOString(),
            isUnlocked: true,
            remainingHours: 0,
            remainingMinutes: 0,
            ambientTemperatureC: 24.2,
            relativeHumidityPct: 66.0,
            operatorId: "Cutter-Master-01",
            statusName: "SpreadingAuthorized"
          },
          {
            id: "rel-4",
            fabricRollId: "roll-4",
            rollBarcode: "ROL-2026-00894",
            dyeLotNumber: "LOT-TW-2026-01",
            fabricType: "98/2 Cotton Spandex Twill (260 GSM)",
            unrolledAtUtc: new Date(Date.now() - 3600000 * 4).toISOString(),
            mandatoryDurationHours: 24,
            unlockedAtUtc: new Date(Date.now() + 3600000 * 20).toISOString(),
            isUnlocked: false,
            remainingHours: 20,
            remainingMinutes: 0,
            ambientTemperatureC: 24.8,
            relativeHumidityPct: 64.0,
            operatorId: "Cutter-Master-02",
            statusName: "Relaxing"
          }
        ]);
      }

      if (resBundles.data && resBundles.data.length > 0) {
        setBundles(resBundles.data);
      } else {
        setBundles([
          { id: "b1", bundleNumber: "BND-2026-0001-01", barcode: "BND-2026-0001-01", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "32/32", color: "Khaki Beige", shadeGroup: "A", componentName: "FRONT PANEL", quantity: 24, serialStart: 1, serialEnd: 24, bundleSequence: 1, totalBundlesInOrder: 10, currentFloorStation: "SEWING_INPUT", currentSection: 1, isCompleted: false },
          { id: "b2", bundleNumber: "BND-2026-0001-02", barcode: "BND-2026-0001-02", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "32/32", color: "Khaki Beige", shadeGroup: "A", componentName: "BACK PANEL", quantity: 24, serialStart: 1, serialEnd: 24, bundleSequence: 2, totalBundlesInOrder: 10, currentFloorStation: "SEWING_INPUT", currentSection: 1, isCompleted: false },
          { id: "b3", bundleNumber: "BND-2026-0001-03", barcode: "BND-2026-0001-03", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "32/32", color: "Khaki Beige", shadeGroup: "A", componentName: "WAISTBAND", quantity: 24, serialStart: 1, serialEnd: 24, bundleSequence: 3, totalBundlesInOrder: 10, currentFloorStation: "FUSING_MACHINE", currentSection: 0, isCompleted: false },
          { id: "b4", bundleNumber: "BND-2026-0001-04", barcode: "BND-2026-0001-04", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "32/32", color: "Khaki Beige", shadeGroup: "A", componentName: "FLY & POCKET", quantity: 24, serialStart: 1, serialEnd: 24, bundleSequence: 4, totalBundlesInOrder: 10, currentFloorStation: "CUTTING_OUTPUT", currentSection: 0, isCompleted: false },
          { id: "b5", bundleNumber: "BND-2026-0001-05", barcode: "BND-2026-0001-05", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "34/32", color: "Khaki Beige", shadeGroup: "A", componentName: "FRONT PANEL", quantity: 24, serialStart: 25, serialEnd: 48, bundleSequence: 5, totalBundlesInOrder: 10, currentFloorStation: "SEWING_INPUT", currentSection: 1, isCompleted: false },
          { id: "b6", bundleNumber: "BND-2026-0001-06", barcode: "BND-2026-0001-06", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "34/32", color: "Khaki Beige", shadeGroup: "A", componentName: "BACK PANEL", quantity: 24, serialStart: 25, serialEnd: 48, bundleSequence: 6, totalBundlesInOrder: 10, currentFloorStation: "SEWING_INPUT", currentSection: 1, isCompleted: false },
          { id: "b7", bundleNumber: "BND-2026-0001-07", barcode: "BND-2026-0001-07", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "34/32", color: "Khaki Beige", shadeGroup: "A", componentName: "WAISTBAND", quantity: 24, serialStart: 25, serialEnd: 48, bundleSequence: 7, totalBundlesInOrder: 10, currentFloorStation: "FUSING_MACHINE", currentSection: 0, isCompleted: false },
          { id: "b8", bundleNumber: "BND-2026-0001-08", barcode: "BND-2026-0001-08", cuttingOrderId: "cut-chino", styleNumber: "ST-2026-CHINO-01", size: "34/32", color: "Khaki Beige", shadeGroup: "A", componentName: "FLY & POCKET", quantity: 24, serialStart: 25, serialEnd: 48, bundleSequence: 8, totalBundlesInOrder: 10, currentFloorStation: "CUTTING_OUTPUT", currentSection: 0, isCompleted: false }
        ]);
      }

      if (resFusing.data && resFusing.data.length > 0) {
        setFusingLogs(resFusing.data);
      } else {
        setFusingLogs([
          {
            id: "fus-1",
            cutOrderNumber: "CUT-2026-0001",
            machineId: "FUS-02",
            componentName: "Waistband",
            interliningType: "Fusible Microdot Woven 70 GSM",
            temperatureC: 150.0,
            pressureBar: 3.5,
            speedSeconds: 12.0,
            operatorName: "Mizanur Rahman",
            isApproved: true,
            remarks: "Adhesion peel test passed (1.45 kg/inch). No surface bubbling.",
            loggedAtUtc: new Date(Date.now() - 3600000 * 15).toISOString()
          }
        ]);
      }

      if (resSub.data && resSub.data.length > 0) {
        setSubcontractChallans(resSub.data);
      } else {
        setSubcontractChallans([
          {
            id: "sub-1",
            challanNumber: "SCC-2026-0012",
            subcontractorName: "Apex Precision Cutting Mills",
            styleNumber: "ST-2026-CHINO-01",
            orderNumber: "BO-2026-0890",
            fabricSentYds: 1200.0,
            expectedCutPanels: 1440,
            receivedCutPanels: 1432,
            defectivePanels: 4,
            shortageCount: 4,
            shortagePercentage: 0.28,
            statusName: "Reconciled",
            dispatchedAtUtc: new Date(Date.now() - 86400000 * 3).toISOString(),
            receivedAtUtc: new Date(Date.now() - 86400000).toISOString(),
            reconciledBy: "Tariqul Islam",
            remarks: "Shortage within 0.5% standard subcontract tolerance. Reconciled."
          }
        ]);
      }
    } catch (err) {
      console.error("Error fetching cutting data:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  const handleAuthorizeSpreading = async (orderId: string, cutOrderNumber: string) => {
    setAuthorizingOrderId(orderId);
    try {
      const res = await apiRequest<{ allowed: boolean; message: string; cutOrderNumber: string; tableNumber: string }>(
        `/api/v1/cutting/orders/${orderId}/authorize-spreading`,
        { method: "POST" }
      );

      if (res.data) {
        setSpreadingGateFeedback({
          allowed: res.data.allowed,
          message: res.data.message,
          cutOrder: cutOrderNumber
        });
        fetchData();
      } else {
        setSpreadingGateFeedback({
          allowed: false,
          message: res.error || "RELAXATION LOCK ACTIVE: Mandatory 24h relaxation required before spreading.",
          cutOrder: cutOrderNumber
        });
      }
    } catch {
      setSpreadingGateFeedback({
        allowed: false,
        message: "Network validation error",
        cutOrder: cutOrderNumber
      });
    } finally {
      setAuthorizingOrderId(null);
    }
  };

  const handleRegisterUnrolling = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      await apiRequest("/api/v1/cutting/relaxation-sessions/unroll", {
        method: "POST",
        body: JSON.stringify({
          fabricRollId: "00000000-0000-0000-0000-000000000000",
          rollBarcode: unrollForm.rollBarcode,
          dyeLotNumber: unrollForm.dyeLotNumber,
          fabricType: unrollForm.fabricType,
          durationHours: Number(unrollForm.durationHours),
          ambientTemperatureC: Number(unrollForm.ambientTemp),
          relativeHumidityPct: Number(unrollForm.humidity),
          operatorId: unrollForm.operatorId
        })
      });
      setShowUnrollModal(false);
      fetchData();
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "24px", minHeight: "100vh" }}>
      {/* Header & Title Banner */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: "16px" }}>
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: "10px", marginBottom: "4px" }}>
            <div style={{
              width: "36px",
              height: "36px",
              borderRadius: "10px",
              background: "linear-gradient(135deg, rgba(6,182,212,0.2), rgba(14,165,233,0.1))",
              border: "1px solid rgba(6,182,212,0.3)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: "#06b6d4"
            }}>
              <Scissors size={20} />
            </div>
            <h1 style={{ fontSize: "1.75rem", fontWeight: 700, letterSpacing: "-0.02em" }}>
              Cutting Execution & CAD Markers
            </h1>
          </div>
          <p style={{ color: "var(--text-muted)", fontSize: "0.9rem" }}>
            Phase 4: CAD marker nesting, 24-hr knit relaxation time-lock gate, lay spreading & thermal bundle ticketing.
          </p>
        </div>

        {/* Global Action Buttons */}
        <div style={{ display: "flex", gap: "10px", flexWrap: "wrap" }}>
          <a
            href="http://localhost:5224/api/reports/cutting-lay-sheet/pdf"
            target="_blank"
            rel="noreferrer"
            className="btn btn-secondary"
            style={{ display: "inline-flex", alignItems: "center", gap: "8px", textDecoration: "none" }}
          >
            <Download size={16} />
            Lay Sheet PDF
          </a>
          <a
            href="http://localhost:5224/api/reports/bundle-tickets/pdf"
            target="_blank"
            rel="noreferrer"
            className="btn btn-secondary"
            style={{ display: "inline-flex", alignItems: "center", gap: "8px", textDecoration: "none" }}
          >
            <Printer size={16} />
            Bundle Tickets PDF
          </a>
          {canCreate && (
            <button
              onClick={() => setShowUnrollModal(true)}
              className="btn btn-primary"
              style={{ display: "flex", alignItems: "center", gap: "8px" }}
            >
              <Clock size={16} />
              Register Relaxation Unroll
            </button>
          )}
        </div>
      </div>

      {/* Top Operational Metrics KPIs */}
      <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(200px, 1fr))", gap: "16px" }}>
        <div className="card" style={{ padding: "16px" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase" }}>Active Cut Orders</div>
          <div style={{ fontSize: "1.75rem", fontWeight: 700, marginTop: "4px", color: "#38bdf8" }}>
            {orders.length} Orders
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginTop: "4px" }}>
            Table 01, 02 & 03 Allocated
          </div>
        </div>

        <div className="card" style={{ padding: "16px" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase" }}>Total Planned Garments</div>
          <div style={{ fontSize: "1.75rem", fontWeight: 700, marginTop: "4px", color: "#10b981" }}>
            {orders.reduce((acc, o) => acc + (o.totalGarmentsPlanned || o.totalRequiredQty || 0), 0).toLocaleString()} Pcs
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginTop: "4px" }}>
            Across 3 Master Styles
          </div>
        </div>

        <div className="card" style={{ padding: "16px" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase" }}>Avg CAD Efficiency</div>
          <div style={{ fontSize: "1.75rem", fontWeight: 700, marginTop: "4px", color: "#a855f7" }}>
            89.00%
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: "4px" }}>
            +2.5% Fabric Saved vs Benchmark
          </div>
        </div>

        <div className="card" style={{ padding: "16px" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase" }}>Relaxation Time-Lock</div>
          <div style={{ fontSize: "1.75rem", fontWeight: 700, marginTop: "4px", color: "#f59e0b" }}>
            {relaxationSessions.filter(s => !s.isUnlocked).length} Active Lock
          </div>
          <div style={{ fontSize: "0.75rem", color: "#10b981", marginTop: "4px" }}>
            {relaxationSessions.filter(s => s.isUnlocked).length} Rolls Cleared for Spreading
          </div>
        </div>

        <div className="card" style={{ padding: "16px" }}>
          <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", textTransform: "uppercase" }}>Component Bundles</div>
          <div style={{ fontSize: "1.75rem", fontWeight: 700, marginTop: "4px", color: "#06b6d4" }}>
            {bundles.length} Bundles
          </div>
          <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginTop: "4px" }}>
            Thermal Barcodes Ready
          </div>
        </div>
      </div>

      {/* Spreading Gate Feedback Banner */}
      {spreadingGateFeedback && (
        <div style={{
          padding: "16px 20px",
          borderRadius: "12px",
          background: spreadingGateFeedback.allowed ? "rgba(16,185,129,0.12)" : "rgba(239,68,68,0.12)",
          border: `1px solid ${spreadingGateFeedback.allowed ? "rgba(16,185,129,0.3)" : "rgba(239,68,68,0.3)"}`,
          display: "flex",
          alignItems: "center",
          justifyContent: "space-between",
          gap: "16px"
        }}>
          <div style={{ display: "flex", alignItems: "center", gap: "12px" }}>
            {spreadingGateFeedback.allowed ? (
              <ShieldCheck size={24} color="#10b981" />
            ) : (
              <ShieldAlert size={24} color="#ef4444" />
            )}
            <div>
              <div style={{ fontWeight: 600, color: spreadingGateFeedback.allowed ? "#10b981" : "#ef4444" }}>
                {spreadingGateFeedback.allowed ? "SPREADING AUTHORIZED" : "MANDATORY RELAXATION TIME-LOCK ACTIVE"}
              </div>
              <div style={{ fontSize: "0.85rem", color: "var(--text-main)", marginTop: "2px" }}>
                {spreadingGateFeedback.message}
              </div>
            </div>
          </div>
          <button
            onClick={() => setSpreadingGateFeedback(null)}
            style={{ background: "transparent", border: "none", color: "var(--text-muted)", cursor: "pointer" }}
          >
            <X size={18} />
          </button>
        </div>
      )}

      {/* Navigation Tabs */}
      <div style={{ display: "flex", gap: "8px", borderBottom: "1px solid var(--border-color)", paddingBottom: "12px", flexWrap: "wrap" }}>
        {[
          { id: "orders", label: "Lay Planning & Orders (CUT-03)", icon: Layers },
          { id: "relaxation", label: "Relaxation Time-Lock Gate (CUT-02)", icon: Clock },
          { id: "markers", label: "CAD Marker Benchmarks (CUT-01)", icon: Sliders },
          { id: "bundles", label: "Component Bundles & ZPL (CUT-04)", icon: Ticket },
          { id: "fusing", label: "Fusing Machine Parameters (CUT-05)", icon: Cpu },
          { id: "subcontract", label: "Subcontract Reconciliation (CUT-06)", icon: Truck }
        ].map(tab => {
          const Icon = tab.icon;
          const isActive = activeTab === tab.id;
          return (
            <button
              key={tab.id}
              onClick={() => setActiveTab(tab.id as any)}
              style={{
                display: "flex",
                alignItems: "center",
                gap: "8px",
                padding: "8px 16px",
                borderRadius: "8px",
                fontSize: "0.85rem",
                fontWeight: 600,
                cursor: "pointer",
                background: isActive ? "linear-gradient(135deg, rgba(6,182,212,0.15), rgba(14,165,233,0.08))" : "transparent",
                color: isActive ? "#06b6d4" : "var(--text-muted)",
                border: isActive ? "1px solid rgba(6,182,212,0.3)" : "1px solid transparent",
                transition: "all 0.2s"
              }}
            >
              <Icon size={16} />
              {tab.label}
            </button>
          );
        })}
      </div>

      {/* TAB 1: LAY PLANNING & CUTTING ORDERS (CUT-03) */}
      {activeTab === "orders" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "16px" }}>
          <div className="card" style={{ padding: "0", overflow: "hidden" }}>
            <div style={{ padding: "16px 20px", borderBottom: "1px solid var(--border-color)", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <div>
                <h3 style={{ fontSize: "1rem", fontWeight: 600 }}>Cutting Lay Orders & Spreading Progress</h3>
                <p style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginTop: "2px" }}>
                  Shade-segregated lay planning with planned vs. actual fabric consumption & end-loss accounting.
                </p>
              </div>
              <input
                type="text"
                placeholder="Search style or cut #..."
                value={search}
                onChange={e => setSearch(e.target.value)}
                style={{
                  padding: "6px 12px",
                  borderRadius: "6px",
                  border: "1px solid var(--border-color)",
                  background: "var(--bg-input)",
                  color: "var(--text-main)",
                  fontSize: "0.85rem"
                }}
              />
            </div>

            <div style={{ overflowX: "auto" }}>
              <table style={{ width: "100%", borderCollapse: "collapse", textAlign: "left", fontSize: "0.85rem" }}>
                <thead>
                  <tr style={{ background: "rgba(255,255,255,0.02)", borderBottom: "1px solid var(--border-color)" }}>
                    <th style={{ padding: "12px 16px" }}>Cut Order #</th>
                    <th style={{ padding: "12px 16px" }}>Style & Shade</th>
                    <th style={{ padding: "12px 16px" }}>Table</th>
                    <th style={{ padding: "12px 16px" }}>Plies & Garments</th>
                    <th style={{ padding: "12px 16px" }}>CAD Marker Length</th>
                    <th style={{ padding: "12px 16px" }}>Actual Used / End-Loss</th>
                    <th style={{ padding: "12px 16px" }}>Wastage %</th>
                    <th style={{ padding: "12px 16px" }}>Status</th>
                    <th style={{ padding: "12px 16px", textAlign: "right" }}>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {orders
                    .filter(o => o.cuttingOrderNumber.toLowerCase().includes(search.toLowerCase()) || o.styleNumber.toLowerCase().includes(search.toLowerCase()))
                    .map(order => {
                      const statusBadge = STATUS_LABELS[order.status] || STATUS_LABELS[0];
                      const isReadyOrCompleted = order.status === 3 || order.status === 4 || order.status === 6;

                      return (
                        <tr key={order.id} style={{ borderBottom: "1px solid var(--border-color)" }}>
                          <td style={{ padding: "14px 16px", fontWeight: 600, color: "#38bdf8" }}>
                            {order.cuttingOrderNumber}
                          </td>
                          <td style={{ padding: "14px 16px" }}>
                            <div style={{ fontWeight: 600 }}>{order.styleNumber}</div>
                            <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>
                              {order.shadeGroup || "Shade A"} • {order.fabricType}
                            </div>
                          </td>
                          <td style={{ padding: "14px 16px" }}>
                            <span style={{
                              padding: "2px 8px",
                              borderRadius: "4px",
                              background: "rgba(255,255,255,0.06)",
                              fontSize: "0.75rem",
                              fontWeight: 600
                            }}>
                              {order.tableNumber}
                            </span>
                          </td>
                          <td style={{ padding: "14px 16px" }}>
                            <div style={{ fontWeight: 600 }}>{order.totalPlies} Plies</div>
                            <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>
                              {order.totalGarmentsPlanned || order.totalRequiredQty} Pcs Planned
                            </div>
                          </td>
                          <td style={{ padding: "14px 16px" }}>
                            <div>{order.markerLength} Yds</div>
                            <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>
                              {order.efficiencyPercentage}% Nesting
                            </div>
                          </td>
                          <td style={{ padding: "14px 16px" }}>
                            <div style={{ fontWeight: 600 }}>{order.actualFabricConsumedYds ? `${order.actualFabricConsumedYds} Yds` : "—"}</div>
                            <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>
                              End-loss: {order.endLossRemnantYds ? `${order.endLossRemnantYds} Yds` : "0.0 Yds"}
                            </div>
                          </td>
                          <td style={{ padding: "14px 16px" }}>
                            <span style={{
                              fontWeight: 600,
                              color: (order.cuttingWastageVariancePct || 0) <= 2.0 ? "#10b981" : "#ef4444"
                            }}>
                              {order.cuttingWastageVariancePct ? `+${order.cuttingWastageVariancePct}%` : "0.0%"}
                            </span>
                          </td>
                          <td style={{ padding: "14px 16px" }}>
                            <span style={{
                              padding: "4px 10px",
                              borderRadius: "6px",
                              fontSize: "0.75rem",
                              fontWeight: 600,
                              color: statusBadge.color,
                              background: statusBadge.bg
                            }}>
                              {statusBadge.label}
                            </span>
                          </td>
                          <td style={{ padding: "14px 16px", textAlign: "right" }}>
                            <div style={{ display: "flex", justifyContent: "flex-end", gap: "8px" }}>
                              {order.status !== 3 && order.status !== 4 && (
                                <button
                                  onClick={() => handleAuthorizeSpreading(order.id, order.cuttingOrderNumber)}
                                  disabled={authorizingOrderId === order.id}
                                  className="btn btn-secondary"
                                  style={{ fontSize: "0.75rem", padding: "4px 8px" }}
                                >
                                  {authorizingOrderId === order.id ? "Checking..." : "Authorize Spreading"}
                                </button>
                              )}
                              <a
                                href={`http://localhost:5224/api/reports/cutting-lay-sheet/${order.id}/pdf`}
                                target="_blank"
                                rel="noreferrer"
                                className="btn btn-secondary"
                                style={{ fontSize: "0.75rem", padding: "4px 8px", textDecoration: "none" }}
                              >
                                Lay Sheet
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
        </div>
      )}

      {/* TAB 2: RELAXATION TIME-LOCK GATE (CUT-02) */}
      {activeTab === "relaxation" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "20px" }}>
          {/* Ambient Conditions Header Banner */}
          <div style={{
            display: "grid",
            gridTemplateColumns: "repeat(auto-fit, minmax(280px, 1fr))",
            gap: "16px"
          }}>
            <div className="card" style={{ padding: "20px", display: "flex", alignItems: "center", gap: "16px" }}>
              <div style={{
                width: "48px",
                height: "48px",
                borderRadius: "12px",
                background: "rgba(245,158,11,0.15)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: "#f59e0b"
              }}>
                <Thermometer size={24} />
              </div>
              <div>
                <div style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Relaxation Room Ambient Temp</div>
                <div style={{ fontSize: "1.5rem", fontWeight: 700, color: "#f59e0b" }}>24.5°C</div>
                <div style={{ fontSize: "0.75rem", color: "#10b981" }}>Compliant (Optimal 22°C - 26°C)</div>
              </div>
            </div>

            <div className="card" style={{ padding: "20px", display: "flex", alignItems: "center", gap: "16px" }}>
              <div style={{
                width: "48px",
                height: "48px",
                borderRadius: "12px",
                background: "rgba(56,189,248,0.15)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: "#38bdf8"
              }}>
                <Droplets size={24} />
              </div>
              <div>
                <div style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Relative Humidity (RH)</div>
                <div style={{ fontSize: "1.5rem", fontWeight: 700, color: "#38bdf8" }}>65.0%</div>
                <div style={{ fontSize: "0.75rem", color: "#10b981" }}>Compliant (Optimal 60% - 70%)</div>
              </div>
            </div>

            <div className="card" style={{ padding: "20px", display: "flex", alignItems: "center", gap: "16px" }}>
              <div style={{
                width: "48px",
                height: "48px",
                borderRadius: "12px",
                background: "rgba(16,185,129,0.15)",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                color: "#10b981"
              }}>
                <ShieldCheck size={24} />
              </div>
              <div>
                <div style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>Mandatory Countdown Protocol</div>
                <div style={{ fontSize: "1.5rem", fontWeight: 700, color: "#10b981" }}>24 Hours Minimum</div>
                <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>Automated Spreading Block Active</div>
              </div>
            </div>
          </div>

          {/* Active Relaxation Rolls Grid */}
          <div className="card" style={{ padding: "20px" }}>
            <h3 style={{ fontSize: "1.1rem", fontWeight: 600, marginBottom: "16px" }}>
              Unrolled Fabric Roll Relaxation Status
            </h3>

            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(300px, 1fr))", gap: "16px" }}>
              {relaxationSessions.map(session => {
                return (
                  <div
                    key={session.id}
                    style={{
                      padding: "18px",
                      borderRadius: "12px",
                      background: session.isUnlocked ? "rgba(16,185,129,0.05)" : "rgba(239,68,68,0.05)",
                      border: `1px solid ${session.isUnlocked ? "rgba(16,185,129,0.25)" : "rgba(239,68,68,0.25)"}`,
                      display: "flex",
                      flexDirection: "column",
                      justifyContent: "space-between",
                      gap: "12px"
                    }}
                  >
                    <div>
                      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center" }}>
                        <div style={{ fontWeight: 700, fontFamily: "monospace", fontSize: "1.05rem" }}>
                          {session.rollBarcode}
                        </div>
                        <span style={{
                          padding: "3px 8px",
                          borderRadius: "4px",
                          fontSize: "0.7rem",
                          fontWeight: 700,
                          background: session.isUnlocked ? "rgba(16,185,129,0.2)" : "rgba(239,68,68,0.2)",
                          color: session.isUnlocked ? "#10b981" : "#ef4444"
                        }}>
                          {session.isUnlocked ? "UNLOCKED (SPREADING ALLOWED)" : "RELAXING (LOCKED)"}
                        </span>
                      </div>
                      <div style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginTop: "4px" }}>
                        Lot: {session.dyeLotNumber} • {session.fabricType}
                      </div>
                    </div>

                    <div style={{
                      padding: "10px",
                      borderRadius: "8px",
                      background: "rgba(0,0,0,0.2)",
                      display: "flex",
                      justifyContent: "space-between",
                      alignItems: "center"
                    }}>
                      <div>
                        <div style={{ fontSize: "0.7rem", color: "var(--text-muted)" }}>Countdown Timer</div>
                        <div style={{ fontSize: "1.1rem", fontWeight: 700, color: session.isUnlocked ? "#10b981" : "#f59e0b" }}>
                          {session.isUnlocked ? "00h 00m Remaining" : `${session.remainingHours}h ${session.remainingMinutes}m Remaining`}
                        </div>
                      </div>
                      <div style={{ textAlign: "right" }}>
                        <div style={{ fontSize: "0.7rem", color: "var(--text-muted)" }}>Sensor Reading</div>
                        <div style={{ fontSize: "0.85rem", fontWeight: 600 }}>
                          {session.ambientTemperatureC}°C • {session.relativeHumidityPct}% RH
                        </div>
                      </div>
                    </div>

                    <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>
                      Unrolled: {new Date(session.unrolledAtUtc).toLocaleTimeString()} • Operator: {session.operatorId}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: CAD MARKER BENCHMARKS (CUT-01) */}
      {activeTab === "markers" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "20px" }}>
          <div className="card" style={{ padding: "20px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
              <div>
                <h3 style={{ fontSize: "1.1rem", fontWeight: 600 }}>CAD Nesting Markers & Benchmark Consumption</h3>
                <p style={{ fontSize: "0.85rem", color: "var(--text-muted)", marginTop: "2px" }}>
                  Gerber AccuMark & Lectra Diamino marker files establishing standard consumption benchmarks.
                </p>
              </div>
            </div>

            <div style={{ display: "grid", gridTemplateColumns: "repeat(auto-fit, minmax(320px, 1fr))", gap: "16px" }}>
              {markers.map(mkr => {
                return (
                  <div
                    key={mkr.id}
                    style={{
                      padding: "18px",
                      borderRadius: "12px",
                      background: "rgba(255,255,255,0.02)",
                      border: "1px solid var(--border-color)",
                      display: "flex",
                      flexDirection: "column",
                      gap: "12px"
                    }}
                  >
                    <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start" }}>
                      <div>
                        <div style={{ fontWeight: 700, fontSize: "1rem", color: "#38bdf8" }}>{mkr.markerName}</div>
                        <div style={{ fontSize: "0.8rem", color: "var(--text-muted)" }}>
                          Style: {mkr.styleNumber} • {mkr.cadSoftware}
                        </div>
                      </div>
                      <span style={{
                        padding: "3px 8px",
                        borderRadius: "4px",
                        fontSize: "0.75rem",
                        fontWeight: 700,
                        background: mkr.markerEfficiencyPct >= 85.0 ? "rgba(16,185,129,0.15)" : "rgba(239,68,68,0.15)",
                        color: mkr.markerEfficiencyPct >= 85.0 ? "#10b981" : "#ef4444"
                      }}>
                        {mkr.markerEfficiencyPct}% Efficiency
                      </span>
                    </div>

                    <div style={{
                      display: "grid",
                      gridTemplateColumns: "1fr 1fr",
                      gap: "10px",
                      background: "rgba(0,0,0,0.15)",
                      padding: "10px",
                      borderRadius: "8px",
                      fontSize: "0.8rem"
                    }}>
                      <div>
                        <span style={{ color: "var(--text-muted)" }}>Cuttable Width:</span>{" "}
                        <strong>{mkr.cuttableWidthInches}&quot;</strong>
                      </div>
                      <div>
                        <span style={{ color: "var(--text-muted)" }}>Marker Length:</span>{" "}
                        <strong>{mkr.markerLengthYards} Yds</strong>
                      </div>
                      <div>
                        <span style={{ color: "var(--text-muted)" }}>Garments / Marker:</span>{" "}
                        <strong>{mkr.garmentsPerMarker} Pcs</strong>
                      </div>
                      <div>
                        <span style={{ color: "var(--text-muted)" }}>Consumption:</span>{" "}
                        <strong style={{ color: "#10b981" }}>{mkr.plannedConsumptionPerPcYds} Yds/Pc</strong>
                      </div>
                    </div>

                    <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>
                      Ratio: {mkr.sizeDistributionRatio} • File: {mkr.fileFormat}
                    </div>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      )}

      {/* TAB 4: COMPONENT BUNDLES & THERMAL BARCODES (CUT-04) */}
      {activeTab === "bundles" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "20px" }}>
          <div className="card" style={{ padding: "0", overflow: "hidden" }}>
            <div style={{ padding: "16px 20px", borderBottom: "1px solid var(--border-color)", display: "flex", justifyContent: "space-between", alignItems: "center" }}>
              <div>
                <h3 style={{ fontSize: "1rem", fontWeight: 600 }}>Cut Component Bundles & Station Barcoding</h3>
                <p style={{ fontSize: "0.8rem", color: "var(--text-muted)", marginTop: "2px" }}>
                  Serialized bundle tracking for Front Panels, Back Panels, Waistbands, and Flys.
                </p>
              </div>
              <a
                href="http://localhost:5224/api/reports/bundle-tickets/pdf"
                target="_blank"
                rel="noreferrer"
                className="btn btn-primary"
                style={{ display: "flex", alignItems: "center", gap: "8px", textDecoration: "none", fontSize: "0.85rem" }}
              >
                <Printer size={16} />
                Print All Thermal Tickets
              </a>
            </div>

            <div style={{ overflowX: "auto" }}>
              <table style={{ width: "100%", borderCollapse: "collapse", textAlign: "left", fontSize: "0.85rem" }}>
                <thead>
                  <tr style={{ background: "rgba(255,255,255,0.02)", borderBottom: "1px solid var(--border-color)" }}>
                    <th style={{ padding: "12px 16px" }}>Bundle Barcode</th>
                    <th style={{ padding: "12px 16px" }}>Component</th>
                    <th style={{ padding: "12px 16px" }}>Size</th>
                    <th style={{ padding: "12px 16px" }}>Shade</th>
                    <th style={{ padding: "12px 16px" }}>Qty & Serials</th>
                    <th style={{ padding: "12px 16px" }}>Current Station</th>
                    <th style={{ padding: "12px 16px", textAlign: "right" }}>Thermal Ticket</th>
                  </tr>
                </thead>
                <tbody>
                  {bundles.map(b => (
                    <tr key={b.id} style={{ borderBottom: "1px solid var(--border-color)" }}>
                      <td style={{ padding: "14px 16px", fontWeight: 600, fontFamily: "monospace", color: "#38bdf8" }}>
                        {b.barcode || b.bundleNumber}
                      </td>
                      <td style={{ padding: "14px 16px", fontWeight: 600 }}>
                        {b.componentName || "FRONT PANEL"}
                      </td>
                      <td style={{ padding: "14px 16px" }}>
                        <span style={{ padding: "2px 8px", borderRadius: "4px", background: "rgba(255,255,255,0.06)", fontWeight: 600 }}>
                          {b.size}
                        </span>
                      </td>
                      <td style={{ padding: "14px 16px" }}>
                        <span style={{ fontWeight: 600, color: "#10b981" }}>{b.shadeGroup || "A"}</span>
                      </td>
                      <td style={{ padding: "14px 16px" }}>
                        <div style={{ fontWeight: 600 }}>{b.quantity} Pcs</div>
                        <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>
                          Serials: {b.serialStart.toString().padStart(3, "0")} - {b.serialEnd.toString().padStart(3, "0")}
                        </div>
                      </td>
                      <td style={{ padding: "14px 16px" }}>
                        <span style={{
                          padding: "3px 8px",
                          borderRadius: "4px",
                          fontSize: "0.75rem",
                          fontWeight: 600,
                          background: b.currentFloorStation === "FUSING_MACHINE" ? "rgba(245,158,11,0.15)" : "rgba(6,182,212,0.15)",
                          color: b.currentFloorStation === "FUSING_MACHINE" ? "#f59e0b" : "#06b6d4"
                        }}>
                          {b.currentFloorStation || "CUTTING_OUTPUT"}
                        </span>
                      </td>
                      <td style={{ padding: "14px 16px", textAlign: "right" }}>
                        <button
                          onClick={() => {
                            setSelectedBundleForZpl(b);
                            setShowZplModal(true);
                          }}
                          className="btn btn-secondary"
                          style={{ fontSize: "0.75rem", padding: "4px 8px", display: "inline-flex", alignItems: "center", gap: "4px" }}
                        >
                          <Barcode size={14} />
                          ZPL Code
                        </button>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 5: FUSING MACHINE PARAMETERS (CUT-05) */}
      {activeTab === "fusing" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "20px" }}>
          <div className="card" style={{ padding: "20px" }}>
            <h3 style={{ fontSize: "1.1rem", fontWeight: 600, marginBottom: "4px" }}>
              Interlining Fusing Machine Parameters Log
            </h3>
            <p style={{ fontSize: "0.85rem", color: "var(--text-muted)", marginBottom: "16px" }}>
              Monitors temperature, pneumatic pressure, and roller speed to guarantee peel bond strength (&gt;= 1.2 kg/inch).
            </p>

            <div style={{ overflowX: "auto" }}>
              <table style={{ width: "100%", borderCollapse: "collapse", textAlign: "left", fontSize: "0.85rem" }}>
                <thead>
                  <tr style={{ background: "rgba(255,255,255,0.02)", borderBottom: "1px solid var(--border-color)" }}>
                    <th style={{ padding: "12px 16px" }}>Cut Order</th>
                    <th style={{ padding: "12px 16px" }}>Machine</th>
                    <th style={{ padding: "12px 16px" }}>Component</th>
                    <th style={{ padding: "12px 16px" }}>Interlining Type</th>
                    <th style={{ padding: "12px 16px" }}>Temperature</th>
                    <th style={{ padding: "12px 16px" }}>Pressure</th>
                    <th style={{ padding: "12px 16px" }}>Dwell Time</th>
                    <th style={{ padding: "12px 16px" }}>Status</th>
                    <th style={{ padding: "12px 16px" }}>Operator</th>
                  </tr>
                </thead>
                <tbody>
                  {fusingLogs.map(log => (
                    <tr key={log.id} style={{ borderBottom: "1px solid var(--border-color)" }}>
                      <td style={{ padding: "14px 16px", fontWeight: 600, color: "#38bdf8" }}>{log.cutOrderNumber}</td>
                      <td style={{ padding: "14px 16px", fontWeight: 600 }}>{log.machineId}</td>
                      <td style={{ padding: "14px 16px" }}>{log.componentName}</td>
                      <td style={{ padding: "14px 16px", fontSize: "0.8rem", color: "var(--text-muted)" }}>{log.interliningType}</td>
                      <td style={{ padding: "14px 16px", fontWeight: 600, color: "#f59e0b" }}>{log.temperatureC}°C</td>
                      <td style={{ padding: "14px 16px", fontWeight: 600 }}>{log.pressureBar} bar</td>
                      <td style={{ padding: "14px 16px" }}>{log.speedSeconds}s</td>
                      <td style={{ padding: "14px 16px" }}>
                        <span style={{
                          padding: "3px 8px",
                          borderRadius: "4px",
                          fontSize: "0.75rem",
                          fontWeight: 700,
                          background: log.isApproved ? "rgba(16,185,129,0.15)" : "rgba(239,68,68,0.15)",
                          color: log.isApproved ? "#10b981" : "#ef4444"
                        }}>
                          {log.isApproved ? "PASSED (NO BUBBLING)" : "OUT OF SPEC"}
                        </span>
                      </td>
                      <td style={{ padding: "14px 16px" }}>{log.operatorName}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 6: SUBCONTRACT CUTTING RECONCILIATION (CUT-06) */}
      {activeTab === "subcontract" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "20px" }}>
          <div className="card" style={{ padding: "20px" }}>
            <h3 style={{ fontSize: "1.1rem", fontWeight: 600, marginBottom: "4px" }}>
              Subcontract Cutting Delivery & Return Shortage Reconciliation
            </h3>
            <p style={{ fontSize: "0.85rem", color: "var(--text-muted)", marginBottom: "16px" }}>
              Reconciles fabric yards sent to external cutting mills against cut component panels returned.
            </p>

            <div style={{ overflowX: "auto" }}>
              <table style={{ width: "100%", borderCollapse: "collapse", textAlign: "left", fontSize: "0.85rem" }}>
                <thead>
                  <tr style={{ background: "rgba(255,255,255,0.02)", borderBottom: "1px solid var(--border-color)" }}>
                    <th style={{ padding: "12px 16px" }}>Challan #</th>
                    <th style={{ padding: "12px 16px" }}>Subcontractor</th>
                    <th style={{ padding: "12px 16px" }}>Fabric Sent</th>
                    <th style={{ padding: "12px 16px" }}>Expected Panels</th>
                    <th style={{ padding: "12px 16px" }}>Received / Defect</th>
                    <th style={{ padding: "12px 16px" }}>Shortage %</th>
                    <th style={{ padding: "12px 16px" }}>Status</th>
                    <th style={{ padding: "12px 16px" }}>Auditor</th>
                  </tr>
                </thead>
                <tbody>
                  {subcontractChallans.map(sub => (
                    <tr key={sub.id} style={{ borderBottom: "1px solid var(--border-color)" }}>
                      <td style={{ padding: "14px 16px", fontWeight: 600, color: "#38bdf8" }}>{sub.challanNumber}</td>
                      <td style={{ padding: "14px 16px" }}>
                        <div style={{ fontWeight: 600 }}>{sub.subcontractorName}</div>
                        <div style={{ fontSize: "0.75rem", color: "var(--text-muted)" }}>{sub.styleNumber}</div>
                      </td>
                      <td style={{ padding: "14px 16px", fontWeight: 600 }}>{sub.fabricSentYds} Yds</td>
                      <td style={{ padding: "14px 16px" }}>{sub.expectedCutPanels} Pcs</td>
                      <td style={{ padding: "14px 16px" }}>
                        <div style={{ fontWeight: 600 }}>{sub.receivedCutPanels} Pcs Good</div>
                        <div style={{ fontSize: "0.75rem", color: "#ef4444" }}>{sub.defectivePanels} Pcs Defect</div>
                      </td>
                      <td style={{ padding: "14px 16px" }}>
                        <span style={{
                          fontWeight: 700,
                          color: sub.shortagePercentage <= 1.0 ? "#10b981" : "#ef4444"
                        }}>
                          {sub.shortageCount} Pcs ({sub.shortagePercentage}%)
                        </span>
                      </td>
                      <td style={{ padding: "14px 16px" }}>
                        <span style={{
                          padding: "3px 8px",
                          borderRadius: "4px",
                          fontSize: "0.75rem",
                          fontWeight: 700,
                          background: sub.statusName === "Reconciled" ? "rgba(16,185,129,0.15)" : "rgba(239,68,68,0.15)",
                          color: sub.statusName === "Reconciled" ? "#10b981" : "#ef4444"
                        }}>
                          {sub.statusName}
                        </span>
                      </td>
                      <td style={{ padding: "14px 16px", color: "var(--text-muted)", fontSize: "0.8rem" }}>
                        {sub.reconciledBy || "Pending Review"}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* MODAL: REGISTER FABRIC UNROLLING */}
      {showUnrollModal && (
        <div style={{
          position: "fixed",
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: "rgba(0,0,0,0.7)",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          zIndex: 1000,
          backdropFilter: "blur(4px)"
        }}>
          <div className="card" style={{ width: "480px", maxWidth: "90vw", padding: "24px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
              <h3 style={{ fontSize: "1.1rem", fontWeight: 700 }}>Register Unrolling for 24h Relaxation</h3>
              <button
                onClick={() => setShowUnrollModal(false)}
                style={{ background: "transparent", border: "none", color: "var(--text-muted)", cursor: "pointer" }}
              >
                <X size={20} />
              </button>
            </div>

            <form onSubmit={handleRegisterUnrolling} style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)", display: "block", marginBottom: "4px" }}>
                  Roll Barcode (e.g. ROL-2026-00895)
                </label>
                <input
                  type="text"
                  value={unrollForm.rollBarcode}
                  onChange={e => setUnrollForm({ ...unrollForm, rollBarcode: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", borderRadius: "6px", border: "1px solid var(--border-color)", background: "var(--bg-input)", color: "var(--text-main)" }}
                  required
                />
              </div>

              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)", display: "block", marginBottom: "4px" }}>
                  Dye Lot Number
                </label>
                <input
                  type="text"
                  value={unrollForm.dyeLotNumber}
                  onChange={e => setUnrollForm({ ...unrollForm, dyeLotNumber: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", borderRadius: "6px", border: "1px solid var(--border-color)", background: "var(--bg-input)", color: "var(--text-main)" }}
                  required
                />
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "12px" }}>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)", display: "block", marginBottom: "4px" }}>
                    Room Temp (°C)
                  </label>
                  <input
                    type="number"
                    step="0.1"
                    value={unrollForm.ambientTemp}
                    onChange={e => setUnrollForm({ ...unrollForm, ambientTemp: Number(e.target.value) })}
                    style={{ width: "100%", padding: "8px 12px", borderRadius: "6px", border: "1px solid var(--border-color)", background: "var(--bg-input)", color: "var(--text-main)" }}
                  />
                </div>
                <div>
                  <label style={{ fontSize: "0.8rem", color: "var(--text-muted)", display: "block", marginBottom: "4px" }}>
                    Humidity (% RH)
                  </label>
                  <input
                    type="number"
                    step="0.1"
                    value={unrollForm.humidity}
                    onChange={e => setUnrollForm({ ...unrollForm, humidity: Number(e.target.value) })}
                    style={{ width: "100%", padding: "8px 12px", borderRadius: "6px", border: "1px solid var(--border-color)", background: "var(--bg-input)", color: "var(--text-main)" }}
                  />
                </div>
              </div>

              <div>
                <label style={{ fontSize: "0.8rem", color: "var(--text-muted)", display: "block", marginBottom: "4px" }}>
                  Operator / Cutting Master
                </label>
                <input
                  type="text"
                  value={unrollForm.operatorId}
                  onChange={e => setUnrollForm({ ...unrollForm, operatorId: e.target.value })}
                  style={{ width: "100%", padding: "8px 12px", borderRadius: "6px", border: "1px solid var(--border-color)", background: "var(--bg-input)", color: "var(--text-main)" }}
                />
              </div>

              <div style={{ display: "flex", justifyContent: "flex-end", gap: "10px", marginTop: "12px" }}>
                <button
                  type="button"
                  onClick={() => setShowUnrollModal(false)}
                  className="btn btn-secondary"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="btn btn-primary"
                >
                  Start 24h Countdown
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: INDUSTRIAL ZEBRA ZPL CODE PREVIEW */}
      {showZplModal && selectedBundleForZpl && (
        <div style={{
          position: "fixed",
          top: 0,
          left: 0,
          right: 0,
          bottom: 0,
          background: "rgba(0,0,0,0.75)",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          zIndex: 1000,
          backdropFilter: "blur(4px)"
        }}>
          <div className="card" style={{ width: "520px", maxWidth: "90vw", padding: "24px" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", marginBottom: "16px" }}>
              <div style={{ display: "flex", alignItems: "center", gap: "8px" }}>
                <Barcode size={20} color="#06b6d4" />
                <h3 style={{ fontSize: "1.1rem", fontWeight: 700 }}>Thermal ZPL II Ticket Preview</h3>
              </div>
              <button
                onClick={() => setShowZplModal(false)}
                style={{ background: "transparent", border: "none", color: "var(--text-muted)", cursor: "pointer" }}
              >
                <X size={20} />
              </button>
            </div>

            {/* Visual Ticket Rendering */}
            <div style={{
              background: "#ffffff",
              color: "#000000",
              padding: "16px",
              borderRadius: "8px",
              border: "2px dashed #333333",
              fontFamily: "Arial, sans-serif",
              marginBottom: "16px"
            }}>
              <div style={{ display: "flex", justifyContent: "space-between", borderBottom: "1px solid #000", paddingBottom: "4px" }}>
                <strong style={{ fontSize: "0.85rem" }}>STITCHMIND FACTORY EXECUTION</strong>
                <span style={{ fontSize: "0.75rem", fontFamily: "monospace" }}>CUT: CUT-2026-0001</span>
              </div>
              <div style={{ margin: "8px 0", fontSize: "0.8rem" }}>
                <div>Style: <strong>{selectedBundleForZpl.styleNumber || "ST-2026-CHINO-01"}</strong> | Buyer: Nordic Fashion</div>
                <div>Color: {selectedBundleForZpl.color} | Shade: {selectedBundleForZpl.shadeGroup || "A"}</div>
              </div>
              <div style={{ background: "#eee", padding: "6px", display: "flex", justifyContent: "space-between", alignItems: "center", margin: "6px 0" }}>
                <strong style={{ fontSize: "0.95rem" }}>{selectedBundleForZpl.componentName || "FRONT PANEL"}</strong>
                <strong style={{ fontSize: "1rem" }}>SIZE: {selectedBundleForZpl.size}</strong>
              </div>
              <div style={{ fontSize: "0.8rem", margin: "6px 0", display: "flex", justifyContent: "space-between" }}>
                <span>Bundle: {selectedBundleForZpl.bundleSequence || 1} / {selectedBundleForZpl.totalBundlesInOrder || 10}</span>
                <strong>Qty: {selectedBundleForZpl.quantity} Pcs ({selectedBundleForZpl.serialStart.toString().padStart(3, "0")}-{selectedBundleForZpl.serialEnd.toString().padStart(3, "0")})</strong>
              </div>
              <div style={{ textAlign: "center", marginTop: "10px", padding: "8px", background: "#f8f8f8", border: "1px solid #ddd" }}>
                <div style={{ letterSpacing: "4px", fontWeight: "bold", fontFamily: "monospace", fontSize: "1rem" }}>
                  ||||| {selectedBundleForZpl.barcode} |||||
                </div>
                <div style={{ fontSize: "0.7rem", fontFamily: "monospace", marginTop: "2px" }}>
                  {selectedBundleForZpl.barcode}
                </div>
              </div>
            </div>

            {/* Raw ZPL Script */}
            <div>
              <div style={{ fontSize: "0.75rem", color: "var(--text-muted)", marginBottom: "4px" }}>
                Zebra ZPL II Raw Stream (Thermal Port 9100 / USB)
              </div>
              <pre style={{
                background: "rgba(0,0,0,0.5)",
                padding: "10px",
                borderRadius: "6px",
                fontSize: "0.75rem",
                fontFamily: "monospace",
                color: "#10b981",
                maxHeight: "100px",
                overflowY: "auto"
              }}>
{`^XA
^PW600
^LL400
^FO50,30^A0N,28,28^FDSTITCHMIND FACTORY EXECUTION^FS
^FO50,65^A0N,22,22^FDSTYLE: ${selectedBundleForZpl.styleNumber || "ST-2026-CHINO-01"} | BUYER: Nordic Fashion^FS
^FO50,95^A0N,22,22^FDCOLOR: ${selectedBundleForZpl.color} | SHADE: ${selectedBundleForZpl.shadeGroup || "A"}^FS
^FO50,125^A0N,22,22^FDSIZE: ${selectedBundleForZpl.size} | CUT: CUT-2026-0001^FS
^FO50,155^A0N,24,24^FDCOMPONENT: ${selectedBundleForZpl.componentName || "FRONT PANEL"}^FS
^FO50,185^A0N,22,22^FDBUNDLE: 01/10 | QTY: ${selectedBundleForZpl.quantity} PCS^FS
^FO50,225^BY2,3,70^BCN,70,Y,N,N^FD${selectedBundleForZpl.barcode}^FS
^XZ`}
              </pre>
            </div>

            <div style={{ display: "flex", justifyContent: "flex-end", marginTop: "16px" }}>
              <button
                onClick={() => setShowZplModal(false)}
                className="btn btn-secondary"
              >
                Close Preview
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
