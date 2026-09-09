import { useMemo, useState } from "react";
import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { markEventRsvpAttended, searchEventParticipants } from "../../api/eventApi";
import { ApiError } from "../../api/httpClient";

export const MIN_SEARCH_NAME_LENGTH = 3;
const MOBILE_PATTERN = /^\d{10}$/;

export interface MarkAttendedError {
  status: number | null;
  message: string;
}

function toMarkAttendedError(err: unknown): MarkAttendedError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't mark attendance. Try again." };
  }
  return { status: null, message: "Couldn't mark attendance. Try again." };
}

function isValidSearch(term: string): boolean {
  const trimmed = term.trim();
  return MOBILE_PATTERN.test(trimmed) || trimmed.length >= MIN_SEARCH_NAME_LENGTH;
}

// CHH-44/US-CHH-005-07 — search RSVP'd participants and mark one attended. ManualAttendance.dc.html
// has no explicit search button: results appear as soon as the term is valid (3+ characters of a
// name, or a full mobile number) — an invalid/short term never calls the API (matches the
// artboard's "searchTooShort" state, which is a pure client-side gate).
export function useEventAttendance(accessToken: string | undefined, eventId: string) {
  const queryClient = useQueryClient();
  const [search, setSearch] = useState("");
  const [justMarkedName, setJustMarkedName] = useState<string | null>(null);

  const trimmedSearch = search.trim();
  const searchIsValid = isValidSearch(trimmedSearch);

  const query = useQuery({
    queryKey: ["events", "attendance", eventId, trimmedSearch, accessToken],
    queryFn: () => searchEventParticipants(accessToken, eventId, trimmedSearch),
    enabled: Boolean(eventId) && searchIsValid,
  });

  const markAttendedMutation = useMutation({
    mutationFn: (rsvpId: string) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return markEventRsvpAttended(accessToken, eventId, rsvpId);
    },
    onSuccess: (data) => {
      setJustMarkedName(data.fullName);
      queryClient.invalidateQueries({ queryKey: ["events", "attendance", eventId] });
    },
  });

  async function markAttended(rsvpId: string) {
    try {
      const data = await markAttendedMutation.mutateAsync(rsvpId);
      return { ok: true as const, data };
    } catch (err) {
      return { ok: false as const, error: toMarkAttendedError(err) };
    }
  }

  const participants = useMemo(() => query.data ?? [], [query.data]);

  return {
    search,
    setSearch,
    searchIsValid,
    participants,
    isLoading: query.isLoading && searchIsValid,
    isError: query.isError,
    markAttended,
    isMarkPending: markAttendedMutation.isPending,
    markError: markAttendedMutation.isError ? toMarkAttendedError(markAttendedMutation.error) : null,
    justMarkedName,
  };
}
