import { z } from "zod";

import { BLOOD_GROUPS } from "./bloodRequestSchemas";

export const GENDERS = ["Male", "Female", "Other"] as const;
export type Gender = (typeof GENDERS)[number];

const MIN_AGE_YEARS = 18;
export const OTHER_ILLNESS_MAX_LENGTH = 200;

function calculateAge(dob: Date, today: Date): number {
  let age = today.getFullYear() - dob.getFullYear();
  const hasHadBirthdayThisYear =
    today.getMonth() > dob.getMonth() ||
    (today.getMonth() === dob.getMonth() && today.getDate() >= dob.getDate());
  if (!hasHadBirthdayThisYear) {
    age--;
  }
  return age;
}

// Mirrors CreateIndividualProfileRequestValidator (backend) — see CHH-F02 Confluence page §6.2.
export const createIndividualProfileSchema = z
  .object({
    fullName: z
      .string()
      .trim()
      .min(2, "Please enter your full name")
      .max(50, "Name must be between 2 and 50 characters"),
    email: z.string().trim().min(1, "Enter a valid email address").email("Enter a valid email address"),
    bloodGroup: z.enum(BLOOD_GROUPS, { message: "Please select your blood group" }),
    dateOfBirth: z
      .string()
      .min(1, "Enter a valid date of birth")
      .refine((v) => !Number.isNaN(new Date(v).getTime()), "Enter a valid date of birth")
      .refine((v) => new Date(v).getTime() <= Date.now(), "Enter a valid date of birth")
      .refine(
        (v) => calculateAge(new Date(v), new Date()) >= MIN_AGE_YEARS,
        "You must be 18 or older to register",
      ),
    gender: z.enum(GENDERS, { message: "Please select your gender" }),
    locationCityArea: z.string().trim().min(1, "Please select your location"),
    isChronicIllness: z.boolean(),
    hasRecentSurgery: z.boolean(),
    isInfectiousDisease: z.boolean(),
    isUnderweight: z.boolean(),
    isOtherIllness: z.boolean(),
    otherIllnessDetails: z
      .string()
      .trim()
      .max(OTHER_ILLNESS_MAX_LENGTH, `Must be ${OTHER_ILLNESS_MAX_LENGTH} characters or fewer`)
      .optional(),
  })
  .superRefine((values, ctx) => {
    if (values.isOtherIllness && !values.otherIllnessDetails?.trim()) {
      ctx.addIssue({
        code: z.ZodIssueCode.custom,
        path: ["otherIllnessDetails"],
        message: "Please specify other illness",
      });
    }
  });

export type CreateIndividualProfileFormValues = z.infer<typeof createIndividualProfileSchema>;

export type EligibilityStatus = "ReceiverOnly" | "EligibleDonor";

// Client-side preview only — mirrors IndividualProfileFactory's IsReceiverOnly derivation
// (backend), but the authoritative flag is IndividualProfileDto.isReceiverOnly from the API
// response (PRD §7 CHH-F02 AC2).
export function previewEligibility(values: {
  isChronicIllness?: boolean;
  hasRecentSurgery?: boolean;
  isInfectiousDisease?: boolean;
  isUnderweight?: boolean;
  isOtherIllness?: boolean;
}): EligibilityStatus {
  const hasRestriction =
    values.isChronicIllness ||
    values.hasRecentSurgery ||
    values.isInfectiousDisease ||
    values.isUnderweight ||
    values.isOtherIllness;
  return hasRestriction ? "ReceiverOnly" : "EligibleDonor";
}
