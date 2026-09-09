import { useState } from "react";
import { useNavigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import type { FacilityDto } from "../../api/facilityApi";
import { useFacilityDashboard } from "../../features/facility/useFacilityDashboard";

const EXPECTED_DECISION_BUSINESS_DAYS = 2;

function formatDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, { day: "numeric", month: "short", year: "numeric" });
}

// Skips weekends when projecting "expected decision" from the submission date (Dashboard.dc.html's
// "Expected decision" meta row) — a fixed +2 calendar days would misleadingly land on a Sunday.
function addBusinessDays(from: Date, days: number): Date {
  const result = new Date(from);
  let remaining = days;
  while (remaining > 0) {
    result.setDate(result.getDate() + 1);
    const day = result.getDay();
    if (day !== 0 && day !== 6) {
      remaining -= 1;
    }
  }
  return result;
}

interface StatusConfig {
  pillLabel: string;
  pillClass: string;
  title: string;
  lede: string;
  metaLabel: string;
  metaValue: string;
  ctaLabel: string;
  ctaPrimary: boolean;
  sectionLabel: string;
  modalBody: string;
}

function buildStatusConfig(facility: FacilityDto): StatusConfig {
  switch (facility.verificationStatus) {
    case "Pending":
      return {
        pillLabel: "Pending verification",
        pillClass: "bg-amber-tint text-amber",
        title: "We're reviewing your licence",
        lede: "An admin is checking the document against the facility name and licence number you gave. Reviews usually finish within two working days, and we notify your primary contact either way.",
        metaLabel: "Expected decision",
        metaValue: formatDate(addBusinessDays(new Date(facility.createdAtUtc), EXPECTED_DECISION_BUSINESS_DAYS).toISOString()),
        ctaLabel: "Check for an update",
        ctaPrimary: false,
        sectionLabel: "Locked until verification",
        modalBody: `${facility.facilityName} is still pending verification, so events, blood requests and inventory stay locked for now.`,
      };
    case "Rejected":
      return {
        pillLabel: "Rejected",
        pillClass: "bg-error-tint text-error",
        title: "Verification was not approved",
        lede: "Your facility stays hidden and publishing is locked until a new licence passes review.",
        metaLabel: "Reviewed on",
        metaValue: formatDate(facility.updatedAtUtc),
        ctaLabel: "Upload a new licence",
        ctaPrimary: true,
        sectionLabel: "Locked until verification",
        modalBody: `${facility.facilityName} was rejected on ${formatDate(facility.updatedAtUtc)}. Upload a valid licence to have it reviewed again.`,
      };
    case "Verified":
    default:
      return {
        pillLabel: "",
        pillClass: "",
        title: "",
        lede: "",
        metaLabel: "",
        metaValue: "",
        ctaLabel: "",
        ctaPrimary: false,
        sectionLabel: "What you can publish",
        modalBody: "",
      };
  }
}

function VerifiedTick({ small = false }: { small?: boolean }) {
  const dimension = small ? "h-4 w-4" : "h-[22px] w-[22px]";
  return (
    <span
      className={`flex ${dimension} shrink-0 items-center justify-center rounded-full bg-leaf text-white`}
      title="Verified facility"
      aria-label="Verified facility"
    >
      <svg width="60%" height="60%" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={3.4} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M5 12.8 9.8 17.5 19 7.5" />
      </svg>
    </span>
  );
}

interface FeatureTile {
  icon: React.ReactNode;
  title: string;
  description: string;
  route?: string;
}

const FEATURE_TILES: FeatureTile[] = [
  {
    icon: (
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <rect x="3.5" y="5" width="17" height="15.5" rx="2.5" />
        <path d="M3.5 10h17M8 3.5v3M16 3.5v3" />
      </svg>
    ),
    title: "Plan an event",
    description: "Publish a donation camp or screening drive to donors nearby.",
    route: "/events/new",
  },
  {
    icon: (
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M12 20s-7-4.6-7-9.5A4.5 4.5 0 0 1 12 7a4.5 4.5 0 0 1 7 3.5C19 15.4 12 20 12 20Z" />
      </svg>
    ),
    title: "Broadcast a blood request",
    description: "Alert matching donors within your search radius.",
  },
  {
    icon: (
      <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.8} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
        <path d="M4.5 8.5 12 4.5l7.5 4v7L12 19.5 4.5 15.5Z" />
        <path d="M4.5 8.5 12 12.5l7.5-4M12 12.5v7" />
      </svg>
    ),
    title: "Publish inventory",
    description: "Show current blood units and bed availability to seekers.",
  },
];

