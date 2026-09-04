import { apiFetch } from "./httpClient";

export interface OtpRequestRequest {
  mobileNumber: string;
}

export interface OtpRequestResponse {
  maskedMobileNumber: string;
  otpExpiresAtUtc: string;
  resendAvailableAtUtc: string;
}

export function requestOtp(mobileNumber: string): Promise<OtpRequestResponse> {
  return apiFetch<OtpRequestResponse>("/auth/otp/request", {
    method: "POST",
    body: JSON.stringify({ mobileNumber } satisfies OtpRequestRequest),
  });
}
