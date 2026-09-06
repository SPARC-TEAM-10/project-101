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

export const OTP_VERIFY_URL = "/api/v1/auth/otp/verify";

export const verifySuccessHandler = http.post(OTP_VERIFY_URL, async ({ request }) => {
  const body = (await request.json()) as { mobileNumber: string };
  return HttpResponse.json({
    maskedMobileNumber: `********${body.mobileNumber.slice(-2)}`,
    verifiedAtUtc: "2026-09-06T10:00:00.000Z",
  });
});

export const verifyInvalidOtpHandler = http.post(OTP_VERIFY_URL, () => {
  return HttpResponse.json(
    {
      title: "Invalid OTP",
      status: 422,
      detail: "Invalid OTP. Please try again.",
    },
    { status: 422 },
  );
});

export const BLOOD_REQUESTS_URL = "/api/v1/blood-requests";

export const createBloodRequestSuccessHandler = http.post(BLOOD_REQUESTS_URL, async ({ request }) => {
  const body = (await request.json()) as Record<string, unknown>;
  return HttpResponse.json(
    {
      id: "11111111-1111-1111-1111-111111111111",
      patientName: body.patientName,
      bloodGroup: body.bloodGroup,
      unitsRequired: body.unitsRequired,
      locationCityArea: body.locationCityArea,
      searchRadiusKm: body.searchRadiusKm,
      urgency: body.urgency,
      status: "Matching",
      createdAtUtc: "2026-09-07T00:00:00.000Z",
      expiresAtUtc: "2026-09-07T06:00:00.000Z",
    },
    { status: 201 },
  );
});

export const createBloodRequestValidationErrorHandler = http.post(BLOOD_REQUESTS_URL, () => {
  return HttpResponse.json(
    {
      title: "Validation failed",
      status: 422,
      detail: "Minimum radius is 5km",
    },
    { status: 422 },
  );
});

export const createBloodRequestUnauthorizedHandler = http.post(BLOOD_REQUESTS_URL, () => {
  return new HttpResponse(null, { status: 401 });
});

export const handlers = [successHandler, verifySuccessHandler, createBloodRequestSuccessHandler];
