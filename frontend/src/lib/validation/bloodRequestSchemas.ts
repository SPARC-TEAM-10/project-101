import { z } from "zod";

export const BLOOD_GROUPS = ["A+", "A-", "B+", "B-", "O+", "O-", "AB+", "AB-"] as const;
export type BloodGroup = (typeof BLOOD_GROUPS)[number];

export const URGENCY_LEVELS = ["Emergency", "Urgent", "Standard"] as const;
export type UrgencyLevel = (typeof URGENCY_LEVELS)[number];

export const MIN_SEARCH_RADIUS_KM = 5;
export const MAX_SEARCH_RADIUS_KM = 100;

// Mirrors CreateBloodRequestRequestValidator (backend) — see CHH-33/US-CHH-004-01 AC2/AC3/AC4.
export const createBloodRequestSchema = z.object({
  patientName: z
    .string()
    .trim()
    .min(2, "Please enter the patient's name")
    .max(100, "Patient name must be between 2 and 100 characters"),
  bloodGroup: z.enum(BLOOD_GROUPS, { message: "Please select a blood group" }),
  unitsRequired: z.number().int().min(1, "Units required must be at least 1"),
  locationCityArea: z.string().trim().min(1, "Please enter a location").max(100),
  searchRadiusKm: z
    .number()
    .int()
    .min(MIN_SEARCH_RADIUS_KM, "Minimum radius is 5km")
    .max(MAX_SEARCH_RADIUS_KM, "Maximum radius is 100km"),
  urgency: z.enum(URGENCY_LEVELS, { message: "Please select an urgency level" }),
});

export type CreateBloodRequestFormValues = z.infer<typeof createBloodRequestSchema>;
