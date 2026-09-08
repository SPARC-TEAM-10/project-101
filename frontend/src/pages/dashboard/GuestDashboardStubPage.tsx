import { useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";

interface FeatureTile {
  icon: React.ReactNode;
  title: string;
  description: string;
  locked?: boolean;
  onClick?: () => void;
}

function formatRemaining(expiresAtUtc: string, now: number): string {
  const remainingMs = new Date(expiresAtUtc).getTime() - now;
  if (remainingMs <= 0) {
    return "Session expired";
  }
  const totalMinutes = Math.floor(remainingMs / 60_000);
  const hours = Math.floor(totalMinutes / 60);
  const minutes = totalMinutes % 60;
  if (hours > 0) {
    return `Session expires in ${hours}h ${minutes}m`;
  }
  return `Session expires in ${minutes}m`;
}

function LogoutIcon() {
  return (
    <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M15 17l5-5-5-5M20 12H9M12 20H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h6" />
    </svg>
  );
}

// Guest Dashboard (CHH-11 AC2) — limited permissions: Search Emergency Hub and Request Blood
// only (PRD §4 Role & Permission Matrix). "Search Emergency Hub" stays disabled until CHH-68
// (Emergency Services Hub) ships its frontend — not built yet, tracked as a separate ticket.
// Header/tile-grid layout matches IndividualDashboardPage and FacilityDashboardPage (same
// blood-drop header bar, same feature-tile pattern as the Facility dashboard's locked tiles) so
// a guest session doesn't look like a different, unfinished app. Log out lives in the header
// (not a bottom button) since a guest session has nothing below the fold to push it down to.
export function GuestDashboardStubPage() {
  const navigate = useNavigate();
  const location = useLocation();
  const { session, clearSession } = useAuth();
  const [now, setNow] = useState(() => Date.now());

  // A guest has no persistent request history view (this dashboard is a stub) — the navigation
  // state set by BloodRequestFormModal on success is the only way back to a just-created
  // request's match status (CHH-36).
  const state = location.state as { bloodRequestCreated?: boolean; id?: string } | null;

  useEffect(() => {
    // Same 24h JWT lifetime as every other role (JwtOptions.AccessTokenLifetimeMinutes, no
    // per-role branching in JwtTokenGenerator) — a minute-granularity tick is plenty, no need
    // for per-second precision over a 24h window.
    const interval = setInterval(() => setNow(Date.now()), 60_000);
    return () => clearInterval(interval);
  }, []);

  function handleLogout() {
    clearSession();
    // "/" (HomeRoute) shows the real LandingPage once there's no session — a guest logging out
    // should land back on the marketing page, not skip straight to the mobile-entry form.
    navigate("/");
  }

  const tiles: FeatureTile[] = [
    {
      icon: (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
        </svg>
      ),
      title: "Request Blood",
      description: "Create an emergency blood request now — nearby donors are alerted right away.",
      onClick: () => navigate("/blood-requests/new"),
    },
    {
      icon: (
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-3.5-3.5" />
        </svg>
      ),
      title: "Search Emergency Hub",
      description: "Find nearby hospitals, blood banks, and NGOs offering emergency services.",
      locked: true,
    },
  ];

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:h-[72px] lg:px-8">
        <button
          type="button"
          onClick={() => navigate("/welcome")}
          aria-label="Back"
          className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <svg width="22" height="22" viewBox="0 0 24 24" fill="currentColor" className="text-blood" aria-hidden="true">
          <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
        </svg>
        <b className="text-[15px] font-extrabold tracking-tight">Community Health Hub</b>
        <div className="flex-1" />
        {session && (
          <span className="hidden text-[12.5px] text-ink-3 sm:inline" title={new Date(session.expiresAtUtc).toLocaleString()}>
            {formatRemaining(session.expiresAtUtc, now)}
          </span>
        )}
        <button
          type="button"
          onClick={handleLogout}
          aria-label="Log out"
          className="flex h-9 items-center gap-1.5 rounded-full border border-line-strong bg-cream px-3 text-[13px] font-semibold text-ink transition-colors hover:bg-sand-2"
        >
          <LogoutIcon />
          <span className="hidden sm:inline">Log out</span>
        </button>
      </div>

      <div className="mx-auto flex max-w-3xl flex-col gap-5 px-4 py-5 lg:px-8 lg:py-8">
        <div>
          <h1 className="text-2xl font-extrabold tracking-tight">Guest Access</h1>
          <p className="mt-1 max-w-[52ch] text-[13.5px] leading-relaxed text-ink-2">
            No account needed. You can search for help nearby or request blood directly.
          </p>
          {session && (
            <p className="mt-1 text-[12.5px] text-ink-3 sm:hidden">{formatRemaining(session.expiresAtUtc, now)}</p>
          )}
        </div>

        {state?.bloodRequestCreated && state.id && (
          <button
            type="button"
            onClick={() => navigate(`/blood-requests/${state.id}/matches`)}
            className="flex flex-col gap-1 rounded-md border-[1.5px] border-go-line bg-go-tint p-4 text-left"
          >
            <b className="text-[14.5px] font-bold text-go-deep">Request sent — view match status</b>
            <span className="text-[12.5px] text-ink-2">See how many donors have been notified.</span>
          </button>
        )}

        <div className="grid grid-cols-1 gap-4 sm:grid-cols-2">
          {tiles.map((tile) => (
            <button
              key={tile.title}
              type="button"
              onClick={tile.onClick}
              disabled={tile.locked}
              aria-disabled={tile.locked}
              className={
                tile.locked
                  ? "flex cursor-not-allowed flex-col gap-2 rounded-lg border border-line bg-sand-2 p-5 text-left shadow-none"
                  : "flex flex-col gap-2 rounded-lg border border-line bg-cream p-5 text-left shadow-sm transition-shadow hover:shadow-md"
              }
            >
              <span className={`flex h-10 w-10 items-center justify-center rounded-md ${tile.locked ? "bg-cream text-ink-off" : "bg-clay-tint text-clay-deep"}`}>
                {tile.icon}
              </span>
              <b className={`text-[15.5px] font-bold ${tile.locked ? "text-ink-off" : ""}`}>{tile.title}</b>
              <p className={`text-[12.5px] leading-relaxed ${tile.locked ? "text-ink-off" : "text-ink-2"}`}>{tile.description}</p>
              {tile.locked && (
                <span className="mt-1 inline-flex w-fit items-center gap-1.5 rounded-full border border-line-strong bg-cream px-2.5 py-1 text-[11.5px] font-bold text-ink-2">
                  Coming soon
                </span>
              )}
            </button>
          ))}
        </div>
      </div>
    </div>
  );
}
