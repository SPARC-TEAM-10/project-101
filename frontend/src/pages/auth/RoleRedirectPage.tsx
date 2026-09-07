import { Navigate, useNavigate } from "react-router-dom";

import { AuthSplitLayout } from "../../components/AuthSplitLayout";
import { useAuth, type Role } from "../../context/AuthProvider";

const DASHBOARD_ROUTE_BY_ROLE: Partial<Record<Role, string>> = {
  Individual: "/dashboard/individual",
  Guest: "/dashboard/guest",
  Hospital: "/dashboard/hospital",
  Ngo: "/dashboard/ngo",
  SystemAdmin: "/dashboard/admin",
};

// Destination copy per role (CHH-10 AC1/AC2/AC3) — the only place this screen shows that
// redirection is role-based at all. "NGO" is spelled out for the user; "Ngo" is only the
// backend's role constant.
const DESTINATION_LABEL_BY_ROLE: Partial<Record<Role, string>> = {
  Individual: "Opening your Individual dashboard",
  Guest: "Opening your Guest dashboard",
  Hospital: "Opening your Hospital dashboard",
  Ngo: "Opening your NGO dashboard",
  SystemAdmin: "Opening the Admin Command Center",
};

// Brief "Verifying..." transition (CHH-10 UI Notes) between OTP verification and the
// role-appropriate dashboard — a two-step checklist rather than a bare spinner, per the design
// standard's Loaders rule, since a bare spinner also can't show that redirection is role-based.
export function RoleRedirectPage() {
  const { session, clearSession } = useAuth();
  const navigate = useNavigate();

  if (!session) {
    return <Navigate to="/login" replace />;
  }

  const dashboardRoute = DASHBOARD_ROUTE_BY_ROLE[session.role];
  const destinationLabel = DESTINATION_LABEL_BY_ROLE[session.role];

  if (!dashboardRoute || !destinationLabel) {
    return (
      <AuthSplitLayout imageSrc="/images/auth-otp-verify.png" imageAlt="">
        <div className="flex min-h-screen flex-col items-center justify-center bg-sand px-7 font-sans text-ink">
          <div className="flex flex-col items-center text-center">
            <div className="mb-4 flex h-[68px] w-[68px] items-center justify-center rounded-full bg-sand-2 text-ink-3">
              <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M6 3.5v17M6 5.5h11l2.5 3-2.5 3H6" />
                <path d="M6 13.5h9l2.5 3-2.5 3H6" />
              </svg>
            </div>
            <b className="mb-1.5 text-[17px] font-bold tracking-tight">We couldn&apos;t open your dashboard</b>
            <p className="mb-6 max-w-[30ch] text-sm leading-relaxed text-ink-2">
              Your role wasn&apos;t confirmed, so we don&apos;t know which dashboard to open. A fresh sign-in usually clears this.
            </p>
            <div className="flex w-full max-w-[300px] flex-col gap-1.5">
              <button
                type="button"
                onClick={() => navigate("/redirecting", { replace: true })}
                className="h-12 rounded-md bg-clay text-[15px] font-semibold text-white hover:bg-clay-hover"
              >
                Try again
              </button>
              <button
                type="button"
                onClick={() => {
                  clearSession();
                  navigate("/login", { replace: true });
                }}
                className="h-11 rounded-md text-[15px] font-semibold text-clay hover:text-clay-hover"
              >
                Sign in again
              </button>
            </div>
          </div>
        </div>
      </AuthSplitLayout>
    );
  }

  return (
    <AuthSplitLayout imageSrc="/images/auth-otp-verify.png" imageAlt="">
      <div className="flex min-h-screen flex-col bg-sand px-7 pt-14 font-sans text-ink">
        <h1 className="mb-2 text-[26px] font-extrabold tracking-tight">Signing you in</h1>
        <p className="mb-6 max-w-[34ch] text-[14.5px] leading-relaxed text-ink-2">
          Your code checked out. We&apos;re opening the dashboard that matches your role.
        </p>

        <div className="flex flex-col gap-3 rounded-md border border-line bg-cream p-4 shadow-sm">
          <div className="flex min-h-[26px] items-center gap-3">
            <span className="flex h-[22px] w-[22px] flex-none items-center justify-center rounded-full bg-leaf-tint text-leaf">
              <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={3} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M4 12.5l5 5L20 6.5" />
              </svg>
            </span>
            <b className="text-[14.5px] font-semibold text-ink-2">Mobile number verified</b>
          </div>
          <div className="flex min-h-[26px] items-center gap-3">
            <span
              className="h-[22px] w-[22px] flex-none animate-spin rounded-full border-2 border-clay border-r-transparent motion-reduce:animate-none"
              aria-hidden="true"
            />
            <b className="text-[14.5px] font-semibold">{destinationLabel}</b>
          </div>
        </div>

        <p className="mt-4 flex items-center gap-2 text-xs text-ink-2">
          <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true" className="flex-none text-ink-3">
            <circle cx="12" cy="12" r="9" />
            <path d="M12 7.5V12l3 2" />
          </svg>
          Your session stays active for 24 hours.
        </p>

        <Navigate to={dashboardRoute} replace />
      </div>
    </AuthSplitLayout>
  );
}
