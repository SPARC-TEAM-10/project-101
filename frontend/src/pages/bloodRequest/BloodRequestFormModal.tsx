import { useNavigate } from "react-router-dom";

import { DripLoader } from "../../components/DripLoader";
import { useAuth } from "../../context/AuthProvider";
import { useToast } from "../../context/ToastProvider";
import { useCreateBloodRequest } from "../../features/bloodRequest/useCreateBloodRequest";
import { BLOOD_GROUPS, URGENCY_LEVELS, type BloodGroup, type UrgencyLevel } from "../../lib/validation/bloodRequestSchemas";

type ChipVariant = "clay" | "blood" | "amber" | "leaf";

const CHIP_SELECTED_CLASSES: Record<ChipVariant, string> = {
  clay: "border-clay bg-clay text-white",
  blood: "border-blood bg-blood text-white",
  amber: "border-amber bg-amber text-white",
  leaf: "border-leaf bg-leaf text-white",
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

// Emergency/Urgent/Standard get visually distinct colors (red/amber/green) so urgency reads at
// a glance, matching the same signal convention as a traffic light.
const URGENCY_VARIANTS: Record<UrgencyLevel, ChipVariant> = {
  Emergency: "blood",
  Urgent: "amber",
  Standard: "leaf",
};

const RADIUS_PRESETS_KM = [5, 10, 25, 50, 100];

// Schematic radius preview, not a real interactive map — no maps/geocoding API key is
// configured yet (backend/CLAUDE.md's Tech Stack row). Concentric rings are illustrative only;
// they don't represent real-world scale. Replace with a real map once a provider is chosen.
function RadiusPreview({ radiusKm, minRadiusKm, maxRadiusKm }: { radiusKm: number; minRadiusKm: number; maxRadiusKm: number }) {
  const clamped = Math.min(Math.max(radiusKm, minRadiusKm), maxRadiusKm);
  const fraction = (clamped - minRadiusKm) / (maxRadiusKm - minRadiusKm);
  const size = 36 + fraction * 56;

  return (
    <div className="relative flex h-36 w-full items-center justify-center overflow-hidden rounded-lg bg-gradient-to-br from-clay-tint to-sand-2">
      <div
        className="absolute rounded-full border-2 border-clay/40 bg-clay/10 transition-[width,height] duration-200"
        style={{ width: `${size}%`, height: `${size}%` }}
        aria-hidden="true"
      />
      <div
        className="absolute rounded-full border-2 border-clay bg-clay-tint transition-[width,height] duration-200"
        style={{ width: `${size * 0.55}%`, height: `${size * 0.55}%` }}
        aria-hidden="true"
      />
      <div className="relative flex h-4 w-4 items-center justify-center rounded-full bg-blood ring-4 ring-white/70" aria-hidden="true" />
      <span className="sr-only">Search radius preview: {radiusKm} kilometers</span>
      <span className="absolute bottom-2.5 right-3 rounded-full bg-cream/90 px-2.5 py-1 text-xs font-semibold text-ink shadow-sm">
        {radiusKm} km
      </span>
    </div>
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

const FORM_ID = "blood-request-form";

export function BloodRequestFormModal() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const toast = useToast();
  const {
    values,
    setPatientName,
    setBloodGroup,
    setUnitsRequired,
    setLocationCityArea,
    setSearchRadiusKm,
    setUrgency,
    fieldErrors,
    touched,
    geolocation,
    minRadiusKm,
    maxRadiusKm,
    submit,
    isPending,
    error,
  } = useCreateBloodRequest(session?.token);

  function close() {
    navigate("/");
  }

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = await submit();
    if (result.ok && result.data) {
      toast.success("Blood request created — notifying nearby donors.");
      navigate("/", { state: { bloodRequestCreated: true, id: result.data.id } });
    } else if (result.error) {
      toast.error(result.error.message);
    }
  }

  const radius = values.searchRadiusKm ?? minRadiusKm;
  const radiusFraction = (radius - minRadiusKm) / (maxRadiusKm - minRadiusKm);
  const locationNotReady = touched && geolocation.status === "idle";

  return (
    <div
      className="fixed inset-0 z-50 flex items-end justify-center bg-[var(--overlay)] p-0 font-sans sm:items-center sm:p-6"
      onClick={close}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby="blood-request-modal-title"
        onClick={(e) => e.stopPropagation()}
        className="flex max-h-[92vh] w-full flex-col overflow-hidden rounded-t-xl bg-sand shadow-[var(--e3)] sm:max-w-2xl sm:rounded-xl"
      >
        {/* Header — pinned above the scrollable body */}
        <div className="flex shrink-0 items-center justify-between border-b border-line bg-sand px-6 py-4">
          <div>
            <h1 id="blood-request-modal-title" className="text-lg font-extrabold tracking-tight text-ink">
              Request blood
            </h1>
            <p className="text-xs text-ink-2">We&apos;ll notify eligible donors nearby.</p>
          </div>
          <button
            type="button"
            onClick={close}
            aria-label="Close"
            className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-ink-3 transition-colors hover:bg-sand-2 hover:text-ink"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" aria-hidden="true">
              <path d="M18 6 6 18M6 6l12 12" />
            </svg>
          </button>
        </div>

        {/* Body — the only scrollable region, so header/footer stay put */}
        <form
          id={FORM_ID}
          onSubmit={handleSubmit}
          noValidate
          className="modal-scroll flex flex-1 flex-col gap-5 overflow-y-auto px-6 py-5"
        >
          <div className="flex flex-col gap-1.5">
            <label htmlFor="patient-name" className="text-sm font-medium text-ink-2">
              Patient name
            </label>
            <input
              id="patient-name"
              type="text"
              value={values.patientName ?? ""}
              onChange={(e) => setPatientName(e.target.value)}
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.patientName && (
              <p className="text-xs text-error">{fieldErrors.patientName[0]}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-ink-2">Blood group</span>
            <div className="grid grid-cols-4 gap-2">
              {BLOOD_GROUPS.map((group) => (
                <ChipButton
                  key={group}
                  label={group}
                  variant="blood"
                  selected={values.bloodGroup === group}
                  onClick={() => setBloodGroup(group as BloodGroup)}
                />
              ))}
            </div>
            {touched && fieldErrors.bloodGroup && (
              <p className="text-xs text-error">{fieldErrors.bloodGroup[0]}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="units-required" className="text-sm font-medium text-ink-2">
              Units required
            </label>
            <div className="flex h-12 w-36 items-center rounded-sm border-[1.5px] border-line-strong bg-cream px-2">
              <button
                type="button"
                aria-label="Decrease units"
                onClick={() => setUnitsRequired(Math.max(1, (values.unitsRequired ?? 1) - 1))}
                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-sm text-lg font-semibold text-ink-2 hover:bg-sand-2"
              >
                −
              </button>
              <input
                id="units-required"
                type="number"
                min={1}
                value={values.unitsRequired ?? 1}
                onChange={(e) => setUnitsRequired(Math.max(1, Number(e.target.value) || 1))}
                className="h-full w-full border-0 bg-transparent text-center text-base outline-none"
              />
              <button
                type="button"
                aria-label="Increase units"
                onClick={() => setUnitsRequired((values.unitsRequired ?? 0) + 1)}
                className="flex h-8 w-8 shrink-0 items-center justify-center rounded-sm text-lg font-semibold text-ink-2 hover:bg-sand-2"
              >
                +
              </button>
            </div>
            {touched && fieldErrors.unitsRequired && (
              <p className="text-xs text-error">{fieldErrors.unitsRequired[0]}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <div className="flex items-center justify-between">
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
              <p className="text-xs text-error">{fieldErrors.locationCityArea[0]}</p>
            )}
            {(geolocation.status === "denied" || geolocation.status === "unavailable") && (
              <p className="text-xs text-error">
                We couldn&apos;t determine your location. Please type it manually or try again.
              </p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <span className="text-sm font-medium text-ink-2">Urgency</span>
            <div className="grid grid-cols-3 gap-2">
              {URGENCY_LEVELS.map((level) => (
                <ChipButton
                  key={level}
                  label={level}
                  variant={URGENCY_VARIANTS[level]}
                  selected={values.urgency === level}
                  onClick={() => setUrgency(level as UrgencyLevel)}
                />
              ))}
            </div>
            {touched && fieldErrors.urgency && <p className="text-xs text-error">{fieldErrors.urgency[0]}</p>}
          </div>

          <div className="flex flex-col gap-3 rounded-xl border border-line bg-cream/70 p-4 shadow-[var(--e1)]">
            <div className="flex items-center gap-2.5">
              <div className="flex h-9 w-9 shrink-0 items-center justify-center rounded-full bg-blood-tint text-blood-deep">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={2} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
                  <circle cx="12" cy="12" r="3" />
                  <path d="M12 2v3M12 19v3M22 12h-3M5 12H2M19.07 4.93l-2.12 2.12M7.05 16.95l-2.12 2.12M19.07 19.07l-2.12-2.12M7.05 7.05 4.93 4.93" />
                </svg>
              </div>
              <div>
                <label htmlFor="radius-slider" className="block text-sm font-semibold text-ink">
                  Search radius
                </label>
                <p className="text-xs text-ink-2">Donors within this range get notified.</p>
              </div>
              <span className="ml-auto rounded-full bg-blood px-3 py-1.5 text-sm font-bold text-white shadow-sm">
                {radius} km
              </span>
            </div>

            <div className="flex gap-1.5">
              {RADIUS_PRESETS_KM.filter((preset) => preset >= minRadiusKm && preset <= maxRadiusKm).map((preset) => (
                <button
                  key={preset}
                  type="button"
                  onClick={() => setSearchRadiusKm(preset)}
                  className={`h-8 flex-1 rounded-full text-xs font-semibold transition-colors ${
                    radius === preset
                      ? "bg-clay text-white"
                      : "bg-cream text-ink-2 ring-1 ring-inset ring-line-strong hover:bg-sand-2"
                  }`}
                >
                  {preset}km
                </button>
              ))}
            </div>

            <input
              id="radius-slider"
              type="range"
              min={minRadiusKm}
              max={maxRadiusKm}
              value={radius}
              onChange={(e) => setSearchRadiusKm(Number(e.target.value))}
              className="radius-slider w-full"
              style={{ ["--fill" as string]: `${radiusFraction * 100}%` }}
              aria-label="Search radius in kilometers"
            />
            <div className="flex justify-between text-xs text-ink-3">
              <span>{minRadiusKm} km</span>
              <span>{maxRadiusKm} km</span>
            </div>
            {touched && fieldErrors.searchRadiusKm && (
              <p className="text-xs text-error">{fieldErrors.searchRadiusKm[0]}</p>
            )}
            <RadiusPreview radiusKm={radius} minRadiusKm={minRadiusKm} maxRadiusKm={maxRadiusKm} />
          </div>
        </form>

        {/* Footer — pinned below the scrollable body */}
        <div className="shrink-0 border-t border-line bg-sand px-6 py-4">
          {locationNotReady && (
            <div className="mb-3 rounded-sm border border-amber bg-amber-tint px-4 py-3 text-sm text-amber">
              Tap the location pin above before submitting.
            </div>
          )}
          {error && (
            <div className="mb-3 rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
              {error.message}
            </div>
          )}
          <button
            form={FORM_ID}
            type="submit"
            disabled={isPending}
            className={`flex h-[54px] w-full items-center justify-center gap-2 rounded-md text-base font-semibold shadow-[var(--e1)] transition-all ${
              isPending
                ? "cursor-not-allowed bg-sand-2 text-ink-off shadow-none"
                : "bg-gradient-to-r from-blood to-blood-hover text-white hover:shadow-[var(--e2)] active:scale-[0.99]"
            }`}
          >
            {isPending ? (
              <>
                <DripLoader size="btn" />
                Notifying donors…
              </>
            ) : (
              <>
                <svg width="18" height="18" viewBox="0 0 24 24" fill="currentColor" aria-hidden="true">
                  <path d="M12 3.2c3.4 4 6 6.9 6 10a6 6 0 0 1-12 0c0-3.1 2.6-6 6-10Z" />
                </svg>
                Notify donors
              </>
            )}
          </button>
        </div>
      </div>
    </div>
  );
}
