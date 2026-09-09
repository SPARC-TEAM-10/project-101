import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { useEventDiscovery, MAX_RADIUS_KM, MIN_RADIUS_KM } from "./useEventDiscovery";
import { server } from "../../../tests/setup";
import { searchEventsEmptyHandler, searchEventsSuccessHandler } from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

function mockGeolocationSuccess() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (success: PositionCallback) =>
        success({ coords: { latitude: 9.9312, longitude: 76.2673 } } as GeolocationPosition),
    },
  });
}

function mockGeolocationDenied() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (_success: PositionCallback, error: PositionErrorCallback) =>
        error({ code: 1, message: "denied" } as GeolocationPositionError),
    },
  });
}

describe("useEventDiscovery", () => {
  it("auto-requests location on mount and searches once resolved (AC1)", async () => {
    mockGeolocationSuccess();
    server.use(searchEventsSuccessHandler);
    const { result } = renderHook(() => useEventDiscovery("token"), { wrapper });

    await waitFor(() => expect(result.current.events.length).toBeGreaterThan(0));
    expect(result.current.coordinates).toEqual({ latitude: 9.9312, longitude: 76.2673 });
  });

  it("shows isGpsDenied and does not search when location is denied (Edge Case)", async () => {
    mockGeolocationDenied();
    const { result } = renderHook(() => useEventDiscovery("token"), { wrapper });

    await waitFor(() => expect(result.current.isGpsDenied).toBe(true));
    expect(result.current.coordinates).toBeNull();
    expect(result.current.hasSearched).toBe(false);
  });

  it("clamps radius to [5,100]", () => {
    mockGeolocationSuccess();
    const { result } = renderHook(() => useEventDiscovery("token"), { wrapper });

    act(() => result.current.setRadiusKm(500));
    expect(result.current.radiusKm).toBe(MAX_RADIUS_KM);

    act(() => result.current.setRadiusKm(-10));
    expect(result.current.radiusKm).toBe(MIN_RADIUS_KM);
  });

  it("returns an empty events list with no events found (AC2)", async () => {
    mockGeolocationSuccess();
    server.use(searchEventsEmptyHandler);
    const { result } = renderHook(() => useEventDiscovery("token"), { wrapper });

    await waitFor(() => expect(result.current.hasSearched).toBe(true));
    await waitFor(() => expect(result.current.isLoading).toBe(false));
    expect(result.current.events).toEqual([]);
  });

  it("filters out full events when hideFullEvents is enabled", async () => {
    mockGeolocationSuccess();
    server.use(searchEventsSuccessHandler);
    const { result } = renderHook(() => useEventDiscovery("token"), { wrapper });

    await waitFor(() => expect(result.current.events.length).toBe(2));

    act(() => result.current.setHideFullEvents(true));

    expect(result.current.events.every((e) => e.spotsRemaining > 0)).toBe(true);
  });
});
