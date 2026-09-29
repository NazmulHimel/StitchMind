"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import {
  Sparkles,
  Lock,
  Mail,
  KeyRound,
  Eye,
  EyeOff,
  CheckCircle2,
  ArrowRight,
  ArrowLeft,
  RefreshCw,
  AlertCircle,
  ShieldCheck,
  Check
} from "lucide-react";
import { apiRequest } from "@/lib/api";

type Step = "EMAIL" | "OTP" | "NEW_PASSWORD" | "SUCCESS";

export default function ForgotPasswordPage() {
  const router = useRouter();

  // State
  const [step, setStep] = useState<Step>("EMAIL");
  const [email, setEmail] = useState("");
  const [otp, setOtp] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [showNewPassword, setShowNewPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  // Status & Timing
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [successInfo, setSuccessInfo] = useState<string | null>(null);
  const [resendCooldown, setResendCooldown] = useState(0);
  const [redirectCountdown, setRedirectCountdown] = useState(3);

  // Resend cooldown timer
  useEffect(() => {
    if (resendCooldown <= 0) return;
    const interval = setInterval(() => {
      setResendCooldown((prev) => (prev > 0 ? prev - 1 : 0));
    }, 1000);
    return () => clearInterval(interval);
  }, [resendCooldown]);

  // Success auto-redirect countdown
  useEffect(() => {
    if (step !== "SUCCESS") return;
    if (redirectCountdown <= 0) {
      router.push("/login");
      return;
    }
    const interval = setInterval(() => {
      setRedirectCountdown((prev) => prev - 1);
    }, 1000);
    return () => clearInterval(interval);
  }, [step, redirectCountdown, router]);

  // Password validation rules
  const hasMinLength = newPassword.length >= 8;
  const hasUppercase = /[A-Z]/.test(newPassword);
  const hasLowercase = /[a-z]/.test(newPassword);
  const hasNumber = /[0-9]/.test(newPassword);
  const hasSpecial = /[^A-Za-z0-9]/.test(newPassword);
  const passwordsMatch = newPassword.length > 0 && newPassword === confirmPassword;
  const isPasswordValid =
    hasMinLength && hasUppercase && hasLowercase && hasNumber && hasSpecial && passwordsMatch;

  // Format cooldown display (e.g. 1m 45s or 45s)
  const formatCooldown = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    if (mins > 0) {
      return `${mins}m ${secs < 10 ? "0" : ""}${secs}s`;
    }
    return `${secs}s`;
  };

  // Step 1: Send OTP to Email
  const handleSendOtp = async (e?: React.FormEvent) => {
    if (e) e.preventDefault();
    if (!email || !email.includes("@")) {
      setErrorMessage("Please enter a valid email address.");
      return;
    }

    if (resendCooldown > 0) {
      setErrorMessage(`An OTP was already sent. Please wait ${formatCooldown(resendCooldown)} before requesting another code.`);
      return;
    }

    setErrorMessage(null);
    setIsLoading(true);

    try {
      const res = await apiRequest("/api/v1/auth/forgot-password", {
        method: "POST",
        body: { email: email.trim().toLowerCase() },
      });

      if (res.isSuccess || res.success) {
        setSuccessInfo("A 6-digit verification code has been sent to your email (valid for 2 minutes).");
        setResendCooldown(120); // 2 minutes cooldown
        setStep("OTP");
      } else {
        const msg = res.error || res.message || "Failed to send verification code. Please try again.";
        setErrorMessage(msg);
        const match = msg.match(/wait (\d+) seconds/i);
        if (match && match[1]) {
          setResendCooldown(parseInt(match[1], 10));
        }
      }
    } catch {
      setErrorMessage("Unable to connect to authentication service. Please check your connection.");
    } finally {
      setIsLoading(false);
    }
  };

  // Step 2: Verify OTP
  const handleVerifyOtp = async (e: React.FormEvent) => {
    e.preventDefault();
    const cleanOtp = otp.trim();
    if (!cleanOtp || cleanOtp.length !== 6) {
      setErrorMessage("Please enter the complete 6-digit OTP code.");
      return;
    }

    setErrorMessage(null);
    setIsLoading(true);

    try {
      const res = await apiRequest("/api/v1/auth/verify-otp", {
        method: "POST",
        body: {
          email: email.trim().toLowerCase(),
          otp: cleanOtp,
        },
      });

      if (res.isSuccess || res.success) {
        setSuccessInfo("OTP verified successfully. Please choose a new password.");
        setStep("NEW_PASSWORD");
      } else {
        setErrorMessage(res.error || res.message || "Invalid or expired OTP code.");
      }
    } catch {
      setErrorMessage("Unable to verify OTP code. Please check your connection.");
    } finally {
      setIsLoading(false);
    }
  };

  // Step 3: Reset Password
  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!isPasswordValid) {
      setErrorMessage("Please ensure all password criteria are satisfied.");
      return;
    }

    setErrorMessage(null);
    setIsLoading(true);

    try {
      const res = await apiRequest("/api/v1/auth/reset-password", {
        method: "POST",
        body: {
          email: email.trim().toLowerCase(),
          otp: otp.trim(),
          newPassword,
          confirmPassword,
        },
      });

      if (res.isSuccess || res.success) {
        setStep("SUCCESS");
        setRedirectCountdown(3);
      } else {
        setErrorMessage(res.error || res.message || "Failed to change password. Please try again.");
      }
    } catch {
      setErrorMessage("Unable to complete password reset. Please try again.");
    } finally {
      setIsLoading(false);
    }
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
        position: "relative",
      }}
    >
      <div
        className="glass-panel"
        style={{
          width: "100%",
          maxWidth: 480,
          padding: 36,
          borderRadius: 20,
          boxShadow: "0 20px 25px -5px rgba(0, 0, 0, 0.08), 0 8px 10px -6px rgba(0, 0, 0, 0.04)",
          background: "#ffffff",
          border: "1px solid #e2e8f0",
        }}
      >
        {/* Header Logo */}
        <div style={{ textAlign: "center", marginBottom: 24 }}>
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
              boxShadow: "0 4px 14px rgba(37, 99, 235, 0.35)",
            }}
          >
            <Sparkles size={28} />
          </div>
          <h1 style={{ fontSize: "1.5rem", fontWeight: 800, color: "#0f172a" }}>
            Stitch<span className="gradient-text">Mind</span>
          </h1>
        </div>

        {/* Step Progress Bar (Hidden on success) */}
        {step !== "SUCCESS" && (
          <div style={{ marginBottom: 24 }}>
            <div style={{ display: "flex", justifyContent: "space-between", marginBottom: 8, fontSize: "0.78rem", fontWeight: 600, color: "#64748b" }}>
              <span style={{ color: step === "EMAIL" ? "var(--primary)" : "#0f172a" }}>1. Email</span>
              <span style={{ color: step === "OTP" ? "var(--primary)" : (step === "NEW_PASSWORD" ? "#0f172a" : "#94a3b8") }}>2. Verify Code</span>
              <span style={{ color: step === "NEW_PASSWORD" ? "var(--primary)" : "#94a3b8" }}>3. New Password</span>
            </div>
            <div style={{ height: 4, background: "#e2e8f0", borderRadius: 2, overflow: "hidden" }}>
              <div
                style={{
                  height: "100%",
                  background: "var(--primary)",
                  borderRadius: 2,
                  transition: "width 0.3s ease",
                  width: step === "EMAIL" ? "33%" : step === "OTP" ? "66%" : "100%",
                }}
              />
            </div>
          </div>
        )}

        {/* Error Notification */}
        {errorMessage && (
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
              gap: 8,
            }}
          >
            <AlertCircle size={16} style={{ flexShrink: 0 }} />
            <span>{errorMessage}</span>
          </div>
        )}

        {/* Info Notification */}
        {successInfo && step !== "SUCCESS" && (
          <div
            style={{
              padding: "10px 14px",
              borderRadius: 8,
              background: "#eff6ff",
              border: "1px solid #bfdbfe",
              color: "#1d4ed8",
              fontSize: "0.85rem",
              marginBottom: 20,
              display: "flex",
              alignItems: "center",
              gap: 8,
            }}
          >
            <ShieldCheck size={16} style={{ flexShrink: 0 }} />
            <span>{successInfo}</span>
          </div>
        )}

        {/* ==================== STEP 1: EMAIL ==================== */}
        {step === "EMAIL" && (
          <div>
            <div style={{ marginBottom: 20 }}>
              <h2 style={{ fontSize: "1.18rem", fontWeight: 700, color: "#1e293b", margin: 0 }}>
                Reset your password
              </h2>
              <p style={{ color: "#64748b", fontSize: "0.85rem", marginTop: 4 }}>
                Enter the email address registered with your account and we will send you a 6-digit OTP verification code (valid for 2 minutes).
              </p>
            </div>

            <form onSubmit={handleSendOtp} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
              <div>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, color: "#475569", marginBottom: 6 }}>
                  Email Address
                </label>
                <div style={{ position: "relative" }}>
                  <Mail
                    size={18}
                    style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "#94a3b8" }}
                  />
                  <input
                    type="email"
                    required
                    autoFocus
                    value={email}
                    onChange={(e) => setEmail(e.target.value)}
                    placeholder="e.g. user@garmentserp.com"
                    style={{
                      width: "100%",
                      padding: "10px 14px 10px 38px",
                      background: "#ffffff",
                      border: "1px solid #cbd5e1",
                      borderRadius: 8,
                      color: "#0f172a",
                      fontSize: "0.9rem",
                    }}
                  />
                </div>
              </div>

              <button
                type="submit"
                disabled={isLoading}
                className="btn btn-primary"
                style={{
                  width: "100%",
                  padding: "12px",
                  fontSize: "0.95rem",
                  marginTop: 4,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  gap: 8,
                }}
              >
                <span>{isLoading ? "Sending Code..." : "Send Verification Code"}</span>
                <ArrowRight size={16} />
              </button>

              <div style={{ textAlign: "center", marginTop: 8 }}>
                <Link
                  href="/login"
                  style={{
                    color: "#64748b",
                    fontSize: "0.84rem",
                    fontWeight: 600,
                    textDecoration: "none",
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 6,
                  }}
                >
                  <ArrowLeft size={14} />
                  <span>Back to Sign In</span>
                </Link>
              </div>
            </form>
          </div>
        )}

        {/* ==================== STEP 2: VERIFY OTP ==================== */}
        {step === "OTP" && (
          <div>
            <div style={{ marginBottom: 20 }}>
              <h2 style={{ fontSize: "1.18rem", fontWeight: 700, color: "#1e293b", margin: 0 }}>
                Enter verification code
              </h2>
              <p style={{ color: "#64748b", fontSize: "0.85rem", marginTop: 4 }}>
                We sent a 6-digit code to <strong style={{ color: "#0f172a" }}>{email}</strong>. It is valid for <strong style={{ color: "#2563eb" }}>2 minutes</strong>.
                {" "}
                <button
                  type="button"
                  onClick={() => {
                    setStep("EMAIL");
                    setErrorMessage(null);
                    setSuccessInfo(null);
                  }}
                  style={{
                    background: "none",
                    border: "none",
                    color: "var(--primary)",
                    fontWeight: 600,
                    cursor: "pointer",
                    padding: 0,
                    fontSize: "0.85rem",
                    textDecoration: "underline",
                  }}
                >
                  Change
                </button>
              </p>
            </div>

            <form onSubmit={handleVerifyOtp} style={{ display: "flex", flexDirection: "column", gap: 18 }}>
              <div>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, color: "#475569", marginBottom: 6 }}>
                  6-Digit OTP Code
                </label>
                <div style={{ position: "relative" }}>
                  <KeyRound
                    size={18}
                    style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "#94a3b8" }}
                  />
                  <input
                    type="text"
                    required
                    autoFocus
                    maxLength={6}
                    value={otp}
                    onChange={(e) => {
                      const val = e.target.value.replace(/[^0-9]/g, "");
                      setOtp(val);
                    }}
                    placeholder="123456"
                    style={{
                      width: "100%",
                      padding: "10px 14px 10px 38px",
                      background: "#ffffff",
                      border: "1px solid #cbd5e1",
                      borderRadius: 8,
                      color: "#0f172a",
                      fontSize: "1.25rem",
                      letterSpacing: "6px",
                      fontWeight: 700,
                    }}
                  />
                </div>
              </div>

              <div style={{ display: "flex", alignItems: "center", justifyContent: "space-between", fontSize: "0.82rem" }}>
                <span style={{ color: "#64748b" }}>Didn&apos;t receive code?</span>
                {resendCooldown > 0 ? (
                  <span style={{ color: "#2563eb", fontWeight: 600, background: "#eff6ff", padding: "3px 8px", borderRadius: 4, border: "1px solid #bfdbfe" }}>
                    Resend in {formatCooldown(resendCooldown)}
                  </span>
                ) : (
                  <button
                    type="button"
                    onClick={() => handleSendOtp()}
                    disabled={isLoading}
                    style={{
                      background: "none",
                      border: "none",
                      color: "var(--primary)",
                      fontWeight: 600,
                      cursor: "pointer",
                      display: "flex",
                      alignItems: "center",
                      gap: 4,
                    }}
                  >
                    <RefreshCw size={13} />
                    <span>Resend Code</span>
                  </button>
                )}
              </div>

              <button
                type="submit"
                disabled={isLoading || otp.length !== 6}
                className="btn btn-primary"
                style={{
                  width: "100%",
                  padding: "12px",
                  fontSize: "0.95rem",
                  marginTop: 4,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  gap: 8,
                }}
              >
                <span>{isLoading ? "Verifying..." : "Verify Code"}</span>
                <ArrowRight size={16} />
              </button>

              <div style={{ textAlign: "center", marginTop: 4 }}>
                <button
                  type="button"
                  onClick={() => {
                    setStep("EMAIL");
                    setErrorMessage(null);
                    setSuccessInfo(null);
                  }}
                  style={{
                    background: "none",
                    border: "none",
                    color: "#64748b",
                    fontSize: "0.84rem",
                    fontWeight: 600,
                    cursor: "pointer",
                    display: "inline-flex",
                    alignItems: "center",
                    gap: 6,
                  }}
                >
                  <ArrowLeft size={14} />
                  <span>Back to Email</span>
                </button>
              </div>
            </form>
          </div>
        )}

        {/* ==================== STEP 3: NEW PASSWORD ==================== */}
        {step === "NEW_PASSWORD" && (
          <div>
            <div style={{ marginBottom: 20 }}>
              <h2 style={{ fontSize: "1.18rem", fontWeight: 700, color: "#1e293b", margin: 0 }}>
                Set new password
              </h2>
              <p style={{ color: "#64748b", fontSize: "0.85rem", marginTop: 4 }}>
                Choose a strong password to secure your account.
              </p>
            </div>

            <form onSubmit={handleResetPassword} style={{ display: "flex", flexDirection: "column", gap: 16 }}>
              <div>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, color: "#475569", marginBottom: 6 }}>
                  New Password
                </label>
                <div style={{ position: "relative" }}>
                  <Lock
                    size={18}
                    style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "#94a3b8" }}
                  />
                  <input
                    type={showNewPassword ? "text" : "password"}
                    required
                    autoFocus
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    placeholder="New secure password"
                    style={{
                      width: "100%",
                      padding: "10px 38px 10px 38px",
                      background: "#ffffff",
                      border: "1px solid #cbd5e1",
                      borderRadius: 8,
                      color: "#0f172a",
                      fontSize: "0.9rem",
                    }}
                  />
                  <button
                    type="button"
                    onClick={() => setShowNewPassword(!showNewPassword)}
                    style={{
                      position: "absolute",
                      right: 12,
                      top: "50%",
                      transform: "translateY(-50%)",
                      background: "none",
                      border: "none",
                      color: "#94a3b8",
                      cursor: "pointer",
                      display: "flex",
                    }}
                  >
                    {showNewPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              <div>
                <label style={{ display: "block", fontSize: "0.82rem", fontWeight: 600, color: "#475569", marginBottom: 6 }}>
                  Confirm New Password
                </label>
                <div style={{ position: "relative" }}>
                  <Lock
                    size={18}
                    style={{ position: "absolute", left: 12, top: "50%", transform: "translateY(-50%)", color: "#94a3b8" }}
                  />
                  <input
                    type={showConfirmPassword ? "text" : "password"}
                    required
                    value={confirmPassword}
                    onChange={(e) => setConfirmPassword(e.target.value)}
                    placeholder="Repeat new password"
                    style={{
                      width: "100%",
                      padding: "10px 38px 10px 38px",
                      background: "#ffffff",
                      border: "1px solid #cbd5e1",
                      borderRadius: 8,
                      color: "#0f172a",
                      fontSize: "0.9rem",
                    }}
                  />
                  <button
                    type="button"
                    onClick={() => setShowConfirmPassword(!showConfirmPassword)}
                    style={{
                      position: "absolute",
                      right: 12,
                      top: "50%",
                      transform: "translateY(-50%)",
                      background: "none",
                      border: "none",
                      color: "#94a3b8",
                      cursor: "pointer",
                      display: "flex",
                    }}
                  >
                    {showConfirmPassword ? <EyeOff size={18} /> : <Eye size={18} />}
                  </button>
                </div>
              </div>

              {/* Password Requirements Checklist */}
              <div
                style={{
                  background: "#f8fafc",
                  border: "1px solid #e2e8f0",
                  borderRadius: 8,
                  padding: "10px 14px",
                  fontSize: "0.78rem",
                  display: "grid",
                  gridTemplateColumns: "1fr 1fr",
                  gap: 6,
                }}
              >
                <RequirementItem label="8+ Characters" satisfied={hasMinLength} />
                <RequirementItem label="1 Uppercase" satisfied={hasUppercase} />
                <RequirementItem label="1 Lowercase" satisfied={hasLowercase} />
                <RequirementItem label="1 Number" satisfied={hasNumber} />
                <RequirementItem label="1 Special char" satisfied={hasSpecial} />
                <RequirementItem label="Passwords match" satisfied={passwordsMatch} />
              </div>

              <button
                type="submit"
                disabled={isLoading || !isPasswordValid}
                className="btn btn-primary"
                style={{
                  width: "100%",
                  padding: "12px",
                  fontSize: "0.95rem",
                  marginTop: 6,
                  display: "flex",
                  alignItems: "center",
                  justifyContent: "center",
                  gap: 8,
                  opacity: isPasswordValid ? 1 : 0.65,
                }}
              >
                <span>{isLoading ? "Updating Password..." : "Change Password"}</span>
                <CheckCircle2 size={16} />
              </button>
            </form>
          </div>
        )}

        {/* ==================== STEP 4: SUCCESS ==================== */}
        {step === "SUCCESS" && (
          <div style={{ textAlign: "center", padding: "10px 0" }}>
            <div
              style={{
                width: 64,
                height: 64,
                borderRadius: "50%",
                background: "#dcfce7",
                color: "#16a34a",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                margin: "0 auto 16px",
              }}
            >
              <CheckCircle2 size={36} />
            </div>

            <h2 style={{ fontSize: "1.3rem", fontWeight: 700, color: "#0f172a", marginBottom: 8 }}>
              Password Changed Successfully!
            </h2>
            <p style={{ color: "#64748b", fontSize: "0.88rem", lineHeight: 1.5, marginBottom: 24 }}>
              Your password has been updated. You can now sign in using your new credentials.
            </p>

            <div
              style={{
                padding: "10px 14px",
                background: "#f1f5f9",
                borderRadius: 8,
                fontSize: "0.82rem",
                color: "#475569",
                marginBottom: 20,
              }}
            >
              Redirecting to login page in <strong>{redirectCountdown}</strong> seconds...
            </div>

            <button
              type="button"
              onClick={() => router.push("/login")}
              className="btn btn-primary"
              style={{
                width: "100%",
                padding: "12px",
                fontSize: "0.95rem",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                gap: 8,
              }}
            >
              <span>Sign In Now</span>
              <ArrowRight size={16} />
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

function RequirementItem({ label, satisfied }: { label: string; satisfied: boolean }) {
  return (
    <div
      style={{
        display: "flex",
        alignItems: "center",
        gap: 6,
        color: satisfied ? "#16a34a" : "#94a3b8",
        fontWeight: satisfied ? 600 : 400,
        transition: "color 0.2s ease",
      }}
    >
      <div
        style={{
          width: 14,
          height: 14,
          borderRadius: "50%",
          display: "flex",
          alignItems: "center",
          justifyContent: "center",
          background: satisfied ? "#dcfce7" : "#e2e8f0",
        }}
      >
        <Check size={10} style={{ strokeWidth: 3 }} />
      </div>
      <span>{label}</span>
    </div>
  );
}
