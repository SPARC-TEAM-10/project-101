import { useNavigate, useParams } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { useEventAnalytics } from "../../features/events/useEventAnalytics";
import type { AttendanceViewStatus, EventParticipantAttendanceDto } from "../../api/eventApi";

function initials(fullName: string): string {
  return fullName
    .trim()
    .split(/\s+/)
    .slice(0, 2)
    .map((p) => p[0]?.toUpperCase() ?? "")
    .join("");
}

function formatTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit", hour12: false });
}

const STATUS_PILL_CLASS: Record<AttendanceViewStatus, string> = {
  Attended: "bg-leaf-tint text-leaf",
  NoShow: "bg-amber-tint text-amber",
  Cancelled: "bg-sand-2 text-ink-2",
  Going: "bg-clay-tint text-clay-deep",
};

const STATUS_LABEL: Record<AttendanceViewStatus, string> = {
  Attended: "Attended",
  NoShow: "No-show",
  Cancelled: "Cancelled",
  Going: "RSVP'd",
};

function ParticipantRow({ participant }: { participant: EventParticipantAttendanceDto }) {
  return (
    <div className="flex items-center gap-3 rounded-lg border border-line bg-cream p-3">
      <span className="flex h-[42px] w-[42px] flex-none items-center justify-center rounded-full bg-clay-tint text-[13.5px] font-bold text-clay-deep">
        {initials(participant.fullName)}
      </span>
      <div className="min-w-0 flex-1">
        <b className="block text-[15px] font-bold">{participant.fullName}</b>
        <span className="text-xs text-ink-2 [font-variant-numeric:tabular-nums]">
          {participant.referenceCode}
          {participant.status === "Attended" && participant.attendedAtUtc
            ? ` · ${formatTime(participant.attendedAtUtc)} · marked by ${participant.attendedByName}`
            : participant.status === "NoShow"
              ? " · never marked attended"
              : ""}
        </span>
      </div>
      <span className={`inline-flex h-[26px] flex-none items-center rounded-full px-2.5 text-xs font-bold ${STATUS_PILL_CLASS[participant.status]}`}>
        {STATUS_LABEL[participant.status]}
      </span>
    </div>
  );
}

/**
 * Event attendance analytics (CHH-45/US-CHH-005-08), matching EventAnalytics.dc.html's stat
 * cards, "from notified to attended" funnel, filter chips, and participant list, with
 * EventAnalyticsWeb.dc.html's Export CSV action. Feedback/Audit-history tabs shown in the desktop
 * artboard aren't built — no story collects feedback or models an audit trail.
 */
