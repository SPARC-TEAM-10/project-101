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
    accessToken: "test-access-token",
    tokenExpiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    role: "Individual",
  });
});

export const verifySuccessGuestRoleHandler = http.post(OTP_VERIFY_URL, async ({ request }) => {
  const body = (await request.json()) as { mobileNumber: string };
  return HttpResponse.json({
    maskedMobileNumber: `********${body.mobileNumber.slice(-2)}`,
    verifiedAtUtc: "2026-09-06T10:00:00.000Z",
    accessToken: "test-access-token-guest",
    tokenExpiresAtUtc: new Date(Date.now() + 60 * 60 * 1000).toISOString(),
    role: "Guest",
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

export const FACILITIES_URL = "/api/v1/facilities";

export const createFacilitySuccessHandler = http.post(FACILITIES_URL, async ({ request }) => {
  const body = (await request.json()) as Record<string, unknown>;
  return HttpResponse.json(
    {
      id: "22222222-2222-2222-2222-222222222222",
      facilityName: body.facilityName,
      category: body.category,
      subCategory: body.subCategory,
      licenseNumber: body.licenseNumber,
      address: body.address,
      contacts: body.contacts,
      verificationStatus: "Pending",
      createdAtUtc: "2026-09-07T00:00:00.000Z",
    },
    { status: 201 },
  );
});

export const createFacilityValidationErrorHandler = http.post(FACILITIES_URL, () => {
  return HttpResponse.json(
    {
      title: "Validation failed",
      status: 422,
      detail: "Licence number can contain letters, numbers and hyphens only.",
    },
    { status: 422 },
  );
});

export const createFacilityNetworkErrorHandler = http.post(FACILITIES_URL, () => {
  return HttpResponse.error();
});

// No MSW handler for POST /api/v1/facilities/:id/upload — @mswjs/interceptors hangs under jsdom
// on any XHR request whose body is a FormData containing a Blob/File (see tests/fakeXhr.ts's doc
// comment). facilityApi.uploadFacilityLicense is tested via that fake XHR instead.

export const FACILITY_ME_URL = "/api/v1/facilities/me";

const facilityMeBase = {
  id: "22222222-2222-2222-2222-222222222222",
  facilityName: "Kochi Metro Hospital",
  category: "Hospital",
  licenseNumber: "KL-HOSP-448120",
  address: "Marine Drive, Ernakulam, Kochi",
  contacts: [{ name: "Anitha Varghese", designation: "Blood bank officer", mobile: "9876500112" }],
  licenseDocumentUrl: null,
  createdAtUtc: "2026-09-05T00:00:00.000Z",
  updatedAtUtc: "2026-09-07T00:00:00.000Z",
};

export const getMyFacilityPendingHandler = http.get(FACILITY_ME_URL, () =>
  HttpResponse.json({ ...facilityMeBase, verificationStatus: "Pending", rejectionReason: null }),
);

export const getMyFacilityApprovedHandler = http.get(FACILITY_ME_URL, () =>
  HttpResponse.json({ ...facilityMeBase, verificationStatus: "Verified", rejectionReason: null }),
);

export const getMyFacilityRejectedHandler = http.get(FACILITY_ME_URL, () =>
  HttpResponse.json({
    ...facilityMeBase,
    verificationStatus: "Rejected",
    rejectionReason: "The licence document expired on 31 March 2025. Upload a currently valid licence and we will review it again.",
  }),
);

export const getMyFacilityNotFoundHandler = http.get(FACILITY_ME_URL, () => new HttpResponse(null, { status: 404 }));

export const INDIVIDUALS_ME_URL = "/api/v1/individuals/me";

export const getMyProfileSuccessHandler = http.get(INDIVIDUALS_ME_URL, () =>
  HttpResponse.json({
    id: "33333333-3333-3333-3333-333333333333",
    fullName: "Ananya Nair",
    bloodGroup: "O+",
    isReceiverOnly: false,
    locationCityArea: "Kaloor, Kochi",
    createdAtUtc: "2026-08-01T00:00:00.000Z",
    isChronicIllness: false,
    hasRecentSurgery: false,
    isInfectiousDisease: false,
    isUnderweight: false,
    isOtherIllness: false,
    otherIllnessDetails: null,
  }),
);

export const getMyProfileNotFoundHandler = http.get(INDIVIDUALS_ME_URL, () => new HttpResponse(null, { status: 404 }));

export const updateMyProfileSuccessHandler = http.patch(INDIVIDUALS_ME_URL, async ({ request }) => {
  const body = (await request.json()) as Record<string, unknown>;
  return HttpResponse.json({
    id: "33333333-3333-3333-3333-333333333333",
    fullName: "Ananya Nair",
    bloodGroup: "O+",
    isReceiverOnly: Boolean(
      body.isChronicIllness || body.hasRecentSurgery || body.isInfectiousDisease || body.isUnderweight || body.isOtherIllness,
    ),
    locationCityArea: body.locationCityArea,
    createdAtUtc: "2026-08-01T00:00:00.000Z",
    isChronicIllness: Boolean(body.isChronicIllness),
    hasRecentSurgery: Boolean(body.hasRecentSurgery),
    isInfectiousDisease: Boolean(body.isInfectiousDisease),
    isUnderweight: Boolean(body.isUnderweight),
    isOtherIllness: Boolean(body.isOtherIllness),
    otherIllnessDetails: body.otherIllnessDetails ?? null,
  });
});

export const BLOOD_REQUESTS_MINE_URL = "/api/v1/blood-requests/mine";

export const getMyBloodRequestsEmptyHandler = http.get(BLOOD_REQUESTS_MINE_URL, () =>
  HttpResponse.json({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
);

export const getMyBloodRequestsSuccessHandler = http.get(BLOOD_REQUESTS_MINE_URL, () =>
  HttpResponse.json({
    items: [
      {
        id: "11111111-1111-1111-1111-111111111111",
        patientName: "John Doe",
        bloodGroup: "O+",
        unitsRequired: 2,
        locationCityArea: "Kaloor, Kochi",
        searchRadiusKm: 10,
        urgency: "Emergency",
        status: "Matching",
        createdAtUtc: new Date().toISOString(),
        expiresAtUtc: new Date(Date.now() + 6 * 60 * 60 * 1000).toISOString(),
      },
    ],
    totalCount: 1,
    page: 1,
    pageSize: 20,
  }),
);

export const nominatimReverseGeocodeHandler = http.get(
  "https://nominatim.openstreetmap.org/reverse",
  () => HttpResponse.json({ address: { city: "Kochi", postcode: "682017" } }),
);

export const INDIVIDUALS_URL = "/api/v1/individuals";

export const registerIndividualSuccessHandler = http.post(INDIVIDUALS_URL, async ({ request }) => {
  const body = (await request.json()) as Record<string, unknown>;
  return HttpResponse.json(
    {
      id: "33333333-3333-3333-3333-333333333333",
      fullName: body.fullName,
      bloodGroup: body.bloodGroup,
      isReceiverOnly: Boolean(
        body.isChronicIllness || body.hasRecentSurgery || body.isInfectiousDisease || body.isUnderweight || body.isOtherIllness,
      ),
      createdAtUtc: "2026-09-08T00:00:00.000Z",
    },
    { status: 201 },
  );
});

export const registerIndividualValidationErrorHandler = http.post(INDIVIDUALS_URL, () => {
  return HttpResponse.json(
    {
      title: "Validation failed",
      status: 422,
      detail: "You must be 18 or older to register.",
    },
    { status: 422 },
  );
});

export const registerIndividualConflictHandler = http.post(INDIVIDUALS_URL, () => {
  return HttpResponse.json(
    {
      title: "Conflict",
      status: 409,
      detail: "An individual profile already exists for this mobile number.",
    },
    { status: 409 },
  );
});

export const NOTIFICATIONS_MINE_URL = "/api/v1/notifications/mine";

export const getMyNotificationsEmptyHandler = http.get(NOTIFICATIONS_MINE_URL, () =>
  HttpResponse.json({ items: [], totalCount: 0, page: 1, pageSize: 20 }),
);

export const getMyNotificationsSuccessHandler = http.get(NOTIFICATIONS_MINE_URL, () =>
  HttpResponse.json({
    items: [
      {
        id: "44444444-4444-4444-4444-444444444444",
        bloodRequestId: "11111111-1111-1111-1111-111111111111",
        bloodGroup: "O+",
        unitsRequired: 2,
        urgency: "Emergency",
        distanceKm: 4.2,
        areaLabel: "Kaloor, Kochi",
        isRead: false,
        createdAtUtc: new Date().toISOString(),
        responseStatus: "Pending",
      },
    ],
    totalCount: 1,
    page: 1,
    pageSize: 20,
  }),
);

export const markNotificationReadSuccessHandler = http.patch("/api/v1/notifications/:id/read", () =>
  HttpResponse.json({
    id: "44444444-4444-4444-4444-444444444444",
    bloodRequestId: "11111111-1111-1111-1111-111111111111",
    bloodGroup: "O+",
    unitsRequired: 2,
    urgency: "Emergency",
    distanceKm: 4.2,
    areaLabel: "Kaloor, Kochi",
    isRead: true,
    createdAtUtc: new Date().toISOString(),
    responseStatus: "Pending",
  }),
);

export const acceptNotificationSuccessHandler = http.patch("/api/v1/notifications/:id/accept", ({ params }) =>
  HttpResponse.json({
    notificationId: params.id,
    responseStatus: "Accepted",
    requesterMobileNumber: "9123456789",
    locationCityArea: "Kaloor, Kochi",
    latitude: 9.9312,
    longitude: 76.2673,
  }),
);

export const acceptNotificationNoLongerActiveHandler = http.patch("/api/v1/notifications/:id/accept", () =>
  HttpResponse.json(
    { title: "Unprocessable Entity", status: 422, detail: "This request is no longer active" },
    { status: 422 },
  ),
);

export const declineNotificationSuccessHandler = http.patch("/api/v1/notifications/:id/decline", ({ params }) =>
  HttpResponse.json({
    notificationId: params.id,
    responseStatus: "Declined",
  }),
);

export const getBloodRequestMatchStatusNoMatchesHandler = http.get(
  "/api/v1/blood-requests/:id/matches",
  ({ params }) =>
    HttpResponse.json({
      bloodRequestId: params.id,
      status: "Matching",
      searchRadiusKm: 10,
      unitsRequired: 2,
      unitsAccepted: 0,
      expiresAtUtc: new Date(Date.now() + 6 * 60 * 60 * 1000).toISOString(),
      notifiedCount: 0,
      viewedCount: 0,
      acceptedCount: 0,
      donors: [],
    }),
);

export const getBloodRequestMatchStatusWithDonorsHandler = http.get(
  "/api/v1/blood-requests/:id/matches",
  ({ params }) =>
    HttpResponse.json({
      bloodRequestId: params.id,
      status: "Matching",
      searchRadiusKm: 10,
      unitsRequired: 2,
      unitsAccepted: 1,
      expiresAtUtc: new Date(Date.now() + 6 * 60 * 60 * 1000).toISOString(),
      notifiedCount: 3,
      viewedCount: 2,
      acceptedCount: 1,
      donors: [
        { label: "Donor 1", isAccepted: false },
        { label: "Ravi Kumar", mobileNumber: "9123456789", isAccepted: true },
        { label: "Donor 3", isAccepted: false },
      ],
    }),
);

export const updateBloodRequestRadiusSuccessHandler = http.patch(
  "/api/v1/blood-requests/:id/radius",
  async ({ params, request }) => {
    const body = (await request.json()) as { searchRadiusKm: number };
    return HttpResponse.json({
      bloodRequestId: params.id,
      status: "Matching",
      searchRadiusKm: body.searchRadiusKm,
      unitsRequired: 2,
      unitsAccepted: 0,
      expiresAtUtc: new Date(Date.now() + 6 * 60 * 60 * 1000).toISOString(),
      notifiedCount: 0,
      viewedCount: 0,
      acceptedCount: 0,
      donors: [],
    });
  },
);

export const handlers = [
  successHandler,
  verifySuccessHandler,
  createBloodRequestSuccessHandler,
  createFacilitySuccessHandler,
  getMyProfileSuccessHandler,
  updateMyProfileSuccessHandler,
  getMyBloodRequestsEmptyHandler,
  getMyNotificationsEmptyHandler,
  markNotificationReadSuccessHandler,
  acceptNotificationSuccessHandler,
  declineNotificationSuccessHandler,
  nominatimReverseGeocodeHandler,
  registerIndividualSuccessHandler,
];
