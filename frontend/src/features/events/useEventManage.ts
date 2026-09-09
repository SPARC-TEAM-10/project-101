import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";

import {
  cancelEvent,
  getEventById,
  getMyEvents,
  updateEvent,
  type CancelEventRequest,
  type UpdateEventRequest,
} from "../../api/eventApi";
import { ApiError } from "../../api/httpClient";

export interface EventManageActionError {
  status: number | null;
  message: string;
}

function toActionError(err: unknown): EventManageActionError {
  if (err instanceof ApiError) {
    return { status: err.status, message: err.problem.detail ?? "Couldn't complete that action. Try again." };
  }
  return { status: null, message: "Couldn't complete that action. Try again." };
}

// CHH-41/US-CHH-005-04 — a facility's "My events" list.
export function useMyEvents(accessToken: string | undefined) {
  const query = useQuery({
    queryKey: ["events", "mine", accessToken],
    queryFn: () => getMyEvents(accessToken),
    enabled: Boolean(accessToken),
  });

  return {
    events: query.data ?? [],
    isLoading: query.isLoading,
    isError: query.isError,
  };
}

// CHH-41/US-CHH-005-04 — single-event manage/edit/cancel for the organizing facility.
export function useEventManage(accessToken: string | undefined, eventId: string) {
  const queryClient = useQueryClient();
  const queryKey = ["events", "manage", eventId, accessToken];

  const query = useQuery({
    queryKey,
    queryFn: () => getEventById(accessToken, eventId),
    enabled: Boolean(eventId),
  });

  const updateMutation = useMutation({
    mutationFn: (request: UpdateEventRequest) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return updateEvent(accessToken, eventId, request);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
      queryClient.invalidateQueries({ queryKey: ["events", "mine"] });
    },
  });

  const cancelMutation = useMutation({
    mutationFn: (request: CancelEventRequest) => {
      if (!accessToken) {
        throw new Error("Not authenticated");
      }
      return cancelEvent(accessToken, eventId, request);
    },
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey });
      queryClient.invalidateQueries({ queryKey: ["events", "mine"] });
    },
  });

  async function update(request: UpdateEventRequest) {
    try {
      const data = await updateMutation.mutateAsync(request);
      return { ok: true as const, data };
    } catch (err) {
      return { ok: false as const, error: toActionError(err) };
    }
  }

  async function cancel(reason: string) {
    try {
      const data = await cancelMutation.mutateAsync({ reason });
      return { ok: true as const, data };
    } catch (err) {
      return { ok: false as const, error: toActionError(err) };
    }
  }

  const event = query.data;

  return {
    event,
    isLoading: query.isLoading,
    isError: query.isError,
    update,
    cancel,
    isUpdatePending: updateMutation.isPending,
    isCancelPending: cancelMutation.isPending,
  };
}
