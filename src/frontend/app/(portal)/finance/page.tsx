"use client";

import React, { useState, useEffect } from "react";
import { 
  DollarSign, Landmark, BookOpen, FileSpreadsheet, Plus, Search, 
  Filter, CheckCircle2, AlertCircle, RefreshCw, ArrowUpRight, 
  ArrowDownRight, Scale, TrendingUp, Receipt, FileText, Check, 
  X, Trash2, Calendar, Eye, Download, Printer
} from "lucide-react";
import { apiRequest } from "@/lib/api";

interface AccountDto {
  id: string;
  accountCode: string;
  accountName: string;
  category: number;
  categoryName: string;
  currency: string;
  openingBalance: number;
  currentBalance: number;
  isActive: boolean;
  description?: string;
}

interface JournalEntryLineDto {
  id: string;
  journalVoucherId: string;
  accountId: string;
  accountCode: string;
  accountName: string;
  accountCategory: number;
  debitAmount: number;
  creditAmount: number;
  lineNarration?: string;
}

interface JournalVoucherDto {
  id: string;
  voucherNumber: string;
  voucherDate: string;
  type: number;
  typeName: string;
  referenceType?: string;
  referenceNumber?: string;
  narration: string;
  totalDebit: number;
  totalCredit: number;
  status: number;
  statusName: string;
  postedAt?: string;
  postedBy?: string;
  lines: JournalEntryLineDto[];
}

interface FinanceMetricsDto {
  cashAndBankBalance: number;
  totalAccountsReceivable: number;
  totalAccountsPayable: number;
  totalRevenueYtd: number;
  totalExpensesYtd: number;
  netProfitYtd: number;
  totalVouchersCount: number;
  pendingDraftVouchersCount: number;
  activeAccountsCount: number;
}

interface TrialBalanceItemDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  category: number;
  debitAmount: number;
  creditAmount: number;
  netBalance: number;
}

interface TrialBalanceDto {
  asOfDate: string;
  items: TrialBalanceItemDto[];
  totalDebit: number;
  totalCredit: number;
  isBalanced: boolean;
}

interface IncomeStatementItemDto {
  accountCode: string;
  accountName: string;
  amount: number;
}

interface IncomeStatementDto {
  fromDate: string;
  toDate: string;
  revenues: IncomeStatementItemDto[];
  totalRevenue: number;
  costOfGoodsSold: IncomeStatementItemDto[];
  totalCOGS: number;
  grossProfit: number;
  operatingExpenses: IncomeStatementItemDto[];
  totalOperatingExpenses: number;
  netProfit: number;
}

interface BalanceSheetItemDto {
  accountCode: string;
  accountName: string;
  amount: number;
}

interface BalanceSheetDto {
  asOfDate: string;
  currentAssets: BalanceSheetItemDto[];
  totalCurrentAssets: number;
  nonCurrentAssets: BalanceSheetItemDto[];
  totalNonCurrentAssets: number;
  totalAssets: number;
  currentLiabilities: BalanceSheetItemDto[];
  totalCurrentLiabilities: number;
  nonCurrentLiabilities: BalanceSheetItemDto[];
  totalNonCurrentLiabilities: number;
  totalLiabilities: number;
  equityItems: BalanceSheetItemDto[];
  currentPeriodProfit: number;
  totalEquity: number;
  totalLiabilitiesAndEquity: number;
  isBalanced: boolean;
}

interface GeneralLedgerEntryDto {
  voucherId: string;
  voucherNumber: string;
  voucherDate: string;
  voucherType: number;
  referenceNumber?: string;
  narration: string;
  lineNarration?: string;
  debit: number;
  credit: number;
  runningBalance: number;
}

interface GeneralLedgerAccountDto {
  accountId: string;
  accountCode: string;
  accountName: string;
  category: number;
  openingBalance: number;
  closingBalance: number;
  entries: GeneralLedgerEntryDto[];
}

