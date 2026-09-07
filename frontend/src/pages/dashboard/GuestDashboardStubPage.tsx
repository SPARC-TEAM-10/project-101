import { Link } from "react-router-dom";

// Placeholder for the real Guest Dashboard — a future ticket. Reachable only via
// RequireAuth roles={["Guest"]} (see router.tsx).
export function GuestDashboardStubPage() {
  return (
    <div className="flex min-h-screen flex-col items-center justify-center gap-3 bg-sand px-7 font-sans text-ink">
      <h1 className="text-[26px] font-extrabold tracking-tight">Guest Dashboard</h1>
      <p className="text-center text-[14.5px] text-ink-2">Coming soon.</p>
      <Link
        to="/register/individual"
        className="mt-2 h-11 rounded-md bg-clay px-5 text-sm font-semibold leading-[44px] text-white hover:bg-clay-hover"
      >
        Complete your profile
      </Link>
    </div>
  );
}
