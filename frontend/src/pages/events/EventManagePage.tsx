import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { useEventManage } from "../../features/events/useEventManage";
import { useVenueGeocoding } from "../../features/events/useVenueGeocoding";
import {
  EVENT_TYPE_LABELS,
  MAX_CANCELLATION_REASON_LENGTH,
  MIN_CANCELLATION_REASON_LENGTH,
} from "../../lib/validation/eventSchemas";
import type { UpdateEventRequest } from "../../api/eventApi";

function toDateTimeLocal(iso: string): string {
  const d = new Date(iso);
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

/**
 * Manage/edit/cancel an event (CHH-41/US-CHH-005-04), combining EventManageMobile.dc.html's
 * "already started, editing/cancelling blocked" warning state with EventEditWeb.dc.html's edit
 * form and cancel-confirmation modal (simplified to one responsive layout rather than two pixel-
 * matched screens — no artboard shows an active mobile edit form, only the blocked state).
 */
export function EventManagePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { session } = useAuth();
  const { event, isLoading, isError, update, cancel, isUpdatePending, isCancelPending } = useEventManage(session?.token, id ?? "");

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [venueName, setVenueName] = useState("");
  const [venueAddress, setVenueAddress] = useState("");
  const [startAtUtc, setStartAtUtc] = useState("");
  const [endAtUtc, setEndAtUtc] = useState("");
  const [capacity, setCapacity] = useState(0);
  const [coordinatorName, setCoordinatorName] = useState("");
  const [coordinatorContact, setCoordinatorContact] = useState("");
  const [formError, setFormError] = useState<string | null>(null);

  const [showCancelModal, setShowCancelModal] = useState(false);
  const [cancelReason, setCancelReason] = useState("");
  const [cancelError, setCancelError] = useState<string | null>(null);

  const geocoding = useVenueGeocoding(venueAddress);

  useEffect(() => {
    if (event) {
      setTitle(event.title);
      setDescription(event.description);
      setVenueName(event.venueName);
      setVenueAddress(event.venueAddress);
      setStartAtUtc(toDateTimeLocal(event.startAtUtc));
      setEndAtUtc(toDateTimeLocal(event.endAtUtc));
      setCapacity(event.capacity);
      setCoordinatorName(event.coordinatorName);
      setCoordinatorContact(event.coordinatorContact);
    }
  }, [event]);

  if (isLoading) {
    return <div className="flex min-h-screen items-center justify-center bg-sand text-ink-2">Loading…</div>;
  }

  if (isError || !event) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-3 bg-sand px-4 text-center text-ink">
        <p className="text-sm text-error">This event couldn&apos;t be found.</p>
        <button type="button" onClick={() => navigate("/events/mine")} className="text-sm font-semibold text-clay hover:text-clay-hover">
          Back to my events
        </button>
      </div>
    );
  }

  const hasStarted = new Date(event.startAtUtc).getTime() <= Date.now();
  const isCancelled = event.status === "Cancelled";
  const locked = hasStarted || isCancelled;
  const spotsGoing = event.capacity - event.spotsRemaining;

  async function handleSave() {
    setFormError(null);
    const request: UpdateEventRequest = {};
    if (event && title !== event.title) request.title = title;
    if (event && description !== event.description) request.description = description;
    if (event && venueName !== event.venueName) request.venueName = venueName;
    if (event && venueAddress !== event.venueAddress) {
      request.venueAddress = venueAddress;
      if (geocoding.coordinates) {
        request.latitude = geocoding.coordinates.latitude;
        request.longitude = geocoding.coordinates.longitude;
      }
    }
    const newStart = new Date(startAtUtc).toISOString();
    const newEnd = new Date(endAtUtc).toISOString();
    if (event && newStart !== event.startAtUtc) request.startAtUtc = newStart;
    if (event && newEnd !== event.endAtUtc) request.endAtUtc = newEnd;
    if (event && capacity !== event.capacity) request.capacity = capacity;
    if (event && coordinatorName !== event.coordinatorName) request.coordinatorName = coordinatorName;
    if (event && coordinatorContact !== event.coordinatorContact) request.coordinatorContact = coordinatorContact;

    const result = await update(request);
    if (!result.ok) {
      setFormError(result.error.message);
    }
  }

  async function handleConfirmCancel() {
    setCancelError(null);
    const result = await cancel(cancelReason);
    if (!result.ok) {
      setCancelError(result.error.message);
      return;
    }
    setShowCancelModal(false);
  }

  const reasonLength = cancelReason.trim().length;
  const reasonValid = reasonLength >= MIN_CANCELLATION_REASON_LENGTH && reasonLength <= MAX_CANCELLATION_REASON_LENGTH;

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-2.5 border-b border-line bg-cream px-4">
        <button type="button" aria-label="Back" onClick={() => navigate("/events/mine")} className="flex h-[42px] w-[42px] items-center justify-center rounded-full hover:bg-sand-2">
          ←
        </button>
        <b className="flex-1 text-center text-[15px] font-extrabold tracking-tight">Manage event</b>
        <span className="w-[42px]" aria-hidden="true" />
      </div>

      <div className="mx-auto flex max-w-2xl flex-col gap-4 px-4 py-5 lg:max-w-3xl lg:px-8 lg:py-8">
        <div className="flex flex-col gap-2 rounded-lg border border-line bg-cream p-4 shadow-sm">
          <div className="flex flex-wrap gap-2">
            <span className="inline-flex h-[26px] items-center rounded-full bg-blood-tint px-2.5 text-xs font-bold text-blood-deep">
              {EVENT_TYPE_LABELS[event.eventType]}
            </span>
            {isCancelled && <span className="inline-flex h-[26px] items-center rounded-full bg-error-tint px-2.5 text-xs font-bold text-error">Cancelled</span>}
          </div>
          <b className="text-base">{event.title}</b>
          <p className="text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">
            {new Date(event.startAtUtc).toLocaleString(undefined, { dateStyle: "medium", timeStyle: "short" })} · {spotsGoing} of {event.capacity} RSVP&apos;d
          </p>
        </div>

        <div className="flex gap-3">
          {!isCancelled && (
            <button
              type="button"
              onClick={() => navigate(`/events/${event.id}/attendance`)}
              className="h-[54px] flex-1 rounded-md bg-clay text-base font-bold text-white hover:bg-clay-hover"
            >
              Mark attendance
            </button>
          )}
          <button
            type="button"
            onClick={() => navigate(`/events/${event.id}/analytics`)}
            className="h-[54px] flex-1 rounded-md border-[1.5px] border-line-strong text-base font-bold hover:bg-sand-2"
          >
            View attendance
          </button>
        </div>

        {isCancelled && (
          <div className="flex gap-2.5 rounded-lg bg-error-tint p-3.5">
            <div>
              <b className="block text-[13.5px] text-error">This event is cancelled</b>
              <p className="m-0 mt-1 text-[12.5px] leading-relaxed text-ink-2">{event.cancellationReason}</p>
            </div>
          </div>
        )}

        {!isCancelled && hasStarted && (
          <div className="flex gap-2.5 rounded-lg bg-amber-tint p-3.5">
            <div>
              <b className="block text-[13.5px] text-amber">This event has already started, so it can&apos;t be cancelled</b>
              <p className="m-0 mt-1 text-[12.5px] leading-relaxed text-ink-2">
                Editing the venue or the times is also closed. If it has to stop, contact attendees directly — the coordinator number is on the event page.
              </p>
            </div>
          </div>
        )}

        {!locked && (
          <>
            <div className="flex gap-2.5 rounded-lg bg-clay-tint p-3.5">
              <div>
                <b className="block text-[13.5px] text-clay-deep">
                  All {spotsGoing} RSVP&apos;d attendee{spotsGoing === 1 ? "" : "s"} will be told what changed
                </b>
                <p className="m-0 mt-1 text-[12.5px] leading-relaxed text-ink-2">
                  Venue, date, time and cancellation changes send a notification. Typo fixes to the description do not.
                </p>
              </div>
            </div>

            <div className="flex flex-col gap-3 rounded-lg border border-line bg-cream p-4 shadow-sm">
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-semibold text-ink-2">Event title</span>
                <input value={title} onChange={(e) => setTitle(e.target.value)} className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay" />
              </label>
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-semibold text-ink-2">Description</span>
                <textarea value={description} onChange={(e) => setDescription(e.target.value)} rows={3} className="rounded-sm border-[1.5px] border-line bg-sand px-3 py-2 text-base outline-none focus:border-clay" />
              </label>
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <label className="flex flex-col gap-1.5 text-sm">
                  <span className="font-semibold text-ink-2">Venue name</span>
                  <input value={venueName} onChange={(e) => setVenueName(e.target.value)} className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay" />
                </label>
                <label className="flex flex-col gap-1.5 text-sm">
                  <span className="font-semibold text-ink-2">Venue address</span>
                  <input value={venueAddress} onChange={(e) => setVenueAddress(e.target.value)} className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay" />
                </label>
              </div>
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-3">
                <label className="flex flex-col gap-1.5 text-sm">
                  <span className="font-semibold text-ink-2">Starts</span>
                  <input type="datetime-local" value={startAtUtc} onChange={(e) => setStartAtUtc(e.target.value)} className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay" />
                </label>
                <label className="flex flex-col gap-1.5 text-sm">
                  <span className="font-semibold text-ink-2">Ends</span>
                  <input type="datetime-local" value={endAtUtc} onChange={(e) => setEndAtUtc(e.target.value)} className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay" />
                </label>
                <label className="flex flex-col gap-1.5 text-sm">
                  <span className="font-semibold text-ink-2">Capacity</span>
                  <input
                    type="number"
                    value={capacity}
                    min={spotsGoing}
                    onChange={(e) => setCapacity(Number(e.target.value))}
                    className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay [font-variant-numeric:tabular-nums]"
                  />
                  <span className="text-xs text-ink-3">Cannot go below {spotsGoing} — the number already RSVP&apos;d.</span>
                </label>
              </div>
              <div className="grid grid-cols-1 gap-3 sm:grid-cols-2">
                <label className="flex flex-col gap-1.5 text-sm">
                  <span className="font-semibold text-ink-2">Coordinator name</span>
                  <input value={coordinatorName} onChange={(e) => setCoordinatorName(e.target.value)} className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay" />
                </label>
                <label className="flex flex-col gap-1.5 text-sm">
                  <span className="font-semibold text-ink-2">Coordinator contact</span>
                  <input value={coordinatorContact} onChange={(e) => setCoordinatorContact(e.target.value)} className="h-11 rounded-sm border-[1.5px] border-line bg-sand px-3 text-base outline-none focus:border-clay [font-variant-numeric:tabular-nums]" />
                </label>
              </div>
            </div>

            {formError && <div className="rounded-lg border border-error bg-error-tint px-3.5 py-3 text-sm text-error">{formError}</div>}

            <div className="flex flex-wrap gap-3">
              <button
                type="button"
                onClick={handleSave}
                disabled={isUpdatePending}
                className="h-[54px] rounded-md bg-clay px-6 text-base font-bold text-white hover:bg-clay-hover disabled:opacity-60"
              >
                {isUpdatePending ? "Saving…" : "Save and notify attendees"}
              </button>
              <button type="button" onClick={() => navigate("/events/mine")} className="h-[54px] rounded-md border-[1.5px] border-line-strong px-6 text-[14.5px] font-bold">
                Discard changes
              </button>
              <div className="flex-1" />
              <button
                type="button"
                onClick={() => setShowCancelModal(true)}
                className="h-[54px] rounded-md border-[1.5px] border-blood-line px-6 text-[14.5px] font-bold text-blood"
              >
                Cancel event
              </button>
            </div>
          </>
        )}

        {!isCancelled && hasStarted && (
          <button type="button" disabled aria-disabled="true" className="h-[54px] cursor-not-allowed rounded-md bg-sand-2 px-6 text-base font-bold text-ink-off">
            Cancel event
          </button>
        )}
      </div>

      {showCancelModal && (
        <div className="fixed inset-0 z-40 flex items-center justify-center bg-[rgba(57,42,29,0.5)] px-4">
          <div className="w-full max-w-md rounded-lg bg-cream shadow-xl">
            <div className="px-6 pt-5">
              <h4 className="m-0 text-xl font-bold tracking-tight">Cancel this event?</h4>
            </div>
            <div className="flex flex-col gap-3.5 px-6 py-4">
              <p className="m-0 text-[13.5px] leading-relaxed text-ink-2">
                All {spotsGoing} RSVP&apos;d attendee{spotsGoing === 1 ? "" : "s"} are notified immediately by SMS. The event is removed from discovery and cannot be reopened — you would publish a new one instead.
              </p>
              <label className="flex flex-col gap-1.5 text-sm">
                <span className="font-semibold text-ink-2">Why is it cancelled? Attendees see this word for word.</span>
                <textarea
                  value={cancelReason}
                  onChange={(e) => setCancelReason(e.target.value)}
                  rows={4}
                  maxLength={MAX_CANCELLATION_REASON_LENGTH}
                  className="rounded-sm border-[1.5px] border-line bg-sand px-3 py-2 text-base outline-none focus:border-clay"
                />
                <span className="text-xs text-ink-3 [font-variant-numeric:tabular-nums]">
                  {cancelReason.length} / {MAX_CANCELLATION_REASON_LENGTH}
                </span>
              </label>
              {cancelError && <div className="rounded-lg border border-error bg-error-tint px-3.5 py-2.5 text-sm text-error">{cancelError}</div>}
            </div>
            <div className="flex justify-end gap-3 border-t border-line px-6 py-4">
              <button type="button" onClick={() => setShowCancelModal(false)} className="h-11 rounded-md border-[1.5px] border-line-strong px-5 text-sm font-semibold">
                Keep the event
              </button>
              <button
                type="button"
                onClick={handleConfirmCancel}
                disabled={!reasonValid || isCancelPending}
                className="h-11 rounded-md border-[1.5px] border-blood-line px-5 text-sm font-semibold text-blood disabled:cursor-not-allowed disabled:opacity-60"
              >
                {isCancelPending ? "Cancelling…" : "Cancel event"}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
