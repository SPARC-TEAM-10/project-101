import { apiFetch } from "./httpClient";
import type { BloodGroup } from "../lib/validation/bloodRequestSchemas";
import type { Gender } from "../lib/validation/individualSchemas";

export interface CreateIndividualProfileRequest {
  mobileNumber: string;
  fullName: string;
  email: string;
  bloodGroup: BloodGroup;
  dateOfBirth: string;
  gender: Gender;
  locationCityArea: string;
  isChronicIllness: boolean;
  hasRecentSurgery: boolean;
  isInfectiousDisease: boolean;
  isUnderweight: boolean;
  isOtherIllness: boolean;
  otherIllnessDetails?: string;
}

export interface IndividualProfileDto {
  id: string;
  fullName: string;
  bloodGroup: BloodGroup;
  isReceiverOnly: boolean;
  locationCityArea: string;
  createdAtUtc: string;
  isChronicIllness: boolean;
  hasRecentSurgery: boolean;
  isInfectiousDisease: boolean;
  isUnderweight: boolean;
  isOtherIllness: boolean;
  otherIllnessDetails?: string;
}

export interface UpdateIndividualProfileRequest {
  locationCityArea: string;
  isChronicIllness: boolean;
  hasRecentSurgery: boolean;
  isInfectiousDisease: boolean;
  isUnderweight: boolean;
  isOtherIllness: boolean;
  otherIllnessDetails?: string;
}

// CHH-F02 (contracts/chh-api.v1.yaml `POST /individuals`) — `security: []`: mobileNumber only
// needs a verified OTP (CHH-9), not a bearer token.
export function registerIndividual(request: CreateIndividualProfileRequest): Promise<IndividualProfileDto> {
  return apiFetch<IndividualProfileDto>("/individuals", {
    method: "POST",
    body: JSON.stringify(request),
  });
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

// [Authorize]-protected (CHH-F02 profile edit). 404 if the caller hasn't completed registration.
export function updateMyProfile(
  accessToken: string,
  request: UpdateIndividualProfileRequest,
): Promise<IndividualProfileDto> {
  return apiFetch<IndividualProfileDto>("/individuals/me", {
    method: "PATCH",
    headers: { Authorization: `Bearer ${accessToken}` },
    body: JSON.stringify(request),
  });
}
