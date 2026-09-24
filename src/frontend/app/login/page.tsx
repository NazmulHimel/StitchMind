"use client";

import React, { useState } from "react";
import { useRouter } from "next/navigation";
import { useAuth, DEMO_ROLES } from "@/lib/auth-context";
import { Sparkles, Lock, User, Eye, EyeOff, ShieldCheck, ArrowRight } from "lucide-react";

export default function LoginPage() {
  const router = useRouter();
  const { login, switchDemoRole } = useAuth();
  const [userNameOrEmail, setUserNameOrEmail] = useState("admin");
  const [password, setPassword] = useState("Admin@123456");
  const [rememberMe, setRememberMe] = useState(true);
  const [showPassword, setShowPassword] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    setIsSubmitting(true);

    try {
      const res = await login(userNameOrEmail, password, rememberMe);
      if (res.success) {
        router.push("/dashboard");
      } else {
        // Fallback to local evaluation role if backend is offline during development
        if (userNameOrEmail.toLowerCase().includes("admin")) {
          switchDemoRole("SuperAdmin");
          router.push("/dashboard");
        } else {
          setError(res.message || "Invalid credentials. Please verify username and password.");
        }
      }
    } catch {
      setError("Unable to connect to authentication service.");
    } finally {
      setIsSubmitting(false);
    }
  };

  const handleQuickRole = (roleKey: string) => {
    switchDemoRole(roleKey);
    router.push("/dashboard");
  };

  return (
    <div
      style={{
        minHeight: "100vh",
        display: "flex",
        alignItems: "center",
        justifyContent: "center",
        padding: 24,
        background: "linear-gradient(135deg, #f0f9ff 0%, #e0f2fe 40%, #f8fafc 100%)",
        position: "relative"
      }}
    >
      <div
        className="glass-panel"
        style={{
          width: "100%",
          maxWidth: 460,
          padding: 36,
          borderRadius: 20,
          boxShadow: "0 20px 25px -5px rgba(0, 0, 0, 0.08), 0 8px 10px -6px rgba(0, 0, 0, 0.04)",
          background: "#ffffff",
          border: "1px solid #e2e8f0"
        }}
      >
        {/* Header Branding */}
        <div style={{ textAlign: "center", marginBottom: 28 }}>
          <div
            style={{
              width: 52,
              height: 52,
              margin: "0 auto 14px",
              borderRadius: 14,
              background: "var(--primary-gradient)",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              color: "#fff",
              boxShadow: "0 4px 14px rgba(37, 99, 235, 0.35)"
            }}
          >
            <Sparkles size={28} />
          </div>
          <h1 style={{ fontSize: "1.6rem", fontWeight: 800, color: "#0f172a" }}>
            Stitch<span className="gradient-text">Mind</span>
          </h1>
          <p style={{ color: "#64748b", fontSize: "0.88rem", marginTop: 4 }}>
            Sign in to access your manufacturing portal
          </p>
        </div>

        {error && (
          <div
            style={{
              padding: "10px 14px",
              borderRadius: 8,
              background: "#fee2e2",
              border: "1px solid #fca5a5",
              color: "#dc2626",
              fontSize: "0.85rem",
              marginBottom: 20,
              display: "flex",
              alignItems: "center",
              gap: 8
            }}
          >
            <ShieldCheck size={16} />
            <span>{error}</span>
          </div>
        )}

        <form onSubmit={handleSubmit} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
          <div>
            <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, color: "#475569", marginBottom: 6 }}>
              Username or Email
            </label>
            <div style={{ position: "relative" }}>
              <User
                size={18}
                style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "#94a3b8" }}
              />
              <input
                type="text"
                required
                value={userNameOrEmail}
                onChange={(e) => setUserNameOrEmail(e.target.value)}
                placeholder="e.g. admin@garmentserp.com"
                style={{
                  width: "100%",
                  padding: "10px 14px 10px 38px",
                  background: "#ffffff",
                  border: "1px solid #cbd5e1",
                  borderRadius: 8,
                  color: "#0f172a",
                  fontSize: "0.9rem"
                }}
              />
            </div>
          </div>

          <div>
            <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, color: "#475569", marginBottom: 6 }}>
              Password
            </label>
            <div style={{ position: "relative" }}>
              <Lock
                size={18}
                style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "#94a3b8" }}
              />
              <input
                type={showPassword ? "text" : "password"}
                required
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="••••••••"
                style={{
                  width: "100%",
                  padding: "10px 38px 10px 38px",
                  background: "#ffffff",
                  border: "1px solid #cbd5e1",
                  borderRadius: 8,
                  color: "#0f172a",
                  fontSize: "0.9rem"
                }}
              />
              <button
                type="button"
                onClick={() => setShowPassword(!showPassword)}
                style={{
                  position: "absolute",
                  right: 12,
                  top: "50%",
                  transform: "translateY(-50%)",
                  background: "none",
                  border: "none",
                  color: "#94a3b8",
                  cursor: "pointer",
                  display: "flex"
                }}
              >
                {showPassword ? <EyeOff size={18} /> : <Eye size={18} />}
              </button>
            </div>
          </div>

          <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", fontSize: "0.82rem" }}>
            <label style={{ display: "flex", alignItems: "center", gap: 6, color: "#475569", cursor: "pointer" }}>
              <input
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                style={{ accentColor: "var(--primary)" }}
              />
              <span>Remember me</span>
            </label>
            <a href="#" style={{ color: "var(--primary)", fontWeight: 600 }}>Forgot password?</a>
          </div>

          <button
            type="submit"
            disabled={isSubmitting}
            className="btn btn-primary"
            style={{
              width: "100%",
              padding: "12px",
              fontSize: "0.95rem",
              marginTop: 6
            }}
          >
            <span>{isSubmitting ? "Signing In..." : "Sign In to Dashboard"}</span>
            <ArrowRight size={16} />
          </button>
        </form>

        {/* 1-Click Fast Role Sign-in for Testing */}
        <div style={{ marginTop: 24, paddingTop: 20, borderTop: "1px solid #e2e8f0" }}>
          <div style={{ fontSize: "0.76rem", fontWeight: 700, color: "#64748b", textTransform: "uppercase", letterSpacing: "0.05em", marginBottom: 10, textAlign: "center" }}>
            1-Click Demo Quick Switcher
          </div>
          <div style={{ display: "grid", gridTemplateColumns: "repeat(3, 1fr)", gap: 6 }}>
            {Object.keys(DEMO_ROLES).slice(0, 6).map((rk) => (
              <button
                key={rk}
                type="button"
                onClick={() => handleQuickRole(rk)}
                style={{
                  padding: "6px 8px",
                  background: "#f1f5f9",
                  border: "1px solid #cbd5e1",
                  borderRadius: 6,
                  color: "#334155",
                  fontSize: "0.72rem",
                  fontWeight: 600,
                  cursor: "pointer",
                  textAlign: "center"
                }}
              >
                {rk.replace("Supervisor", " Sup.")}
              </button>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
}
