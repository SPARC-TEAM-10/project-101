import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { useQuery, useQueryClient } from "@tanstack/react-query";

import { useAuth } from "../../context/AuthProvider";
import { useToast } from "../../context/ToastProvider";
import { getMyProfile } from "../../api/individualApi";
import { useUpdateIndividualProfile } from "../../features/individual/useUpdateIndividualProfile";
import { MAX_OTHER_ILLNESS_LENGTH } from "../../lib/validation/individualSchemas";

function FieldError({ message }: { message?: string }) {
  if (!message) return null;
  return (
    <span className="flex items-start gap-1.5 text-[12.5px] leading-tight text-error">
      <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2.2} strokeLinecap="round" strokeLinejoin="round" className="mt-0.5 flex-none" aria-hidden="true">
        <path d="M12 4.5 21 19.5H3L12 4.5Z" />
        <path d="M12 10v4M12 16.8v.1" />
      </svg>
      {message}
    </span>
  );
}

/**
 * Profile screen (CHH-F02 profile edit). Read-only by default; "Edit" switches location and the
 * health-screening flags into an editable form (name, email, blood group, DOB, and gender stay
 * fixed — no product requirement to change them yet, see UpdateIndividualProfileRequest).
 */
export function ProfilePage() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const toast = useToast();
  const queryClient = useQueryClient();
  const [isEditing, setIsEditing] = useState(false);

  const { data: profile, isLoading, isError } = useQuery({
    queryKey: ["individual", "me", session?.token],
    queryFn: () => getMyProfile(session!.token),
    enabled: Boolean(session?.token),
  });

  const {
    values,
    locationShared,
    shareLocation,
    isSharingLocation,
    clearSharingLocation,
    setLocationCityArea,
    setChronicIllness,
    setRecentSurgery,
    setInfectiousDisease,
    setUnderweight,
    setOtherIllness,
    setOtherIllnessDetails,
    fieldErrors,
    touched,
    geolocation,
    isPending,
    isSuccess,
    error,
    submit,
  } = useUpdateIndividualProfile(session?.token ?? null, profile);

  useEffect(() => {
    if (isSuccess) {
      setIsEditing(false);
    }
  }, [isSuccess]);

  async function saveProfile() {
    const result = await submit();
    if (result.ok && result.data) {
      queryClient.setQueryData(["individual", "me", session?.token], result.data);
      toast.success("Profile updated.");
    } else if (result.error) {
      toast.error(result.error.message);
    }
    return result;
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    await saveProfile();
  }

  // CHH-85: "Share my location" auto-saves as soon as the browser resolves coordinates, reusing
  // the same save path (and its toast/cache-sync) as a manual edit — no need to enter edit mode.
  // Keyed on `values.latitude`/`values.longitude` (not `geolocation.status`) because the hook's
  // own effect that copies resolved coordinates into `values` runs in the same effect flush as
  // this one, one step earlier — reading `geolocation.status` here would race and submit before
  // `values` (and therefore `submit()`'s payload) actually has the new coordinates.
  useEffect(() => {
    if (!isSharingLocation || values.latitude == null || values.longitude == null) {
      return;
    }
    clearSharingLocation();
    void saveProfile();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isSharingLocation, values.latitude, values.longitude]);

  const otherIllnessLength = (values.otherIllnessDetails ?? "").length;

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
        <b className="flex-1 text-[15px] font-bold">Profile</b>
        {profile && !isEditing && (
          <button
            type="button"
            onClick={() => setIsEditing(true)}
            className="rounded-sm px-3 py-1.5 text-sm font-semibold text-clay transition-colors hover:bg-clay-tint"
          >
            Edit
          </button>
        )}
      </div>

      <div className="mx-auto max-w-xl px-4 py-6 lg:px-0">
        {isLoading && <p className="text-sm text-ink-2">Loading…</p>}
        {isError && (
          <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
            Couldn&apos;t load your profile. Try refreshing the page.
          </div>
        )}

        {profile && !isEditing && (
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
              <div className="flex items-center justify-between px-5 py-3.5">
                <dt className="text-sm text-ink-2">Location sharing</dt>
                <dd className="text-sm font-semibold">
                  {locationShared ? (
                    <span className="inline-flex items-center gap-1.5 rounded-full border border-go bg-go-tint px-2.5 py-1 text-[12.5px] font-bold text-go-deep">
                      Shared
                    </span>
                  ) : geolocation.status === "locating" ? (
                    <span className="text-ink-3">Detecting…</span>
                  ) : geolocation.status === "denied" ? (
                    <span className="flex flex-col items-end gap-1">
                      <span className="text-[12.5px] text-error">Permission denied</span>
                      <button type="button" onClick={shareLocation} className="text-[13px] font-semibold text-clay underline">
                        Try again
                      </button>
                    </span>
                  ) : geolocation.status === "unavailable" ? (
                    <span className="text-[12.5px] text-ink-3">Not supported on this device</span>
                  ) : (
                    <button type="button" onClick={shareLocation} className="text-[13px] font-semibold text-clay underline">
                      Share my location
                    </button>
                  )}
                </dd>
              </div>
            </dl>
            {!locationShared && geolocation.status !== "denied" && geolocation.status !== "unavailable" && (
              <p className="px-1 text-xs text-ink-3">
                Sharing your location lets us alert you when someone nearby needs your blood type.
              </p>
            )}

            <p className="px-1 text-xs text-ink-3">
              Name, email, blood group, date of birth, and gender can&apos;t be changed here — reach out to support if
              any of those need to change.
            </p>
          </div>
        )}

        {profile && isEditing && (
          <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-5">
            <div className="flex flex-col gap-1.5">
              <div className="flex items-center justify-between">
                <label htmlFor="edit-location" className="text-sm font-semibold text-ink-2">
                  Location (City / Area) <i className="not-italic text-error">*</i>
                </label>
                {geolocation.status === "locating" && (
                  <span className="text-[12.5px] font-medium text-ink-3">Detecting…</span>
                )}
              </div>
              <div className="relative">
                <input
                  id="edit-location"
                  type="text"
                  value={values.locationCityArea ?? ""}
                  onChange={(e) => setLocationCityArea(e.target.value)}
                  placeholder="Search city or area"
                  aria-invalid={touched && !!fieldErrors.locationCityArea}
                  className={`h-[50px] w-full rounded-sm border-[1.5px] bg-cream pl-4 pr-11 text-base outline-none transition-colors focus:border-clay ${
                    touched && fieldErrors.locationCityArea ? "border-error" : "border-line-strong"
                  }`}
                />
                <button
                  type="button"
                  onClick={() => geolocation.request()}
                  disabled={geolocation.status === "locating"}
                  aria-label="Use current location"
                  title="Use current location"
                  className="absolute right-1.5 top-1.5 flex h-9 w-9 items-center justify-center rounded-sm text-clay transition-colors hover:bg-clay-tint disabled:opacity-60"
                >
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.9} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <circle cx="12" cy="12" r="3" />
                    <path d="M12 3v3M12 18v3M3 12h3M18 12h3" />
                  </svg>
                </button>
              </div>
              {touched && <FieldError message={fieldErrors.locationCityArea?.[0]} />}
            </div>

            <h2 className="text-[11px] font-extrabold uppercase tracking-wide text-ink-3">Health screening</h2>

            <div className="grid grid-cols-1 gap-2 md:grid-cols-2 md:gap-2.5">
              <label className="flex items-start gap-3 rounded-sm border-[1.5px] border-line bg-cream p-3.5">
                <input
                  type="checkbox"
                  className="mt-0.5 h-[19px] w-[19px] flex-none accent-clay"
                  checked={values.isChronicIllness ?? false}
                  onChange={(e) => setChronicIllness(e.target.checked)}
                />
                <span className="text-[13.5px] leading-snug text-ink">Chronic illness — diabetes, heart, kidney</span>
              </label>
              <label className="flex items-start gap-3 rounded-sm border-[1.5px] border-line bg-cream p-3.5">
                <input
                  type="checkbox"
                  className="mt-0.5 h-[19px] w-[19px] flex-none accent-clay"
                  checked={values.hasRecentSurgery ?? false}
                  onChange={(e) => setRecentSurgery(e.target.checked)}
                />
                <span className="text-[13.5px] leading-snug text-ink">Recent surgery in the last 6 months</span>
              </label>
              <label className="flex items-start gap-3 rounded-sm border-[1.5px] border-line bg-cream p-3.5">
                <input
                  type="checkbox"
                  className="mt-0.5 h-[19px] w-[19px] flex-none accent-clay"
                  checked={values.isInfectiousDisease ?? false}
                  onChange={(e) => setInfectiousDisease(e.target.checked)}
                />
                <span className="text-[13.5px] leading-snug text-ink">Infectious disease (HIV, Hepatitis B/C)</span>
              </label>
              <label className="flex items-start gap-3 rounded-sm border-[1.5px] border-line bg-cream p-3.5">
                <input
                  type="checkbox"
                  className="mt-0.5 h-[19px] w-[19px] flex-none accent-clay"
                  checked={values.isUnderweight ?? false}
                  onChange={(e) => setUnderweight(e.target.checked)}
                />
                <span className="text-[13.5px] leading-snug text-ink">Currently underweight</span>
              </label>
              <label className="flex items-start gap-3 rounded-sm border-[1.5px] border-line bg-cream p-3.5">
                <input
                  type="checkbox"
                  className="mt-0.5 h-[19px] w-[19px] flex-none accent-clay"
                  checked={values.isOtherIllness ?? false}
                  onChange={(e) => setOtherIllness(e.target.checked)}
                />
                <span className="text-[13.5px] leading-snug text-ink">Other</span>
              </label>
            </div>

            {values.isOtherIllness && (
              <div className="flex flex-col gap-1.5">
                <label htmlFor="edit-other-illness" className="text-sm font-semibold text-ink-2">
                  Specify other illness <i className="not-italic text-error">*</i>
                </label>
                <textarea
                  id="edit-other-illness"
                  value={values.otherIllnessDetails ?? ""}
                  onChange={(e) => setOtherIllnessDetails(e.target.value.slice(0, MAX_OTHER_ILLNESS_LENGTH))}
                  maxLength={MAX_OTHER_ILLNESS_LENGTH}
                  placeholder="Tell us briefly"
                  aria-invalid={touched && !!fieldErrors.otherIllnessDetails}
                  className={`h-[76px] resize-y rounded-sm border-[1.5px] bg-cream px-4 py-3 text-sm leading-relaxed outline-none transition-colors focus:border-clay ${
                    touched && fieldErrors.otherIllnessDetails ? "border-error" : "border-line-strong"
                  }`}
                />
                <div className="text-right text-[11.5px] text-ink-3">
                  {otherIllnessLength} / {MAX_OTHER_ILLNESS_LENGTH}
                </div>
                {touched && <FieldError message={fieldErrors.otherIllnessDetails?.[0]} />}
              </div>
            )}

            {error && (
              <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
                {error.message}
              </div>
            )}

            <div className="flex gap-3 pt-2">
              <button
                type="submit"
                disabled={isPending}
                className={`flex h-[48px] flex-1 items-center justify-center rounded-md text-[15px] font-semibold transition-colors ${
                  isPending ? "cursor-not-allowed bg-sand-2 text-ink-off" : "bg-clay text-white hover:bg-clay-hover"
                }`}
              >
                {isPending ? "Saving…" : "Save changes"}
              </button>
              <button
                type="button"
                onClick={() => setIsEditing(false)}
                disabled={isPending}
                className="flex h-[48px] flex-1 items-center justify-center rounded-md border-[1.5px] border-line-strong text-[15px] font-semibold text-ink-2 transition-colors hover:bg-sand-2"
              >
                Cancel
              </button>
            </div>
          </form>
        )}
      </div>
    </div>
  );
}
