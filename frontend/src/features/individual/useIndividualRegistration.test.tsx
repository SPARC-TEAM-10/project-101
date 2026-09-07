import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it, vi, beforeEach } from "vitest";
import type { ReactNode } from "react";

import { useIndividualRegistration } from "./useIndividualRegistration";
import { server } from "../../../tests/setup";
import {
  registerIndividualConflictHandler,
  registerIndividualValidationErrorHandler,
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

const validValues = {
  fullName: "Jane Doe",
  email: "jane@example.com",
  bloodGroup: "O+" as const,
  dateOfBirth: "1998-05-10",
  gender: "Female" as const,
  locationCityArea: "Kaloor, Kochi",
};

function fillValidForm(result: ReturnType<typeof useIndividualRegistration>) {
  result.setFullName(validValues.fullName);
  result.setEmail(validValues.email);
  result.setBloodGroup(validValues.bloodGroup);
  result.setDateOfBirth(validValues.dateOfBirth);
  result.setGender(validValues.gender);
  result.setLocationCityArea(validValues.locationCityArea);
}

describe("useIndividualRegistration", () => {
  beforeEach(() => {
    vi.restoreAllMocks();
  });

  it("starts invalid with all default (empty) values", () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    expect(result.current.isValid).toBe(false);
    expect(result.current.touched).toBe(false);
  });

  it("becomes valid once every required field is filled with valid data", () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => fillValidForm(result.current));

    expect(result.current.isValid).toBe(true);
  });

  it("submit is a no-op and marks the form touched when required fields are missing", async () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: false });
    expect(result.current.touched).toBe(true);
  });

  it("submits successfully and returns the created profile", async () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => fillValidForm(result.current));

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({
      ok: true,
      data: expect.objectContaining({ fullName: validValues.fullName, isReceiverOnly: false }),
    });
  });

  it("marks the profile Receiver Only when a health restriction flag is set on submit", async () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      fillValidForm(result.current);
      result.current.setIsChronicIllness(true);
    });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({
      ok: true,
      data: expect.objectContaining({ isReceiverOnly: true }),
    });
  });

  it("surfaces a 409 conflict with a specific message", async () => {
    server.use(registerIndividualConflictHandler);
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => fillValidForm(result.current));

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() =>
      expect(result.current.error).toEqual({
        status: 409,
        message: "A profile already exists for this mobile number.",
      }),
    );
  });

  it("surfaces a 422 validation error from the API", async () => {
    server.use(registerIndividualValidationErrorHandler);
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => fillValidForm(result.current));

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error?.message).toBe("Please enter a valid email address"));
  });

  it("fails with a generic error when no mobile number is available", async () => {
    const { result } = renderHook(() => useIndividualRegistration(undefined), { wrapper });

    act(() => fillValidForm(result.current));

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({
      ok: false,
      error: { status: null, message: "Couldn't complete registration. Try again." },
    });
  });

  it("auto-fills the location field once current-location detection resolves an address", async () => {
    mockGeolocationSuccess();
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      result.current.geolocation.request();
    });

    await waitFor(() => expect(result.current.values.locationCityArea).toBeTruthy());
  });
});
