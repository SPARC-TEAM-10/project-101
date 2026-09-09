import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { useCreateEvent } from "./useCreateEvent";
import { server } from "../../../tests/setup";
import {
  createEventForbiddenHandler,
  createEventSuccessHandler,
  createEventValidationErrorHandler,
} from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false }, mutations: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

const START = new Date(Date.now() + 2 * 24 * 60 * 60 * 1000);
const END = new Date(START.getTime() + 5 * 60 * 60 * 1000);

function toLocalInputValue(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, "0");
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

function fillValidForm(result: { current: ReturnType<typeof useCreateEvent> }) {
  act(() => {
    result.current.setTitle("Community blood drive — Kaloor");
    result.current.setEventType("BloodDonationCamp");
    result.current.setDescription("Walk-in donors welcome. Bring a photo ID. Refreshments provided.");
    result.current.setVenueName("Kaloor Community Hall");
    result.current.setVenueAddress("Stadium Link Road, Kaloor, Kochi 682017");
    result.current.setStartAtUtc(toLocalInputValue(START));
    result.current.setEndAtUtc(toLocalInputValue(END));
    result.current.setCapacity(60);
    result.current.setCoordinatorName("Dr Anitha Varghese");
    result.current.setCoordinatorContact("9000010023");
  });
}

describe("useCreateEvent", () => {
  it("resolves venue coordinates from the typed address before submit succeeds", async () => {
    server.use(createEventSuccessHandler);
    const { result } = renderHook(() => useCreateEvent("token"), { wrapper });

    fillValidForm(result);
    await waitFor(() => expect(result.current.geocoding.status).toBe("resolved"), { timeout: 3000 });

    const submitResult = await act(() => result.current.submit());

    expect(submitResult.ok).toBe(true);
    expect(submitResult.data?.status).toBe("Published");
  });

  it("does not submit when the form is incomplete", async () => {
    const { result } = renderHook(() => useCreateEvent("token"), { wrapper });

    const submitResult = await act(() => result.current.submit());

    expect(submitResult.ok).toBe(false);
  });

  it("surfaces a 403 when the facility isn't verified", async () => {
    server.use(createEventForbiddenHandler);
    const { result } = renderHook(() => useCreateEvent("token"), { wrapper });

    fillValidForm(result);
    await waitFor(() => expect(result.current.geocoding.status).toBe("resolved"), { timeout: 3000 });

    const submitResult = await act(() => result.current.submit());

    expect(submitResult.ok).toBe(false);
    expect(submitResult.error?.status).toBe(403);
    expect(submitResult.error?.message).toMatch(/verified facility/i);
  });

  it("surfaces a 422 validation error from the server", async () => {
    server.use(createEventValidationErrorHandler);
    const { result } = renderHook(() => useCreateEvent("token"), { wrapper });

    fillValidForm(result);
    await waitFor(() => expect(result.current.geocoding.status).toBe("resolved"), { timeout: 3000 });

    const submitResult = await act(() => result.current.submit());

    expect(submitResult.ok).toBe(false);
    expect(submitResult.error?.status).toBe(422);
  });

  it("rejects an RSVP cut-off at or after the start time", async () => {
    const { result } = renderHook(() => useCreateEvent("token"), { wrapper });

    fillValidForm(result);
    await waitFor(() => expect(result.current.geocoding.status).toBe("resolved"), { timeout: 3000 });
    act(() => {
      result.current.setRsvpCutoffEnabled(true);
      result.current.setRsvpCutoffAtUtc(toLocalInputValue(new Date(START.getTime() + 60 * 60 * 1000)));
    });

    const submitResult = await act(() => result.current.submit());

    expect(submitResult.ok).toBe(false);
    expect(result.current.fieldErrors.rsvpCutoffAtUtc?.[0]).toMatch(/before the event start time/i);
  });

  it("allows setManualVenueCoordinates to override the geocoded pin", async () => {
    const { result } = renderHook(() => useCreateEvent("token"), { wrapper });

    fillValidForm(result);
    await waitFor(() => expect(result.current.geocoding.status).toBe("resolved"), { timeout: 3000 });

    act(() => result.current.setManualVenueCoordinates({ latitude: 10.5, longitude: 76.5 }));

    expect(result.current.geocoding.coordinates).toEqual({ latitude: 10.5, longitude: 76.5 });
  });
});
