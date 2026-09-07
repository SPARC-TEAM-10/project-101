import { useNavigate } from "react-router-dom";

import { AuthSplitLayout } from "../../components/AuthSplitLayout";
import { useAuth } from "../../context/AuthProvider";
import { useToast } from "../../context/ToastProvider";
import { useIndividualRegistration } from "../../features/individual/useIndividualRegistration";
import { BLOOD_GROUPS, type BloodGroup } from "../../lib/validation/bloodRequestSchemas";
import {
  GENDERS,
  OTHER_ILLNESS_MAX_LENGTH,
  previewEligibility,
  type Gender,
} from "../../lib/validation/individualSchemas";

type ChipVariant = "clay";

const CHIP_SELECTED_CLASSES: Record<ChipVariant, string> = {
  clay: "border-clay bg-clay text-white",
};

function ChipButton({
  label,
  selected,
  onClick,
  variant = "clay",
}: {
  label: string;
  selected: boolean;
  onClick: () => void;
  variant?: ChipVariant;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-pressed={selected}
      className={`h-11 rounded-md border-[1.5px] text-sm font-semibold transition-colors ${
        selected
          ? CHIP_SELECTED_CLASSES[variant]
          : "border-line-strong bg-cream text-ink-2 hover:border-line-strong hover:bg-sand-2"
      }`}
    >
      {label}
    </button>
  );
}

function HealthFlagRow({
  label,
  checked,
  onChange,
  id,
}: {
  label: string;
  checked: boolean;
  onChange: (checked: boolean) => void;
  id: string;
}) {
  return (
    <label
      htmlFor={id}
      className="flex cursor-pointer items-center gap-3 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 py-3 transition-colors hover:bg-sand-2"
    >
      <input
        id={id}
        type="checkbox"
        checked={checked}
        onChange={(e) => onChange(e.target.checked)}
        className="h-5 w-5 shrink-0 accent-clay"
      />
      <span className="text-sm text-ink">{label}</span>
    </label>
  );
}

function LocationStatus({
  status,
  isResolvingAddress,
}: {
  status: "idle" | "locating" | "resolved" | "denied" | "unavailable";
  isResolvingAddress: boolean;
}) {
  if (status === "resolved" && isResolvingAddress) {
    return <span className="text-xs font-medium text-ink-3">Finding address…</span>;
  }
  if (status === "resolved") {
    return <span className="text-xs font-medium text-leaf">Location detected ✓</span>;
  }
  if (status === "locating") {
    return <span className="text-xs font-medium text-ink-3">Detecting…</span>;
  }
  if (status === "denied" || status === "unavailable") {
    return <span className="text-xs font-medium text-error">Couldn&apos;t detect location</span>;
  }
  return null;
}

const TODAY_ISO = new Date().toISOString().slice(0, 10);

