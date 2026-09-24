"use client";

import React from "react";
import { DollarSign, BookOpen, FileSpreadsheet } from "lucide-react";

export default function FinancePage() {
  const ledger = [
    { voucher: "JV-2026-0045", account: "1010 - Cash & Bank Accounts", debit: "$45,000.00", credit: "-", date: "2026-09-24", memo: "LC Payment Received from H&M" },
    { voucher: "PV-2026-0089", account: "2010 - Accounts Payable (Pioneer Denim)", debit: "-", credit: "$38,250.00", date: "2026-09-23", memo: "Fabric invoice GRN-0891" },
    { voucher: "PR-2026-0012", account: "5010 - Direct Labor Wages Expense", debit: "$68,400.00", credit: "-", date: "2026-09-20", memo: "Bi-Weekly Factory Wages" }
  ];

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: "1.4rem", fontWeight: 800 }}>Accounting & Financial Ledger</h2>
        <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
          General Ledger, Journal Vouchers, Accounts Payable (AP), Accounts Receivable (AR), and Trial Balance.
        </p>
      </div>

      <div className="card" style={{ padding: 24 }}>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Voucher #</th>
                <th>Account Title</th>
                <th>Debit ($)</th>
                <th>Credit ($)</th>
                <th>Date</th>
                <th>Transaction Memo</th>
              </tr>
            </thead>
            <tbody>
              {ledger.map((l) => (
                <tr key={l.voucher}>
                  <td style={{ fontWeight: 700, color: "var(--text-accent)" }}>{l.voucher}</td>
                  <td style={{ fontWeight: 600 }}>{l.account}</td>
                  <td style={{ fontWeight: 700, color: "var(--success)" }}>{l.debit}</td>
                  <td style={{ fontWeight: 700, color: "var(--danger)" }}>{l.credit}</td>
                  <td style={{ color: "var(--text-muted)" }}>{l.date}</td>
                  <td>{l.memo}</td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
