import { describe, expect, it } from "vitest";

import { createIndividualProfileSchema, OTHER_ILLNESS_MAX_LENGTH, previewEligibility } from "./individualSchemas";

function isoDateYearsAgo(years: number, extraDays = 0): string {
  const d = new Date();
  d.setFullYear(d.getFullYear() - years);
  d.setDate(d.getDate() + extraDays);
  return d.toISOString().slice(0, 10);
}

const validValues = {
  fullName: "Jane Doe",
  email: "jane@example.com",
  bloodGroup: "O+" as const,
  dateOfBirth: isoDateYearsAgo(25),
  gender: "Female" as const,
  locationCityArea: "Kaloor, Kochi",
  isChronicIllness: false,
  hasRecentSurgery: false,
  isInfectiousDisease: false,
  isUnderweight: false,
  isOtherIllness: false,
};

describe("createIndividualProfileSchema", () => {
  it("accepts a fully valid submission", () => {
    const result = createIndividualProfileSchema.safeParse(validValues);
    expect(result.success).toBe(true);
  });

  it("rejects a full name shorter than 2 characters", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, fullName: "J" });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.fullName?.[0]).toBe("Please enter your full name");
    }
  });

  it("rejects a full name longer than 50 characters", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, fullName: "a".repeat(51) });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.fullName?.[0]).toBe("Name must be between 2 and 50 characters");
    }
  });

  it("trims whitespace-only names to an empty value and rejects it", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, fullName: "   " });
    expect(result.success).toBe(false);
  });

  it("rejects an invalid email address", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, email: "not-an-email" });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.email?.[0]).toBe("Enter a valid email address");
    }
  });

  it("rejects a blood group not in the enum", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, bloodGroup: "Z+" });
    expect(result.success).toBe(false);
  });

  it("rejects a date of birth in the future", () => {
    const result = createIndividualProfileSchema.safeParse({
      ...validValues,
      dateOfBirth: isoDateYearsAgo(0, 30),
    });
    expect(result.success).toBe(false);
  });

  it("rejects a date of birth under 18 years old", () => {
    const result = createIndividualProfileSchema.safeParse({
      ...validValues,
      dateOfBirth: isoDateYearsAgo(17, 300),
    });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.dateOfBirth?.[0]).toBe("You must be 18 or older to register");
    }
  });

  it("accepts a date of birth exactly 18 years ago today (inclusive boundary)", () => {
    const result = createIndividualProfileSchema.safeParse({
      ...validValues,
      dateOfBirth: isoDateYearsAgo(18),
    });
    expect(result.success).toBe(true);
  });

  it("rejects a gender not in the enum", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, gender: "Unknown" });
    expect(result.success).toBe(false);
  });

  it("rejects an empty location", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, locationCityArea: "  " });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.locationCityArea?.[0]).toBe("Please select your location");
    }
  });

  it("requires otherIllnessDetails when isOtherIllness is true", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, isOtherIllness: true });
    expect(result.success).toBe(false);
    if (!result.success) {
      expect(result.error.flatten().fieldErrors.otherIllnessDetails?.[0]).toBe("Please specify other illness");
    }
  });

  it("accepts isOtherIllness true with non-empty otherIllnessDetails", () => {
    const result = createIndividualProfileSchema.safeParse({
      ...validValues,
      isOtherIllness: true,
      otherIllnessDetails: "Seasonal allergy flare-up",
    });
    expect(result.success).toBe(true);
  });

  it("rejects otherIllnessDetails longer than 200 characters", () => {
    const result = createIndividualProfileSchema.safeParse({
      ...validValues,
      isOtherIllness: true,
      otherIllnessDetails: "a".repeat(OTHER_ILLNESS_MAX_LENGTH + 1),
    });
    expect(result.success).toBe(false);
  });

  it("does not require otherIllnessDetails when isOtherIllness is false", () => {
    const result = createIndividualProfileSchema.safeParse({ ...validValues, isOtherIllness: false });
    expect(result.success).toBe(true);
  });
});

describe("previewEligibility", () => {
  it("returns EligibleDonor when no restriction flags are set", () => {
    expect(
      previewEligibility({
        isChronicIllness: false,
        hasRecentSurgery: false,
        isInfectiousDisease: false,
        isUnderweight: false,
        isOtherIllness: false,
      }),
    ).toBe("EligibleDonor");
  });

  it("returns ReceiverOnly when any single restriction flag is set", () => {
    expect(previewEligibility({ isChronicIllness: true })).toBe("ReceiverOnly");
    expect(previewEligibility({ hasRecentSurgery: true })).toBe("ReceiverOnly");
    expect(previewEligibility({ isInfectiousDisease: true })).toBe("ReceiverOnly");
    expect(previewEligibility({ isUnderweight: true })).toBe("ReceiverOnly");
    expect(previewEligibility({ isOtherIllness: true })).toBe("ReceiverOnly");
  });

  it("returns EligibleDonor for an empty input (no flags provided)", () => {
    expect(previewEligibility({})).toBe("EligibleDonor");
  });
});
