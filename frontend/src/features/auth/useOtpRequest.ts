import { useState } from "react";
import { useMutation } from "@tanstack/react-query";

import { requestOtp, type OtpRequestResponse } from "../../api/authApi";
import { ApiError } from "../../api/httpClient";
import { mobileNumberSchema } from "../../lib/validation/authSchemas";

export interface OtpRequestError {
  status: number | null;
  message: string;
}

export interface OtpRequestSubmitResult {
  ok: boolean;
  data?: OtpRequestResponse;
}

export interface UseOtpRequestResult {
  mobileNumber: string;
  setMobileNumber: (value: string) => void;
  isValid: boolean;
  touched: boolean;
  markTouched: () => void;
  submit: () => Promise<OtpRequestSubmitResult>;
  isPending: boolean;
  error: OtpRequestError | null;
}

function toOtpRequestError(err: unknown): OtpRequestError {
  if (err instanceof ApiError) {
    if (err.status === 422) {
      return {
        status: 422,
        message: err.problem.detail ?? "Please enter a valid 10-digit mobile number",
      };
    }
    if (err.status === 429) {
      return {
        status: 429,
        message: err.problem.detail ?? "Please wait before requesting another code.",
      };
    }
    return { status: err.status, message: "Couldn't send the code. Try again." };
  }
  return { status: null, message: "Couldn't send the code. Try again." };
}

export function useOtpRequest(): UseOtpRequestResult {
  const [mobileNumber, setMobileNumberState] = useState("");
  const [touched, setTouched] = useState(false);

  const isValid = mobileNumberSchema.safeParse(mobileNumber).success;

  const mutation = useMutation<OtpRequestResponse, unknown, string>({
    mutationFn: (value: string) => requestOtp(value),
  });

  function setMobileNumber(value: string) {
    setMobileNumberState(value);
    if (mutation.isError) {
      mutation.reset();
    }
  }

  async function submit(): Promise<OtpRequestSubmitResult> {
    setTouched(true);
    if (!isValid) {
      return { ok: false };
    }
    try {
      const data = await mutation.mutateAsync(mobileNumber);
      return { ok: true, data };
    } catch {
      return { ok: false };
    }
  }

  return {
    mobileNumber,
    setMobileNumber,
    isValid,
    touched,
    markTouched: () => setTouched(true),
    submit,
    isPending: mutation.isPending,
    error: mutation.error ? toOtpRequestError(mutation.error) : null,
  };
}
