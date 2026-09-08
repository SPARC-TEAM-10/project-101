import { useNavigate } from "react-router-dom";
import { useQuery } from "@tanstack/react-query";

import { useAuth } from "../../context/AuthProvider";
import { getMyProfile } from "../../api/individualApi";

/**
 * Read-only profile screen (CHH-81 follow-up). No `PATCH /individuals/me` exists yet, so this
 * page only displays what's already collected at registration — editing is a separate,
 * not-yet-scoped ticket.
 */
export function ProfilePage() {
  const navigate = useNavigate();
  const { session } = useAuth();

  const { data: profile, isLoading, isError } = useQuery({
    queryKey: ["individual", "me", session?.token],
    queryFn: () => getMyProfile(session!.token),
    enabled: Boolean(session?.token),
  });

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-3 border-b border-line bg-cream px-4 lg:px-8">
        <button
          type="button"
          onClick={() => navigate("/dashboard/individual")}
          aria-label="Back to dashboard"
          className="flex h-9 w-9 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2 hover:text-ink"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <b className="text-[15px] font-bold">Profile</b>
      </div>

      <div className="mx-auto max-w-xl px-4 py-6 lg:px-0">
        {isLoading && <p className="text-sm text-ink-2">Loading…</p>}
        {isError && (
          <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
            Couldn&apos;t load your profile. Try refreshing the page.
          </div>
        )}
        {profile && (
          <div className="flex flex-col gap-4">
            <div className="flex items-center gap-4 rounded-md border border-line bg-cream p-5 shadow-sm">
              <div className="flex h-16 w-16 shrink-0 items-center justify-center rounded-full bg-clay-tint text-2xl font-extrabold text-clay-deep">
                {profile.fullName.charAt(0).toUpperCase()}
              </div>
              <div>
                <p className="text-xl font-extrabold tracking-tight">{profile.fullName}</p>
                <p className="text-sm text-ink-2">{profile.locationCityArea}</p>
              </div>
            </div>

            <dl className="divide-y divide-line rounded-md border border-line bg-cream shadow-sm">
              <div className="flex items-center justify-between px-5 py-3.5">
                <dt className="text-sm text-ink-2">Blood group</dt>
                <dd className="text-sm font-semibold">{profile.bloodGroup}</dd>
              </div>
              <div className="flex items-center justify-between px-5 py-3.5">
                <dt className="text-sm text-ink-2">Donor status</dt>
                <dd className="text-sm font-semibold">{profile.isReceiverOnly ? "Receiver only" : "Eligible to donate"}</dd>
              </div>
              <div className="flex items-center justify-between px-5 py-3.5">
                <dt className="text-sm text-ink-2">Location</dt>
                <dd className="text-sm font-semibold">{profile.locationCityArea}</dd>
              </div>
            </dl>

            <p className="px-1 text-xs text-ink-3">
              Editing your profile isn&apos;t available yet — reach out to support if any of this needs to change.
            </p>
          </div>
        )}
      </div>
    </div>
  );
}
