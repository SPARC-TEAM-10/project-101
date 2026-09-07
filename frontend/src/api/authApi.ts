import { apiFetch } from "./httpClient";
import type { Role } from "../context/AuthProvider";

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

export interface OtpVerifyRequest {
  mobileNumber: string;
  otpCode: string;
}

export interface OtpVerifyResponse {
  maskedMobileNumber: string;
  verifiedAtUtc: string;
  accessToken: string;
  tokenExpiresAtUtc: string;
  role: Role;
}

export function verifyOtp(mobileNumber: string, otpCode: string): Promise<OtpVerifyResponse> {
  return apiFetch<OtpVerifyResponse>("/auth/otp/verify", {
    method: "POST",
    body: JSON.stringify({ mobileNumber, otpCode } satisfies OtpVerifyRequest),
  });
}
