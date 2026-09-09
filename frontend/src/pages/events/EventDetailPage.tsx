import { useState } from "react";
import { useNavigate, useLocation, useParams } from "react-router-dom";

import { EventVenueMap } from "../../components/EventVenueMap";
import { useAuth } from "../../context/AuthProvider";
import { useEventDetail } from "../../features/events/useEventDetail";
import { EVENT_TYPE_LABELS } from "../../lib/validation/eventSchemas";
import type { Coordinates } from "../../features/shared/useGeolocation";

function formatEventDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, { weekday: "long", day: "numeric", month: "long" });
}

function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit", hour12: false });
}

function formatShortDateTime(iso: string): string {
  const d = new Date(iso);
  return `${d.toLocaleDateString(undefined, { day: "numeric", month: "short" })}, ${formatTime(iso)}`;
}

function toIcsDate(iso: string): string {
  return new Date(iso).toISOString().replace(/[-:]/g, "").split(".")[0] + "Z";
}

function downloadCalendarInvite(title: string, description: string, venueName: string, startAtUtc: string, endAtUtc: string): void {
  const ics = [
    "BEGIN:VCALENDAR",
    "VERSION:2.0",
    "BEGIN:VEVENT",
    `SUMMARY:${title}`,
    `DESCRIPTION:${description}`,
    `LOCATION:${venueName}`,
    `DTSTART:${toIcsDate(startAtUtc)}`,
    `DTEND:${toIcsDate(endAtUtc)}`,
    "END:VEVENT",
    "END:VCALENDAR",
  ].join("\r\n");
  const blob = new Blob([ics], { type: "text/calendar" });
  const url = URL.createObjectURL(blob);
  const link = document.createElement("a");
  link.href = url;
  link.download = `${title.replace(/[^a-z0-9]+/gi, "-")}.ics`;
  link.click();
  URL.revokeObjectURL(url);
}

/**
 * Event detail page (CHH-40/US-CHH-005-03), matching the approved design at
 * design/drafts/CHH-37-artboards/{EventRsvp,EventRsvpWeb}.dc.html — "before" (RSVP/Not this time)
 * and "going" (Add to calendar/Cancel RSVP) states, driven by EventDetailDto.myRsvpStatus.
 */
