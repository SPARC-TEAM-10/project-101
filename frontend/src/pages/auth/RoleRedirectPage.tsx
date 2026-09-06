import { Navigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";

const DASHBOARD_ROUTE_BY_ROLE = {
  Individual: "/dashboard/individual",
  Guest: "/dashboard/guest",
} as const;

// Brief "Verifying..." transition (CHH-10 UI Notes) between OTP verification and the
// role-appropriate dashboard. Hospital/NGO/Admin aren't resolvable yet (see AuthProvider's
// Role type) so only Individual/Guest are routed here.
export function RoleRedirectPage() {
  const { session } = useAuth();

  if (!session) {
    return <Navigate to="/login" replace />;
  }

  const dashboardRoute = DASHBOARD_ROUTE_BY_ROLE[session.role];
  if (!dashboardRoute) {
    return <Navigate to="/login" replace />;
  }

  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-4 bg-sand font-sans text-ink">
      <svg
        className="h-8 w-8 animate-spin text-clay motion-reduce:animate-none"
        viewBox="0 0 24 24"
        fill="none"
        aria-hidden="true"
      >
        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" />
        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 0 1 8-8v3a5 5 0 0 0-5 5H4Z" />
      </svg>
      <p className="text-sm text-ink-2">Verifying…</p>
      <Navigate to={dashboardRoute} replace />
    </div>
  );
}
