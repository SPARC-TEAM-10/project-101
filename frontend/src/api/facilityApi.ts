import { apiFetch } from "./httpClient";
import type { FacilityCategory } from "../lib/validation/facilitySchemas";

export interface CreateFacilityContactRequest {
  name: string;
  designation: string;
  mobile: string;
}

export interface CreateFacilityRequest {
  facilityName: string;
  category: FacilityCategory;
  licenseNumber: string;
  address: string;
  contacts: CreateFacilityContactRequest[];
}

export interface FacilityDto {
  id: string;
  facilityName: string;
  category: FacilityCategory;
  licenseNumber: string;
  address: string;
  contacts: CreateFacilityContactRequest[];
  verificationStatus: "Pending";
  createdAtUtc: string;
}

// ASSUMED SHAPE (CHH-78) — not yet in contracts/chh-api.v1.yaml; derived from the CHH-F03
// Data Dictionary and task breakdown. Flagged for backend contract sign-off (see plan §4/§11).
// No [Authorize] gate assumed yet — Hospital/NGO role isn't issued (see AuthProvider.tsx).
export function createFacility(
  accessToken: string | undefined,
  request: CreateFacilityRequest,
): Promise<FacilityDto> {
  return apiFetch<FacilityDto>("/facilities", {
    method: "POST",
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
    body: JSON.stringify(request),
  });
}
