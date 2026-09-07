import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { usePendingFacilities } from "./usePendingFacilities";
import { pendingFacilitiesEmptyHandler, pendingFacilitiesErrorHandler } from "../../../tests/msw/handlers";
import { server } from "../../../tests/setup";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

describe("usePendingFacilities", () => {
  it("starts loading, then resolves to list with items mapped from the API", async () => {
    const { result } = renderHook(() => usePendingFacilities("test-token", 1), { wrapper });

    expect(result.current.status).toBe("loading");

    await waitFor(() => expect(result.current.status).toBe("list"));
    expect(result.current.items).toHaveLength(2);
    expect(result.current.items[0].facilityName).toBe("Sreedhara Multispeciality");
    expect(result.current.totalCount).toBe(2);
  });

  it("resolves to empty when the API returns no items", async () => {
    server.use(pendingFacilitiesEmptyHandler);
    const { result } = renderHook(() => usePendingFacilities("test-token", 1), { wrapper });

    await waitFor(() => expect(result.current.status).toBe("empty"));
    expect(result.current.items).toHaveLength(0);
  });

  it("resolves to error when the request fails", async () => {
    server.use(pendingFacilitiesErrorHandler);
    const { result } = renderHook(() => usePendingFacilities("test-token", 1), { wrapper });

    await waitFor(() => expect(result.current.status).toBe("error"));
  });

  it("does not fetch when no access token is present", () => {
    const { result } = renderHook(() => usePendingFacilities(undefined, 1), { wrapper });

    expect(result.current.status).toBe("loading");
    expect(result.current.items).toHaveLength(0);
  });
});