export function EventAnalyticsPage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { session } = useAuth();
  const {
    summary,
    isSummaryLoading,
    isSummaryError,
    participants,
    isParticipantsLoading,
    statusFilter,
    setStatusFilter,
    search,
    setSearch,
    exportCsv,
    isExporting,
    exportError,
  } = useEventAnalytics(session?.token, id ?? "");

  if (isSummaryLoading) {
    return <div className="flex min-h-screen items-center justify-center bg-sand text-ink-2">Loading…</div>;
  }

  if (isSummaryError || !summary) {
    return (
      <div className="flex min-h-screen flex-col items-center justify-center gap-3 bg-sand px-4 text-center text-ink">
        <p className="text-sm text-error">This event couldn&apos;t be found.</p>
        <button type="button" onClick={() => navigate("/events/mine")} className="text-sm font-semibold text-clay hover:text-clay-hover">
          Back to my events
        </button>
      </div>
    );
  }

  const hasEnded = new Date(summary.endAtUtc).getTime() <= Date.now();
  const funnelMax = Math.max(summary.notifiedCount, 1);
  const funnelBars: { label: string; value: number; colorClass: string }[] = [
    { label: "Notified", value: summary.notifiedCount, colorClass: "bg-line-strong" },
    { label: "RSVP'd", value: summary.rsvpdCount, colorClass: "bg-clay" },
    { label: "Attended", value: summary.attendedCount, colorClass: "bg-leaf" },
    { label: "No-show", value: summary.noShowCount, colorClass: "bg-amber" },
  ];

  const chips: { key: AttendanceViewStatus | undefined; label: string; count: number }[] = [
    { key: undefined, label: "All", count: summary.rsvpdCount + summary.cancelledCount },
    { key: "Attended", label: "Attended", count: summary.attendedCount },
    { key: "NoShow", label: "No-show", count: summary.noShowCount },
    { key: "Cancelled", label: "Cancelled", count: summary.cancelledCount },
  ];

  return (
    <div className="min-h-screen bg-sand font-sans text-ink">
      <div className="flex h-[58px] items-center gap-2.5 border-b border-line bg-cream px-4">
        <button
          type="button"
          aria-label="Back"
          onClick={() => navigate(`/events/${id}/manage`)}
          className="flex h-[42px] w-[42px] items-center justify-center rounded-full hover:bg-sand-2"
        >
          ←
        </button>
        <b className="flex-1 text-center text-[15px] font-extrabold tracking-tight">Attendance</b>
        <span className="w-[42px]" aria-hidden="true" />
      </div>

      <div className="mx-auto flex max-w-2xl flex-col gap-3 px-4 py-5 pb-32 lg:max-w-3xl lg:px-8 lg:py-8">
        <div className="flex flex-col gap-1 rounded-lg border border-line bg-cream p-3.5">
          <b className="text-[15.5px]">{summary.title}</b>
          <span className="text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">
            {new Date(summary.startAtUtc).toLocaleDateString(undefined, { weekday: "short", day: "numeric", month: "short" })} ·{" "}
            {hasEnded ? `ended ${formatTime(summary.endAtUtc)}` : `starts ${formatTime(summary.startAtUtc)}`}
          </span>
        </div>

        <div className="grid grid-cols-2 gap-2.5 sm:grid-cols-3">
          <div className="flex flex-col gap-1 rounded-lg border border-line bg-cream p-3.5">
            <dt className="text-[11px] font-bold uppercase tracking-wide text-ink-3">RSVP&apos;d</dt>
            <dd className="m-0 text-[28px] font-extrabold leading-8 [font-variant-numeric:tabular-nums]">{summary.rsvpdCount}</dd>
            <small className="text-xs text-ink-2">of {summary.capacity} capacity</small>
          </div>
          <div className="flex flex-col gap-1 rounded-lg border border-line bg-cream p-3.5">
            <dt className="text-[11px] font-bold uppercase tracking-wide text-ink-3">Attended</dt>
            <dd className="m-0 text-[28px] font-extrabold leading-8 text-leaf [font-variant-numeric:tabular-nums]">{summary.attendedCount}</dd>
            <small className="text-xs text-ink-2">of {summary.rsvpdCount} RSVP&apos;d</small>
          </div>
          <div className="flex flex-col gap-1 rounded-lg border border-line bg-cream p-3.5">
            <dt className="text-[11px] font-bold uppercase tracking-wide text-ink-3">No-show</dt>
            <dd className="m-0 text-[28px] font-extrabold leading-8 text-amber [font-variant-numeric:tabular-nums]">{summary.noShowCount}</dd>
            <small className="text-xs text-ink-2">RSVP&apos;d, never attended</small>
          </div>
          <div className="flex flex-col gap-1 rounded-lg border border-line bg-cream p-3.5">
            <dt className="text-[11px] font-bold uppercase tracking-wide text-ink-3">Attendance rate</dt>
            <dd className="m-0 text-[28px] font-extrabold leading-8 [font-variant-numeric:tabular-nums]">{summary.attendanceRatePercent}%</dd>
            <small className="text-xs text-ink-2">attended ÷ RSVP&apos;d</small>
          </div>
          <div className="flex flex-col gap-1 rounded-lg border border-line bg-cream p-3.5">
            <dt className="text-[11px] font-bold uppercase tracking-wide text-ink-3">Remaining capacity</dt>
            <dd className="m-0 text-[28px] font-extrabold leading-8 [font-variant-numeric:tabular-nums]">{summary.remainingCapacity}</dd>
            <small className="text-xs text-ink-2">never taken up</small>
          </div>
          <div className="flex flex-col gap-1 rounded-lg border border-line bg-cream p-3.5">
            <dt className="text-[11px] font-bold uppercase tracking-wide text-ink-3">Cancelled RSVPs</dt>
            <dd className="m-0 text-[28px] font-extrabold leading-8 [font-variant-numeric:tabular-nums]">{summary.cancelledCount}</dd>
            <small className="text-xs text-ink-2">spots returned to the pool</small>
          </div>
        </div>

        <div className="flex flex-col gap-3 rounded-lg border border-line bg-cream p-4 shadow-sm">
          <b className="text-[14.5px]">From notified to attended</b>
          <div className="flex flex-col gap-3">
            {funnelBars.map((bar) => (
              <div key={bar.label} className="flex flex-col gap-1.5">
                <div className="flex items-baseline justify-between text-[12.5px] text-ink-2 [font-variant-numeric:tabular-nums]">
                  <span>{bar.label}</span>
                  <b className="text-[15px] text-ink">{bar.value}</b>
                </div>
                <div className="h-3.5 overflow-hidden rounded-full bg-sand-2">
                  <div className={`h-full rounded-full ${bar.colorClass}`} style={{ width: `${Math.min(100, (bar.value / funnelMax) * 100)}%` }} />
                </div>
              </div>
            ))}
          </div>
        </div>

        <div className="flex flex-wrap gap-2">
          <input
            type="text"
            value={search}
            onChange={(e) => setSearch(e.target.value)}
            placeholder="Search name or mobile"
            aria-label="Search participants by name or mobile number"
            className="h-[42px] flex-1 rounded-full border-[1.5px] border-line bg-sand px-4 text-[14px] outline-none focus:border-clay"
          />
        </div>
        <div className="flex flex-wrap gap-2">
          {chips.map((chip) => (
            <button
              key={chip.label}
              type="button"
              onClick={() => setStatusFilter(chip.key)}
              className={`h-[34px] rounded-full border px-3.5 text-[12.5px] font-semibold transition-colors ${
                statusFilter === chip.key ? "border-clay-line bg-clay-tint text-clay-deep" : "border-line bg-sand-2 text-ink-2"
              }`}
            >
              {chip.label} <b className="[font-variant-numeric:tabular-nums]">{chip.count}</b>
            </button>
          ))}
        </div>

        {isParticipantsLoading && <p className="text-sm text-ink-2">Loading participants…</p>}
        {!isParticipantsLoading && participants.length === 0 && <p className="text-sm text-ink-2">No participants match.</p>}
        <div className="flex flex-col gap-2.5">
          {participants.map((participant) => (
            <ParticipantRow key={participant.rsvpId} participant={participant} />
          ))}
        </div>
      </div>

      <div className="fixed inset-x-0 bottom-0 flex flex-col gap-2 border-t border-line bg-cream px-4 py-3 pb-[calc(env(safe-area-inset-bottom)+12px)]">
        <div className="mx-auto flex w-full max-w-2xl flex-col gap-2 lg:max-w-3xl lg:px-4">
          {exportError && <div className="rounded-lg border border-error bg-error-tint px-3.5 py-2.5 text-sm text-error">{exportError.message}</div>}
          <button
            type="button"
            onClick={() => exportCsv()}
            disabled={isExporting}
            className="h-[54px] rounded-md border-[1.5px] border-line-strong text-base font-bold disabled:opacity-60"
          >
            {isExporting ? "Exporting…" : "Export CSV"}
          </button>
          <p className="m-0 text-center text-xs text-ink-3">Export is limited to permitted fields and to admins of this facility.</p>
        </div>
      </div>
    </div>
  );
}
