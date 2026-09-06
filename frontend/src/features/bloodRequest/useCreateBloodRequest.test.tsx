import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it, vi, beforeEach } from "vitest";
import type { ReactNode } from "react";

import { useCreateBloodRequest } from "./useCreateBloodRequest";
import { server } from "../../../tests/setup";
import {
  createBloodRequestValidationErrorHandler,
} from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

function mockGeolocationSuccess() {
  Object.defineProperty(global.navigator, "geolocation", {
    configurable: true,
    value: {
      getCurrentPosition: (success: PositionCallback) =>
        success({
          coords: { latitude: 9.9312, longitude: 76.2673 },
        } as GeolocationPosition),
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

const validValues = {
  patientName: "John Doe",
  bloodGroup: "O+" as const,
  unitsRequired: 2,
  locationCityArea: "Kochi",
  searchRadiusKm: 10,
  urgency: "Emergency" as const,
};

describe("useCreateBloodRequest", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("starts invalid with only the default radius set", () => {
    const { result } = renderHook(() => useCreateBloodRequest("token"), { wrapper });

    expect(result.current.isValid).toBe(false);
    expect(result.current.values.searchRadiusKm).toBe(10);
  });

  it("flags a radius below the minimum", () => {
    const { result } = renderHook(() => useCreateBloodRequest("token"), { wrapper });

    act(() => {
      result.current.setPatientName(validValues.patientName);
      result.current.setBloodGroup(validValues.bloodGroup);
      result.current.setUnitsRequired(validValues.unitsRequired);
      result.current.setLocationCityArea(validValues.locationCityArea);
      result.current.setUrgency(validValues.urgency);
      result.current.setSearchRadiusKm(4);
    });

    expect(result.current.fieldErrors.searchRadiusKm?.[0]).toBe("Minimum radius is 5km");
  });

  it("on first submit with unresolved location, requests geolocation instead of calling the API", async () => {
    mockGeolocationSuccess();
    const { result } = renderHook(() => useCreateBloodRequest("token"), { wrapper });

    act(() => {
      result.current.setPatientName(validValues.patientName);
      result.current.setBloodGroup(validValues.bloodGroup);
      result.current.setUnitsRequired(validValues.unitsRequired);
      result.current.setLocationCityArea(validValues.locationCityArea);
      result.current.setUrgency(validValues.urgency);
    });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: false });
    await waitFor(() => expect(result.current.geolocation.status).toBe("resolved"));
  });

  it("submits successfully once the form is valid and geolocation resolves", async () => {
    mockGeolocationSuccess();
    const { result } = renderHook(() => useCreateBloodRequest("token"), { wrapper });

    act(() => {
      result.current.setPatientName(validValues.patientName);
      result.current.setBloodGroup(validValues.bloodGroup);
      result.current.setUnitsRequired(validValues.unitsRequired);
      result.current.setLocationCityArea(validValues.locationCityArea);
      result.current.setUrgency(validValues.urgency);
    });

    // First submit resolves geolocation (see previous test); second submit actually calls the API.
    await act(async () => {
      await result.current.submit();
    });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: true, data: expect.objectContaining({ status: "Matching" }) });
  });

  it("surfaces a validation error from the API", async () => {
    server.use(createBloodRequestValidationErrorHandler);
    mockGeolocationSuccess();
    const { result } = renderHook(() => useCreateBloodRequest("token"), { wrapper });

    act(() => {
      result.current.setPatientName(validValues.patientName);
      result.current.setBloodGroup(validValues.bloodGroup);
      result.current.setUnitsRequired(validValues.unitsRequired);
      result.current.setLocationCityArea(validValues.locationCityArea);
      result.current.setUrgency(validValues.urgency);
    });

    await act(async () => {
      await result.current.submit();
    });
    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error?.message).toBe("Minimum radius is 5km"));
  });

  it("marks geolocation as denied when the user rejects the permission prompt", async () => {
    mockGeolocationDenied();
    const { result } = renderHook(() => useCreateBloodRequest("token"), { wrapper });

    act(() => {
      result.current.geolocation.request();
    });

    await waitFor(() => expect(result.current.geolocation.status).toBe("denied"));
  });
});
