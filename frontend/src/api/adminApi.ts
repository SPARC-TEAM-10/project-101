import { apiFetch } from "./httpClient";
import type { FacilityCategory } from "../lib/validation/facilitySchemas";

export type FacilityVerificationStatus = "Pending" | "Verified" | "Rejected";

export interface FacilityContactDto {
  name: string;
  designation: string;
  mobile: string;
}

export interface FacilityDto {
  id: string;
  facilityName: string;
  category: FacilityCategory;
  licenseNumber: string;
  address: string;
  contacts: FacilityContactDto[];
  verificationStatus: FacilityVerificationStatus;
  licenseDocumentUrl: string | null;
  createdAtUtc: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// [Authorize(Roles = "SystemAdmin")]-protected (CHH-73/US-CHH-001-01).
export function getPendingFacilities(
  accessToken: string,
  page: number,
  pageSize: number,
): Promise<PagedResponse<FacilityDto>> {
  return apiFetch<PagedResponse<FacilityDto>>(`/admin/facilities/pending?page=${page}&pageSize=${pageSize}`, {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}
