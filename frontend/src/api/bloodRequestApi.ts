import { apiFetch } from "./httpClient";
import type { BloodGroup, UrgencyLevel } from "../lib/validation/bloodRequestSchemas";

export interface CreateBloodRequestRequest {
  patientName: string;
  bloodGroup: BloodGroup;
  unitsRequired: number;
  locationCityArea: string;
  latitude: number;
  longitude: number;
  searchRadiusKm: number;
  urgency: UrgencyLevel;
}

export interface BloodRequestDto {
  id: string;
  patientName: string;
  bloodGroup: BloodGroup;
  unitsRequired: number;
  locationCityArea: string;
  searchRadiusKm: number;
  urgency: UrgencyLevel;
  status: "Matching" | "Expired";
  createdAtUtc: string;
  expiresAtUtc: string;
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
