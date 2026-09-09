import { renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import type { ReactNode } from "react";
import { describe, expect, it } from "vitest";

import { useFacilityDashboard } from "./useFacilityDashboard";
import { server } from "../../../tests/setup";
import { getMyFacilityNotFoundHandler, getMyFacilityPendingHandler } from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({ defaultOptions: { queries: { retry: false } } });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

describe("useFacilityDashboard", () => {
  it("does not fetch when there is no access token", () => {
    const { result } = renderHook(() => useFacilityDashboard(undefined), { wrapper });

    expect(result.current.isLoading).toBe(false);
    expect(result.current.facility).toBeUndefined();
  });

  it("returns the facility once loaded", async () => {
    server.use(getMyFacilityPendingHandler);
    const { result } = renderHook(() => useFacilityDashboard("token"), { wrapper });

    await waitFor(() => expect(result.current.facility).toBeDefined());
    expect(result.current.facility?.facilityName).toBe("Kochi Metro Hospital");
    expect(result.current.facility?.verificationStatus).toBe("Pending");
    expect(result.current.isError).toBe(false);
  });

  it("surfaces isError when the facility can't be found", async () => {
    server.use(getMyFacilityNotFoundHandler);
    const { result } = renderHook(() => useFacilityDashboard("token"), { wrapper });

    await waitFor(() => expect(result.current.isError).toBe(true));
    expect(result.current.facility).toBeUndefined();
  });
});
