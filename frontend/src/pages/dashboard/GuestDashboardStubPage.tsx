import { useLocation, useNavigate } from "react-router-dom";

// Guest Dashboard (CHH-11 AC2) — limited permissions: Search Emergency Hub and Request Blood
// only (PRD §4 Role & Permission Matrix). "Search Emergency Hub" stays disabled until CHH-68
// (Emergency Services Hub) ships its frontend — not built yet, tracked as a separate ticket.
export function GuestDashboardStubPage() {
  const navigate = useNavigate();
  const location = useLocation();
  // A guest has no persistent request history view (this dashboard is a stub) — the navigation
  // state set by BloodRequestFormModal on success is the only way back to a just-created
  // request's match status (CHH-36).
  const state = location.state as { bloodRequestCreated?: boolean; id?: string } | null;

  return (
    <div className="flex min-h-screen flex-col bg-sand px-7 pt-16 font-sans text-ink">
      <h1 className="mb-2 text-center text-[26px] font-extrabold tracking-tight">Guest Access</h1>
      <p className="mb-10 text-center text-[14.5px] leading-relaxed text-ink-2">
        No account needed. You can search for help nearby or request blood directly.
      </p>

      {state?.bloodRequestCreated && state.id && (
        <button
          type="button"
          onClick={() => navigate(`/blood-requests/${state.id}/matches`)}
          className="mb-4 flex flex-col gap-1 rounded-md border-[1.5px] border-go-line bg-go-tint p-4 text-left"
        >
          <b className="text-[14.5px] font-bold text-go-deep">Request sent — view match status</b>
          <span className="text-[12.5px] text-ink-2">See how many donors have been notified.</span>
        </button>
      )}

      <div className="flex flex-col gap-4">
        <button
          type="button"
          onClick={() => navigate("/blood-requests/new")}
          className="flex flex-col gap-1.5 rounded-md border-[1.5px] border-transparent bg-clay p-6 text-left text-white shadow-[var(--e1)]"
        >
          <span className="flex items-center gap-3">
            <span className="flex h-9.5 w-9.5 flex-none items-center justify-center rounded-full bg-white/[.18]">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
              </svg>
            </span>
            <b className="text-[17px] font-extrabold tracking-tight">Request Blood</b>
          </span>
          <span className="text-[13px] opacity-85">Create an emergency blood request now.</span>
        </button>

        <button
          type="button"
          disabled
          aria-disabled="true"
          className="flex cursor-not-allowed flex-col gap-1.5 rounded-md border-[1.5px] border-line-strong bg-transparent p-6 text-left text-ink-off"
        >
          <span className="flex items-center gap-3">
            <span className="flex h-9.5 w-9.5 flex-none items-center justify-center rounded-full bg-sand-2 text-ink-off">
              <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <circle cx="11" cy="11" r="7" />
                <path d="m20 20-3.5-3.5" />
              </svg>
            </span>
            <b className="text-[17px] font-extrabold tracking-tight">Search Emergency Hub</b>
          </span>
          <span className="text-[13px] opacity-85">Coming soon.</span>
        </button>
      </div>
    </div>
  );
}
