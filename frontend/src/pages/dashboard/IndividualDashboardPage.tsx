import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import type { BloodRequestDto } from "../../api/bloodRequestApi";
import { useIndividualDashboard } from "../../features/dashboard/useIndividualDashboard";
import { useNotifications } from "../../features/notifications/useNotifications";

// "Home" and "My requests" both resolve on this same page (request history already lives here —
// "My requests" jumps to that section) since there's no separate page for it yet. Events/
// Emergency services stay disabled: CHH-37 and CHH-68 exist as Jira epics but neither has a
// frontend route yet — an enabled link with nowhere real to go would be worse than being honest.
const NAV_ITEMS = [
  { label: "Home", to: "/dashboard/individual", enabled: true },
  { label: "My requests", to: "/dashboard/individual#your-requests", enabled: true },
  { label: "Events", to: "/dashboard/individual", enabled: false },
  { label: "Emergency services", to: "/dashboard/individual", enabled: false },
] as const;

const STATUS_PILL_CLASSES: Record<string, string> = {
  Matching: "bg-amber-tint text-amber",
  Expired: "bg-sand-2 text-ink-2",
};

function BloodGroupBadge({ group, size = "sm" }: { group: string; size?: "sm" | "lg" }) {
  const dimension = size === "lg" ? "h-[46px] w-[46px] text-base" : "h-9 w-9 text-[13.5px]";
  return (
    <div className={`flex ${dimension} shrink-0 items-center justify-center rounded-full bg-blood-tint font-extrabold text-blood-deep`}>
      {group}
    </div>
  );
}

function EmptyState({
  icon,
  title,
  description,
  arrivesWith,
}: {
  icon: React.ReactNode;
  title: string;
  description: string;
  arrivesWith: string;
}) {
  return (
    <div className="flex flex-col items-center rounded-md border border-dashed border-line-strong bg-cream px-4 py-8 text-center">
      <div className="mb-3 flex h-[68px] w-[68px] items-center justify-center rounded-full bg-sand-2 text-ink-3">
        {icon}
      </div>
      <h3 className="mb-1 text-base font-bold text-ink">{title}</h3>
      <p className="max-w-[36ch] text-[13px] leading-[19px] text-ink-2">{description}</p>
      <span className="mt-3 rounded-full bg-sand-2 px-[11px] py-[5px] text-[11.5px] font-semibold text-ink-3">
        {arrivesWith}
      </span>
    </div>
  );
}

function RequestHistoryRow({ request }: { request: BloodRequestDto }) {
  const posted = new Date(request.createdAtUtc).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
  return (
    <div className="flex items-center gap-3 rounded-sm border border-line bg-cream px-4 py-3">
      <BloodGroupBadge group={request.bloodGroup} />
      <div className="min-w-0 flex-1">
        <div className="truncate text-[14.5px] font-semibold text-ink">
          {request.unitsRequired} unit{request.unitsRequired === 1 ? "" : "s"} &middot; {request.locationCityArea}
        </div>
        <div className="text-[12.5px] text-ink-3">
          {posted} &middot; {request.urgency}
        </div>
      </div>
      <span className={`rounded-full px-[10px] py-[4px] text-[12.5px] font-semibold ${STATUS_PILL_CLASSES[request.status] ?? "bg-sand-2 text-ink-2"}`}>
        {request.status === "Matching" ? "Active" : request.status}
      </span>
    </div>
  );
}

function BellIcon() {
  return (
    <svg width="21" height="21" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
      <path d="M18 8a6 6 0 1 0-12 0c0 6-2 7-2 7h16s-2-1-2-7" />
      <path d="M10.5 20a1.8 1.8 0 0 0 3 0" />
    </svg>
  );
}

/**
 * Individual's home dashboard (CHH-81), replacing IndividualDashboardStubPage. Sections whose
 * backing stories aren't built yet (donor responses/CHH-35, notifications/CHH-34, events/CHH-37)
 * render an honest empty state rather than fabricated data — see the approved design at
 * https://claude.ai/code/artifact/28d8f6bf-cfea-421e-8c84-8be5a9954604.
 */
