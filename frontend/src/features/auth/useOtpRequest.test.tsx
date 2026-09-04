import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import type { ReactNode } from "react";

import { useOtpRequest } from "./useOtpRequest";
import { server } from "../../../tests/setup";
import {
  cooldownErrorHandler,
  gatewayErrorHandler,
  networkErrorHandler,
  validationErrorHandler,
} from "../../../tests/msw/handlers";

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

describe("useOtpRequest", () => {
  it("starts with an empty, invalid, untouched state", () => {
    const { result } = renderHook(() => useOtpRequest(), { wrapper });

    expect(result.current.mobileNumber).toBe("");
    expect(result.current.isValid).toBe(false);
    expect(result.current.touched).toBe(false);
    expect(result.current.isPending).toBe(false);
    expect(result.current.error).toBeNull();
  });

  it("marks the number invalid for fewer than 10 digits", () => {
    const { result } = renderHook(() => useOtpRequest(), { wrapper });

    act(() => result.current.setMobileNumber("98765"));

    expect(result.current.isValid).toBe(false);
  });

  it("marks the number invalid when it contains non-numeric characters", () => {
    const { result } = renderHook(() => useOtpRequest(), { wrapper });

    act(() => result.current.setMobileNumber("98765abc10"));

    expect(result.current.isValid).toBe(false);
  });

  it("marks a 10-digit numeric number valid", () => {
    const { result } = renderHook(() => useOtpRequest(), { wrapper });

    act(() => result.current.setMobileNumber("9876543210"));

    expect(result.current.isValid).toBe(true);
  });

  it("does not call the API when submit is invoked on an invalid number", async () => {
    const { result } = renderHook(() => useOtpRequest(), { wrapper });

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: false });
    expect(result.current.touched).toBe(true);
    expect(result.current.isPending).toBe(false);
  });

  it("returns the response data on a successful submit", async () => {
    const { result } = renderHook(() => useOtpRequest(), { wrapper });
    act(() => result.current.setMobileNumber("9876543210"));

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({
      ok: true,
      data: {
        maskedMobileNumber: "********10",
        otpExpiresAtUtc: "2026-09-03T10:05:00.000Z",
        resendAvailableAtUtc: "2026-09-03T10:02:00.000Z",
      },
    });
  });

  it("maps a 422 response to a validation error", async () => {
    server.use(validationErrorHandler);
    const { result } = renderHook(() => useOtpRequest(), { wrapper });
    act(() => result.current.setMobileNumber("9876543210"));

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error?.status).toBe(422));
    expect(result.current.error?.message).toBe("Please enter a valid 10-digit mobile number");
  });

  it("maps a 429 response to a cooldown error", async () => {
    server.use(cooldownErrorHandler);
    const { result } = renderHook(() => useOtpRequest(), { wrapper });
    act(() => result.current.setMobileNumber("9876543210"));

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error?.status).toBe(429));
    expect(result.current.error?.message).toBe("Please wait before requesting another code.");
  });

  it("maps a 502 response to a generic retry error", async () => {
    server.use(gatewayErrorHandler);
    const { result } = renderHook(() => useOtpRequest(), { wrapper });
    act(() => result.current.setMobileNumber("9876543210"));

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error?.status).toBe(502));
    expect(result.current.error?.message).toBe("Couldn't send the code. Try again.");
  });

  it("maps a plain network failure to a generic retry error", async () => {
    server.use(networkErrorHandler);
    const { result } = renderHook(() => useOtpRequest(), { wrapper });
    act(() => result.current.setMobileNumber("9876543210"));

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error?.status).toBeNull());
    expect(result.current.error?.message).toBe("Couldn't send the code. Try again.");
  });

  it("clears a previous error once the user edits the number again", async () => {
    server.use(validationErrorHandler);
    const { result } = renderHook(() => useOtpRequest(), { wrapper });
    act(() => result.current.setMobileNumber("9876543210"));
    await act(async () => {
      await result.current.submit();
    });
    await waitFor(() => expect(result.current.error).not.toBeNull());

    act(() => result.current.setMobileNumber("9876543211"));

    expect(result.current.error).toBeNull();
  });
});
