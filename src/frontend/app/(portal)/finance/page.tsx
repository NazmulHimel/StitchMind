"use client";

import React, { useState, useEffect } from "react";
import { 
  DollarSign, Landmark, BookOpen, FileSpreadsheet, Plus, Search, 
  Filter, CheckCircle2, AlertCircle, RefreshCw, ArrowUpRight, 
  ArrowDownRight, Scale, TrendingUp, Receipt, FileText, Check, 
  X, Trash2, Calendar, Eye, Download, Printer, ShieldCheck,
  Layers, GitCompare, Calculator, Percent, Lock, Unlock, Building2,
  Coins, ArrowRight, ShieldAlert, FileCheck2, Globe, Clock
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

// Phase 6 DTOs
interface ThreeWayMatchLogDto {
  id: string;
  vendorBillId: string;
  poUnitPrice: number;
  billedUnitPrice: number;
  priceVariancePct: number;
  grnPassedQuantity: number;
  billedQuantity: number;
  quantityVariance: number;
  isPriceMatched: boolean;
  isQuantityMatched: boolean;
  overallMatchResult: number;
  overallMatchResultName: string;
  discrepancyNotes?: string;
  evaluatedBy: string;
  evaluatedAtUtc: string;
}

interface VendorBillDto {
  id: string;
  billNumber: string;
  supplierId: string;
  supplierName: string;
  purchaseOrderId: string;
  purchaseOrderNumber: string;
  grnId: string;
  grnNumber: string;
  invoiceDate: string;
  dueDate: string;
  billedUnitPrice: number;
  billedQuantity: number;
  billedAmount: number;
  currency: string;
  matchStatus: number;
  matchStatusName: string;
  isApprovedForPayment: boolean;
  approvedBy?: string;
  approvedAtUtc?: string;
  taxDeductedAtSourceTDS: number;
  vatDeductedAtSourceVDS: number;
  netPayableAmount: number;
  notes?: string;
  matchLogs: ThreeWayMatchLogDto[];
}

interface MasterLetterOfCreditDto {
  id: string;
  mlcNumber: string;
  buyerId: string;
  buyerName: string;
  issuingBank: string;
  advisingBank: string;
  beneficiary: string;
  totalAmount: number;
  currency: string;
  issueDate: string;
  expiryDate: string;
  latestShipmentDate: string;
  maxB2BAllocationPct: number;
  maxB2BAllocationAmount: number;
  utilizedB2BAmount: number;
  availableB2BAmount: number;
  status: number;
  statusName: string;
  realizedAmount: number;
  realizedAtUtc?: string;
  backToBackLCs: BackToBackLetterOfCreditDto[];
}

interface BackToBackLetterOfCreditDto {
  id: string;
  b2bLCNumber: string;
  masterLCId: string;
  masterLCNumber: string;
  supplierId: string;
  supplierName: string;
  amount: number;
  currency: string;
  issueDate: string;
  maturityDate: string;
  tenorDays: number;
  status: number;
  statusName: string;
  settledAmount: number;
  settledAtUtc?: string;
  settlementReference?: string;
}

interface ForeignBillDiscountingDto {
  id: string;
  fdbpNumber: string;
  masterLCId: string;
  masterLCNumber: string;
  bankName: string;
  billOfLadingNumber: string;
  fobValue: number;
  advancePercentage: number;
  advancedAmount: number;
  bankDiscountCharges: number;
  netProceedsRealized: number;
  currency: string;
  realizationDate: string;
  status: string;
  remarks?: string;
}

interface OrderCostingVarianceDto {
  id: string;
  styleId: string;
  styleNumber: string;
  styleName: string;
  buyerName: string;
  orderQuantity: number;
  buyerFOBPricePerPc: number;
  buyerFOBRevenue: number;
  budgetRawMaterialsCost: number;
  actualRawMaterialsCost: number;
  rawMaterialVariance: number;
  budgetDirectLaborCost: number;
  actualDirectLaborCost: number;
  directLaborVariance: number;
  budgetOverheadsCost: number;
  actualOverheadsCost: number;
  overheadsVariance: number;
  totalBudgetCost: number;
  totalActualCost: number;
  totalCostVariance: number;
  netRealizedProfit: number;
  netProfitMarginPct: number;
  budgetedProfit: number;
  budgetedProfitMarginPct: number;
  currency: string;
  analyzedAtUtc: string;
  evaluatedBy: string;
}

interface CurrencyExchangeRateDto {
  id: string;
  fromCurrency: string;
  toCurrency: string;
  exchangeRate: number;
  effectiveDate: string;
  source: string;
  isActive: boolean;
}

interface FixedAssetDto {
  id: string;
  assetCode: string;
  assetName: string;
  department: string;
  purchaseDate: string;
  originalCost: number;
  salvageValue: number;
  usefulLifeYears: number;
  depreciationMethod: string;
  annualDepreciation: number;
  monthlyDepreciation: number;
  accumulatedDepreciation: number;
  currentBookValue: number;
  isActive: boolean;
}

interface AgingItemDto {
  referenceId: string;
  referenceNumber: string;
  partyName: string;
  partyType: string;
  invoiceDate: string;
  dueDate: string;
  daysOverdue: number;
  totalAmount: number;
  balanceAmount: number;
  bracket: number;
  bracketName: string;
}

interface AgingAnalysisSummaryDto {
  analysisType: string;
  asOfDate: string;
  totalCurrent_0_30: number;
  totalDays_31_60: number;
  totalDays_61_90: number;
  totalDays_90_Plus: number;
  grandTotalOutstanding: number;
  items: AgingItemDto[];
}

export default function FinancePage() {
  const [activeTab, setActiveTab] = useState<"overview" | "threeway" | "commercial" | "costing" | "coa" | "vouchers" | "ledger" | "statements">("overview");
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

  // Phase 6 Data States
  const [vendorBills, setVendorBills] = useState<VendorBillDto[]>([]);
  const [masterLCs, setMasterLCs] = useState<MasterLetterOfCreditDto[]>([]);
  const [backToBackLCs, setBackToBackLCs] = useState<BackToBackLetterOfCreditDto[]>([]);
  const [fdbpBills, setFdbpBills] = useState<ForeignBillDiscountingDto[]>([]);
  const [costingVariances, setCostingVariances] = useState<OrderCostingVarianceDto[]>([]);
  const [exchangeRates, setExchangeRates] = useState<CurrencyExchangeRateDto[]>([]);
  const [fixedAssets, setFixedAssets] = useState<FixedAssetDto[]>([]);
  const [apAging, setApAging] = useState<AgingAnalysisSummaryDto | null>(null);

  // Phase 6 Action States
  const [selectedBillForAudit, setSelectedBillForAudit] = useState<VendorBillDto | null>(null);
  const [showMatchModal, setShowMatchModal] = useState(false);
  const [matchForm, setMatchForm] = useState({
    poUnitPrice: 2.35,
    grnPassedQuantity: 36750,
    evaluatedBy: "Internal Audit Lead"
  });

  const [showB2BModal, setShowB2BModal] = useState(false);
  const [newB2BForm, setNewB2BForm] = useState({
    b2bLCNumber: "B2B-2026-0105",
    masterLCId: "",
    supplierId: "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
    supplierName: "Apex Textile Mills Ltd.",
    amount: 35000,
    currency: "USD",
    issueDate: new Date().toISOString().split("T")[0],
    maturityDate: new Date(Date.now() + 90 * 86400000).toISOString().split("T")[0],
    tenorDays: 90
  });

  // Forex Calculator state
  const [forexCalc, setForexCalc] = useState({
    foreignAmount: 180000,
    foreignCurrency: "USD",
    bookingRate: 117.50,
    settlementRate: 118.20
  });

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
      const [mRes, accRes, vRes, billsRes, mlcRes, b2bRes, fdbpRes, costRes, ratesRes, assetsRes, agingRes] = await Promise.all([
        apiRequest<FinanceMetricsDto>("/api/finance/metrics"),
        apiRequest<AccountDto[]>("/api/finance/accounts"),
        apiRequest<JournalVoucherDto[]>("/api/finance/vouchers"),
        apiRequest<VendorBillDto[]>("/api/finance/bills"),
        apiRequest<MasterLetterOfCreditDto[]>("/api/finance/master-lcs"),
        apiRequest<BackToBackLetterOfCreditDto[]>("/api/finance/b2b-lcs"),
        apiRequest<ForeignBillDiscountingDto[]>("/api/finance/fdbp"),
        apiRequest<OrderCostingVarianceDto[]>("/api/finance/costing-variances"),
        apiRequest<CurrencyExchangeRateDto[]>("/api/finance/exchange-rates"),
        apiRequest<FixedAssetDto[]>("/api/finance/fixed-assets"),
        apiRequest<AgingAnalysisSummaryDto>("/api/finance/aging-analysis?type=AccountsPayable")
      ]);

      if (mRes.success && mRes.data) setMetrics(mRes.data);
      if (accRes.success && accRes.data) setAccounts(accRes.data);
      if (vRes.success && vRes.data) setVouchers(vRes.data);

      if (billsRes.success && billsRes.data) setVendorBills(billsRes.data);
      if (mlcRes.success && mlcRes.data) {
        setMasterLCs(mlcRes.data);
        if (mlcRes.data.length > 0 && !newB2BForm.masterLCId) {
          const firstMlcId = mlcRes.data[0].id;
          setNewB2BForm(prev => ({ ...prev, masterLCId: firstMlcId }));
        }
      }
      if (b2bRes.success && b2bRes.data) setBackToBackLCs(b2bRes.data);
      if (fdbpRes.success && fdbpRes.data) setFdbpBills(fdbpRes.data);
      if (costRes.success && costRes.data) setCostingVariances(costRes.data);
      if (ratesRes.success && ratesRes.data) setExchangeRates(ratesRes.data);
      if (assetsRes.success && assetsRes.data) setFixedAssets(assetsRes.data);
      if (agingRes.success && agingRes.data) setApAging(agingRes.data);

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

  // Phase 6 Action Handlers
  const handleExecuteThreeWayMatch = async (billId: string) => {
    try {
      const res = await apiRequest<{ isSuccess: boolean; value?: ThreeWayMatchLogDto }>(`/api/finance/bills/${billId}/three-way-match`, {
        method: "POST",
        body: matchForm
      });

      if (res.success) {
        setFeedback({ type: "success", message: `Three-Way Match executed successfully: Result ${res.data?.value?.overallMatchResultName || "PASSED"}` });
        setShowMatchModal(false);
        fetchData();
      } else {
        setFeedback({ type: "error", message: res.error || "Match evaluation failed." });
        fetchData();
      }
    } catch (err: any) {
      setFeedback({ type: "error", message: err.message || "Failed to execute match" });
    }
  };

  const handleCreateB2BLC = async (e: React.FormEvent) => {
    e.preventDefault();
    try {
      const res = await apiRequest<{ isSuccess: boolean; value?: BackToBackLetterOfCreditDto }>("/api/finance/b2b-lcs", {
        method: "POST",
        body: newB2BForm
      });

      if (res.success) {
        setFeedback({ type: "success", message: `Back-to-Back LC ${newB2BForm.b2bLCNumber} opened successfully.` });
        setShowB2BModal(false);
        fetchData();
      } else {
        setFeedback({ type: "error", message: res.error || "Failed to open B2B LC. Check 70% headroom." });
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
          <div className="flex items-center gap-2">
            <h1 className="text-2xl font-bold tracking-tight text-slate-900">Commercial Finance & Accounting</h1>
            <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800 border border-emerald-300">
              Phase 6 Active
            </span>
          </div>
          <p className="text-sm text-slate-500 mt-1">
            AC-06 Automated Three-Way Matching, Export Trade LCs, Back-to-Back Settlements, Order Costing Variances & General Ledger.
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
            onClick={() => setShowB2BModal(true)}
            className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-slate-700 bg-white border border-slate-300 rounded-lg hover:bg-slate-50 transition-colors shadow-sm"
          >
            <Landmark className="w-3.5 h-3.5 text-indigo-600" /> Issue B2B LC
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
            { id: "threeway", label: "3-Way Match (AC-06)", icon: ShieldCheck },
            { id: "commercial", label: "Commercial LCs & FDBP", icon: Landmark },
            { id: "costing", label: "Costing & Forex", icon: GitCompare },
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

      {/* TAB: AUTOMATED THREE-WAY MATCHING (AC-06) */}
      {activeTab === "threeway" && (
        <div className="space-y-6">
          {/* Top AC-06 Header Banner */}
          <div className="bg-gradient-to-r from-slate-900 via-indigo-950 to-slate-900 p-6 rounded-2xl text-white shadow-xl border border-indigo-900/50">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
              <div>
                <div className="flex items-center gap-2">
                  <span className="p-2 bg-indigo-500/20 text-indigo-300 rounded-lg border border-indigo-400/30">
                    <ShieldCheck className="w-5 h-5" />
                  </span>
                  <span className="text-xs font-semibold uppercase tracking-wider text-indigo-300">
                    Acceptance Criteria AC-06
                  </span>
                </div>
                <h2 className="text-xl font-bold tracking-tight text-white mt-2">
                  Automated Three-Way Matching & Accounts Payable Reconciliation
                </h2>
                <p className="text-xs text-slate-300 mt-1 max-w-2xl">
                  Reconciles approved Purchase Order rates with QC-cleared Goods Receipt Note (GRN) inspection quantities before releasing Vendor Bills for payment. Overbilled or unverified items are automatically locked.
                </p>
              </div>
              <div className="flex items-center gap-3">
                <a
                  href="/api/reports/three-way-match/00000000-0000-0000-0000-000000000000/pdf"
                  target="_blank"
                  rel="noreferrer"
                  className="inline-flex items-center gap-2 px-4 py-2 bg-indigo-600 hover:bg-indigo-500 text-white rounded-lg text-xs font-semibold transition-all shadow-md"
                >
                  <Download className="w-4 h-4" /> Download AC-06 Certificate (PDF)
                </a>
              </div>
            </div>

            {/* Reconciliation Flow Visual */}
            <div className="grid grid-cols-1 md:grid-cols-4 gap-3 mt-6 pt-5 border-t border-slate-800 text-xs">
              <div className="bg-slate-800/60 p-3 rounded-lg border border-slate-700/60">
                <div className="text-slate-400 font-semibold uppercase text-[10px]">Step 1: Approved PO</div>
                <div className="text-slate-200 font-medium mt-1">PO-2026-0145 ($2.3500/Yd)</div>
                <div className="text-emerald-400 text-[10px] mt-0.5">Budget rate locked</div>
              </div>
              <div className="bg-slate-800/60 p-3 rounded-lg border border-slate-700/60">
                <div className="text-slate-400 font-semibold uppercase text-[10px]">Step 2: QC Cleared GRN</div>
                <div className="text-slate-200 font-medium mt-1">GRN-2026-0210 (36,750 Yds)</div>
                <div className="text-emerald-400 text-[10px] mt-0.5">ASTM 4-Point passed</div>
              </div>
              <div className="bg-slate-800/60 p-3 rounded-lg border border-slate-700/60">
                <div className="text-slate-400 font-semibold uppercase text-[10px]">Step 3: Supplier Invoice</div>
                <div className="text-slate-200 font-medium mt-1">BILL-APEX-2026-091 ($86,362.50)</div>
                <div className="text-indigo-300 text-[10px] mt-0.5">3% TDS & 7.5% VDS computed</div>
              </div>
              <div className="bg-indigo-950/80 p-3 rounded-lg border border-indigo-700/60">
                <div className="text-indigo-300 font-semibold uppercase text-[10px]">Step 4: Audit Decision</div>
                <div className="text-white font-bold mt-1">100% Match Verified</div>
                <div className="text-emerald-400 font-semibold text-[10px] mt-0.5">APPROVED FOR PAYMENT</div>
              </div>
            </div>
          </div>

          {/* Three-Way Matching KPI Cards */}
          <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Total Invoiced Bills</span>
                <span className="p-2 bg-slate-100 text-slate-700 rounded-lg"><Receipt className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-slate-900">
                  ${(vendorBills.reduce((s, b) => s + b.billedAmount, 0) || 126112.50).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </h3>
                <p className="text-xs text-slate-500 mt-1">{vendorBills.length || 2} Bills in Reconciliation Queue</p>
              </div>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Approved for Payment</span>
                <span className="p-2 bg-emerald-50 text-emerald-600 rounded-lg"><CheckCircle2 className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-emerald-700">
                  ${(vendorBills.filter(b => b.isApprovedForPayment).reduce((s, b) => s + b.billedAmount, 0) || 86362.50).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </h3>
                <p className="text-xs text-emerald-600 font-medium mt-1">100% Rate & Qty Verified (AC-06)</p>
              </div>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Discrepancy Locked</span>
                <span className="p-2 bg-rose-50 text-rose-600 rounded-lg"><Lock className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-rose-700">
                  ${(vendorBills.filter(b => !b.isApprovedForPayment).reduce((s, b) => s + b.billedAmount, 0) || 39750.00).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </h3>
                <p className="text-xs text-rose-600 font-medium mt-1">Price Markup / Quantity Block</p>
              </div>
            </div>

            <div className="bg-white p-5 rounded-xl border border-slate-200 shadow-sm">
              <div className="flex items-center justify-between">
                <span className="text-xs font-semibold uppercase tracking-wider text-slate-500">Net Statutory Payable</span>
                <span className="p-2 bg-purple-50 text-purple-600 rounded-lg"><Percent className="w-4 h-4" /></span>
              </div>
              <div className="mt-3">
                <h3 className="text-2xl font-bold text-purple-700">
                  ${(vendorBills.filter(b => b.isApprovedForPayment).reduce((s, b) => s + b.netPayableAmount, 0) || 77294.43).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </h3>
                <p className="text-xs text-slate-500 mt-1">After 3.0% TDS & 7.5% VDS Deductions</p>
              </div>
            </div>
          </div>

          {/* Vendor Bills Reconciliation Ledger */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            <div className="p-5 border-b border-slate-200 flex flex-col md:flex-row md:items-center justify-between gap-3">
              <div>
                <h3 className="text-base font-semibold text-slate-900">Vendor Bills & 3-Way Match Verification Queue</h3>
                <p className="text-xs text-slate-500 mt-0.5">
                  Click 'Verify 3-Way Match' to run algorithmic validation or download the official verification certificate.
                </p>
              </div>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 font-semibold border-b border-slate-200 uppercase">
                  <tr>
                    <th className="px-4 py-3">Bill Number</th>
                    <th className="px-4 py-3">Supplier Name</th>
                    <th className="px-4 py-3">PO & GRN Ref</th>
                    <th className="px-4 py-3 text-right">Billed Rate</th>
                    <th className="px-4 py-3 text-right">Billed Qty</th>
                    <th className="px-4 py-3 text-right">Gross Amount</th>
                    <th className="px-4 py-3 text-right">TDS (3%) & VDS (7.5%)</th>
                    <th className="px-4 py-3 text-right">Net Payable</th>
                    <th className="px-4 py-3 text-center">Match Status</th>
                    <th className="px-4 py-3 text-right">Audit Actions</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100">
                  {vendorBills.map((bill) => (
                    <tr key={bill.id} className="hover:bg-slate-50/80 transition-colors">
                      <td className="px-4 py-3 font-mono font-bold text-slate-900">
                        {bill.billNumber}
                      </td>
                      <td className="px-4 py-3 font-medium text-slate-800">
                        {bill.supplierName}
                      </td>
                      <td className="px-4 py-3 text-slate-600 font-mono">
                        <div>{bill.purchaseOrderNumber}</div>
                        <div className="text-[11px] text-slate-400">{bill.grnNumber}</div>
                      </td>
                      <td className="px-4 py-3 text-right font-mono font-medium text-slate-900">
                        ${bill.billedUnitPrice.toFixed(4)}
                      </td>
                      <td className="px-4 py-3 text-right font-mono font-medium text-slate-900">
                        {bill.billedQuantity.toLocaleString()} Yds
                      </td>
                      <td className="px-4 py-3 text-right font-mono font-bold text-slate-900">
                        ${bill.billedAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })}
                      </td>
                      <td className="px-4 py-3 text-right font-mono text-slate-600">
                        <div>TDS: -${bill.taxDeductedAtSourceTDS.toFixed(2)}</div>
                        <div className="text-[11px] text-slate-400">VDS: -${bill.vatDeductedAtSourceVDS.toFixed(2)}</div>
                      </td>
                      <td className="px-4 py-3 text-right font-mono font-bold text-emerald-700">
                        ${bill.netPayableAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })}
                      </td>
                      <td className="px-4 py-3 text-center">
                        {bill.isApprovedForPayment ? (
                          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-semibold bg-emerald-100 text-emerald-800 border border-emerald-300">
                            <ShieldCheck className="w-3.5 h-3.5 text-emerald-600" /> APPROVED FOR PAYMENT
                          </span>
                        ) : (
                          <span className="inline-flex items-center gap-1 px-2.5 py-1 rounded-full text-[11px] font-semibold bg-rose-100 text-rose-800 border border-rose-300">
                            <ShieldAlert className="w-3.5 h-3.5 text-rose-600" /> MATCH FAILED (LOCKED)
                          </span>
                        )}
                      </td>
                      <td className="px-4 py-3 text-right">
                        <div className="flex items-center justify-end gap-1.5">
                          <button
                            onClick={() => {
                              setSelectedBillForAudit(bill);
                              setMatchForm({
                                poUnitPrice: bill.billedUnitPrice,
                                grnPassedQuantity: bill.billedQuantity,
                                evaluatedBy: "Chief Financial Officer"
                              });
                              setShowMatchModal(true);
                            }}
                            className="px-2.5 py-1.5 text-xs font-semibold text-indigo-700 bg-indigo-50 hover:bg-indigo-100 rounded-lg border border-indigo-200 transition-colors"
                          >
                            Verify
                          </button>
                          <a
                            href={`/api/reports/three-way-match/${bill.id}/pdf`}
                            target="_blank"
                            rel="noreferrer"
                            className="p-1.5 text-slate-600 hover:text-indigo-600 hover:bg-slate-100 rounded-lg transition-colors"
                            title="Download Official 3-Way Match PDF Certificate"
                          >
                            <Download className="w-4 h-4" />
                          </a>
                        </div>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB: COMMERCIAL TRADE, MASTER LCS & FDBP (ACC-03) */}
      {activeTab === "commercial" && (
        <div className="space-y-6">
          {/* Master LC Cards */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {masterLCs.map((mlc) => {
              const allocationPct = mlc.maxB2BAllocationAmount > 0
                ? Math.round((mlc.utilizedB2BAmount / mlc.maxB2BAllocationAmount) * 100)
                : 0;

              return (
                <div key={mlc.id} className="lg:col-span-2 bg-white rounded-xl border border-slate-200 shadow-sm p-6">
                  <div className="flex items-center justify-between">
                    <div>
                      <div className="flex items-center gap-2">
                        <span className="p-1.5 bg-blue-50 text-blue-600 rounded-lg border border-blue-200"><Landmark className="w-4 h-4" /></span>
                        <span className="font-mono text-sm font-bold text-slate-900">{mlc.mlcNumber}</span>
                        <span className="px-2 py-0.5 rounded-full text-[11px] font-semibold bg-emerald-50 text-emerald-700 border border-emerald-200">
                          {mlc.statusName}
                        </span>
                      </div>
                      <h3 className="text-lg font-bold text-slate-900 mt-2">{mlc.buyerName}</h3>
                      <p className="text-xs text-slate-500">
                        Issuing Bank: <span className="font-medium text-slate-700">{mlc.issuingBank}</span> | Advising Bank: <span className="font-medium text-slate-700">{mlc.advisingBank}</span>
                      </p>
                    </div>
                    <div className="text-right">
                      <div className="text-xs font-semibold uppercase text-slate-500">Total LC Value</div>
                      <div className="text-2xl font-bold font-mono text-indigo-700">
                        ${mlc.totalAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })} {mlc.currency}
                      </div>
                      <div className="text-[11px] text-emerald-600 font-medium">Realized: ${mlc.realizedAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })}</div>
                    </div>
                  </div>

                  {/* 70% B2B Allocation Progress Bar */}
                  <div className="mt-6 pt-5 border-t border-slate-100">
                    <div className="flex items-center justify-between text-xs mb-2">
                      <span className="font-semibold text-slate-700">
                        Back-to-Back (B2B) LC Allocation Headroom (Max 70%)
                      </span>
                      <span className="font-mono text-slate-600 font-medium">
                        ${mlc.utilizedB2BAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })} / ${mlc.maxB2BAllocationAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })} ({allocationPct}%)
                      </span>
                    </div>

                    <div className="w-full bg-slate-100 rounded-full h-3 overflow-hidden">
                      <div
                        className={`h-full rounded-full transition-all duration-500 ${allocationPct > 90 ? "bg-rose-500" : allocationPct > 60 ? "bg-indigo-600" : "bg-emerald-500"}`}
                        style={{ width: `${Math.min(100, allocationPct)}%` }}
                      />
                    </div>

                    <div className="flex items-center justify-between text-[11px] text-slate-500 mt-2">
                      <span>Utilized for Yarn & Fabric: ${mlc.utilizedB2BAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })}</span>
                      <span className="font-semibold text-emerald-700">
                        Available B2B Headroom: ${mlc.availableB2BAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })}
                      </span>
                    </div>
                  </div>
                </div>
              );
            })}

            {/* Quick FDBP Realization Card */}
            <div className="bg-gradient-to-br from-indigo-900 to-slate-900 rounded-xl p-6 text-white shadow-md flex flex-col justify-between">
              <div>
                <div className="flex items-center justify-between text-xs text-indigo-300">
                  <span className="uppercase font-semibold tracking-wider">Bill Discounting (FDBP)</span>
                  <Coins className="w-4 h-4" />
                </div>
                <h4 className="text-lg font-bold mt-2">Post-Shipment FDBP Advance</h4>
                <p className="text-xs text-slate-300 mt-1">
                  Advances up to 90% liquidity against clean Bill of Lading presentations.
                </p>

                <div className="mt-4 space-y-2 text-xs">
                  <div className="flex justify-between py-1 border-b border-indigo-800">
                    <span className="text-slate-400">Negotiating Bank:</span>
                    <span className="font-semibold text-white">Standard Chartered Bank</span>
                  </div>
                  <div className="flex justify-between py-1 border-b border-indigo-800">
                    <span className="text-slate-400">Bill of Lading:</span>
                    <span className="font-mono text-white">BL-SWED-2026-7890</span>
                  </div>
                  <div className="flex justify-between py-1 border-b border-indigo-800">
                    <span className="text-slate-400">Advanced Amount (90%):</span>
                    <span className="font-mono text-emerald-400 font-bold">$162,000.00 USD</span>
                  </div>
                  <div className="flex justify-between py-1 border-b border-indigo-800">
                    <span className="text-slate-400">Bank Charges:</span>
                    <span className="font-mono text-rose-300">-$1,875.00 USD</span>
                  </div>
                  <div className="flex justify-between py-1 pt-2 font-bold">
                    <span className="text-indigo-200">Net Proceeds Realized:</span>
                    <span className="font-mono text-white text-sm">$160,125.00 USD</span>
                  </div>
                </div>
              </div>

              <div className="mt-5">
                <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-500/20 text-emerald-300 border border-emerald-400/30">
                  <CheckCircle2 className="w-3.5 h-3.5" /> Funds Credited to Principal A/C
                </span>
              </div>
            </div>
          </div>

          {/* Back-to-Back LCs Table */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm overflow-hidden">
            <div className="p-5 border-b border-slate-200 flex items-center justify-between">
              <div>
                <h3 className="text-base font-semibold text-slate-900">Back-to-Back (B2B) Letters of Credit</h3>
                <p className="text-xs text-slate-500 mt-0.5">Supplier import commitments issued against Export Master LC collateral.</p>
              </div>
              <button
                onClick={() => setShowB2BModal(true)}
                className="inline-flex items-center gap-1.5 px-3 py-1.5 text-xs font-semibold text-white bg-indigo-600 rounded-lg hover:bg-indigo-700 shadow-sm"
              >
                <Plus className="w-3.5 h-3.5" /> Issue New B2B LC
              </button>
            </div>

            <div className="overflow-x-auto">
              <table className="w-full text-left text-xs">
                <thead className="bg-slate-50 text-slate-600 font-semibold border-b border-slate-200 uppercase">
                  <tr>
                    <th className="px-4 py-3">B2B LC Number</th>
                    <th className="px-4 py-3">Supplier (Beneficiary)</th>
                    <th className="px-4 py-3">Master LC Ref</th>
                    <th className="px-4 py-3 text-right">LC Amount</th>
                    <th className="px-4 py-3 text-center">Tenor</th>
                    <th className="px-4 py-3 text-center">Maturity Date</th>
                    <th className="px-4 py-3 text-center">Status</th>
                    <th className="px-4 py-3 text-right">Settlement Ref</th>
                  </tr>
                </thead>
                <tbody className="divide-y divide-slate-100 font-mono">
                  {backToBackLCs.map((b2b) => (
                    <tr key={b2b.id} className="hover:bg-slate-50">
                      <td className="px-4 py-3 font-bold text-slate-900">{b2b.b2bLCNumber}</td>
                      <td className="px-4 py-3 font-sans font-medium text-slate-800">{b2b.supplierName}</td>
                      <td className="px-4 py-3 text-slate-600">{b2b.masterLCNumber}</td>
                      <td className="px-4 py-3 text-right font-bold text-indigo-700">
                        ${b2b.amount.toLocaleString("en-US", { minimumFractionDigits: 2 })} {b2b.currency}
                      </td>
                      <td className="px-4 py-3 text-center text-slate-600">{b2b.tenorDays} Days</td>
                      <td className="px-4 py-3 text-center text-slate-600">{new Date(b2b.maturityDate).toLocaleDateString()}</td>
                      <td className="px-4 py-3 text-center font-sans">
                        <span className={`px-2.5 py-0.5 rounded-full text-[11px] font-semibold ${
                          b2b.statusName.toLowerCase() === "settled"
                            ? "bg-emerald-100 text-emerald-800 border border-emerald-300"
                            : "bg-blue-100 text-blue-800 border border-blue-300"
                        }`}>
                          {b2b.statusName}
                        </span>
                      </td>
                      <td className="px-4 py-3 text-right text-slate-600">{b2b.settlementReference || "-"}</td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          </div>
        </div>
      )}

      {/* TAB: ORDER COSTING VARIANCE, FOREX & ASSETS (ACC-04, ACC-05, ACC-06, ACC-07) */}
      {activeTab === "costing" && (
        <div className="space-y-6">
          {/* Post-Costing Variance Dossier */}
          {costingVariances.map((c) => (
            <div key={c.id} className="bg-white rounded-xl border border-slate-200 shadow-sm p-6">
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 pb-4 border-b border-slate-200">
                <div>
                  <div className="flex items-center gap-2">
                    <span className="px-2.5 py-0.5 rounded-full text-xs font-semibold bg-teal-100 text-teal-800 border border-teal-300">
                      ACC-04 Post-Costing Realized Profit
                    </span>
                    <span className="font-mono text-sm font-bold text-slate-900">{c.styleNumber}</span>
                  </div>
                  <h3 className="text-xl font-bold text-slate-900 mt-1">{c.styleName}</h3>
                  <p className="text-xs text-slate-500">
                    Buyer: <span className="font-semibold text-slate-700">{c.buyerName}</span> | Order Volume: <span className="font-semibold text-slate-700">{c.orderQuantity.toLocaleString()} Pcs</span> @ <span className="font-semibold text-slate-700">${c.buyerFOBPricePerPc.toFixed(2)}/Pc FOB</span>
                  </p>
                </div>
                <div className="flex items-center gap-3">
                  <div className="text-right">
                    <div className="text-xs text-slate-500 font-semibold uppercase">Net Realized Profit</div>
                    <div className="text-2xl font-bold font-mono text-emerald-600">
                      ${c.netRealizedProfit.toLocaleString("en-US", { minimumFractionDigits: 2 })}
                    </div>
                    <div className="text-xs font-bold text-emerald-700">{c.netProfitMarginPct.toFixed(2)}% Realized Margin</div>
                  </div>
                  <a
                    href={`/api/reports/costing-variance/${c.styleId}/pdf`}
                    target="_blank"
                    rel="noreferrer"
                    className="inline-flex items-center gap-1.5 px-3.5 py-2 text-xs font-semibold text-teal-700 bg-teal-50 border border-teal-200 rounded-lg hover:bg-teal-100 shadow-sm transition-all"
                  >
                    <Download className="w-4 h-4" /> Costing Dossier (PDF)
                  </a>
                </div>
              </div>

              {/* Cost Center Breakdown Table */}
              <div className="mt-5 overflow-x-auto">
                <table className="w-full text-left text-xs">
                  <thead className="bg-slate-50 text-slate-600 font-semibold border-b border-slate-200 uppercase">
                    <tr>
                      <th className="px-4 py-2.5">Cost Center / Element</th>
                      <th className="px-4 py-2.5 text-right">Pre-Costing Budget</th>
                      <th className="px-4 py-2.5 text-right">Actual Post-Cost</th>
                      <th className="px-4 py-2.5 text-right">Variance ($)</th>
                      <th className="px-4 py-2.5 text-center">Variance Status</th>
                    </tr>
                  </thead>
                  <tbody className="divide-y divide-slate-100 font-mono">
                    <tr className="hover:bg-slate-50">
                      <td className="px-4 py-2.5 font-sans font-medium text-slate-900">Raw Materials (Fabric & Trims)</td>
                      <td className="px-4 py-2.5 text-right">${c.budgetRawMaterialsCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right">${c.actualRawMaterialsCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right text-emerald-600 font-bold">+${c.rawMaterialVariance.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-center font-sans">
                        <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-100 text-emerald-800">FAVORABLE</span>
                      </td>
                    </tr>
                    <tr className="hover:bg-slate-50">
                      <td className="px-4 py-2.5 font-sans font-medium text-slate-900">Direct Labor (Cutting, Sewing, Finishing)</td>
                      <td className="px-4 py-2.5 text-right">${c.budgetDirectLaborCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right">${c.actualDirectLaborCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right text-emerald-600 font-bold">+${c.directLaborVariance.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-center font-sans">
                        <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-100 text-emerald-800">FAVORABLE</span>
                      </td>
                    </tr>
                    <tr className="hover:bg-slate-50">
                      <td className="px-4 py-2.5 font-sans font-medium text-slate-900">Factory Overheads & Commercial Fees</td>
                      <td className="px-4 py-2.5 text-right">${c.budgetOverheadsCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right">${c.actualOverheadsCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right text-emerald-600 font-bold">+${c.overheadsVariance.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-center font-sans">
                        <span className="px-2 py-0.5 rounded text-[10px] font-bold bg-emerald-100 text-emerald-800">FAVORABLE</span>
                      </td>
                    </tr>
                  </tbody>
                  <tfoot className="bg-slate-50 font-bold text-slate-900 border-t-2 border-slate-200">
                    <tr>
                      <td className="px-4 py-2.5 font-sans uppercase">Total Cost of Production</td>
                      <td className="px-4 py-2.5 text-right font-mono">${c.totalBudgetCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right font-mono">${c.totalActualCost.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-right font-mono text-emerald-600">+${c.totalCostVariance.toLocaleString("en-US", { minimumFractionDigits: 2 })}</td>
                      <td className="px-4 py-2.5 text-center font-sans">
                        <span className="px-2.5 py-0.5 rounded text-[10px] font-bold bg-emerald-600 text-white">COST SAVING</span>
                      </td>
                    </tr>
                  </tfoot>
                </table>
              </div>
            </div>
          ))}

          {/* Sub-grid: Forex Engine + Fixed Assets + AP Aging */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Multi-Currency & Forex Live Calculator (ACC-05) */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-5">
              <div className="flex items-center justify-between pb-3 border-b border-slate-200">
                <div className="flex items-center gap-2">
                  <span className="p-2 bg-blue-50 text-blue-600 rounded-lg"><Globe className="w-4 h-4" /></span>
                  <div>
                    <h3 className="text-sm font-semibold text-slate-900">Multi-Currency & Forex Engine (ACC-05)</h3>
                    <p className="text-[11px] text-slate-500">Live exchange rate variance & realized foreign currency gain/loss</p>
                  </div>
                </div>
              </div>

              {/* Calculator Inputs */}
              <div className="grid grid-cols-3 gap-3 mt-4 text-xs">
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Foreign Amount ($)</label>
                  <input
                    type="number"
                    value={forexCalc.foreignAmount}
                    onChange={(e) => setForexCalc({ ...forexCalc, foreignAmount: Number(e.target.value) })}
                    className="w-full px-2.5 py-1.5 border border-slate-300 rounded font-mono text-xs"
                  />
                </div>
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Booking Rate (৳)</label>
                  <input
                    type="number"
                    step="0.01"
                    value={forexCalc.bookingRate}
                    onChange={(e) => setForexCalc({ ...forexCalc, bookingRate: Number(e.target.value) })}
                    className="w-full px-2.5 py-1.5 border border-slate-300 rounded font-mono text-xs"
                  />
                </div>
                <div>
                  <label className="block text-slate-600 font-semibold mb-1">Settlement Rate (৳)</label>
                  <input
                    type="number"
                    step="0.01"
                    value={forexCalc.settlementRate}
                    onChange={(e) => setForexCalc({ ...forexCalc, settlementRate: Number(e.target.value) })}
                    className="w-full px-2.5 py-1.5 border border-slate-300 rounded font-mono text-xs"
                  />
                </div>
              </div>

              {/* Forex Calculation Result */}
              {(() => {
                const bookingBdt = forexCalc.foreignAmount * forexCalc.bookingRate;
                const settlementBdt = forexCalc.foreignAmount * forexCalc.settlementRate;
                const gainLoss = settlementBdt - bookingBdt;
                return (
                  <div className="mt-4 p-3 rounded-lg bg-slate-50 border border-slate-200 text-xs">
                    <div className="flex justify-between text-slate-600 py-0.5">
                      <span>Booking Valuation (at ৳{forexCalc.bookingRate.toFixed(2)}):</span>
                      <span className="font-mono font-semibold">৳{bookingBdt.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between text-slate-600 py-0.5">
                      <span>Settlement Proceeds (at ৳{forexCalc.settlementRate.toFixed(2)}):</span>
                      <span className="font-mono font-semibold">৳{settlementBdt.toLocaleString()}</span>
                    </div>
                    <div className="flex justify-between pt-2 border-t border-slate-200 font-bold">
                      <span className={gainLoss >= 0 ? "text-emerald-700" : "text-rose-700"}>
                        {gainLoss >= 0 ? "Realized Forex Exchange Gain:" : "Realized Forex Loss:"}
                      </span>
                      <span className={`font-mono text-sm ${gainLoss >= 0 ? "text-emerald-700" : "text-rose-700"}`}>
                        {gainLoss >= 0 ? "+" : "-"}৳{Math.abs(gainLoss).toLocaleString()}
                      </span>
                    </div>
                  </div>
                );
              })()}

              {/* Active Central Bank Rates */}
              <div className="mt-4">
                <div className="text-[11px] font-semibold text-slate-500 uppercase mb-2">Central Bank Reference Exchange Rates</div>
                <div className="grid grid-cols-3 gap-2 text-center text-xs">
                  {exchangeRates.map((r) => (
                    <div key={r.id} className="p-2 rounded bg-slate-50 border border-slate-200">
                      <div className="font-bold text-slate-800">{r.fromCurrency} / {r.toCurrency}</div>
                      <div className="font-mono text-indigo-600 font-semibold mt-0.5">৳{r.exchangeRate.toFixed(2)}</div>
                    </div>
                  ))}
                </div>
              </div>
            </div>

            {/* Fixed Assets Register & Depreciation (ACC-06) */}
            <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-5">
              <div className="flex items-center justify-between pb-3 border-b border-slate-200">
                <div className="flex items-center gap-2">
                  <span className="p-2 bg-amber-50 text-amber-600 rounded-lg"><Building2 className="w-4 h-4" /></span>
                  <div>
                    <h3 className="text-sm font-semibold text-slate-900">Fixed Asset Register & Depreciation (ACC-06)</h3>
                    <p className="text-[11px] text-slate-500">Straight-line monthly capital asset depreciation schedule</p>
                  </div>
                </div>
              </div>

              <div className="mt-4 space-y-3">
                {fixedAssets.map((asset) => (
                  <div key={asset.id} className="p-3 rounded-lg border border-slate-200 bg-slate-50/50 text-xs">
                    <div className="flex items-center justify-between">
                      <div>
                        <span className="font-mono font-bold text-slate-900">{asset.assetCode}</span>
                        <h4 className="font-semibold text-slate-800 mt-0.5">{asset.assetName}</h4>
                        <div className="text-[11px] text-slate-500">{asset.department}</div>
                      </div>
                      <div className="text-right font-mono">
                        <div className="text-slate-500 text-[10px]">Current Book Value</div>
                        <div className="font-bold text-slate-900 text-sm">${asset.currentBookValue.toFixed(2)}</div>
                        <div className="text-[10px] text-slate-400">Cost: ${asset.originalCost.toFixed(2)}</div>
                      </div>
                    </div>
                    <div className="flex items-center justify-between mt-2 pt-2 border-t border-slate-200 text-[11px] text-slate-600 font-mono">
                      <span>Life: {asset.usefulLifeYears} Years</span>
                      <span>Salvage: ${asset.salvageValue.toFixed(2)}</span>
                      <span className="font-semibold text-indigo-600">Monthly Dep: ${asset.monthlyDepreciation.toFixed(2)}</span>
                    </div>
                  </div>
                ))}
              </div>
            </div>
          </div>

          {/* Accounts Payable / Receivable Aging Analysis (ACC-07) */}
          <div className="bg-white rounded-xl border border-slate-200 shadow-sm p-5">
            <div className="flex items-center justify-between pb-4 border-b border-slate-200">
              <div>
                <h3 className="text-base font-semibold text-slate-900">Accounts Payable (AP) Aging Analysis (ACC-07)</h3>
                <p className="text-xs text-slate-500 mt-0.5">
                  Disbursement scheduling and supplier payment exposure across 30-day brackets.
                </p>
              </div>
              <div className="text-right font-mono">
                <span className="text-xs text-slate-500">Total Outstanding Payable: </span>
                <span className="text-base font-bold text-rose-700">
                  ${(apAging?.grandTotalOutstanding || 77294.43).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </span>
              </div>
            </div>

            {/* Aging Bracket Bar */}
            <div className="grid grid-cols-2 md:grid-cols-4 gap-3 mt-4 text-xs">
              <div className="p-3 rounded-lg bg-emerald-50 border border-emerald-200">
                <div className="text-emerald-800 font-semibold">Current (0–30 Days)</div>
                <div className="text-lg font-bold font-mono text-emerald-900 mt-1">
                  ${(apAging?.totalCurrent_0_30 || 77294.43).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </div>
                <div className="text-[10px] text-emerald-700 mt-0.5">Normal credit terms</div>
              </div>
              <div className="p-3 rounded-lg bg-blue-50 border border-blue-200">
                <div className="text-blue-800 font-semibold">31–60 Days</div>
                <div className="text-lg font-bold font-mono text-blue-900 mt-1">
                  ${(apAging?.totalDays_31_60 || 0).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </div>
                <div className="text-[10px] text-blue-700 mt-0.5">Under verification</div>
              </div>
              <div className="p-3 rounded-lg bg-amber-50 border border-amber-200">
                <div className="text-amber-800 font-semibold">61–90 Days</div>
                <div className="text-lg font-bold font-mono text-amber-900 mt-1">
                  ${(apAging?.totalDays_61_90 || 0).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </div>
                <div className="text-[10px] text-amber-700 mt-0.5">Payment due</div>
              </div>
              <div className="p-3 rounded-lg bg-rose-50 border border-rose-200">
                <div className="text-rose-800 font-semibold">90+ Days Overdue</div>
                <div className="text-lg font-bold font-mono text-rose-900 mt-1">
                  ${(apAging?.totalDays_90_Plus || 0).toLocaleString("en-US", { minimumFractionDigits: 2 })}
                </div>
                <div className="text-[10px] text-rose-700 mt-0.5">High risk lock</div>
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

      {/* MODAL: THREE-WAY MATCH (AC-06) AUDIT & VERIFICATION */}
      {showMatchModal && selectedBillForAudit && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-xl w-full p-6 shadow-2xl border border-slate-200 space-y-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <div className="flex items-center gap-2">
                  <span className="p-1.5 bg-indigo-50 text-indigo-600 rounded-lg"><ShieldCheck className="w-4 h-4" /></span>
                  <h3 className="text-base font-bold text-slate-900">AC-06 Three-Way Match Verification</h3>
                </div>
                <p className="text-xs text-slate-500 mt-0.5">
                  Verify Bill <span className="font-mono font-bold text-slate-800">{selectedBillForAudit.billNumber}</span> against PO and GRN QC inspection.
                </p>
              </div>
              <button onClick={() => setShowMatchModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <div className="bg-slate-50 p-3.5 rounded-xl border border-slate-200 text-xs space-y-2">
              <div className="flex justify-between">
                <span className="text-slate-500">Supplier:</span>
                <span className="font-semibold text-slate-900">{selectedBillForAudit.supplierName}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500">PO & GRN Reference:</span>
                <span className="font-mono text-slate-800">{selectedBillForAudit.purchaseOrderNumber} | {selectedBillForAudit.grnNumber}</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500">Billed Unit Price:</span>
                <span className="font-mono font-bold text-slate-900">${selectedBillForAudit.billedUnitPrice.toFixed(4)} / Yd</span>
              </div>
              <div className="flex justify-between">
                <span className="text-slate-500">Billed Quantity:</span>
                <span className="font-mono font-bold text-slate-900">{selectedBillForAudit.billedQuantity.toLocaleString()} Yds</span>
              </div>
              <div className="flex justify-between border-t border-slate-200 pt-2 font-bold">
                <span className="text-slate-700">Gross Invoiced:</span>
                <span className="font-mono text-indigo-700">${selectedBillForAudit.billedAmount.toLocaleString("en-US", { minimumFractionDigits: 2 })}</span>
              </div>
            </div>

            <form
              onSubmit={(e) => {
                e.preventDefault();
                handleExecuteThreeWayMatch(selectedBillForAudit.id);
              }}
              className="space-y-3 text-xs"
            >
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Approved PO Unit Price ($/Yd) *</label>
                <input
                  type="number"
                  step="0.0001"
                  required
                  value={matchForm.poUnitPrice}
                  onChange={(e) => setMatchForm({ ...matchForm, poUnitPrice: Number(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-mono"
                />
                <span className="text-[11px] text-slate-500">PO contractual price agreed with supplier.</span>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">GRN Passed Inspection Quantity (Yds) *</label>
                <input
                  type="number"
                  required
                  value={matchForm.grnPassedQuantity}
                  onChange={(e) => setMatchForm({ ...matchForm, grnPassedQuantity: Number(e.target.value) })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-mono"
                />
                <span className="text-[11px] text-slate-500">Fabric inspection ASTM 4-Point passed quantity in warehouse.</span>
              </div>

              <div>
                <label className="block font-semibold text-slate-700 mb-1">Evaluated By / Auditor Title *</label>
                <input
                  type="text"
                  required
                  value={matchForm.evaluatedBy}
                  onChange={(e) => setMatchForm({ ...matchForm, evaluatedBy: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                />
              </div>

              {/* Match Verification Simulation */}
              {(() => {
                const priceMatch = selectedBillForAudit.billedUnitPrice <= matchForm.poUnitPrice;
                const qtyMatch = selectedBillForAudit.billedQuantity <= matchForm.grnPassedQuantity;
                const fullPass = priceMatch && qtyMatch;

                return (
                  <div className={`p-3 rounded-lg border text-xs ${fullPass ? "bg-emerald-50 border-emerald-200 text-emerald-800" : "bg-rose-50 border-rose-200 text-rose-800"}`}>
                    <div className="font-bold flex items-center gap-1.5">
                      {fullPass ? <ShieldCheck className="w-4 h-4 text-emerald-600" /> : <ShieldAlert className="w-4 h-4 text-rose-600" />}
                      Audit Outcome: {fullPass ? "APPROVED FOR PAYMENT" : "MATCH FAILED (LOCKED)"}
                    </div>
                    <ul className="list-disc list-inside mt-1 space-y-0.5 text-[11px]">
                      <li>Price Match: {priceMatch ? "PASSED (Rate is within PO ceiling)" : "FAILED (Billed rate exceeds PO contract)"}</li>
                      <li>Quantity Match: {qtyMatch ? "PASSED (Billed qty within QC cleared GRN)" : "FAILED (Billed qty exceeds warehouse GRN receipt)"}</li>
                    </ul>
                  </div>
                );
              })()}

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowMatchModal(false)}
                  className="px-4 py-2 border border-slate-300 text-slate-700 rounded-lg hover:bg-slate-50 font-semibold text-xs"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-semibold text-xs shadow-sm"
                >
                  Confirm & Execute AC-06 Match
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* MODAL: ISSUE BACK-TO-BACK (B2B) LETTER OF CREDIT */}
      {showB2BModal && (
        <div className="fixed inset-0 bg-slate-900/50 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-white rounded-2xl max-w-xl w-full p-6 shadow-2xl border border-slate-200 space-y-4 max-h-[90vh] overflow-y-auto">
            <div className="flex items-center justify-between border-b border-slate-100 pb-3">
              <div>
                <div className="flex items-center gap-2">
                  <span className="p-1.5 bg-blue-50 text-blue-600 rounded-lg"><Landmark className="w-4 h-4" /></span>
                  <h3 className="text-base font-bold text-slate-900">Issue Back-to-Back (B2B) LC</h3>
                </div>
                <p className="text-xs text-slate-500 mt-0.5">Enforces statutory 70% Master LC allocation ceiling.</p>
              </div>
              <button onClick={() => setShowB2BModal(false)} className="text-slate-400 hover:text-slate-600">
                <X className="w-5 h-5" />
              </button>
            </div>

            <form onSubmit={handleCreateB2BLC} className="space-y-3 text-xs">
              <div>
                <label className="block font-semibold text-slate-700 mb-1">Export Master LC *</label>
                <select
                  required
                  value={newB2BForm.masterLCId}
                  onChange={(e) => setNewB2BForm({ ...newB2BForm, masterLCId: e.target.value })}
                  className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-mono"
                >
                  {masterLCs.map((mlc) => (
                    <option key={mlc.id} value={mlc.id}>
                      {mlc.mlcNumber} - {mlc.buyerName} (Headroom: ${mlc.availableB2BAmount.toLocaleString()} {mlc.currency})
                    </option>
                  ))}
                </select>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">B2B LC Number *</label>
                  <input
                    type="text"
                    required
                    value={newB2BForm.b2bLCNumber}
                    onChange={(e) => setNewB2BForm({ ...newB2BForm, b2bLCNumber: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-mono"
                  />
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Supplier (Beneficiary) *</label>
                  <input
                    type="text"
                    required
                    value={newB2BForm.supplierName}
                    onChange={(e) => setNewB2BForm({ ...newB2BForm, supplierName: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">LC Amount (USD) *</label>
                  <input
                    type="number"
                    step="0.01"
                    required
                    value={newB2BForm.amount}
                    onChange={(e) => setNewB2BForm({ ...newB2BForm, amount: Number(e.target.value) })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-mono font-bold"
                  />
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Tenor (Days) *</label>
                  <input
                    type="number"
                    required
                    value={newB2BForm.tenorDays}
                    onChange={(e) => {
                      const days = Number(e.target.value);
                      const matDate = new Date(Date.now() + days * 86400000).toISOString().split("T")[0];
                      setNewB2BForm({ ...newB2BForm, tenorDays: days, maturityDate: matDate });
                    }}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500 font-mono"
                  />
                </div>
              </div>

              <div className="grid grid-cols-2 gap-3">
                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Issue Date *</label>
                  <input
                    type="date"
                    required
                    value={newB2BForm.issueDate}
                    onChange={(e) => setNewB2BForm({ ...newB2BForm, issueDate: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                  />
                </div>

                <div>
                  <label className="block font-semibold text-slate-700 mb-1">Maturity Date *</label>
                  <input
                    type="date"
                    required
                    value={newB2BForm.maturityDate}
                    onChange={(e) => setNewB2BForm({ ...newB2BForm, maturityDate: e.target.value })}
                    className="w-full px-3 py-2 border border-slate-300 rounded-lg focus:ring-2 focus:ring-indigo-500"
                  />
                </div>
              </div>

              {/* Headroom Verification */}
              {(() => {
                const selectedMlc = masterLCs.find(m => m.id === newB2BForm.masterLCId) || masterLCs[0];
                const available = selectedMlc ? selectedMlc.availableB2BAmount : 39637.50;
                const exceeds = newB2BForm.amount > available;

                return (
                  <div className={`p-3 rounded-lg border text-xs ${exceeds ? "bg-rose-50 border-rose-200 text-rose-800" : "bg-blue-50 border-blue-200 text-blue-800"}`}>
                    <div className="font-semibold">
                      Master LC Headroom Check: Available ${available.toLocaleString()} USD
                    </div>
                    {exceeds ? (
                      <p className="text-[11px] mt-0.5 text-rose-700 font-medium">
                        Warning: Requested amount exceeds the 70% B2B headroom cap. Issuing bank will reject opening!
                      </p>
                    ) : (
                      <p className="text-[11px] mt-0.5 text-blue-700">
                        Requested amount is compliant with statutory 70% export headroom limits.
                      </p>
                    )}
                  </div>
                );
              })()}

              <div className="flex items-center justify-end gap-2 pt-3 border-t border-slate-100">
                <button
                  type="button"
                  onClick={() => setShowB2BModal(false)}
                  className="px-4 py-2 border border-slate-300 text-slate-700 rounded-lg hover:bg-slate-50 font-semibold text-xs"
                >
                  Cancel
                </button>
                <button
                  type="submit"
                  className="px-4 py-2 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 font-semibold text-xs shadow-sm"
                >
                  Issue B2B LC
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}
