import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { useEventAnalytics } from "./useEventAnalytics";
import { server } from "../../../tests/setup";
import {
  EVENT_DETAIL_ID,
  getAttendanceSummarySuccessHandler,
  getAttendanceParticipantsSuccessHandler,
  exportAttendanceCsvSuccessHandler,
} from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

describe("useEventAnalytics", () => {
  it("loads the summary and participants", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler);
    const { result } = renderHook(() => useEventAnalytics("token", EVENT_DETAIL_ID), { wrapper });

    await waitFor(() => expect(result.current.summary).toBeDefined());
    expect(result.current.summary?.attendedCount).toBe(31);
    await waitFor(() => expect(result.current.participants).toHaveLength(3));
  });

  it("re-fetches participants when the status filter changes", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler);
    const { result } = renderHook(() => useEventAnalytics("token", EVENT_DETAIL_ID), { wrapper });

    await waitFor(() => expect(result.current.participants).toHaveLength(3));

    act(() => result.current.setStatusFilter("Cancelled"));

    await waitFor(() => expect(result.current.participants).toHaveLength(1));
    expect(result.current.participants[0].fullName).toBe("Vishnu Nair");
  });

  it("exportCsv() succeeds", async () => {
    server.use(getAttendanceSummarySuccessHandler, getAttendanceParticipantsSuccessHandler, exportAttendanceCsvSuccessHandler);
    const { result } = renderHook(() => useEventAnalytics("token", EVENT_DETAIL_ID), { wrapper });

    const response = await result.current.exportCsv();

    expect(response.ok).toBe(true);
  });
});
