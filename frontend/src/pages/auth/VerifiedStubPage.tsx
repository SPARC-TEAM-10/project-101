import { Navigate, useLocation } from "react-router-dom";

interface VerifiedState {
  maskedMobileNumber: string;
  verifiedAtUtc: string;
}

function isVerifiedState(state: unknown): state is VerifiedState {
  return typeof state === "object" && state !== null && "maskedMobileNumber" in state;
}

// Stub for CHH-9 — the real backend endpoint is verify-only (no session token).
// Role-based redirection to the correct dashboard is CHH-10's job.
export function VerifiedStubPage() {
  const location = useLocation();

  if (!isVerifiedState(location.state)) {
    return <Navigate to="/login" replace />;
  }

  const { maskedMobileNumber, verifiedAtUtc } = location.state;

  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-2 bg-sand px-7 font-sans text-ink">
      <h1 className="text-[26px] font-extrabold tracking-tight">Verified</h1>
      <p className="text-center text-[14.5px] text-ink-2">
        {maskedMobileNumber} verified at {new Date(verifiedAtUtc).toLocaleTimeString()}.
        Dashboard routing is coming soon.
      </p>
    </div>
  );
}
