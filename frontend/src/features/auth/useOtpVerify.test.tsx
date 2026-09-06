import { act, renderHook, waitFor } from "@testing-library/react";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { describe, expect, it } from "vitest";
import type { ReactNode } from "react";
import { http, HttpResponse } from "msw";

import { useOtpVerify } from "./useOtpVerify";
import { OTP_REQUEST_URL, gatewayErrorHandler, verifyInvalidOtpHandler } from "../../../tests/msw/handlers";
import { server } from "../../../tests/setup";

const FAR_FUTURE = new Date(Date.now() + 5 * 60 * 1000).toISOString();

function wrapper({ children }: { children: ReactNode }) {
  const queryClient = new QueryClient({
    defaultOptions: { queries: { retry: false }, mutations: { retry: false } },
  });
  return <QueryClientProvider client={queryClient}>{children}</QueryClientProvider>;
}

function fillDigits(setDigit: (i: number, v: string) => void, code: string) {
  code.split("").forEach((digit, i) => act(() => setDigit(i, digit)));
}

describe("useOtpVerify", () => {
  it("starts with 6 empty digits and a countdown derived from resendAvailableAtUtc", () => {
    const { result } = renderHook(() => useOtpVerify("9876543210", FAR_FUTURE), { wrapper });

    expect(result.current.digits).toEqual(["", "", "", "", "", ""]);
    expect(result.current.resendSecondsLeft).toBeGreaterThan(0);
    expect(result.current.canResend).toBe(false);
  });

  it("submit() does nothing until all 6 digits are present", async () => {
    const { result } = renderHook(() => useOtpVerify("9876543210", FAR_FUTURE), { wrapper });
    fillDigits(result.current.setDigit, "427");

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({ ok: false });
  });

  it("submit() verifies and returns the response on success", async () => {
    const { result } = renderHook(() => useOtpVerify("9876543210", FAR_FUTURE), { wrapper });
    fillDigits(result.current.setDigit, "427159");

    let submitResult;
    await act(async () => {
      submitResult = await result.current.submit();
    });

    expect(submitResult).toEqual({
      ok: true,
      data: { maskedMobileNumber: "********10", verifiedAtUtc: "2026-09-06T10:00:00.000Z" },
    });
  });

  it("does not re-submit the same code twice", async () => {
    const { result } = renderHook(() => useOtpVerify("9876543210", FAR_FUTURE), { wrapper });
    fillDigits(result.current.setDigit, "427159");

    await act(async () => {
      await result.current.submit();
    });
    let secondResult;
    await act(async () => {
      secondResult = await result.current.submit();
    });

    expect(secondResult).toEqual({ ok: false });
  });

  it("maps a 422 to the AC2 error and clears the digits", async () => {
    server.use(verifyInvalidOtpHandler);
    const { result } = renderHook(() => useOtpVerify("9876543210", FAR_FUTURE), { wrapper });
    fillDigits(result.current.setDigit, "427159");

    await act(async () => {
      await result.current.submit();
    });

    await waitFor(() => expect(result.current.error).toBe("Invalid OTP. Please try again."));
    expect(result.current.digits).toEqual(["", "", "", "", "", ""]);
  });

  it("counts down and flips canResend once the resend timestamp elapses", async () => {
    const nearFuture = new Date(Date.now() + 1200).toISOString();
    const { result } = renderHook(() => useOtpVerify("9876543210", nearFuture), { wrapper });

    expect(result.current.canResend).toBe(false);
    await waitFor(() => expect(result.current.canResend).toBe(true), { timeout: 3000 });
    expect(result.current.resendSecondsLeft).toBe(0);
  });

  it("resend() resets the countdown from the new resendAvailableAtUtc on success", async () => {
    server.use(
      http.post(OTP_REQUEST_URL, () =>
        HttpResponse.json({
          maskedMobileNumber: "********10",
          otpExpiresAtUtc: FAR_FUTURE,
          resendAvailableAtUtc: FAR_FUTURE,
        }),
      ),
    );
    const nearFuture = new Date(Date.now() + 500).toISOString();
    const { result } = renderHook(() => useOtpVerify("9876543210", nearFuture), { wrapper });
    await waitFor(() => expect(result.current.canResend).toBe(true), { timeout: 3000 });

    await act(async () => {
      await result.current.resend();
    });

    expect(result.current.resendSecondsLeft).toBeGreaterThan(0);
    expect(result.current.resendError).toBeNull();
  });

  it("resend() catches failure and surfaces resendError instead of throwing", async () => {
    server.use(gatewayErrorHandler);
    const nearFuture = new Date(Date.now() + 500).toISOString();
    const { result } = renderHook(() => useOtpVerify("9876543210", nearFuture), { wrapper });
    await waitFor(() => expect(result.current.canResend).toBe(true), { timeout: 3000 });

    await act(async () => {
      await result.current.resend();
    });

    await waitFor(() =>
      expect(result.current.resendError).toBe("Couldn't resend the code. Try again."),
    );
  });
});
