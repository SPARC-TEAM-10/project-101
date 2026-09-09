import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import type { ReactNode } from "react";

import { http, HttpResponse } from "msw";

import { useFacilitySearch } from "./useFacilitySearch";
import { server } from "../../../tests/setup";
import {
  FACILITIES_SEARCH_URL,
  searchFacilitiesSuccessHandler,
  searchFacilitiesEmptyHandler,
} from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
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

function mockGeolocationSuccess() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (success: PositionCallback) =>
        success({ coords: { latitude: 9.9312, longitude: 76.2673 } } as GeolocationPosition),
    },
  });
}

describe("useFacilitySearch", () => {
  it("transitions status from loading to list on a non-empty successful response", async () => {
    mockGeolocationDenied();
    server.use(searchFacilitiesSuccessHandler);
    const { result } = renderHook(() => useFacilitySearch("token"), { wrapper });

    expect(result.current.status).toBe("loading");
    await waitFor(() => expect(result.current.status).toBe("list"));
    expect(result.current.results).toHaveLength(1);
  });

  it("reports status 'empty' on a successful response with items: []", async () => {
    mockGeolocationDenied();
    server.use(searchFacilitiesEmptyHandler);
    const { result } = renderHook(() => useFacilitySearch("token"), { wrapper });

    await waitFor(() => expect(result.current.status).toBe("empty"));
  });

  it("sets geolocationStatus to 'denied' when the user rejects the permission prompt", async () => {
    mockGeolocationDenied();
    server.use(searchFacilitiesEmptyHandler);
    const { result } = renderHook(() => useFacilitySearch("token"), { wrapper });

    await waitFor(() => expect(result.current.geolocationStatus).toBe("denied"));
  });

  it("passes latitude/longitude through once useGeolocation resolves", async () => {
    mockGeolocationSuccess();
    const captured: { params: URLSearchParams | null } = { params: null };
    server.use(
      http.get(FACILITIES_SEARCH_URL, ({ request }) => {
        captured.params = new URL(request.url).searchParams;
        return HttpResponse.json({ items: [], totalCount: 0, page: 1, pageSize: 20 });
      }),
    );
    const { result } = renderHook(() => useFacilitySearch("token"), { wrapper });

    await waitFor(() => expect(result.current.geolocationStatus).toBe("resolved"));
    await waitFor(() => expect(captured.params?.get("latitude")).toBe("9.9312"));
    expect(captured.params?.get("longitude")).toBe("76.2673");
  });

  it("resets q and category via clearFilters", async () => {
    mockGeolocationDenied();
    server.use(searchFacilitiesEmptyHandler);
    const { result } = renderHook(() => useFacilitySearch("token"), { wrapper });
    await waitFor(() => expect(result.current.status).toBe("empty"));

    result.current.setQ("City Hospital");
    result.current.setCategory("Hospital");
    result.current.clearFilters();

    await waitFor(() => {
      expect(result.current.q).toBe("");
      expect(result.current.category).toBeUndefined();
    });
  });
});
