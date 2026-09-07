import { apiFetch } from "./httpClient";
import type { BloodGroup } from "../lib/validation/bloodRequestSchemas";

export interface IndividualProfileDto {
  id: string;
  fullName: string;
  bloodGroup: BloodGroup;
  isReceiverOnly: boolean;
  locationCityArea: string;
  createdAtUtc: string;
}

// [Authorize]-protected (CHH-81 Individual Dashboard). 404 if the caller hasn't completed
// registration yet — surfaced to callers as an ApiError with status 404, not thrown as a
// generic Error, so the dashboard can show a "complete your profile" prompt instead of an
// error toast.
export function getMyProfile(accessToken: string): Promise<IndividualProfileDto> {
  return apiFetch<IndividualProfileDto>("/individuals/me", {
    headers: { Authorization: `Bearer ${accessToken}` },
  });
}