export default function FinancePage() {
  const [activeTab, setActiveTab] = useState<"overview" | "coa" | "vouchers" | "ledger" | "statements">("overview");
  const [statementSubTab, setStatementSubTab] = useState<"trial-balance" | "income-statement" | "balance-sheet">("trial-balance");
  const [loading, setLoading] = useState(false);
  const [feedback, setFeedback] = useState<{ type: "success" | "error"; message: string } | null>(null);

  // Core Data
  const [metrics, setMetrics] = useState<FinanceMetricsDto | null>(null);
  const [accounts, setAccounts] = useState<AccountDto[]>([]);
  const [vouchers, setVouchers] = useState<JournalVoucherDto[]>([]);
  const [trialBalance, setTrialBalance] = useState<TrialBalanceDto | null>(null);
  const [incomeStatement, setIncomeStatement] = useState<IncomeStatementDto | null>(null);
  const [balanceSheet, setBalanceSheet] = useState<BalanceSheetDto | null>(null);
  const [ledgerAccounts, setLedgerAccounts] = useState<GeneralLedgerAccountDto[]>([]);

  // Filters & Selected states
  const [coaCategoryFilter, setCoaCategoryFilter] = useState<string>("ALL");
  const [coaSearch, setCoaSearch] = useState("");
  const [voucherStatusFilter, setVoucherStatusFilter] = useState<string>("ALL");
  const [voucherSearch, setVoucherSearch] = useState("");
  const [selectedLedgerAccountId, setSelectedLedgerAccountId] = useState<string>("ALL");

  // Modals
  const [showNewAccountModal, setShowNewAccountModal] = useState(false);
  const [showNewVoucherModal, setShowNewVoucherModal] = useState(false);
  const [selectedVoucherForView, setSelectedVoucherForView] = useState<JournalVoucherDto | null>(null);

  // New Account Form
  const [newAccForm, setNewAccForm] = useState({
    accountCode: "",
    accountName: "",
    category: 1, // 1: Asset, 2: Liability, 3: Equity, 4: Revenue, 5: Expense
    currency: "BDT",
    openingBalance: 0,
    description: ""
  });

  // New Voucher Form
  interface NewLineForm {
    accountId: string;
    debitAmount: number;
    creditAmount: number;
    lineNarration: string;
  }
  const [newVoucherForm, setNewVoucherForm] = useState({
    voucherDate: new Date().toISOString().split("T")[0],
    type: 1, // 1: Journal, 2: Payment, 3: Receipt, 4: Contra
    referenceType: "DISPATCH",
    referenceNumber: "",
    narration: "",
    lines: [
      { accountId: "", debitAmount: 0, creditAmount: 0, lineNarration: "" },
      { accountId: "", debitAmount: 0, creditAmount: 0, lineNarration: "" }
    ] as NewLineForm[]
  });

  // Fetch data
  const fetchData = async () => {
    setLoading(true);
    try {
      const [mRes, accRes, vRes] = await Promise.all([
        apiRequest<FinanceMetricsDto>("/api/finance/metrics"),
        apiRequest<AccountDto[]>("/api/finance/accounts"),
        apiRequest<JournalVoucherDto[]>("/api/finance/vouchers")
      ]);

      if (mRes.success && mRes.data) setMetrics(mRes.data);
      if (accRes.success && accRes.data) setAccounts(accRes.data);
      if (vRes.success && vRes.data) setVouchers(vRes.data);

      // Fetch Statements
      const [tbRes, isRes, bsRes, glRes] = await Promise.all([
        apiRequest<TrialBalanceDto>("/api/finance/trial-balance"),
        apiRequest<IncomeStatementDto>("/api/finance/income-statement"),
        apiRequest<BalanceSheetDto>("/api/finance/balance-sheet"),
        apiRequest<{ accounts: GeneralLedgerAccountDto[] }>("/api/finance/ledger")
      ]);

      if (tbRes.success && tbRes.data) setTrialBalance(tbRes.data);
      if (isRes.success && isRes.data) setIncomeStatement(isRes.data);
      if (bsRes.success && bsRes.data) setBalanceSheet(bsRes.data);
      if (glRes.success && glRes.data) setLedgerAccounts(glRes.data.accounts || []);
    } catch (err: any) {
      setFeedback({ type: "error", message: err.message || "Failed to load financial records" });
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchData();
  }, []);

  // Category Color Badge Helpers
  const getCategoryBadge = (category: number | string) => {
    const cat = String(category).toLowerCase();
    if (cat === "1" || cat === "asset") {
      return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-blue-50 text-blue-700 border border-blue-200">Asset</span>;
    }
    if (cat === "2" || cat === "liability") {
      return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200">Liability</span>;
    }
    if (cat === "3" || cat === "equity") {
      return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-purple-50 text-purple-700 border border-purple-200">Equity</span>;
    }
    if (cat === "4" || cat === "revenue") {
      return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">Revenue</span>;
    }
    return <span className="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200">Expense</span>;
  };

  const getStatusBadge = (statusName: string) => {
    const s = statusName.toLowerCase();
    if (s === "posted") {
      return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200"><CheckCircle2 className="w-3 h-3" /> Posted</span>;
    }
    if (s === "cancelled") {
      return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-rose-50 text-rose-700 border border-rose-200"><X className="w-3 h-3" /> Cancelled</span>;
    }
    return <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-semibold bg-amber-50 text-amber-700 border border-amber-200"><AlertCircle className="w-3 h-3" /> Draft</span>;
  };

  const formatCurrency = (val: number) => {
    return `৳${(val || 0).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 })}`;
  };

  // Submit New Account
  const handleCreateAccount = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newAccForm.accountCode || !newAccForm.accountName) {
      setFeedback({ type: "error", message: "Account Code and Account Name are required." });
      return;
    }

    try {
      const res = await apiRequest("/api/finance/accounts", {
        method: "POST",
        body: newAccForm
      });

      if (res.success) {
        setFeedback({ type: "success", message: `Account ${newAccForm.accountCode} created successfully.` });
        setShowNewAccountModal(false);
        setNewAccForm({ accountCode: "", accountName: "", category: 1, currency: "BDT", openingBalance: 0, description: "" });
        fetchData();
      } else {
        setFeedback({ type: "error", message: res.error || "Failed to create account" });
      }
    } catch (err: any) {
      setFeedback({ type: "error", message: err.message || "Request failed" });
    }
  };

  // Submit New Journal Voucher
  const handleCreateVoucher = async (e: React.FormEvent) => {
    e.preventDefault();
    const totalDebit = newVoucherForm.lines.reduce((sum, l) => sum + Number(l.debitAmount || 0), 0);
    const totalCredit = newVoucherForm.lines.reduce((sum, l) => sum + Number(l.creditAmount || 0), 0);

    if (newVoucherForm.lines.some(l => !l.accountId)) {
      setFeedback({ type: "error", message: "Please select an account for every entry line." });
      return;
    }

    if (Math.abs(totalDebit - totalCredit) > 0.01 || totalDebit <= 0) {
      setFeedback({ type: "error", message: `Voucher is unbalanced! Total Debit (${formatCurrency(totalDebit)}) must equal Total Credit (${formatCurrency(totalCredit)}).` });
      return;
    }

    try {
      const payload = {
        voucherDate: new Date(newVoucherForm.voucherDate).toISOString(),
        type: Number(newVoucherForm.type),
        referenceType: newVoucherForm.referenceType,
        referenceNumber: newVoucherForm.referenceNumber,
        narration: newVoucherForm.narration,
        lines: newVoucherForm.lines.map(l => ({
          accountId: l.accountId,
          debitAmount: Number(l.debitAmount || 0),
          creditAmount: Number(l.creditAmount || 0),
          lineNarration: l.lineNarration || newVoucherForm.narration
        }))
      };

      const res = await apiRequest("/api/finance/vouchers", {
        method: "POST",
        body: payload
      });

      if (res.success) {
        setFeedback({ type: "success", message: "Journal voucher draft created successfully." });
        setShowNewVoucherModal(false);
        setNewVoucherForm({
          voucherDate: new Date().toISOString().split("T")[0],
          type: 1,
          referenceType: "DISPATCH",
          referenceNumber: "",
          narration: "",
          lines: [
            { accountId: "", debitAmount: 0, creditAmount: 0, lineNarration: "" },
            { accountId: "", debitAmount: 0, creditAmount: 0, lineNarration: "" }
          ]
        });
        fetchData();
      } else {
        setFeedback({ type: "error", message: res.error || "Failed to create voucher" });
      }
    } catch (err: any) {
      setFeedback({ type: "error", message: err.message || "Request failed" });
    }
  };

  // Post Voucher
  const handlePostVoucher = async (voucherId: string) => {
    if (!confirm("Are you sure you want to post this journal voucher? Posted vouchers immediately update the general ledger and trial balance.")) {
      return;
    }

    try {
      const res = await apiRequest(`/api/finance/vouchers/${voucherId}/post`, {
        method: "POST"
      });

      if (res.success) {
        setFeedback({ type: "success", message: "Journal voucher posted to General Ledger successfully." });
        fetchData();
      } else {
        setFeedback({ type: "error", message: res.error || "Failed to post voucher" });
      }
    } catch (err: any) {
      setFeedback({ type: "error", message: err.message || "Request failed" });
    }
  };

  // Filtered Accounts
  const filteredAccounts = accounts.filter(a => {
    const matchesCat = coaCategoryFilter === "ALL" || a.categoryName.toUpperCase() === coaCategoryFilter;
    const matchesSearch = !coaSearch || a.accountCode.toLowerCase().includes(coaSearch.toLowerCase()) || a.accountName.toLowerCase().includes(coaSearch.toLowerCase());
    return matchesCat && matchesSearch;
  });

  // Filtered Vouchers
  const filteredVouchers = vouchers.filter(v => {
    const matchesStatus = voucherStatusFilter === "ALL" || v.statusName.toUpperCase() === voucherStatusFilter;
    const matchesSearch = !voucherSearch || v.voucherNumber.toLowerCase().includes(voucherSearch.toLowerCase()) || v.narration.toLowerCase().includes(voucherSearch.toLowerCase()) || (v.referenceNumber && v.referenceNumber.toLowerCase().includes(voucherSearch.toLowerCase()));
    return matchesStatus && matchesSearch;
  });

  // Dynamic voucher line calculations
  const voucherLinesTotalDebit = newVoucherForm.lines.reduce((s, l) => s + (Number(l.debitAmount) || 0), 0);
  const voucherLinesTotalCredit = newVoucherForm.lines.reduce((s, l) => s + (Number(l.creditAmount) || 0), 0);
  const isVoucherBalanced = Math.abs(voucherLinesTotalDebit - voucherLinesTotalCredit) < 0.01 && voucherLinesTotalDebit > 0;

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col md:flex-row md:items-center md:justify-between gap-4">
        <div>
          <h1 className="text-2xl font-bold tracking-tight text-slate-900">Accounting & Financial Management</h1>
          <p className="text-sm text-slate-500 mt-1">
            General Ledger, Double-Entry Journal Vouchers, Chart of Accounts, and Real-Time Financial Statements.
          </p>
        </div>
        <div className="flex items-center gap-2">
          <button
            onClick={fetchData}
            disabled={loading}
            className="inline-flex items-center gap-1.5 px-3 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors shadow-sm"
          >
            <RefreshCw className={`w-3.5 h-3.5 ${loading ? "animate-spin" : ""}`} /> Refresh
          </button>
          <button
            onClick={() => setShowNewVoucherModal(true)}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
          >
            <Plus className="w-3.5 h-3.5" /> New Journal Voucher
          </button>
        </div>
      </div>

      {/* Feedback Alert */}
      {feedback && (
        <div className={`p-4 rounded-lg flex items-center justify-between text-sm ${feedback.type === "success" ? "bg-emerald-50 text-emerald-800 border border-emerald-200" : "bg-rose-50 text-rose-800 border border-rose-200"}`}>
          <div className="flex items-center gap-2">
            {feedback.type === "success" ? <CheckCircle2 className="w-4 h-4 text-emerald-600" /> : <AlertCircle className="w-4 h-4 text-rose-600" />}
            <span>{feedback.message}</span>
          </div>
          <button onClick={() => setFeedback(null)} className="text-slate-400 hover:text-slate-600">
            <X className="w-4 h-4" />
          </button>
        </div>
      )}

      {/* Primary Navigation Tabs */}
      <div className="border-b border-slate-200">
        <nav className="flex space-x-6 overflow-x-auto" aria-label="Tabs">
          {[
            { id: "overview", label: "Financial Overview", icon: TrendingUp },
            { id: "coa", label: "Chart of Accounts", icon: BookOpen },
            { id: "vouchers", label: "Journal Vouchers", icon: Receipt },
            { id: "ledger", label: "General Ledger", icon: FileText },
            { id: "statements", label: "Financial Statements", icon: Scale }
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

      {/* TAB 1: OVERVIEW */}
      {activeTab === "overview" && (
        <div className="space-y-6">
          {/* Top KPI Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Cash & Bank Position</span>
                <span className="p-2 bg-blue-50 text-blue-600 rounded-lg"><Landmark className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-slate-900">{formatCurrency(metrics?.cashAndBankBalance || 0)}</h3>
                <p className="text-xs text-slate-500 mt-1">SCB Principal A/C + Factory Petty Cash</p>
              </div>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Accounts Receivable (AR)</span>
                <span className="p-2 bg-emerald-50 text-emerald-600 rounded-lg"><ArrowUpRight className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-slate-900">{formatCurrency(metrics?.totalAccountsReceivable || 0)}</h3>
                <p className="text-xs text-emerald-600 font-medium mt-1">H&M, Zara, Target Export LCs</p>
              </div>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Accounts Payable (AP)</span>
                <span className="p-2 bg-amber-50 text-amber-600 rounded-lg"><ArrowDownRight className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-slate-900">{formatCurrency(metrics?.totalAccountsPayable || 0)}</h3>
                <p className="text-xs text-amber-600 font-medium mt-1">Yarn Suppliers + Accrued Wages</p>
              </div>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">YTD Revenue Realized</span>
                <span className="p-2 bg-purple-50 text-purple-600 rounded-lg"><DollarSign className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-slate-900">{formatCurrency(metrics?.totalRevenueYtd || 0)}</h3>
                <p className="text-xs text-slate-500 mt-1">{metrics?.totalVouchersCount || 0} Total Journal Vouchers</p>
              </div>
            </div>
          </div>

          {/* Quick Actions & Recent Vouchers */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            <div className="lg:col-span-2 bg-white rounded-xl border border-slate-200 shadow-sm p-5">
              <div className="flex items-center justify-between mb-4">
                <div>
                  <h3 className="text-base font-semibold text-slate-900">Recent Journal Vouchers</h3>
                  <p className="text-xs text-slate-500">Latest financial transaction bookings</p>
                </div>
                <button
                  onClick={() => setActiveTab("vouchers")}
                  className="text-xs font-semibold text-indigo-600 hover:text-indigo-800"
                >
                  View All &rarr;
                </button>
              </div>

              <div className="overflow-x-auto">
                <table className="w-full text-left text-xs">
                  <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                    <tr>
                      <th className="px-3 py-2.5">Voucher #</th>
                      <th className="px-3 py-2.5">Date</th>
                      <th className="px-3 py-2.5">Narration</th>
                      <th className="px-3 py-2.5 text-right">Amount (৳)</th>
                      <th className="px-3 py-2.5">Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {vouchers.slice(0, 5).map((v) => (
                      <tr key={v.id} className="hover:bg-slate-50 transition-colors">
                        <td className="px-3 py-3 font-semibold text-indigo-600">{v.voucherNumber}</td>
                        <td className="px-3 py-3 text-slate-500">{new Date(v.voucherDate).toLocaleDateString()}</td>
                        <td className="px-3 py-3 text-slate-700 max-w-xs truncate">{v.narration}</td>
                        <td className="px-3 py-3 text-right font-bold text-slate-900">{formatCurrency(v.totalDebit)}</td>
                        <td className="px-3 py-3">{getStatusBadge(v.statusName)}</td>
                      </tr>
                    ))}
                    {vouchers.length === 0 && (
                      <tr>
                        <td colSpan={5} className="px-3 py-6 text-center text-slate-400">No journal vouchers recorded yet.</td>
                      </tr>
                    )}
                  </tbody>
                </table>
              </div>
            </div>

            {/* Quick Actions Card */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-5 space-y-4">
              <h3 className="text-base font-semibold text-slate-900">Financial Quick Actions</h3>
              <div className="space-y-2.5">
                <button
                  onClick={() => setShowNewVoucherModal(true)}
                  className="w-full flex items-center justify-between p-3 rounded-lg border border-slate-200 hover:border-indigo-300 hover:bg-indigo-50/50 transition-all text-left"
                >
                  <div className="flex items-center gap-3">
                    <span className="p-2 bg-indigo-50 text-indigo-600 rounded-lg"><Plus className="w-4 h-4" /></span>
                    <div>
                      <div className="text-xs font-semibold text-slate-900">Record Journal Entry</div>
                      <div className="text-[11px] text-slate-500">Post debit/credit transaction</div>
                    </div>
                  </div>
                  <ArrowUpRight className="w-4 h-4 text-slate-400" />
                </button>

                <button
                  onClick={() => setShowNewAccountModal(true)}
                  className="w-full flex items-center justify-between p-3 rounded-lg border border-slate-200 hover:border-emerald-300 hover:bg-emerald-50/50 transition-all text-left"
                >
                  <div className="flex items-center gap-3">
                    <span className="p-2 bg-emerald-50 text-emerald-600 rounded-lg"><BookOpen className="w-4 h-4" /></span>
                    <div>
                      <div className="text-xs font-semibold text-slate-900">Add General Ledger A/C</div>
                      <div className="text-[11px] text-slate-500">Expand chart of accounts</div>
                    </div>
                  </div>
                  <ArrowUpRight className="w-4 h-4 text-slate-400" />
                </button>

                <button
                  onClick={() => { setActiveTab("statements"); setStatementSubTab("trial-balance"); }}
                  className="w-full flex items-center justify-between p-3 rounded-lg border border-slate-200 hover:border-blue-300 hover:bg-blue-50/50 transition-all text-left"
                >
                  <div className="flex items-center gap-3">
                    <span className="p-2 bg-blue-50 text-blue-600 rounded-lg"><Scale className="w-4 h-4" /></span>
                    <div>
                      <div className="text-xs font-semibold text-slate-900">Check Trial Balance</div>
                      <div className="text-[11px] text-slate-500">Audit debit/credit equality</div>
                    </div>
                  </div>
                  <ArrowUpRight className="w-4 h-4 text-slate-400" />
                </button>

                <button
                  onClick={() => { setActiveTab("statements"); setStatementSubTab("income-statement"); }}
                  className="w-full flex items-center justify-between p-3 rounded-lg border border-slate-200 hover:border-purple-300 hover:bg-purple-50/50 transition-all text-left"
                >
                  <div className="flex items-center gap-3">
                    <span className="p-2 bg-purple-50 text-purple-600 rounded-lg"><FileSpreadsheet className="w-4 h-4" /></span>
                    <div>
                      <div className="text-xs font-semibold text-slate-900">View P&L Statement</div>
                      <div className="text-[11px] text-slate-500">Gross & net profit analysis</div>
                    </div>
                  </div>
                  <ArrowUpRight className="w-4 h-4 text-slate-400" />
                </button>
              </div>
            </div>
          </div>
        </div>
      )}

      {/* TAB 2: CHART OF ACCOUNTS (COA) */}
      {activeTab === "coa" && (
        <div className="space-y-4">
          {/* Controls */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-3">
            <div className="flex flex-wrap items-center gap-2">
              {["ALL", "ASSET", "LIABILITY", "EQUITY", "REVENUE", "EXPENSE"].map((cat) => (
                <button
                  key={cat}
                  onClick={() => setCoaCategoryFilter(cat)}
                  className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                    coaCategoryFilter === cat
                      ? "bg-indigo-600 text-white shadow-sm"
                      : "bg-slate-100 text-slate-600 hover:bg-slate-200"
                  }`}
                >
                  {cat}
                </button>
              ))}
            </div>

            <div className="flex items-center gap-2">
              <div className="relative">
                <Search className="w-3.5 h-3.5 absolute left-3 top-2.5 text-slate-400" />
                <input
                  type="text"
                  placeholder="Search account..."
                  value={coaSearch}
                  onChange={(e) => setCoaSearch(e.target.value)}
                  className="pl-8 pr-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:bg-white w-56"
                />
              </div>
              <button
                onClick={() => setShowNewAccountModal(true)}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
              >
                <Plus className="w-3.5 h-3.5" /> Add Account
              </button>
            </div>
          </div>

          {/* Accounts Table */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-3">Code</th>
                    <th className="px-4 py-3">Account Title</th>
                    <th className="px-4 py-3">Category</th>
                    <th className="px-4 py-3">Currency</th>
                    <th className="px-4 py-3 text-right">Opening Balance</th>
                    <th className="px-4 py-3 text-right">Current Balance</th>
                    <th className="px-4 py-3">Description</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {filteredAccounts.map((a) => (
                    <tr key={a.id} className="hover:bg-slate-50 transition-colors">
                      <td className="px-4 py-3 font-mono font-bold text-indigo-700">{a.accountCode}</td>
                      <td className="px-4 py-3 font-semibold text-slate-900">{a.accountName}</td>
                      <td className="px-4 py-3">{getCategoryBadge(a.categoryName)}</td>
                      <td className="px-4 py-3 text-slate-500 font-mono">{a.currency}</td>
                      <td className="px-4 py-3 text-right text-slate-600 font-mono">{formatCurrency(a.openingBalance)}</td>
                      <td className="px-4 py-3 text-right font-bold text-slate-900 font-mono">{formatCurrency(a.currentBalance)}</td>
                      <td className="px-4 py-3 text-slate-500 truncate max-w-xs">{a.description || "-"}</td>
                    </tr>
                  ))}
                  {filteredAccounts.length === 0 && (
                    <tr>
                      <td colSpan={7} className="px-4 py-8 text-center text-slate-400">No accounts matching criteria.</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 3: JOURNAL VOUCHERS */}
      {activeTab === "vouchers" && (
        <div className="space-y-4">
          {/* Controls */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-3">
            <div className="flex items-center gap-2">
              {["ALL", "DRAFT", "POSTED", "CANCELLED"].map((st) => (
                <button
                  key={st}
                  onClick={() => setVoucherStatusFilter(st)}
                  className={`px-3 py-1.5 rounded-lg text-xs font-semibold transition-all ${
                    voucherStatusFilter === st
                      ? "bg-indigo-600 text-white shadow-sm"
                      : "bg-slate-100 text-slate-600 hover:bg-slate-200"
                  }`}
                >
                  {st}
                </button>
              ))}
            </div>

            <div className="flex items-center gap-2">
              <div className="relative">
                <Search className="w-3.5 h-3.5 absolute left-3 top-2.5 text-slate-400" />
                <input
                  type="text"
                  placeholder="Search voucher # or narration..."
                  value={voucherSearch}
                  onChange={(e) => setVoucherSearch(e.target.value)}
                  className="pl-8 pr-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:bg-white w-64"
                />
              </div>
              <button
                onClick={() => setShowNewVoucherModal(true)}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 transition-colors shadow-sm"
              >
                <Plus className="w-3.5 h-3.5" /> New Voucher
              </button>
            </div>
          </div>

          {/* Vouchers Table */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-4 py-3">Voucher Number</th>
                    <th className="px-4 py-3">Date</th>
                    <th className="px-4 py-3">Type</th>
                    <th className="px-4 py-3">Reference</th>
                    <th className="px-4 py-3">Narration</th>
                    <th className="px-4 py-3 text-right">Debit (৳)</th>
                    <th className="px-4 py-3 text-right">Credit (৳)</th>
                    <th className="px-4 py-3">Status</th>
                    <th className="px-4 py-3 text-center">Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {filteredVouchers.map((v) => (
                    <tr key={v.id} className="hover:bg-slate-50 transition-colors">
                      <td className="px-4 py-3 font-mono font-bold text-indigo-700">{v.voucherNumber}</td>
                      <td className="px-4 py-3 text-slate-500">{new Date(v.voucherDate).toLocaleDateString()}</td>
                      <td className="px-4 py-3">
                        <span className="px-2 py-0.5 rounded text-[11px] font-semibold bg-slate-100 text-slate-700">{v.typeName}</span>
                      </td>
                      <td className="px-4 py-3 text-slate-600 font-mono">
                        {v.referenceNumber ? `${v.referenceType || ""}: ${v.referenceNumber}` : "-"}
                      </td>
                      <td className="px-4 py-3 text-slate-800 max-w-xs truncate">{v.narration}</td>
                      <td className="px-4 py-3 text-right font-bold text-slate-900 font-mono">{formatCurrency(v.totalDebit)}</td>
                      <td className="px-4 py-3 text-right font-bold text-slate-900 font-mono">{formatCurrency(v.totalCredit)}</td>
                      <td className="px-4 py-3">{getStatusBadge(v.statusName)}</td>
                      <td className="px-4 py-3 text-center">
                        <div className="flex items-center justify-center gap-1.5">
                          <button
                            onClick={() => setSelectedVoucherForView(v)}
                            title="View Voucher Details"
                            className="p-1 rounded text-slate-500 hover:text-indigo-600 hover:bg-slate-100"
                          >
                            <Eye className="w-3.5 h-3.5" />
                          </button>
                          {v.statusName.toLowerCase() === "draft" && (
                            <button
                              onClick={() => handlePostVoucher(v.id)}
                              title="Post to General Ledger"
                              className="px-2 py-1 rounded text-[11px] font-bold text-white bg-emerald-600 hover:bg-emerald-700"
                            >
                              Post
                            </button>
                          )}
                        </div>
                      </td>
                    </tr>
                  ))}
                  {filteredVouchers.length === 0 && (
                    <tr>
                      <td colSpan={9} className="px-4 py-8 text-center text-slate-400">No journal vouchers found.</td>
                    </tr>
                  )}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB 4: GENERAL LEDGER */}
      {activeTab === "ledger" && (
        <div className="space-y-4">
          {/* Account selector */}
          <div className="bg-white p-4 rounded-xl border border-slate-200 shadow-sm flex flex-col md:flex-row md:items-center md:justify-between gap-3">
            <div className="flex items-center gap-3">
              <label className="text-xs font-semibold text-slate-700">Filter By Account:</label>
              <select
                value={selectedLedgerAccountId}
                onChange={(e) => setSelectedLedgerAccountId(e.target.value)}
                className="px-3 py-1.5 text-xs bg-slate-50 border border-slate-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 focus:bg-white"
              >
                <option value="ALL">All General Ledger Accounts</option>
                {accounts.map((a) => (
                  <option key={a.id} value={a.id}>
                    {a.accountCode} - {a.accountName} ({a.categoryName})
                  </option>
                ))}
              </select>
            </div>
            <div className="text-xs text-slate-500 font-medium">
              Showing posted double-entry journal transactions with running balances
            </div>
          </div>

          {/* Ledger Accounts Feed */}
          <div className="space-y-6">
            {ledgerAccounts
              .filter(la => selectedLedgerAccountId === "ALL" || la.accountId === selectedLedgerAccountId)
              .map((la) => (
                <div key={la.accountId} className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
                  <div className="bg-slate-50/80 px-4 py-3 border-b border-slate-200 flex items-center justify-between">
                    <div className="flex items-center gap-3">
                      <span className="font-mono font-bold text-sm text-indigo-700">{la.accountCode}</span>
                      <span className="font-semibold text-slate-900 text-sm">{la.accountName}</span>
                      {getCategoryBadge(la.category)}
                    </div>
                    <div className="flex items-center gap-4 text-xs">
                      <div>
                        <span className="text-slate-500 mr-1">Opening:</span>
                        <span className="font-mono font-semibold text-slate-700">{formatCurrency(la.openingBalance)}</span>
                      </div>
                      <div>
                        <span className="text-slate-500 mr-1">Closing Balance:</span>
                        <span className="font-mono font-bold text-indigo-900">{formatCurrency(la.closingBalance)}</span>
                      </div>
                    </div>
                  </div>

                  <div className="overflow-x-auto">
                    <table className="w-full text-left text-xs">
                      <thead className="text-slate-500 bg-slate-50/40 uppercase font-medium border-b border-slate-200">
                        <tr>
                          <th className="px-4 py-2.5">Date</th>
                          <th className="px-4 py-2.5">Voucher #</th>
                          <th className="px-4 py-2.5">Reference</th>
                          <th className="px-4 py-2.5">Narration</th>
                          <th className="px-4 py-2.5 text-right">Debit (৳)</th>
                          <th className="px-4 py-2.5 text-right">Credit (৳)</th>
                          <th className="px-4 py-2.5 text-right font-semibold text-slate-700">Running Balance (৳)</th>
                        </tr>
                      </thead>
                      <tbody className="divide-y divide-slate-100 font-mono">
                        {la.entries.map((entry, idx) => (
                          <tr key={idx} className="hover:bg-slate-50/60 transition-colors">
                            <td className="px-4 py-2 text-slate-500 font-sans">{new Date(entry.voucherDate).toLocaleDateString()}</td>
                            <td className="px-4 py-2 text-indigo-600 font-bold">{entry.voucherNumber}</td>
                            <td className="px-4 py-2 text-slate-600">{entry.referenceNumber || "-"}</td>
                            <td className="px-4 py-2 text-slate-800 font-sans">{entry.lineNarration || entry.narration}</td>
                            <td className="px-4 py-2 text-right font-medium text-slate-900">{entry.debit > 0 ? formatCurrency(entry.debit) : "-"}</td>
                            <td className="px-4 py-2 text-right font-medium text-slate-900">{entry.credit > 0 ? formatCurrency(entry.credit) : "-"}</td>
                            <td className="px-4 py-2 text-right font-bold text-indigo-700">{formatCurrency(entry.runningBalance)}</td>
                          </tr>
                        ))}
                        {la.entries.length === 0 && (
                          <tr>
                            <td colSpan={7} className="px-4 py-4 text-center text-slate-400 font-sans">
                              No posted journal entries in this period. Opening balance: {formatCurrency(la.openingBalance)}
                            </td>
                          </tr>
                        )}
                      </tbody>
                    </table>
                  </div>
                </div>
              ))}
          </div>
        </div>
      )}

      {/* TAB 5: FINANCIAL STATEMENTS */}
      {activeTab === "statements" && (
        <div className="space-y-6">
          {/* Sub Tab Navigation */}
          <div className="flex items-center justify-between bg-white p-3 rounded-xl border border-slate-200 shadow-sm">
            <div className="flex items-center gap-2">
              {[
                { id: "trial-balance", label: "Trial Balance", icon: Scale },
                { id: "income-statement", label: "Profit & Loss (P&L)", icon: TrendingUp },
                { id: "balance-sheet", label: "Balance Sheet", icon: Landmark }
              ].map((st) => {
                const Icon = st.icon;
                const isSelected = statementSubTab === st.id;
                return (
                  <button
                    key={st.id}
                    onClick={() => setStatementSubTab(st.id as any)}
                    className={`inline-flex items-center gap-1.5 px-3 py-2 rounded-lg text-xs font-semibold transition-all ${
                      isSelected
                        ? "bg-indigo-600 text-white shadow-sm"
                        : "bg-slate-100 text-slate-600 hover:bg-slate-200"
                    }`}
                  >
                    <Icon className="w-3.5 h-3.5" />
                    {st.label}
                  </button>
                );
              })}
            </div>

            <div className="text-xs text-slate-500 font-medium">
              As of: {new Date().toLocaleDateString("en-US", { year: "numeric", month: "long", day: "numeric" })}
            </div>
          </div>

          {/* 1. Trial Balance */}
          {statementSubTab === "trial-balance" && (
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden p-6 space-y-4">
              <div className="flex items-center justify-between border-b border-slate-100 pb-4">
                <div>
                  <h3 className="text-lg font-bold text-slate-900">Trial Balance Statement</h3>
                  <p className="text-xs text-slate-500">Summary of all debit and credit account balances verifying double-entry ledger equality.</p>
                </div>
                {trialBalance?.isBalanced ? (
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                    <CheckCircle2 className="w-4 h-4" /> Trial Balance Balanced (Debit == Credit)
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-rose-50 text-rose-700 border border-rose-200">
                    <AlertCircle className="w-4 h-4" /> Unbalanced Discrepancy Detected
                  </span>
                )}
              </div>

              <div className="overflow-x-auto">
                <table className="w-full text-left text-xs font-mono">
                  <thead className="bg-slate-50 text-slate-600 uppercase font-semibold border-b border-slate-200 font-sans">
                    <tr>
                      <th className="px-4 py-3">Account Code</th>
                      <th className="px-4 py-3">Account Title</th>
                      <th className="px-4 py-3">Category</th>
                      <th className="px-4 py-3 text-right">Debit Balance (৳)</th>
                      <th className="px-4 py-3 text-right">Credit Balance (৳)</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100">
                    {trialBalance?.items.map((item) => (
                      <tr key={item.accountId} className="hover:bg-slate-50 transition-colors">
                        <td className="px-4 py-2.5 font-bold text-indigo-700">{item.accountCode}</td>
                        <td className="px-4 py-2.5 font-sans font-semibold text-slate-900">{item.accountName}</td>
                        <td className="px-4 py-2.5 font-sans">{getCategoryBadge(item.category)}</td>
                        <td className="px-4 py-2.5 text-right font-medium text-slate-900">
                          {item.debitAmount > 0 ? formatCurrency(item.debitAmount) : "-"}
                        </td>
                        <td className="px-4 py-2.5 text-right font-medium text-slate-900">
                          {item.creditAmount > 0 ? formatCurrency(item.creditAmount) : "-"}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                  <tfoot className="bg-slate-100/80 font-bold text-slate-900 border-t-2 border-slate-300 text-sm">
                    <tr>
                      <td colSpan={3} className="px-4 py-3 uppercase tracking-wider font-sans">Total Balanced Sum</td>
                      <td className="px-4 py-3 text-right text-indigo-900">{formatCurrency(trialBalance?.totalDebit || 0)}</td>
                      <td className="px-4 py-3 text-right text-indigo-900">{formatCurrency(trialBalance?.totalCredit || 0)}</td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          )}

          {/* 2. Income Statement (P&L) */}
          {statementSubTab === "income-statement" && (
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-6">
              <div className="border-b border-slate-100 pb-4 flex items-center justify-between">
                <div>
                  <h3 className="text-lg font-bold text-slate-900">Statement of Profit & Loss (Income Statement)</h3>
                  <p className="text-xs text-slate-500">For the period: {new Date(incomeStatement?.fromDate || "").toLocaleDateString()} to {new Date(incomeStatement?.toDate || "").toLocaleDateString()}</p>
                </div>
                <div className="text-right">
                  <div className="text-xs text-slate-500 font-semibold uppercase">Net Profit Margin</div>
                  <div className={`text-base font-bold ${(incomeStatement?.netProfit || 0) >= 0 ? "text-emerald-600" : "text-rose-600"}`}>
                    {(((incomeStatement?.netProfit || 0) / (incomeStatement?.totalRevenue || 1)) * 100).toFixed(1)}%
                  </div>
                </div>
              </div>

              {/* Revenues Section */}
              <div className="space-y-3">
                <div className="flex items-center justify-between font-bold text-sm text-slate-900 border-b border-slate-200 pb-2">
                  <span>Operating Revenues (Sales Turnover)</span>
                  <span className="font-mono text-emerald-700">{formatCurrency(incomeStatement?.totalRevenue || 0)}</span>
                </div>
                <div className="space-y-1 pl-4 text-xs font-mono">
                  {incomeStatement?.revenues.map((r, i) => (
                    <div key={i} className="flex items-center justify-between text-slate-700 py-1 border-b border-slate-50">
                      <span className="font-sans">{r.accountCode} - {r.accountName}</span>
                      <span>{formatCurrency(r.amount)}</span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Cost of Goods Sold (COGS) */}
              <div className="space-y-3">
                <div className="flex items-center justify-between font-bold text-sm text-slate-900 border-b border-slate-200 pb-2">
                  <span>Less: Cost of Goods Sold (COGS: Direct Materials & Labor)</span>
                  <span className="font-mono text-amber-700">({formatCurrency(incomeStatement?.totalCOGS || 0)})</span>
                </div>
                <div className="space-y-1 pl-4 text-xs font-mono">
                  {incomeStatement?.costOfGoodsSold.map((c, i) => (
                    <div key={i} className="flex items-center justify-between text-slate-700 py-1 border-b border-slate-50">
                      <span className="font-sans">{c.accountCode} - {c.accountName}</span>
                      <span>{formatCurrency(c.amount)}</span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Gross Profit Banner */}
              <div className="bg-indigo-50/70 p-4 rounded-xl border border-indigo-200 flex items-center justify-between">
                <div>
                  <h4 className="text-sm font-bold text-indigo-950">Gross Manufacturing Profit</h4>
                  <p className="text-xs text-indigo-700">Total Sales Turnover minus Direct Manufacturing Materials & Wages</p>
                </div>
                <div className="text-lg font-bold font-mono text-indigo-900">
                  {formatCurrency(incomeStatement?.grossProfit || 0)}
                </div>
              </div>

              {/* Operating Expenses */}
              <div className="space-y-3">
                <div className="flex items-center justify-between font-bold text-sm text-slate-900 border-b border-slate-200 pb-2">
                  <span>Less: Operating Expenses (Factory Overhead & Administration)</span>
                  <span className="font-mono text-rose-700">({formatCurrency(incomeStatement?.totalOperatingExpenses || 0)})</span>
                </div>
                <div className="space-y-1 pl-4 text-xs font-mono">
                  {incomeStatement?.operatingExpenses.map((o, i) => (
                    <div key={i} className="flex items-center justify-between text-slate-700 py-1 border-b border-slate-50">
                      <span className="font-sans">{o.accountCode} - {o.accountName}</span>
                      <span>{formatCurrency(o.amount)}</span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Net Profit Banner */}
              <div className={`p-5 rounded-xl border flex items-center justify-between ${(incomeStatement?.netProfit || 0) >= 0 ? "bg-emerald-50 border-emerald-300" : "bg-rose-50 border-rose-300"}`}>
                <div>
                  <h4 className={`text-base font-bold ${(incomeStatement?.netProfit || 0) >= 0 ? "text-emerald-950" : "text-rose-950"}`}>
                    {(incomeStatement?.netProfit || 0) >= 0 ? "Net Operating Profit" : "Net Operating Loss"}
                  </h4>
                  <p className={`text-xs ${(incomeStatement?.netProfit || 0) >= 0 ? "text-emerald-700" : "text-rose-700"}`}>
                    Final operating bottom-line transferrable to Balance Sheet Retained Earnings.
                  </p>
                </div>
                <div className={`text-2xl font-bold font-mono ${(incomeStatement?.netProfit || 0) >= 0 ? "text-emerald-900" : "text-rose-900"}`}>
                  {formatCurrency(incomeStatement?.netProfit || 0)}
                </div>
              </div>
            </div>
          )}

          {/* 3. Balance Sheet */}
          {statementSubTab === "balance-sheet" && (
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-6 space-y-6">
              <div className="flex items-center justify-between border-b border-slate-100 pb-4">
                <div>
                  <h3 className="text-lg font-bold text-slate-900">Statement of Financial Position (Balance Sheet)</h3>
                  <p className="text-xs text-slate-500">Accounting Fundamental Equation: Total Assets = Total Liabilities + Total Shareholder Equity</p>
                </div>
                {balanceSheet?.isBalanced ? (
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-emerald-50 text-emerald-700 border border-emerald-200">
                    <CheckCircle2 className="w-4 h-4" /> Balanced: Assets = Liabilities + Equity
                  </span>
                ) : (
                  <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-bold bg-rose-50 text-rose-700 border border-rose-200">
                    <AlertCircle className="w-4 h-4" /> Balance Discrepancy
                  </span>
                )}
              </div>

              <div className="grid grid-cols-1 lg:grid-cols-2 gap-8">
                {/* Left Column: ASSETS */}
                <div className="space-y-6">
                  <div className="bg-slate-50 p-3 rounded-lg border border-slate-200 font-bold text-sm text-slate-900 flex items-center justify-between">
                    <span>ASSETS</span>
                    <span className="font-mono text-indigo-700">{formatCurrency(balanceSheet?.totalAssets || 0)}</span>
                  </div>

                  {/* Current Assets */}
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-xs font-bold text-slate-700 uppercase tracking-wider border-b border-slate-200 pb-1">
                      <span>Current Assets</span>
                      <span className="font-mono">{formatCurrency(balanceSheet?.totalCurrentAssets || 0)}</span>
                    </div>
                    <div className="space-y-1 pl-2 text-xs font-mono">
                      {balanceSheet?.currentAssets.map((a, i) => (
                        <div key={i} className="flex items-center justify-between text-slate-700 py-1 border-b border-slate-50">
                          <span className="font-sans">{a.accountCode} - {a.accountName}</span>
                          <span>{formatCurrency(a.amount)}</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* Non-Current Assets */}
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-xs font-bold text-slate-700 uppercase tracking-wider border-b border-slate-200 pb-1">
                      <span>Non-Current Assets (Plant & Machinery)</span>
                      <span className="font-mono">{formatCurrency(balanceSheet?.totalNonCurrentAssets || 0)}</span>
                    </div>
                    <div className="space-y-1 pl-2 text-xs font-mono">
                      {balanceSheet?.nonCurrentAssets.map((a, i) => (
                        <div key={i} className="flex items-center justify-between text-slate-700 py-1 border-b border-slate-50">
                          <span className="font-sans">{a.accountCode} - {a.accountName}</span>
                          <span>{formatCurrency(a.amount)}</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* Total Assets Summary */}
                  <div className="bg-indigo-50 p-4 rounded-xl border border-indigo-200 flex items-center justify-between font-bold">
                    <span className="text-sm text-indigo-950">TOTAL ASSETS</span>
                    <span className="text-base font-mono text-indigo-900">{formatCurrency(balanceSheet?.totalAssets || 0)}</span>
                  </div>
                </div>

                {/* Right Column: LIABILITIES & EQUITY */}
                <div className="space-y-6">
                  <div className="bg-slate-50 p-3 rounded-lg border border-slate-200 font-bold text-sm text-slate-900 flex items-center justify-between">
                    <span>LIABILITIES & SHAREHOLDER EQUITY</span>
                    <span className="font-mono text-indigo-700">{formatCurrency(balanceSheet?.totalLiabilitiesAndEquity || 0)}</span>
                  </div>

                  {/* Current Liabilities */}
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-xs font-bold text-slate-700 uppercase tracking-wider border-b border-slate-200 pb-1">
                      <span>Current Liabilities</span>
                      <span className="font-mono">{formatCurrency(balanceSheet?.totalCurrentLiabilities || 0)}</span>
                    </div>
                    <div className="space-y-1 pl-2 text-xs font-mono">
                      {balanceSheet?.currentLiabilities.map((l, i) => (
                        <div key={i} className="flex items-center justify-between text-slate-700 py-1 border-b border-slate-50">
                          <span className="font-sans">{l.accountCode} - {l.accountName}</span>
                          <span>{formatCurrency(l.amount)}</span>
                        </div>
                      ))}
                    </div>
                  </div>

                  {/* Shareholder Equity */}
                  <div className="space-y-2">
                    <div className="flex items-center justify-between text-xs font-bold text-slate-700 uppercase tracking-wider border-b border-slate-200 pb-1">
                      <span>Shareholder Equity & Retained Profits</span>
                      <span className="font-mono">{formatCurrency(balanceSheet?.totalEquity || 0)}</span>
                    </div>
                    <div className="space-y-1 pl-2 text-xs font-mono">
                      {balanceSheet?.equityItems.map((e, i) => (
                        <div key={i} className="flex items-center justify-between text-slate-700 py-1 border-b border-slate-50">
                          <span className="font-sans">{e.accountCode} - {e.accountName}</span>
                          <span>{formatCurrency(e.amount)}</span>
                        </div>
                      ))}
                      {balanceSheet?.currentPeriodProfit !== undefined && (
                        <div className="flex items-center justify-between text-emerald-800 font-bold py-1 border-b border-slate-50">
                          <span className="font-sans">Current Period Net Operating Income</span>
                          <span>{formatCurrency(balanceSheet.currentPeriodProfit)}</span>
                        </div>
                      )}
                    </div>
                  </div>

                  {/* Total Liabilities & Equity Summary */}
                  <div className="bg-indigo-50 p-4 rounded-xl border border-indigo-200 flex items-center justify-between font-bold">
                    <span className="text-sm text-indigo-950">TOTAL LIABILITIES & EQUITY</span>
                    <span className="text-base font-mono text-indigo-900">{formatCurrency(balanceSheet?.totalLiabilitiesAndEquity || 0)}</span>
                  </div>
                </div>
              </div>
            </div>
          )}
        </div>
      )}

      {/* MODAL: NEW ACCOUNT */}
      {showNewAccountModal && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-md w-full p-6 shadow-2xl border border-slate-200 space-y-4">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <h3 className="text-base font-bold text-slate-900">Create General Ledger Account</h3>
              <button onClick={() => setShowNewAccountModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateAccount} className="space-y-3 text-xs">
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Account Code *</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. 1007, 2004, 5006"
                  value={newAccForm.accountCode}
                  onChange={(e) => setNewAccForm({ ...newAccForm, accountCode: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Account Name *</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. Foreign Currency Account (USD)"
                  value={newAccForm.accountName}
                  onChange={(e) => setNewAccForm({ ...newAccForm, accountName: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Category *</label>
                  <select
                    value={newAccForm.category}
                    onChange={(e) => setNewAccForm({ ...newAccForm, category: Number(e.target.value) })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value={1}>1 - Asset</option>
                    <option value={2}>2 - Liability</option>
                    <option value={3}>3 - Equity</option>
                    <option value={4}>4 - Revenue</option>
                    <option value={5}>5 - Expense</option>
                  </select>
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Currency</label>
                  <select
                    value={newAccForm.currency}
                    onChange={(e) => setNewAccForm({ ...newAccForm, currency: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono"
                  >
                    <option value="BDT">BDT (৳)</option>
                    <option value="USD">USD ($)</option>
                    <option value="EUR">EUR (€)</option>
                  </select>
                </div>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Opening Balance</label>
                <input
                  type="number"
                  step="0.01"
                  value={newAccForm.openingBalance}
                  onChange={(e) => setNewAccForm({ ...newAccForm, openingBalance: Number(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono"
                />
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Description</label>
                <textarea
                  rows={2}
                  placeholder="Optional account notes and purpose"
                  value={newAccForm.description}
                  onChange={(e) => setNewAccForm({ ...newAccForm, description: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowNewAccountModal(false)}
                  className="px-4 py-2 border border-slate-300 text-slate-700 rounded-lg hover:bg-slate-50 font-semibold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-semibold shadow-sm"
                >
                  Create Account
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: CREATE JOURNAL VOUCHER */}
      {showNewVoucherModal && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-3xl w-full p-6 shadow-2xl border border-slate-200 space-y-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <h3 className="text-base font-bold text-slate-900">Record Double-Entry Journal Voucher</h3>
                <p className="text-xs text-slate-500">Every transaction must satisfy Total Debit == Total Credit</p>
              </div>
              <button onClick={() => setShowNewVoucherModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateVoucher} className="space-y-4 text-xs">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Voucher Date *</label>
                  <input
                    type="date"
                    required
                    value={newVoucherForm.voucherDate}
                    onChange={(e) => setNewVoucherForm({ ...newVoucherForm, voucherDate: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  />
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Voucher Type *</label>
                  <select
                    value={newVoucherForm.type}
                    onChange={(e) => setNewVoucherForm({ ...newVoucherForm, type: Number(e.target.value) })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                  >
                    <option value={1}>Journal Voucher (JV)</option>
                    <option value={2}>Payment Voucher (PV)</option>
                    <option value={3}>Receipt Voucher (RV)</option>
                    <option value={4}>Contra Voucher (CV)</option>
                  </select>
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Reference Number</label>
                  <input
                    type="text"
                    placeholder="e.g. PO-2026-0042, DC-01"
                    value={newVoucherForm.referenceNumber}
                    onChange={(e) => setNewVoucherForm({ ...newVoucherForm, referenceNumber: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500 font-mono"
                  />
                </div>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Voucher Narration / Memo *</label>
                <input
                  type="text"
                  required
                  placeholder="e.g. Fabric Procurement settlement from Standard Chartered Bank"
                  value={newVoucherForm.narration}
                  onChange={(e) => setNewVoucherForm({ ...newVoucherForm, narration: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              {/* Dynamic Entry Lines Table */}
              <div className="space-y-2 border border-slate-200 rounded-xl p-3 bg-slate-50/50">
                <div className="flex items-center justify-between">
                  <h4 className="font-bold text-slate-900">Journal Entry Lines</h4>
                  <button
                    type="button"
                    onClick={() => setNewVoucherForm({
                      ...newVoucherForm,
                      lines: [...newVoucherForm.lines, { accountId: "", debitAmount: 0, creditAmount: 0, lineNarration: "" }]
                    })}
                    className="inline-flex items-center gap-1 text-xs font-semibold text-indigo-600 hover:text-indigo-800"
                  >
                    <Plus className="w-3.5 h-3.5" /> Add Row
                  </button>
                </div>

                <div className="space-y-2">
                  {newVoucherForm.lines.map((line, idx) => (
                    <div key={idx} className="grid grid-cols-12 gap-2 items-center bg-white p-2 rounded-lg border border-slate-200">
                      <div className="col-span-5">
                        <select
                          required
                          value={line.accountId}
                          onChange={(e) => {
                            const updated = [...newVoucherForm.lines];
                            updated[idx].accountId = e.target.value;
                            setNewVoucherForm({ ...newVoucherForm, lines: updated });
                          }}
                          className="w-full px-2 py-1.5 border border-slate-300 rounded text-xs focus:ring-1 focus:ring-indigo-500"
                        >
                          <option value="">-- Select Account --</option>
                          {accounts.map((acc) => (
                            <option key={acc.id} value={acc.id}>
                              {acc.accountCode} - {acc.accountName} ({acc.categoryName})
                            </option>
                          ))}
                        </select>
                      </div>

                      <div className="col-span-3">
                        <input
                          type="number"
                          step="0.01"
                          placeholder="Debit (৳)"
                          value={line.debitAmount || ""}
                          onChange={(e) => {
                            const updated = [...newVoucherForm.lines];
                            updated[idx].debitAmount = Number(e.target.value);
                            if (Number(e.target.value) > 0) updated[idx].creditAmount = 0;
                            setNewVoucherForm({ ...newVoucherForm, lines: updated });
                          }}
                          className="w-full px-2 py-1.5 border border-slate-300 rounded text-xs text-right font-mono focus:ring-1 focus:ring-indigo-500"
                        />
                      </div>

                      <div className="col-span-3">
                        <input
                          type="number"
                          step="0.01"
                          placeholder="Credit (৳)"
                          value={line.creditAmount || ""}
                          onChange={(e) => {
                            const updated = [...newVoucherForm.lines];
                            updated[idx].creditAmount = Number(e.target.value);
                            if (Number(e.target.value) > 0) updated[idx].debitAmount = 0;
                            setNewVoucherForm({ ...newVoucherForm, lines: updated });
                          }}
                          className="w-full px-2 py-1.5 border border-slate-300 rounded text-xs text-right font-mono focus:ring-1 focus:ring-indigo-500"
                        />
                      </div>

                      <div className="col-span-1 text-center">
                        {newVoucherForm.lines.length > 2 && (
                          <button
                            type="button"
                            onClick={() => {
                              const updated = newVoucherForm.lines.filter((_, i) => i !== idx);
                              setNewVoucherForm({ ...newVoucherForm, lines: updated });
                            }}
                            className="p-1 text-rose-500 hover:text-rose-700 hover:bg-rose-50 rounded"
                          >
                            <Trash2 className="w-3.5 h-3.5" />
                          </button>
                        )}
                      </div>
                    </div>
                  ))}
                </div>

                {/* Summary / Balancer Indicator */}
                <div className="flex items-center justify-between pt-2 border-t border-slate-200 text-xs font-mono">
                  <div className="flex items-center gap-4">
                    <span>Total Debit: <strong>{formatCurrency(voucherLinesTotalDebit)}</strong></span>
                    <span>Total Credit: <strong>{formatCurrency(voucherLinesTotalCredit)}</strong></span>
                  </div>

                  {isVoucherBalanced ? (
                    <span className="flex items-center gap-1 font-bold text-emerald-600">
                      <CheckCircle2 className="w-4 h-4" /> Balanced
                    </span>
                  ) : (
                    <span className="flex items-center gap-1 font-bold text-rose-600">
                      <AlertCircle className="w-4 h-4" /> Unbalanced (Diff: {formatCurrency(Math.abs(voucherLinesTotalDebit - voucherLinesTotalCredit))})
                    </span>
                  )}
                </div>
              </div>

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowNewVoucherModal(false)}
                  className="px-4 py-2 border border-slate-300 text-slate-700 rounded-lg hover:bg-slate-50 font-semibold"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  disabled={!isVoucherBalanced}
                  className={`px-4 py-2 rounded-lg font-semibold text-white shadow-sm transition-all ${
                    isVoucherBalanced
                      ? "bg-indigo-600 hover:bg-indigo-700 cursor-pointer"
                      : "bg-slate-300 cursor-not-allowed"
                  }`}
                >
                  Save Draft Voucher
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: VIEW VOUCHER DETAILS */}
      {selectedVoucherForView && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-2xl w-full p-6 shadow-2xl border border-slate-200 space-y-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <div className="flex items-center gap-2">
                  <h3 className="text-lg font-bold text-slate-900">{selectedVoucherForView.voucherNumber}</h3>
                  {getStatusBadge(selectedVoucherForView.statusName)}
                </div>
                <p className="text-xs text-slate-500 mt-0.5">
                  Date: {new Date(selectedVoucherForView.voucherDate).toLocaleDateString()} | Type: {selectedVoucherForView.typeName}
                </p>
              </div>
              <button onClick={() => setSelectedVoucherForView(null)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="bg-slate-50 p-3 rounded-lg border border-slate-200 text-xs space-y-1">
              <div><span className="font-semibold text-slate-600">Narration:</span> {selectedVoucherForView.narration}</div>
              {selectedVoucherForView.referenceNumber && (
                <div><span className="font-semibold text-slate-600">Reference:</span> {selectedVoucherForView.referenceType}: {selectedVoucherForView.referenceNumber}</div>
              )}
              {selectedVoucherForView.postedBy && (
                <div><span className="font-semibold text-slate-600">Posted By:</span> {selectedVoucherForView.postedBy} on {new Date(selectedVoucherForView.postedAt || "").toLocaleString()}</div>
              )}
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs font-mono">
                <thead className="bg-slate-100/70 text-slate-600 uppercase font-sans font-semibold border-b border-slate-200">
                  <tr>
                    <th className="px-3 py-2">Account</th>
                    <th className="px-3 py-2">Line Narration</th>
                    <th className="px-3 py-2 text-right">Debit (৳)</th>
                    <th className="px-3 py-2 text-right">Credit (৳)</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {selectedVoucherForView.lines.map((l) => (
                    <tr key={l.id} className="hover:bg-slate-50">
                      <td className="px-3 py-2 font-sans font-medium text-slate-900">
                        {l.accountCode} - {l.accountName}
                      </td>
                      <td className="px-3 py-2 font-sans text-slate-600">{l.lineNarration || "-"}</td>
                      <td className="px-3 py-2 text-right font-bold text-slate-900">{l.debitAmount > 0 ? formatCurrency(l.debitAmount) : "-"}</td>
                      <td className="px-3 py-2 text-right font-bold text-slate-900">{l.creditAmount > 0 ? formatCurrency(l.creditAmount) : "-"}</td>
                    </tr>
                  ))}
                </tbody>
                <tfoot className="bg-slate-50 font-bold text-slate-900 border-t-2 border-slate-200">
                  <tr>
                    <td colSpan={2} className="px-3 py-2 font-sans uppercase">Total</td>
                    <td className="px-3 py-2 text-right">{formatCurrency(selectedVoucherForView.totalDebit)}</td>
                    <td className="px-3 py-2 text-right">{formatCurrency(selectedVoucherForView.totalCredit)}</td>
                  </tr>
                </tfoot>
              </table>
            </div>

            <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
              <button
                type="button"
                onClick={() => setSelectedVoucherForView(null)}
                className="px-4 py-2 border border-slate-300 text-slate-700 rounded-lg hover:bg-slate-50 font-semibold text-xs"
              >
                Close
              </button>
              {selectedVoucherForView.statusName.toLowerCase() === "draft" && (
                <button
                  type="button"
                  onClick={() => {
                    handlePostVoucher(selectedVoucherForView.id);
                    setSelectedVoucherForView(null);
                  }}
                  className="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 font-semibold text-xs shadow-sm"
                >
                  Post to General Ledger
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
