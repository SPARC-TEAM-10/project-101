import { apiFetch, ApiError, type ProblemDetails } from "./httpClient";
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
  verificationStatus: "Pending" | "Verified" | "Rejected";
  licenseDocumentUrl?: string | null;
  rejectionReason?: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
}

// Matches contracts/chh-api.v1.yaml's POST /facilities (CHH-78). No [Authorize] gate — registering
// is what makes a mobile number resolve to the Hospital/Ngo role in the first place (CHH-10), so
// there's no session to gate on yet, matching individualApi.ts's registerIndividual precedent.
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

// Matches contracts/chh-api.v1.yaml's GET /facilities/me (CHH-28). [Authorize(Roles = "Hospital,Ngo")]
// on the backend — callers with any other role get a 401/403, handled the same as any other
// apiFetch error by the caller (useFacilityDashboard).
export function getMyFacility(accessToken: string | undefined): Promise<FacilityDto> {
  return apiFetch<FacilityDto>("/facilities/me", {
    headers: accessToken ? { Authorization: `Bearer ${accessToken}` } : undefined,
  });
}

const UPLOAD_API_BASE_URL = `${import.meta.env.VITE_API_BASE_URL ?? ""}/api/v1`;

// ASSUMED SHAPE (CHH-79) — not yet in contracts/chh-api.v1.yaml; multipart/form-data with a
// single "file" field. Uses XMLHttpRequest instead of apiFetch/fetch for two reasons: (1) fetch
// cannot report upload progress (no request-body progress event), and the UI Notes require a
// progress bar; (2) multipart requests must NOT set a Content-Type header themselves — the
// browser sets it (with the multipart boundary) only when left unset, and apiFetch always forces
// "Content-Type: application/json".
export function uploadFacilityLicense(
  accessToken: string | undefined,
  facilityId: string,
  file: File,
  onProgress: (percent: number) => void,
): Promise<FacilityDto> {
  return new Promise((resolve, reject) => {
    const xhr = new XMLHttpRequest();
    xhr.open("POST", `${UPLOAD_API_BASE_URL}/facilities/${facilityId}/upload`);
    if (accessToken) {
      xhr.setRequestHeader("Authorization", `Bearer ${accessToken}`);
    }

    xhr.upload.onprogress = (event) => {
      if (event.lengthComputable) {
        onProgress(Math.round((event.loaded / event.total) * 100));
      }
    };

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve(JSON.parse(xhr.responseText) as FacilityDto);
        return;
      }
      const problem = (() => {
        try {
          return JSON.parse(xhr.responseText) as ProblemDetails;
        } catch {
          return {};
        }
      })();
      reject(new ApiError(xhr.status, problem));
    };

    xhr.onerror = () => reject(new ApiError(0, { detail: "Network error during upload." }));

    const formData = new FormData();
    formData.append("file", file);
    xhr.send(formData);
  });
}
