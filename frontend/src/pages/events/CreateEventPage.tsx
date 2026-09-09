import { useNavigate } from "react-router-dom";

import { VenuePinMap } from "../../components/VenuePinMap";
import { useAuth } from "../../context/AuthProvider";
import { useToast } from "../../context/ToastProvider";
import { useCreateEvent } from "../../features/events/useCreateEvent";
import { EVENT_TYPES, EVENT_TYPE_LABELS, MAX_DESCRIPTION_LENGTH } from "../../lib/validation/eventSchemas";

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

function Hint({ children }: { children: React.ReactNode }) {
  return <span className="text-[12.5px] leading-tight text-ink-3">{children}</span>;
}

const DEFAULT_LATITUDE = 9.9312;
const DEFAULT_LONGITUDE = 76.2673;

/**
 * Event creation form (CHH-38/US-CHH-005-01), matching the approved design at
 * design/drafts/CHH-37-artboards/{CreateEvent,CreateEventWeb}.dc.html — one scrolling form
 * (not a wizard), reachable only once CHH-28's dashboard "Plan an event" tile unlocks (facility
 * must be Verified — enforced again server-side, this page only assumes the role check passed).
 */
export function CreateEventPage() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const toast = useToast();
  const {
    values,
    setTitle,
    setEventType,
    setDescription,
    setVenueName,
    setVenueAddress,
    setStartAtUtc,
    setEndAtUtc,
    setCapacity,
    setCoordinatorName,
    setCoordinatorContact,
    setRsvpCutoffEnabled,
    setRsvpCutoffAtUtc,
    geocoding,
    setManualVenueCoordinates,
    fieldErrors,
    touched,
    submit,
    isPending,
    error,
  } = useCreateEvent(session?.token);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    const result = await submit();
    if (result.ok) {
      toast.success("Event published.");
      navigate("/dashboard/facility");
    } else if (result.error) {
      toast.error(result.error.message);
    }
  }

  const pinLat = geocoding.coordinates?.latitude ?? DEFAULT_LATITUDE;
  const pinLng = geocoding.coordinates?.longitude ?? DEFAULT_LONGITUDE;
  const descriptionLength = (values.description ?? "").length;

  return (
    <div className="flex min-h-screen flex-col bg-sand font-sans text-ink">
      <div className="flex h-[58px] flex-none items-center gap-2.5 border-b border-line bg-cream px-3 md:h-16 md:px-8">
        <button
          type="button"
          onClick={() => navigate("/dashboard/facility")}
          aria-label="Back"
          className="flex h-10 w-10 items-center justify-center rounded-full text-ink-2 transition-colors hover:bg-sand-2"
        >
          <svg width="20" height="20" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth={1.75} strokeLinecap="round" strokeLinejoin="round" aria-hidden="true">
            <path d="M19 12H5M11 6l-6 6 6 6" />
          </svg>
        </button>
        <b className="flex-1 text-center text-[15px] font-bold md:text-left md:text-base">New event</b>
        <span className="hidden w-10 md:block" aria-hidden="true" />
      </div>

      <form
        onSubmit={handleSubmit}
        noValidate
        className="mx-auto flex w-full max-w-3xl flex-1 flex-col gap-5 self-center px-4 py-5 md:px-8 md:py-8 lg:flex-row lg:items-start lg:gap-8"
      >
        <div className="flex flex-1 flex-col gap-5">
          <div className="flex flex-col gap-1.5">
            <label htmlFor="event-title" className="text-sm font-semibold text-ink-2">
              Event title <i className="not-italic text-error">*</i>
            </label>
            <input
              id="event-title"
              type="text"
              value={values.title ?? ""}
              onChange={(e) => setTitle(e.target.value)}
              placeholder="e.g. Community blood drive — Kaloor"
              className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.title ? <FieldError message={fieldErrors.title[0]} /> : <Hint>5–100 characters.</Hint>}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="event-type" className="text-sm font-semibold text-ink-2">
              Event type <i className="not-italic text-error">*</i>
            </label>
            <select
              id="event-type"
              value={values.eventType ?? ""}
              onChange={(e) => setEventType(e.target.value as (typeof EVENT_TYPES)[number])}
              className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            >
              <option value="" disabled>
                Select a type
              </option>
              {EVENT_TYPES.map((type) => (
                <option key={type} value={type}>
                  {EVENT_TYPE_LABELS[type]}
                </option>
              ))}
            </select>
            {touched && fieldErrors.eventType && <FieldError message={fieldErrors.eventType[0]} />}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="event-description" className="text-sm font-semibold text-ink-2">
              Description <i className="not-italic text-error">*</i>
            </label>
            <div className="relative">
              <textarea
                id="event-description"
                value={values.description ?? ""}
                onChange={(e) => setDescription(e.target.value)}
                placeholder="Walk-in donors welcome. Bring a photo ID."
                className="min-h-[92px] w-full resize-y rounded-sm border-[1.5px] border-line-strong bg-cream px-4 py-3 pr-16 text-[15px] leading-relaxed outline-none transition-colors focus:border-clay"
              />
              <span className="pointer-events-none absolute bottom-2 right-3 text-xs text-ink-3 [font-variant-numeric:tabular-nums]">
                {descriptionLength} / {MAX_DESCRIPTION_LENGTH}
              </span>
            </div>
            {touched && fieldErrors.description ? <FieldError message={fieldErrors.description[0]} /> : <Hint>At least 20 characters.</Hint>}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="venue-name" className="text-sm font-semibold text-ink-2">
              Venue name <i className="not-italic text-error">*</i>
            </label>
            <input
              id="venue-name"
              type="text"
              value={values.venueName ?? ""}
              onChange={(e) => setVenueName(e.target.value)}
              className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.venueName && <FieldError message={fieldErrors.venueName[0]} />}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="venue-address" className="text-sm font-semibold text-ink-2">
              Venue address <i className="not-italic text-error">*</i>
            </label>
            <textarea
              id="venue-address"
              value={values.venueAddress ?? ""}
              onChange={(e) => setVenueAddress(e.target.value)}
              placeholder="Building, street, area, city, PIN code"
              className="min-h-[66px] resize-y rounded-sm border-[1.5px] border-line-strong bg-cream px-4 py-3 text-base leading-relaxed outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.venueAddress && <FieldError message={fieldErrors.venueAddress[0]} />}
          </div>

          <div className="lg:hidden">
            <div className="flex flex-col gap-2 rounded-md border border-line bg-cream p-3">
              <VenuePinMap latitude={pinLat} longitude={pinLng} onMove={(lat, lng) => setManualVenueCoordinates({ latitude: lat, longitude: lng })} />
              <div className="flex items-center gap-2 text-xs text-ink-2 [font-variant-numeric:tabular-nums]">
                {geocoding.status === "resolving" && <span>Finding the map location…</span>}
                {geocoding.status === "resolved" && geocoding.coordinates && (
                  <span>
                    Resolved from the address · {geocoding.coordinates.latitude.toFixed(3)} N, {geocoding.coordinates.longitude.toFixed(3)} E
                  </span>
                )}
                {geocoding.status === "notFound" && <span className="text-amber">Couldn&apos;t resolve that address — drag the pin to set it manually.</span>}
                <span className="ml-auto font-semibold text-clay">Move pin</span>
              </div>
              {touched && fieldErrors.latitude && <FieldError message="Please select a valid location on the map." />}
            </div>
          </div>

          <div className="grid grid-cols-2 gap-3">
            <div className="flex flex-col gap-1.5">
              <label htmlFor="event-start" className="text-sm font-semibold text-ink-2">
                Starts <i className="not-italic text-error">*</i>
              </label>
              <input
                id="event-start"
                type="datetime-local"
                value={values.startAtUtc ?? ""}
                onChange={(e) => setStartAtUtc(e.target.value)}
                className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-cream px-3 text-[15px] outline-none transition-colors focus:border-clay"
              />
            </div>
            <div className="flex flex-col gap-1.5">
              <label htmlFor="event-end" className="text-sm font-semibold text-ink-2">
                Ends <i className="not-italic text-error">*</i>
              </label>
              <input
                id="event-end"
                type="datetime-local"
                value={values.endAtUtc ?? ""}
                onChange={(e) => setEndAtUtc(e.target.value)}
                className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-cream px-3 text-[15px] outline-none transition-colors focus:border-clay"
              />
            </div>
          </div>
          {touched && (fieldErrors.startAtUtc || fieldErrors.endAtUtc) ? (
            <FieldError message={fieldErrors.startAtUtc?.[0] ?? fieldErrors.endAtUtc?.[0]} />
          ) : (
            <Hint>Must start at least 1 hour from now, and end after it starts.</Hint>
          )}

          <div className="flex flex-col gap-1.5">
            <label htmlFor="event-capacity" className="text-sm font-semibold text-ink-2">
              Capacity <i className="not-italic text-error">*</i>
            </label>
            <input
              id="event-capacity"
              type="number"
              min={1}
              max={1000}
              value={values.capacity ?? ""}
              onChange={(e) => setCapacity(Number(e.target.value))}
              className="h-[50px] w-full rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay [font-variant-numeric:tabular-nums]"
            />
            {touched && fieldErrors.capacity ? <FieldError message={fieldErrors.capacity[0]} /> : <Hint>Between 1 and 1,000.</Hint>}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="coordinator-name" className="text-sm font-semibold text-ink-2">
              Coordinator name <i className="not-italic text-error">*</i>
            </label>
            <input
              id="coordinator-name"
              type="text"
              value={values.coordinatorName ?? ""}
              onChange={(e) => setCoordinatorName(e.target.value)}
              className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-cream px-4 text-base outline-none transition-colors focus:border-clay"
            />
            {touched && fieldErrors.coordinatorName && <FieldError message={fieldErrors.coordinatorName[0]} />}
          </div>

          <div className="flex flex-col gap-1.5">
            <label htmlFor="coordinator-contact" className="text-sm font-semibold text-ink-2">
              Coordinator contact <i className="not-italic text-error">*</i>
            </label>
            <div className="flex">
              <span className="flex h-[50px] items-center rounded-l-sm border-[1.5px] border-r-0 border-line-strong bg-sand-2 px-3 font-mono text-[15px] text-ink-2">+91</span>
              <input
                id="coordinator-contact"
                type="tel"
                inputMode="numeric"
                value={values.coordinatorContact ?? ""}
                onChange={(e) => setCoordinatorContact(e.target.value)}
                placeholder="10 digits"
                className="h-[50px] w-full rounded-r-sm border-[1.5px] border-line-strong bg-cream px-3.5 font-mono text-base outline-none transition-colors focus:border-clay"
              />
            </div>
            {touched && fieldErrors.coordinatorContact ? (
              <FieldError message={fieldErrors.coordinatorContact[0]} />
            ) : (
              <Hint>Attendees see this number on the event page.</Hint>
            )}
          </div>

          <div className="flex flex-col gap-3 rounded-md border border-line bg-cream p-4">
            <div className="flex items-start gap-3">
              <div className="flex-1">
                <b className="text-[14.5px]">Close RSVPs early</b>
                <p className="mt-1 text-[12.5px] leading-tight text-ink-3">Off means RSVPs stay open until the event starts or the last place goes.</p>
              </div>
              <button
                type="button"
                role="switch"
                aria-checked={Boolean(values.rsvpCutoffEnabled)}
                aria-label="Close RSVPs early"
                onClick={() => setRsvpCutoffEnabled(!values.rsvpCutoffEnabled)}
                className={`relative mt-0.5 h-7 w-[46px] flex-none rounded-full transition-colors ${values.rsvpCutoffEnabled ? "bg-clay" : "bg-line-strong"}`}
              >
                <span
                  className={`absolute top-[3px] h-[22px] w-[22px] rounded-full bg-white transition-transform ${values.rsvpCutoffEnabled ? "translate-x-[21px]" : "translate-x-[3px]"}`}
                />
              </button>
            </div>
            {values.rsvpCutoffEnabled && (
              <div className="flex flex-col gap-1.5">
                <label htmlFor="rsvp-cutoff" className="text-sm font-semibold text-ink-2">
                  RSVP cut-off
                </label>
                <input
                  id="rsvp-cutoff"
                  type="datetime-local"
                  value={values.rsvpCutoffAtUtc ?? ""}
                  onChange={(e) => setRsvpCutoffAtUtc(e.target.value)}
                  className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-sand px-3 text-[15px] outline-none transition-colors focus:border-clay"
                />
                {touched && fieldErrors.rsvpCutoffAtUtc ? (
                  <FieldError message={fieldErrors.rsvpCutoffAtUtc[0]} />
                ) : (
                  <Hint>Must be before the event start time.</Hint>
                )}
              </div>
            )}
          </div>

          <div className="rounded-md bg-sand-2 p-4">
            <b className="text-[14px]">Who gets told when you publish</b>
            <p className="mt-1.5 text-[13px] leading-relaxed text-ink-2">
              Everyone registered in the venue&apos;s region gets notified — by push, and by SMS where the app is not installed.
            </p>
            <Hint>The region follows the venue. There is no radius to choose.</Hint>
          </div>

          {error && (
            <div className="rounded-sm border border-error bg-error-tint px-4 py-3 text-sm text-error">{error.message}</div>
          )}

          <button
            type="submit"
            disabled={isPending}
            className={`flex h-[54px] w-full items-center justify-center gap-2 rounded-md text-base font-semibold transition-colors lg:hidden ${
              isPending ? "cursor-not-allowed bg-sand-2 text-ink-off" : "bg-clay text-white hover:bg-clay-hover"
            }`}
          >
            {isPending ? "Publishing…" : "Publish event"}
          </button>
        </div>

        <aside className="hidden lg:flex lg:w-[380px] lg:flex-none lg:flex-col lg:gap-4">
          <div className="flex flex-col gap-2.5 rounded-md border border-line bg-cream p-4">
            <div className="flex items-center justify-between">
              <b className="text-[15px]">
                Venue location <i className="not-italic text-error">*</i>
              </b>
              <span className="text-[13px] font-semibold text-clay">Move pin</span>
            </div>
            <VenuePinMap latitude={pinLat} longitude={pinLng} onMove={(lat, lng) => setManualVenueCoordinates({ latitude: lat, longitude: lng })} />
            {geocoding.status === "resolving" && <Hint>Finding the map location…</Hint>}
            {geocoding.status === "notFound" && <span className="text-xs font-semibold text-amber">Couldn&apos;t resolve that address — drag the pin to set it manually.</span>}
            {touched && fieldErrors.latitude && <FieldError message="Please select a valid location on the map." />}
          </div>
          <div className="rounded-md bg-sand-2 p-4">
            <b className="text-[15px]">Who gets told</b>
            <p className="mt-1.5 text-[13px] leading-relaxed text-ink-2">
              Everyone registered in the venue&apos;s region, once the location is set. Push where the app is installed, SMS where it is not.
            </p>
          </div>
          <button
            type="submit"
            disabled={isPending}
            className={`flex h-[54px] w-full items-center justify-center gap-2 rounded-md text-base font-semibold transition-colors ${
              isPending ? "cursor-not-allowed bg-sand-2 text-ink-off" : "bg-clay text-white hover:bg-clay-hover"
            }`}
          >
            {isPending ? "Publishing…" : "Publish event"}
          </button>
        </aside>
      </form>
    </div>
  );
}
