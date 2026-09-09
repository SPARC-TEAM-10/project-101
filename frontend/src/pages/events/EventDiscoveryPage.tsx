import { useNavigate } from "react-router-dom";

import { EventDiscoveryMap } from "../../components/EventDiscoveryMap";
import { useAuth } from "../../context/AuthProvider";
import { useEventDiscovery, MIN_RADIUS_KM, MAX_RADIUS_KM } from "../../features/events/useEventDiscovery";
import { EVENT_TYPES, EVENT_TYPE_LABELS, type EventType } from "../../lib/validation/eventSchemas";
import type { EventSummaryDto } from "../../api/eventApi";

function formatDateBadge(iso: string): { weekday: string; day: string; month: string } {
  const d = new Date(iso);
  return {
    weekday: d.toLocaleDateString(undefined, { weekday: "short" }),
    day: d.toLocaleDateString(undefined, { day: "numeric" }),
    month: d.toLocaleDateString(undefined, { month: "short" }),
  };
}

function formatTimeRange(startIso: string, endIso: string): string {
  const fmt = (iso: string) => new Date(iso).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit", hour12: false });
  return `${fmt(startIso)}–${fmt(endIso)}`;
}

function EventTypePill({ eventType }: { eventType: EventType }) {
  const cls = eventType === "BloodDonationCamp" ? "bg-blood-tint text-blood-deep" : "bg-sand-2 text-ink-2";
  return <span className={`inline-flex h-[26px] items-center rounded-full px-2.5 text-xs font-bold ${cls}`}>{EVENT_TYPE_LABELS[eventType]}</span>;
}

function EventCard({ event, onOpen }: { event: EventSummaryDto; onOpen: () => void }) {
  const { weekday, day, month } = formatDateBadge(event.startAtUtc);
  const isFull = event.spotsRemaining <= 0;
  const pctFilled = Math.round(((event.capacity - event.spotsRemaining) / event.capacity) * 100);

  return (
    <button
      type="button"
      onClick={onOpen}
      className="flex gap-3 rounded-lg border border-line bg-cream p-3.5 text-left shadow-sm transition-colors hover:bg-sand-2"
    >
      <div className="flex w-14 flex-none flex-col items-center justify-center rounded-md bg-sand-2 py-2 text-ink-2">
        <span className="text-[11.5px] font-semibold">{weekday}</span>
        <b className="text-xl font-extrabold leading-none text-ink [font-variant-numeric:tabular-nums]">{day}</b>
        <span className="text-[11.5px] font-semibold">{month}</span>
      </div>
      <div className="flex min-w-0 flex-1 flex-col gap-1.5">
        <div className="flex flex-wrap items-center gap-2">
          <EventTypePill eventType={event.eventType} />
          <span className="inline-flex h-[26px] items-center rounded-full bg-sand-2 px-2.5 text-xs font-bold text-ink-2 [font-variant-numeric:tabular-nums]">
            about {Math.round(event.distanceKm)} km
          </span>
          {isFull && <span className="inline-flex h-[26px] items-center rounded-full bg-amber-tint px-2.5 text-xs font-bold text-amber">Event Full</span>}
        </div>
        <h3 className="text-base font-bold leading-tight">{event.title}</h3>
        <p className="text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">
          {event.facilityName} · {formatTimeRange(event.startAtUtc, event.endAtUtc)}
        </p>
        {!isFull && (
          <div className="flex flex-col gap-1.5 pt-1">
            <div className="h-2 overflow-hidden rounded-full bg-sand-2">
              <div className={`h-full rounded-full ${pctFilled >= 90 ? "bg-amber" : "bg-clay"}`} style={{ width: `${pctFilled}%` }} />
            </div>
            <div className="flex items-baseline gap-2 text-xs text-ink-2 [font-variant-numeric:tabular-nums]">
              <b className="text-[13px] font-bold text-ink">{event.spotsRemaining} spots remaining</b>
              <span>of {event.capacity}</span>
            </div>
          </div>
        )}
      </div>
    </button>
  );
}

