import { useNavigate } from "react-router-dom";

import { BrandPanel } from "../../components/BrandPanel";
import { DateField } from "../../components/DateField";
import { LoadingOverlay } from "../../components/LoadingOverlay";
import { SelectField } from "../../components/SelectField";
import { useAuth } from "../../context/AuthProvider";
import { useToast } from "../../context/ToastProvider";
import { getMobileNumberFromToken } from "../../lib/authToken";
import { useIndividualRegistration } from "../../features/individual/useIndividualRegistration";
import { BLOOD_GROUPS, type BloodGroup } from "../../lib/validation/bloodRequestSchemas";
import { GENDERS, MAX_OTHER_ILLNESS_LENGTH, type Gender } from "../../lib/validation/individualSchemas";

const BLOOD_GROUP_OPTIONS = BLOOD_GROUPS.map((group) => ({ value: group, label: group }));
const GENDER_OPTIONS = GENDERS.map((gender) => ({ value: gender, label: gender }));

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

function LocationHint({
  status,
  isResolvingAddress,
}: {
  status: "idle" | "locating" | "resolved" | "denied" | "unavailable";
  isResolvingAddress: boolean;
}) {
  if (status === "resolved" && isResolvingAddress) {
    return <span className="text-[12.5px] font-medium text-ink-3">Finding your area…</span>;
  }
  if (status === "locating") {
    return <span className="text-[12.5px] font-medium text-ink-3">Detecting…</span>;
  }
  return null;
}

const TODAY = new Date().toISOString().slice(0, 10);

