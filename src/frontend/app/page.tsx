"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/lib/auth-context";

export default function HomePage() {
  const router = useRouter();
  const { isAuthenticated, isLoading } = useAuth();

  useEffect(() => {
    if (!isLoading) {
      if (isAuthenticated) {
        router.push("/dashboard");
      } else {
        router.push("/login");
      }
    }
  }, [isAuthenticated, isLoading, router]);

  return (
    <div style={{ minHeight: "100vh", display: "flex", alignItems: "center", justifyContent: "center" }}>
      <div style={{ textAlign: "center" }}>
        <div className="sidebar-brand-icon" style={{ margin: "0 auto 16px", width: 48, height: 48 }}>
          ★
        </div>
        <h2 style={{ fontSize: "1.2rem", fontWeight: 700 }}>StitchMind</h2>
        <p style={{ color: "var(--text-muted)", fontSize: "0.85rem", marginTop: 4 }}>Loading enterprise modules...</p>
      </div>
    </div>
  );
}