export function EventDetailPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const location = useLocation();
  const { session } = useAuth();
  const coordinates = (location.state as { coordinates?: Coordinates } | null)?.coordinates ?? null;

  const [actionError, setActionError] = useState<string | null>(null);

  const { event, isLoading, isError, rsvp, cancelRsvp, isRsvpPending, isCancelPending } = useEventDetail(
    session?.token,
    id ?? "",
    coordinates,
  );

  const canRsvp = session?.role === "Individual";
  const isGoing = event?.myRsvpStatus === "Going";

  async function handleRsvp() {
    setActionError(null);
    const result = await rsvp();
    if (!result.ok) {
      setActionError(result.error.message);
    }
  }

  async function handleCancel() {
    if (!window.confirm("Cancel your RSVP? Your spot goes back to the pool right away.")) {
      return;
    }
    setActionError(null);
    const result = await cancelRsvp();
    if (!result.ok) {
      setActionError(result.error.message);
    }
  }

  if (isLoading) {
    return (
      <div className="min-h-screen bg-sand font-sans text-ink">
        <div className="flex h-[58px] items-center gap-2.5 border-b border-line bg-cream px-4">
          <b className="text-[15px] font-extrabold tracking-tight">Event</b>
        </div>
        <div className="mx-auto flex max-w-3xl flex-col gap-3 px-4 py-5">
          <div className="h-6 w-2/5 animate-pulse rounded bg-sand-2" />
          <div className="h-8 w-4/5 animate-pulse rounded bg-sand-2" />
          <div className="h-32 w-full animate-pulse rounded-lg bg-sand-2" />
        </div>
      </div>
    );
  }

  if (isError || !event) {
    return (
      <div className="min-h-screen bg-sand font-sans text-ink">
        <div className="flex h-[58px] items-center gap-2.5 border-b border-line bg-cream px-4">
          <b className="text-[15px] font-extrabold tracking-tight">Event</b>
        </div>
        <div className="mx-auto flex max-w-3xl flex-col items-center gap-3 px-4 py-16 text-center">
          <b className="text-lg font-bold">This event couldn't be found.</b>
          <button
            type="button"
            onClick={() => navigate("/events")}
            className="mt-2 h-11 rounded-md border-[1.5px] border-line-strong px-5 text-sm font-semibold hover:bg-sand-2"
          >
            Back to events
          </button>
        </div>
      </div>
    );
  }

  const pctFilled = Math.round(((event.capacity - event.spotsRemaining) / event.capacity) * 100);

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-2.5 border-b border-line bg-cream px-4">
        <button type="button" aria-label="Back" onClick={() => navigate(-1)} className="flex h-[42px] w-[42px] items-center justify-center rounded-full hover:bg-sand-2">
          ←
        </button>
        <b className="flex-1 text-center text-[15px] font-extrabold tracking-tight">Event</b>
        <span className="w-[42px]" aria-hidden="true" />
      </div>

      <div className="mx-auto flex max-w-3xl flex-col gap-3 px-4 py-5 pb-32 lg:max-w-4xl lg:px-8 lg:py-8">
        <div className="flex flex-wrap gap-2">
          <span className="inline-flex h-[26px] items-center rounded-full bg-blood-tint px-2.5 text-xs font-bold text-blood-deep">
            {EVENT_TYPE_LABELS[event.eventType]}
          </span>
          {!isGoing && event.distanceKm != null && (
            <span className="inline-flex h-[26px] items-center rounded-full bg-sand-2 px-2.5 text-xs font-bold text-ink-2 [font-variant-numeric:tabular-nums]">
              about {Math.round(event.distanceKm)} km away
            </span>
          )}
          {isGoing && <span className="inline-flex h-[26px] items-center rounded-full bg-leaf-tint px-2.5 text-xs font-bold text-leaf">You&apos;re going</span>}
        </div>

        <h2 className="text-[26px] font-extrabold leading-8 tracking-tight">{event.title}</h2>

        {isGoing ? (
          <div className="flex flex-col gap-2.5 rounded-lg border border-go-line bg-leaf-tint p-4">
            <div className="flex items-center gap-2.5">
              <span className="flex h-[38px] w-[38px] flex-none items-center justify-center rounded-full bg-go text-white">✓</span>
              <div>
                <b className="text-base text-go-deep">You&apos;re going</b>
                <div className="text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">reference {event.myReferenceCode}</div>
              </div>
            </div>
            <p className="m-0 text-[13px] leading-relaxed text-ink-2">
              Give reference {event.myReferenceCode} at the organizer&apos;s desk when you arrive — they mark you attended.
            </p>
          </div>
        ) : (
          <div className="flex items-center gap-2 text-[13px] text-ink-2">
            {event.facilityName}
            <span className="inline-flex items-center gap-1 font-bold text-leaf">✓ Verified</span>
          </div>
        )}

        <div className="flex flex-col gap-3 rounded-lg border border-line bg-cream p-3.5 shadow-sm">
          <div className="flex gap-2.5">
            <span className="text-ink-3">📅</span>
            <div>
              <b className="text-[14.5px]">{formatEventDate(event.startAtUtc)}</b>
              <div className="text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">
                {formatTime(event.startAtUtc)}–{formatTime(event.endAtUtc)}
                {!isGoing && event.rsvpCutoffAtUtc && ` · RSVPs close ${formatShortDateTime(event.rsvpCutoffAtUtc)}`}
              </div>
            </div>
          </div>
          <div className="flex gap-2.5 border-t border-line pt-3">
            <span className="text-ink-3">📍</span>
            <div className="flex-1">
              <b className="text-[14.5px]">{event.venueName}</b>
              <div className="text-[12.5px] text-ink-2">{event.venueAddress}</div>
            </div>
          </div>
        </div>

        {!isGoing && (
          <EventVenueMap
            latitude={event.latitude}
            longitude={event.longitude}
            myLatitude={coordinates?.latitude}
            myLongitude={coordinates?.longitude}
          />
        )}

        <div className="flex flex-col gap-2.5 rounded-lg border border-line bg-cream p-3.5 shadow-sm">
          <div className="flex items-baseline justify-between text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">
            <b className="text-[14px] text-ink">{event.spotsRemaining} spots remaining</b>
            <span>{event.capacity - event.spotsRemaining} of {event.capacity} taken</span>
          </div>
          <div className="h-2 overflow-hidden rounded-full bg-sand-2">
            <div className="h-full rounded-full bg-clay" style={{ width: `${pctFilled}%` }} />
          </div>
          <div className="text-xs leading-relaxed text-ink-3">
            {isGoing ? "Updated live — your RSVP took one of them." : "One RSVP per person. You can cancel any time before it starts."}
          </div>
        </div>

        {!isGoing && <p className="m-0 text-sm leading-relaxed text-ink-2">{event.description}</p>}

        {isGoing && (
          <div className="flex gap-2.5 rounded-lg bg-clay-tint p-3.5">
            <span className="text-clay-deep">ℹ️</span>
            <div>
              <b className="block text-[13.5px] text-clay-deep">Cancelling gives your spot back</b>
              <p className="m-0 text-[12.5px] leading-relaxed text-ink-2">The spot returns to the pool straight away and someone else can take it.</p>
            </div>
          </div>
        )}

        {actionError && (
          <div className="rounded-lg border border-error bg-error-tint px-3.5 py-3 text-sm text-error">{actionError}</div>
        )}
      </div>

      {canRsvp && event.status === "Published" && (
        <div className="fixed inset-x-0 bottom-0 flex flex-col gap-2 border-t border-line bg-cream px-4 py-3 pb-[calc(env(safe-area-inset-bottom)+12px)]">
          <div className="mx-auto flex w-full max-w-3xl flex-col gap-2 lg:max-w-4xl lg:px-4">
            {isGoing ? (
              <>
                <button
                  type="button"
                  onClick={() => downloadCalendarInvite(event.title, event.description, event.venueName, event.startAtUtc, event.endAtUtc)}
                  className="h-[54px] rounded-md bg-clay text-base font-bold text-white hover:bg-clay-hover"
                >
                  Add to my calendar
                </button>
                <button
                  type="button"
                  onClick={handleCancel}
                  disabled={isCancelPending}
                  className="h-[50px] rounded-md border-[1.5px] border-line-strong text-[14.5px] font-bold disabled:opacity-60"
                >
                  {isCancelPending ? "Cancelling…" : "Cancel my RSVP"}
                </button>
              </>
            ) : (
              <>
                <button
                  type="button"
                  onClick={handleRsvp}
                  disabled={isRsvpPending || event.spotsRemaining <= 0}
                  className="h-[54px] rounded-md bg-go text-base font-bold text-white hover:bg-go-hover disabled:cursor-not-allowed disabled:opacity-60"
                >
                  {event.spotsRemaining <= 0 ? "Event Full" : isRsvpPending ? "RSVPing…" : "RSVP"}
                </button>
                <button type="button" onClick={() => navigate(-1)} className="h-[50px] rounded-md border-[1.5px] border-line-strong text-[14.5px] font-bold">
                  Not this time
                </button>
              </>
            )}
          </div>
        </div>
      )}
    </div>
  );
}
