import { useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { useToast } from "../../context/ToastProvider";
import { useBloodRequestMatchStatus } from "../../features/bloodRequest/useBloodRequestMatchStatus";

const STATUS_LABELS: Record<string, string> = {
  Matching: "Searching for donors",
  Fulfilled: "Request fulfilled",
  Expired: "Request expired",
};

const RADIUS_INCREMENT_KM = 10;
const MAX_RADIUS_KM = 100;

function StatCard({ label, value }: { label: string; value: number }) {
  return (
    <div className="flex flex-1 flex-col items-center gap-0.5 rounded-md border border-line bg-cream py-3">
      <span className="text-[22px] font-extrabold tracking-tight text-ink">{value}</span>
      <span className="text-[11.5px] font-semibold text-ink-3">{label}</span>
    </div>
  );
}

function ProgressBar({ notified, viewed, accepted }: { notified: number; viewed: number; accepted: number }) {
  if (notified === 0) return null;
  const acceptedPct = (accepted / notified) * 100;
  const viewedPct = (Math.max(viewed - accepted, 0) / notified) * 100;

  return (
    <div className="flex h-2.5 w-full overflow-hidden rounded-full bg-sand-2">
      <div className="h-full bg-go" style={{ width: `${acceptedPct}%` }} />
      <div className="h-full bg-clay" style={{ width: `${viewedPct}%` }} />
    </div>
  );
}

function DonorRow({ label, mobileNumber, isAccepted }: { label: string; mobileNumber?: string; isAccepted: boolean }) {
  return (
    <div className="flex items-center justify-between gap-3 rounded-sm border border-line bg-cream px-4 py-3">
      <div className="min-w-0 flex-1">
        <div className="truncate text-[14px] font-semibold text-ink">{label}</div>
        {isAccepted && mobileNumber && (
          <a href={`tel:${mobileNumber}`} className="text-[12.5px] font-semibold text-go-deep underline">
            {mobileNumber}
          </a>
        )}
      </div>
      <span
        className={`shrink-0 rounded-full px-[10px] py-[4px] text-[12px] font-semibold ${
          isAccepted ? "bg-leaf-tint text-leaf" : "bg-sand-2 text-ink-2"
        }`}
      >
        {isAccepted ? "Accepted" : "Pending"}
      </span>
    </div>
  );
}

/**
 * Requester Match Tracking Dashboard (CHH-36). Shows Notified/Viewed/Accepted counts (AC1) and
 * the matched-donor list (AC2), already shaped by the backend per the guest-vs-registered
 * visibility rule — this page renders whatever donors it's given without its own role branching.
 * "No eligible donors found" (AC3) offers Increase Radius (AC4), capped at 100km.
 */
export function RequesterMatchDashboardPage() {
  const navigate = useNavigate();
  const { id } = useParams<{ id: string }>();
  const { session } = useAuth();
  const toast = useToast();
  const { status, isLoading, isError, increaseRadius, isIncreasingRadius } = useBloodRequestMatchStatus(session?.token, id!);
  const [showRadiusPrompt, setShowRadiusPrompt] = useState(false);

  async function handleIncreaseRadius() {
    if (!status) return;
    const newRadius = Math.min(status.searchRadiusKm + RADIUS_INCREMENT_KM, MAX_RADIUS_KM);
    try {
      await increaseRadius(newRadius);
      toast.success(`Search radius increased to ${newRadius}km — re-checking for donors.`);
      setShowRadiusPrompt(false);
    } catch {
      toast.error("Couldn't update the radius. Try again.");
    }
  }

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:px-8">
        <button
          type="button"
          onClick={() => navigate(-1)}
          aria-label="Back"
          className="flex h-9 w-9 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <b className="text-[15px] font-bold">Match status</b>
      </div>

      <div className="mx-auto max-w-xl px-4 py-6 lg:px-0">
        {isLoading && <p className="text-sm text-ink-2">Loading…</p>}

        {isError && (
          <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
            Couldn&apos;t load this request&apos;s status. Try refreshing the page.
          </div>
        )}

        {status && (
          <div className="flex flex-col gap-5">
            <div className="rounded-md border border-line bg-cream p-4">
              <p className="mb-3 text-[13px] font-semibold text-ink-2">
                {STATUS_LABELS[status.status] ?? status.status} &middot; radius {status.searchRadiusKm}km
              </p>

              <div className="flex gap-2">
                <StatCard label="Notified" value={status.notifiedCount} />
                <StatCard label="Viewed" value={status.viewedCount} />
                <StatCard label="Accepted" value={status.acceptedCount} />
              </div>

              <div className="mt-3">
                <ProgressBar notified={status.notifiedCount} viewed={status.viewedCount} accepted={status.acceptedCount} />
              </div>
            </div>

            {status.notifiedCount === 0 && status.status === "Matching" && (
              <div className="flex flex-col items-center gap-3 rounded-md border border-dashed border-line-strong bg-cream px-4 py-8 text-center">
                <p className="text-[14px] font-semibold text-ink">
                  No eligible donors found within {status.searchRadiusKm}km
                </p>
                {!showRadiusPrompt ? (
                  <button
                    type="button"
                    onClick={() => setShowRadiusPrompt(true)}
                    disabled={status.searchRadiusKm >= MAX_RADIUS_KM}
                    className="flex h-11 items-center justify-center rounded-md bg-clay px-5 text-sm font-semibold text-white transition-colors hover:bg-clay-hover disabled:cursor-not-allowed disabled:opacity-60"
                  >
                    Increase Radius
                  </button>
                ) : (
                  <div className="flex flex-col items-center gap-2">
                    <p className="text-[13px] text-ink-2">
                      Expand to {Math.min(status.searchRadiusKm + RADIUS_INCREMENT_KM, MAX_RADIUS_KM)}km and search again?
                    </p>
                    <div className="flex gap-2">
                      <button
                        type="button"
                        onClick={handleIncreaseRadius}
                        disabled={isIncreasingRadius}
                        className="flex h-10 items-center justify-center rounded-md bg-clay px-4 text-sm font-semibold text-white transition-colors hover:bg-clay-hover disabled:cursor-not-allowed disabled:opacity-60"
                      >
                        {isIncreasingRadius ? "Updating…" : "Confirm"}
                      </button>
                      <button
                        type="button"
                        onClick={() => setShowRadiusPrompt(false)}
                        className="flex h-10 items-center justify-center rounded-md border-[1.5px] border-line-strong px-4 text-sm font-semibold text-ink transition-colors hover:bg-sand-2"
                      >
                        Cancel
                      </button>
                    </div>
                  </div>
                )}
              </div>
            )}

            {status.donors.length > 0 && (
              <div>
                <h2 className="mb-2 text-[15px] font-bold tracking-tight">Matched donors</h2>
                <div className="flex flex-col gap-2">
                  {status.donors.map((donor, index) => (
                    <DonorRow key={`${donor.label}-${index}`} label={donor.label} mobileNumber={donor.mobileNumber} isAccepted={donor.isAccepted} />
                  ))}
                </div>
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
}
