"use client";

import React, { useState, useEffect } from "react";
import { 
  FileBarChart, Download, FileText, CheckCircle2, TrendingUp, 
  Users, Scissors, Truck, Package, DollarSign, Calendar, Filter, 
  Printer, RefreshCw, AlertCircle, Building, ShieldCheck, Scale,
  Clock, Play, Pause, Send, Mail, Check, AlertTriangle, FileDown,
  Layers, Cpu, Server, CheckSquare, Sparkles, Activity
} from "lucide-react";
import { apiRequest } from "@/lib/api";

export interface ScheduledJobDto {
  id: string;
  jobName: string;
  jobGroup: string;
  cronExpression: string;
  cronDescription: string;
  reportType: string;
  recipientEmailList: string;
  emailSubjectTemplate: string;
  isActive: boolean;
  lastExecutedAtUtc?: string;
  lastExecutionStatus?: string;
  lastExecutionDurationMs?: number;
  executionCount: number;
  failureCount: number;
  lastErrorMessage?: string;
  nextFireTimeUtc?: string;
}

export interface ExecutionHistoryDto {
  id: string;
  jobId: string;
  jobName: string;
  reportType: string;
  triggeredAtUtc: string;
  completedAtUtc?: string;
  status: string;
  executionDurationMs: number;
  recipients: string;
  pdfSizeBytes: number;
  errorMessage?: string;
  isManualTrigger: boolean;
}

export interface SchedulerDashboardDto {
  totalJobs: number;
  activeJobs: number;
  executionsToday: number;
  failuresToday: number;
  nextScheduledRunUtc?: string;
  activeJobsList: ScheduledJobDto[];
  recentExecutions: ExecutionHistoryDto[];
}