/**
 * Facility verification status dashboard (CHH-28/US-CHH-003-03), matching the approved design at
 * design/drafts/CHH-F03-facility-verification/{Dashboard,MobileStatus}.dc.html (revision 2).
 * Approved facilities show no status card — only a verified tick beside the facility name (AC4).
 */
export function FacilityDashboardPage() {
  const navigate = useNavigate();
  const { session, clearSession } = useAuth();
  const { facility, isLoading, isError } = useFacilityDashboard(session?.token);
  const [showModal, setShowModal] = useState(false);

  function handleLogout() {
    clearSession();
    navigate("/login");
  }

  if (isLoading) {
    return (
      <div className="flex min-h-screen items-center justify-center bg-sand font-sans text-ink-2">
        Loading your facility…
      </div>
    );
  }

  if (isError || !facility) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-3 bg-sand px-4 font-sans text-ink">
        <p className="text-sm text-error">Couldn&apos;t load your facility. Try refreshing the page.</p>
        <button type="button" onClick={handleLogout} className="text-sm font-semibold text-clay hover:text-clay-hover">
          Log out
        </button>
      </div>
    );
  }

  const isApproved = facility.verificationStatus === "Verified";
  const isRejected = facility.verificationStatus === "Rejected";
  const locked = !isApproved;
  const config = buildStatusConfig(facility);
  const primaryContact = facility.contacts[0];

  function tryLocked() {
    if (locked) {
      setShowModal(true);
    }
  }

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:px-8">
        <svg width="22" height="22" viewBox="0 0 24 24" fill="currentColor" className="text-blood" aria-hidden="true">
          <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
        </svg>
        <b className="text-[15px] font-extrabold tracking-tight">Community Health Hub</b>
        <div className="flex-1" />
        <span className="hidden text-[13px] text-ink-2 sm:inline">{facility.facilityName}</span>
        {isApproved && <VerifiedTick small />}
      </div>

      <div className="mx-auto flex max-w-3xl flex-col gap-5 px-4 py-5 lg:px-8 lg:py-8">
        <div className="flex items-start gap-3">
          <div className="min-w-0 flex-1">
            <div className="flex items-center gap-2">
              <h1 className="text-2xl font-extrabold tracking-tight">{facility.facilityName}</h1>
              {isApproved && <VerifiedTick />}
            </div>
            <p className="mt-1 text-[13.5px] text-ink-2">
              {facility.category} &middot; Licence {facility.licenseNumber} &middot; {facility.address}
            </p>
          </div>
          {!isApproved && (
            <span className={`inline-flex h-7 shrink-0 items-center gap-1.5 rounded-full px-3 text-[12.5px] font-bold ${config.pillClass}`}>
              {config.pillLabel}
            </span>
          )}
        </div>

        {!isApproved && (
          <section className={`flex overflow-hidden rounded-lg border border-line bg-cream shadow-sm`}>
            <div className={`w-[6px] shrink-0 ${isRejected ? "bg-error" : "bg-amber"}`} />
            <div className="flex flex-1 flex-col gap-4 p-5">
              <h2 className="text-lg font-bold tracking-tight sm:text-xl">{config.title}</h2>
              <p className="max-w-[62ch] text-sm leading-relaxed text-ink-2">{config.lede}</p>

              {isRejected && facility.rejectionReason && (
                <div className="flex gap-3 rounded-md bg-error-tint p-4">
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" className="mt-0.5 shrink-0 text-error" aria-hidden="true">
                    <path d="M12 4.5 21 19.5H3L12 4.5Z" />
                    <path d="M12 10v4M12 16.8v.1" />
                  </svg>
                  <div>
                    <b className="mb-1 block text-[13px] font-bold text-error">Reason given by the reviewing admin</b>
                    <p className="max-w-[64ch] text-[13px] leading-relaxed text-ink-2">{facility.rejectionReason}</p>
                    <span className="mt-2 block text-xs text-ink-3">Reviewed {formatDate(facility.updatedAtUtc)}</span>
                  </div>
                </div>
              )}

              <div className="flex flex-wrap gap-x-7 gap-y-3 border-t border-line pt-4 text-sm">
                <div className="flex flex-col gap-1">
                  <dt className="text-[11px] font-bold uppercase tracking-wider text-ink-3">Submitted</dt>
                  <dd className="text-sm font-semibold tabular-nums">{formatDate(facility.createdAtUtc)}</dd>
                </div>
                <div className="flex flex-col gap-1">
                  <dt className="text-[11px] font-bold uppercase tracking-wider text-ink-3">{config.metaLabel}</dt>
                  <dd className="text-sm font-semibold tabular-nums">{config.metaValue}</dd>
                </div>
                {primaryContact && (
                  <div className="flex flex-col gap-1">
                    <dt className="text-[11px] font-bold uppercase tracking-wider text-ink-3">Primary contact</dt>
                    <dd className="text-sm font-semibold">{primaryContact.name}</dd>
                  </div>
                )}
              </div>

              <div className="flex flex-wrap gap-3">
                <button
                  type="button"
                  className={
                    config.ctaPrimary
                      ? "h-11 rounded-md bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-hover"
                      : "h-11 rounded-md border-[1.5px] border-line-strong px-5 text-sm font-semibold text-ink hover:border-ink-3"
                  }
                >
                  {config.ctaLabel}
                </button>
                <button type="button" className="h-11 rounded-md px-4 text-sm font-semibold text-clay hover:bg-clay-tint">
                  View submitted details
                </button>
              </div>
            </div>
          </section>
        )}

        <h3 className="text-base font-bold">{config.sectionLabel}</h3>
        <div className="grid grid-cols-1 gap-4 sm:grid-cols-3">
          {FEATURE_TILES.map((tile) => (
            <button
              key={tile.title}
              type="button"
              onClick={() => (locked ? tryLocked() : tile.route && navigate(tile.route))}
              aria-disabled={locked}
              className={
                locked
                  ? "flex cursor-not-allowed flex-col gap-2 rounded-lg border border-line bg-sand-2 p-5 text-left shadow-none"
                  : "flex flex-col gap-2 rounded-lg border border-line bg-cream p-5 text-left shadow-sm hover:shadow-md"
              }
            >
              <span className={`flex h-10 w-10 items-center justify-center rounded-md ${locked ? "bg-cream text-ink-off" : "bg-clay-tint text-clay-deep"}`}>
                {tile.icon}
              </span>
              <b className={`text-[15.5px] font-bold ${locked ? "text-ink-off" : ""}`}>{tile.title}</b>
              <p className={`text-[12.5px] leading-relaxed ${locked ? "text-ink-off" : "text-ink-2"}`}>{tile.description}</p>
              {locked && (
                <span className="mt-1 inline-flex w-fit items-center gap-1.5 rounded-full border border-line-strong bg-cream px-2.5 py-1 text-[11.5px] font-bold text-ink-2">
                  <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <rect x="4.5" y="10.5" width="15" height="9.5" rx="2" />
                    <path d="M8 10.5V8a4 4 0 0 1 8 0v2.5" />
                  </svg>
                  Locked
                </span>
              )}
            </button>
          ))}
        </div>

        <button
          type="button"
          onClick={handleLogout}
          className="mt-2 flex h-12 w-full items-center justify-center gap-2 rounded-md border-[1.5px] border-line-strong bg-transparent text-[15px] font-semibold text-ink transition-colors hover:bg-sand-2 sm:w-fit sm:self-end"
        >
          Log out
        </button>
      </div>

      {showModal && (
        <div className="fixed inset-0 z-40 flex items-end justify-center bg-[rgba(57,42,29,0.5)] sm:items-center">
          <div className="w-full rounded-t-2xl bg-cream shadow-lg sm:w-[480px] sm:rounded-2xl">
            <div className="flex items-center gap-3 px-6 pt-6">
              <span className="flex h-10 w-10 shrink-0 items-center justify-center rounded-md bg-amber-tint text-amber">
                <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <rect x="4.5" y="10.5" width="15" height="9.5" rx="2" />
                  <path d="M8 10.5V8a4 4 0 0 1 8 0v2.5" />
                </svg>
              </span>
              <h4 className="text-lg font-bold tracking-tight">Feature locked until account verification.</h4>
            </div>
            <div className="px-6 py-4">
              <p className="mb-3 text-sm leading-relaxed text-ink-2">{config.modalBody}</p>
              <p className="text-sm leading-relaxed text-ink-2">You can keep editing your facility profile and contacts while you wait.</p>
            </div>
            <div className="flex justify-end gap-3 border-t border-line px-6 py-4">
              <button
                type="button"
                onClick={() => setShowModal(false)}
                className="h-11 rounded-md border-[1.5px] border-line-strong px-5 text-sm font-semibold text-ink hover:border-ink-3"
              >
                Close
              </button>
              <button
                type="button"
                onClick={() => setShowModal(false)}
                className="h-11 rounded-md bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-hover"
              >
                {isRejected ? "Upload a new licence" : "View verification status"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
