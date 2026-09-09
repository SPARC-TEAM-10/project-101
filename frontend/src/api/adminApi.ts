import { apiFetch } from "./httpClient";
import type { FacilityCategory } from "../lib/validation/facilitySchemas";

export type FacilityVerificationStatus = "Pending" | "Verified" | "Rejected";
export type FacilityVerificationDecision = "Approve" | "Reject";
export type AccountStatus = "Active" | "Suspended";

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
  rejectionReason: string | null;
  createdAtUtc: string;
}

export interface AdminUserDto {
  id: string;
  mobileNumber: string;
  fullName: string;
  bloodGroup: string;
  accountStatus: AccountStatus;
  suspensionReason: string | null;
  createdAtUtc: string;
}

export interface PagedResponse<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

function authHeaders(accessToken: string) {
  return { headers: { Authorization: `Bearer ${accessToken}` } };
}

// licenseDocumentUrl is app-relative ("/uploads/facility-documents/...", served by the API host
// itself, not under /api/v1 — see backend's FacilityDocumentStorageOptions.UrlPrefix) — resolve it
// against the API origin (not the frontend's Vercel origin) so "View Document" opens correctly
// across the split-cloud deployment (root CLAUDE.md Decisions Log 2026-09-05).
export function resolveDocumentUrl(licenseDocumentUrl: string): string {
  return `${import.meta.env.VITE_API_BASE_URL ?? ""}${licenseDocumentUrl}`;
}

// [Authorize(Roles = "SystemAdmin")]-protected (CHH-73/US-CHH-001-01).
export function getPendingFacilities(
  accessToken: string,
  page: number,
  pageSize: number,
): Promise<PagedResponse<FacilityDto>> {
  return apiFetch<PagedResponse<FacilityDto>>(
    `/admin/facilities/pending?page=${page}&pageSize=${pageSize}`,
    authHeaders(accessToken),
  );
}

// [Authorize(Roles = "SystemAdmin")]-protected (CHH-75/US-CHH-001-03).
export function reviewFacility(
  accessToken: string,
  facilityId: string,
  decision: FacilityVerificationDecision,
  rejectionReason?: string,
): Promise<FacilityDto> {
  return apiFetch<FacilityDto>(`/admin/facilities/${facilityId}/verification`, {
    method: "PATCH",
    ...authHeaders(accessToken),
    body: JSON.stringify({ decision, rejectionReason: rejectionReason ?? null }),
  });
}

// [Authorize(Roles = "SystemAdmin")]-protected (CHH-76/US-CHH-001-04).
export function searchAdminUsers(
  accessToken: string,
  search: string,
  page: number,
  pageSize: number,
): Promise<PagedResponse<AdminUserDto>> {
  const query = new URLSearchParams({ page: String(page), pageSize: String(pageSize) });
  if (search.trim()) {
    query.set("search", search.trim());
  }
  return apiFetch<PagedResponse<AdminUserDto>>(`/admin/users?${query.toString()}`, authHeaders(accessToken));
}

// [Authorize(Roles = "SystemAdmin")]-protected (CHH-76/US-CHH-001-04 AC1).
export function suspendUser(accessToken: string, userId: string, reason: string): Promise<AdminUserDto> {
  return apiFetch<AdminUserDto>(`/admin/users/${userId}/suspend`, {
    method: "PATCH",
    ...authHeaders(accessToken),
    body: JSON.stringify({ reason }),
  });
}