export function RegisterIndividualPage() {
  const navigate = useNavigate();
  const { session, setSession } = useAuth();
  const toast = useToast();
  const {
    values,
    setFullName,
    setEmail,
    setBloodGroup,
    setDateOfBirth,
    setGender,
    setLocationCityArea,
    setIsChronicIllness,
    setHasRecentSurgery,
    setIsInfectiousDisease,
    setIsUnderweight,
    setIsOtherIllness,
    setOtherIllnessDetails,
    fieldErrors,
    touched,
    geolocation,
    submit,
    isPending,
    error,
  } = useIndividualRegistration(session?.mobileNumber);

  const eligibility = previewEligibility(values);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = await submit();
    if (result.ok && result.data) {
      toast.success(
        result.data.isReceiverOnly
          ? "Profile created — you can request blood, but won't appear in donor search."
          : "Profile created — you're now an eligible donor.",
      );
      if (session) {
        setSession({ ...session, role: "Individual" });
      }
      navigate("/dashboard/individual", { replace: true });
    } else if (result.error) {
      toast.error(result.error.message);
    }
  }

  return (
    <AuthSplitLayout imageSrc="/images/auth-mobile-entry.png" imageAlt="">
      <div className="flex min-h-screen flex-col bg-sand font-sans text-ink">
        <div className="flex h-[58px] flex-none items-center gap-3 border-b border-line bg-cream px-4">
          <a
            href="/dashboard/guest"
            onClick={(e) => {
              e.preventDefault();
              navigate("/dashboard/guest");
            }}
            className="inline-flex items-center gap-1 text-sm font-semibold text-ink-2 underline underline-offset-2 hover:text-ink"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M19 12H5M11 6l-6 6 6 6" />
            </svg>
            Back
          </a>
          <b className="mr-9 flex-1 text-center text-[15px] font-bold">Complete your profile</b>
        </div>

        <form onSubmit={handleSubmit} noValidate className="flex flex-1 flex-col gap-6 px-7 py-6">
          <div>
            <h1 className="mb-1 text-[22px] font-extrabold tracking-tight">Personal details</h1>
            <p className="text-[13.5px] text-ink-2">So we can match you correctly as a donor or receiver.</p>
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="full-name" className="text-sm font-medium text-ink-2">
              Full name
            </label>
            <input
              id="full-name"
              type="text"
              value={values.fullName ?? ""}
              onChange={(e) => setFullName(e.target.value)}
              aria-describedby="full-name-error"
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.fullName && (
              <p id="full-name-error" className="text-xs text-error">
                {fieldErrors.fullName[0]}
              </p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="email" className="text-sm font-medium text-ink-2">
              Email
            </label>
            <input
              id="email"
              type="email"
              value={values.email ?? ""}
              onChange={(e) => setEmail(e.target.value)}
              aria-describedby="email-error"
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.email && (
              <p id="email-error" className="text-xs text-error">
                {fieldErrors.email[0]}
              </p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-ink-2">Blood group</span>
            <div className="grid grid-cols-4 gap-2">
              {BLOOD_GROUPS.map((group) => (
                <ChipButton
                  key={group}
                  label={group}
                  selected={values.bloodGroup === group}
                  onClick={() => setBloodGroup(group as BloodGroup)}
                />
              ))}
            </div>
            {touched && fieldErrors.bloodGroup && <p className="text-xs text-error">{fieldErrors.bloodGroup[0]}</p>}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="dob" className="text-sm font-medium text-ink-2">
              Date of birth
            </label>
            <input
              id="dob"
              type="date"
              max={TODAY_ISO}
              value={values.dateOfBirth ?? ""}
              onChange={(e) => setDateOfBirth(e.target.value)}
              aria-describedby="dob-error"
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.dateOfBirth && (
              <p id="dob-error" className="text-xs text-error">
                {fieldErrors.dateOfBirth[0]}
              </p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-ink-2">Gender</span>
            <div className="grid grid-cols-3 gap-2">
              {GENDERS.map((g) => (
                <ChipButton key={g} label={g} selected={values.gender === g} onClick={() => setGender(g as Gender)} />
              ))}
            </div>
            {touched && fieldErrors.gender && <p className="text-xs text-error">{fieldErrors.gender[0]}</p>}
          </div>

          <div className="border-t border-line pt-6">
            <div className="mb-1 flex items-center justify-between">
              <h2 className="text-[18px] font-extrabold tracking-tight">Health screening</h2>
              <span
                className={`rounded-full px-3 py-1 text-xs font-semibold ${
                  eligibility === "EligibleDonor" ? "bg-leaf-tint text-leaf" : "bg-amber-tint text-amber"
                }`}
              >
                {eligibility === "EligibleDonor" ? "Eligible Donor" : "Receiver Only"}
              </span>
            </div>
            <p className="mb-4 text-[13.5px] text-ink-2">
              Any restriction below keeps you eligible to request blood, but excludes you from donor search.
            </p>

            <div className="flex flex-col gap-2">
              <HealthFlagRow
                id="health-chronic"
                label="Chronic illness"
                checked={values.isChronicIllness ?? false}
                onChange={setIsChronicIllness}
              />
              <HealthFlagRow
                id="health-surgery"
                label="Recent surgery"
                checked={values.hasRecentSurgery ?? false}
                onChange={setHasRecentSurgery}
              />
              <HealthFlagRow
                id="health-infectious"
                label="Infectious disease"
                checked={values.isInfectiousDisease ?? false}
                onChange={setIsInfectiousDisease}
              />
              <HealthFlagRow
                id="health-underweight"
                label="Underweight"
                checked={values.isUnderweight ?? false}
                onChange={setIsUnderweight}
              />
              <HealthFlagRow
                id="health-other"
                label="Other"
                checked={values.isOtherIllness ?? false}
                onChange={setIsOtherIllness}
              />
            </div>

            {values.isOtherIllness && (
              <div className="mt-3 flex flex-col gap-1.5">
                <label htmlFor="other-illness-details" className="text-sm font-medium text-ink-2">
                  Specify other illness
                </label>
                <input
                  id="other-illness-details"
                  type="text"
                  maxLength={OTHER_ILLNESS_MAX_LENGTH}
                  value={values.otherIllnessDetails ?? ""}
                  onChange={(e) => setOtherIllnessDetails(e.target.value)}
                  aria-describedby="other-illness-error"
                  className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
                />
                {touched && fieldErrors.otherIllnessDetails && (
                  <p id="other-illness-error" className="text-xs text-error">
                    {fieldErrors.otherIllnessDetails[0]}
                  </p>
                )}
              </div>
            )}
          </div>

          <div className="border-t border-line pt-6">
            <div className="mb-1.5 flex items-center justify-between">
              <label htmlFor="location" className="text-sm font-medium text-ink-2">
                Location (city/area)
              </label>
              <LocationStatus status={geolocation.status} isResolvingAddress={geolocation.isResolvingAddress} />
            </div>
            <div className="relative">
              <input
                id="location"
                type="text"
                value={values.locationCityArea ?? ""}
                onChange={(e) => setLocationCityArea(e.target.value)}
                placeholder="e.g. Kaloor, Kochi, 682017"
                aria-describedby="location-error"
                className="h-12 w-full rounded-sm border-[1.5px] border-line-strong bg-cream pl-4 pr-11 text-base outline-none transition-colors focus:border-clay"
              />
              <button
                type="button"
                onClick={() => geolocation.request()}
                disabled={geolocation.status === "locating"}
                aria-label="Use current location"
                title="Use current location"
                className="absolute right-1.5 top-1.5 flex h-9 w-9 items-center justify-center rounded-sm text-clay transition-colors hover:bg-clay-tint disabled:opacity-60"
              >
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <path d="M12 2a7 7 0 0 0-7 7c0 5.25 7 13 7 13s7-7.75 7-13a7 7 0 0 0-7-7Z" />
                  <circle cx="12" cy="9" r="2.5" />
                </svg>
              </button>
            </div>
            {touched && fieldErrors.locationCityArea && (
              <p id="location-error" className="text-xs text-error">
                {fieldErrors.locationCityArea[0]}
              </p>
            )}
            {(geolocation.status === "denied" || geolocation.status === "unavailable") && (
              <p className="text-xs text-error">
                We couldn&apos;t determine your location. Please select your City/Area manually.
              </p>
            )}
          </div>

          {error && (
            <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
              {error.message}
            </div>
          )}

          <button
            type="submit"
            disabled={isPending}
            className={`mt-2 h-[54px] w-full rounded-md text-base font-semibold shadow-[var(--e1)] transition-all ${
              isPending
                ? "cursor-not-allowed bg-sand-2 text-ink-off shadow-none"
                : "bg-clay text-white hover:bg-clay-hover active:scale-[0.99]"
            }`}
          >
            {isPending ? "Creating profile…" : "Complete registration"}
          </button>
        </form>
      </div>
    </AuthSplitLayout>
  );
}
