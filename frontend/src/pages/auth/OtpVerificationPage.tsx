import { Navigate, useLocation } from "react-router-dom";

interface OtpVerificationState {
  mobileNumber: string;
  maskedMobileNumber: string;
  otpExpiresAtUtc: string;
  resendAvailableAtUtc: string;
}

function isOtpVerificationState(state: unknown): state is OtpVerificationState {
  return (
    typeof state === "object" &&
    state !== null &&
    "maskedMobileNumber" in state
  );
}

// Stub for CHH-8 — full 6-digit entry, resend timer, and verification logic
// is CHH-9 (OTP Verification & Resend Logic).
export function OtpVerificationPage() {
  const location = useLocation();

  if (!isOtpVerificationState(location.state)) {
    return <Navigate to="/login" replace />;
  }

  const { maskedMobileNumber } = location.state;

  return (
    <div className="flex min-h-screen flex-col items-center justify-center bg-sand px-7 font-sans text-ink">
      <h1 className="mb-2 text-[26px] font-extrabold tracking-tight">Verify your number</h1>
      <p className="max-w-[30ch] text-center text-[14.5px] text-ink-2">
        We sent a code to {maskedMobileNumber}. OTP entry is coming soon.
      </p>
    </div>
  );
}
