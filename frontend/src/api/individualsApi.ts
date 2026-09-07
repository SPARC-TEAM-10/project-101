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
  createdAtUtc: string;
}

// AllowAnonymous (CHH-F02) — the OTP-verified mobileNumber carried in the body is the actual
// gate (see backend's IndividualsController), not a bearer token.
export function registerIndividual(request: CreateIndividualProfileRequest): Promise<IndividualProfileDto> {
  return apiFetch<IndividualProfileDto>("/individuals", {
    method: "POST",
    body: JSON.stringify(request),
  });
}
