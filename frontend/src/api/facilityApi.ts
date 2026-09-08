import { apiFetch } from "./httpClient";
import type { FacilityCategory, FacilitySubCategory } from "../lib/validation/facilitySchemas";

export interface CreateFacilityContactRequest {
  name: string;
  designation: string;
  mobile: string;
}

export interface CreateFacilityRequest {
  facilityName: string;
  category: FacilityCategory;
  subCategory: FacilitySubCategory;
  licenseNumber: string;
  address: string;
  contacts: CreateFacilityContactRequest[];
}

export interface FacilityDto {
  id: string;
  facilityName: string;
  category: FacilityCategory;
  subCategory: FacilitySubCategory;
  licenseNumber: string;
  address: string;
  contacts: CreateFacilityContactRequest[];
  verificationStatus: "Pending";
  createdAtUtc: string;
}

// Matches contracts/chh-api.v1.yaml's POST /facilities (CHH-78). No [Authorize] gate — Hospital/NGO
// role isn't issued yet (see AuthProvider.tsx), matching individualApi.ts's registerIndividual precedent.
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
