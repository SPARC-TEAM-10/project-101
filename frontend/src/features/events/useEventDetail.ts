import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import { cancelEventRsvp, getEventById, rsvpToEvent, type EventDetailDto } from "../../api/eventApi";
import { ApiError } from "../../api/httpClient";
import type { Coordinates } from "../shared/useGeolocation";

export interface RsvpActionError {
  status: number | null;
  message: string;
}

function toRsvpActionError(err: unknown): RsvpActionError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't complete that action. Try again." };
  }
  return { status: null, message: "Couldn't complete that action. Try again." };
}

// CHH-40/US-CHH-005-03 event detail + RSVP/cancel actions. coordinates (resolved by the
// discovery page and passed via router state) are optional — the detail page still works without
// them, just without a distanceKm.
export function useEventDetail(accessToken: string | undefined, eventId: string, coordinates?: Coordinates | null) {
  const queryClient = useQueryClient();
  const queryKey = ["events", "detail", eventId, coordinates, accessToken];

  const query = useQuery({
    queryKey,
    queryFn: () => getEventById(accessToken, eventId, coordinates ?? undefined),
    enabled: Boolean(eventId),
  });

  const rsvpMutation = useMutation({
    mutationFn: () => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return rsvpToEvent(accessToken, eventId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });

  const cancelMutation = useMutation({
    mutationFn: () => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return cancelEventRsvp(accessToken, eventId);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
    },
  });

  async function rsvp() {
    try {
      const data = await rsvpMutation.mutateAsync();
      return { ok: true as const, data };
    } catch (err) {
      return { ok: false as const, error: toRsvpActionError(err) };
    }
  }

  async function cancelRsvp() {
    try {
      const data = await cancelMutation.mutateAsync();
      return { ok: true as const, data };
    } catch (err) {
      return { ok: false as const, error: toRsvpActionError(err) };
    }
  }

  const event: EventDetailDto | undefined = query.data;

  return {
    event,
    isLoading: query.isLoading,
    isError: query.isError,
    rsvp,
    cancelRsvp,
    isRsvpPending: rsvpMutation.isPending,
    isCancelPending: cancelMutation.isPending,
    rsvpError: rsvpMutation.isError ? toRsvpActionError(rsvpMutation.error) : null,
    cancelError: cancelMutation.isError ? toRsvpActionError(cancelMutation.error) : null,
  };
}
