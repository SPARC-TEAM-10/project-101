import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import type { ReactNode } from "react";

import { useUpdateIndividualProfile } from "./useUpdateIndividualProfile";
import type { IndividualProfileDto } from "../../api/individualApi";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

function makeProfile(overrides: Partial<IndividualProfileDto> = {}): IndividualProfileDto {
  return {
    id: "33333333-3333-3333-3333-333333333333",
    fullName: "Ananya Nair",
    bloodGroup: "O+",
    isReceiverOnly: false,
    locationCityArea: "Kaloor, Kochi",
    createdAtUtc: "2026-08-01T00:00:00.000Z",
    isChronicIllness: false,
    hasRecentSurgery: false,
    isInfectiousDisease: false,
    isUnderweight: false,
    isOtherIllness: false,
    otherIllnessDetails: undefined,
    ...overrides,
  };
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

describe("useUpdateIndividualProfile", () => {
  it("CHH-85: reports locationShared as false when the profile has no coordinates yet", () => {
    const { result } = renderHook(() => useUpdateIndividualProfile("token", makeProfile()), { wrapper });

    expect(result.current.locationShared).toBe(false);
  });

  it("CHH-85: reports locationShared as true once the profile has coordinates", () => {
    const { result } = renderHook(
      () => useUpdateIndividualProfile("token", makeProfile({ latitude: 9.9312, longitude: 76.2673 })),
      { wrapper },
    );

    expect(result.current.locationShared).toBe(true);
  });

  it("CHH-85: resolved coordinates flow into the form's submitted values after shareLocation()", async () => {
    mockGeolocationSuccess();
    const { result } = renderHook(() => useUpdateIndividualProfile("token", makeProfile()), { wrapper });

    act(() => {
      result.current.shareLocation();
    });

    await waitFor(() => expect(result.current.geolocation.status).toBe("resolved"));
    expect(result.current.values.latitude).toBe(9.9312);
    expect(result.current.values.longitude).toBe(76.2673);
  });

  it("CHH-85: shareLocation() sets isSharingLocation, and clearSharingLocation() resets it", () => {
    mockGeolocationSuccess();
    const { result } = renderHook(() => useUpdateIndividualProfile("token", makeProfile()), { wrapper });

    act(() => {
      result.current.shareLocation();
    });
    expect(result.current.isSharingLocation).toBe(true);

    act(() => {
      result.current.clearSharingLocation();
    });
    expect(result.current.isSharingLocation).toBe(false);
  });

  it("CHH-85: denied geolocation permission never populates coordinates", async () => {
    mockGeolocationDenied();
    const { result } = renderHook(() => useUpdateIndividualProfile("token", makeProfile()), { wrapper });

    act(() => {
      result.current.shareLocation();
    });

    await waitFor(() => expect(result.current.geolocation.status).toBe("denied"));
    expect(result.current.values.latitude).toBeUndefined();
    expect(result.current.values.longitude).toBeUndefined();
  });

  it("does not fail validation for a profile that has never shared a location (null coordinates from the API)", () => {
    const { result } = renderHook(() => useUpdateIndividualProfile("token", makeProfile()), { wrapper });

    expect(result.current.fieldErrors.latitude).toBeUndefined();
    expect(result.current.fieldErrors.longitude).toBeUndefined();
  });
});