export function IndividualDashboardPage() {
  const navigate = useNavigate();
  const { session, clearSession } = useAuth();
  const { data, isLoading, isError } = useIndividualDashboard(session?.token);
  const { unreadCount } = useNotifications(session?.token);
  const [userMenuOpen, setUserMenuOpen] = useState(false);

  function handleLogout() {
    clearSession();
    navigate("/login");
  }

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:h-[72px] lg:px-8">
        <svg width="22" height="22" viewBox="0 0 24 24" fill="currentColor" className="text-blood" aria-hidden="true">
          <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
        </svg>
        <b className="text-[15px] font-extrabold tracking-tight">Community Health Hub</b>

        <nav className="ml-6 hidden gap-1 lg:flex">
          {NAV_ITEMS.map((item) =>
            item.enabled ? (
              <Link
                key={item.label}
                to={item.to}
                className="rounded-sm px-3.5 py-2 text-sm font-semibold text-clay-deep transition-colors hover:bg-sand-2"
              >
                {item.label}
              </Link>
            ) : (
              <span
                key={item.label}
                title="Coming soon"
                className="cursor-not-allowed rounded-sm px-3.5 py-2 text-sm font-semibold text-ink-3"
              >
                {item.label}
              </span>
            ),
          )}
        </nav>

        <div className="flex-1" />

        <Link to="/notifications" aria-label="Notifications" className="relative flex h-[42px] w-[42px] items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink">
          <BellIcon />
          {unreadCount > 0 && (
            <span className="absolute right-1.5 top-1.5 flex h-[9px] w-[9px] rounded-full bg-blood ring-2 ring-cream" aria-label={`${unreadCount} unread notifications`} />
          )}
        </Link>

        {/* User menu (desktop) — carries Log out, matching the approved design's header
            placement, instead of a full-width button at the bottom of the page. */}
        <div className="relative ml-1 hidden lg:block">
          <button
            type="button"
            onClick={() => setUserMenuOpen((open) => !open)}
            className="flex items-center gap-2.5 rounded-full border border-line bg-cream py-1.5 pl-1.5 pr-3 transition-colors hover:bg-sand-2"
          >
            <div className="flex h-8 w-8 items-center justify-center rounded-full bg-clay-tint text-xs font-extrabold text-clay-deep">
              {data.profile?.fullName.charAt(0).toUpperCase() ?? "?"}
            </div>
            <span className="text-left leading-tight">
              <span className="block text-[13.5px] font-bold">{data.profile?.fullName ?? "Your account"}</span>
              <span className="block text-xs text-ink-3">
                {data.profile ? `${data.profile.bloodGroup} · ${data.profile.locationCityArea}` : ""}
              </span>
            </span>
          </button>
          {userMenuOpen && (
            <div className="absolute right-0 top-full z-10 mt-2 w-44 overflow-hidden rounded-md border border-line bg-cream shadow-md">
              <Link
                to="/profile"
                onClick={() => setUserMenuOpen(false)}
                className="block px-4 py-2.5 text-sm text-ink hover:bg-sand-2"
              >
                View profile
              </Link>
              <button
                type="button"
                onClick={handleLogout}
                className="block w-full px-4 py-2.5 text-left text-sm text-error hover:bg-sand-2"
              >
                Log out
              </button>
            </div>
          )}
        </div>
      </div>

      <div className="mx-auto flex max-w-5xl flex-col gap-5 px-4 py-5 lg:grid lg:grid-cols-[minmax(0,1fr)_380px] lg:items-start lg:gap-6 lg:px-8 lg:py-8">
        <div className="flex flex-col gap-5 lg:gap-6">
          {isError && (
            <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
              Couldn&apos;t load your dashboard. Try refreshing the page.
            </div>
          )}

          {data.needsRegistration && (
            <div className="rounded-sm border border-amber bg-amber-tint px-4 py-3 text-sm text-amber">
              Finish setting up your profile to request or donate blood.
            </div>
          )}

          <section className="flex flex-col gap-3 rounded-lg border border-blood-line bg-blood-tint p-4 lg:flex-row lg:items-center lg:p-5">
            <div className="flex-1">
              <h2 className="text-xl font-extrabold tracking-tight text-blood-deep">Someone needs blood?</h2>
              <p className="mt-1 max-w-[44ch] text-[13.5px] leading-[19px] text-blood-deep/90">
                Post a request and we&apos;ll alert eligible donors around you straight away.
              </p>
            </div>
            <button
              type="button"
              onClick={() => navigate("/blood-requests/new")}
              className="flex h-[54px] items-center justify-center gap-2 rounded-md bg-gradient-to-r from-blood to-blood-hover px-6 text-base font-semibold text-white shadow-sm transition-shadow hover:shadow-md active:scale-[0.99]"
            >
              <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
              </svg>
              Notify donors
            </button>
          </section>

          {data.activeRequest && (
            <section>
              <h2 className="mb-2 text-[17px] font-bold tracking-tight">Your active request</h2>
              <div className="flex overflow-hidden rounded-md border border-line bg-cream shadow-sm">
                <div className="w-[5px] shrink-0 bg-blood" />
                <div className="flex flex-1 flex-col gap-2 p-4">
                  <div className="flex items-center gap-3">
                    <BloodGroupBadge group={data.activeRequest.bloodGroup} />
                    <div className="min-w-0 flex-1">
                      <div className="truncate text-[15px] font-bold">
                        {data.activeRequest.unitsRequired} unit{data.activeRequest.unitsRequired === 1 ? "" : "s"} &middot; {data.activeRequest.locationCityArea}
                      </div>
                      <div className="text-[12.5px] text-ink-3">{data.activeRequest.urgency}</div>
                    </div>
                  </div>
                  <div className="flex items-center justify-between text-[13px] text-ink-2">
                    <span>Expires {new Date(data.activeRequest.expiresAtUtc).toLocaleTimeString(undefined, { hour: "numeric", minute: "2-digit" })}</span>
                    {/* CHH-36 (donor match tracking) isn't built yet — inert text, not a dead link. */}
                    <span className="text-ink-3">Donor matches — coming soon (CHH-36)</span>
                  </div>
                </div>
              </div>
            </section>
          )}

          <section id="your-requests" className="scroll-mt-20">
            <h2 className="mb-2 text-[17px] font-bold tracking-tight">Your requests</h2>
            {isLoading ? (
              <div className="rounded-sm border border-line bg-cream px-4 py-6 text-center text-sm text-ink-2">Loading…</div>
            ) : data.requests.length === 0 ? (
              <div className="rounded-sm border border-line bg-cream px-4 py-6 text-center text-sm text-ink-2">
                You haven&apos;t created a blood request yet.
              </div>
            ) : (
              <div className="flex flex-col gap-2">
                {data.requests.map((request) => (
                  <RequestHistoryRow key={request.id} request={request} />
                ))}
              </div>
            )}
          </section>
        </div>

        <div className="flex flex-col gap-5 lg:gap-6">
          <div className="rounded-md border border-line bg-cream p-4 shadow-sm">
            {isLoading ? (
              <p className="text-sm text-ink-2">Loading profile…</p>
            ) : data.profile ? (
              <div className="flex items-start gap-3">
                <div className="flex h-[52px] w-[52px] shrink-0 items-center justify-center rounded-full bg-clay-tint text-lg font-extrabold text-clay-deep">
                  {data.profile.fullName.charAt(0).toUpperCase()}
                </div>
                <div className="min-w-0 flex-1">
                  <p className="truncate text-[19px] font-extrabold tracking-tight">{data.profile.fullName}</p>
                  <p className="text-[13px] text-ink-2">{data.profile.locationCityArea}</p>
                  <span className={`mt-2 inline-block rounded-full px-[10px] py-[4px] text-[12.5px] font-semibold ${data.profile.isReceiverOnly ? "bg-sand-2 text-ink-2" : "bg-leaf-tint text-leaf"}`}>
                    {data.profile.isReceiverOnly ? "Receiver only" : "Eligible to donate"}
                  </span>
                </div>
                <BloodGroupBadge group={data.profile.bloodGroup} size="lg" />
              </div>
            ) : (
              <p className="text-sm text-ink-2">Complete your profile to see it here.</p>
            )}
            {data.profile && (
              <Link to="/profile" className="mt-3 inline-block border-t border-line pt-3 text-[13px] font-semibold text-clay hover:text-clay-hover">
                Edit profile
              </Link>
            )}
          </div>

          <EmptyState
            icon={
              <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <path d="M20.8 5.6a5 5 0 0 0-7.1 0L12 7.3l-1.7-1.7a5 5 0 1 0-7.1 7.1L12 21.5l8.8-8.8a5 5 0 0 0 0-7.1Z" />
              </svg>
            }
            title="No donor responses yet"
            description="When you accept or decline a blood request near you, it will be recorded here."
            arrivesWith="Arrives with donor responses · CHH-35"
          />

          <EmptyState
            icon={
              <svg width="30" height="30" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.6} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                <rect x="3" y="5" width="18" height="16" rx="2.5" />
                <path d="M3 10h18M8 3v4M16 3v4" />
              </svg>
            }
            title="No events near you yet"
            description="Blood drives and health camps within your area will be listed here."
            arrivesWith="Arrives with events · CHH-37"
          />

          {/* Desktop moves Log out into the header user menu above — this stays mobile-only,
              matching the approved mobile design's full-width bottom button. */}
          <button
            type="button"
            onClick={handleLogout}
            className="flex h-12 w-full items-center justify-center gap-2 rounded-md border-[1.5px] border-line-strong bg-transparent text-[15px] font-semibold text-ink transition-colors hover:bg-sand-2 lg:hidden"
          >
            <svg width="17" height="17" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M15 17l5-5-5-5M20 12H9M12 20H6a2 2 0 0 1-2-2V6a2 2 0 0 1 2-2h6" />
            </svg>
            Log out
          </button>
        </div>
      </div>
    </div>
  );
}