// Real CHH-F02 individual registration form (US-CHH-002-01/02/03) — reached via
// /register/individual, after RoleSelectionPage. Kept as RegisterStubPage (name and file both)
// to match the GuestDashboardStubPage precedent of not renaming a route's component once the
// real feature lands, so router.tsx doesn't churn.
//
// Layout mirrors the two design canvas artboards: Registration.dc.html (mobile — single-column
// scroll, appbar title) and RegistrationWeb{Enabled,Disabled}.dc.html (web — "Almost there." panel
// on the left, a 2-column field grid on the right). Split at `md:`, same breakpoint every other
// auth/onboarding screen's BrandPanel split uses.
export function RegisterStubPage() {
  const navigate = useNavigate();
  const { session, setSession } = useAuth();
  const toast = useToast();
  const mobileNumber = session ? getMobileNumberFromToken(session.token) : null;
  const {
    values,
    setFullName,
    setEmail,
    setBloodGroup,
    setDateOfBirth,
    setGender,
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
    isReceiverOnly,
    isPending,
    error,
    submit,
  } = useIndividualRegistration(mobileNumber);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = await submit();
    if (result.ok && result.data) {
      toast.success("Account created. Welcome to Community Health Hub!");
      if (session) {
        setSession({ ...session, role: "Individual" });
      }
      navigate("/redirecting");
    } else if (result.error) {
      toast.error(result.error.message);
    }
  }

  const otherIllnessLength = (values.otherIllnessDetails ?? "").length;

  return (
    <div className="flex min-h-screen flex-col bg-sand font-sans text-ink md:grid md:grid-cols-[420px_1fr]">
      {isPending && <LoadingOverlay message="Creating your account…" />}

      {/* Left panel — desktop only (RegistrationWeb*.dc.html's ".left"), sticky so it stays in
          view while the form scrolls. */}
      <BrandPanel
        heading="Almost there."
        description="Your health screening decides your eligibility — donate, request, or both. You can update it any time from your profile."
        className="md:sticky md:top-0 md:h-screen"
      />

      {/* Right column */}
      <div className="flex flex-1 flex-col">
        <header className="flex h-[58px] flex-none items-center gap-2.5 border-b border-line bg-cream px-3 md:hidden">
          <button
            type="button"
            onClick={() => navigate("/register")}
            aria-label="Back"
            className="flex h-10 w-10 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2"
          >
            <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
              <path d="M19 12H5M11 6l-6 6 6 6" />
            </svg>
          </button>
          <b className="flex-1 text-center text-[15px] font-bold">Create Account</b>
          <span className="w-10" aria-hidden="true" />
        </header>

        <form
          onSubmit={handleSubmit}
          noValidate
          className="mx-auto flex w-full max-w-[680px] flex-1 flex-col gap-5 px-4 py-5 md:gap-0 md:px-10 md:py-14"
        >
          <div className="hidden md:block">
            <h1 className="mb-1.5 text-[26px] font-extrabold tracking-tight">Create your account</h1>
            <p className="mb-7 text-[14.5px] text-ink-2">Tell us a little about yourself.</p>
          </div>

          <h2 className="text-[11px] font-extrabold uppercase tracking-wide text-ink-3 md:mb-3.5">Personal details</h2>

          <div className="grid grid-cols-1 gap-4 md:grid-cols-2">
            <div className="flex flex-col gap-1.5">
              <label htmlFor="full-name" className="text-sm font-semibold text-ink-2">
                Full name <i className="not-italic text-error">*</i>
              </label>
              <input
                id="full-name"
                type="text"
                value={values.fullName ?? ""}
                onChange={(e) => setFullName(e.target.value)}
                placeholder="As per your ID"
                aria-invalid={touched && !!fieldErrors.fullName}
                className={`h-[50px] rounded-sm border-[1.5px] bg-cream px-4 text-base outline-none transition-colors focus:border-clay ${
                  touched && fieldErrors.fullName ? "border-error" : "border-line-strong"
                }`}
              />
              {touched && <FieldError message={fieldErrors.fullName?.[0]} />}
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="email" className="text-sm font-semibold text-ink-2">
                Email <i className="not-italic text-error">*</i>
              </label>
              <input
                id="email"
                type="email"
                value={values.email ?? ""}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="you@example.com"
                aria-invalid={touched && !!fieldErrors.email}
                className={`h-[50px] rounded-sm border-[1.5px] bg-cream px-4 text-base outline-none transition-colors focus:border-clay ${
                  touched && fieldErrors.email ? "border-error" : "border-line-strong"
                }`}
              />
              {touched && <FieldError message={fieldErrors.email?.[0]} />}
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="blood-group" className="text-sm font-semibold text-ink-2">
                Blood group <i className="not-italic text-error">*</i>
              </label>
              <SelectField
                id="blood-group"
                value={values.bloodGroup ?? ""}
                onChange={(v) => setBloodGroup(v as BloodGroup)}
                options={BLOOD_GROUP_OPTIONS}
                placeholder="Select blood group"
                invalid={touched && !!fieldErrors.bloodGroup}
                describedBy="blood-group-hint"
              />
              <span id="blood-group-hint">{touched && <FieldError message={fieldErrors.bloodGroup?.[0]} />}</span>
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="date-of-birth" className="text-sm font-semibold text-ink-2">
                Date of birth <i className="not-italic text-error">*</i>
              </label>
              <DateField
                id="date-of-birth"
                max={TODAY}
                value={values.dateOfBirth ?? ""}
                onChange={setDateOfBirth}
                invalid={touched && !!fieldErrors.dateOfBirth}
                describedBy="date-of-birth-hint"
              />
              <span id="date-of-birth-hint">{touched && <FieldError message={fieldErrors.dateOfBirth?.[0]} />}</span>
            </div>

            <div className="flex flex-col gap-1.5">
              <label htmlFor="gender" className="text-sm font-semibold text-ink-2">
                Gender <i className="not-italic text-error">*</i>
              </label>
              <SelectField
                id="gender"
                value={values.gender ?? ""}
                onChange={(v) => setGender(v as Gender)}
                options={GENDER_OPTIONS}
                placeholder="Select gender"
                invalid={touched && !!fieldErrors.gender}
                describedBy="gender-hint"
              />
              <span id="gender-hint">{touched && <FieldError message={fieldErrors.gender?.[0]} />}</span>
            </div>

            <div className="flex flex-col gap-1.5">
              <div className="flex items-center justify-between">
                <label htmlFor="location" className="text-sm font-semibold text-ink-2">
                  Location (City / Area) <i className="not-italic text-error">*</i>
                </label>
                <LocationHint status={geolocation.status} isResolvingAddress={geolocation.isResolvingAddress} />
              </div>
              <div className="relative">
                <input
                  id="location"
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
                  <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                    <path d="M12 2a7 7 0 0 0-7 7c0 5.25 7 13 7 13s7-7.75 7-13a7 7 0 0 0-7-7Z" />
                    <circle cx="12" cy="9" r="2.5" />
                  </svg>
                </button>
              </div>
              {touched && <FieldError message={fieldErrors.locationCityArea?.[0]} />}
              {(geolocation.status === "denied" || geolocation.status === "unavailable") && (
                <span className="flex items-start gap-1.5 rounded-sm bg-amber-tint px-2.5 py-2 text-[12px] leading-tight text-amber">
                  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" className="mt-0.5 flex-none" aria-hidden="true">
                    <path d="M12 4.5 21 19.5H3L12 4.5Z" />
                    <path d="M12 10v4M12 16.8v.1" />
                  </svg>
                  We couldn&apos;t determine your location. Please select your City/Area manually.
                </span>
              )}
            </div>
          </div>

          <h2 className="mt-2 text-[11px] font-extrabold uppercase tracking-wide text-ink-3 md:mt-7 md:mb-3.5">Health screening</h2>

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
            <div className="flex flex-col gap-1.5 md:mt-3.5">
              <label htmlFor="other-illness" className="text-sm font-semibold text-ink-2">
                Specify other illness <i className="not-italic text-error">*</i>
              </label>
              <textarea
                id="other-illness"
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

          {isReceiverOnly ? (
            <div className="flex gap-2.5 rounded-sm bg-clay-tint p-3.5 text-ink md:mt-4">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="flex-none" aria-hidden="true">
                <circle cx="12" cy="12" r="8.5" />
                <path d="M12 11v5.5M12 7.9v.1" />
              </svg>
              <div>
                <b className="block text-[13px] font-bold">Receiver only</b>
                <p className="m-0 text-[12px] leading-relaxed opacity-85">
                  Based on your screening you can request blood but not donate. You can update this later.
                </p>
              </div>
            </div>
          ) : (
            <div className="flex gap-2.5 rounded-sm bg-leaf-tint p-3.5 text-ink md:mt-4">
              <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" className="flex-none text-leaf" aria-hidden="true">
                <circle cx="12" cy="12" r="8.5" />
                <path d="M8.4 12.3 11 15l4.6-5.4" />
              </svg>
              <div>
                <b className="block text-[13px] font-bold">Eligible donor</b>
                <p className="m-0 text-[12px] leading-relaxed opacity-85">
                  No restrictions flagged. You&apos;ll appear in donor search results.
                </p>
              </div>
            </div>
          )}

          {error && (
            <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error md:mt-4">
              {error.message}
            </div>
          )}

          <div className="pt-2 md:mt-7 md:pt-0">
            <button
              type="submit"
              disabled={isPending}
              className={`flex h-[52px] w-full items-center justify-center rounded-md text-[15px] font-semibold transition-colors md:w-[220px] ${
                isPending ? "cursor-not-allowed bg-sand-2 text-ink-off" : "bg-clay text-white hover:bg-clay-hover"
              }`}
            >
              {isPending ? "Creating account…" : "Create Account"}
            </button>
          </div>
        </form>
      </div>
    </div>
  );
}
