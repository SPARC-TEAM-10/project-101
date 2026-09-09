import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { useEventAttendance } from "./useEventAttendance";
import { server } from "../../../tests/setup";
import {
  EVENT_DETAIL_ID,
  searchParticipantsSuccessHandler,
  markAttendedSuccessHandler,
  RSVP_ID_GOING,
} from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

describe("useEventAttendance", () => {
  it("does not search until the term is valid (3+ chars or a full mobile number)", () => {
    const { result } = renderHook(() => useEventAttendance("token", EVENT_DETAIL_ID), { wrapper });

    act(() => result.current.setSearch("ni"));

    expect(result.current.searchIsValid).toBe(false);
    expect(result.current.participants).toEqual([]);
  });

  it("searches once the term is valid and returns matching participants", async () => {
    server.use(searchParticipantsSuccessHandler);
    const { result } = renderHook(() => useEventAttendance("token", EVENT_DETAIL_ID), { wrapper });

    act(() => result.current.setSearch("nith"));

    await waitFor(() => expect(result.current.participants.length).toBe(2));
  });

  it("markAttended() succeeds and sets justMarkedName", async () => {
    server.use(searchParticipantsSuccessHandler, markAttendedSuccessHandler);
    const { result } = renderHook(() => useEventAttendance("token", EVENT_DETAIL_ID), { wrapper });

    const response = await result.current.markAttended(RSVP_ID_GOING);

    expect(response.ok).toBe(true);
    await waitFor(() => expect(result.current.justMarkedName).toBe("Nithya Menon"));
  });
});
