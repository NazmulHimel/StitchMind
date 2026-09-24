"use client";

import React from "react";
import { FileBarChart, Download, FileText, CheckCircle } from "lucide-react";

export default function ReportsPage() {
  const reports = [
    { title: "Daily Plant Production Summary", code: "RPT-001", engine: "QuestPDF C# Engine", module: "Production", format: "PDF & Excel" },
    { title: "Cutting Lay Plan & Yield Sheet", code: "RPT-002", engine: "QuestPDF C# Engine", module: "Cutting", format: "PDF" },
    { title: "Supplier Goods Receipt (GRN) Document", code: "RPT-003", engine: "QuestPDF C# Engine", module: "Procurement", format: "PDF" },
    { title: "AQL 2.5 Buyer Quality Certificate", code: "RPT-004", engine: "QuestPDF C# Engine", module: "Quality Control", format: "PDF" },
    { title: "Financial Profit & Loss Statement", code: "RPT-005", engine: "QuestPDF C# Engine", module: "Finance", format: "PDF & Excel" }
  ];

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: "1.4rem", fontWeight: 800 }}>QuestPDF High-Performance Reports</h2>
        <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
          Generate pixel-perfect manufacturing reports, commercial invoices, and production certificates via QuestPDF.
        </p>
      </div>

      <div className="card" style={{ padding: 24 }}>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Report Code</th>
                <th>Report Name</th>
                <th>Generating Engine</th>
                <th>Originating Module</th>
                <th>Formats Available</th>
                <th>Action</th>
              </tr>
            </thead>
            <tbody>
              {reports.map((r) => (
                <tr key={r.code}>
                  <td style={{ fontWeight: 700, color: "var(--text-accent)" }}>{r.code}</td>
                  <td style={{ fontWeight: 600 }}>{r.title}</td>
                  <td>{r.engine}</td>
                  <td>{r.module}</td>
                  <td>{r.format}</td>
                  <td>
                    <button className="btn btn-secondary" style={{ padding: "6px 12px", fontSize: "0.78rem" }}>
                      <Download size={14} />
                      <span>Generate</span>
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
