import { z } from "zod";

export const FACILITY_CATEGORIES = ["Hospital", "NGO"] as const;
export type FacilityCategory = (typeof FACILITY_CATEGORIES)[number];

export const MIN_CONTACTS = 1;
export const MAX_CONTACTS = 3;

// Matches Main.dc.html's actual validation message — more precise than the Data
// Dictionary's plain "Alphanumeric" (letters, numbers and hyphens, per the mockup).
const LICENSE_NUMBER_PATTERN = /^[A-Za-z0-9-]+$/;
const MOBILE_PATTERN = /^\d{10}$/;

// Mirrors CHH-78/US-CHH-003-01 AC1 — see CHH-F03 Data Dictionary.
export const facilityDetailsSchema = z.object({
  facilityName: z.string().trim().min(3, "Facility name must be at least 3 characters."),
  category: z.enum(FACILITY_CATEGORIES, { message: "Select a category." }),
  licenseNumber: z
    .string()
    .trim()
    .min(1, "Enter license number")
    .regex(LICENSE_NUMBER_PATTERN, "Licence number can contain letters, numbers and hyphens only."),
  address: z.string().trim().min(1, "Enter address"),
});
export type FacilityDetailsFormValues = z.infer<typeof facilityDetailsSchema>;

// Mirrors CHH-78 AC2 — one contact's fields, validated per-entry so the UI can point at the
// exact contact index (see Contacts.dc.html's per-contact error placement).
export const contactSchema = z.object({
  name: z.string().trim().min(1, "Enter contact name"),
  designation: z.string().trim().min(1, "Enter designation"),
  mobile: z.string().trim().regex(MOBILE_PATTERN, "Enter all 10 digits of the mobile number."),
});
export type ContactFormValues = z.infer<typeof contactSchema>;

export const createFacilitySchema = facilityDetailsSchema.extend({
  contacts: z.array(contactSchema).min(MIN_CONTACTS).max(MAX_CONTACTS),
});
export type CreateFacilityFormValues = z.infer<typeof createFacilitySchema>;

// Duplicate-mobile check across contacts — kept separate from the Zod schema (rather than a
// superRefine) so the hook can attach the error to the exact later contact's mobile field,
// matching Contacts.dc.html's "Contact <N> already uses this number" placement.
export function findDuplicateMobileIndex(contacts: { mobile: string }[]): number | null {
  const seen = new Map<string, number>();
  for (let i = 0; i < contacts.length; i++) {
    const mobile = contacts[i].mobile;
    if (!mobile) continue;
    if (seen.has(mobile)) return i;
    seen.set(mobile, i);
  }
  return null;
}
