// Mirrors CHH-79/US-CHH-003-02 AC2/AC3. A plain function, not Zod — File objects aren't
// JSON-schema-shaped, and the two rules here (MIME type, byte size) don't benefit from a schema
// library's declarative rule composition the way form-field validation does.

export const ALLOWED_FILE_TYPES = ["application/pdf", "image/jpeg", "image/png"] as const;
export const MAX_FILE_SIZE_BYTES = 5 * 1024 * 1024; // 5 MB (AC3)

export const INVALID_FILE_TYPE_MESSAGE = "Invalid file format. Please upload PDF or Image.";
export const FILE_TOO_LARGE_MESSAGE = "File too large. Maximum size is 5MB.";

export function validateFacilityLicenseFile(file: File): string | null {
  if (!ALLOWED_FILE_TYPES.includes(file.type as (typeof ALLOWED_FILE_TYPES)[number])) {
    return INVALID_FILE_TYPE_MESSAGE;
  }
  if (file.size > MAX_FILE_SIZE_BYTES) {
    return FILE_TOO_LARGE_MESSAGE;
  }
  return null;
}
