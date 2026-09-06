import { useNavigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { useCreateBloodRequest } from "../../features/bloodRequest/useCreateBloodRequest";
import { BLOOD_GROUPS, URGENCY_LEVELS, type BloodGroup, type UrgencyLevel } from "../../lib/validation/bloodRequestSchemas";

// Schematic radius preview, not a real interactive map — no maps/geocoding API key is
// configured yet (backend/CLAUDE.md's Tech Stack row). Circle size is illustrative only; it
// doesn't represent real-world scale. Replace with a real map once a provider is chosen.
function RadiusPreview({ radiusKm, minRadiusKm, maxRadiusKm }: { radiusKm: number; minRadiusKm: number; maxRadiusKm: number }) {
  const clamped = Math.min(Math.max(radiusKm, minRadiusKm), maxRadiusKm);
  const fraction = (clamped - minRadiusKm) / (maxRadiusKm - minRadiusKm);
  const size = 40 + fraction * 60; // 40%–100% of the preview box

  return (
    <div className="relative flex h-40 w-full items-center justify-center overflow-hidden rounded-md border border-line bg-cream">
      <div
        className="absolute rounded-full border-2 border-clay bg-clay-tint transition-all"
        style={{ width: `${size}%`, height: `${size}%` }}
        aria-hidden="true"
      />
      <div className="relative flex h-3 w-3 items-center justify-center rounded-full bg-blood shadow" aria-hidden="true" />
      <span className="sr-only">Search radius preview: {radiusKm} kilometers</span>
      <span className="absolute bottom-2 right-2 text-xs font-medium text-ink-2">{radiusKm}km radius</span>
    </div>
  );
}

export function BloodRequestFormPage() {
  const navigate = useNavigate();
  const { session } = useAuth();
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

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = await submit();
    if (result.ok && result.data) {
      navigate("/", { state: { bloodRequestCreated: true, id: result.data.id } });
    }
  }

  const radius = values.searchRadiusKm ?? minRadiusKm;
  const locationBlocked = geolocation.status === "denied" || geolocation.status === "unavailable";

  return (
    <div className="min-h-screen bg-sand px-5 py-8 font-sans text-ink">
      <div className="mx-auto max-w-md">
        <h1 className="mb-1 text-[24px] font-extrabold tracking-tight">Request blood</h1>
        <p className="mb-6 text-[14.5px] text-ink-2">
          We&apos;ll notify eligible donors within your chosen radius.
        </p>

        <form onSubmit={handleSubmit} noValidate className="flex flex-col gap-5">
          <div className="flex flex-col gap-1.5">
            <label htmlFor="patient-name" className="text-sm font-medium text-ink-2">
              Patient name
            </label>
            <input
              id="patient-name"
              type="text"
              value={values.patientName ?? ""}
              onChange={(e) => setPatientName(e.target.value)}
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base"
            />
            {touched && fieldErrors.patientName && (
              <p className="text-xs text-error">{fieldErrors.patientName[0]}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="blood-group" className="text-sm font-medium text-ink-2">
              Blood group
            </label>
            <select
              id="blood-group"
              value={values.bloodGroup ?? ""}
              onChange={(e) => setBloodGroup(e.target.value as BloodGroup)}
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base"
            >
              <option value="" disabled>
                Select blood group
              </option>
              {BLOOD_GROUPS.map((group) => (
                <option key={group} value={group}>
                  {group}
                </option>
              ))}
            </select>
            {touched && fieldErrors.bloodGroup && (
              <p className="text-xs text-error">{fieldErrors.bloodGroup[0]}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="units-required" className="text-sm font-medium text-ink-2">
              Units required
            </label>
            <input
              id="units-required"
              type="number"
              min={1}
              value={values.unitsRequired ?? ""}
              onChange={(e) => setUnitsRequired(Number(e.target.value))}
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base"
            />
            {touched && fieldErrors.unitsRequired && (
              <p className="text-xs text-error">{fieldErrors.unitsRequired[0]}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="location" className="text-sm font-medium text-ink-2">
              Location (city/area)
            </label>
            <input
              id="location"
              type="text"
              value={values.locationCityArea ?? ""}
              onChange={(e) => setLocationCityArea(e.target.value)}
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base"
            />
            {touched && fieldErrors.locationCityArea && (
              <p className="text-xs text-error">{fieldErrors.locationCityArea[0]}</p>
            )}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="urgency" className="text-sm font-medium text-ink-2">
              Urgency
            </label>
            <select
              id="urgency"
              value={values.urgency ?? ""}
              onChange={(e) => setUrgency(e.target.value as UrgencyLevel)}
              className="h-12 rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base"
            >
              <option value="" disabled>
                Select urgency
              </option>
              {URGENCY_LEVELS.map((level) => (
                <option key={level} value={level}>
                  {level}
                </option>
              ))}
            </select>
            {touched && fieldErrors.urgency && <p className="text-xs text-error">{fieldErrors.urgency[0]}</p>}
          </div>

          <div className="flex flex-col gap-2">
            <div className="flex items-center justify-between">
              <label htmlFor="radius-slider" className="text-sm font-medium text-ink-2">
                Search radius
              </label>
              <input
                type="number"
                aria-label="Search radius in kilometers"
                min={minRadiusKm}
                max={maxRadiusKm}
                value={radius}
                onChange={(e) => setSearchRadiusKm(Number(e.target.value))}
                className="h-9 w-20 rounded-sm border-[1.5px] border-line-strong bg-cream px-2 text-right text-sm"
              />
            </div>
            <input
              id="radius-slider"
              type="range"
              min={minRadiusKm}
              max={maxRadiusKm}
              value={radius}
              onChange={(e) => setSearchRadiusKm(Number(e.target.value))}
              className="w-full accent-clay"
            />
            {touched && fieldErrors.searchRadiusKm && (
              <p className="text-xs text-error">{fieldErrors.searchRadiusKm[0]}</p>
            )}
            <RadiusPreview radiusKm={radius} minRadiusKm={minRadiusKm} maxRadiusKm={maxRadiusKm} />
          </div>

          {locationBlocked && (
            <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
              We couldn&apos;t determine your location. Please enable location access and try
              again.
            </div>
          )}

          {error && (
            <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">
              {error.message}
            </div>
          )}

          <button
            type="submit"
            disabled={isPending}
            className={`h-[54px] w-full rounded-md text-base font-semibold transition-colors ${
              isPending ? "cursor-not-allowed bg-sand-2 text-ink-off" : "bg-blood text-white hover:bg-blood-hover"
            }`}
          >
            {isPending ? "Notifying donors…" : "Notify donors"}
          </button>
        </form>
      </div>
    </div>
  );
}