/**
 * Event discovery (CHH-39/US-CHH-005-02), matching the approved design at
 * design/drafts/CHH-37-artboards/{Discovery,DiscoveryWeb}.dc.html — List/Map toggle, radius
 * slider, type filter chips, and a manual-city fallback when GPS is denied (AC1/AC2/AC3, Edge Case).
 */
export function EventDiscoveryPage() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const {
    radiusKm,
    setRadiusKm,
    eventType,
    setEventType,
    viewMode,
    setViewMode,
    hideFullEvents,
    setHideFullEvents,
    manualLocationText,
    setManualLocationText,
    isGpsDenied,
    coordinates,
    events,
    isLoading,
    hasSearched,
  } = useEventDiscovery(session?.token);

  const radiusFraction = (radiusKm - MIN_RADIUS_KM) / (MAX_RADIUS_KM - MIN_RADIUS_KM);

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-2.5 border-b border-line bg-cream px-4">
        <b className="text-[15px] font-extrabold tracking-tight">Events near you</b>
      </div>

      <div className="mx-auto flex max-w-3xl flex-col gap-4 px-4 py-5 lg:max-w-5xl lg:px-8 lg:py-8">
        {isGpsDenied && !coordinates && (
          <div className="flex flex-col items-center gap-3 rounded-lg border border-line bg-cream px-6 py-10 text-center">
            <b className="text-lg font-bold tracking-tight">Location is switched off for this site</b>
            <p className="max-w-[56ch] text-sm leading-relaxed text-ink-2">
              Events are ranked by how far they are from you, so we need either your location or a city to measure from.
            </p>
            <div className="mt-2 flex w-full max-w-sm flex-col gap-2">
              <label htmlFor="manual-location" className="text-left text-sm font-semibold text-ink-2">
                Search from a city or area
              </label>
              <input
                id="manual-location"
                type="text"
                value={manualLocationText}
                onChange={(e) => setManualLocationText(e.target.value)}
                placeholder="City, area or PIN code"
                className="h-[50px] rounded-sm border-[1.5px] border-line-strong bg-sand px-4 text-base outline-none transition-colors focus:border-clay"
              />
            </div>
          </div>
        )}

        {coordinates && (
          <>
            <div className="flex flex-col gap-3 rounded-lg bg-cream p-4 lg:flex-row lg:items-end lg:gap-6">
              <div className="flex-1">
                <label htmlFor="radius-slider" className="mb-2 block text-sm font-semibold text-ink-2">
                  Within
                </label>
                <input
                  id="radius-slider"
                  type="range"
                  min={MIN_RADIUS_KM}
                  max={MAX_RADIUS_KM}
                  value={radiusKm}
                  onChange={(e) => setRadiusKm(Number(e.target.value))}
                  className="radius-slider w-full"
                  style={{ ["--fill" as string]: `${radiusFraction * 100}%` }}
                  aria-label="Search radius in kilometers"
                />
                <div className="mt-1.5 flex justify-between text-xs text-ink-3">
                  <span>{MIN_RADIUS_KM}</span>
                  <span>25</span>
                  <span>50</span>
                  <span>75</span>
                  <span>{MAX_RADIUS_KM}</span>
                </div>
              </div>
              <div className="flex h-[50px] items-center justify-center rounded-sm border-[1.5px] border-line px-3 text-base font-bold [font-variant-numeric:tabular-nums]">
                {radiusKm}<span className="ml-1 text-[13px] font-semibold text-ink-3">km</span>
              </div>
              <div className="flex h-11 rounded-sm bg-sand-2 p-1">
                <button
                  type="button"
                  onClick={() => setViewMode("list")}
                  className={`flex-1 rounded-[9px] px-4 text-[13.5px] font-semibold transition-colors ${viewMode === "list" ? "bg-cream text-ink shadow-sm" : "text-ink-2"}`}
                >
                  List
                </button>
                <button
                  type="button"
                  onClick={() => setViewMode("map")}
                  className={`flex-1 rounded-[9px] px-4 text-[13.5px] font-semibold transition-colors ${viewMode === "map" ? "bg-cream text-ink shadow-sm" : "text-ink-2"}`}
                >
                  Map
                </button>
              </div>
            </div>

            <div className="flex flex-wrap gap-2">
              <button
                type="button"
                onClick={() => setEventType(undefined)}
                className={`h-[34px] rounded-full border px-3.5 text-[12.5px] font-semibold transition-colors ${
                  !eventType ? "border-clay-line bg-clay-tint text-clay-deep" : "border-line bg-sand-2 text-ink-2"
                }`}
              >
                All types
              </button>
              {EVENT_TYPES.map((type) => (
                <button
                  key={type}
                  type="button"
                  onClick={() => setEventType(type)}
                  className={`h-[34px] rounded-full border px-3.5 text-[12.5px] font-semibold transition-colors ${
                    eventType === type ? "border-clay-line bg-clay-tint text-clay-deep" : "border-line bg-sand-2 text-ink-2"
                  }`}
                >
                  {EVENT_TYPE_LABELS[type]}
                </button>
              ))}
              <button
                type="button"
                onClick={() => setHideFullEvents(!hideFullEvents)}
                className={`h-[34px] rounded-full border px-3.5 text-[12.5px] font-semibold transition-colors ${
                  hideFullEvents ? "border-clay-line bg-clay-tint text-clay-deep" : "border-line bg-sand-2 text-ink-2"
                }`}
              >
                Spots remaining
              </button>
            </div>

            {isLoading && (
              <div className="flex flex-col gap-3">
                {[0, 1, 2].map((i) => (
                  <div key={i} className="flex gap-3 rounded-lg border border-line bg-cream p-3.5">
                    <div className="h-14 w-14 flex-none rounded-md bg-sand-2" />
                    <div className="flex flex-1 flex-col gap-2">
                      <div className="h-4 w-2/5 rounded bg-sand-2" />
                      <div className="h-5 w-4/5 rounded bg-sand-2" />
                      <div className="h-3 w-3/5 rounded bg-sand-2" />
                    </div>
                  </div>
                ))}
              </div>
            )}

            {!isLoading && events.length === 0 && hasSearched && (
              <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-line-strong px-6 py-12 text-center">
                <b className="max-w-[36ch] text-base font-bold leading-snug">No events found in your area. Try increasing your search radius.</b>
                <p className="max-w-[38ch] text-sm text-ink-2">Hospitals and NGOs add events as they plan them. You can also be told when one appears.</p>
                <button
                  type="button"
                  onClick={() => setRadiusKm(Math.min(MAX_RADIUS_KM, radiusKm + 35))}
                  className="mt-2 h-[54px] rounded-md bg-clay px-6 text-base font-semibold text-white hover:bg-clay-hover"
                >
                  Search within {Math.min(MAX_RADIUS_KM, radiusKm + 35)} km
                </button>
                <button type="button" disabled className="h-11 cursor-not-allowed px-4 text-sm font-semibold text-ink-off" title="Coming soon — no notification-preference system exists yet">
                  Tell me when one is added
                </button>
              </div>
            )}

            {!isLoading && events.length > 0 && viewMode === "list" && (
              <div className="flex flex-col gap-3">
                {events.map((event) => (
                  <EventCard
                    key={event.id}
                    event={event}
                    onOpen={() => navigate(`/events/${event.id}`, { state: { coordinates } })}
                  />
                ))}
              </div>
            )}

            {!isLoading && events.length > 0 && viewMode === "map" && (
              <EventDiscoveryMap latitude={coordinates.latitude} longitude={coordinates.longitude} radiusKm={radiusKm} events={events} />
            )}
          </>
        )}

        <button
          type="button"
          onClick={() => navigate("/dashboard/individual")}
          className="mt-2 flex h-11 w-fit items-center justify-center gap-2 rounded-md border-[1.5px] border-line-strong px-5 text-sm font-semibold text-ink hover:bg-sand-2"
        >
          Back to dashboard
        </button>
      </div>
    </div>
  );
}
