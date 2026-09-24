"use client";

import React from "react";
import { Users, Fingerprint, CreditCard } from "lucide-react";

export default function HRPage() {
  const staff = [
    { empId: "EMP-1044", name: "Md. Karim Ullah", dept: "Sewing Section", designation: "Senior Machine Operator", shift: "Shift A (Morning)", status: "Present" },
    { empId: "EMP-1045", name: "Fatema Begum", dept: "Cutting Section", designation: "Marker Pattern Maker", shift: "Shift A (Morning)", status: "Present" },
    { empId: "EMP-1046", name: "Tariqul Islam", dept: "Quality Assurance", designation: "In-Line QC Inspector", shift: "Shift A (Morning)", status: "Present" }
  ];

  return (
    <div>
      <div style={{ marginBottom: 24 }}>
        <h2 style={{ fontSize: "1.4rem", fontWeight: 800 }}>Human Resources & Biometric Attendance</h2>
        <p style={{ color: "var(--text-secondary)", fontSize: "0.85rem", marginTop: 4 }}>
          Employee records, automated biometric device integration, shift scheduling, and salary calculation.
        </p>
      </div>

      <div className="card" style={{ padding: 24 }}>
        <div className="table-container">
          <table className="data-table">
            <thead>
              <tr>
                <th>Emp ID</th>
                <th>Full Name</th>
                <th>Department</th>
                <th>Designation</th>
                <th>Assigned Shift</th>
                <th>Today Attendance</th>
              </tr>
            </thead>
            <tbody>
              {staff.map((s) => (
                <tr key={s.empId}>
                  <td style={{ fontWeight: 700, color: "var(--text-accent)" }}>{s.empId}</td>
                  <td style={{ fontWeight: 600 }}>{s.name}</td>
                  <td>{s.dept}</td>
                  <td>{s.designation}</td>
                  <td>{s.shift}</td>
                  <td>
                    <span className="badge badge-success">{s.status}</span>
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
