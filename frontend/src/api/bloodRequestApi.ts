import { apiFetch } from "./httpClient";
import type { BloodGroup, UrgencyLevel } from "../lib/validation/bloodRequestSchemas";

export interface CreateBloodRequestRequest {
  requesterName: string;
  patientName: string;
  bloodGroup: BloodGroup;
  unitsRequired: number;
  locationCityArea: string;
  latitude: number;
  longitude: number;
  searchRadiusKm: number;
  urgency: UrgencyLevel;
}

export type BloodRequestStatus = "Matching" | "Expired" | "Fulfilled";

export interface BloodRequestDto {
  id: string;
  requesterName: string;
  patientName: string;
  bloodGroup: BloodGroup;
  unitsRequired: number;
  locationCityArea: string;
  searchRadiusKm: number;
  urgency: UrgencyLevel;
  status: BloodRequestStatus;
  createdAtUtc: string;
  expiresAtUtc: string;
}

export interface DonorStatusDto {
  label: string;
  mobileNumber?: string;
  isAccepted: boolean;
}

export interface BloodRequestMatchStatusDto {
  bloodRequestId: string;
  status: BloodRequestStatus;
  searchRadiusKm: number;
  unitsRequired: number;
  unitsAccepted: number;
  expiresAtUtc: string;
  notifiedCount: number;
  viewedCount: number;
  acceptedCount: number;
  donors: DonorStatusDto[];
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}

// [Authorize]-protected (CHH-33/US-CHH-004-01) — the caller must supply a valid access token
// (from OTP verify, CHH-F01 AC3).
export function createBloodRequest(
  accessToken: string,
  request: CreateBloodRequestRequest,
): Promise<BloodRequestDto> {
  return apiFetch<BloodRequestDto>("/blood-requests", {
    method: "POST",
    headers: { Authorization: `Bearer ${accessToken}` },
    body: JSON.stringify(request),
  });
}

// [Authorize]-protected (CHH-81 Individual Dashboard) — the caller's own requests, newest first.
export function getMyBloodRequests(
  accessToken: string,
  page = 1,
  pageSize = 20,
): Promise<PagedResponse<BloodRequestDto>> {
  return apiFetch<PagedResponse<BloodRequestDto>>(`/blood-requests/mine?page=${page}&pageSize=${pageSize}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

// [Authorize]-protected (CHH-36 AC1/AC2) — the caller's own request's match/response status.
export function getBloodRequestMatchStatus(accessToken: string, id: string): Promise<BloodRequestMatchStatusDto> {
  return apiFetch<BloodRequestMatchStatusDto>(`/blood-requests/${id}/matches`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}

// [Authorize]-protected (CHH-36 AC4) — expands the caller's own request's search radius and
// re-triggers matching. 422 if the request is no longer Matching, or the new radius isn't larger.
export function updateBloodRequestRadius(
  accessToken: string,
  id: string,
  searchRadiusKm: number,
): Promise<BloodRequestMatchStatusDto> {
  return apiFetch<BloodRequestMatchStatusDto>(`/blood-requests/${id}/radius`, {
    method: "PATCH",
    headers: { Authorization: `Bearer ${accessToken}` },
    body: JSON.stringify({ searchRadiusKm }),
  });
}
