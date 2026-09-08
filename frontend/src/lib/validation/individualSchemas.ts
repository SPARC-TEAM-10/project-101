import { z } from "zod";

import { BLOOD_GROUPS } from "./bloodRequestSchemas";

export const GENDERS = ["Male", "Female", "Other"] as const;
export type Gender = (typeof GENDERS)[number];

export const MAX_OTHER_ILLNESS_LENGTH = 200;

const INVALID_DOB_MESSAGE = "Enter a valid date of birth. You must be 18 or older to register.";

function isAtLeast18(dateOfBirth: string): boolean {
  const dob = new Date(dateOfBirth);
  if (Number.isNaN(dob.getTime()) || dob.getTime() > Date.now()) {
    return false;
  }
  const today = new Date();
  let age = today.getFullYear() - dob.getFullYear();
  const hadBirthdayThisYear =
    today.getMonth() > dob.getMonth() || (today.getMonth() === dob.getMonth() && today.getDate() >= dob.getDate());
  if (!hadBirthdayThisYear) {
    age -= 1;
  }
  return age >= 18;
}

// Mirrors CreateIndividualProfileRequest (contracts/chh-api.v1.yaml) — see the CHH-F02 Data
// Dictionary and §6.2 Standard Validation Requirements for the exact copy.
export const individualRegistrationSchema = z
  .object({
    fullName: z
      .string()
      .trim()
      .min(2, "Please enter your full name. Name must be between 2 and 50 characters.")
      .max(50, "Please enter your full name. Name must be between 2 and 50 characters."),
    email: z.string().trim().min(1, "Enter a valid email address.").email("Enter a valid email address."),
    bloodGroup: z.enum(BLOOD_GROUPS, { message: "Please select your blood group." }),
    dateOfBirth: z.string().refine(isAtLeast18, INVALID_DOB_MESSAGE),
    gender: z.enum(GENDERS, { message: "Please select your gender." }),
    locationCityArea: z.string().trim().min(1, "Please select your location."),
    isChronicIllness: z.boolean(),
    hasRecentSurgery: z.boolean(),
    isInfectiousDisease: z.boolean(),
    isUnderweight: z.boolean(),
    isOtherIllness: z.boolean(),
    otherIllnessDetails: z.string().trim().max(MAX_OTHER_ILLNESS_LENGTH, "Please specify other illness."),
  })
  .refine((values) => !values.isOtherIllness || values.otherIllnessDetails.length > 0, {
    message: "Please specify other illness.",
    path: ["otherIllnessDetails"],
  });

export type IndividualRegistrationFormValues = z.infer<typeof individualRegistrationSchema>;

export interface HealthScreeningFlags {
  isChronicIllness: boolean;
  hasRecentSurgery: boolean;
  isInfectiousDisease: boolean;
  isUnderweight: boolean;
  isOtherIllness: boolean;
}

// Any restriction flag (including Other) marks the profile Receiver Only (PRD §7 CHH-F02 AC2 /
// §6.4 AC3); no restriction flagged marks it an Eligible Donor.
export function isReceiverOnly(flags: HealthScreeningFlags): boolean {
  return (
    flags.isChronicIllness || flags.hasRecentSurgery || flags.isInfectiousDisease || flags.isUnderweight || flags.isOtherIllness
  );
}
