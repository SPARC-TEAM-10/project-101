import { useNavigate, useParams } from "react-router-dom";

import { useAuth } from "../../context/AuthProvider";
import { useEventAttendance, MIN_SEARCH_NAME_LENGTH } from "../../features/events/useEventAttendance";
import type { EventParticipantDto } from "../../api/eventApi";

function initials(fullName: string): string {
  const parts = fullName.trim().split(/\s+/);
  return parts.slice(0, 2).map((p) => p[0]?.toUpperCase() ?? "").join("");
}

function formatRsvpDate(iso: string): string {
  return new Date(iso).toLocaleDateString(undefined, { day: "numeric", month: "short" });
}

function formatAttendedTime(iso: string): string {
  return new Date(iso).toLocaleTimeString(undefined, { hour: "2-digit", minute: "2-digit", hour12: false });
}

function ParticipantRow({
  participant,
  onMarkAttended,
  isMarkPending,
}: {
  participant: EventParticipantDto;
  onMarkAttended: () => void;
  isMarkPending: boolean;
}) {
  const isAttended = participant.status === "Attended";

  return (
    <div className={`flex items-center gap-3 rounded-lg border border-line bg-cream p-3 ${isAttended ? "opacity-65" : ""}`}>
      <span
        className={`flex h-[42px] w-[42px] flex-none items-center justify-center rounded-full text-[13.5px] font-bold ${isAttended ? "bg-sand-2 text-ink-3" : "bg-clay-tint text-clay-deep"}`}
      >
        {isAttended ? "✓" : initials(participant.fullName)}
      </span>
      <div className="min-w-0 flex-1">
        <b className="block text-[15px] font-bold">{participant.fullName}</b>
        <span className="text-xs text-ink-2 [font-variant-numeric:tabular-nums]">
          {isAttended
            ? `Attended ${participant.attendedAtUtc ? formatAttendedTime(participant.attendedAtUtc) : ""} · marked by ${participant.attendedByName}`
            : `${participant.maskedMobileNumber} · RSVP ${formatRsvpDate(participant.rsvpCreatedAtUtc)}`}
        </span>
      </div>
      {isAttended ? (
        <span className="inline-flex h-[26px] flex-none items-center rounded-full bg-leaf-tint px-2.5 text-xs font-bold text-leaf">Attended</span>
      ) : (
        <button
          type="button"
          onClick={onMarkAttended}
          disabled={isMarkPending}
          className="h-[38px] flex-none rounded-md bg-go px-3.5 text-[13px] font-bold text-white hover:bg-go-hover disabled:opacity-60"
        >
          Mark attended
        </button>
      )}
    </div>
  );
}

/**
 * Manual attendance marking (CHH-44/US-CHH-005-07), matching ManualAttendance.dc.html's
 * "results"/"searchTooShort" states. The event-level "N marked attended of M RSVP'd" summary shown
 * in the artboard's header card is deferred to CHH-45's attendance dashboard rather than
 * duplicated here — this page focuses on the search-and-mark flow itself (AC1/AC2).
 */
export function MarkAttendancePage() {
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();
  const { session } = useAuth();
  const {
    search,
    setSearch,
    searchIsValid,
    participants,
    isLoading,
    markAttended,
    isMarkPending,
    markError,
    justMarkedName,
  } = useEventAttendance(session?.token, id ?? "");

  async function handleMarkAttended(rsvpId: string) {
    await markAttended(rsvpId);
  }

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
        <b className="flex-1 text-center text-[15px] font-extrabold tracking-tight">Mark attendance</b>
        <span className="w-[42px]" aria-hidden="true" />
      </div>

      <div className="mx-auto flex max-w-2xl flex-col gap-3 px-4 py-5 lg:max-w-3xl lg:px-8 lg:py-8">
        <input
          type="text"
          value={search}
          onChange={(e) => setSearch(e.target.value)}
          placeholder="Search by name or mobile number"
          aria-label="Search RSVP'd participants by name or mobile number"
          className={`h-[50px] rounded-full border-[1.5px] bg-sand px-4 text-base outline-none focus:border-clay ${
            search.length > 0 && !searchIsValid ? "border-error" : "border-line"
          }`}
        />

        {search.length > 0 && !searchIsValid && (
          <p className="m-0 text-[12.5px] font-semibold text-error">
            Enter at least {MIN_SEARCH_NAME_LENGTH} characters of a name, or a full mobile number.
          </p>
        )}
        {(search.length === 0 || !searchIsValid) && (
          <p className="m-0 text-[12.5px] text-ink-3">Name (3 characters or more) or full mobile number.</p>
        )}

        {markError && <div className="rounded-lg border border-error bg-error-tint px-3.5 py-2.5 text-sm text-error">{markError.message}</div>}

        {searchIsValid && (
          <>
            {isLoading && <p className="text-sm text-ink-2">Searching…</p>}
            {!isLoading && (
              <p className="text-xs font-bold uppercase tracking-wide text-ink-3">
                {participants.length} RSVP&apos;d participant{participants.length === 1 ? "" : "s"} match
              </p>
            )}
            <div className="flex flex-col gap-2.5">
              {participants.map((participant) => (
                <ParticipantRow
                  key={participant.rsvpId}
                  participant={participant}
                  onMarkAttended={() => handleMarkAttended(participant.rsvpId)}
                  isMarkPending={isMarkPending}
                />
              ))}
            </div>
          </>
        )}

        {!searchIsValid && (
          <div className="flex flex-col items-center gap-2 rounded-lg border border-dashed border-line-strong px-6 py-10 text-center">
            <b className="text-base font-bold">Nothing searched yet</b>
            <p className="m-0 max-w-[34ch] text-sm text-ink-2">
              Type a longer name, or the participant&apos;s full mobile number, to find them in the RSVP list.
            </p>
          </div>
        )}

        {justMarkedName && (
          <div className="flex gap-2.5 rounded-lg border border-line bg-cream p-3.5 shadow-sm">
            <div>
              <b className="block text-[13.5px]">{justMarkedName} marked attended</b>
              <p className="m-0 mt-0.5 text-xs text-ink-2">Just now</p>
            </div>
          </div>
        )}

        <div className="rounded-lg bg-sand-2 p-3.5">
          <b className="block text-[13.5px]">Every mark is logged</b>
          <p className="m-0 mt-1 text-[12.5px] leading-relaxed text-ink-2">Who marked it and when.</p>
        </div>
      </div>
    </div>
  );
}
