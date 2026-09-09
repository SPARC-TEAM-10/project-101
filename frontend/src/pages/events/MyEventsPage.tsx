import { useNavigate } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { useMyEvents } from "../../features/events/useEventManage";
import { EVENT_TYPE_LABELS } from "../../lib/validation/eventSchemas";
import type { EventDto } from "../../api/eventApi";

function formatDateTime(iso: string): string {
  const d = new Date(iso);
  return `${d.toLocaleDateString(undefined, { day: "numeric", month: "short" })}, ${d.toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit", hour12: false })}`;
}

function StatusPill({ status, hasStarted }: { status: EventDto["status"]; hasStarted: boolean }) {
  if (status === "Cancelled") {
    return <span className="inline-flex h-[26px] items-center rounded-full bg-error-tint px-2.5 text-xs font-bold text-error">Cancelled</span>;
  }
  if (hasStarted) {
    return <span className="inline-flex h-[26px] items-center rounded-full bg-clay-tint px-2.5 text-xs font-bold text-clay-deep">In progress / ended</span>;
  }
  return <span className="inline-flex h-[26px] items-center rounded-full bg-leaf-tint px-2.5 text-xs font-bold text-leaf">Published</span>;
}

/**
 * "My events" list for a Hospital/Ngo facility (CHH-41's manage-event entry point) — no dedicated
 * artboard exists for this list itself (EventManageMobile.dc.html starts one level deeper, on a
 * single event), so this page is a plain functional list rather than a pixel match.
 */
export function MyEventsPage() {
  const navigate = useNavigate();
  const { session } = useAuth();
  const { events, isLoading, isError } = useMyEvents(session?.token);

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-2.5 border-b border-line bg-cream px-4">
        <button type="button" aria-label="Back" onClick={() => navigate("/dashboard/facility")} className="flex h-[42px] w-[42px] items-center justify-center rounded-full hover:bg-sand-2">
          ←
        </button>
        <b className="flex-1 text-center text-[15px] font-extrabold tracking-tight">My events</b>
        <span className="w-[42px]" aria-hidden="true" />
      </div>

      <div className="mx-auto flex max-w-3xl flex-col gap-3 px-4 py-5 lg:max-w-4xl lg:px-8 lg:py-8">
        {isLoading && <p className="text-sm text-ink-2">Loading your events…</p>}
        {isError && <p className="text-sm text-error">Couldn&apos;t load your events. Try refreshing the page.</p>}

        {!isLoading && !isError && events.length === 0 && (
          <div className="flex flex-col items-center gap-3 rounded-lg border border-dashed border-line-strong px-6 py-12 text-center">
            <b className="text-base font-bold">You haven&apos;t published any events yet.</b>
            <button
              type="button"
              onClick={() => navigate("/events/new")}
              className="mt-2 h-11 rounded-md bg-clay px-5 text-sm font-semibold text-white hover:bg-clay-hover"
            >
              Plan an event
            </button>
          </div>
        )}

        {events.map((event) => {
          const hasStarted = new Date(event.startAtUtc).getTime() <= Date.now();
          return (
            <button
              key={event.id}
              type="button"
              onClick={() => navigate(`/events/${event.id}/manage`)}
              className="flex flex-col gap-1.5 rounded-lg border border-line bg-cream p-3.5 text-left shadow-sm transition-colors hover:bg-sand-2"
            >
              <div className="flex flex-wrap items-center gap-2">
                <span className="inline-flex h-[26px] items-center rounded-full bg-sand-2 px-2.5 text-xs font-bold text-ink-2">
                  {EVENT_TYPE_LABELS[event.eventType]}
                </span>
                <StatusPill status={event.status} hasStarted={hasStarted} />
              </div>
              <h3 className="text-base font-bold leading-tight">{event.title}</h3>
              <p className="text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">
                {event.venueName} · {formatDateTime(event.startAtUtc)}
              </p>
            </button>
          );
        })}
      </div>
    </div>
  );
}
