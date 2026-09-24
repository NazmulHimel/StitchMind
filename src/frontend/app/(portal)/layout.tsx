"use client";

import React, { useState, useEffect } from "react";
import { Sidebar } from "@/components/Sidebar";
import { Header } from "@/components/Header";
import { useAuth } from "@/lib/auth-context";
import { useRouter } from "next/navigation";

export default function DashboardLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const { isAuthenticated, isLoading } = useAuth();
  const router = useRouter();

  useEffect(() => {
    if (!isLoading && !isAuthenticated) {
      router.push("/login");
    }
  }, [isLoading, isAuthenticated, router]);

  if (isLoading || !isAuthenticated) {
    return (
      <div style={{ minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center", background: "#f8fafc" }}>
        <p style={{ color: "#64748b", fontSize: "0.875rem", fontWeight: 500 }}>Authenticating session...</p>
      </div>
    );
  }

  return (
    <div className="app-container">
      <Sidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)} />
      <div className="main-wrapper">
        <Header onToggleSidebar={() => setIsSidebarOpen(!isSidebarOpen)} />
        <main className="content-body">{children}</main>
      </div>
    </div>
  );
}
