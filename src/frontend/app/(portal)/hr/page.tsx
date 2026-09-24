"use client";

import React, { useState, useEffect } from "react";
import { 
  Users, Fingerprint, CreditCard, Plus, Search, Filter, CheckCircle2, 
  XCircle, Clock, Calendar, Download, Printer, Truck, Package, ShieldCheck, 
  FileText, AlertCircle, RefreshCw, Smartphone, Building, DollarSign,
  ChevronRight, ArrowUpRight, Award, UserCheck, Check, Send
} from "lucide-react";
import { apiRequest } from "@/lib/api";

interface EmployeeDto {
  id: string;
  employeeCode: string;
  fullName: string;
  email: string;
  phoneNumber: string;
  department: string;
  designation: string;
  assignedLine?: string;
  basicSalary: number;
  grossSalary: number;
  joiningDate: string;
  statusName: string;
  nationalId?: string;
  mobileBankingNumber?: string;
  emergencyContact?: string;
}

interface AttendanceDto {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  department: string;
  assignedLine?: string;
  date: string;
  checkInTime?: string;
  checkOutTime?: string;
  overtimeHours: number;
  statusName: string;
  shiftName?: string;
  remarks?: string;
}

interface LeaveDto {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  department: string;
  leaveTypeName: string;
  startDate: string;
  endDate: string;
  totalDays: number;
  reason: string;
  statusName: string;
  approvedBy?: string;
  approvedAt?: string;
}

interface PayrollDto {
  id: string;
  employeeId: string;
  employeeCode: string;
  employeeName: string;
  department: string;
  designation: string;
  payMonth: number;
  payYear: number;
  basicSalary: number;
  allowances: number;
  overtimeHours: number;
  overtimeAmount: number;
  absentDeduction: number;
  taxDeduction: number;
  advanceDeduction: number;
  totalDeductions: number;
  netPayable: number;
  statusName: string;
  disbursedDate?: string;
  paymentReference?: string;
}

interface CartonDto {
  id: string;
  cartonBarcode: string;
  buyerOrderId: string;
  orderNumber: string;
  buyerName: string;
  styleNumber: string;
  color: string;
  sizeBreakdownJson: string;
  totalQuantityPcs: number;
  grossWeightKg: number;
  netWeightKg: number;
  statusName: string;
  assignedPallet?: string;
}

interface ChallanDto {
  id: string;
  challanNumber: string;
  buyerId: string;
  buyerName: string;
  buyerCode: string;
  destinationAddress: string;
  vehicleNumber: string;
  driverName: string;
  driverPhone: string;
  gatePassNumber?: string;
  totalCartons: number;
  totalGarmentPcs: number;
  totalWeightKg: number;
  dispatchDate: string;
  statusName: string;
  remarks?: string;
}

