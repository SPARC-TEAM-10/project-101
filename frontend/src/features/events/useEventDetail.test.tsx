import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { useEventDetail } from "./useEventDetail";
import { server } from "../../../tests/setup";
import {
  EVENT_DETAIL_ID,
  getEventNotGoingHandler,
  rsvpToEventSuccessHandler,
  rsvpToEventFullHandler,
  cancelEventRsvpSuccessHandler,
  getEventGoingHandler,
} from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

describe("useEventDetail", () => {
  it("loads the event detail", async () => {
    server.use(getEventNotGoingHandler);
    const { result } = renderHook(() => useEventDetail("token", EVENT_DETAIL_ID), { wrapper });

    await waitFor(() => expect(result.current.event).toBeDefined());
    expect(result.current.event?.title).toBe("Community blood drive — Kaloor");
    expect(result.current.event?.myRsvpStatus).toBeNull();
  });

  it("rsvp() succeeds and returns the updated status", async () => {
    server.use(getEventNotGoingHandler, rsvpToEventSuccessHandler);
    const { result } = renderHook(() => useEventDetail("token", EVENT_DETAIL_ID), { wrapper });

    await waitFor(() => expect(result.current.event).toBeDefined());
    const response = await result.current.rsvp();

    expect(response.ok).toBe(true);
    if (response.ok) {
      expect(response.data.status).toBe("Going");
      expect(response.data.referenceCode).toBe("A24");
    }
  });

  it("rsvp() surfaces a 422 EventFullException message", async () => {
    server.use(getEventNotGoingHandler, rsvpToEventFullHandler);
    const { result } = renderHook(() => useEventDetail("token", EVENT_DETAIL_ID), { wrapper });

    await waitFor(() => expect(result.current.event).toBeDefined());
    const response = await result.current.rsvp();

    expect(response.ok).toBe(false);
    if (!response.ok) {
      expect(response.error.status).toBe(422);
      expect(response.error.message).toBe("This event is full.");
    }
  });

  it("cancelRsvp() releases the spot", async () => {
    server.use(getEventGoingHandler, cancelEventRsvpSuccessHandler);
    const { result } = renderHook(() => useEventDetail("token", EVENT_DETAIL_ID), { wrapper });

    await waitFor(() => expect(result.current.event).toBeDefined());
    const response = await result.current.cancelRsvp();

    expect(response.ok).toBe(true);
    if (response.ok) {
      expect(response.data.status).toBe("Cancelled");
    }
  });
});
