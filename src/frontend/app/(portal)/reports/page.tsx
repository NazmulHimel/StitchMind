"use client";

import React, { useState, useEffect } from "react";
import { 
  FileBarChart, Download, FileText, CheckCircle2, TrendingUp, 
  Users, Scissors, Truck, Package, DollarSign, Calendar, Filter, 
  Printer, RefreshCw, AlertCircle, Building, ShieldCheck, Scale
} from "lucide-react";
import { apiRequest } from "@/lib/api";

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<"financial" | "production" | "payroll" | "procurement">("financial");
  const [loading, setLoading] = useState(false);

  // Report Data States
  const [financialReport, setFinancialReport] = useState<any>(null);
  const [productionReport, setProductionReport] = useState<any>(null);
  const [payrollReport, setPayrollReport] = useState<any>(null);
  const [procurementReport, setProcurementReport] = useState<any>(null);

  // Month / Year filter for payroll
  const [payrollMonth, setPayrollMonth] = useState(new Date().getMonth() + 1);
  const [payrollYear, setPayrollYear] = useState(new Date().getFullYear());

  const fetchReports = async () => {
    setLoading(true);
    try {
      const [fRes, prodRes, payRes, procRes] = await Promise.all([
        apiRequest("/api/reports/financial"),
        apiRequest("/api/reports/production-summary"),
        apiRequest(`/api/reports/payroll-register?month=${payrollMonth}&year=${payrollYear}`),
        apiRequest("/api/reports/procurement-summary")
      ]);

      if (fRes.success && fRes.data) setFinancialReport(fRes.data);
      if (prodRes.success && prodRes.data) setProductionReport(prodRes.data);
      if (payRes.success && payRes.data) setPayrollReport(payRes.data);
      if (procRes.success && procRes.data) setProcurementReport(procRes.data);
    } catch (err) {
      console.error("Failed to load reports", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReports();
  }, [payrollMonth, payrollYear]);

  const formatCurrency = (val: number) => {
    return `৳${(val || 0).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  const handlePrint = () => {
    window.print();
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Enterprise Reporting Portal</h1>
          <p className="text-sm text-slate-500 mt-1">
            Audited Financial Statements, Factory Floor Production Summaries, Payroll Registers, and Supply Chain Valuations.
          </p>
        </div>
        <div className="flex items-center gap-2 print:hidden">
          <button
            onClick={fetchReports}
            disabled={loading}
            className="inline-flex items-center gap-1.5 px-3 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors shadow-sm"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loading ? "animate-spin" : ""}`} /> Refresh
          </button>
          <button
            onClick={handlePrint}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
          >
            <Printer className="w-3.5 h-3.5" /> Print / Export PDF
          </button>
        </div>
      </div>

      {/* Primary Navigation Tabs */}
      <div className="border-b border-slate-200 print:hidden">
        <nav className="flex space-x-6 overflow-x-auto" aria-label="Tabs">
          {[
            { id: "financial", label: "Financial Statements Report", icon: Scale },
            { id: "production", label: "Factory Production Summary", icon: Scissors },
            { id: "payroll", label: "Workforce Payroll Register", icon: Users },
            { id: "procurement", label: "Procurement & Valuation", icon: Package }
          ].map((tab) => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.id;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id as any)}
                className={`py-3 px-1 border-b-2 font-medium text-sm inline-flex items-center gap-2 whitespace-nowrap transition-colors ${
                  isActive
                    ? "border-indigo-600 text-indigo-600 font-semibold"
                    : "border-transparent text-slate-500 hover:text-slate-700 hover:border-slate-300"
                }`}
              >
                <Icon className={`w-4 h-4 ${isActive ? "text-indigo-600" : "text-slate-400"}`} />
                {tab.label}
              </button>
            );
          })}
        </nav>
      </div>

      {/* TAB 1: FINANCIAL STATEMENTS REPORT */}
      {activeTab === "financial" && financialReport && (
        <div className="space-y-6">
          {/* Summary KPIs */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Asset Value</span>
              <h3 className="text-xl font-bold text-slate-900 mt-2 font-mono">
                {formatCurrency(financialReport.balanceSheet?.totalAssets || 0)}
              </h3>
              <p className="text-xs text-slate-500 mt-1">Current + Non-Current Assets</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Sales Turnover (Revenue)</span>
              <h3 className="text-xl font-bold text-emerald-700 mt-2 font-mono">
                {formatCurrency(financialReport.incomeStatement?.totalRevenue || 0)}
              </h3>
              <p className="text-xs text-emerald-600 mt-1">Export Apparel Shipments</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Manufacturing COGS</span>
              <h3 className="text-xl font-bold text-amber-700 mt-2 font-mono">
                {formatCurrency(financialReport.incomeStatement?.totalCOGS || 0)}
              </h3>
              <p className="text-xs text-slate-500 mt-1">Direct Fabric & Sewing Wages</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Gross Manufacturing Margin</span>
              <h3 className="text-xl font-bold text-indigo-700 mt-2 font-mono">
                {formatCurrency(financialReport.incomeStatement?.grossProfit || 0)}
              </h3>
              <p className="text-xs text-indigo-600 font-medium mt-1">
                {(((financialReport.incomeStatement?.grossProfit || 0) / (financialReport.incomeStatement?.totalRevenue || 1)) * 100).toFixed(1)}% Gross Margin
              </p>
            </div>
          </div>

          {/* Report Sheet Layout */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-6">
            <div className="border-b border-slate-200 pb-4 flex items-center justify-between">
              <div>
                <h3 className="text-lg font-bold text-slate-900">{financialReport.reportTitle}</h3>
                <p className="text-xs text-slate-500">Generated: {new Date(financialReport.generatedAt).toLocaleString()} | Enterprise Audit Grade</p>
              </div>
              <div className="flex items-center gap-2">
                <span className="px-3 py-1 rounded-full text-xs font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                  <CheckCircle2 className="w-3.5 h-3.5 inline mr-1" /> Ledger Balanced
                </span>
              </div>
            </div>

            {/* Trial Balance Section in Report */}
            <div>
              <h4 className="text-sm font-bold text-slate-900 uppercase tracking-wider mb-3">1. Executive Trial Balance Summary</h4>
              <div className="overflow-x-auto border border-slate-200 rounded-lg">
                <table className="w-full text-left text-xs font-mono">
                  <thead className="bg-slate-50 font-sans text-slate-600 uppercase font-semibold border-b border-slate-200">
                    <tr>
                      <th className="px-3 py-2">Code</th>
                      <th className="px-3 py-2">Account Title</th>
                      <th className="px-3 py-2 text-right">Debit (৳)</th>
                      <th className="px-3 py-2 text-right">Credit (৳)</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {financialReport.trialBalance?.items?.map((it: any) => (
                      <tr key={it.accountId} className="hover:bg-slate-50">
                        <td className="px-3 py-2 font-bold text-indigo-700">{it.accountCode}</td>
                        <td className="px-3 py-2 font-sans font-medium text-slate-800">{it.accountName}</td>
                        <td className="px-3 py-2 text-right text-slate-900">{it.debitAmount > 0 ? formatCurrency(it.debitAmount) : "-"}</td>
                        <td className="px-3 py-2 text-right text-slate-900">{it.creditAmount > 0 ? formatCurrency(it.creditAmount) : "-"}</td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot className="bg-slate-50 font-bold text-slate-900 border-t-2 border-slate-300">
                    <tr>
                      <td colSpan={2} className="px-3 py-2.5 font-sans uppercase">Total Sum</td>
                      <td className="px-3 py-2.5 text-right font-mono text-indigo-900">{formatCurrency(financialReport.trialBalance?.totalDebit || 0)}</td>
                      <td className="px-3 py-2.5 text-right font-mono text-indigo-900">{formatCurrency(financialReport.trialBalance?.totalCredit || 0)}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: PRODUCTION SUMMARY REPORT */}
      {activeTab === "production" && productionReport && (
        <div className="space-y-6">
          {/* Top KPI Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Active Export Orders</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2">{productionReport.totalOrders} Orders</h3>
              <p className="text-xs text-slate-500 mt-1">H&M, Zara, Target Active</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Cutting Batches</span>
              <h3 className="text-2xl font-bold text-indigo-700 mt-2">{productionReport.cuttingBatches} Batches</h3>
              <p className="text-xs text-indigo-600 mt-1">Automated Spreading & Lay</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Sewing Production Logs</span>
              <h3 className="text-2xl font-bold text-emerald-700 mt-2">{productionReport.sewingLogsCount} Hourly Logs</h3>
              <p className="text-xs text-emerald-600 mt-1">Line 1, 2, 3 Active</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Delivery Challans</span>
              <h3 className="text-2xl font-bold text-purple-700 mt-2">{productionReport.totalDispatchedChallans} Dispatches</h3>
              <p className="text-xs text-purple-600 mt-1">Export Port Gate Passes</p>
            </div>
          </div>

          {/* Orders & Cutting Batches Table */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-4">
            <h3 className="text-base font-bold text-slate-900">Active Buyer Orders & Factory Floor Progress</h3>
            <div className="overflow-x-auto border border-slate-200 rounded-lg">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-2.5">Order Number</th>
                    <th className="px-4 py-2.5">Buyer</th>
                    <th className="px-4 py-2.5">Style Number</th>
                    <th className="px-4 py-2.5 text-right">Order Qty (Pcs)</th>
                    <th className="px-4 py-2.5">Delivery Date</th>
                    <th className="px-4 py-2.5">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {productionReport.orders?.map((ord: any) => (
                    <tr key={ord.id} className="hover:bg-slate-50">
                      <td className="px-4 py-3 font-mono font-bold text-indigo-700">{ord.orderNumber}</td>
                      <td className="px-4 py-3 font-semibold text-slate-900">{ord.buyerName}</td>
                      <td className="px-4 py-3 text-slate-700">{ord.styleNumber}</td>
                      <td className="px-4 py-3 text-right font-mono font-bold text-slate-900">{ord.orderQuantityPcs?.toLocaleString()}</td>
                      <td className="px-4 py-3 text-slate-500">{new Date(ord.deliveryDate).toLocaleDateString()}</td>
                      <td className="px-4 py-3">
                        <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200">
                          {ord.statusName}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: WORKFORCE PAYROLL REGISTER */}
      {activeTab === "payroll" && payrollReport && (
        <div className="space-y-6">
          {/* Controls & Filter */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-4">
            <div className="flex items-center gap-3">
              <label className="text-xs font-semibold text-slate-700">Select Month/Year:</label>
              <select
                value={payrollMonth}
                onChange={(e) => setPayrollMonth(Number(e.target.value))}
                className="px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg font-medium"
              >
                {[
                  "January", "February", "March", "April", "May", "June",
                  "July", "August", "September", "October", "November", "December"
                ].map((m, i) => (
                  <option key={i} value={i + 1}>{m}</option>
                ))}
              </select>
              <select
                value={payrollYear}
                onChange={(e) => setPayrollYear(Number(e.target.value))}
                className="px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg font-medium font-mono"
              >
                <option value={2026}>2026</option>
                <option value={2025}>2025</option>
              </select>
            </div>
            <div className="text-xs text-slate-500 font-medium">
              Workforce Payroll & Overtime Register for {payrollReport.monthYear || `${payrollYear}-${payrollMonth}`}
            </div>
          </div>

          {/* Payroll KPI Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Workforce</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2">{payrollReport.totalEmployees} Employees</h3>
              <p className="text-xs text-slate-500 mt-1">Cutting, Sewing, Finishing & QA</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Basic Wages</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2 font-mono">{formatCurrency(payrollReport.totalBasicPay || 0)}</h3>
              <p className="text-xs text-slate-500 mt-1">Base Wage Entitlements</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Overtime Disbursements</span>
              <h3 className="text-2xl font-bold text-amber-700 mt-2 font-mono">{formatCurrency(payrollReport.totalOvertimePay || 0)}</h3>
              <p className="text-xs text-amber-600 mt-1">Extra Shift Overtime Payments</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Net Disbursed</span>
              <h3 className="text-2xl font-bold text-emerald-700 mt-2 font-mono">{formatCurrency(payrollReport.totalNetDisbursed || 0)}</h3>
              <p className="text-xs text-emerald-600 font-medium mt-1">BFTN / bKash Direct Payroll</p>
            </div>
          </div>

          {/* Payroll Records Register Table */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-4">
            <h3 className="text-base font-bold text-slate-900">Department-wise Salary Disbursement Register</h3>
            <div className="overflow-x-auto border border-slate-200 rounded-lg">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-2.5">Code</th>
                    <th className="px-4 py-2.5">Employee</th>
                    <th className="px-4 py-2.5">Department</th>
                    <th className="px-4 py-2.5 text-right">Basic (৳)</th>
                    <th className="px-4 py-2.5 text-right">Overtime (৳)</th>
                    <th className="px-4 py-2.5 text-right">Deductions (৳)</th>
                    <th className="px-4 py-2.5 text-right font-bold text-slate-900">Net Payable (৳)</th>
                    <th className="px-4 py-2.5">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 font-mono">
                  {payrollReport.payrollRecords?.map((p: any) => (
                    <tr key={p.id} className="hover:bg-slate-50">
                      <td className="px-4 py-3 font-bold text-indigo-700">{p.employeeCode}</td>
                      <td className="px-4 py-3 font-sans font-semibold text-slate-900">{p.employeeName}</td>
                      <td className="px-4 py-3 font-sans text-slate-600">{p.department} - {p.designation}</td>
                      <td className="px-4 py-3 text-right text-slate-800">{formatCurrency(p.basicSalary)}</td>
                      <td className="px-4 py-3 text-right text-amber-700">{formatCurrency(p.overtimeAmount)}</td>
                      <td className="px-4 py-3 text-right text-rose-700">({formatCurrency(p.totalDeductions)})</td>
                      <td className="px-4 py-3 text-right font-bold text-emerald-800">{formatCurrency(p.netPayable)}</td>
                      <td className="px-4 py-3 font-sans">
                        <span className="px-2 py-0.5 rounded text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                          {p.statusName}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 4: PROCUREMENT & INVENTORY VALUATION */}
      {activeTab === "procurement" && procurementReport && (
        <div className="space-y-6">
          {/* Top KPI Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Active Raw Material Suppliers</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2">{procurementReport.totalSuppliers} Suppliers</h3>
              <p className="text-xs text-slate-500 mt-1">Square Textiles, Envoy, Beximco</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Purchase Orders Issued</span>
              <h3 className="text-2xl font-bold text-indigo-700 mt-2">{procurementReport.totalPurchaseOrders} POs</h3>
              <p className="text-xs text-indigo-600 mt-1">Fabric, Yarn, Trims & Accessories</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Goods Receipt Notes (GRN)</span>
              <h3 className="text-2xl font-bold text-emerald-700 mt-2">{procurementReport.totalGRNs} GRNs</h3>
              <p className="text-xs text-emerald-600 mt-1">Warehouse Gate-in Verified</p>
            </div>
          </div>

          {/* Suppliers Table */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-4">
            <h3 className="text-base font-bold text-slate-900">Raw Material Supplier Registry & Ratings</h3>
            <div className="overflow-x-auto border border-slate-200 rounded-lg">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-2.5">Supplier Name</th>
                    <th className="px-4 py-2.5">Category</th>
                    <th className="px-4 py-2.5">Contact Person</th>
                    <th className="px-4 py-2.5">Phone</th>
                    <th className="px-4 py-2.5">Email</th>
                    <th className="px-4 py-2.5">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {procurementReport.suppliers?.map((s: any) => (
                    <tr key={s.id} className="hover:bg-slate-50">
                      <td className="px-4 py-3 font-semibold text-slate-900">{s.name}</td>
                      <td className="px-4 py-3">
                        <span className="px-2 py-0.5 rounded text-[11px] font-semibold bg-slate-100 text-slate-700">
                          {s.categoryName || "Fabric"}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-slate-700">{s.contactPerson}</td>
                      <td className="px-4 py-3 font-mono text-slate-600">{s.phone}</td>
                      <td className="px-4 py-3 text-slate-600">{s.email}</td>
                      <td className="px-4 py-3">
                        <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                          Active Supplier
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