export default function HRPage() {
  const [activeTab, setActiveTab] = useState<"employees" | "attendance" | "leaves" | "payroll" | "dispatch">("employees");
  const [loading, setLoading] = useState(false);
  const [feedbackMsg, setFeedbackMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // Data states
  const [employees, setEmployees] = useState<EmployeeDto[]>([]);
  const [attendance, setAttendance] = useState<AttendanceDto[]>([]);
  const [leaves, setLeaves] = useState<LeaveDto[]>([]);
  const [payrolls, setPayrolls] = useState<PayrollDto[]>([]);
  const [cartons, setCartons] = useState<CartonDto[]>([]);
  const [challans, setChallans] = useState<ChallanDto[]>([]);

  // Filter & Search
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedDept, setSelectedDept] = useState("ALL");

  // Modals
  const [showAddEmpModal, setShowAddEmpModal] = useState(false);
  const [showPunchModal, setShowPunchModal] = useState(false);
  const [showApplyLeaveModal, setShowApplyLeaveModal] = useState(false);
  const [showPayslipModal, setShowPayslipModal] = useState<PayrollDto | null>(null);
  const [showCreateChallanModal, setShowCreateChallanModal] = useState(false);

  // Form states
  const [newEmp, setNewEmp] = useState({
    employeeCode: "",
    fullName: "",
    email: "",
    phoneNumber: "",
    department: "Sewing Section",
    designation: "Machine Operator",
    assignedLine: "Line-01",
    basicSalary: 18000,
    houseRentAllowance: 7000,
    medicalAllowance: 2500,
    conveyanceAllowance: 1500,
    joiningDate: new Date().toISOString().split("T")[0],
    nationalId: "",
    mobileBankingNumber: "",
    emergencyContact: ""
  });

  const [punchForm, setPunchForm] = useState({
    employeeId: "",
    isCheckOut: false,
    shiftName: "Morning Shift (08:00 - 17:00)",
    deviceId: "BIO-GATE-01 (Main Gate)",
    remarks: "Live Fingerprint Verification"
  });

  const [leaveForm, setLeaveForm] = useState({
    employeeId: "",
    type: 1, // Casual
    startDate: new Date().toISOString().split("T")[0],
    endDate: new Date().toISOString().split("T")[0],
    totalDays: 1,
    reason: ""
  });

  const [challanForm, setChallanForm] = useState({
    buyerId: "",
    destinationAddress: "Chittagong Port Yard 4, Bangladesh",
    vehicleNumber: "DHAKA-METRO-TA-11-9044",
    driverName: "Md. Jashim Uddin",
    driverPhone: "+880 1712-889900",
    totalCartons: 120,
    totalGarmentPcs: 2880,
    totalWeightKg: 740.5,
    remarks: "Export packing compliant with Inditex QA standards"
  });

  // Load active tab data
  const loadData = async () => {
    setLoading(true);
    try {
      const [empRes, attRes, leaveRes, payRes, cartRes, chalRes] = await Promise.all([
        apiRequest<any>("/api/hr/employees"),
        apiRequest<any>("/api/hr/attendance"),
        apiRequest<any>("/api/hr/leaves"),
        apiRequest<any>("/api/hr/payroll"),
        apiRequest<any>("/api/dispatch/cartons"),
        apiRequest<any>("/api/dispatch/challans")
      ]);

      if (empRes.isSuccess && Array.isArray(empRes.value)) setEmployees(empRes.value);
      if (attRes.isSuccess && Array.isArray(attRes.value)) setAttendance(attRes.value);
      if (leaveRes.isSuccess && Array.isArray(leaveRes.value)) setLeaves(leaveRes.value);
      if (payRes.isSuccess && Array.isArray(payRes.value)) setPayrolls(payRes.value);
      if (cartRes.isSuccess && Array.isArray(cartRes.value)) setCartons(cartRes.value);
      if (chalRes.isSuccess && Array.isArray(chalRes.value)) setChallans(chalRes.value);

      if (empRes.isSuccess && Array.isArray(empRes.value) && empRes.value.length > 0) {
        if (!punchForm.employeeId) setPunchForm(prev => ({ ...prev, employeeId: empRes.value[0].id }));
        if (!leaveForm.employeeId) setLeaveForm(prev => ({ ...prev, employeeId: empRes.value[0].id }));
      }
    } catch (err: any) {
      console.error("Failed loading data:", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadData();
  }, []);

  const showToast = (text: string, type: "success" | "error" = "success") => {
    setFeedbackMsg({ type, text });
    setTimeout(() => setFeedbackMsg(null), 4000);
  };

  // Add Employee Handler
  const handleAddEmployee = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newEmp.fullName || !newEmp.employeeCode) {
      showToast("Please enter Employee Code and Full Name", "error");
      return;
    }
    try {
      const res = await apiRequest<any>("/api/hr/employees", {
        method: "POST",
        body: newEmp
      });
      if (res.isSuccess) {
        showToast("Employee enrolled successfully!");
        setShowAddEmpModal(false);
        loadData();
      } else {
        showToast(res.error || "Failed to create employee", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Punch Simulator Handler
  const handleSimulatePunch = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await apiRequest<any>("/api/hr/attendance/punch", {
        method: "POST",
        body: {
          employeeId: punchForm.employeeId,
          isCheckOut: punchForm.isCheckOut,
          shiftName: punchForm.shiftName,
          biometricDeviceId: punchForm.deviceId,
          remarks: punchForm.remarks
        }
      });
      if (res.isSuccess) {
        showToast(`Biometric punch ${punchForm.isCheckOut ? "OUT" : "IN"} verified successfully!`);
        setShowPunchModal(false);
        loadData();
      } else {
        showToast(res.error || "Punch simulation failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Leave Approval Handler
  const handleApproveLeave = async (id: string, approve: boolean) => {
    try {
      const res = await apiRequest<any>(`/api/hr/leaves/${id}/approve`, {
        method: "POST",
        body: {
          approve,
          approverName: "HR Manager",
          rejectionReason: approve ? null : "Operational staffing requirement"
        }
      });
      if (res.isSuccess) {
        showToast(`Leave request ${approve ? "Approved" : "Rejected"} successfully!`);
        loadData();
      } else {
        showToast(res.error || "Operation failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Apply Leave Handler
  const handleApplyLeave = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await apiRequest<any>("/api/hr/leaves", {
        method: "POST",
        body: {
          employeeId: leaveForm.employeeId,
          type: Number(leaveForm.type),
          startDate: leaveForm.startDate,
          endDate: leaveForm.endDate,
          totalDays: Number(leaveForm.totalDays),
          reason: leaveForm.reason
        }
      });
      if (res.isSuccess) {
        showToast("Leave application submitted!");
        setShowApplyLeaveModal(false);
        loadData();
      } else {
        showToast(res.error || "Leave submission failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Generate Monthly Payroll
  const handleGeneratePayroll = async () => {
    const month = new Date().getMonth() + 1;
    const year = new Date().getFullYear();
    try {
      setLoading(true);
      const res = await apiRequest<any>("/api/hr/payroll/generate", {
        method: "POST",
        body: { month, year }
      });
      if (res.isSuccess) {
        showToast(`Monthly payroll batch for ${month}/${year} generated successfully!`);
        loadData();
      } else {
        showToast(res.error || "Payroll batch generation failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    } finally {
      setLoading(false);
    }
  };

  // Dispatch Challan Handler
  const handleDispatchChallan = async (id: string) => {
    try {
      const res = await apiRequest<any>(`/api/dispatch/challans/${id}/dispatch`, {
        method: "POST",
        body: { gatePassNumber: `GP-${Date.now().toString().slice(-6)}` }
      });
      if (res.isSuccess) {
        showToast("Delivery Challan & Gate Pass dispatched successfully!");
        loadData();
      } else {
        showToast(res.error || "Dispatch failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Create Challan Handler
  const handleCreateChallan = async (e: React.FormEvent) => {
    e.preventDefault();
    if (cartons.length === 0) {
      showToast("No finished cartons available to dispatch", "error");
      return;
    }
    try {
      const targetBuyerId = cartons[0].buyerOrderId;
      const res = await apiRequest<any>("/api/dispatch/challans", {
        method: "POST",
        body: {
          buyerId: targetBuyerId,
          destinationAddress: challanForm.destinationAddress,
          vehicleNumber: challanForm.vehicleNumber,
          driverName: challanForm.driverName,
          driverPhone: challanForm.driverPhone,
          totalCartons: Number(challanForm.totalCartons),
          totalGarmentPcs: Number(challanForm.totalGarmentPcs),
          totalWeightKg: Number(challanForm.totalWeightKg),
          remarks: challanForm.remarks,
          cartonBarcodes: cartons.map(c => c.cartonBarcode)
        }
      });
      if (res.isSuccess) {
        showToast("Delivery Challan created successfully!");
        setShowCreateChallanModal(false);
        loadData();
      } else {
        showToast(res.error || "Challan creation failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Filtered employees
  const filteredEmployees = employees.filter(emp => {
    const matchesSearch = 
      emp.fullName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      emp.employeeCode.toLowerCase().includes(searchQuery.toLowerCase()) ||
      emp.designation.toLowerCase().includes(searchQuery.toLowerCase()) ||
      emp.phoneNumber.includes(searchQuery);
    const matchesDept = selectedDept === "ALL" || emp.department === selectedDept;
    return matchesSearch && matchesDept;
  });

  const totalPresentToday = attendance.filter(a => a.statusName === "Present").length;
  const totalLateToday = attendance.filter(a => a.statusName === "Late").length;
  const totalOTHours = attendance.reduce((acc, a) => acc + (a.overtimeHours || 0), 0);
  const totalPayrollGross = payrolls.reduce((acc, p) => acc + (p.netPayable || 0), 0);

  return (
    <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
      {/* Toast Notification */}
      {feedbackMsg && (
        <div style={{
          position: "fixed",
          bottom: 24,
          right: 24,
          zIndex: 100,
          background: feedbackMsg.type === "success" ? "#0f172a" : "#b91c1c",
          color: "#ffffff",
          padding: "12px 20px",
          borderRadius: 8,
          boxShadow: "0 10px 25px rgba(0,0,0,0.2)",
          display: "flex",
          alignItems: "center",
          gap: 10,
          fontSize: "0.875rem",
          fontWeight: 600
        }}>
          {feedbackMsg.type === "success" ? <CheckCircle2 size={18} color="#22c55e" /> : <AlertCircle size={18} />}
          <span>{feedbackMsg.text}</span>
        </div>
      )}

      {/* Header & Actions */}
      <div style={{ display: "flex", justifyContent: "space-between", alignItems: "flex-start", flexWrap: "wrap", gap: "1rem" }}>
        <div>
          <h2 style={{ fontSize: "1.5rem", fontWeight: 700, color: "#0f172a", margin: 0 }}>
            Phase 4: Human Resources & Dispatch
          </h2>
          <p style={{ color: "#64748b", fontSize: "0.875rem", marginTop: 4 }}>
            Biometric attendance logs, shift scheduling, automated payroll calculation (1.5x OT), and export delivery challans.
          </p>
        </div>

        <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap" }}>
          <button 
            onClick={loadData}
            className="btn btn-outline"
            style={{ display: "flex", alignItems: "center", gap: 6 }}
          >
            <RefreshCw size={15} className={loading ? "animate-spin" : ""} />
            <span>Sync Live Data</span>
          </button>

          {activeTab === "employees" && (
            <button 
              onClick={() => setShowAddEmpModal(true)}
              className="btn btn-primary"
              style={{ display: "flex", alignItems: "center", gap: 6 }}
            >
              <Plus size={16} />
              <span>Enroll Employee</span>
            </button>
          )}

          {activeTab === "attendance" && (
            <button 
              onClick={() => setShowPunchModal(true)}
              className="btn btn-primary"
              style={{ display: "flex", alignItems: "center", gap: 6 }}
            >
              <Fingerprint size={16} />
              <span>Biometric Punch Simulator</span>
            </button>
          )}

          {activeTab === "leaves" && (
            <button 
              onClick={() => setShowApplyLeaveModal(true)}
              className="btn btn-primary"
              style={{ display: "flex", alignItems: "center", gap: 6 }}
            >
              <Plus size={16} />
              <span>Apply Leave</span>
            </button>
          )}

          {activeTab === "payroll" && (
            <button 
              onClick={handleGeneratePayroll}
              className="btn btn-primary"
              style={{ display: "flex", alignItems: "center", gap: 6 }}
            >
              <CreditCard size={16} />
              <span>Generate Monthly Payroll Batch</span>
            </button>
          )}

          {activeTab === "dispatch" && (
            <button 
              onClick={() => setShowCreateChallanModal(true)}
              className="btn btn-primary"
              style={{ display: "flex", alignItems: "center", gap: 6 }}
            >
              <Truck size={16} />
              <span>Generate Delivery Challan</span>
            </button>
          )}
        </div>
      </div>

      {/* KPI Cards */}
      <div className="kpi-grid">
        <div className="kpi-card">
          <div className="kpi-icon-wrap" style={{ background: "#eff6ff", color: "#2563eb" }}>
            <Users size={22} />
          </div>
          <div className="kpi-details">
            <div className="kpi-label">Active Workforce</div>
            <div className="kpi-val">{employees.length} Staff</div>
            <div className="kpi-sub" style={{ color: "#16a34a" }}>100% compliant profiles</div>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrap" style={{ background: "#ecfdf5", color: "#10b981" }}>
            <Fingerprint size={22} />
          </div>
          <div className="kpi-details">
            <div className="kpi-label">Present Today</div>
            <div className="kpi-val">{totalPresentToday} / {employees.length}</div>
            <div className="kpi-sub" style={{ color: totalLateToday > 0 ? "#d97706" : "#16a34a" }}>
              {totalLateToday} late punch logs
            </div>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrap" style={{ background: "#fef3c7", color: "#d97706" }}>
            <Clock size={22} />
          </div>
          <div className="kpi-details">
            <div className="kpi-label">Overtime Hours</div>
            <div className="kpi-val">{totalOTHours.toFixed(1)} hrs</div>
            <div className="kpi-sub">Calculated @ 1.5x basic rate</div>
          </div>
        </div>

        <div className="kpi-card">
          <div className="kpi-icon-wrap" style={{ background: "#f5f3ff", color: "#7c3aed" }}>
            <Truck size={22} />
          </div>
          <div className="kpi-details">
            <div className="kpi-label">Cartons Packed</div>
            <div className="kpi-val">{cartons.length} Cartons</div>
            <div className="kpi-sub" style={{ color: "#2563eb" }}>{challans.length} challans processed</div>
          </div>
        </div>
      </div>

      {/* Navigation Tabs */}
      <div style={{
        display: "flex",
        gap: 6,
        borderBottom: "1px solid #e2e8f0",
        paddingBottom: 2,
        overflowX: "auto"
      }}>
        {[
          { key: "employees", label: "👥 Employee Directory", count: employees.length },
          { key: "attendance", label: "⏰ Biometric Attendance", count: attendance.length },
          { key: "leaves", label: "🏖️ Leave Requests", count: leaves.length },
          { key: "payroll", label: "💵 Monthly Payroll Register", count: payrolls.length },
          { key: "dispatch", label: "📦 Packing & Delivery Dispatch", count: cartons.length }
        ].map(tab => (
          <button
            key={tab.key}
            onClick={() => setActiveTab(tab.key as any)}
            style={{
              padding: "10px 16px",
              fontSize: "0.875rem",
              fontWeight: activeTab === tab.key ? 600 : 500,
              color: activeTab === tab.key ? "#2563eb" : "#64748b",
              borderBottom: activeTab === tab.key ? "2px solid #2563eb" : "2px solid transparent",
              background: "transparent",
              borderTop: "none",
              borderLeft: "none",
              borderRight: "none",
              cursor: "pointer",
              display: "flex",
              alignItems: "center",
              gap: 8,
              whiteSpace: "nowrap"
            }}
          >
            <span>{tab.label}</span>
            <span style={{
              fontSize: "0.75rem",
              background: activeTab === tab.key ? "#dbeafe" : "#f1f5f9",
              color: activeTab === tab.key ? "#1e40af" : "#64748b",
              padding: "2px 7px",
              borderRadius: 12
            }}>
              {tab.count}
            </span>
          </button>
        ))}
      </div>

      {/* TAB 1: EMPLOYEE DIRECTORY */}
      {activeTab === "employees" && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          <div style={{ display: "flex", justifyContent: "space-between", flexWrap: "wrap", gap: "0.75rem" }}>
            <div style={{ display: "flex", gap: "0.5rem", flex: 1, minWidth: 260 }}>
              <div style={{ position: "relative", width: "100%", maxWidth: 320 }}>
                <Search size={15} style={{ position: "absolute", left: 10, top: 11, color: "#94a3b8" }} />
                <input 
                  type="text" 
                  placeholder="Search by name, ID, or designation..." 
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="form-control"
                  style={{ paddingLeft: 34, height: 38 }}
                />
              </div>

              <select 
                value={selectedDept}
                onChange={(e) => setSelectedDept(e.target.value)}
                className="form-control"
                style={{ width: 180, height: 38 }}
              >
                <option value="ALL">All Departments</option>
                <option value="Sewing Section">Sewing Section</option>
                <option value="Cutting Section">Cutting Section</option>
                <option value="Quality Assurance">Quality Assurance</option>
                <option value="Finishing & Packing">Finishing & Packing</option>
              </select>
            </div>

            <div style={{ fontSize: "0.85rem", color: "#64748b", alignSelf: "center" }}>
              Showing <strong>{filteredEmployees.length}</strong> registered staff
            </div>
          </div>

          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Department & Role</th>
                  <th>Assigned Line</th>
                  <th>Salary Breakdown</th>
                  <th>Banking / MFS</th>
                  <th>Emergency Contact</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {filteredEmployees.map((emp) => (
                  <tr key={emp.id}>
                    <td>
                      <div style={{ display: "flex", alignItems: "center", gap: 10 }}>
                        <div style={{
                          width: 34,
                          height: 34,
                          borderRadius: "50%",
                          background: "#f1f5f9",
                          color: "#334155",
                          display: "flex",
                          alignItems: "center",
                          justifyContent: "center",
                          fontWeight: 700,
                          fontSize: "0.85rem"
                        }}>
                          {emp.fullName.charAt(0)}
                        </div>
                        <div>
                          <div style={{ fontWeight: 600, color: "#0f172a" }}>{emp.fullName}</div>
                          <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{emp.employeeCode} · Joined {new Date(emp.joiningDate).toLocaleDateString()}</div>
                        </div>
                      </div>
                    </td>
                    <td>
                      <div style={{ fontWeight: 500 }}>{emp.designation}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{emp.department}</div>
                    </td>
                    <td>
                      <span className="badge badge-secondary">{emp.assignedLine || "General Floor"}</span>
                    </td>
                    <td>
                      <div style={{ fontWeight: 600, color: "#0f172a" }}>৳ {emp.grossSalary.toLocaleString()}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>Basic: ৳ {emp.basicSalary.toLocaleString()}</div>
                    </td>
                    <td>
                      <div style={{ fontSize: "0.8rem", color: "#334155" }}>{emp.mobileBankingNumber || "Pending"}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>NID: {emp.nationalId?.slice(0, 10)}...</div>
                    </td>
                    <td>
                      <span style={{ fontSize: "0.8rem", color: "#334155" }}>{emp.emergencyContact || emp.phoneNumber}</span>
                    </td>
                    <td>
                      <span className="badge badge-success">{emp.statusName}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TAB 2: BIOMETRIC ATTENDANCE */}
      {activeTab === "attendance" && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
            <div>
              <h3 style={{ fontSize: "1.1rem", fontWeight: 700, color: "#0f172a", margin: 0 }}>
                Today's Biometric Log Stream
              </h3>
              <p style={{ fontSize: "0.8rem", color: "#64748b", margin: 0 }}>
                Real-time punch verification from RFID, Facial Recognition & Optical Fingerprint readers.
              </p>
            </div>

            <button 
              onClick={() => setShowPunchModal(true)}
              className="btn btn-outline"
              style={{ display: "flex", alignItems: "center", gap: 6 }}
            >
              <Fingerprint size={16} />
              <span>Simulate Device Punch</span>
            </button>
          </div>

          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Shift Schedule</th>
                  <th>In-Time</th>
                  <th>Out-Time</th>
                  <th>Overtime</th>
                  <th>Punch Status</th>
                  <th>Verification Notes</th>
                </tr>
              </thead>
              <tbody>
                {attendance.map((att) => (
                  <tr key={att.id}>
                    <td>
                      <div style={{ fontWeight: 600, color: "#0f172a" }}>{att.employeeName}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{att.employeeCode} · {att.department}</div>
                    </td>
                    <td>
                      <span style={{ fontSize: "0.8rem", color: "#334155" }}>{att.shiftName}</span>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{att.assignedLine}</div>
                    </td>
                    <td>
                      <div style={{ fontWeight: 600, color: "#16a34a" }}>{att.checkInTime || "--:--"}</div>
                    </td>
                    <td>
                      <div style={{ fontWeight: 600, color: "#2563eb" }}>{att.checkOutTime || "--:--"}</div>
                    </td>
                    <td>
                      {att.overtimeHours > 0 ? (
                        <span className="badge badge-warning">+{att.overtimeHours} hrs OT</span>
                      ) : (
                        <span style={{ color: "#94a3b8", fontSize: "0.8rem" }}>0.0 hrs</span>
                      )}
                    </td>
                    <td>
                      <span className={`badge ${
                        att.statusName === "Present" ? "badge-success" :
                        att.statusName === "Late" ? "badge-warning" : "badge-danger"
                      }`}>
                        {att.statusName}
                      </span>
                    </td>
                    <td>
                      <span style={{ fontSize: "0.8rem", color: "#64748b" }}>{att.remarks || "Biometric verified"}</span>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TAB 3: LEAVE MANAGEMENT */}
      {activeTab === "leaves" && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
            <div>
              <h3 style={{ fontSize: "1.1rem", fontWeight: 700, color: "#0f172a", margin: 0 }}>
                Leave Requests & Approval Workflow
              </h3>
              <p style={{ fontSize: "0.8rem", color: "#64748b", margin: 0 }}>
                Manage Casual, Sick, Annual and Maternity leaves with 1-click supervisor approval.
              </p>
            </div>

            <button 
              onClick={() => setShowApplyLeaveModal(true)}
              className="btn btn-primary"
              style={{ display: "flex", alignItems: "center", gap: 6 }}
            >
              <Plus size={16} />
              <span>Apply Leave</span>
            </button>
          </div>

          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Leave Type</th>
                  <th>Duration</th>
                  <th>Days</th>
                  <th>Reason</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {leaves.map((l) => (
                  <tr key={l.id}>
                    <td>
                      <div style={{ fontWeight: 600, color: "#0f172a" }}>{l.employeeName}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{l.employeeCode} · {l.department}</div>
                    </td>
                    <td>
                      <span className="badge badge-secondary">{l.leaveTypeName} Leave</span>
                    </td>
                    <td>
                      <div style={{ fontSize: "0.85rem" }}>
                        {new Date(l.startDate).toLocaleDateString()} - {new Date(l.endDate).toLocaleDateString()}
                      </div>
                    </td>
                    <td>
                      <strong>{l.totalDays} day(s)</strong>
                    </td>
                    <td>
                      <span style={{ fontSize: "0.85rem", color: "#334155" }}>{l.reason}</span>
                    </td>
                    <td>
                      <span className={`badge ${
                        l.statusName === "Approved" ? "badge-success" :
                        l.statusName === "Rejected" ? "badge-danger" : "badge-warning"
                      }`}>
                        {l.statusName}
                      </span>
                    </td>
                    <td>
                      {l.statusName === "Pending" ? (
                        <div style={{ display: "flex", gap: 6 }}>
                          <button
                            onClick={() => handleApproveLeave(l.id, true)}
                            className="btn btn-primary"
                            style={{ padding: "4px 10px", fontSize: "0.75rem", background: "#16a34a" }}
                          >
                            Approve
                          </button>
                          <button
                            onClick={() => handleApproveLeave(l.id, false)}
                            className="btn btn-outline"
                            style={{ padding: "4px 10px", fontSize: "0.75rem", color: "#dc2626", borderColor: "#fca5a5" }}
                          >
                            Reject
                          </button>
                        </div>
                      ) : (
                        <span style={{ fontSize: "0.75rem", color: "#64748b" }}>
                          {l.statusName === "Approved" ? `Approved by ${l.approvedBy || "HR"}` : "Rejected"}
                        </span>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TAB 4: MONTHLY PAYROLL REGISTER */}
      {activeTab === "payroll" && (
        <div className="card" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
          <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
            <div>
              <h3 style={{ fontSize: "1.1rem", fontWeight: 700, color: "#0f172a", margin: 0 }}>
                Automated Monthly Salary Register
              </h3>
              <p style={{ fontSize: "0.8rem", color: "#64748b", margin: 0 }}>
                Basic + Allowances + Overtime Pay (1.5x basic hourly) - Absenteeism/Tax Deductions = Net Payable.
              </p>
            </div>

            <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
              <div style={{ textAlign: "right" }}>
                <div style={{ fontSize: "0.75rem", color: "#64748b" }}>Total Batch Net Payable</div>
                <div style={{ fontSize: "1.1rem", fontWeight: 700, color: "#16a34a" }}>৳ {totalPayrollGross.toLocaleString()}</div>
              </div>
              <button
                onClick={handleGeneratePayroll}
                className="btn btn-primary"
                style={{ display: "flex", alignItems: "center", gap: 6 }}
              >
                <RefreshCw size={15} />
                <span>Re-Calculate Payroll</span>
              </button>
            </div>
          </div>

          <div className="table-container">
            <table>
              <thead>
                <tr>
                  <th>Employee</th>
                  <th>Basic Salary</th>
                  <th>Allowances</th>
                  <th>Overtime (1.5x)</th>
                  <th>Deductions</th>
                  <th>Net Payable</th>
                  <th>Status</th>
                  <th>Payslip</th>
                </tr>
              </thead>
              <tbody>
                {payrolls.map((p) => (
                  <tr key={p.id}>
                    <td>
                      <div style={{ fontWeight: 600, color: "#0f172a" }}>{p.employeeName}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{p.employeeCode} · {p.designation}</div>
                    </td>
                    <td>৳ {p.basicSalary.toLocaleString()}</td>
                    <td>৳ {p.allowances.toLocaleString()}</td>
                    <td>
                      <div style={{ fontWeight: 600, color: "#16a34a" }}>+৳ {p.overtimeAmount.toLocaleString()}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{p.overtimeHours} hrs @ 1.5x</div>
                    </td>
                    <td>
                      <div style={{ color: "#dc2626", fontWeight: 500 }}>-৳ {p.totalDeductions.toLocaleString()}</div>
                      <div style={{ fontSize: "0.75rem", color: "#64748b" }}>Tax/Absence</div>
                    </td>
                    <td>
                      <div style={{ fontSize: "1rem", fontWeight: 700, color: "#0f172a" }}>
                        ৳ {p.netPayable.toLocaleString()}
                      </div>
                    </td>
                    <td>
                      <span className="badge badge-success">{p.statusName}</span>
                    </td>
                    <td>
                      <button
                        onClick={() => setShowPayslipModal(p)}
                        className="btn btn-outline"
                        style={{ padding: "4px 10px", fontSize: "0.75rem", display: "flex", alignItems: "center", gap: 4 }}
                      >
                        <FileText size={13} />
                        <span>View Payslip</span>
                      </button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </div>
      )}

      {/* TAB 5: PACKING & DELIVERY DISPATCH */}
      {activeTab === "dispatch" && (
        <div style={{ display: "flex", flexDirection: "column", gap: "1.5rem" }}>
          {/* Section 1: Finished Goods Cartons */}
          <div className="card" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
              <div>
                <h3 style={{ fontSize: "1.1rem", fontWeight: 700, color: "#0f172a", margin: 0 }}>
                  Finished Goods Carton Packaging Registry
                </h3>
                <p style={{ fontSize: "0.8rem", color: "#64748b", margin: 0 }}>
                  Barcoded export cartons verified for final shipment and customs loading.
                </p>
              </div>

              <span className="badge badge-secondary">{cartons.length} Cartons in Warehouse</span>
            </div>

            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Carton Barcode</th>
                    <th>Buyer & Order</th>
                    <th>Style & Color</th>
                    <th>Size Ratio</th>
                    <th>Total Pcs</th>
                    <th>Gross / Net Wt</th>
                    <th>Pallet Loc</th>
                    <th>Status</th>
                  </tr>
                </thead>
                <tbody>
                  {cartons.map((c) => (
                    <tr key={c.id}>
                      <td>
                        <div style={{ fontWeight: 700, color: "#2563eb", fontFamily: "monospace" }}>
                          {c.cartonBarcode}
                        </div>
                      </td>
                      <td>
                        <div style={{ fontWeight: 600, color: "#0f172a" }}>{c.buyerName}</div>
                        <div style={{ fontSize: "0.75rem", color: "#64748b" }}>Order: {c.orderNumber}</div>
                      </td>
                      <td>
                        <div style={{ fontWeight: 500 }}>{c.styleNumber}</div>
                        <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{c.color}</div>
                      </td>
                      <td>
                        <code style={{ background: "#f1f5f9", padding: "2px 6px", borderRadius: 4, fontSize: "0.75rem" }}>
                          {c.sizeBreakdownJson}
                        </code>
                      </td>
                      <td>
                        <strong>{c.totalQuantityPcs} pcs</strong>
                      </td>
                      <td>
                        <span style={{ fontSize: "0.85rem" }}>{c.grossWeightKg} kg / {c.netWeightKg} kg</span>
                      </td>
                      <td>
                        <span className="badge badge-secondary">{c.assignedPallet || "Floor A"}</span>
                      </td>
                      <td>
                        <span className="badge badge-success">{c.statusName}</span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Section 2: Delivery Challans & Gate Passes */}
          <div className="card" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
            <div style={{ display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: "1rem" }}>
              <div>
                <h3 style={{ fontSize: "1.1rem", fontWeight: 700, color: "#0f172a", margin: 0 }}>
                  Delivery Challans & Gate Pass Log
                </h3>
                <p style={{ fontSize: "0.8rem", color: "#64748b", margin: 0 }}>
                  Formal legal dispatch challans with driver details, container number, and security clearance.
                </p>
              </div>

              <button 
                onClick={() => setShowCreateChallanModal(true)}
                className="btn btn-primary"
                style={{ display: "flex", alignItems: "center", gap: 6 }}
              >
                <Plus size={16} />
                <span>Create New Challan</span>
              </button>
            </div>

            <div className="table-container">
              <table>
                <thead>
                  <tr>
                    <th>Challan #</th>
                    <th>Destination Port / Buyer</th>
                    <th>Transport & Driver</th>
                    <th>Total Cartons</th>
                    <th>Garment Pcs</th>
                    <th>Gate Pass #</th>
                    <th>Status</th>
                    <th>Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {challans.length === 0 ? (
                    <tr>
                      <td colSpan={8} style={{ textAlign: "center", padding: "2rem", color: "#94a3b8" }}>
                        No delivery challans issued yet. Click "Create New Challan" to generate gate pass.
                      </td>
                    </tr>
                  ) : (
                    challans.map((ch) => (
                      <tr key={ch.id}>
                        <td>
                          <div style={{ fontWeight: 700, color: "#0f172a" }}>{ch.challanNumber}</div>
                          <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{new Date(ch.dispatchDate).toLocaleDateString()}</div>
                        </td>
                        <td>
                          <div style={{ fontWeight: 600 }}>{ch.buyerName}</div>
                          <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{ch.destinationAddress}</div>
                        </td>
                        <td>
                          <div style={{ fontWeight: 500 }}>{ch.vehicleNumber}</div>
                          <div style={{ fontSize: "0.75rem", color: "#64748b" }}>{ch.driverName} ({ch.driverPhone})</div>
                        </td>
                        <td><strong>{ch.totalCartons} CTN</strong></td>
                        <td><strong>{ch.totalGarmentPcs} pcs</strong></td>
                        <td>
                          {ch.gatePassNumber ? (
                            <span className="badge badge-success">{ch.gatePassNumber}</span>
                          ) : (
                            <span style={{ color: "#94a3b8", fontSize: "0.75rem" }}>Pending Gate Pass</span>
                          )}
                        </td>
                        <td>
                          <span className={`badge ${
                            ch.statusName === "Dispatched" ? "badge-success" :
                            ch.statusName === "Delivered" ? "badge-primary" : "badge-secondary"
                          }`}>
                            {ch.statusName}
                          </span>
                        </td>
                        <td>
                          {ch.statusName === "Prepared" && (
                            <button
                              onClick={() => handleDispatchChallan(ch.id)}
                              className="btn btn-primary"
                              style={{ padding: "4px 10px", fontSize: "0.75rem", display: "flex", alignItems: "center", gap: 4 }}
                            >
                              <Send size={13} />
                              <span>Dispatch</span>
                            </button>
                          )}
                        </td>
                      </tr>
                    ))
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* MODAL: ADD EMPLOYEE */}
      {showAddEmpModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 640 }}>
            <div className="modal-header">
              <h3 style={{ margin: 0, fontSize: "1.2rem", fontWeight: 700 }}>Enroll New Employee</h3>
              <button onClick={() => setShowAddEmpModal(false)} className="btn btn-outline" style={{ padding: 4 }}>✕</button>
            </div>
            <form onSubmit={handleAddEmployee}>
              <div className="modal-body" style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: "1rem" }}>
                <div className="form-group">
                  <label>Employee Code *</label>
                  <input 
                    type="text" 
                    required 
                    placeholder="EMP-1050"
                    value={newEmp.employeeCode}
                    onChange={(e) => setNewEmp({...newEmp, employeeCode: e.target.value})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Full Name *</label>
                  <input 
                    type="text" 
                    required 
                    placeholder="Md. Shakil Ahmed"
                    value={newEmp.fullName}
                    onChange={(e) => setNewEmp({...newEmp, fullName: e.target.value})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Department</label>
                  <select 
                    value={newEmp.department}
                    onChange={(e) => setNewEmp({...newEmp, department: e.target.value})}
                    className="form-control"
                  >
                    <option value="Sewing Section">Sewing Section</option>
                    <option value="Cutting Section">Cutting Section</option>
                    <option value="Quality Assurance">Quality Assurance</option>
                    <option value="Finishing & Packing">Finishing & Packing</option>
                    <option value="Admin & HR">Admin & HR</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Designation</label>
                  <input 
                    type="text" 
                    value={newEmp.designation}
                    onChange={(e) => setNewEmp({...newEmp, designation: e.target.value})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Assigned Line / Floor</label>
                  <input 
                    type="text" 
                    placeholder="Line-03"
                    value={newEmp.assignedLine}
                    onChange={(e) => setNewEmp({...newEmp, assignedLine: e.target.value})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Phone Number</label>
                  <input 
                    type="text" 
                    placeholder="+880 1711-000000"
                    value={newEmp.phoneNumber}
                    onChange={(e) => setNewEmp({...newEmp, phoneNumber: e.target.value})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Basic Salary (৳)</label>
                  <input 
                    type="number" 
                    value={newEmp.basicSalary}
                    onChange={(e) => setNewEmp({...newEmp, basicSalary: Number(e.target.value)})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>House Rent Allowance (৳)</label>
                  <input 
                    type="number" 
                    value={newEmp.houseRentAllowance}
                    onChange={(e) => setNewEmp({...newEmp, houseRentAllowance: Number(e.target.value)})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Mobile Banking / DBBL A/C</label>
                  <input 
                    type="text" 
                    placeholder="bKash: 01711..."
                    value={newEmp.mobileBankingNumber}
                    onChange={(e) => setNewEmp({...newEmp, mobileBankingNumber: e.target.value})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Emergency Contact</label>
                  <input 
                    type="text" 
                    placeholder="+880 1812-..."
                    value={newEmp.emergencyContact}
                    onChange={(e) => setNewEmp({...newEmp, emergencyContact: e.target.value})}
                    className="form-control"
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" onClick={() => setShowAddEmpModal(false)} className="btn btn-outline">Cancel</button>
                <button type="submit" className="btn btn-primary">Save & Register</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: BIOMETRIC PUNCH SIMULATOR */}
      {showPunchModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 480 }}>
            <div className="modal-header">
              <h3 style={{ margin: 0, fontSize: "1.2rem", fontWeight: 700 }}>Biometric Terminal Punch Simulator</h3>
              <button onClick={() => setShowPunchModal(false)} className="btn btn-outline" style={{ padding: 4 }}>✕</button>
            </div>
            <form onSubmit={handleSimulatePunch}>
              <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
                <div className="form-group">
                  <label>Select Employee *</label>
                  <select 
                    value={punchForm.employeeId}
                    onChange={(e) => setPunchForm({...punchForm, employeeId: e.target.value})}
                    className="form-control"
                    required
                  >
                    {employees.map(emp => (
                      <option key={emp.id} value={emp.id}>
                        {emp.fullName} ({emp.employeeCode}) - {emp.department}
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label>Terminal Reader Device</label>
                  <select 
                    value={punchForm.deviceId}
                    onChange={(e) => setPunchForm({...punchForm, deviceId: e.target.value})}
                    className="form-control"
                  >
                    <option value="BIO-GATE-01 (Main Entry)">BIO-GATE-01 (Main Entry Optical Fingerprint)</option>
                    <option value="BIO-GATE-02 (Floor 2)">BIO-GATE-02 (Floor 2 AI Facial Recognition)</option>
                    <option value="BIO-GATE-03 (QA Station)">BIO-GATE-03 (QA Station RFID Terminal)</option>
                  </select>
                </div>

                <div className="form-group">
                  <label>Punch Action</label>
                  <div style={{ display: "flex", gap: 12 }}>
                    <label style={{ display: "flex", alignItems: "center", gap: 6, cursor: "pointer" }}>
                      <input 
                        type="radio" 
                        name="punchType" 
                        checked={!punchForm.isCheckOut}
                        onChange={() => setPunchForm({...punchForm, isCheckOut: false})}
                      />
                      <span>Check-In (Morning Entry)</span>
                    </label>
                    <label style={{ display: "flex", alignItems: "center", gap: 6, cursor: "pointer" }}>
                      <input 
                        type="radio" 
                        name="punchType" 
                        checked={punchForm.isCheckOut}
                        onChange={() => setPunchForm({...punchForm, isCheckOut: true})}
                      />
                      <span>Check-Out (Overtime / Exit)</span>
                    </label>
                  </div>
                </div>

                <div className="form-group">
                  <label>Verification Note / Remarks</label>
                  <input 
                    type="text" 
                    value={punchForm.remarks}
                    onChange={(e) => setPunchForm({...punchForm, remarks: e.target.value})}
                    className="form-control"
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" onClick={() => setShowPunchModal(false)} className="btn btn-outline">Cancel</button>
                <button type="submit" className="btn btn-primary" style={{ background: "#2563eb" }}>Simulate Hardware Punch</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: APPLY LEAVE */}
      {showApplyLeaveModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 480 }}>
            <div className="modal-header">
              <h3 style={{ margin: 0, fontSize: "1.2rem", fontWeight: 700 }}>Apply Employee Leave</h3>
              <button onClick={() => setShowApplyLeaveModal(false)} className="btn btn-outline" style={{ padding: 4 }}>✕</button>
            </div>
            <form onSubmit={handleApplyLeave}>
              <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
                <div className="form-group">
                  <label>Employee *</label>
                  <select 
                    value={leaveForm.employeeId}
                    onChange={(e) => setLeaveForm({...leaveForm, employeeId: e.target.value})}
                    className="form-control"
                    required
                  >
                    {employees.map(emp => (
                      <option key={emp.id} value={emp.id}>
                        {emp.fullName} ({emp.employeeCode})
                      </option>
                    ))}
                  </select>
                </div>

                <div className="form-group">
                  <label>Leave Category</label>
                  <select 
                    value={leaveForm.type}
                    onChange={(e) => setLeaveForm({...leaveForm, type: Number(e.target.value)})}
                    className="form-control"
                  >
                    <option value={1}>Casual Leave</option>
                    <option value={2}>Sick Leave</option>
                    <option value={3}>Annual Earned Leave</option>
                    <option value={4}>Maternity Leave</option>
                    <option value={5}>Unpaid Leave</option>
                  </select>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                  <div className="form-group">
                    <label>Start Date</label>
                    <input 
                      type="date" 
                      value={leaveForm.startDate}
                      onChange={(e) => setLeaveForm({...leaveForm, startDate: e.target.value})}
                      className="form-control"
                    />
                  </div>
                  <div className="form-group">
                    <label>End Date</label>
                    <input 
                      type="date" 
                      value={leaveForm.endDate}
                      onChange={(e) => setLeaveForm({...leaveForm, endDate: e.target.value})}
                      className="form-control"
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Total Days</label>
                  <input 
                    type="number" 
                    value={leaveForm.totalDays}
                    onChange={(e) => setLeaveForm({...leaveForm, totalDays: Number(e.target.value)})}
                    className="form-control"
                  />
                </div>

                <div className="form-group">
                  <label>Reason / Medical Note</label>
                  <textarea 
                    rows={3}
                    placeholder="Medical prescription submitted or personal emergency..."
                    value={leaveForm.reason}
                    onChange={(e) => setLeaveForm({...leaveForm, reason: e.target.value})}
                    className="form-control"
                    required
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" onClick={() => setShowApplyLeaveModal(false)} className="btn btn-outline">Cancel</button>
                <button type="submit" className="btn btn-primary">Submit Application</button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: VIEW PAYSLIP */}
      {showPayslipModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 560, background: "#ffffff" }}>
            <div className="modal-header" style={{ borderBottom: "1px solid #e2e8f0" }}>
              <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <Award size={20} color="#2563eb" />
                <h3 style={{ margin: 0, fontSize: "1.2rem", fontWeight: 700 }}>Employee Salary Payslip</h3>
              </div>
              <button onClick={() => setShowPayslipModal(null)} className="btn btn-outline" style={{ padding: 4 }}>✕</button>
            </div>
            
            <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
              <div style={{ textAlign: "center", padding: "8px 0", borderBottom: "1px dashed #cbd5e1" }}>
                <h4 style={{ margin: 0, fontSize: "1.1rem", fontWeight: 800 }}>StitchMind Apparel OS</h4>
                <div style={{ fontSize: "0.8rem", color: "#64748b" }}>Corporate Apparel Manufacturing Complex · Dhaka, Bangladesh</div>
                <div style={{ fontSize: "0.85rem", fontWeight: 600, color: "#2563eb", marginTop: 4 }}>
                  Pay Slip for Month {showPayslipModal.payMonth} / {showPayslipModal.payYear}
                </div>
              </div>

              <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 10, fontSize: "0.85rem" }}>
                <div><strong>Employee:</strong> {showPayslipModal.employeeName}</div>
                <div><strong>Employee ID:</strong> {showPayslipModal.employeeCode}</div>
                <div><strong>Department:</strong> {showPayslipModal.department}</div>
                <div><strong>Designation:</strong> {showPayslipModal.designation}</div>
              </div>

              <div style={{ background: "#f8fafc", padding: "12px", borderRadius: 8, fontSize: "0.85rem" }}>
                <div style={{ fontWeight: 700, marginBottom: 8, color: "#0f172a" }}>Earnings & Additions</div>
                <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 4 }}>
                  <span>Basic Salary</span>
                  <span>৳ {showPayslipModal.basicSalary.toLocaleString()}</span>
                </div>
                <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 4 }}>
                  <span>Allowances (House Rent + Medical + Conv)</span>
                  <span>৳ {showPayslipModal.allowances.toLocaleString()}</span>
                </div>
                <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 4, color: "#16a34a" }}>
                  <span>Overtime Pay ({showPayslipModal.overtimeHours} hrs @ 1.5x)</span>
                  <span>+৳ {showPayslipModal.overtimeAmount.toLocaleString()}</span>
                </div>

                <div style={{ fontWeight: 700, marginTop: 12, marginBottom: 8, color: "#0f172a" }}>Deductions</div>
                <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 4, color: "#dc2626" }}>
                  <span>Tax & Absenteeism Deductions</span>
                  <span>-৳ {showPayslipModal.totalDeductions.toLocaleString()}</span>
                </div>

                <div style={{
                  display: "flex",
                  justifyContent: "space-between",
                  marginTop: 12,
                  paddingTop: 8,
                  borderTop: "2px solid #0f172a",
                  fontSize: "1rem",
                  fontWeight: 800,
                  color: "#0f172a"
                }}>
                  <span>Net Payable Amount</span>
                  <span>৳ {showPayslipModal.netPayable.toLocaleString()}</span>
                </div>
              </div>

              <div style={{ fontSize: "0.75rem", color: "#64748b", textAlign: "center" }}>
                Reference: {showPayslipModal.paymentReference || "Direct Bank / MFS Disbursed"} · Digitally verified
              </div>
            </div>

            <div className="modal-footer">
              <button onClick={() => window.print()} className="btn btn-outline" style={{ display: "flex", alignItems: "center", gap: 6 }}>
                <Printer size={15} />
                <span>Print Payslip</span>
              </button>
              <button onClick={() => setShowPayslipModal(null)} className="btn btn-primary">Close</button>
            </div>
          </div>
        </div>
      )}

      {/* MODAL: CREATE DELIVERY CHALLAN */}
      {showCreateChallanModal && (
        <div className="modal-backdrop">
          <div className="modal-dialog" style={{ maxWidth: 540 }}>
            <div className="modal-header">
              <h3 style={{ margin: 0, fontSize: "1.2rem", fontWeight: 700 }}>Generate Delivery Challan & Gate Pass</h3>
              <button onClick={() => setShowCreateChallanModal(false)} className="btn btn-outline" style={{ padding: 4 }}>✕</button>
            </div>
            <form onSubmit={handleCreateChallan}>
              <div className="modal-body" style={{ display: "flex", flexDirection: "column", gap: "1rem" }}>
                <div className="form-group">
                  <label>Destination Port / Warehouse *</label>
                  <input 
                    type="text" 
                    required 
                    value={challanForm.destinationAddress}
                    onChange={(e) => setChallanForm({...challanForm, destinationAddress: e.target.value})}
                    className="form-control"
                  />
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                  <div className="form-group">
                    <label>Vehicle / Truck No *</label>
                    <input 
                      type="text" 
                      required 
                      value={challanForm.vehicleNumber}
                      onChange={(e) => setChallanForm({...challanForm, vehicleNumber: e.target.value})}
                      className="form-control"
                    />
                  </div>
                  <div className="form-group">
                    <label>Driver Name *</label>
                    <input 
                      type="text" 
                      required 
                      value={challanForm.driverName}
                      onChange={(e) => setChallanForm({...challanForm, driverName: e.target.value})}
                      className="form-control"
                    />
                  </div>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                  <div className="form-group">
                    <label>Driver Phone Number</label>
                    <input 
                      type="text" 
                      value={challanForm.driverPhone}
                      onChange={(e) => setChallanForm({...challanForm, driverPhone: e.target.value})}
                      className="form-control"
                    />
                  </div>
                  <div className="form-group">
                    <label>Total Cartons</label>
                    <input 
                      type="number" 
                      value={challanForm.totalCartons}
                      onChange={(e) => setChallanForm({...challanForm, totalCartons: Number(e.target.value)})}
                      className="form-control"
                    />
                  </div>
                </div>

                <div style={{ display: "grid", gridTemplateColumns: "1fr 1fr", gap: 12 }}>
                  <div className="form-group">
                    <label>Total Garment Pieces</label>
                    <input 
                      type="number" 
                      value={challanForm.totalGarmentPcs}
                      onChange={(e) => setChallanForm({...challanForm, totalGarmentPcs: Number(e.target.value)})}
                      className="form-control"
                    />
                  </div>
                  <div className="form-group">
                    <label>Total Gross Weight (Kg)</label>
                    <input 
                      type="number" 
                      step="0.1"
                      value={challanForm.totalWeightKg}
                      onChange={(e) => setChallanForm({...challanForm, totalWeightKg: Number(e.target.value)})}
                      className="form-control"
                    />
                  </div>
                </div>

                <div className="form-group">
                  <label>Dispatch Notes</label>
                  <input 
                    type="text" 
                    value={challanForm.remarks}
                    onChange={(e) => setChallanForm({...challanForm, remarks: e.target.value})}
                    className="form-control"
                  />
                </div>
              </div>
              <div className="modal-footer">
                <button type="button" onClick={() => setShowCreateChallanModal(false)} className="btn btn-outline">Cancel</button>
                <button type="submit" className="btn btn-primary" style={{ background: "#2563eb" }}>Generate & Print Challan</button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
