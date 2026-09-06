import { useEffect, useRef, useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { requestOtp, verifyOtp, type OtpVerifyResponse } from "../../api/authApi";
import { ApiError } from "../../api/httpClient";

const DIGIT_COUNT = 6;

export interface OtpVerifySubmitResult {
  ok: boolean;
  data?: OtpVerifyResponse;
}

export interface UseOtpVerifyResult {
  digits: string[];
  setDigit: (index: number, value: string) => void;
  clearDigits: () => void;
  error: string | null;
  isPending: boolean;
  resendSecondsLeft: number;
  canResend: boolean;
  resend: () => Promise<void>;
  submit: () => Promise<OtpVerifySubmitResult>;
}

function secondsUntil(isoTimestamp: string): number {
  const diffMs = new Date(isoTimestamp).getTime() - Date.now();
  return Math.max(0, Math.ceil(diffMs / 1000));
}

export function useOtpVerify(
  mobileNumber: string,
  initialResendAvailableAtUtc: string,
): UseOtpVerifyResult {
  const [digits, setDigits] = useState<string[]>(() => Array(DIGIT_COUNT).fill(""));
  const [resendSecondsLeft, setResendSecondsLeft] = useState(() =>
    secondsUntil(initialResendAvailableAtUtc),
  );
  const submittedCode = useRef<string | null>(null);

  const verifyMutation = useMutation<OtpVerifyResponse, unknown, string>({
    mutationFn: (otpCode: string) => verifyOtp(mobileNumber, otpCode),
  });
  const resendMutation = useMutation({
    mutationFn: () => requestOtp(mobileNumber),
  });

  const isCounting = resendSecondsLeft > 0;
  useEffect(() => {
    if (!isCounting) return;
    const id = setInterval(() => {
      setResendSecondsLeft((s) => Math.max(0, s - 1));
    }, 1000);
    return () => clearInterval(id);
    // Deliberately keyed on isCounting only: the interval itself advances the
    // countdown via a functional update, so it must not restart every tick.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isCounting]);

  function clearDigits() {
    setDigits(Array(DIGIT_COUNT).fill(""));
    submittedCode.current = null;
  }

  function setDigit(index: number, value: string) {
    setDigits((prev) => {
      const next = [...prev];
      next[index] = value;
      return next;
    });
    if (verifyMutation.isError) {
      verifyMutation.reset();
    }
  }

  async function submit(): Promise<OtpVerifySubmitResult> {
    const code = digits.join("");
    if (code.length !== DIGIT_COUNT || submittedCode.current === code) {
      return { ok: false };
    }
    submittedCode.current = code;
    try {
      const data = await verifyMutation.mutateAsync(code);
      return { ok: true, data };
    } catch {
      clearDigits();
      return { ok: false };
    }
  }

  async function resend(): Promise<void> {
    if (resendSecondsLeft > 0) return;
    const data = await resendMutation.mutateAsync();
    setResendSecondsLeft(secondsUntil(data.resendAvailableAtUtc));
    verifyMutation.reset();
  }

  const error = verifyMutation.error
    ? verifyMutation.error instanceof ApiError
      ? "Invalid OTP. Please try again."
      : "Couldn't verify the code. Try again."
    : null;

  return {
    digits,
    setDigit,
    clearDigits,
    error,
    isPending: verifyMutation.isPending,
    resendSecondsLeft,
    canResend: resendSecondsLeft <= 0,
    resend,
    submit,
  };
}