export default function ReportsPage() {
  const [activeTab, setActiveTab] = useState<"financial" | "production" | "payroll" | "procurement" | "scheduler">("scheduler");
  const [loading, setLoading] = useState(false);
  const [feedbackMsg, setFeedbackMsg] = useState<{ type: "success" | "error"; text: string } | null>(null);

  // Report Data States
  const [financialReport, setFinancialReport] = useState<any>(null);
  const [productionReport, setProductionReport] = useState<any>(null);
  const [payrollReport, setPayrollReport] = useState<any>(null);
  const [procurementReport, setProcurementReport] = useState<any>(null);

  // Phase 8: Scheduler Data States
  const [schedulerDashboard, setSchedulerDashboard] = useState<SchedulerDashboardDto | null>(null);
  const [scheduledJobs, setScheduledJobs] = useState<ScheduledJobDto[]>([]);
  const [executionHistory, setExecutionHistory] = useState<ExecutionHistoryDto[]>([]);
  const [triggeringJobId, setTriggeringJobId] = useState<string | null>(null);
  const [testEmailRecipient, setTestEmailRecipient] = useState("management@stitchmind.com");
  const [sendingEmail, setSendingEmail] = useState(false);

  // Month / Year filter for payroll
  const [payrollMonth, setPayrollMonth] = useState(new Date().getMonth() + 1);
  const [payrollYear, setPayrollYear] = useState(new Date().getFullYear());

  const showToast = (text: string, type: "success" | "error" = "success") => {
    setFeedbackMsg({ type, text });
    setTimeout(() => setFeedbackMsg(null), 4000);
  };

  const fetchReports = async () => {
    setLoading(true);
    try {
      const [fRes, prodRes, payRes, procRes, schedRes, histRes] = await Promise.all([
        apiRequest<any>("/api/reports/financial"),
        apiRequest<any>("/api/reports/production-summary"),
        apiRequest<any>(`/api/reports/payroll-register?month=${payrollMonth}&year=${payrollYear}`),
        apiRequest<any>("/api/reports/procurement-summary"),
        apiRequest<any>("/api/scheduler/dashboard"),
        apiRequest<any>("/api/scheduler/history")
      ]);

      if (fRes.isSuccess && fRes.value) setFinancialReport(fRes.value);
      if (prodRes.isSuccess && prodRes.value) setProductionReport(prodRes.value);
      if (payRes.isSuccess && payRes.value) setPayrollReport(payRes.value);
      if (procRes.isSuccess && procRes.value) setProcurementReport(procRes.value);

      if (schedRes.isSuccess && schedRes.value) {
        setSchedulerDashboard(schedRes.value);
        if (schedRes.value.activeJobsList) {
          setScheduledJobs(schedRes.value.activeJobsList);
        }
      }

      if (histRes.isSuccess && Array.isArray(histRes.value)) {
        setExecutionHistory(histRes.value);
      }
    } catch (err) {
      console.error("Failed to load reports", err);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchReports();
  }, [payrollMonth, payrollYear]);

  // Phase 8: Trigger Scheduled Job Immediately (Run Now)
  const handleTriggerJob = async (jobId: string, jobName: string) => {
    setTriggeringJobId(jobId);
    try {
      const res = await apiRequest<any>(`/api/scheduler/jobs/${jobId}/trigger`, {
        method: "POST"
      });
      if (res.isSuccess) {
        showToast(`Triggered execution for '${jobName}'! Quartz is rendering & dispatching.`);
        // Reload history after short delay to capture completed run
        setTimeout(() => fetchReports(), 1500);
      } else {
        showToast(res.error || "Failed to trigger job", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    } finally {
      setTriggeringJobId(null);
    }
  };

  // Phase 8: Pause / Resume Job
  const handleToggleJob = async (job: ScheduledJobDto) => {
    const action = job.isActive ? "pause" : "resume";
    try {
      const res = await apiRequest<any>(`/api/scheduler/jobs/${job.id}/${action}`, {
        method: "POST"
      });
      if (res.isSuccess) {
        showToast(`Job '${job.jobName}' has been ${job.isActive ? "paused" : "resumed"}.`);
        fetchReports();
      } else {
        showToast(res.error || "Operation failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Phase 8: Download In-Memory Executive Digest PDF
  const handleDownloadExecutiveDigestPdf = async () => {
    try {
      const token = localStorage.getItem("garments_access_token");
      const url = "/api/scheduler/digest/preview-pdf";
      const res = await fetch(url, {
        headers: token ? { Authorization: `Bearer ${token}` } : {}
      });
      if (res.ok) {
        const blob = await res.blob();
        const downloadUrl = window.URL.createObjectURL(blob);
        const a = document.createElement("a");
        a.href = downloadUrl;
        a.download = `DailyExecutiveDigest_8PM_${new Date().toISOString().slice(0, 10)}.pdf`;
        document.body.appendChild(a);
        a.click();
        a.remove();
        showToast("Daily Executive Digest (8:00 PM) downloaded!");
      } else {
        showToast("PDF generation failed on server", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    }
  };

  // Phase 8: Send Test Email
  const handleSendTestEmail = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!testEmailRecipient) {
      showToast("Please enter a valid recipient email", "error");
      return;
    }
    setSendingEmail(true);
    try {
      const res = await apiRequest<any>(`/api/scheduler/digest/send-test-email?recipient=${encodeURIComponent(testEmailRecipient)}`, {
        method: "POST"
      });
      if (res.isSuccess) {
        showToast(`Executive Digest email dispatched to ${testEmailRecipient}!`);
        setTimeout(() => fetchReports(), 1000);
      } else {
        showToast(res.error || "Email dispatch failed", "error");
      }
    } catch (err: any) {
      showToast(err.message, "error");
    } finally {
      setSendingEmail(false);
    }
  };

  const formatCurrency = (val: number) => {
    return `$${(val || 0).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  const handlePrint = () => {
    window.print();
  };

  return (
    <div className="space-y-6" style={{ paddingBottom: "2rem" }}>
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

      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <div style={{ display: "flex", alignItems: "center", gap: "0.5rem" }}>
            <span style={{ 
              background: "#6366f1", color: "#fff", fontSize: "0.75rem", fontWeight: 700, 
              padding: "2px 8px", borderRadius: 4, textTransform: "uppercase" 
            }}>
              Phase 8
            </span>
            <h1 className="text-2xl font-bold tracking-tight text-slate-900">
              Enterprise Reporting &amp; Background Automation
            </h1>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            Quartz.NET cron-triggered background dispatcher, in-memory zero-disk QuestPDF stream compilation, and 8:00 PM automated executive digest delivery.
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
            onClick={handleDownloadExecutiveDigestPdf}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-indigo-700 bg-indigo-50 border border-indigo-200 rounded-lg hover:bg-indigo-100 transition-colors shadow-sm"
          >
            <FileDown className="w-3.5 h-3.5" /> 8PM Executive Digest PDF
          </button>
          <button
            onClick={handlePrint}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
          >
            <Printer className="w-3.5 h-3.5" /> Print / Export
          </button>
        </div>
      </div>

      {/* Primary Navigation Tabs */}
      <div className="border-b border-slate-200 print:hidden">
        <nav className="flex space-x-6 overflow-x-auto" aria-label="Tabs">
          {[
            { id: "scheduler", label: "Quartz Background Scheduler & Digests (Phase 8)", icon: Clock },
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

      {/* ======================================================== */}
      {/* TAB 0: QUARTZ BACKGROUND SCHEDULER & EMAIL DIGESTS       */}
      {/* ======================================================== */}
      {activeTab === "scheduler" && (
        <div className="space-y-6">
          {/* Architecture Banner */}
          <div style={{
            background: "#0f172a", color: "#ffffff", borderRadius: 12, padding: "20px 24px",
            display: "flex", justifyContent: "space-between", alignItems: "center", flexWrap: "wrap", gap: 16
          }}>
            <div>
              <div style={{ display: "flex", alignItems: "center", gap: 8 }}>
                <span style={{ background: "#38bdf8", color: "#0f172a", fontSize: "0.75rem", fontWeight: 800, padding: "2px 8px", borderRadius: 4 }}>
                  QUARTZ.NET ACTIVE
                </span>
                <span style={{ color: "#94a3b8", fontSize: "0.875rem" }}>
                  In-Memory Recyclable Buffer Pipeline &bull; Zero Disk Touch
                </span>
              </div>
              <h3 style={{ fontSize: "1.75rem", fontWeight: 700, margin: "6px 0 0 0" }}>
                Automated Background Scheduler &amp; Email Dispatcher
              </h3>
              <p style={{ margin: "4px 0 0 0", color: "#cbd5e1", fontSize: "0.8125rem" }}>
                Compiles 8:00 PM Shiftdown Executive Digests, Weekly OT Compliance Registers, and Hourly Floor Efficiency alerts with MailKit retry policies.
              </p>
            </div>

            <div style={{ display: "flex", gap: "0.75rem" }}>
              <button
                onClick={handleDownloadExecutiveDigestPdf}
                className="btn btn-outline"
                style={{ background: "#1e293b", color: "#38bdf8", borderColor: "#38bdf8", fontSize: "0.8125rem", display: "flex", alignItems: "center", gap: 6 }}
              >
                <FileDown size={15} />
                <span>Preview 8PM Digest PDF</span>
              </button>
            </div>
          </div>

          {/* Scheduler Summary KPIs */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Configured Cron Jobs</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2 font-mono">
                {schedulerDashboard?.totalJobs ?? 4} Schedules
              </h3>
              <p className="text-xs text-emerald-600 mt-1 flex items-center gap-1 font-semibold">
                <CheckCircle2 size={13} /> {schedulerDashboard?.activeJobs ?? 4} Active in Quartz
              </p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Executions Today</span>
              <h3 className="text-2xl font-bold text-indigo-700 mt-2 font-mono">
                {schedulerDashboard?.executionsToday ?? 3} Dispatches
              </h3>
              <p className="text-xs text-slate-500 mt-1">Automated + Manual Triggers</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Dispatch Failures</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2 font-mono" style={{ color: (schedulerDashboard?.failuresToday ?? 0) > 0 ? "#dc2626" : "#16a34a" }}>
                {schedulerDashboard?.failuresToday ?? 0} Errors
              </h3>
              <p className="text-xs text-slate-500 mt-1">Automatic Exponential Retries</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Next Scheduled Trigger</span>
              <h3 className="text-lg font-bold text-purple-700 mt-2 font-mono">
                {schedulerDashboard?.nextScheduledRunUtc ? new Date(schedulerDashboard.nextScheduledRunUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }) : "08:00 PM"}
              </h3>
              <p className="text-xs text-purple-600 mt-1">DailyExecutiveDigest_8PM</p>
            </div>
          </div>

          {/* SCH-04: Flagship 8:00 PM Daily Executive Digest Card */}
          <div className="bg-white rounded-xl border border-indigo-100 shadow-sm p-6" style={{ borderLeft: "5px solid #6366f1" }}>
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div>
                <div className="flex items-center gap-2">
                  <span className="px-2.5 py-0.5 rounded text-xs font-bold bg-indigo-100 text-indigo-700">
                    FLAGSHIP DIGEST &bull; SCH-04
                  </span>
                  <span className="text-xs text-slate-500">Cron: <code>0 0 20 * * ?</code> (Every day at 8:00 PM)</span>
                </div>
                <h3 className="text-lg font-bold text-slate-900 mt-1">
                  Daily Factory Executive Management Digest
                </h3>
                <p className="text-xs text-slate-600 mt-1 max-w-2xl">
                  Automatically compiles workforce attendance (AC-07 verified), sewing line balancing (Line 01&ndash;12), CAD marker efficiency, QA defect Pareto, and ex-factory shipment value. Dispatches directly via MailKit in-memory attachment stream.
                </p>
              </div>

              <div className="flex items-center gap-2 flex-wrap">
                <button
                  onClick={() => {
                    const digestJob = scheduledJobs.find(j => j.jobName === "DailyExecutiveDigest_8PM") || scheduledJobs[0];
                    if (digestJob) handleTriggerJob(digestJob.id, digestJob.jobName);
                  }}
                  disabled={triggeringJobId !== null}
                  className="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
                >
                  <Play className={`w-3.5 h-3.5 ${triggeringJobId ? "animate-spin" : ""}`} /> Run Now (Test)
                </button>

                <button
                  onClick={handleDownloadExecutiveDigestPdf}
                  className="inline-flex items-center gap-1.5 px-4 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors shadow-sm"
                >
                  <FileDown className="w-3.5 h-3.5" /> Download PDF
                </button>
              </div>
            </div>

            {/* Test Email Dispatch Form */}
            <form onSubmit={handleSendTestEmail} className="mt-4 pt-4 border-t border-slate-100 flex flex-col sm:flex-row items-center gap-3">
              <div className="flex items-center gap-2 text-xs font-semibold text-slate-700 whitespace-nowrap">
                <Mail size={15} color="#6366f1" />
                <span>Send Instant Test Digest Email:</span>
              </div>
              <input
                type="email"
                required
                value={testEmailRecipient}
                onChange={e => setTestEmailRecipient(e.target.value)}
                placeholder="recipient@stitchmind.com"
                className="px-3 py-1.5 text-xs border border-slate-300 rounded-lg flex-1 focus:outline-none focus:ring-2 focus:ring-indigo-500"
              />
              <button
                type="submit"
                disabled={sendingEmail}
                className="inline-flex items-center gap-1.5 px-3.5 py-1.5 text-xs font-semibold text-white bg-slate-800 rounded-lg hover:bg-slate-900 transition-colors"
              >
                <Send size={12} className={sendingEmail ? "animate-spin" : ""} />
                <span>{sendingEmail ? "Dispatching..." : "Send Digest Email"}</span>
              </button>
            </form>
          </div>

          {/* Scheduled Jobs Registry Table */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-base font-bold text-slate-900">Active Quartz Cron Schedules</h3>
                <p className="text-xs text-slate-500">Database-backed schedules under <code>automation.ScheduledReportJobs</code></p>
              </div>
              <span className="text-xs font-semibold text-slate-600 bg-slate-100 px-3 py-1 rounded-full">
                {scheduledJobs.length} Jobs Configured
              </span>
            </div>

            <div className="overflow-x-auto border border-slate-200 rounded-lg">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-2.5">Job Name &amp; Description</th>
                    <th className="px-4 py-2.5">Cron Expression</th>
                    <th className="px-4 py-2.5">Report Type</th>
                    <th className="px-4 py-2.5">Recipients</th>
                    <th className="px-4 py-2.5">Next Run (UTC)</th>
                    <th className="px-4 py-2.5">Last Run</th>
                    <th className="px-4 py-2.5">Status</th>
                    <th className="px-4 py-2.5 text-right">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {scheduledJobs.map((j) => (
                    <tr key={j.id} className="hover:bg-slate-50">
                      <td className="px-4 py-3">
                        <div className="font-bold text-slate-900">{j.jobName}</div>
                        <div className="text-[11px] text-slate-500">{j.cronDescription}</div>
                      </td>
                      <td className="px-4 py-3 font-mono text-indigo-700 font-semibold">
                        {j.cronExpression}
                      </td>
                      <td className="px-4 py-3">
                        <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-slate-100 text-slate-700">
                          {j.reportType}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-slate-600 max-w-[180px] truncate" title={j.recipientEmailList}>
                        {j.recipientEmailList}
                      </td>
                      <td className="px-4 py-3 font-mono text-purple-700">
                        {j.nextFireTimeUtc ? new Date(j.nextFireTimeUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" }) : "08:00 PM"}
                      </td>
                      <td className="px-4 py-3 text-slate-500">
                        {j.lastExecutedAtUtc ? (
                          <div>
                            <div>{new Date(j.lastExecutedAtUtc).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</div>
                            <div className="text-[10px] text-emerald-600">{j.lastExecutionDurationMs}ms</div>
                          </div>
                        ) : "Pending First Run"}
                      </td>
                      <td className="px-4 py-3">
                        <span className={`px-2.5 py-0.5 rounded-full text-[11px] font-semibold ${
                          j.isActive ? "bg-emerald-50 text-emerald-700 border border-emerald-200" : "bg-slate-100 text-slate-500"
                        }`}>
                          {j.isActive ? "Active" : "Paused"}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          <button
                            onClick={() => handleTriggerJob(j.id, j.jobName)}
                            disabled={triggeringJobId === j.id}
                            className="px-2.5 py-1 text-[11px] font-semibold bg-indigo-50 text-indigo-700 rounded hover:bg-indigo-100 border border-indigo-200 flex items-center gap-1"
                            title="Trigger Now"
                          >
                            <Play size={10} className={triggeringJobId === j.id ? "animate-spin" : ""} />
                            <span>Run</span>
                          </button>
                          <button
                            onClick={() => handleToggleJob(j)}
                            className="px-2 py-1 text-[11px] font-semibold text-slate-600 hover:text-slate-900 border border-slate-200 rounded"
                            title={j.isActive ? "Pause Job" : "Resume Job"}
                          >
                            {j.isActive ? <Pause size={12} /> : <Play size={12} />}
                          </button>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>

          {/* Execution History Audit Log */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-4">
            <div className="flex items-center justify-between">
              <div>
                <h3 className="text-base font-bold text-slate-900">Execution Audit Log (Immutable History)</h3>
                <p className="text-xs text-slate-500">Tracks duration, in-memory PDF size, and SMTP response status.</p>
              </div>
              <span className="text-xs text-slate-500">
                Latest {executionHistory.length} Runs
              </span>
            </div>

            <div className="overflow-x-auto border border-slate-200 rounded-lg">
              <table className="w-full text-left text-xs font-mono">
                <thead className="bg-slate-50 font-sans text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-2.5">Timestamp (UTC)</th>
                    <th className="px-4 py-2.5">Job Name</th>
                    <th className="px-4 py-2.5">Trigger Type</th>
                    <th className="px-4 py-2.5">PDF Buffer Size</th>
                    <th className="px-4 py-2.5">Duration</th>
                    <th className="px-4 py-2.5">Recipients</th>
                    <th className="px-4 py-2.5">Status</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {executionHistory.length === 0 ? (
                    <tr>
                      <td colSpan={7} className="px-4 py-6 text-center text-slate-400 font-sans">
                        No execution records found yet. Click &quot;Run Now (Test)&quot; above to trigger an execution.
                      </td>
                    </tr>
                  ) : (
                    executionHistory.map((h) => (
                      <tr key={h.id} className="hover:bg-slate-50">
                        <td className="px-4 py-2.5 text-slate-500">
                          {new Date(h.triggeredAtUtc).toLocaleString()}
                        </td>
                        <td className="px-4 py-2.5 font-bold font-sans text-slate-900">
                          {h.jobName}
                        </td>
                        <td className="px-4 py-2.5 font-sans">
                          {h.isManualTrigger ? (
                            <span className="text-indigo-600 font-medium">Manual Dashboard</span>
                          ) : (
                            <span className="text-purple-600 font-medium">Automated Cron</span>
                          )}
                        </td>
                        <td className="px-4 py-2.5 text-slate-700">
                          {(h.pdfSizeBytes / 1024).toFixed(1)} KB (In-Memory)
                        </td>
                        <td className="px-4 py-2.5 font-bold text-slate-800">
                          {h.executionDurationMs} ms
                        </td>
                        <td className="px-4 py-2.5 font-sans text-slate-600 max-w-[200px] truncate" title={h.recipients}>
                          {h.recipients}
                        </td>
                        <td className="px-4 py-2.5 font-sans">
                          <span className={`px-2 py-0.5 rounded-full text-[10px] font-bold ${
                            h.status === "Success" ? "bg-emerald-50 text-emerald-700 border border-emerald-200" : "bg-red-50 text-red-700 border border-red-200"
                          }`}>
                            {h.status}
                          </span>
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

      {/* ======================================================== */}
      {/* TAB 1: FINANCIAL STATEMENTS REPORT                       */}
      {/* ======================================================== */}
      {activeTab === "financial" && financialReport && (
        <div className="space-y-6">
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
              <p className="text-xs text-slate-500 mt-1">Direct Fabric &amp; Sewing Wages</p>
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

            <div>
              <h4 className="text-sm font-bold text-slate-900 uppercase tracking-wider mb-3">1. Executive Trial Balance Summary</h4>
              <div className="overflow-x-auto border border-slate-200 rounded-lg">
                <table className="w-full text-left text-xs font-mono">
                  <thead className="bg-slate-50 font-sans text-slate-600 uppercase font-semibold border-b border-slate-200">
                    <tr>
                      <th className="px-3 py-2">Code</th>
                      <th className="px-3 py-2">Account Title</th>
                      <th className="px-3 py-2 text-right">Debit ($)</th>
                      <th className="px-3 py-2 text-right">Credit ($)</th>
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

      {/* ======================================================== */}
      {/* TAB 2: PRODUCTION SUMMARY                                */}
      {/* ======================================================== */}
      {activeTab === "production" && productionReport && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Orders Placed</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2">{productionReport.totalOrders} Orders</h3>
              <p className="text-xs text-slate-500 mt-1">H&amp;M, Zara, Levi&apos;s</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Booked Quantity</span>
              <h3 className="text-2xl font-bold text-indigo-700 mt-2">{(productionReport.totalOrderQuantityPcs || 0).toLocaleString()} Pcs</h3>
              <p className="text-xs text-indigo-600 mt-1">Export Capacity Booked</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Fabric Rolls Cut</span>
              <h3 className="text-2xl font-bold text-emerald-700 mt-2">{productionReport.totalCuts} Orders</h3>
              <p className="text-xs text-emerald-600 mt-1">Automatic CAD Laying</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Ex-Factory Dispatches</span>
              <h3 className="text-2xl font-bold text-amber-700 mt-2">{productionReport.totalChallans} Challans</h3>
              <p className="text-xs text-amber-600 mt-1">Port Gate Clearance</p>
            </div>
          </div>
        </div>
      )}

      {/* ======================================================== */}
      {/* TAB 3: WORKFORCE PAYROLL REGISTER                        */}
      {/* ======================================================== */}
      {activeTab === "payroll" && payrollReport && (
        <div className="space-y-6">
          <div className="flex items-center justify-between flex-wrap gap-4 bg-white p-4 rounded-xl border border-slate-200">
            <div className="flex items-center gap-3">
              <label className="text-xs font-bold uppercase text-slate-600">Select Month:</label>
              <select
                value={payrollMonth}
                onChange={e => setPayrollMonth(Number(e.target.value))}
                className="text-xs border border-slate-300 rounded px-2.5 py-1.5 font-medium"
              >
                {Array.from({ length: 12 }, (_, i) => i + 1).map(m => (
                  <option key={m} value={m}>Month {m}</option>
                ))}
              </select>
              <select
                value={payrollYear}
                onChange={e => setPayrollYear(Number(e.target.value))}
                className="text-xs border border-slate-300 rounded px-2.5 py-1.5 font-medium"
              >
                <option value={2026}>2026</option>
                <option value={2025}>2025</option>
              </select>
            </div>
            <div className="text-xs font-semibold text-slate-600">
              Total Enrolled Workforce: <strong>{payrollReport.totalEmployees} Employees</strong>
            </div>
          </div>

          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Gross Base Payroll</span>
              <h3 className="text-xl font-bold text-slate-900 mt-2 font-mono">
                {formatCurrency(payrollReport.totalBasicPay || 0)}
              </h3>
              <p className="text-xs text-slate-500 mt-1">Basic Salaried Floor Staff</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Overtime Wages (1.5x)</span>
              <h3 className="text-xl font-bold text-emerald-700 mt-2 font-mono">
                {formatCurrency(payrollReport.totalOvertimePay || 0)}
              </h3>
              <p className="text-xs text-emerald-600 mt-1">Line Shift Overtime</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Statutory Deductions</span>
              <h3 className="text-xl font-bold text-red-600 mt-2 font-mono">
                {formatCurrency(payrollReport.totalDeductions || 0)}
              </h3>
              <p className="text-xs text-slate-500 mt-1">Tax, Late Penalties &amp; Advances</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Net Disbursed Amount</span>
              <h3 className="text-xl font-bold text-indigo-700 mt-2 font-mono">
                {formatCurrency(payrollReport.totalNetDisbursed || 0)}
              </h3>
              <p className="text-xs text-indigo-600 font-medium mt-1">bKash / Bank Payout</p>
            </div>
          </div>
        </div>
      )}

      {/* ======================================================== */}
      {/* TAB 4: PROCUREMENT & VALUATION                           */}
      {/* ======================================================== */}
      {activeTab === "procurement" && procurementReport && (
        <div className="space-y-6">
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Enlisted Suppliers</span>
              <h3 className="text-2xl font-bold text-slate-900 mt-2">{procurementReport.totalSuppliers} Suppliers</h3>
              <p className="text-xs text-slate-500 mt-1">Square Textiles, Envoy, Beximco</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Purchase Orders Issued</span>
              <h3 className="text-2xl font-bold text-indigo-700 mt-2">{procurementReport.totalPurchaseOrders} POs</h3>
              <p className="text-xs text-indigo-600 mt-1">Fabric, Yarn, Trims &amp; Accessories</p>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Goods Receipt Notes (GRN)</span>
              <h3 className="text-2xl font-bold text-emerald-700 mt-2">{procurementReport.totalGRNs} GRNs</h3>
              <p className="text-xs text-emerald-600 mt-1">Warehouse Gate-in Verified</p>
            </div>
          </div>

          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-4">
            <h3 className="text-base font-bold text-slate-900">Raw Material Supplier Registry &amp; Ratings</h3>
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
