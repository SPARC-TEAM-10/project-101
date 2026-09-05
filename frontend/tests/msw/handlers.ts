import { http, HttpResponse } from "msw";

export const OTP_REQUEST_URL = "/api/v1/auth/otp/request";

export const successHandler = http.post(OTP_REQUEST_URL, async ({ request }) => {
  const body = (await request.json()) as { mobileNumber: string };
  return HttpResponse.json({
    maskedMobileNumber: `********${body.mobileNumber.slice(-2)}`,
    otpExpiresAtUtc: "2026-09-03T10:05:00.000Z",
    resendAvailableAtUtc: "2026-09-03T10:02:00.000Z",
  });
});

export const validationErrorHandler = http.post(OTP_REQUEST_URL, () => {
  return HttpResponse.json(
    {
      title: "Validation failed",
      status: 422,
      detail: "Please enter a valid 10-digit mobile number",
    },
    { status: 422 },
  );
});

export const cooldownErrorHandler = http.post(OTP_REQUEST_URL, () => {
  return HttpResponse.json(
    {
      title: "Too many requests",
      status: 429,
      detail: "Please wait before requesting another code.",
    },
    { status: 429 },
  );
});

export const gatewayErrorHandler = http.post(OTP_REQUEST_URL, () => {
  return HttpResponse.json(
    {
      title: "SMS gateway failure",
      status: 502,
      detail: "The SMS gateway failed to dispatch the OTP.",
    },
    { status: 502 },
  );
});

export const networkErrorHandler = http.post(OTP_REQUEST_URL, () => {
  return HttpResponse.error();
});

export const malformedErrorBodyHandler = http.post(OTP_REQUEST_URL, () => {
  return new HttpResponse("not json", { status: 500 });
});

export const handlers = [successHandler];
