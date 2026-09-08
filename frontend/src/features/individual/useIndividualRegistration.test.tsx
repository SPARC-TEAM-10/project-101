import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import type { ReactNode } from "react";

import { useIndividualRegistration, type IndividualRegistrationSubmitResult } from "./useIndividualRegistration";
import { server } from "../../../tests/setup";
import { registerIndividualValidationErrorHandler } from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

function fillValid(result: { current: ReturnType<typeof useIndividualRegistration> }) {
  result.current.setFullName("Asha Menon");
  result.current.setEmail("asha.menon@example.com");
  result.current.setBloodGroup("O+");
  result.current.setDateOfBirth("2000-01-15");
  result.current.setGender("Female");
  result.current.setLocationCityArea("Kochi, Ernakulam");
}

describe("useIndividualRegistration", () => {
  it("blocks submit and reports field errors when the form is empty (AC1/AC3)", async () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: false });
    expect(result.current.touched).toBe(true);
    expect(result.current.fieldErrors.fullName?.[0]).toBe(
      "Please enter your full name. Name must be between 2 and 50 characters.",
    );
  });

  it("rejects a date of birth under 18 years old (AC3)", () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      fillValid(result);
      const under18 = new Date();
      under18.setFullYear(under18.getFullYear() - 10);
      result.current.setDateOfBirth(under18.toISOString().slice(0, 10));
    });

    expect(result.current.fieldErrors.dateOfBirth?.[0]).toBe(
      "Enter a valid date of birth. You must be 18 or older to register.",
    );
  });

  it("defaults to Eligible Donor when no health-restriction flag is set (AC3, US-CHH-002-02)", () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    expect(result.current.isReceiverOnly).toBe(false);
  });

  it("flags Receiver Only as soon as any restriction checkbox is set (AC2, US-CHH-002-02)", () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      result.current.setChronicIllness(true);
    });

    expect(result.current.isReceiverOnly).toBe(true);
  });

  it("requires other-illness details only when Other is checked (§6.4 AC2)", async () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      fillValid(result);
      result.current.setOtherIllness(true);
    });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: false });
    expect(result.current.fieldErrors.otherIllnessDetails?.[0]).toBe("Please specify other illness.");
  });

  it("submits successfully once all required fields are valid", async () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      fillValid(result);
    });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({
      ok: true,
      data: expect.objectContaining({ fullName: "Asha Menon", isReceiverOnly: false }),
    });
  });

  it("marks the created profile Receiver Only when a restriction was flagged", async () => {
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      fillValid(result);
      result.current.setUnderweight(true);
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

  it("fails submission when no mobile number could be resolved from the session", async () => {
    const { result } = renderHook(() => useIndividualRegistration(null), { wrapper });

    act(() => {
      fillValid(result);
    });

    let submitResult: IndividualRegistrationSubmitResult | undefined;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult?.ok).toBe(false);
    expect(submitResult?.error?.message).toBe("Couldn't create your account. Try again.");
  });

  it("surfaces a validation error from the API", async () => {
    server.use(registerIndividualValidationErrorHandler);
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      fillValid(result);
    });

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error?.message).toBe("You must be 18 or older to register."));
  });

  it("clears a prior submit error once a field is edited again", async () => {
    server.use(registerIndividualValidationErrorHandler);
    const { result } = renderHook(() => useIndividualRegistration("9876543210"), { wrapper });

    act(() => {
      fillValid(result);
    });
    await act(async () => {
      await result.current.submit();
    });
    await waitFor(() => expect(result.current.error).not.toBeNull());

    act(() => {
      result.current.setFullName("Asha M");
    });

    expect(result.current.error).toBeNull();
  });
});
